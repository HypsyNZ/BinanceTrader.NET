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
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using PrecisionTiming;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.LockedTask;
using System.Windows;
using System.Windows.Input;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    public class MainContext : ObservableObject
    {
        private const int ONE_HUNDRED = 100;

        private const int UPDATE_SELECTED_QUOTE_MS = 100;
        private const int UPDATE_SELECTED_BID_ASK_MS = 30;
        private const int UPDATE_SELECTED_ORDERS_PNL_MS = 100;
        private const int UPDATE_SELECTED_ORDERS_MS = 50;
        private const int UPDATE_SELECTED_ASSETS_MS = 2000;
        private const int UPDATE_SELECTED_ACCOUNT_MS = 2000;

        private const int UPDATE_ALL_ACCOUNTS_PERIODIC_MS = 60000;
        private const int UPDATE_FLEXIBLE_PERIODIC_MS = 900_000;
        private const int UPDATE_EXCHANGE_INFO_PERIODIC_MS = 2000;

        private const int UPDATE_SERVER_TIME_MS = 100;
        private const int UPDATE_ALERTS_MS = 100;

        private const int UPDATE_KEEP_ALIVE_MS = 900_000;
        private const int UPDATE_PRIORITY_MS = 60000;
        private const int UPDATE_LOCAL_TIME_MS = 3_600_000;
        private const int WATCHDOG_TIME = 250;

        private const int SERVER_TIME_UPDATE_START_HOUR = 23;
        private const int SERVER_TIME_UPDATE_END_HOUR = 00;

        private const int EX_HALF_TIME_ONE = 30;
        private const int EX_HALF_TIME_TWO = 0;
        private const int EX_HALF_TIME_DIFF = 2;

        private const int RESOLUTION = 1;
        private const int EXIT_REASON = 7;
        private const int CONNECTION_LIMIT = 50;

        public const int TEN_THOUSAND_TICKS = 10000;

        public const int BORDER_THICKNESS = 7;
        public const int RAPID_CLICKS_TO_MAXIMIZE_WINDOW = 2;

        public const int DEFAULT_ROUNDING_PLACES = 8;

        public const int FEE_MULTIPLIER = 2;
        public const int MIN_PNL_FEE_MULTIPLIER = 5;

        public const int QUOTE_DELAY = UPDATE_SELECTED_QUOTE_MS + 25;

        /// <summary>
        /// If you have a lot of symbols on your Alert List or Watch List you shouldn't change this
        /// You need to replace your settings files between launches to currently make any use of this
        /// </summary>
        private const int MAX_INSTANCES = 1;

        private static LockedTask UpdateOrdersAsyncTask = new LockedTask();
        private static LockedTask UpdateServerTimeAsyncTask = new LockedTask();
        private static LockedTask UpdateQuoteAsyncTask = new LockedTask();
        private static LockedTask UpdateAlertsAsyncTask = new LockedTask();
        private static LockedTask UpdateSelectedBidAskAsyncTask = new LockedTask();
        private static LockedTask UpdateSelectedSymbolAssetsAsyncTask = new LockedTask();
        private static LockedTask UpdateSelectedOrdersPnLAsyncTask = new LockedTask();
        private static LockedTask UpdateSelectedAccountInformationAsyncTask = new LockedTask();
        private static LockedTask UpdateAllAccountInformationPeriodicAsyncTask = new LockedTask();
        private static LockedTask UpdateKeepAliveKeysAsyncTask = new LockedTask();
        private static LockedTask UpdateEnforcePriorityAsyncTask = new LockedTask();
        private static LockedTask UpdateSyncLocalTimeAsyncTask = new LockedTask();
        private static LockedTask UpdateExchangeInfoPeriodicAsyncTask = new LockedTask();
        private static LockedTask UpdateFlexiblePositionsProductsPeriodicAsyncTask = new LockedTask();

        public MainContext()
        {
#if DESIGN
            return;
#endif

            try
            {
                WatchMan.Task_Three.SetWorking();
                General.LimitInstances("BinanceTrader.NET", MAX_INSTANCES);

                if (!General.IsAdministrator())
                {
                    Message.ShowBox("Binance Trader must run as Administrator so it can run in Real Time", "Please Restart As Administrator!", waitForReply: true, exit: true);
                }

                #region [WAIT]

                Settings.LoadSettingsAsync().ConfigureAwait(false);
                Browser.ConfigureBrowserAsync();

                App.ApplicationStarted += OnStarted;
                App.TradingModeChanged += OnChangeTradingMode;
                App.SearchChanged += OnSearchUpdated;
                App.SymbolChanged += OnChangeSymbol;
                App.QuoteUpdateEvent += OnQuoteUpdate;

                App.TabChanged += VisibilityVM.OrderSettingsOnTabChanged;
                App.TabChanged += BorrowVM.BorrowVMOnTabChanged;
                App.TabChanged += TradeVM.TradeVMOnTabChanged;

                MainVM = new MainViewModel(this);
                MainVM.IsCurrentlyLoading = true;

                InitializeAllCommands();
                ManageExchangeInfo.LoadExchangeInfoFromFileAsync();
                Stored.Quotes = ManageStoredQuotes.LoadQuotesFromFileAsync().Result;

                WatchMan.Task_Three.SetCompleted();

                CreateNewSinks(null, 0);

                #endregion [WAIT]

                #region [ASYNC]

                _ = Task.Run(() =>
                {
                    ServicePointManager.DefaultConnectionLimit = CONNECTION_LIMIT;
                    TimingSettings.SetMinimumTimerResolution(RESOLUTION);
                    _ = General.ProcessAffinityAsync().ConfigureAwait(false);
                    _ = General.ProcessPriorityAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);

                _ = ManageStoredOrders.LoadAllStoredOrdersFromFileStorageAsync().ConfigureAwait(false);

                _ = Task.Run(() =>
                {
                    WatchMan.Task_Four.SetWorking();
                    _ = Deleted.InitializeDeletedListAsync().ConfigureAwait(false);
                    UpdateAllAccountInformationPeriodicAsyncTask.RunAsync(UpdateAllAccountInformationPeriodicAsync);
                    NotepadVM.LoadNotes();
                    WatchMan.Task_Four.SetCompleted();
                }).ConfigureAwait(false);

                _ = Task.Run(async () =>
                {
                    WatchMan.Task_Two.SetWorking();
                    try
                    {
                        MainVM.IsWatchlistStillLoading = true;
                        _ = Socket.SubscribeToAllSymbolTickerUpdatesAsync().ConfigureAwait(false);
                        _ = Alert.StartAlertSymbolTickersAsync().ConfigureAwait(false);
                        _ = Static.ManageStoredAlerts.LoadStoredAlertsAsync().ConfigureAwait(false);

                        await Exchange.ExchangeInfoAllPricesAsync().ConfigureAwait(false);
                        SymbolSearch = SymbolSearch;

                        _ = Task.Run(async () =>
                        {
                            await WatchListVM.InitializeWatchListAsync().ConfigureAwait(false);
                            MainVM.IsWatchlistStillLoading = false;
                        }).ConfigureAwait(false);

                        if (Settings.KeysLoaded)
                        {
                            _ = StartUserStreamsAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            MainVM.SavingsEnabled = false;
                            MainVM.BuyButtonEnabled = false;
                            MainVM.SellButtonEnabled = false;
                            WatchMan.Load_InterestMargin.SetWaiting();
                            WatchMan.Load_InterestIsolated.SetWaiting();
                            WatchMan.Load_TradeFee.SetWaiting();
                            WatchMan.UserStreams.SetWaiting();
                        }

                        if (CurrentTradingMode == TradingMode.Error || Stored.ExchangeInfo == null)
                        {
                            DisplayErrorAndReset("Failed to Start Correctly and will now Exit", "Please Restart", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error(ex);
                        WatchMan.ExceptionWhileStarting.SetError();
                    }

                    WatchMan.Task_Two.SetCompleted();
                }).ConfigureAwait(false);

                #endregion [ASYNC]
            }
            catch (Exception ex)
            {
                WriteLog.Error("Binance Trader Failed to Start, Exception: ", ex);
                WatchMan.ExceptionWhileStarting.SetError();
            }
        }

        #region [ Initialize Commands ]

        public ICommand? DeleteRowLocalCommand { get; set; }

        /// <summary>
        /// Initialize All Commands across all ViewModels
        /// </summary>
        private void InitializeAllCommands()
        {
            MainVM.InitializeCommands();
            SettingsVM.InitializeCommands();
            BorrowVM.InitializeCommands();
            AlertVM.InitializeCommands();
            TradeVM.InitializeCommands();
            WatchListVM.InitializeCommands();
            SettleVM.InitializeCommands();
            NotepadVM.InitializeCommands();
            FlexibleVM.InitializeCommands();
            VisibilityVM.InitializeCommands();
            DeleteRowLocalCommand = new DelegateCommand(DeleteRowLocal);
            WriteLog.Info("Initialized Commands");
        }

        #endregion [ Initialize Commands ]

        #region [ Timing Sinks ]

        /// <summary>
        /// Timing Sink One
        /// </summary>
        private void CreateSinkOne()
        {
            Sink = new();

            Sink.TimingSinkItems.Add(new(() => UpdateQuoteAsyncTask.RunAsync(UpdateQuoteAsync), UPDATE_SELECTED_QUOTE_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateAlertsAsyncTask.RunAsync(UpdateAlertsAsync), UPDATE_ALERTS_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateSelectedBidAskAsyncTask.RunAsync(UpdateSelectedBidAskAsync), UPDATE_SELECTED_BID_ASK_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateOrdersAsyncTask.RunAsync(UpdateSelectedOrdersAsync), UPDATE_SELECTED_ORDERS_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateSelectedOrdersPnLAsyncTask.RunAsync(UpdateSelectedOrdersPnLAsync), UPDATE_SELECTED_ORDERS_PNL_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateFlexiblePositionsProductsPeriodicAsyncTask.RunAsync(UpdateFlexiblePositionsProductsPeriodicAsync), UPDATE_FLEXIBLE_PERIODIC_MS));

            Sink.TimingSinkItems.Add(new(() => UpdateExchangeInfoPeriodicAsyncTask.RunAsync(UpdateExchangeInfoPeriodicAsync), UPDATE_EXCHANGE_INFO_PERIODIC_MS));
        }

        /// <summary>
        /// Timing Sink Two
        /// </summary>
        private void CreateSinkTwo()
        {
            SinkTwo = new();

            SinkTwo.TimingSinkItems.Add(new(() => UpdateSelectedSymbolAssetsAsyncTask.RunAsync(UpdateSelectedSymbolAssetsAsync), UPDATE_SELECTED_ASSETS_MS));

            SinkTwo.TimingSinkItems.Add(new(() => UpdateSelectedAccountInformationAsyncTask.RunAsync(UpdateSelectedAccountInformationAsync), UPDATE_SELECTED_ACCOUNT_MS));

            SinkTwo.TimingSinkItems.Add(new(() => UpdateAllAccountInformationPeriodicAsyncTask.RunAsync(UpdateAllAccountInformationPeriodicAsync), UPDATE_ALL_ACCOUNTS_PERIODIC_MS));

            SinkTwo.TimingSinkItems.Add(new(() => UpdateKeepAliveKeysAsyncTask.RunAsync(UpdateKeepAliveKeysAsync), UPDATE_KEEP_ALIVE_MS));

            SinkTwo.TimingSinkItems.Add(new(() => UpdateEnforcePriorityAsyncTask.RunAsync(UpdateEnforcePriorityAsync), UPDATE_PRIORITY_MS));

            SinkTwo.TimingSinkItems.Add(new(() => UpdateServerTimeAsyncTask.RunAsync(UpdateServerTimeAsync), UPDATE_SERVER_TIME_MS));
        }

        /// <summary>
        /// Create New Timing Sinks during startup or in an attempt to recover from a serious error
        /// </summary>
        /// <param name="sender">sender is null</param>
        /// <param name="args">which sink to create - zero to recreate all sinks</param>
        private void CreateNewSinks(object? sender, int args)
        {
            WatchMan.Task_One.SetWorking();

            _ = Task.Run(async () =>
            {
                await Task.Delay(1).ConfigureAwait(false);

                if (Timers != null)
                {
                    Timers.DisposeWatchdog();
                    Timers.SinkMissing -= CreateNewSinks;
                }

                try
                {
                    switch (args)
                    {
                        case 0:
                            CreateSinkOne();
                            CreateSinkTwo();
                            break;
                        case 1:
                            CreateSinkOne();
                            break;
                        case 2:
                            CreateSinkTwo();
                            break;
                    }

                    if (Sink == null || SinkTwo == null)
                    {
                        WriteLog.Error("Creating new Sinks appears to have failed.. Goodbye.");
                        Timing.Panic();
                        return;
                    }

                    if (Sink.SinkFaulted)
                    {
                        Sink.Start();
                    }

                    if (SinkTwo.SinkFaulted)
                    {
                        SinkTwo.Start();
                    }

                    Timers = new();
                    Timers.SinkMissing += CreateNewSinks;
                    Timers.Multimedia_Timer_Watchdog();
                }
                catch (Exception ex)
                {
                    WriteLog.Error(ex);
                    WatchMan.ExceptionWhileStarting.SetError();
                }

                WatchMan.Task_One.SetCompleted();
            }).ConfigureAwait(false);
        }

        #endregion [ Timing Sinks ]

        #region [ Long Running Tasks ]

        private Task UpdateAlertsAsync()
        {
            if (AlertVM.Alerts.Count == 0 || !ManageStoredAlerts.LoadedAlerts)
            {
                return Task.CompletedTask;
            }

            try
            {
                foreach (var alert in AlertVM.Alerts)
                {
                    Alert.CheckAlertItem(alert);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Alerts", ex);
            }

            return Task.CompletedTask;
        }

        private Task UpdateQuoteAsync()
        {
            if (!MainVM.IsSymbolSelected)
            {
                return Task.CompletedTask;
            }

            try
            {
                RealTimeQuote.GetQuoteOrderQuantityLocal();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Quote: ", ex);
            }

            return Task.CompletedTask;
        }

        private Task UpdateSelectedBidAskAsync()
        {
            if (!MainVM.IsSymbolSelected)
            {
                return Task.CompletedTask;
            }

            try
            {
                RealTimeVM.AskPrice = RealTimeUpdate.BestAskPrice;
                RealTimeVM.AskQuantity = RealTimeUpdate.BestAskQuantity;
                RealTimeVM.BidPrice = RealTimeUpdate.BestBidPrice;
                RealTimeVM.BidQuantity = RealTimeUpdate.BestBidQuantity;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Bid/Ask: ", ex);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateSelectedSymbolAssetsAsync()
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                if (MainVM.IsMargin && Assets.MarginAssets != null)
                {
                    await Assets.SelectedMarginAssetUpdateAsync().ConfigureAwait(false);
                }
                else if (MainVM.IsIsolated && Assets.IsolatedAssets != null)
                {
                    await Assets.SelectedIsolatedAssetUpdateAsync().ConfigureAwait(false);
                }
                else
                {
                    await Assets.SelectedSpotAssetUpdateAsync().ConfigureAwait(false);
                }

                BorrowVM.BorrowVisibility();
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Borrow View Model: ", ex);
            }
        }

        private async Task UpdateSelectedOrdersAsync()
        {
            if (!HasAuth() || Static.IsInvalidSymbol())
            {
                StoredOrders.lastTotal = 0;
                return;
            }

            try
            {
                await Orders.UpdateOrdersCurrentSymbolAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Orders: ", ex);
            }
        }

        private async Task UpdateSelectedOrdersPnLAsync()
        {
            if (Static.IsInvalidSymbol() || Orders.Current.Count == 0)
            {
                return;
            }

            try
            {
                await Orders.UpdatePnlAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating PnL: ", ex);
            }
        }

        private async Task UpdateSelectedAccountInformationAsync()
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                switch (CurrentTradingMode)
                {
                    case TradingMode.Spot:
                        await Account.UpdateSpotInformationAsync().ConfigureAwait(false);
                        MainVM.IsMargin = false;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Margin:
                        await Account.UpdateMarginInformationAsync().ConfigureAwait(false);
                        MainVM.IsMargin = true;
                        MainVM.IsIsolated = false;
                        break;

                    case TradingMode.Isolated:
                        await Account.UpdateIsolatedInformationAsync().ConfigureAwait(false);
                        MainVM.IsIsolated = true;
                        MainVM.IsMargin = false;
                        break;

                    default:
                        Message.ShowBox("Trading Mode was Expected", "Select Symbol Again");
                        ResetSymbol();
                        break;
                }

                SettleVM.CheckRepay();
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Account Information: " + ex.Message);
            }
        }

        private async Task UpdateAllAccountInformationPeriodicAsync()
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                await Account.UpdateSpotInformationAsync().ConfigureAwait(false);

                ObservableCollection<BinanceBalance>? assets = null;

                lock (Assets.SpotAssetLock)
                {
                    assets = Assets.SpotAssets;
                }

                if (assets != null)
                {
                    Assets.SpotAssetsLending = new();
                    foreach (var asset in assets)
                    {
                        if (asset.Asset.StartsWith("LD"))
                        {
                            Assets.SpotAssetsLending.Add(asset);
                        }
                    }
                }

                await Account.UpdateMarginInformationAsync().ConfigureAwait(false);

                await Account.UpdateIsolatedInformationAsync().ConfigureAwait(false);

#if DEBUG

                var forDebug = Assets.SpotAssets;
                var forDebug2 = Assets.MarginAssets;
                var forDebug3 = Assets.IsolatedAssets;
                var forDebug4 = Assets.SpotAssetsLending;
#endif
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Account Information: " + ex.Message);
            }
        }

        private async Task UpdateKeepAliveKeysAsync()
        {
            if (!HasAuth())
            {
                return;
            }

            try
            {
                await UserStreams.KeepAliveKeysAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while refreshing listen keys: ", ex);
            }
        }

        private async Task UpdateFlexiblePositionsProductsPeriodicAsync()
        {
            if (!HasAuth())
            {
                return;
            }

            if (ServerTimeVM.Time.Hour == SERVER_TIME_UPDATE_START_HOUR || ServerTimeVM.Time.Hour == SERVER_TIME_UPDATE_END_HOUR)
            {
                try
                {
                    await FlexibleVM.GetAllPositionsAsync(false).ConfigureAwait(false);

                    await FlexibleVM.GetAllProductsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Failure while updating Flexible Positions and Products: ", ex);
                }
            }
        }

        private async Task UpdateExchangeInfoPeriodicAsync()
        {
            if (!IsStarted)
            {
                return;
            }

            var now = DateTime.Now;
            if ((now.Minute == EX_HALF_TIME_ONE || now.Minute == EX_HALF_TIME_TWO) && now.Second > 1)
            {
                if (Exchange.ExchangeInfoUpdateTime + TimeSpan.FromMinutes(EX_HALF_TIME_DIFF) < now)
                {
                    try
                    {
                        // Update All Exchange Information and Search Prices
                        await Exchange.ExchangeInfoAllPricesAsync().ConfigureAwait(false);
                        InvokeUI.CheckAccess(() =>
                        {
                            SymbolSearch = SymbolSearch; // Property Changed
                            MainVM.SearchEnabled = true;
                            MainVM.SymbolSelectionHitTest = true;
                        });

                        // Update All Interest Rates
                        await InterestRate.GetAllInterestRatesAsync().ConfigureAwait(false);

                        // Update Interest Rates for Current Symbol
                        await Orders.UpdateInterestRateAsync().ConfigureAwait(false);

                        // Update All Trade Fees
                        await TradeFee.GetAllTradeFeesAsync().ConfigureAwait(false);

                        // Update Trade Fees for Current Symbol
                        await Orders.UpdateTradeFeeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error("Failure while updating Exchange Info: ", ex);
                    }
                }
            }
        }

        private Task UpdateServerTimeAsync()
        {
            try
            {
                ServerTimeVM.Time = new DateTime(ServerTimeClient.ServerTimeTicks);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating server time: ", ex);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateEnforcePriorityAsync()
        {
            try
            {
                // If you don't periodically set this then the OS will assume you don't care and gradually lower the priority of your threads
                // You can observe this behavior with ProcessExplorer
                await General.ProcessPriorityAsync().ConfigureAwait(false);

                // Make sure the UserStream is still active
                await UserStreams.CheckUserStreamSubscriptionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while updating Process Priority: ", ex);
            }
        }

        #endregion [ Long Running Tasks ]

        public static async Task StartUserStreamsAsync()
        {
            _ = TradeFee.GetAllTradeFeesAsync().ConfigureAwait(false);
            _ = InterestRate.GetAllInterestRatesAsync().ConfigureAwait(false);

            var open = await UserStreams.GetUserStreamSubscriptionAsync().ConfigureAwait(false);
            if (!open)
            {
                Message.ShowBox("Couldn't Open Default User Streams and will now Exit", "Error Subscribing to Userstreams", waitForReply: true, exit: true);
            }

            MainVM.SavingsEnabled = true;
            MainVM.BuyButtonEnabled = true;
            MainVM.SellButtonEnabled = true;
        }

        public void OnStarted(object sender, EventArgs e)
        {
            IsStarted = true;
            MainVM.IsCurrentlyLoading = false;
            SettingsVM.CheckForUpdateCheckBoxEnabled = true;
            MainVM.SearchEnabled = true;
            MainVM.SymbolSelectionHitTest = true;
            WriteLog.Info("Binance Trader Started Successfully after: [" + ((DateTime.UtcNow.Ticks - App.ClientLaunchTime.Ticks) / App.TEN_THOUSAND_TICKS) + "ms]");
        }

        public async void OnQuoteUpdate(object sender, EventArgs args)
        {
            await Task.Delay(QUOTE_DELAY).ConfigureAwait(false);

            var loadquote = Stored.Quotes.Where(t => t.Symbol == CurrentlySelectedSymbol!.SymbolView.Symbol).FirstOrDefault();
            if (loadquote != null)
            {
                InvokeUI.CheckAccess(() =>
                {
                    QuoteVM.ObserveQuoteOrderQuantityLocalBuy = loadquote.QuantityBuy;
                    QuoteVM.ObserveQuoteOrderQuantityLocalSell = loadquote.QuantitySell;
                });

                InvokeUI.CheckAccess(() =>
                {
                    if (TradeVM.UseLimitBuyBool)
                    {
                        TradeVM.SymbolPriceBuy = loadquote.LimitBuyPrice;
                    }

                    if (TradeVM.UseLimitSellBool)
                    {
                        TradeVM.SymbolPriceSell = loadquote.LimitSellPrice;
                    }
                });
            }
        }

        private void OnChangeSymbol(object sender, bool updateQuote)
        {
            if (CurrentlySelectedSymbol != null)
            {
                MainVM.IsCurrentlyLoading = true;
                MainVM.SymbolSelectionHitTest = false;

                WriteLog.Info("Changing to Symbol: " + CurrentlySelectedSymbol.SymbolView.Symbol);

                _ = Task.Run(async () =>
                {
                    var changed = await OnChangeSymbolAsync().ConfigureAwait(false);
                    if (changed)
                    {
                        await PaddingWidthAsync().ConfigureAwait(false);

                        InvokeUI.CheckAccess(() =>
                        {
                            MainVM.IsSymbolSelected = true;
                            MainVM.IsCurrentlyLoading = false;
                            MainVM.SymbolSelectionHitTest = true;
                        });

                        _ = Orders.UpdateInterestRateAsync().ConfigureAwait(false);
                        _ = Orders.UpdateTradeFeeAsync().ConfigureAwait(false);

                        if (updateQuote)
                        {
                            App.QuoteUpdateEvent?.Invoke(null, null);
                        }
                    }
                }).ConfigureAwait(false);
            }
        }

        private async Task<bool> OnChangeSymbolAsync()
        {
            if (Stored.ExchangeInfo == null || CurrentlySelectedSymbol == null)
            {
                DisplayErrorAndReset("Failed to Select Symbol", "Try Again");
                return false;
            }

            ResetSymbol();

            MainVM.SelectedTabUI = CurrentTradingMode == TradingMode.Spot ? (int)SelectedTab.Buy : (int)SelectedTab.Settle;
            VisibilityVM.HideSettleTab = CurrentTradingMode != TradingMode.Spot;

            CurrentSymbolInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(CurrentlySelectedSymbol.SymbolView.Symbol);

            if (CurrentSymbolInfo == null)
            {
                DisplayErrorAndReset("Couldn't find information for the Selected Symbol", "Try Again");
                return false;
            }

            bool r = await ChangeMode.ChangeSelectedSymbolModeAsync(CurrentlySelectedSymbol.SymbolView.Symbol);
            if (!r)
            {
                DisplayErrorAndReset("Couldn't Open Default User Streams", "Error Subscribing to Userstream");
                return false;
            }

            await Socket.CurrentSymbolTickerAsync().ConfigureAwait(false);

            QuantityMin = CurrentSymbolInfo.LotSizeFilter?.MinQuantity ?? 0;
            QuantityMax = CurrentSymbolInfo.LotSizeFilter?.MaxQuantity ?? 0;
            QuantityTickSize = CurrentSymbolInfo.LotSizeFilter?.StepSize ?? 0;

            PriceMin = CurrentSymbolInfo.PriceFilter?.MinPrice ?? 0;
            PriceMax = CurrentSymbolInfo.PriceFilter?.MaxPrice ?? 0;
            PriceTickSize = CurrentSymbolInfo.PriceFilter?.TickSize ?? 0;

            BorrowVM.SymbolName = CurrentSymbolInfo!.Name;
            BorrowVM.BorrowLabelBase = CurrentSymbolInfo.BaseAsset;
            BorrowVM.BorrowLabelQuote = CurrentSymbolInfo.QuoteAsset;

            if (Static.CurrentTradingMode == TradingMode.Margin)
            {
                CurrentlySelectedSymbol.DailyInterestRateString = (InterestRate.GetDailyInterestRate(CurrentlySelectedSymbol.SymbolView.Symbol)).Normalize() + "%";
                CurrentlySelectedSymbol.YearlyInterestRateString = (InterestRate.GetYearlyInterestRate(CurrentlySelectedSymbol.SymbolView.Symbol) ?? 0).Normalize() + "%";
            }
            else if (Static.CurrentTradingMode == TradingMode.Isolated)
            {
                CurrentlySelectedSymbol.DailyInterestRateString = (InterestRate.GetDailyInterestRateIsolated(CurrentlySelectedSymbol.SymbolView.Symbol)).Normalize() + "%";
                CurrentlySelectedSymbol.YearlyInterestRateString = (InterestRate.GetYearlyInterestRateIsolated(CurrentlySelectedSymbol.SymbolView.Symbol)).Normalize() + "%";
            }

            return true;
        }

        public void OnChangeTradingMode(object sender, TradingMode mode)
        {
            if (IsStarted)
            {
                MainVM.IsCurrentlyLoading = true;
                MainVM.SearchEnabled = false;
                MainVM.SymbolSelectionHitTest = false;

                if (MainVM.IsSymbolSelected)
                {
                    App.SymbolChanged?.Invoke(null, false);
                }

                Task.Run(async () =>
                {
                    await Search.SearchPricesUpdateAsync().ConfigureAwait(false);
                    if (!String.IsNullOrEmpty(SymbolSearch))
                    {
                        SymbolSearch = SymbolSearch;
                    }

                    InvokeUI.CheckAccess(() =>
                    {
                        MainVM.SearchEnabled = true;
                        MainVM.SymbolSelectionHitTest = true;
                        MainVM.IsCurrentlyLoading = false;
                    });
                }).ConfigureAwait(false);

                WriteLog.Info("Selected Mode: " + (CurrentTradingMode == TradingMode.Spot ? "Spot"
                    : CurrentTradingMode == TradingMode.Margin ? "Margin"
                    : CurrentTradingMode == TradingMode.Isolated ? "Isolated"
                    : "Unknown"));
            }
        }

        public void OnSearchUpdated(object sender, EventArgs args)
        {
            InvokeUI.CheckAccess(() =>
            {
                if (!string.IsNullOrWhiteSpace(SymbolSearch))
                {
                    IsSearching = true;
                    MainVM.AllSymbolsOnUI = new ObservableCollection<BinanceSymbolViewModel>(AllPrices.Where(ap => ap.SymbolView.Symbol.Contains(SymbolSearchValue.ToUpper())));
                    return;
                }

                IsSearching = false;
                MainVM.AllSymbolsOnUI = AllPrices;
            });
        }

        /// <summary>
        /// Prevents the symbol from being unselected when using the search.
        /// This will always be the last coin you selected in the search.
        /// Sets CurrentlySelectedSymbol
        /// </summary>
        public BinanceSymbolViewModel? LastSelectedSymbol
        {
            get => LastSelectedSymbolViewModel;
            set
            {
                LastSelectedSymbolViewModel = value;
                PC();

                if (LastSelectedSymbol != null)
                {
                    CurrentlySelectedSymbol = LastSelectedSymbol;
                }
            }
        }

        public BinanceSymbolViewModel? CurrentlySelectedSymbol
        {
            get => SelectedSymbolViewModel;
            set
            {
                Quotes.AddStoredQuote();

                SelectedSymbolViewModel = value;
                PC();

                if (value == null)
                {
                    MainVM.IsSymbolSelected = false;
                }

                App.SymbolChanged?.Invoke(null, true);
            }
        }

        public int SelectedTradingMode
        {
            get => ((int)CurrentTradingMode);
            set
            {
                CurrentTradingMode = ((TradingMode)value);
                PC();

                App.TradingModeChanged?.Invoke(this, CurrentTradingMode);
            }
        }

        public string SymbolSearch
        {
            get => SymbolSearchValue;
            set
            {
                SymbolSearchValue = value;
                PC();

                App.SearchChanged?.Invoke(value, null);
            }
        }

        #region [ UI ]

        public void DeleteRowLocal(object o)
        {
            if (MainVM.IsListValidTarget())
            {
                Deleted.AddToDeletedListAsync(SelectedListItem.OrderId);
                Deleted.IsDeletedTriggered = true;
            }
        }

        public static async Task ResetControlPositionsAsync()
        {
            await MainVM.ResetControlPositionsAsync();
        }

        public static async Task PaddingWidthAsync()
        {
            await VisibilityVM.AdjustWidthAsync(CurrentTradingMode);
        }

        private void DisplayErrorAndReset(string text, string caption, bool exit = false)
        {
            Message.ShowBox(text, caption, waitForReply: exit, shutdownOrExit: true);

            ResetSymbol();
            MainVM.SearchEnabled = true;
            SymbolSearch = SymbolSearch;
        }

        public static void ResetSymbol()
        {
            try
            {
                MainVM.IsSymbolSelected = false;
                MainVM.IsIsolated = false;
                MainVM.IsMargin = false;

                CurrentSymbolInfo = null;
                RealTimeUpdate = new();

                MainOrders.LastRun = new();
                MainOrders.IsUpdatingOrders = false;
                MainOrders.Orders.Current = new();

                BorrowVM.Clear();
                SettleVM.Clear();
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        public static Thickness BorderAdjustment(WindowState windowState, bool offset = false)
        {
            return windowState == WindowState.Maximized ? new Thickness(App.BORDER_THICKNESS - (offset ? App.BORDER_THICKNESS_OFFSET : 0)) : new Thickness(0);
        }

        #endregion [ UI ]
    }
}
