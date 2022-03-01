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
using BTNET.BVVM.HELPERS;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class TradeViewModel : ObservableObject
    {
        public ICommand TradePanelSellQuoteGotFocusCommand { get; set; }
        public ICommand TradePanelSellQuoteLostFocusCommand { get; set; }
        public ICommand TradePanelSellBaseGotFocusCommand { get; set; }
        public ICommand TradePanelSellBaseLostFocusCommand { get; set; }
        public ICommand TradePanelBuyQuoteGotFocusCommand { get; set; }
        public ICommand TradePanelBuyQuoteLostFocusCommand { get; set; }
        public ICommand TradePanelBuyBaseGotFocusCommand { get; set; }
        public ICommand TradePanelBuyBaseLostFocusCommand { get; set; }

        public ICommand UseBaseCommandBuy { get; set; }
        public ICommand UseLimitBuyCommand { get; set; }
        public ICommand UseLimitSellCommand { get; set; }
        public ICommand UseBaseCommandSell { get; set; }

        public void InitializeCommands()
        {
            TradePanelSellQuoteGotFocusCommand = new DelegateCommand(TradePanelSellQuoteGotFocus);
            TradePanelSellQuoteLostFocusCommand = new DelegateCommand(TradePanelSellQuoteLostFocus);
            TradePanelSellBaseGotFocusCommand = new DelegateCommand(TradePanelSellBaseGotFocus);
            TradePanelSellBaseLostFocusCommand = new DelegateCommand(TradePanelSellBaseLostFocus);
            TradePanelBuyQuoteGotFocusCommand = new DelegateCommand(TradePanelBuyQuoteGotFocus);
            TradePanelBuyQuoteLostFocusCommand = new DelegateCommand(TradePanelBuyQuoteLostFocus);
            TradePanelBuyBaseGotFocusCommand = new DelegateCommand(TradePanelBuyBaseGotFocus);
            TradePanelBuyBaseLostFocusCommand = new DelegateCommand(TradePanelBuyBaseLostFocus);
            UseBaseCommandBuy = new DelegateCommand(UseBaseForQuoteBuy);
            UseLimitBuyCommand = new DelegateCommand(UseLimitBuy);
            UseLimitSellCommand = new DelegateCommand(UseLimitSell);

            UseBaseCommandSell = new DelegateCommand(UseBaseForQuoteSell);
        }

        public TradeViewModel()
        {
            this.quotehasfocus = QuoteHasFocus;
            this.basehasfocus = BaseHasfocus;
        }

        private decimal symbolPrice, orderQuantity;

        public decimal SymbolPrice
        { get => this.symbolPrice; set { this.symbolPrice = value; PC(); } }

        public decimal OrderQuantity
        { get => this.orderQuantity; set { this.orderQuantity = value; PC(); } }

        private bool useLimitBuy = false; public bool UseLimitBuyBool
        { get => this.useLimitBuy; set { this.useLimitBuy = value; PC(); } }

        private bool useLimitSell = false; public bool UseLimitSellBool
        { get => this.useLimitSell; set { this.useLimitSell = value; PC(); } }

        private bool enableQuotePriceBuy = true; public bool EnableQuotePriceBuy
        { get => this.enableQuotePriceBuy; set { this.enableQuotePriceBuy = value; PC(); } }

        private bool enableQuotePriceSell = true; public bool EnableQuotePriceSell
        { get => this.enableQuotePriceSell; set { this.enableQuotePriceSell = value; PC(); } }

        public void UseLimitBuy(object o)
        {
            UseLimitBuyBool = UseLimitBuyBool != true && (UseLimitBuyBool = true);
            EnableQuotePriceBuy = !UseLimitBuyBool && !UseBaseForQuoteBoolBuy;
            PC("UseLimitBuy");
        }

        public void UseLimitSell(object o)
        {
            UseLimitSellBool = UseLimitSellBool != true && (UseLimitSellBool = true);
            EnableQuotePriceSell = !UseLimitSellBool && !UseBaseForQuoteBoolSell;
            PC("UseLimitSell");
        }

        private bool useBaseBuy = false; public bool UseBaseForQuoteBoolBuy
        { get => this.useBaseBuy; set { this.useBaseBuy = value; PC(); } }

        public void UseBaseForQuoteBuy(object o)
        {
            UseBaseForQuoteBoolBuy = UseBaseForQuoteBoolBuy != true && (UseBaseForQuoteBoolBuy = true);
            EnableQuotePriceBuy = !UseLimitBuyBool && !UseBaseForQuoteBoolBuy;
            PC("UseBaseForQuoteBuy");
        }

        private bool useBaseSell = false; public bool UseBaseForQuoteBoolSell
        { get => this.useBaseSell; set { this.useBaseSell = value; PC(); } }

        public void UseBaseForQuoteSell(object o)
        {
            UseBaseForQuoteBoolSell = UseBaseForQuoteBoolSell != true && (UseBaseForQuoteBoolSell = true);
            EnableQuotePriceSell = !UseLimitSellBool && !UseBaseForQuoteBoolSell;
            PC("UseBaseForQuoteSell");
        }

        public void TradePanelSellQuoteGotFocus(object o)
        {
            QuoteHasFocus = true;
        }

        public void TradePanelSellQuoteLostFocus(object o)
        {
            QuoteHasFocus = false;
        }

        public void TradePanelBuyQuoteGotFocus(object o)
        {
            QuoteHasFocus = true;
        }

        public void TradePanelBuyQuoteLostFocus(object o)
        {
            QuoteHasFocus = false;
        }

        public void TradePanelSellBaseGotFocus(object o)
        {
            BaseHasfocus = true;
        }

        public void TradePanelSellBaseLostFocus(object o)
        {
            BaseHasfocus = false;
        }

        public void TradePanelBuyBaseGotFocus(object o)
        {
            BaseHasfocus = true;
        }

        public void TradePanelBuyBaseLostFocus(object o)
        {
            BaseHasfocus = false;
        }

        private bool quotehasfocus, basehasfocus;

        public bool QuoteHasFocus
        {
            get => this.quotehasfocus;

            set
            {
                this.quotehasfocus = value; PC();
                //   log.LogInfo("Quote Has Focus: " + value);
            }
        }

        public bool BaseHasfocus
        {
            get => this.basehasfocus;

            set
            {
                this.basehasfocus = value; PC();
                //   log.LogInfo("Base Has Focus: " + value);
            }
        }
    }
}