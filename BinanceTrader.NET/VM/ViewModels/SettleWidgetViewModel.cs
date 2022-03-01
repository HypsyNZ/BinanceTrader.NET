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
using BTNET.Base;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.HELPERS;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class SettleWidgetViewModel : ObservableObject
    {
        public ICommand BuyAndSettleCommand { get; set; }
        public ICommand BuyBorrowAndSettleCommand { get; set; }

        public ICommand SellAndSettleCommand { get; set; }
        public ICommand SellBorrowAndSettleCommand { get; set; }

        public void InitializeCommands()
        {
            BuyAndSettleCommand = new DelegateCommand(BuyAndSettle);
            BuyBorrowAndSettleCommand = new DelegateCommand(BuyBorrowAndSettle);
            SellAndSettleCommand = new DelegateCommand(SellAndSettle);
            SellBorrowAndSettleCommand = new DelegateCommand(SellBorrowAndSettle);
        }

        private void BuyAndSettle(object o)
        {
            Settle.ProcessOrder((OrderBase)o, OrderSide.Buy, false);
        }

        private void BuyBorrowAndSettle(object o)
        {
            Settle.ProcessOrder((OrderBase)o, OrderSide.Buy, true);
        }

        private void SellAndSettle(object o)
        {
            Settle.ProcessOrder((OrderBase)o, OrderSide.Sell, false);
        }

        private void SellBorrowAndSettle(object o)
        {
            Settle.ProcessOrder((OrderBase)o, OrderSide.Sell, true);
        }

        public SettleWidgetViewModel()
        {
        }
    }
}