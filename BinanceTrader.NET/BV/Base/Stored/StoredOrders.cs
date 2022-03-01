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

using BTNET.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using static BTNET.BVVM.Stored;

namespace BTNET.Base
{
    public class StoredOrders : ObservableObject
    {
        public bool IsUpdatingStoredOrders { get; set; } = false;
        public bool IsLoadingStoredOrders { get; set; } = false;

        #region [ Load ]

        public void LoadStoredOrdersFromFile()
        {
            Directory.CreateDirectory("c:\\BNET\\Orders\\");

            ToS = LoadStoredOrders(storedSpotOrders);
            ToM = LoadStoredOrders(storedMarginOrders);
            ToI = LoadStoredOrders(storedIsolatedOrders);

            if (ToS.Count > 0 || ToM.Count > 0 || ToI.Count > 0)
            {
                MiniLog.AddLine("Loaded Stored Orders..");
            }
        }

        private ObservableCollection<OrderBase> LoadStoredOrders(string storedOrdersString)
        {
            if (File.Exists(storedOrdersString))
            {
                try
                {
                    string _storedOrders = File.ReadAllText(storedOrdersString);
                    if (_storedOrders != null)
                    {
                        var _dstoredOrders = JsonConvert.DeserializeObject<ObservableCollection<OrderBase>>(_storedOrders);

                        if (_dstoredOrders != null)
                        {
                            return _dstoredOrders;
                        }
                        else
                        {
                            WriteLog.Error("StoredOrders: TempOrders was Null");
                            return new();
                        }
                    }
                    else
                    {
                        MiniLog.AddLine("No Stored Orders");
                        WriteLog.Info("No stored Orders");
                        return new();
                    }
                }
                catch (System.Security.SecurityException)
                {
                    _ = Static.MessageBox.ShowMessage($"Why did you do this", "Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return new();
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Stored Orders: ", ex);
                    return new();
                }
            }

            return new();
        }

        public ObservableCollection<OrderBase> LoadStoredOrdersCurrentSymbol(ObservableCollection<OrderBase> orders)
        {
            if (IsLoadingStoredOrders)
            {
                if ((TempOrders != null) && (TempOrders.Count > 0))
                {
                    return TempOrders;
                }
            }

            return orders != null ? orders : new();
        }

        #endregion [ Load ]

        #region [ Get ]

        /// <summary>
        /// Get Stored Orders for the Current Symbol
        /// </summary>
        /// <param name="storedOrders">The stored orders</param>
        /// <param name="filterDeleted">Filter deleted orders from the stored orders</param>
        private void GetStoredOrdersCurrentSymbol(List<long> DeletedList, ObservableCollection<OrderBase> storedOrders)
        {
            if (storedOrders != null)
            {
                TempOrders = new ObservableCollection<OrderBase>(storedOrders.Where(s => s.Symbol == Static.GetCurrentlySelectedSymbol.SymbolView.Symbol).Where(s => !DeletedList.Contains(s.OrderId)));

                if (TempOrders != null && TempOrders.Count > 0)
                {
                    IsLoadingStoredOrders = true;
                    WriteLog.Info("Loaded [" + storedOrders.Count(s => s.Symbol == Static.GetCurrentlySelectedSymbol.SymbolView.Symbol) + "] Orders from file");

                    return;
                }
            }
            else
            {
                IsLoadingStoredOrders = false;
                TempOrders = null;
                WriteLog.Error("Stored Orders was Null while setting Orders");
            }
        }

        /// <summary>
        /// Current Symbol and Mode Stored Orders
        /// </summary>
        /// <param name="DeletedList">list of previously deleted orders</param>
        public void CurrentSymbolModeStoredOrders(List<long> DeletedList)
        {
            // Load stored orders for current symbol
            switch (Static.CurrentTradingMode)
            {
                // S
                case TradingMode.Spot:
                    GetStoredOrdersCurrentSymbol(DeletedList, ToS);
                    break;

                // M
                case TradingMode.Margin:
                    GetStoredOrdersCurrentSymbol(DeletedList, ToM);
                    break;

                // I
                case TradingMode.Isolated:
                    GetStoredOrdersCurrentSymbol(DeletedList, ToI);
                    break;

                default:
                    // Error
                    WriteLog.Error("StoredOrders: Mode wasn't selected during symbol change");
                    MiniLog.AddLine("Stored Orders Wont Load");
                    break;
            }
        }

        #endregion [ Get ]

        #region [ Add ]

        public bool UpdateStoredOrdersAndRefresh(ObservableCollection<OrderBase> OrderUpdate)
        {
            if (IsUpdatingStoredOrders || IsLoadingStoredOrders)
            {
                AddCurrentOrdersToCollections(OrderUpdate);
                IsUpdatingStoredOrders = false;
                IsLoadingStoredOrders = false;

                return true;
            }
            return false;
        }

        public void AddCurrentOrdersToCollections(ObservableCollection<OrderBase> orders)
        {
            switch (Static.CurrentTradingMode)
            {
                // Spot Orders
                case TradingMode.Spot:
                    {
                        ToS = AddCurrent(ToS, orders);
                        break;
                    }
                // Margin Orders
                case TradingMode.Margin:
                    {
                        ToM = AddCurrent(ToM, orders);
                        break;
                    }
                // Isolated Orders
                case TradingMode.Isolated:
                    {
                        ToI = AddCurrent(ToI, orders);
                        break;
                    }
                default:
                    {
                        // Error
                        WriteLog.Error("StoredOrders: Mode wasn't selected during order save");
                        break;
                    }
            }
        }

        private ObservableCollection<OrderBase> AddCurrent(ObservableCollection<OrderBase> OrdersToStore, ObservableCollection<OrderBase> orders)
        {
            if (OrdersToStore == null) { return null; }

            if (orders == null) { WriteLog.Error("No New Orders to Add to Collections.."); return OrdersToStore; }

            foreach (var order in orders)
            {
                AddSingleOrderToCollection(OrdersToStore, order);
            }

            return OrdersToStore;
        }

        public void AddSingleOrder(OrderBase order)
        {
            switch (Static.CurrentTradingMode)
            {
                case TradingMode.Spot:
                    {
                        AddSingleOrderToCollection(ToS, order);
                        break;
                    }

                case TradingMode.Margin:
                    {
                        AddSingleOrderToCollection(ToM, order);

                        break;
                    }

                case TradingMode.Isolated:
                    {
                        AddSingleOrderToCollection(ToI, order);
                        break;
                    }

                default:
                    break;
            }
        }

        private void AddSingleOrderToCollection(ObservableCollection<OrderBase> OrdersToStore, OrderBase order)
        {
            var orderExists = OrdersToStore.Where(s => s.OrderId == order.OrderId);
            if (!orderExists.Any())
            {
                OrdersToStore.Add(order);
            }
            else
            {
                foreach (var o in orderExists)
                {
                    o.Price = order.Price;
                    o.QuantityFilled = order.QuantityFilled;
                    o.Quantity = order.Quantity;
                    o.Status = order.Status;
                    o.TimeInForce = order.TimeInForce;

                    o.MinPos = order.MinPos;
                    o.OrderFee = order.OrderFee;
                    o.Fee = order.Fee;
                    o.Pnl = order.Pnl;

                    o.ITD = order.ITD;
                    o.ITDQ = order.ITDQ;
                    o.IPH = order.IPH;
                    o.IPD = order.IPD;
                }
            }
        }

        #endregion [ Add ]

        #region [ Save ]

        private bool StoreOrders(ObservableCollection<OrderBase> OrdersToStore, string OrdersToStoreFile)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new TypeOnlyContractResolver<OrderBase>()
            };

            if (OrdersToStore != null && OrdersToStore.Count() > 0)
            {
                var convert = JsonConvert.SerializeObject(OrdersToStore, Formatting.Indented, jsonSettings);

                File.WriteAllText(OrdersToStoreFile, convert);
                return true;
            }

            return false;
        }

        public void StoreAllOrderCollections()
        {
            if (!StoreOrders(ToS, storedSpotOrders)) { WriteLog.Info("No Reason to Update Stored Spot Orders"); }

            if (!StoreOrders(ToM, storedMarginOrders)) { WriteLog.Info("No Reason to Update Stored Margin Orders"); }

            if (!StoreOrders(ToI, storedIsolatedOrders)) { WriteLog.Info("No Reason to Update Stored Isolated Orders"); }
        }

        #endregion [ Save ]
    }
}