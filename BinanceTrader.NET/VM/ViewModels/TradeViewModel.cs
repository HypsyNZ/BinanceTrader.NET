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
    public class TradeViewModel : ObservableObject
    {
        private decimal symbolPriceBuy;
        private decimal symbolPriceSell;
        private decimal orderQuantity;
        private bool useBaseBuy;
        private bool useBaseSell;
        private bool useLimitBuy;
        private bool useLimitSell;
        private bool enableQuotePriceBuy = true;
        private bool enableQuotePriceSell = true;
        private bool quotehasfocus;
        private bool basehasfocus;
        private bool useBaseToggleCheckbox;
        private bool useLimitToggleCheckbox;

        public ICommand? TradePanelSellQuoteGotFocusCommand { get; set; }
        public ICommand? TradePanelSellQuoteLostFocusCommand { get; set; }
        public ICommand? TradePanelSellBaseGotFocusCommand { get; set; }
        public ICommand? TradePanelSellBaseLostFocusCommand { get; set; }
        public ICommand? TradePanelBuyQuoteGotFocusCommand { get; set; }
        public ICommand? TradePanelBuyQuoteLostFocusCommand { get; set; }
        public ICommand? TradePanelBuyBaseGotFocusCommand { get; set; }
        public ICommand? TradePanelBuyBaseLostFocusCommand { get; set; }

        public ICommand? UseLimitBuyCommand { get; set; }
        public ICommand? UseLimitSellCommand { get; set; }

        public ICommand? UseBaseToggleCommand { get; set; }

        public ICommand? UseLimitToggleCommand { get; set; }

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

            UseLimitToggleCommand = new DelegateCommand(UseLimitToggleBasedOnTab);
            UseBaseToggleCommand = new DelegateCommand(UseBaseToggleBasedOnTab);
        }

        public TradeViewModel()
        {
            this.quotehasfocus = QuoteHasFocus;
            this.basehasfocus = BaseHasfocus;
        }

        public bool UseLimitCheckboxToggle
        {
            get => useLimitToggleCheckbox;
            set
            {
                useLimitToggleCheckbox = value;
                PC();
            }
        }

        public bool UseBaseCheckboxToggle
        {
            get => useBaseToggleCheckbox;
            set
            {
                useBaseToggleCheckbox = value;
                PC();
            }
        }

        public void TradeVMOnTabChanged(object sender, EventArgs args)
        {
            switch (Static.CurrentlySelectedSymbolTab)
            {
                case SelectedTab.Settle:
                    return;

                case SelectedTab.Buy:
                    UseBaseCheckboxToggle = UseBaseForQuoteBoolBuy;
                    UseLimitCheckboxToggle = UseLimitBuyBool;
                    break;

                case SelectedTab.Sell:
                    UseBaseCheckboxToggle = UseBaseForQuoteBoolSell;
                    UseLimitCheckboxToggle = UseLimitSellBool;
                    break;

                default:
                    WriteLog.Error("Selected Tab Error: Unexpected Exception");
                    break;
            }
        }

        public void UseBaseToggleBasedOnTab(object o)
        {
            switch (Static.CurrentlySelectedSymbolTab)
            {
                case SelectedTab.Settle:
                    return;

                case SelectedTab.Buy:
                    UseBaseForQuoteBoolBuy = !UseBaseForQuoteBoolBuy;
                    EnableQuotePriceBuy = !UseLimitBuyBool && !UseBaseForQuoteBoolBuy;
                    PC("UseBaseForQuoteBuy");
                    break;

                case SelectedTab.Sell:
                    UseBaseForQuoteBoolSell = !UseBaseForQuoteBoolSell;
                    EnableQuotePriceSell = !UseLimitSellBool && !UseBaseForQuoteBoolSell;
                    PC("UseBaseForQuoteSell");
                    break;
            }
        }

        public void UseLimitToggleBasedOnTab(object o)
        {
            switch (Static.CurrentlySelectedSymbolTab)
            {
                case SelectedTab.Settle:
                    return;

                case SelectedTab.Buy:
                    UseLimitBuyBool = !UseLimitBuyBool;
                    EnableQuotePriceBuy = !UseLimitBuyBool && !UseBaseForQuoteBoolBuy;
                    PC("UseLimitBuy");
                    break;

                case SelectedTab.Sell:
                    UseLimitSellBool = !UseLimitSellBool;
                    EnableQuotePriceSell = !UseLimitSellBool && !UseBaseForQuoteBoolSell;
                    PC("UseLimitSell");
                    break;
            }
        }

        public bool UseBaseForQuoteBoolBuy
        {
            get => this.useBaseBuy;
            set
            {
                this.useBaseBuy = value;
                PC();
            }
        }

        public bool UseBaseForQuoteBoolSell
        {
            get => this.useBaseSell;
            set
            {
                this.useBaseSell = value;
                PC();
            }
        }

        public decimal SymbolPriceBuy
        {
            get => this.symbolPriceBuy;
            set
            {
                this.symbolPriceBuy = value;
                PC();
            }
        }

        public decimal SymbolPriceSell
        {
            get => this.symbolPriceSell;
            set
            {
                this.symbolPriceSell = value;
                PC();
            }
        }

        public decimal OrderQuantity
        {
            get => this.orderQuantity;
            set
            {
                this.orderQuantity = value;
                PC();
            }
        }

        public bool UseLimitBuyBool
        {
            get => this.useLimitBuy;
            set
            {
                this.useLimitBuy = value;
                PC();
            }
        }

        public bool UseLimitSellBool
        {
            get => this.useLimitSell;
            set
            {
                this.useLimitSell = value;
                PC();
            }
        }

        public bool EnableQuotePriceBuy
        {
            get => this.enableQuotePriceBuy;
            set
            {
                this.enableQuotePriceBuy = value;
                PC();
            }
        }

        public bool EnableQuotePriceSell
        {
            get => this.enableQuotePriceSell;
            set
            {
                this.enableQuotePriceSell = value;
                PC();
            }
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

        public bool QuoteHasFocus
        {
            get => this.quotehasfocus;

            set
            {
                this.quotehasfocus = value;
                PC();
                //   log.LogInfo("Quote Has Focus: " + value);
            }
        }

        public bool BaseHasfocus
        {
            get => this.basehasfocus;

            set
            {
                this.basehasfocus = value;
                PC();
                //   log.LogInfo("Base Has Focus: " + value);
            }
        }
    }
}
