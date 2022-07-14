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
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class BorrowViewModel : ObservableObject
    {
        private const string NOT_FOUND = "Not Found";

        public ICommand? SettleBaseCommand { get; set; }
        public ICommand? SettleAllCommand { get; set; }
        public ICommand? SettleQuoteCommand { get; set; }

        public ICommand? BorrowToggleCommand { get; set; }

        public void InitializeCommands()
        {
            BorrowToggleCommand = new DelegateCommand(BorrowToggleBasedOnTab);
        }

        public BorrowViewModel()
        {
            MarginLevel = this.marginlevel;

            BorrowLabelBase = this.LabelBase;
            BorrowedBase = this.borrowedbase;
            InterestBase = this.interestbase;

            SymbolName = this.LabelSymbolName;

            BorrowLabelQuote = this.LabelQuote;
            BorrowedQuote = this.borrowedquote;
            InterestQuote = this.interestquote;

            TotalNetAssetOfBtc = this.totalNetAssetOfBtc;
            TotalLiabilityOfBtc = this.totalLiabilityOfBtc;
            TotalAssetOfBtc = this.totalAssetOfBtc;
        }

        #region [ Properties ]

        private decimal interestquote;
        private decimal borrowedquote;
        private decimal interestbase;
        private decimal borrowedbase;
        private decimal marginlevel;
        private decimal liquidationprice;

        private decimal totalAssetOfBtc;
        private decimal totalLiabilityOfBtc;
        private decimal totalNetAssetOfBtc;

        private decimal lockedbase;
        private decimal freequote;
        private decimal lockedquote;
        private decimal freebase;
        private decimal totalbase;
        private decimal totalquote;

        private bool borrowSell;
        private bool borrowBuy;
        private bool borrowInfoVisible;
        private bool showBreakdown;

        private string s1 = "";
        private string s2 = "";
        private string LabelSymbolName = "NaN";
        private string LabelBase = "Not Found";
        private string LabelQuote = "Not Found";
        private bool marginInfoVisible;
        private bool isolatedInfoVisible;
        private bool borrowCheckboxToggle;

        #endregion [ Properties ]

        #region [ Borrow ]

        public decimal LiquidationPrice
        {
            get => this.liquidationprice;
            set
            {
                this.liquidationprice = value;
                PC();
            }
        }

        public decimal MarginLevel
        {
            get => this.marginlevel;
            set
            {
                this.marginlevel = value;
                PC();
            }
        }

        public string SymbolName
        {
            get => this.LabelSymbolName;
            set
            {
                string s = value;
                this.LabelSymbolName = s;
                PC();
            }
        }

        public string BorrowLabelBase
        {
            get => this.LabelBase;
            set
            {
                string s = value;
                this.LabelBase = s;
                BorrowLabelBaseFree = s;
                PC();
            }
        }

        public string BorrowLabelBaseFree
        {
            get => this.s2;
            set
            {
                string s = value;
                this.s2 = s + " Free";
                PC();
            }
        }

        public decimal InterestBase
        {
            get => this.interestbase;
            set
            {
                this.interestbase = value;
                PC();
            }
        }

        public decimal BorrowedBase
        {
            get => this.borrowedbase;
            set
            {
                this.borrowedbase = value;
                PC();
            }
        }

        public decimal FreeBase
        {
            get => this.freebase;
            set
            {
                this.freebase = value;
                PC();
            }
        }

        public decimal LockedBase
        {
            get => this.lockedbase;
            set
            {
                this.lockedbase = value;
                PC();
            }
        }

        public decimal TotalBase
        {
            get => this.totalbase;
            set
            {
                this.totalbase = value;
                PC();
            }
        }

        public string BorrowLabelQuote
        {
            get => this.LabelQuote;
            set
            {
                string s = value;
                this.LabelQuote = s;
                BorrowLabelQuoteFree = s;
                PC();
            }
        }

        public string BorrowLabelQuoteFree
        {
            get => this.s1;
            set
            {
                string s = value;
                this.s1 = s + " Free";
                PC();
            }
        }

        public decimal InterestQuote
        {
            get => this.interestquote;
            set
            {
                this.interestquote = value;
                PC();
            }
        }

        public decimal BorrowedQuote
        {
            get => this.borrowedquote;
            set
            {
                this.borrowedquote = value;
                PC();
            }
        }

        public decimal FreeQuote
        {
            get => this.freequote;
            set
            {
                this.freequote = value;
                PC();
            }
        }

        public decimal LockedQuote
        {
            get => this.lockedquote;
            set
            {
                this.lockedquote = value;
                PC();
            }
        }

        public decimal TotalQuote
        {
            get => this.totalquote;
            set
            {
                this.totalquote = value;
                PC();
            }
        }

        public decimal TotalAssetOfBtc
        {
            get => this.totalAssetOfBtc;
            set
            {
                this.totalAssetOfBtc = value;
                PC();
            }
        }

        public decimal TotalLiabilityOfBtc
        {
            get => this.totalLiabilityOfBtc;
            set
            {
                this.totalLiabilityOfBtc = value;
                PC();
            }
        }

        public decimal TotalNetAssetOfBtc
        {
            get => this.totalNetAssetOfBtc;
            set
            {
                this.totalNetAssetOfBtc = value;
                PC();
            }
        }

        public bool QuoteLockedVisible
        {
            get => LockedQuote != 0;
            set => PC();
        }

        public bool QuoteFreeVisible
        {
            get => FreeQuote != 0;
            set => PC();
        }

        public bool QuoteTotalVisible
        {
            get => TotalQuote != 0;
            set => PC();
        }

        public bool QuoteInterestVisible
        {
            get => InterestQuote != 0;
            set => PC();
        }

        public bool QuoteBorrowVisible
        {
            get => BorrowedQuote != 0;
            set => PC();
        }

        public bool BaseLockedVisible
        {
            get => LockedBase != 0;
            set => PC();
        }

        public bool BaseFreeVisible
        {
            get => FreeBase != 0;
            set => PC();
        }

        public bool BaseTotalVisible
        {
            get => TotalBase != 0;
            set => PC();
        }

        public bool BaseInterestVisible
        {
            get => InterestBase != 0;
            set => PC();
        }

        public bool BaseBorrowVisible
        {
            get => BorrowedBase != 0;
            set => PC();
        }

        public bool QuoteVisible
        {
            get => BorrowedQuote != 0 || InterestQuote != 0 || FreeQuote != 0 || LockedQuote != 0;
            set => PC();
        }

        public bool BaseVisible
        {
            get => BorrowedBase != 0 || InterestBase != 0 || FreeBase != 0 || LockedBase != 0;
            set => PC();
        }

        public bool ShowBreakdown
        {
            get => showBreakdown;
            set
            {
                showBreakdown = value;
                PC();
            }
        }

        public bool BorrowInfoVisible
        {
            get => borrowInfoVisible;
            set
            {
                borrowInfoVisible = value;
                PC();
            }
        }

        public bool MarginInfoVisible
        {
            get => marginInfoVisible;
            set
            {
                marginInfoVisible = MainVM.IsMargin ? value : false;
                PC();
            }
        }

        public bool IsolatedInfoVisible
        {
            get => isolatedInfoVisible;
            set
            {
                isolatedInfoVisible = MainVM.IsIsolated ? value : false;
                PC();
            }
        }

        public string BorrowInformationHeader
        {
            get => Static.CurrentTradingMode == TradingMode.Spot ? "Asset Info" : "Borrow Info";
            set { PC(); }
        }

        public bool BorrowSell
        {
            get => this.borrowSell;
            set
            {
                this.borrowSell = value;
                PC();
            }
        }

        public bool BorrowBuy
        {
            get => this.borrowBuy;
            set
            {
                this.borrowBuy = value;
                PC();
            }
        }

        public bool BorrowCheckboxToggle
        {
            get => borrowCheckboxToggle;
            set
            {
                borrowCheckboxToggle = value;
                PC();
            }
        }

        public void Clear()
        {
            BorrowInformationHeader = "";

            MarginLevel = 0;

            this.LabelBase = NOT_FOUND;
            BorrowedBase = 0;
            InterestBase = 0;
            FreeBase = 0;
            LockedBase = 0;
            TotalBase = 0;

            this.LabelQuote = NOT_FOUND;
            BorrowedQuote = 0;
            InterestQuote = 0;
            FreeQuote = 0;
            LockedQuote = 0;
            TotalQuote = 0;

            TotalNetAssetOfBtc = 0;
            TotalLiabilityOfBtc = 0;
            TotalAssetOfBtc = 0;

            ShowBreakdown = false;
            BorrowInfoVisible = false;
            QuoteVisible = false;
            BaseVisible = false;

            BorrowVisibility();
        }

        public void BorrowVisibility()
        {
            QuoteLockedVisible = true;
            QuoteFreeVisible = true;
            QuoteInterestVisible = true;
            QuoteBorrowVisible = true;
            QuoteTotalVisible = true;

            BaseLockedVisible = true;
            BaseFreeVisible = true;
            BaseInterestVisible = true;
            BaseBorrowVisible = true;
            BaseTotalVisible = true;

            ShowBreakdown = SettingsVM.ShowBreakDownInfoIsChecked ?? false;
            MarginInfoVisible = SettingsVM.ShowMarginInfoIsChecked ?? false;
            IsolatedInfoVisible = SettingsVM.ShowIsolatedInfoIsChecked ?? false;

            if (BaseVisible || QuoteVisible)
            {
                BorrowInfoVisible = SettingsVM.ShowBorrowInfoIsChecked ?? false;
                return;
            }
            BorrowInfoVisible = false;
        }

        public void BorrowVMOnTabChanged(object sender, EventArgs args)
        {
            switch (Static.CurrentlySelectedSymbolTab)
            {
                case SelectedTab.Settle:
                    return;

                case SelectedTab.Buy:
                    BorrowCheckboxToggle = BorrowBuy;
                    break;

                case SelectedTab.Sell:
                    BorrowCheckboxToggle = BorrowSell;
                    break;

                default:
                    WriteLog.Error("Selected Tab Error: Unexpected Exception");
                    break;
            }
        }

        public void BorrowToggleBasedOnTab(object o)
        {
            switch (Static.CurrentlySelectedSymbolTab)
            {
                case SelectedTab.Settle:
                    return;

                case SelectedTab.Buy:
                    BorrowBuy = !BorrowBuy;
                    PC("BorrowBuy");
                    break;

                case SelectedTab.Sell:
                    BorrowSell = !BorrowSell;
                    PC("BorrowSell");
                    break;
            }
        }

        #endregion [ Borrow ]
    }
}
