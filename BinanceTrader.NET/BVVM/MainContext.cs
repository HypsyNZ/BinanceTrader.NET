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

using BinanceAPI;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.Controls;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimerSink;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    public class MainContext : ObservableObject
    {
        private DateTime startTime = DateTime.UtcNow;

        public MainContext()
        {
            try
            {
                Sink = new();

                // UI Data Context, Don't work in MainViewModel
                MainVM = new MainViewModel(this);

                ConsoleAllocator.StartConsole();
                Console.WriteLine("BinanceTrader.NET Logging Console");
                Console.WriteLine("---------------------------------");

                Timers = new();
                MainVM.IsCurrentlyLoading = true;
                Settings.LoadSettings();
#if !DEBUG_SLOW
                Task.Run(() =>
                {
                    Sink.TimingSinkItems.Add(new(async () => await UpdateServerTimeTask(), 125));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateQuoteTask(), 100));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateBidAskTask(), 50));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateAlertsTask(), 100));

                    Sink.TimingSinkItems.Add(new(async () => await UpdatePnlTask(), 100));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateBorrowViewModelTask(), 2000));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateAccountInformationTask(), 2000));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateOrdersCurrentSymbolTask(), 100));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateKeepAliveKeysTask(), 900000));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateEnforcePriorityTask(), 60000));

                    Sink.TimingSinkItems.Add(new(async () => await UpdateSyncLocalTimeTask(), 3600000));

                    Sink.TimingSinkItems.Add(new(() => { Timers.IsTimerStillRunningSink = DateTime.Now; }, 250));

                    Sink.Start();

                    Timers.Multimedia_Timer_Watchdog();
                }).ConfigureAwait(false);
