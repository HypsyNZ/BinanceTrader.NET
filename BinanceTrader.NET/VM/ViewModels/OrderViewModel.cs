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

using BinanceAPI;
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderViewModel : ObservableObject
    {
        public ICommand CancelCommand { get; set; }

        public ICommand OptionsCommand { get; set; }

        public ICommand ResetInterestCommand { get; set; }

        public ICommand CloseWindowCommand { get; set; }

        public ICommand SettleOrderToggleCommand { get; set; }

        public ICommand BorrowForSettleToggleCommand { get; set; }

        public OrderDetailView? OrderDetail { get; set; }

        public OrderViewModel(OrderSide side, TradingMode tradingMode, string symbol)
        {
            OrderTradingMode = tradingMode;
            _side = side;

            OrderTasks.InitializeCommands();
            CancelCommand = new DelegateCommand(Cancel);
            OptionsCommand = new DelegateCommand(OrderOptions);
            ResetInterestCommand = new DelegateCommand(ResetInterest);
            CloseWindowCommand = new DelegateCommand(CloseWindow);
            SettleOrderToggleCommand = new DelegateCommand(ToggleSettleOrder);
            BorrowForSettleToggleCommand = new DelegateCommand(BorrowForSettleOrder);

            DisplayTradingMode = OrderTradingMode == TradingMode.Spot
                ? "Spot: " + symbol
                : OrderTradingMode == TradingMode.Margin
                ? "Margin: " + symbol
                : OrderTradingMode == TradingMode.Isolated
                ? "Isolated: " + symbol
                : "Error Don't Attempt Tasks!";

            _symbol = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);
            StepSize = _symbol?.LotSizeFilter?.StepSize ?? App.DEFAULT_STEP;
        }

        public OrderTasksViewModel OrderTasks
        {
            get => this.orderTasks;
            set
            {
                this.orderTasks = value;
                PC();
            }
        }

        public TradingMode OrderTradingMode { get; }

        public string DisplayTradingMode { get; }

        public bool IsOrderBuySide => _side is OrderSide.Buy;

        public bool IsOrderSellSide => _side is OrderSide.Sell;

        public bool IsOrderBuySideMargin => _side is OrderSide.Buy && OrderTradingMode != TradingMode.Spot;

        public bool IsOrderSellSideMargin => _side is OrderSide.Sell && OrderTradingMode != TradingMode.Spot;

        public bool IsNotSpot => OrderTradingMode != TradingMode.Spot;

        public decimal StepSize
        {
            get => stepSize;
            set
            {
                stepSize = value.Normalize();
                PC();
            }
        }

        public bool ToggleSettleChecked
        {
            get => toggleSettleChecked;
            set
            {
                toggleSettleChecked = value;
                PC();
            }
        }

        public bool BorrowForSettleChecked
        {
            get => borrowForSettle;
            set
            {
                borrowForSettle = value;
                PC();
            }
        }

        public bool SettleControlsEnabled
        {
            get => settleControlsEnabled;
            set
            {
                settleControlsEnabled = value;
                PC();
            }
        }

        public bool SettleOrderEnabled
        {
            get => settleOrderEnabled;
            set
            {
                settleOrderEnabled = value;
                PC();
            }
        }

        public decimal SettlePercent
        {
            get => settlePercent;
            set
            {
                settlePercent = value;
                PC();
            }
        }

        public decimal QuantityModifier
        {
            get => quantityModifier;
            set
            {
                quantityModifier = value;
                PC();
            }
        }

        public ComboBoxItem? SettleMode
        {
            get => settleMode;
            set
            {
                settleMode = value;
                PC();
            }
        }

        public void BorrowForSettleOrder(object o)
        {
            if (_orderref != null)
            {
                BorrowForSettleChecked = !BorrowForSettleChecked;
            }
            else
            {
                BorrowForSettleChecked = false;
            }
        }

        public void ToggleSettleOrder(object o)
        {
            if (_orderref != null)
            {
                ToggleSettleChecked = !ToggleSettleChecked;
                SettleControlsEnabled = !ToggleSettleChecked;

                if (ToggleSettleChecked && !_block)
                {
                    if (_orderref.Status != OrderStatus.Filled)
                    {
                        WriteLog.Error("OrderId: " + _orderref.OrderId + " with status [" + _orderref.Status + "] is not a valid order for this feature");
                    }
                    else
                    {
                        bool cumulative = false;
                        decimal total = 0;

                        if (_orderref!.CumulativeQuoteQuantityFilled != 0)
                        {
                            total = (_orderref.CumulativeQuoteQuantityFilled / _orderref.QuantityFilled) * _orderref.QuantityFilled;
                            cumulative = true;
                        }
                        else
                        {
                            total = _orderref.Price * _orderref.QuantityFilled;
                        }

                        if (total > 0)
                        {
                            WriteLog.Info("OrderId: " + _orderref.OrderId + " | Total: " + total + " | Desired Settle %: " + SettlePercent + " | Current Pnl: " + _orderref.Pnl + " | Quantity Modifier: " + QuantityModifier + " | Cumulative: " + cumulative);
                            _settleLoop = true;
                            SettleLoop(total);
                            return;
                        }
                        else
                        {
                            WriteLog.Info("OrderId: " + _orderref.OrderId + " | Total: " + total + " - An error occurred while trying to figure out the total");
                        }
                    }
                }
            }

            _settleLoop = false;
            ToggleSettleChecked = false;
        }

        private async void SettleLoop(decimal total)
        {
            while (_settleLoop)
            {
                await Task.Delay(1).ConfigureAwait(false);

                if (!_settleLoop)
                {
                    return;
                }

                if (_orderref!.Pnl > 0)
                {
                    decimal percent = (_orderref.Pnl / total) * 100;

                    if (SettlePercent <= percent)
                    {
                        SettleOrderEnabled = false;

                        InternalOrderTasks.ProcessOrder(_orderref, _orderref.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy, BorrowForSettleChecked, _orderref.Helper!.OrderTradingMode, true, QuantityModifier);

                        ToggleSettleChecked = false;
                        SettleControlsEnabled = false;
                        _settleLoop = false;
                        _block = true;

                        Static.SettledOrders.Add(_orderref.OrderId);

                        WriteLog.Info("OrderId Settled: " + _orderref.OrderId + " | Total: " + total + " | Percent: " + percent + " | SettlePercent: " + SettlePercent + " | Pnl: " + _orderref.Pnl + " | Quantity Modifier: " + QuantityModifier);
                        return;
                    }
                }
            }
        }

        public void Cancel(object o)
        {
            _orderref ??= (OrderBase)o;

            if (_orderref.Symbol != null)
            {
                _ = Task.Run(() =>
                {
                    var result = _orderref.Helper!.OrderTradingMode switch
                    {
                        TradingMode.Spot =>
                        Client.Local.Spot.Order?.CancelOrderAsync(_orderref.Symbol, _orderref.OrderId, receiveWindow: 1000),
                        TradingMode.Margin =>
                        Client.Local.Margin.Order?.CancelMarginOrderAsync(_orderref.Symbol, _orderref.OrderId, receiveWindow: 1000),
                        TradingMode.Isolated =>
                        Client.Local.Margin.Order?.CancelMarginOrderAsync(_orderref.Symbol, _orderref.OrderId, receiveWindow: 1000, isIsolated: true),
                        _ => null,
                    };

                    if (result != null)
                    {
                        if (result.Result.Success)
                        {
                            Static.DeletedList.Add(_orderref.OrderId);
                            WriteLog.Info("Order Cancelled and Hidden: " + _orderref.OrderId);
                        }
                        else
                        {
                            WriteLog.Info("Order Cancel Failed: " + _orderref.OrderId);
                            _ = MessageBox.Show($"Order canceling failed: {(result.Result.Error != null ? result.Result.Error?.Message : "Internal Error")}",
                                "Failed");
                        }
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                WriteLog.Info("Order Cancel Failed");
                _ = MessageBox.Show($"Order canceling Failed", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ResetInterest(object o)
        {
            _orderref ??= (OrderBase)o;
            _orderref.ResetTime = DateTime.UtcNow;
        }

        public void CloseWindow(object o)
        {
            CloseWindow();
        }

        public void CloseWindow()
        {
            OrderDetail?.Close();
            OrderDetail = null;
        }

        public void OrderOptions(object o)
        {
            if (_orderref == null)
            {
                _orderref = (OrderBase)o;
            }

            if (OrderDetail == null)
            {
                OrderDetail = new OrderDetailView(_orderref);
                OrderDetail.Show();
            }
            else
            {
                if (!OrderDetail.IsLoaded)
                {
                    OrderDetail = new OrderDetailView(_orderref);
                    OrderDetail.Show();
                    return;
                }

                OrderDetail?.Close();
                OrderDetail = null;
            }
        }

        private OrderSide _side;
        private OrderBase? _orderref;
        private BinanceSymbol? _symbol;
        private ComboBoxItem? settleMode;
        private OrderTasksViewModel orderTasks = new();
        private bool toggleSettleChecked = false;
        private bool borrowForSettle = false;
        private bool settleControlsEnabled = true;
        private bool settleOrderEnabled = true;
        private volatile bool _settleLoop;
        private volatile bool _block = false;
        private decimal settlePercent = 0.25m;
        private decimal quantityModifier = 0;
        private decimal stepSize;
    }
}
