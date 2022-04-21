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
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderTasksViewModel : ObservableObject
    {
        public ICommand BuyCommand { get; set; }
        public ICommand BuyAndSettleCommand { get; set; }
        public ICommand BuyBorrowAndSettleCommand { get; set; }

        public ICommand SellCommand { get; set; }
        public ICommand SellAndSettleCommand { get; set; }
        public ICommand SellBorrowAndSettleCommand { get; set; }

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
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Buy, false, false);
        }

        private void BuyAndSettle(object o)
        {
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Buy, false);
        }

        private void BuyBorrowAndSettle(object o)
        {
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Buy, true);
        }

        private void Sell(object o)
        {
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Sell, false, false);
        }

        private void SellAndSettle(object o)
        {
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Sell, false);
        }

        private void SellBorrowAndSettle(object o)
        {
            OrderTasks.ProcessOrder((OrderBase)o, OrderSide.Sell, true);
        }

        public OrderTasksViewModel()
        {
        }
    }
}