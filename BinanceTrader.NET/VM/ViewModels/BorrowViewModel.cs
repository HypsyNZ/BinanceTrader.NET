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

using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.HELPERS;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class BorrowViewModel : ObservableObject
    {
        public ICommand BorrowBuyCommand { get; set; }
        public ICommand BorrowSellCommand { get; set; }
        public ICommand SettleBaseCommand { get; set; }
        public ICommand SettleAllCommand { get; set; }
        public ICommand SettleQuoteCommand { get; set; }

        public void InitializeCommands()
        {
            BorrowBuyCommand = new DelegateCommand(BorrowBuyToggle);
            BorrowSellCommand = new DelegateCommand(BorrowSellToggle);
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

        private decimal interestquote, borrowedquote, interestbase, borrowedbase, marginlevel, liquidationprice;
        private decimal totalAssetOfBtc, totalLiabilityOfBtc, totalNetAssetOfBtc;
        private decimal lockedbase, freequote, lockedquote, freebase, totalbase, totalquote;
        private string s1, s2;
        private string LabelSymbolName = "NaN";
        private string LabelBase = "Not Found";
        private string LabelQuote = "Not Found";

        #endregion [ Properties ]

        #region [ Borrow ]

        public decimal LiquidationPrice
        { get => this.liquidationprice; set { this.liquidationprice = value; PC(); } }

        public decimal MarginLevel
        { get => this.marginlevel; set { this.marginlevel = value; PC(); } }

        public string SymbolName
        { get => this.LabelSymbolName; set { string s = value; this.LabelSymbolName = s; PC(); } }

        public string BorrowLabelBase
        { get => this.LabelBase; set { string s = value; this.LabelBase = s; BorrowLabelBaseFree = s; PC(); } }

        public string BorrowLabelBaseFree
        { get => this.s2; set { string s = value; this.s2 = s + " Free"; PC(); } }

        public decimal InterestBase
        { get => this.interestbase; set { this.interestbase = value; PC(); } }

        public decimal BorrowedBase
        { get => this.borrowedbase; set { this.borrowedbase = value; PC(); } }

        public decimal FreeBase
        { get => this.freebase; set { this.freebase = value; PC(); } }

        public decimal LockedBase
        { get => this.lockedbase; set { this.lockedbase = value; PC(); } }

        public decimal TotalBase
        { get => this.totalbase; set { this.totalbase = value; PC(); } }

        public string BorrowLabelQuote
        { get => this.LabelQuote; set { string s = value; this.LabelQuote = s; BorrowLabelQuoteFree = s; PC(); } }

        public string BorrowLabelQuoteFree
        { get => this.s1; set { string s = value; this.s1 = s + " Free"; PC(); } }

        public decimal InterestQuote
        { get => this.interestquote; set { this.interestquote = value; PC(); } }

        public decimal BorrowedQuote
        { get => this.borrowedquote; set { this.borrowedquote = value; PC(); } }

        public decimal FreeQuote
        { get => this.freequote; set { this.freequote = value; PC(); } }

        public decimal LockedQuote
        { get => this.lockedquote; set { this.lockedquote = value; PC(); } }

        public decimal TotalQuote
        { get => this.totalquote; set { this.totalquote = value; PC(); } }

        public decimal TotalAssetOfBtc
        { get => this.totalAssetOfBtc; set { this.totalAssetOfBtc = value; PC(); } }

        public decimal TotalLiabilityOfBtc
        { get => this.totalLiabilityOfBtc; set { this.totalLiabilityOfBtc = value; PC(); } }

        public decimal TotalNetAssetOfBtc
        { get => this.totalNetAssetOfBtc; set { this.totalNetAssetOfBtc = value; PC(); } }

        public bool QuoteLockedVisible { get => LockedQuote != 0; set => PC(); }
        public bool QuoteFreeVisible { get => FreeQuote != 0; set => PC(); }
        public bool QuoteTotalVisible { get => TotalQuote != 0; set => PC(); }

        public bool QuoteInterestVisible { get => InterestQuote != 0; set => PC(); }
        public bool QuoteBorrowVisible { get => BorrowedQuote != 0; set => PC(); }

        public bool BaseLockedVisible { get => LockedBase != 0; set => PC(); }
        public bool BaseFreeVisible { get => FreeBase != 0; set => PC(); }

        public bool BaseTotalVisible { get => TotalBase != 0; set => PC(); }

        public bool BaseInterestVisible { get => InterestBase != 0; set => PC(); }
        public bool BaseBorrowVisible { get => BorrowedBase != 0; set => PC(); }

        public bool QuoteVisible { get => (BorrowedQuote != 0 || InterestQuote != 0 || FreeQuote != 0 || LockedQuote != 0); set => PC(); }

        public bool BaseVisible { get => (BorrowedBase != 0 || InterestBase != 0 || FreeBase != 0 || LockedBase != 0); set => PC(); }

        public bool ShowBreakdown
        { get => showBreakdown; set { showBreakdown = value; PC(); } }

        public bool BorrowInfoVisible
        { get => borrowInfoVisible; set { borrowInfoVisible = value; PC(); } }

        private bool borrowSell = false; public bool BorrowSell
        { get => this.borrowSell; set { this.borrowSell = value; PC(); } }

        private bool borrowBuy = false;
        private bool borrowInfoVisible;
        private bool showBreakdown;

        public string BorrowInformationHeader
        { get => Static.CurrentTradingMode == TradingMode.Spot ? "Asset Info" : "Borrow Info"; set { PC(); } }

        public bool BorrowBuy
        { get => this.borrowBuy; set { this.borrowBuy = value; PC(); } }

        public void Clear()
        {
            ShowBreakdown = false;
            BorrowInformationHeader = "";

            MarginLevel = 0;

            this.LabelBase = "Not Found";
            BorrowedBase = 0;
            InterestBase = 0;
            FreeBase = 0;
            LockedBase = 0;
            TotalBase = 0;

            this.LabelQuote = "Not Found";
            BorrowedQuote = 0;
            InterestQuote = 0;
            FreeQuote = 0;
            LockedQuote = 0;
            TotalQuote = 0;

            TotalNetAssetOfBtc = 0;
            TotalLiabilityOfBtc = 0;
            TotalAssetOfBtc = 0;

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

            if (BaseVisible || QuoteVisible) { BorrowInfoVisible = true; return; }
            BorrowInfoVisible = false;
        }

        public void BorrowBuyToggle(object o)
        {
            BorrowBuy = BorrowBuy != true && (BorrowBuy = true);
            //   WriteLog.Error("BorrowBuy: " + borrowBuy);
            PC("BorrowBuy");
        }

        public void BorrowSellToggle(object o)
        {
            BorrowSell = BorrowSell != true && (BorrowSell = true);
            //   WriteLog.Error("BorrowSell: " + borrowBuy);
            PC("BorrowSell");
        }

        #endregion [ Borrow ]
    }
}