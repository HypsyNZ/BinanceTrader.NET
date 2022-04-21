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
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderViewModel : ObservableObject
    {
        public OrderSide Side { get; set; }
        public OrderStatus Status { get; set; }
        private OrderTasksViewModel orderTasks = new();

        public ICommand CancelCommand { get; set; }

        public ICommand ResetInterestCommand { get; set; }

        public OrderViewModel(OrderSide side, OrderStatus status)
        {
            OrderTasks.InitializeCommands();
            CancelCommand = new DelegateCommand(Cancel);
            ResetInterestCommand = new DelegateCommand(ResetInterest);
            Status = status;
            Side = side;
        }

        public OrderTasksViewModel OrderTasks
        { get => this.orderTasks; set { this.orderTasks = value; PC(); } }

        public bool IsOrderBuySide => Side is OrderSide.Buy;

        public bool IsOrderSellSide => Side is OrderSide.Sell;

        public bool IsOrderBuySideMargin => Side is OrderSide.Buy && Static.CurrentTradingMode != TradingMode.Spot;

        public bool IsOrderSellSideMargin => Side is OrderSide.Sell && Static.CurrentTradingMode != TradingMode.Spot;

        public bool CanCancel => Status is OrderStatus.New or OrderStatus.PartiallyFilled;

        public System.Windows.Data.BindingBase TargetNullValue => null;

        /// <summary>
        /// Cancel an Order
        /// </summary>
        /// <param name="o">The currently selected Order</param>
        public void Cancel(object o)
        {
            var order = (OrderBase)o;
            if (order == null)
            {
                WriteLog.Info("Order Cancel Failed: " + order.OrderId);
                _ = Static.MessageBox.ShowMessage($"Order canceling Failed", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _ = Task.Run(() =>
            {
                var result = Static.CurrentTradingMode switch
                {
                    TradingMode.Spot => BTClient.Local.Spot.Order.CancelOrderAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, order.OrderId, receiveWindow: 1000),
                    TradingMode.Margin => BTClient.Local.Margin.Order.CancelMarginOrderAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, order.OrderId, receiveWindow: 1000),
                    TradingMode.Isolated => BTClient.Local.Margin.Order.CancelMarginOrderAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, order.OrderId, receiveWindow: 1000, isIsolated: true),
                    _ => null,
                };
                if (result != null && result.Result.Success)
                {
                    Static.DeletedList.Add(order.OrderId);
                    WriteLog.Info("Order Cancelled and Hidden: " + order.OrderId);
                }
                else
                {
                    WriteLog.Info("Order Cancel Failed: " + order.OrderId);
                    _ = Static.MessageBox.ShowMessage($"Order canceling failed: {result.Result.Error.Message}", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Resets the running interest on an Order
        /// This data is saved and included in backups
        /// </summary>
        /// <param name="o">The currently selected Order</param>
        public void ResetInterest(object o)
        {
            var order = (OrderBase)o;
            order.ResetTime = DateTime.UtcNow;
        }
    }
}