#endif
                Stored.ExchangeInfo = ManageExchangeInfo.LoadExchangeInfoFromFile();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Exchange.ExchangeInfoAllPrices().ConfigureAwait(false);
                        SearchEnabled = await Search.SearchPricesUpdate().ConfigureAwait(false);
                        SymbolSearch = SymbolSearch;

                        MainVM.IsWatchlistStillLoading = true;

                        _ = Task.Run(async () => { await WatchListVM.InitializeWatchList().ConfigureAwait(false); MainVM.IsWatchlistStillLoading = false; }).ConfigureAwait(false);

                        _ = Task.Run(() => Socket.SubscribeToAllSymbolTickerUpdates().ConfigureAwait(false)).ConfigureAwait(false);

                        MainVM.Chart = "https://www.binance.com";
                        MainVM.IsCurrentlyLoading = false;
                        SearchEnabled = true;

                        TimingSinkItem UpdateExchangeInfoTaskItem = new(async () => await UpdateExchangeInfoTask().ConfigureAwait(false), 1500);
                        Sink.TimingSinkItems.Add(UpdateExchangeInfoTaskItem);

                        IsStarted = true;
                        WriteLog.Info("BTNET Started Successfully");
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error(ex);
                    }
                }).ConfigureAwait(false);

                InitializeAllCommands();
                Deleted.InitializeDeletedList();
                ManageStoredOrders.LoadStoredOrdersFromFile();
                NotepadVM.LoadNotes();
            }
            catch (Exception ex)
            {
                WriteLog.Error("BTNET Failed to Start, Exception: ", ex);
            }
        }

        #region [ Initialize Commands ]

        public ICommand DeleteRowLocalCommand { get; set; }

        /// <summary>
        /// Initialize All Commands across all ViewModels
        /// </summary>
        private void InitializeAllCommands()
        {
            MainVM.InitializeCommands();
            BorrowVM.InitializeCommands();
            AlertVM.InitializeCommands();
            TradeVM.InitializeCommands();
            WatchListVM.InitializeCommands();
            SettleVM.InitializeCommands();
            NotepadVM.InitializeCommands();
            DeleteRowLocalCommand = new DelegateCommand(DeleteRowLocal);
            WriteLog.Info("Initialized Commands");
        }

        #endregion [ Initialize Commands ]

        #region [ Long Running Tasks ]

        private async Task UpdateEnforcePriorityTask()
        {
            try
            {
                // If you don't periodically set this then the OS will assume you don't care and gradually lower the priority of your threads
                // You can observe this behavior with ProcessExplorer
                await General.ProcessPriority().ConfigureAwait(false);

                // Make sure the UserStream is still active
                await UserStreams.CheckUserStreamSubscription().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Process Priority: ", ex);
            }
        }

        private Task UpdateBidAskTask()
        {
            if (IsSymbolSelected)
            {
                try
                {
                    RealTimeVM.AskPrice = RTUB.BestAskPrice;
                    RealTimeVM.AskQuantity = RTUB.BestAskQuantity;
                    RealTimeVM.BidPrice = RTUB.BestBidPrice;
                    RealTimeVM.BidQuantity = RTUB.BestBidQuantity;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Failure while updating Bid/Ask: ", ex);
                }
            }

            return Task.CompletedTask;
        }

        private Task UpdateQuoteTask()
        {
            if (IsSymbolSelected)
            {
                try
                {
                    Quote.GetQuoteOrderQuantityLocal();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Failure while updating Quote: ", ex);
                }
            }

            return Task.CompletedTask;
        }

        private Task UpdateAlertsTask()
        {
            if (IsSymbolSelected)
            {
                try
                {
                    foreach (var a in AlertVM.Alerts)
                    {
                        a.CheckAlert();
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Failure while updating Alerts", ex);
                }
            }

            return Task.CompletedTask;
        }

        private async Task UpdatePnlTask()
        {
            try
            {
                await Orders.UpdatePnl();
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating PnL: ", ex);
            }
        }

        private Task UpdateServerTimeTask()
        {
            try
            {
                ServerTime.ServerTimeTicks = ServerTimeClient.ServerTime.Ticks;
                ServerTime.Time = ServerTimeClient.ServerTime;
                ServerTime.UsedWeight = ServerTimeClient.UsedWeight;                
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating server time: ", ex);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateSyncLocalTimeTask()
        {
            try
            {
                // The server is constantly syncing its time so we should do the same
                // Syncing time hourly reduces drift to be almost nothing
                // This increases the overall accuracy and reduces the requirement for the offset on the timestamps sent in authenticated requests.
                await General.SyncLocalTime().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while syncing local time: ", ex);
            }
        }

        private async Task UpdateKeepAliveKeysTask()
        {
            if (!IsSymbolSelected) { return; }

            try
            {
                if (MarginListenKey != string.Empty)
                {
                    var o = await BTClient.Local.Margin.UserStream.KeepAliveUserStreamAsync(MarginListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        UserStreams.lastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Margin Userstream Alive: " + MarginListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + MarginListenKey);
                        }
                    }
                }

                if (SpotListenKey != string.Empty)
                {
                    var o = await BTClient.Local.Spot.UserStream.KeepAliveUserStreamAsync(SpotListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        UserStreams.lastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Spot Userstream Alive: " + SpotListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + SpotListenKey);
                        }
                    }
                }

                if (IsolatedListenKey != string.Empty)
                {
                    var o = await BTClient.Local.Margin.UserStream.KeepAliveUserStreamAsync(IsolatedListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        UserStreams.lastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Userstream Alive: " + IsolatedListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + IsolatedListenKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while refreshing listen keys: ", ex);
            }
        }

        private async Task UpdateBorrowViewModelTask()
        {
            if (!IsSymbolSelected) { return; }

            try
            {
                if (MainVM.IsMargin && Assets.MarginAssets != null)
                {
                    var pair = await BTClient.Local.Margin.Market.GetMarginPairAsync(CurrentlySelectedSymbol.SymbolView.Symbol).ConfigureAwait(false);
                    if (pair.Success)
                    {
                        foreach (var s in Assets.MarginAssets)
                        {
                            if (s.Asset == pair.Data.BaseAsset)
                            {
                                BorrowVM.BorrowLabelBase = s.Asset;
                                BorrowVM.BorrowedBase = s.Borrowed;
                                BorrowVM.InterestBase = s.Interest;
                                BorrowVM.LockedBase = s.Locked;
                                BorrowVM.FreeBase = s.Available;
                            }
                            if (s.Asset == pair.Data.QuoteAsset)
                            {
                                BorrowVM.BorrowLabelQuote = s.Asset;
                                BorrowVM.BorrowedQuote = s.Borrowed;
                                BorrowVM.InterestQuote = s.Interest;
                                BorrowVM.LockedQuote = s.Locked;
                                BorrowVM.FreeQuote = s.Available;
                            }
                        }

                        BorrowVM.MarginLevel = MarginAccount.MarginLevel;
                        BorrowVM.TotalAssetOfBtc = MarginAccount.TotalAssetOfBtc;
                        BorrowVM.TotalLiabilityOfBtc = MarginAccount.TotalLiabilityOfBtc;
                        BorrowVM.TotalNetAssetOfBtc = MarginAccount.TotalNetAssetOfBtc;

                        BorrowVM.ShowBreakdown = true;
                    }
                }
                else
                if (MainVM.IsIsolated && Assets.IsolatedAssets != null)
                {
                    var iso = Assets.IsolatedAssets.SingleOrDefault(p => p.Symbol == CurrentlySelectedSymbol.SymbolView.Symbol);

                    BorrowVM.MarginLevel = iso.MarginLevel;
                    BorrowVM.LiquidationPrice = iso.LiquidatePrice;

                    BorrowVM.BorrowLabelBase = iso.BaseAsset.Asset;
                    BorrowVM.BorrowedBase = iso.BaseAsset.Borrowed;
                    BorrowVM.InterestBase = iso.BaseAsset.Interest;
                    BorrowVM.LockedBase = iso.BaseAsset.Locked;
                    BorrowVM.FreeBase = iso.BaseAsset.Available;

                    BorrowVM.BorrowLabelQuote = iso.QuoteAsset.Asset;
                    BorrowVM.BorrowedQuote = iso.QuoteAsset.Borrowed;
                    BorrowVM.InterestQuote = iso.QuoteAsset.Interest;
                    BorrowVM.LockedQuote = iso.QuoteAsset.Locked;
                    BorrowVM.FreeQuote = iso.QuoteAsset.Available;

                    BorrowVM.ShowBreakdown = true;
                    BorrowVM.BorrowVisibility();
                    return;
                }
                else
                {
                    BorrowVM.ShowBreakdown = false;
                    var spotBase = Assets.SpotAssets.SingleOrDefault(p => p.Asset == chartBases.SymbolLeft);
                    if (spotBase != null)
                    {
                        BorrowVM.BorrowLabelBase = spotBase.Asset;
                        BorrowVM.LockedBase = spotBase.Locked;
                        BorrowVM.FreeBase = spotBase.Available;
                        BorrowVM.TotalBase = spotBase.Total;
                    }

                    var spotQuote = Assets.SpotAssets.SingleOrDefault(p => p.Asset == chartBases.SymbolRight);
                    if (spotQuote != null)
                    {
                        BorrowVM.BorrowLabelQuote = spotQuote.Asset;
                        BorrowVM.LockedQuote = spotQuote.Locked;
                        BorrowVM.FreeQuote = spotQuote.Available;
                        BorrowVM.TotalQuote = spotQuote.Total;
                    }
                }

                BorrowVM.BorrowVisibility();
                return;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Borrow View Model: ", ex);
                return;
            }
        }

        private async Task UpdateExchangeInfoTask()
        {
            if (!Static.IsStarted || DateTime.UtcNow < startTime + TimeSpan.FromMinutes(1)) { return; }

            try
            {
                if (Static.ShouldUpdateExchangeInfo && (ServerTime.Time.Minute == 30 || ServerTime.Time.Minute == 0))
                {
                    ShouldUpdateExchangeInfo = false;
                    await Exchange.ExchangeInfoAllPrices().ConfigureAwait(false);
                    SymbolSearch = SymbolSearch;
                }

                if (!ShouldUpdateExchangeInfo && (ServerTime.Time.Minute != 30 && ServerTime.Time.Minute != 0))
                {
                    ShouldUpdateExchangeInfo = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Exchange Info: ", ex);
            }
        }

        private async Task UpdateAccountInformationTask()
        {
            if (!IsSymbolSelected) { return; }

            try
            {
                switch (CurrentTradingMode)
                {
                    case TradingMode.Spot:
                        await Account.UpdateSpotInformation().ConfigureAwait(false);
                        MainVM.IsMargin = false;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Margin:
                        await Account.UpdateMarginInformation().ConfigureAwait(false);
                        MainVM.IsMargin = true;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Isolated:
                        await Account.UpdateIsolatedInformation().ConfigureAwait(false);
                        MainVM.IsIsolated = true;
                        MainVM.IsMargin = false;
                        break;
                }

                SettleVM.CheckRepay();
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Account Information: " + ex.Message);
            }
        }

        /// <summary>
        /// Load Stored Orders for the current symbol or get current Orders
        /// Retrieve the last 200 Orders from the server
        /// Add Missing Orders if there are any
        /// Prepare Orders that weren't already Stored for Storage
        /// Refresh Orders if they changed
        /// </summary>
        private async Task UpdateOrdersCurrentSymbolTask()
        {
            try
            {
                if (MainOrders.IsUpdatingOrders) { return; }
                await Orders.UpdateOrdersCurrentSymbol().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Orders: ", ex);
            }
        }

        #endregion [ Long Running Tasks ]

        private async void ChangeSymbol()
        {
            await Task.Run(async () =>
            {
                var changed = await OnChangeSymbol().ConfigureAwait(false);
                if (changed)
                {
                    _ = UpdateOrdersCurrentSymbolTask().ConfigureAwait(false);
                }
                await PaddingWidth().ConfigureAwait(false);
            }).ConfigureAwait(false);

            MainVM.IsCurrentlyLoading = false;
            MainVM.ShouldSuspendSymbolSelection = true;
        }

        private async Task<bool> OnChangeSymbol()
        {
            MainVM.IsIsolated = false;
            MainVM.IsMargin = false;
            VisibilityVM.HideSettleTab = CurrentTradingMode == TradingMode.Spot ? false : true;
            MainVM.SelectedTabUI = CurrentTradingMode == TradingMode.Spot ? (int)SelectedTab.Buy : (int)SelectedTab.Settle;
            MainOrders.BlockOrderUpdates = true;
            MainOrders.LastRun = new();

            BorrowVM.Clear();
            SettleVM.Clear();

            CurrentSymbolInfo = Stored.ExchangeInfo.Symbols.SingleOrDefault(r => r.Name == Static.GetCurrentlySelectedSymbol.SymbolView.Symbol);
            IncrementLotSizeMin = DecimalLayout.TrimDecimal(CurrentSymbolInfo.LotSizeFilter.StepSize);
            MinTickSize = DecimalLayout.TrimDecimal(CurrentSymbolInfo.PriceFilter.TickSize);
            CurrentStepSize = IncrementLotSizeMin;

            Socket.CurrentSymbolTicker();

            QuoteVM.TradeAmountBuy = 0;
            QuoteVM.TradeAmountSell = 0;
            QuoteVM.TradePrice = CurrentlySelectedSymbol.SymbolView.LastPrice;
            BorrowVM.SymbolName = CurrentSymbolInfo.Name;
            BorrowVM.BorrowLabelBase = CurrentSymbolInfo.BaseAsset;
            BorrowVM.BorrowLabelQuote = CurrentSymbolInfo.QuoteAsset;

            var open = await UserStreams.GetUserStreamSubscription().ConfigureAwait(false);
            if (!open)
            {
                IsSymbolSelected = false;
                SearchEnabled = await UserStreams.ResetUserStreamsOnError().ConfigureAwait(false);
                SymbolSearch = SymbolSearch;
                _ = Task.Run(() => { Static.MessageBox.ShowMessage("Please Try Selecting a Symbol Again, If this is an Isolated Pair, Please enable it on the website.", "Error Subscribing to Userstream", MessageBoxButton.OK, MessageBoxImage.Hand); }).ConfigureAwait(false);
                return false;
            }

            CurrentlySelectedSymbol.TradeFee = await Exchange.GetTradeFee().ConfigureAwait(false);

            ManageStoredOrders.CurrentSymbolModeStoredOrders(DeletedList);

            Orders.ResetOrders();
            IsSymbolSelected = true;
            return true;
        }

        public BinanceSymbolViewModel CurrentlySelectedSymbol
        {
            get => GetCurrentlySelectedSymbol;
            set
            {
                selectedSymbolViewModel = value; PC();

                IsSymbolSelected = false;

                if (CurrentlySelectedSymbol != null)
                {
                    WriteLog.Info("Changing to Symbol: " + CurrentlySelectedSymbol.SymbolView.Symbol);
                    MainVM.IsCurrentlyLoading = true;
                    MainVM.ShouldSuspendSymbolSelection = false;
                    ChangeSymbol();
                }
            }
        }

        /// <summary>
        /// Prevents the symbol from being unselected when using the search.
        /// This will always be the last coin you selected in the search.
        /// Sets CurrentlySelectedSymbol
        /// </summary>
        public BinanceSymbolViewModel LastSelectedSymbol
        {
            get => lastSelectedSymbolViewModel;
            set
            {
                lastSelectedSymbolViewModel = value; PC();

                if (LastSelectedSymbol != null)
                {
                    CurrentlySelectedSymbol = LastSelectedSymbol;
                }
            }
        }

        /// <summary>
        /// The UI Context for the currently selected trading mode
        /// </summary>
        ///
        public int SelectedTradingMode
        {
            get => ((int)CurrentTradingMode);
            set
            {
                CurrentTradingMode = ((TradingMode)value);
                IsSymbolSelected = false;
                PC();

                if (IsStarted)
                {
                    MainVM.IsCurrentlyLoading = true;
                    SearchEnabled = false;

                    Task.Run(async () =>
                    {
                        await Search.SearchPricesUpdate().ConfigureAwait(false);
                        SymbolSearch = SymbolSearch;
                        SearchEnabled = true;
                        MainVM.IsCurrentlyLoading = false;
                    }).ConfigureAwait(false);

                    if (CurrentlySelectedSymbol != null)
                    {
                        MainVM.IsCurrentlyLoading = true;
                        MainVM.ShouldSuspendSymbolSelection = false;
                        WriteLog.Info("Changing to Symbol: " + CurrentlySelectedSymbol.SymbolView.Symbol);
                        ChangeSymbol();
                    }
                }

                WriteLog.Info("Selected Mode: " + (CurrentTradingMode == TradingMode.Spot ? "Spot" : CurrentTradingMode == TradingMode.Margin ? "Margin" : CurrentTradingMode == TradingMode.Isolated ? "Isolated" : "Unknown"));
            }
        }

        public string SymbolSearch
        {
            get => this.symbolSearchValue;
            set
            {
                this.symbolSearchValue = value; PC();

                Invoke.InvokeUI(() =>
                {
                    if (SymbolSearch is not null and not "")
                    {
                        IsSearching = true;
                        MainVM.AllSymbolsOnUI = new ObservableCollection<BinanceSymbolViewModel>(Static.AllPrices.Where(ap => ap.SymbolView.Symbol.Contains(this.symbolSearchValue.ToUpper())));
                        return;
                    }

                    IsSearching = false;
                    MainVM.AllSymbolsOnUI = Static.AllPrices;
                });
            }
        }

        #region [ UI ]

        public void DeleteRowLocal(object o)
        {
            if (MainVM.IsListValidTarget())
            {
                Deleted.AddToDeletedList(SelectedListItem.OrderId);
                Deleted.IsDeletedTriggered = true;
            }
        }

        public static async Task ResetControlPositions()
        {
            await MainVM.ResetControlPositions();
        }

        public static async Task PaddingWidth()
        {
            await VisibilityVM.AdjustWidth(CurrentTradingMode);
        }

        #endregion [ UI ]
    }
}