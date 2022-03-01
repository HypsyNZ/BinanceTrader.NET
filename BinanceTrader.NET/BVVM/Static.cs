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

using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.HELPERS;
using BTNET.BVVM.MessageBox;
using BTNET.ViewModels;
using ExchangeAPI.Sockets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BTNET.BVVM
{
    // There can only be one
    public static class Static
    {
        public static IMessageBoxService MessageBox = new MessageBoxService();

        public static string SystemDrive = Path.GetPathRoot(Environment.SystemDirectory);
        public static readonly string SettingsPath = SystemDrive + @"BNET\Settings\";

        public static DecimalHelper CurrentStepSize = new();

        public static readonly string logClient = SystemDrive + @"BNET\logClient.txt";
        public static readonly string logSocket = SystemDrive + @"BNET\logSocket.txt";
        public static readonly string settingsfile = SettingsPath + @"keys.json";
        public static readonly string listofdeletedorders = SettingsPath + @"list.json";
        public static readonly string listofwatchlistsymbols = SettingsPath + @"symbols.json";

        public static TradingMode CurrentTradingMode = TradingMode.Error;
        public static SelectedTab CurrentlySelectedSymbolTab = SelectedTab.Error;

        public static StoredExchangeInfo ManageExchangeInfo = new StoredExchangeInfo();
        public static StoredOrders ManageStoredOrders { get; set; } = new();
        public static RealTimeUpdateBase RTUB { get; set; } = new();
        public static BinanceMarginAccount MarginAccount { get; set; } = new();
        public static BinanceSymbol CurrentSymbolInfo { get; set; } = new();

        public static ChartBase chartBases = new();
        public static QuoteBase Quote = new();

        public static ObservableCollection<BinanceSymbolViewModel> AllPrices { get; set; } = new();
        public static ObservableCollection<BinanceSymbolViewModel> AllPricesFiltered { get; set; } = new();

        public static UpdateSubscription CurrentSymbolTickerUpdateSubscription = null;

        public static List<long> DeletedList { get; set; } = new List<long>();

        public static string SpotListenKey { get; set; } = "";
        public static string MarginListenKey { get; set; } = "";
        public static string IsolatedListenKey { get; set; } = "";
        public static string LastIsolatedListenKeySymbol { get; set; } = "";

        public static bool IsStarted { get; set; } = false;
        public static bool IsListFocus { get; set; } = false;
        public static bool IsSearching { get; set; } = false;
        public static bool WaitingForOrderUpdate { get; set; } = false;
        public static bool BlockOrderUpdates { get; set; } = false;
        public static bool ShouldUpdateExchangeInfo { get; set; } = false;

        public static volatile ObservableCollection<OrderBase> orders;
        public static volatile BinanceSymbolViewModel selectedSymbolViewModel;
        public static volatile BinanceSymbolViewModel lastSelectedSymbolViewModel;

        #region [ Read Only ]

        public static BinanceSymbolViewModel GetCurrentlySelectedSymbol => selectedSymbolViewModel;
        public static ObservableCollection<OrderBase> GetOrders => orders;

        #endregion [ Read Only ]
    }
}