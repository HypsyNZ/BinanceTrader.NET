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

using BinanceAPI;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.VM.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BTNET.BVVM
{
    // There can only be one
    public static class Static
    {
        private static volatile BinanceSymbolViewModel? selectedSymbolViewModel;
        private static volatile BinanceSymbolViewModel? lastSelectedSymbolViewModel;

        public static StoredExchangeInfo ManageExchangeInfo { get; set; } = new();
        public static StoredOrders ManageStoredOrders { get; set; } = new();
        public static StoredAlerts ManageStoredAlerts { get; set; } = new();
        public static StoredQuotes ManageStoredQuotes { get; set; } = new();

        public static RealTimeUpdateBase RealTimeUpdate { get; set; } = new();
        public static BinanceMarginAccount MarginAccount { get; set; } = new();
        public static BinanceSymbol? CurrentSymbolInfo { get; set; } = new();

        public static ObservableCollection<BinanceSymbolViewModel> AllPrices { get; set; } = new();
        public static ObservableCollection<BinanceSymbolViewModel> AllPricesUnfiltered { get; set; } = new();

        public static List<long> DeletedList { get; set; } = new List<long>();
        public static List<long> SettledOrders { get; set; } = new List<long>();

        public static string SpotListenKey { get; set; } = "";
        public static string MarginListenKey { get; set; } = "";
        public static string IsolatedListenKey { get; set; } = "";
        public static string LastIsolatedListenKeySymbol { get; set; } = "";

        public static bool IsStarted { get; set; }
        public static bool IsListFocus { get; set; }
        public static bool IsSearching { get; set; }
        public static bool GetIsSymbolSelected { get; set; }

        public static BinanceSymbolViewModel? SelectedSymbolViewModel
        {
            get => selectedSymbolViewModel;
            set => selectedSymbolViewModel = value;
        }

        public static BinanceSymbolViewModel? LastSelectedSymbolViewModel
        {
            get => lastSelectedSymbolViewModel;
            set => lastSelectedSymbolViewModel = value;
        }

        public static Message MessageBox { get; set; } = new();

        public static SelectedTab CurrentlySelectedSymbolTab { get; set; } = SelectedTab.Error;
        public static TradingMode CurrentTradingMode { get; set; } = TradingMode.Error;

        public static ChartBase ChartBases { get; set; } = new();
        public static QuoteBase RealTimeQuote { get; set; } = new();

        internal static bool HasAuth()
        {
            return Settings.KeysLoaded && ObservableObject.MainVM.IsSymbolSelected && ServerTimeClient.IsReady();
        }

        public static bool IsInvalidSymbol()
        {
            if (!ObservableObject.MainVM.IsSymbolSelected || Static.SelectedSymbolViewModel == null || Static.CurrentTradingMode == TradingMode.Error)
            {
                return true;
            }

            return false;
        }
    }
}
