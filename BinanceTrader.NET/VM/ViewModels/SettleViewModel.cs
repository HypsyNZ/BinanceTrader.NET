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

using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.HELPERS;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class SettleViewModel : ObservableObject
    {
        public ICommand SettleBaseCommand { get; set; }
        public ICommand SettleAllCommand { get; set; }
        public ICommand SettleQuoteCommand { get; set; }

        public void InitializeCommands()
        {
            SettleBaseCommand = new DelegateCommand(SettleBase);
            SettleQuoteCommand = new DelegateCommand(SettleQuote);
            SettleAllCommand = new DelegateCommand(SettleAll);
        }

        public SettleViewModel()
        {
        }

        private bool cra, crb, crq;

        public bool CanRepayAll
        { get => this.cra; set { this.cra = value; PC(); } }

        public bool CanRepayBase
        { get => this.crb; set { this.crb = value; PC(); } }

        public bool CanRepayQuote
        { get => this.crq; set { this.crq = value; PC(); } }

        public void CheckRepay()
        {
#if DEBUG
            CanRepayBase = true;
            CanRepayQuote = true;
            CanRepayAll = true;
            return;
#else
            CanRepayBase = BorrowVM.FreeBase > 0 && BorrowVM.BorrowedBase > 0;
            CanRepayQuote = BorrowVM.FreeQuote > 0 && BorrowVM.BorrowedQuote > 0;
            CanRepayAll = this.CanRepayBase && this.CanRepayQuote;
#endif
        }

        private void SettleBase(object o)
        {
            _ = Settle.SettleAsset(BorrowVM.FreeBase, BorrowVM.BorrowedBase, Static.CurrentSymbolInfo.BaseAsset, Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
        }

        private void SettleQuote(object o)
        {
            _ = Settle.SettleAsset(BorrowVM.FreeQuote, BorrowVM.BorrowedQuote, Static.CurrentSymbolInfo.QuoteAsset, Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
        }

        private void SettleAll(object o)
        {
            var s = Settle.SettleAsset(BorrowVM.FreeBase, BorrowVM.BorrowedBase, Static.CurrentSymbolInfo.BaseAsset, Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
            var s2 = Settle.SettleAsset(BorrowVM.FreeQuote, BorrowVM.BorrowedQuote, Static.CurrentSymbolInfo.QuoteAsset, Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);

            if (s.Result & s2.Result)
            {
                MiniLog.AddLine("Settled Both Assets!");
            }
        }

        public void Clear()
        {
            CanRepayBase = false;
            CanRepayQuote = false;
            CanRepayAll = false;
        }
    }
}