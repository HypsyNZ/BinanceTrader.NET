//******************************************************************************************************
//  Copyright © 2022, S. Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using BinanceAPI.Enums;
using BinanceAPI.Objects;
using BinanceAPI.Objects.Shared;
using BinanceAPI.Objects.Spot.UserStream;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.Controls;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using LoopDelay.NET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    public class MainOrders : ObservableObject
    {
        /// <summary>
        /// The last time orders were updated, Needs to be reset when mode/symbol changes
        /// </summary>
        public static DateTime LastRun { get; set; } = new();

        public static volatile ObservableCollection<OrderBase> orders = null;
        private static ConcurrentQueue<BinanceStreamOrderUpdate> dataEventWaiting = new();

        public ObservableCollection<OrderBase> Current
        { get => orders; set { orders = value; PC(); } }

        public static bool IsUpdatingOrders { get; set; } = false;

        public static ObservableCollection<OrderBase> GetOrders { get => orders; }
        public static bool BlockOrderUpdates { get; set; } = false;
        public bool OrderUpdateWaiting { get; set; } = false;

        public static ConcurrentQueue<BinanceStreamOrderUpdate> DataEventWaiting
        {
            get => dataEventWaiting;
            set { dataEventWaiting = value; lastDataEvent = DateTime.Now; }
        }

        private static DateTime lastDataEvent = new();

        public void AddNewOrderUpdateEvents(BinanceStreamOrderUpdate order)
        {
            DataEventWaiting.Enqueue(order);
            Orders.OrderUpdateWaiting = true;
        }

        public async Task UpdateOrdersCurrentSymbol()
        {
            try
            {
                MainOrders.IsUpdatingOrders = true;

                // Load Orders From File
                if (Static.ManageStoredOrders.IsLoadingStoredOrders)
                {
                    Static.ManageStoredOrders.IsLoadingStoredOrders = false;
                    await UpdateOrdersFromFile().ConfigureAwait(false);
                }

                // Add OnOrderUpdates that are recieved in "real time"
                if (OrderUpdateWaiting)
                {
                    await UpdateFromOrderUpdates().ConfigureAwait(false);
                }

                // Remove Deleted Orders
                if (Deleted.IsDeletedTriggered)
                {
                    Deleted.IsDeletedTriggered = false;
                    await Deleted.EnumerateDeletedList(Current).ConfigureAwait(false);
                }

                // Update last 200 orders from server
                // Happens first run and then once every 5 minutes just in case
                var time = DateTime.UtcNow;
                if (!BlockOrderUpdate() && time > (LastRun + TimeSpan.FromMinutes(5)))
                {
                    LastRun = DateTime.UtcNow;
                    await UpdateOrdersFromServer().ConfigureAwait(false);
                }

                MainOrders.IsUpdatingOrders = false;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while getting Orders: ", ex);
            }
        }

        private async Task UpdateOrdersFromFile()
        {
            ObservableCollection<OrderBase> OrderUpdate = Static.ManageStoredOrders.LoadStoredOrdersCurrentSymbol(Current);

            await AddOrdersToCollectionsAndRefresh(OrderUpdate);
        }

        private Task AddOrdersToCollectionsAndRefresh(ObservableCollection<OrderBase> OrderUpdate)
        {
            if (Static.ManageStoredOrders.AddOrderUpdateToCollections(OrderUpdate))
            {
                MainOrders.BlockOrderUpdates = true;
                Invoke.InvokeUI(() =>
                {
                    Current = new ObservableCollection<OrderBase>(OrderUpdate.OrderByDescending(d => d.CreateTime));
                    MainOrders.BlockOrderUpdates = false;
                });
            }

            return Task.CompletedTask;
        }

        private async Task UpdateOrdersFromServer()
        {
            ObservableCollection<OrderBase> OrderUpdate = Current != null ? Current : new();

            Task<WebCallResult<IEnumerable<BinanceOrderBase>>> webCallResult = null;

            webCallResult = Static.CurrentTradingMode switch
            {
                TradingMode.Spot => BTClient.Local.Spot.Order.GetOrdersAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, receiveWindow: 1500),
                TradingMode.Margin => BTClient.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, receiveWindow: 1500),
                TradingMode.Isolated => BTClient.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, true, receiveWindow: 1500),
                _ => null,
            };

            await webCallResult.ConfigureAwait(false);

            if (webCallResult.Result.Success)
            {
                foreach (var o in webCallResult.Result.Data)
                {
                    if (BlockOrderUpdate()) { break; }

                    if (!Static.DeletedList.Contains(o.OrderId) && OrderUpdate.FirstOrDefault(f => f.OrderId == o.OrderId) == null)
                    {
                        Invoke.InvokeUI(() =>
                        {
                            OrderUpdate.Add(Order.NewOrder(o, Static.GetCurrentlySelectedSymbol.TradeFee, Static.GetCurrentlySelectedSymbol.InterestRate));
                            Static.ManageStoredOrders.IsUpdatingStoredOrders = true;
                        });
                    }
                }
            }

            if (Static.ManageStoredOrders.IsUpdatingStoredOrders)
            {
                await AddOrdersToCollectionsAndRefresh(OrderUpdate);
                Static.ManageStoredOrders.IsUpdatingStoredOrders = false;
            }
            return;
        }

        private Task UpdateFromOrderUpdates()
        {
            OrderUpdateWaiting = false;
            try
            {
                var start = DateTime.UtcNow.Ticks;
                while (DataEventWaiting.Count() > 0 && Loop.Delay(start, 3, 60000, (() => { OrderUpdateError(); })).Result)
                {
                    BinanceStreamOrderUpdate result;
                    if (DataEventWaiting.TryDequeue(out result))
                    {
                        if (result != null)
                        {
                            decimal rprice = result.Price != 0 ? result.Price : result.LastPriceFilled;
                            decimal convertedPrice = rprice != 0 ? DecimalLayout.Convert(rprice, result.Symbol, Stored.ExchangeInfo) : 0;

                            OrderBase OrderExists = Current.Where(x => x.OrderId == result.OrderId).FirstOrDefault();
                            if (OrderExists != null)
                            {
                                if (OrderExists.QuantityFilled < result.QuantityFilled)
                                {
                                    // Existing Orders that have useful updates
                                    SaveAndUpdateOrder(OrderExists, result, convertedPrice);
                                }
                            }
                            else
                            {
                                // New Orders with no collisions
                                SaveAndAddOrder(result, convertedPrice);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { WriteLog.Error(ex); }
            return Task.CompletedTask;
        }

        private Task OrderUpdateError()
        {
            WriteLog.Error("Order Updates got stuck in a loop and it was manually broken");
            return Task.CompletedTask;
        }

        public Task UpdatePnl()
        {
            if (!BlockOrderUpdate() && Current != null)
            {
                try
                {
                    long pnlUpdateTime = DateTime.UtcNow.Ticks;
                    foreach (var r in Current)
                    {
                        if (!BlockOrderUpdate())
                        {
                            Invoke.InvokeUI(() =>
                            {
                                if (Static.GetCurrentlySelectedSymbol.InterestRate > 0)
                                {
                                    r.IPH = Static.GetCurrentlySelectedSymbol.InterestRate;
                                    r.IPD = Static.GetCurrentlySelectedSymbol.InterestRate;
                                    r.ITD = (decimal)new TimeSpan(pnlUpdateTime - InterestTimeStamp(r.CreateTime, r.ResetTime)).TotalHours;
                                    r.ITDQ = 0;
                                }

                                r.Fulfilled = r.Fulfilled;
                                r.Pnl = r.Status != OrderStatus.Canceled ? (r.Side == OrderSide.Sell ? (r.Price * r.QuantityFilled) - (Static.RTUB.BestAskPrice * r.QuantityFilled) : (Static.RTUB.BestBidPrice * r.QuantityFilled) - (r.Price * r.QuantityFilled)) : 0;
                            });
                        }
                    }

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {                   
                    if (ex.Message != "Collection was modified; enumeration operation may not execute.") // Ignore
                    {
                        WriteLog.Error("UpdatePnl Error: " + ex.Message + "| HRESULT: " + ex.HResult);
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines which Timestamp to use for running Interest Update
        /// </summary>
        /// <param name="CreateTime">Time Order was created</param>
        /// <param name="ResetTime">Time the Reset interest button was clicked</param>
        /// <returns></returns>
        public long InterestTimeStamp(DateTime CreateTime, DateTime ResetTime)
        {
            if (ResetTime != DateTime.MinValue) { return ResetTime.Ticks; }

            return CreateTime.Ticks;
        }

        public bool BlockOrderUpdate()
        {
            if (MainOrders.BlockOrderUpdates) { return true; }
            if (!IsSymbolSelected || Static.GetCurrentlySelectedSymbol == null) { ResetOrders(); return true; }
            return false;
        }

        public void ResetOrders()
        {
            Current = null;
            Static.ManageStoredOrders.IsUpdatingStoredOrders = false;
            MainOrders.BlockOrderUpdates = false;
        }

        public void SaveAndUpdateOrder(OrderBase order, BinanceStreamOrderUpdate d, decimal convertedPrice)
        {
            MainOrders.BlockOrderUpdates = true;
            Invoke.InvokeUI(() =>
            {
                order.QuantityFilled = d.QuantityFilled;
                order.Quantity = DecimalLayout.TrimDecimal(d.Quantity);
                order.Price = convertedPrice;
                order.Status = d.Status;
                order.TimeInForce = d.TimeInForce.ToString();
                order.UpdateTime = d.UpdateTime;

                Current = new ObservableCollection<OrderBase>(Current.OrderByDescending(d => d.CreateTime));
                MainOrders.BlockOrderUpdates = false;
            });

            Static.ManageStoredOrders.AddSingleOrder(order);
        }

        public void SaveAndAddOrder(BinanceStreamOrderUpdate data, decimal convertedPrice)
        {
            var fee = Static.GetCurrentlySelectedSymbol.TradeFee;
            var interestrate = Static.GetCurrentlySelectedSymbol.InterestRate;

            OrderBase OrderToAdd = Order.AddNewOrderOnUpdate(data, convertedPrice, fee, interestrate);
            WaitAndAddOrder(OrderToAdd);

            Static.ManageStoredOrders.AddSingleOrder(OrderToAdd);
        }

        private void WaitAndAddOrder(OrderBase order)
        {
            MainOrders.BlockOrderUpdates = true;
            Invoke.InvokeUI(() =>
            {
                Current.Add(order);
                Current = new ObservableCollection<OrderBase>(Current.OrderByDescending(d => d.CreateTime));
                MainOrders.BlockOrderUpdates = false;
            });
        }
    }
}