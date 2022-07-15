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

using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BTNET.VM.Views
{
    public partial class OrderDetailView : Window
    {
        private readonly OrderBase CurrentOrder;
        private Ticker? SymbolTickerFeed;

        public OrderDetailView(OrderBase initialOrder)
        {
            CurrentOrder = GetOrderContextFromStorage(initialOrder);
            DataContext = CurrentOrder;

            SymbolTickerFeed = Tickers.AddTicker(CurrentOrder.Symbol);
            SymbolTickerFeed.TickerUpdated += TickerUpdated;
            InitializeComponent();
        }

        ~OrderDetailView()
        {
            StopDetailTicker();
        }

        public OrderBase GetOrderContextFromStorage(OrderBase order)
        {
            switch (order.Helper!.OrderTradingMode)
            {
                case TradingMode.Spot:
                    order = Stored.ToS.Where(s => s.OrderId == order.OrderId).FirstOrDefault();
                    break;

                case TradingMode.Margin:
                    order = Stored.ToM.Where(s => s.OrderId == order.OrderId).FirstOrDefault();
                    break;

                case TradingMode.Isolated:
                    order = Stored.ToI.Where(s => s.OrderId == order.OrderId).FirstOrDefault();
                    break;

                default: return order;
            }

            return order;
        }

        private void StopDetailTicker()
        {
            if (SymbolTickerFeed != null)
            {
                SymbolTickerFeed.TickerUpdated -= TickerUpdated;
                SymbolTickerFeed = null;
            }
        }

        public void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            try
            {
                InvokeUI.CheckAccess(() =>
                {
                    CurrentOrder.Fulfilled = OrderHelper.Fulfilled(CurrentOrder.Quantity, CurrentOrder.QuantityFilled);
                    CurrentOrder.Pnl = decimal.Round(OrderHelper.PnL(CurrentOrder, e.BestAsk, e.BestBid), App.DEFAULT_ROUNDING_PLACES);
                });
            }
            catch (Exception ex)
            {
                if (ex.Message != "Collection was modified; enumeration operation may not execute.") // Ignore
                {
                    WriteLog.Error("UpdatePnl Error: " + ex.Message + "| HRESULT: " + ex.HResult);
                }
            }
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;

            DragMove();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            StopDetailTicker();
            DataContext = null;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-pressed.png"));
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleButton(sender);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButton(sender);
        }

        internal void ToggleButton(object sender)
        {
            var s = sender as Image;
            if (!this.Topmost)
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top.png"));
                }
            }
            else
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
                }
            }
        }
    }
}
