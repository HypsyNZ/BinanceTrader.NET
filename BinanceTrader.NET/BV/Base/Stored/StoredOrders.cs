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

using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BTNET.BVVM.Stored;

namespace BTNET.BV.Base
{
    public class StoredOrders : ObservableObject
    {
        public static int lastTotal { get; set; }

        #region [ Load ]

        public Task LoadAllStoredOrdersFromFileStorageAsync()
        {
            Directory.CreateDirectory(App.OrderPath);

            ToS = LoadStoredOrdersFromFileStorage(App.StoredSpotOrders, TradingMode.Spot);
            ToM = LoadStoredOrdersFromFileStorage(App.StoredMarginOrders, TradingMode.Margin);
            ToI = LoadStoredOrdersFromFileStorage(App.StoredIsolatedOrders, TradingMode.Isolated);

            if (ToS.Count > 0)
            {
                WriteLog.Info("Loaded [" + ToS.Count() + "] Spot Orders from file");
            }

            if (ToM.Count > 0)
            {
                WriteLog.Info("Loaded [" + ToM.Count() + "] Margin Orders from file");
            }

            if (ToI.Count > 0)
            {
                WriteLog.Info("Loaded [" + ToI.Count() + "] Isolated Orders from file");
            }

            return Task.CompletedTask;
        }

        private List<OrderBase> LoadStoredOrdersFromFileStorage(string storedOrdersString, TradingMode tradingMode)
        {
            return TJson.Load<List<OrderBase>>(storedOrdersString) ?? new List<OrderBase>();
        }

        #endregion [ Load ]

        #region [ Get ]

        /// <summary>
        /// Get Stored Orders for the Current Symbol
        /// </summary>
        /// <param name="DeletedList">List of deleted orders to filter</param>
        /// <param name="storedOrders">Collection of stored orders</param>
        /// <param name="tradingMode">The current trading mode</param>
        /// <returns></returns>
        private List<OrderBase>? GetMemoryStoredOrdersCurrentSymbol(List<long> DeletedList, List<OrderBase>? storedOrders, TradingMode tradingMode)
        {
            if (Static.SelectedSymbolViewModel == null)
            {
                lastTotal = 0;
                return null;
            }

            if (storedOrders != null)
            {
                var count = storedOrders.Count();
                if (count != lastTotal)
                {
                    lastTotal = count;
                    var TempOrders = new List<OrderBase>(storedOrders.Where(s => s.Symbol == Static.SelectedSymbolViewModel.SymbolView.Symbol && !DeletedList.Contains(s.OrderId)));

                    if (TempOrders != null && TempOrders.Count > 0)
                    {
                        foreach (var order in TempOrders)
                        {
                            order.Helper ??= new OrderViewModel(order.Side, tradingMode, order.Symbol!);
                        }

                        return TempOrders;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Current Symbol and Mode Stored Orders
        /// </summary>
        /// <param name="DeletedList">list of previously deleted orders</param>
        public List<OrderBase>? CurrentSymbolMemoryStoredOrders(List<long> DeletedList)
        {
            // Load stored orders for current symbol
            switch (Static.CurrentTradingMode)
            {
                // S
                case TradingMode.Spot:
                    return GetMemoryStoredOrdersCurrentSymbol(DeletedList, ToS, TradingMode.Spot);

                // M
                case TradingMode.Margin:
                    return GetMemoryStoredOrdersCurrentSymbol(DeletedList, ToM, TradingMode.Margin);

                // I
                case TradingMode.Isolated:
                    return GetMemoryStoredOrdersCurrentSymbol(DeletedList, ToI, TradingMode.Isolated);

                default:
                    // Error
                    WriteLog.Error("StoredOrders: Mode wasn't selected during symbol change");
                    return null;
            }
        }

        #endregion [ Get ]

        #region [ Add ]

        public bool AddOrderUpdatesToMemoryStorage(List<OrderBase> OrderUpdate)
        {
            if (OrderUpdate != null)
            {
                foreach (var order in OrderUpdate)
                {
                    AddSingleOrderToMemoryStorage(order);
                }

                return true;
            }

            return false;
        }

        public void AddSingleOrderToMemoryStorage(OrderBase order)
        {
            switch (order.Helper!.OrderTradingMode)
            {
                case TradingMode.Spot:
                    {
                        AddSingleOrderToMemoryStorageCollection(ToS, order);
                        break;
                    }

                case TradingMode.Margin:
                    {
                        AddSingleOrderToMemoryStorageCollection(ToM, order);
                        break;
                    }

                case TradingMode.Isolated:
                    {
                        AddSingleOrderToMemoryStorageCollection(ToI, order);
                        break;
                    }
                default:
                    WriteLog.Error("Trading Mode information was Expected while loading Stored Order: " + order.OrderId);
                    break;
            }
        }

        private void AddSingleOrderToMemoryStorageCollection(List<OrderBase>? OrdersToStore, OrderBase order)
        {
            if (OrdersToStore == null)
            {
                return;
            }

            var orderExists = OrdersToStore.Where(s => s.OrderId == order.OrderId).FirstOrDefault();
            if (orderExists == null)
            {
                OrdersToStore.Add(order);
            }
            else
            {
                orderExists.Price = order.Price;

                orderExists.QuantityFilled = order.QuantityFilled;
                orderExists.Quantity = order.Quantity;
                orderExists.Fulfilled = order.Fulfilled;
                orderExists.CumulativeQuoteQuantityFilled = order.CumulativeQuoteQuantityFilled;

                orderExists.Status = order.Status;
                orderExists.Pnl = order.Pnl;

                orderExists.Fee = order.Fee;
                orderExists.MinPos = order.MinPos;

                orderExists.InterestToDate = order.InterestToDate;
                orderExists.InterestToDateQuote = order.InterestToDateQuote;
                orderExists.InterestPerHour = order.InterestPerHour;
                orderExists.InterestPerDay = order.InterestPerDay;

                orderExists.ResetTime = order.ResetTime;
            }
        }

        #endregion [ Add ]

        #region [ Save ]

        private bool WriteOrdersToFileStorage(List<OrderBase> OrdersToStore, string OrdersToStoreFile)
        {
            if (OrdersToStore != null && OrdersToStore.Count() > 0)
            {
                var saved = TJson.Save(OrdersToStore, OrdersToStoreFile);
                if (saved)
                {
                    return true;
                }
            }

            return false;
        }

        public void WriteAllOrderCollectionsToFileStorage()
        {
            if (!WriteOrdersToFileStorage(ToS, App.StoredSpotOrders))
            {
                WriteLog.Info("No Reason to Update Stored Spot Orders");
            }

            if (!WriteOrdersToFileStorage(ToM, App.StoredMarginOrders))
            {
                WriteLog.Info("No Reason to Update Stored Margin Orders");
            }

            if (!WriteOrdersToFileStorage(ToI, App.StoredIsolatedOrders))
            {
                WriteLog.Info("No Reason to Update Stored Isolated Orders");
            }
        }

        #endregion [ Save ]
    }
}
