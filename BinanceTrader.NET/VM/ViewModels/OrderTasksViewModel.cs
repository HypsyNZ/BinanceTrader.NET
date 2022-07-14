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
using BTNET.BV.Base;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderTasksViewModel : ObservableObject
    {
        public ICommand? BuyCommand { get; set; }
        public ICommand? BuyAndSettleCommand { get; set; }
        public ICommand? BuyBorrowAndSettleCommand { get; set; }

        public ICommand? SellCommand { get; set; }
        public ICommand? SellAndSettleCommand { get; set; }
        public ICommand? SellBorrowAndSettleCommand { get; set; }

        public void InitializeCommands()
        {
            BuyCommand = new DelegateCommand(Buy);
            BuyAndSettleCommand = new DelegateCommand(BuyAndSettle);
            BuyBorrowAndSettleCommand = new DelegateCommand(BuyBorrowAndSettle);

            SellCommand = new DelegateCommand(Sell);
            SellAndSettleCommand = new DelegateCommand(SellAndSettle);
            SellBorrowAndSettleCommand = new DelegateCommand(SellBorrowAndSettle);
        }

        private void Buy(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Buy, false, order.Helper!.OrderTradingMode, false);
        }

        private void BuyAndSettle(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder((OrderBase)o, OrderSide.Buy, false, order.Helper!.OrderTradingMode);
        }

        private void BuyBorrowAndSettle(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder((OrderBase)o, OrderSide.Buy, true, order.Helper!.OrderTradingMode);
        }

        private void Sell(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Sell, false, order.Helper!.OrderTradingMode, false);
        }

        private void SellAndSettle(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder((OrderBase)o, OrderSide.Sell, false, order.Helper!.OrderTradingMode);
        }

        private void SellBorrowAndSettle(object o)
        {
            var order = (OrderBase)o;
            InternalOrderTasks.ProcessOrder((OrderBase)o, OrderSide.Sell, true, order.Helper!.OrderTradingMode);
        }
    }
}
