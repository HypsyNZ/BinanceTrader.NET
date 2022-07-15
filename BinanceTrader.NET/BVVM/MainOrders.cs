/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using BinanceAPI.Enums;
using BinanceAPI.Objects;
using BinanceAPI.Objects.Shared;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Orders;
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
        private const int ONE_HUNDRED = 100;

        private const int DELAY_TIME_MS = 1;
        private const int EXPIRE_TIME_MS = 60000;

        private const int RECV_WINDOW = 1500;

        private const int NUM_ORDERS = 500;
        private const int SERVER_ORDER_DELAY_MINS = 5;

        private const int INTEREST_RATE_CHECK_DELAY_MINS = 60;
        private const int TRADE_FEE_CHECK_DELAY_MINS = 60;

        private decimal _currentInterestRate;
        private BinanceTradeFee? _currentTradeFee;

        public static readonly object OrderUpdateLock = new object();

        /// <summary>
        /// The last time orders were updated, Needs to be reset when mode/symbol changes
        /// </summary>
        public static DateTime LastRun { get; set; }

        private ObservableCollection<OrderBase> orders = new();

        public ObservableCollection<OrderBase> Current
        {
            get => orders;
            set
            {
                orders = value;
                PC();
            }
        }

        public static bool IsUpdatingOrders { get; set; }

        public static ConcurrentQueue<OrderUpdate> DataEventWaiting { get; } = new();

        public void AddNewOrderUpdateEventsToQueue(BinanceStreamOrderUpdate order, TradingMode tradingMode)
        {
            OrderUpdate orderUpdate = new OrderUpdate(order, tradingMode);
            DataEventWaiting.Enqueue(orderUpdate);
#if DEBUG
            WriteLog.Info("Got An Order Update: " + order.OrderId + " | " + order.Quantity + "| " + order.Type + " | " + order.Status);
#endif
        }

        public async Task UpdateOrdersCurrentSymbolAsync()
        {
            try
            {
                // Load Orders From Memory
                await UpdateOrdersFromMemoryAsync().ConfigureAwait(false);

                // Add OnOrderUpdates that are recieved in "real time"
                await UpdateFromOrderUpdatesAsync().ConfigureAwait(false); // Added To Memory Storage

                // Remove Deleted Orders
                if (Deleted.IsDeletedTriggered)
                {
                    Deleted.IsDeletedTriggered = false;

                    lock (MainOrders.OrderUpdateLock)
                    {
                        Deleted.EnumerateDeletedListAsync(Current).ConfigureAwait(false);
                    }
                }

                // Update last 200 orders from server
                // Happens first run and then once every 5 minutes just in case
                var time = DateTime.UtcNow;
                if (!Static.IsInvalidSymbol() && time > (LastRun + TimeSpan.FromMinutes(SERVER_ORDER_DELAY_MINS)))
                {
                    LastRun = time;

                    // Added to Memory Storage
                    await UpdateOrdersFromServerAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while getting Orders: ", ex);
            }
            finally
            {
                IsUpdatingOrders = false;
            }
        }

        private Task UpdateOrdersFromMemoryAsync()
        {
            bool updated = false;
            List<OrderBase> temp;
            List<OrderBase>? OrderUpdate = Static.ManageStoredOrders.CurrentSymbolMemoryStoredOrders(Static.DeletedList)?.ToList();

            if (OrderUpdate != null)
            {
                lock (MainOrders.OrderUpdateLock)
                {
                    temp = Current.ToList();

                    if (temp.Count == 0)
                    {
                        temp = OrderUpdate;
                        if (temp.Count > 0)
                        {
                            updated = true;
                        }
                    }
                    else
                    {
                        if (OrderUpdate.Count == temp.Count)
                        {
                            return Task.CompletedTask;
                        }

                        foreach (var order in OrderUpdate)
                        {
                            var exists = temp.Where(t => t.OrderId == order.OrderId).Any();
                            if (!exists)
                            {
                                temp.Add(order);
#if DEBUG
                                WriteLog.Info("Dequeued: " + order.OrderId + " | " + order.Quantity + "| " + order.Type + " | " + order.Status);
#endif
                                updated = true;
                            }
#if DEBUG
                            else
                            {
                                WriteLog.Info("Exists: " + order.OrderId + " | " + order.Quantity + "| " + order.Type + " | " + order.Status);
                            }
#endif
                        }
                    }

                    if (updated)
                    {
                        InvokeUI.CheckAccess(() =>
                        {
                            Current = new ObservableCollection<OrderBase>(temp.OrderByDescending(d => d.CreateTime));
                        });

#if DEBUG
                        WriteLog.Info("Updated Orders: " + OrderUpdate.Count + "/" + temp.Count);
#endif
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task UpdateOrdersFromServerAsync()
        {
            // Currently Selected Symbol

            if (Static.SelectedSymbolViewModel != null)
            {
                WebCallResult<IEnumerable<BinanceOrderBase>>? webCallResult;

                if ((webCallResult = Static.CurrentTradingMode switch
                {
                    TradingMode.Spot =>
                    await Client.Local.Spot.Order.GetOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, receiveWindow: RECV_WINDOW),
                    TradingMode.Margin =>
                    await Client.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, receiveWindow: RECV_WINDOW),
                    TradingMode.Isolated =>
                    await Client.Local.Margin.Order.GetMarginAccountOrdersAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, null, null, null, NUM_ORDERS, true, receiveWindow: RECV_WINDOW),
                    _ => null,
                }) != null && webCallResult.Success)
                {
                    await EnumerateOrdersFromServerAsync(webCallResult).ConfigureAwait(false);
                }
            }
        }

        private Task EnumerateOrdersFromServerAsync(WebCallResult<IEnumerable<BinanceOrderBase>> webCallResult)
        {
            // Currently Selected Symbol

            List<OrderBase> OrderUpdate = new();
            foreach (var o in webCallResult.Data)
            {
                if (!Static.DeletedList.Contains(o.OrderId) && Current.FirstOrDefault(f => f.OrderId == o.OrderId) == null)
                {
                    OrderUpdate.Add(Order.AddNewOrderFromServer(o, Static.CurrentTradingMode));
                }
            }

            Static.ManageStoredOrders.AddOrderUpdatesToMemoryStorage(OrderUpdate);

            return Task.CompletedTask;
        }

        private Task UpdateFromOrderUpdatesAsync()
        {
            // Any Symbol
            if (Stored.ExchangeInfo == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                var start = DateTime.UtcNow.Ticks;
                while (DataEventWaiting.TryPeek(out _) && Loop.Delay(start, DELAY_TIME_MS, EXPIRE_TIME_MS, (() =>
                {
                    OrderUpdateError();
                })).Result)
                {
                    if (DataEventWaiting.TryDequeue(out OrderUpdate NewOrder))
                    {
                        switch (NewOrder.Update.Status)
                        {
                            case OrderStatus.Canceled:
                                {
                                    _ = Deleted.AddToDeletedListAsync(NewOrder.Update.OrderId);
                                    WriteLog.Info("Added Cancelled Order to Deleted List: " + NewOrder.Update.OrderId);
                                    continue;
                                }
                            case OrderStatus.Rejected:
                                {
                                    WriteLog.Error($"Order: " + NewOrder.Update.OrderId + " was Rejected OnOrderUpdate: " + NewOrder.Update.RejectReason);
                                    continue;
                                }
                            default:
                                {
                                    ProcessOrder(NewOrder);
                                    continue;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }

            return Task.CompletedTask;
        }

        private void ProcessOrder(OrderUpdate update)
        {
            decimal orderPrice;
            if (update.Update.QuoteQuantityFilled != 0)
            {
                // Average Price (Actual Price for this order)
                orderPrice = update.Update.QuoteQuantityFilled / update.Update.QuantityFilled;
            }
            else
            {
                // Order filled at one price or fallback to lastpricefilled
                orderPrice = update.Update.Price != 0 ? update.Update.Price : update.Update.LastPriceFilled;
            }

            OrderBase OrderIsCurrentSymbol;

            lock (OrderUpdateLock)
            {
                OrderIsCurrentSymbol = Current.Where(x => x.OrderId == update.Update.OrderId).FirstOrDefault();
            }

            if (OrderIsCurrentSymbol != null)
            {
                if (OrderIsCurrentSymbol.QuantityFilled < update.Update.QuantityFilled)
                {
                    // Existing Orders that have useful updates
                    SaveAndUpdateOrder(OrderIsCurrentSymbol, update.Update, orderPrice);
                }
            }
            else
            {
#if DEBUG
                WriteLog.Info("New Order: " + update.Update.EventTime);
#endif
                // New Orders with no collisions
                SaveAndAddOrder(update.Update, orderPrice, update.TradingMode);
            }
        }

        private void OrderUpdateError()
        {
            WriteLog.Error("Order Updates got stuck in a loop and it was manually broken");
        }

        public Task UpdateTradeFeeAsync()
        {
            if ((MainVM.IsSymbolSelected && Static.HasAuth()) && (Static.SelectedSymbolViewModel?.SymbolView != null) == true)
            {
                _currentTradeFee = TradeFee.GetTradeFee(Static.SelectedSymbolViewModel.SymbolView.Symbol);
                Static.SelectedSymbolViewModel.TradeFee = _currentTradeFee;

                if (_currentTradeFee != null)
                {
                    if (_currentTradeFee.TakerFee == _currentTradeFee.MakerFee)
                    {
                        Static.SelectedSymbolViewModel.TradeFeeString = (_currentTradeFee.TakerFee * ONE_HUNDRED).ToString().Normalize() + "%";

                        WriteLog.Info("[Main] Trade Fees for " + _currentTradeFee.Symbol + ": " + "[" + _currentTradeFee.TakerFee + "]" + "(" + (_currentTradeFee.TakerFee * ONE_HUNDRED) + " %)");
                    }
                    else
                    {
                        Static.SelectedSymbolViewModel.TradeFeeString = "M:" + (_currentTradeFee.MakerFee * ONE_HUNDRED).ToString().Normalize() + "% | T:" + (_currentTradeFee.TakerFee * ONE_HUNDRED).ToString().Normalize() + "%";

                        WriteLog.Info("[Main] Trade Fees for " + _currentTradeFee.Symbol
                            + " | Taker Fee: [" + _currentTradeFee.TakerFee + "](" + (_currentTradeFee.TakerFee * ONE_HUNDRED)
                            + " %) | Maker Fee: [" + _currentTradeFee.MakerFee + "](" + (_currentTradeFee.MakerFee * ONE_HUNDRED) + " %)");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UpdateInterestRateAsync()
        {
            if ((MainVM.IsSymbolSelected && Static.HasAuth()) && (Static.SelectedSymbolViewModel?.SymbolView != null) == true)
            {
                _currentInterestRate = InterestRate.GetDailyInterestRate(Static.SelectedSymbolViewModel.SymbolView.Symbol);
                Static.SelectedSymbolViewModel.InterestRate = _currentInterestRate;
            }

            return Task.CompletedTask;
        }

        public Task UpdatePnlAsync()
        {
            lock (MainOrders.OrderUpdateLock)
            {
                foreach (var order in Current)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        if (_currentInterestRate > 0)
                        {
                            var dailyInterestRate = _currentInterestRate;
                            var interestPerHour = OrderHelper.InterestPerHour(order.QuantityFilled, dailyInterestRate);
                            var interestToDate = OrderHelper.InterestToDate(interestPerHour, InterestTimeStamp(order.CreateTime, order.ResetTime));
                            order.InterestPerHour = interestPerHour;
                            order.InterestPerDay = OrderHelper.InterestPerDay(order.QuantityFilled, dailyInterestRate);
                            order.InterestToDate = interestToDate;
                            order.InterestToDateQuote = OrderHelper.InterestToDateQuote(interestToDate, order.Price);
                        }

                        order.Fulfilled = OrderHelper.Fulfilled(order.Quantity, order.QuantityFilled);
                        order.Pnl = decimal.Round(OrderHelper.PnL(order, Static.RealTimeUpdate.BestAskPrice, Static.RealTimeUpdate.BestBidPrice), App.DEFAULT_ROUNDING_PLACES);
                    });
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
        public static long InterestTimeStamp(DateTime CreateTime, DateTime ResetTime)
        {
            if (ResetTime != DateTime.MinValue)
            {
                return ResetTime.Ticks;
            }

            return CreateTime.Ticks;
        }

        public void SaveAndUpdateOrder(OrderBase order, BinanceStreamOrderUpdate d, decimal convertedPrice)
        {
            order.QuantityFilled = d.QuantityFilled;
            order.CumulativeQuoteQuantityFilled = d.QuoteQuantityFilled;
            order.Quantity = d.Quantity;
            order.Price = convertedPrice;
            order.Status = d.Status;
            order.TimeInForce = OrderHelper.TIF(d.TimeInForce.ToString());
            order.UpdateTime = d.UpdateTime;
#if DEBUG
            WriteLog.Info("Updated Order: " + order.CreateTime);
#endif
            // Queue for File Storage
            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order);
        }

        public void SaveAndAddOrder(BinanceStreamOrderUpdate data, decimal convertedPrice, TradingMode tradingMode)
        {
            OrderBase OrderToAdd = Order.AddNewOrderOnUpdate(data, convertedPrice, tradingMode);
#if DEBUG
            WriteLog.Info("New Order: " + data.CreateTime);
#endif
            // Queue for File Storage
            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(OrderToAdd);
        }
    }
}
