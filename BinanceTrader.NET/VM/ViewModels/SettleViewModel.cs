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

using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class SettleViewModel : ObservableObject
    {
        public ICommand? SettleBaseCommand { get; set; }
        public ICommand? SettleAllCommand { get; set; }
        public ICommand? SettleQuoteCommand { get; set; }

        public void InitializeCommands()
        {
            SettleBaseCommand = new DelegateCommand(SettleBase);
            SettleQuoteCommand = new DelegateCommand(SettleQuote);
            SettleAllCommand = new DelegateCommand(SettleAll);
        }

        private bool cra;
        private bool crb;
        private bool crq;

        public bool CanRepayAll
        {
            get => this.cra;
            set
            {
                this.cra = value;
                PC();
            }
        }

        public bool CanRepayBase
        {
            get => this.crb;
            set
            {
                this.crb = value;
                PC();
            }
        }

        public bool CanRepayQuote
        {
            get => this.crq;
            set
            {
                this.crq = value;
                PC();
            }
        }

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
            CanRepayAll = CanRepayBase && CanRepayQuote;
#endif
        }

        private void SettleBase(object o)
        {
            _ = InternalOrderTasks.SettleAssetAsync(BorrowVM.FreeBase, BorrowVM.BorrowedBase, Static.CurrentSymbolInfo!.BaseAsset,
                Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
        }

        private void SettleQuote(object o)
        {
            _ = InternalOrderTasks.SettleAssetAsync(BorrowVM.FreeQuote, BorrowVM.BorrowedQuote, Static.CurrentSymbolInfo!.QuoteAsset,
                Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
        }

        private void SettleAll(object o)
        {
            var s = InternalOrderTasks.SettleAssetAsync(BorrowVM.FreeBase, BorrowVM.BorrowedBase, Static.CurrentSymbolInfo!.BaseAsset,
                Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);
            var s2 = InternalOrderTasks.SettleAssetAsync(BorrowVM.FreeQuote, BorrowVM.BorrowedQuote, Static.CurrentSymbolInfo!.QuoteAsset,
                Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin);

            if (s.Result & s2.Result)
            {
                WriteLog.Info("Settled Both Assets Sucessfully!");
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
