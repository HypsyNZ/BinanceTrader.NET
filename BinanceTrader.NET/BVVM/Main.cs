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
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.Base;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.HELPERS;
using ExchangeAPI.Objects;
using ExchangeAPI.Sockets;
using PrecisionTiming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TimerSink;

using static BTNET.BVVM.Static;

namespace BTNET.ViewModels
{
    public class Main : ObservableObject
    {
        public readonly PrecisionTimer PrecisionTimer = new();
        public readonly PrecisionTimer PrecisionTimerRT = new();

        public ObservableCollection<OrderBase> Orders
        { get => GetOrders; set { orders = value; PC(); } }

        public BinanceSymbolViewModel CurrentlySelectedSymbol
        {
            get => GetCurrentlySelectedSymbol;
            set
            {
                selectedSymbolViewModel = value; PC();

                IsSymbolSelected = false;

                if (CurrentlySelectedSymbol != null)
                {
                    MiniLog.AddLine("Changing Symbol..");
                    MainVM.IsCurrentlyLoading = true;
                    ChangeSymbol();
                }
            }
        }

        public Main()
        {
            // UI Data Context, Don't work in MainViewModel
            MainVM = new MainViewModel(this);
            try
            {
                MainVM.IsCurrentlyLoading = true;
                Settings.LoadSettings();

                Task.Run(() =>
                {
#if DEBUG_SLOW
                    TimingSinkItem UpdateGetServerTimeUpdateItem = new(async ()
                        => await UpdateServerTimeTask().ConfigureAwait(false), 325);
                    SinkRT.TimingSinkItems.Add(UpdateGetServerTimeUpdateItem);

                    TimingSinkItem UpdateQuoteUpdateItem = new(async ()
                        => await UpdateQuoteTask().ConfigureAwait(false), 500);
                    SinkRT.TimingSinkItems.Add(UpdateQuoteUpdateItem);

                    TimingSinkItem UpdateBidAskUpdateItem = new(async ()
                        => await UpdateBidAskTask().ConfigureAwait(false), 500);
                    SinkRT.TimingSinkItems.Add(UpdateBidAskUpdateItem);

                    TimingSinkItem UpdateAlertsUpdateItem = new(async ()
                        => await UpdateAlertsTask().ConfigureAwait(false), 500);
                    SinkRT.TimingSinkItems.Add(UpdateAlertsUpdateItem);

                    TimingSinkItem UpdatePNLUpdateItem = new(async ()
                        => await UpdatePnlTask().ConfigureAwait(false), 500);
                    SinkRT.TimingSinkItems.Add(UpdatePNLUpdateItem);

                    TimingSinkItem UpdateBorrowViewUpdateItem = new(async ()
                        => await UpdateBorrowViewModelTask().ConfigureAwait(false), 10000);
                    Sink.TimingSinkItems.Add(UpdateBorrowViewUpdateItem);

                    TimingSinkItem UpdateAccountInformationUpdateItem = new(async ()
                        => await UpdateAccountInformationTask().ConfigureAwait(false), 20000);
                    Sink.TimingSinkItems.Add(UpdateAccountInformationUpdateItem);

                    TimingSinkItem UpdateOrdersUpdateItem = new(async ()
                        => await UpdateOrdersCurrentSymbolTask().ConfigureAwait(false), 100000);
                    Sink.TimingSinkItems.Add(UpdateOrdersUpdateItem);

                    TimingSinkItem UpdateKeepAliveUpdateItem = new(async ()
                        => await UpdateKeepAliveKeysTask().ConfigureAwait(false), 900000);
                    Sink.TimingSinkItems.Add(UpdateKeepAliveUpdateItem);

                    TimingSinkItem UpdateEnforcePriorityUpdateItem = new(async ()
                        => await UpdateEnforcePriorityTask().ConfigureAwait(false), 60000);
                    Sink.TimingSinkItems.Add(UpdateEnforcePriorityUpdateItem);

                    TimingSinkItem MessagePumpUpdateItem = new(async ()
                    => await UpdateMessagePumpTask().ConfigureAwait(false), 2000);
                    Sink.TimingSinkItems.Add(MessagePumpUpdateItem);
#else
                    TimingSinkItem UpdateGetServerTimeUpdateItem = new(async ()
                        => await UpdateServerTimeTask().ConfigureAwait(false), 125);
                    SinkRT.TimingSinkItems.Add(UpdateGetServerTimeUpdateItem);

                    TimingSinkItem UpdateQuoteUpdateItem = new(async ()
                        => await UpdateQuoteTask().ConfigureAwait(false), 100);
                    SinkRT.TimingSinkItems.Add(UpdateQuoteUpdateItem);

                    TimingSinkItem UpdateBidAskUpdateItem = new(async ()
                        => await UpdateBidAskTask().ConfigureAwait(false), 50);
                    SinkRT.TimingSinkItems.Add(UpdateBidAskUpdateItem);

                    TimingSinkItem UpdateAlertsUpdateItem = new(async ()
                        => await UpdateAlertsTask().ConfigureAwait(false), 100);
                    SinkRT.TimingSinkItems.Add(UpdateAlertsUpdateItem);

                    TimingSinkItem UpdatePNLUpdateItem = new(async ()
                        => await UpdatePnlTask().ConfigureAwait(false), 100);
                    SinkRT.TimingSinkItems.Add(UpdatePNLUpdateItem);

                    TimingSinkItem UpdateBorrowViewUpdateItem = new(async ()
                        => await UpdateBorrowViewModelTask().ConfigureAwait(false), 2000);
                    Sink.TimingSinkItems.Add(UpdateBorrowViewUpdateItem);

                    TimingSinkItem UpdateAccountInformationUpdateItem = new(async ()
                        => await UpdateAccountInformationTask().ConfigureAwait(false), 2000);
                    Sink.TimingSinkItems.Add(UpdateAccountInformationUpdateItem);

                    TimingSinkItem UpdateOrdersUpdateItem = new(async ()
                        => await UpdateOrdersCurrentSymbolTask().ConfigureAwait(false), 10000);
                    Sink.TimingSinkItems.Add(UpdateOrdersUpdateItem);

                    TimingSinkItem UpdateKeepAliveUpdateItem = new(async ()
                        => await UpdateKeepAliveKeysTask().ConfigureAwait(false), 900000);
                    Sink.TimingSinkItems.Add(UpdateKeepAliveUpdateItem);

                    TimingSinkItem UpdateEnforcePriorityUpdateItem = new(async ()
                        => await UpdateEnforcePriorityTask().ConfigureAwait(false), 60000);
                    Sink.TimingSinkItems.Add(UpdateEnforcePriorityUpdateItem);

                    TimingSinkItem MessagePumpUpdateItem = new(async ()
                    => await UpdateMessagePumpTask().ConfigureAwait(false), 200);
                    Sink.TimingSinkItems.Add(MessagePumpUpdateItem);
#endif

                    PrecisionTimerRT.SetInterval(SinkRT.SinkTimerTask, 1);
                    SinkRT.Start();
                    PrecisionTimer.SetInterval(Sink.SinkTimerTask, 1);
                    Sink.Start();
                }).ConfigureAwait(false);

                Stored.ExchangeInfo = ManageExchangeInfo.LoadExchangeInfoFromFile();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        SearchEnabled = await Exchange.ExchangeInfoAllPrices().ConfigureAwait(false);
                        SymbolSearch = SymbolSearch;

                        TimingSinkItem UpdateExchangeInfoTaskItem = new(async () => await UpdateExchangeInfoTask().ConfigureAwait(false), 1500);
                        Sink.TimingSinkItems.Add(UpdateExchangeInfoTaskItem);

                        MainVM.IsWatchlistStillLoading = true;

                        _ = Task.Run(async () => { await WatchListVM.InitializeWatchList().ConfigureAwait(false); MainVM.IsWatchlistStillLoading = false; }).ConfigureAwait(false);

                        _ = Task.Run(() => Socket.SubscribeToAllSymbolTickerUpdates()).ConfigureAwait(false);

                        IsStarted = true;
                        MainVM.Chart = "https://www.binance.com";
                        MainVM.IsCurrentlyLoading = false;
                        WriteLog.Info("BTNET Started Successfully");
                        MiniLog.AddLine("BTNET Started Successfully..");
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Error(ex);
                    }
                }).ConfigureAwait(false);

                InitializeAllCommands();
                Deleted.InitializeDeletedList();
                ManageStoredOrders.LoadStoredOrdersFromFile();
            }
            catch (Exception ex)
            {
                MiniLog.AddLine("BTNET Failed to Start Correctly..");
                WriteLog.Error("BTNET Starting Exception: ", ex);
            }
        }

        #region [ Initialize Commands ]

        public ICommand DeleteRowLocalCommand { get; set; }

        /// <summary>
        /// Initialize All Commands across all ViewModels
        /// </summary>
        private void InitializeAllCommands()
        {
            MiniLog.AddLine("Loading Commands");
            MainVM.InitializeCommands();
            BorrowVM.InitializeCommands();
            AlertVM.InitializeCommands();
            TradeVM.InitializeCommands();
            WatchListVM.InitializeCommands();
            SettleVM.InitializeCommands();
            DeleteRowLocalCommand = new DelegateCommand(DeleteRowLocal);
        }

        #endregion [ Initialize Commands ]

        #region [ Long Running Tasks ]

        private Task UpdateMessagePumpTask()
        {
            Invoke.InvokeUI(() =>
            {
                // Set triggers the property changed event its not the actual setter
                MainVM.MiniLogOut = "";
            });

            return Task.CompletedTask;
        }

        private Task UpdateEnforcePriorityTask()
        {
            using (Process p = Process.GetCurrentProcess())
            {
                p.PriorityBoostEnabled = true;
                p.PriorityClass = ProcessPriorityClass.RealTime;
            }

            return Task.CompletedTask;
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
                catch
                {
                    WriteLog.Error("Failed to update Bid/Ask");
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
                catch
                {
                    WriteLog.Error("Failed to update Quote");
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
                    WriteLog.Error("[2] Crashed because of Alerts", ex);
                }
            }

            return Task.CompletedTask;
        }

        private Task UpdatePnlTask()
        {
            if (!BlockOrderUpdate() && Orders != default)
            {
                try
                {
                    foreach (var r in Orders.ToList())
                    {
                        if (BlockOrderUpdate()) { break; }

                        Invoke.InvokeUI(() =>
                        {
                            if (CurrentlySelectedSymbol.InterestRate > 0)
                            {
                                r.IPH = CurrentlySelectedSymbol.InterestRate;
                                r.IPD = CurrentlySelectedSymbol.InterestRate;
                                r.ITD = (decimal)new TimeSpan(DateTime.UtcNow.Ticks - r.CreateTime.Ticks).TotalHours;
                                r.ITDQ = 0;
                            }

                            r.Fulfilled = r.Fulfilled;
                            r.Pnl = r.Status != OrderStatus.Canceled ? (r.Side == OrderSide.Sell ? (r.Price * r.QuantityFilled) - (RTUB.BestAskPrice * r.QuantityFilled) : (RTUB.BestBidPrice * r.QuantityFilled) - (r.Price * r.QuantityFilled)) : 0;
                        });
                    }

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("UpdatePnl Error: " + ex.Message);
                }
            }

            return Task.CompletedTask;
        }

        private Task UpdateServerTimeTask()
        {
            if (IsSymbolSelected)
            {
                try
                {
                    ServerTime.ServerTimeTicks = BTClient.Local.ServerTimeTicks;
                    ServerTime.Time = BTClient.Local.ServerTime;
                    ServerTime.UsedWeight = BTClient.Local.UsedWeight;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("[2] Failure while syncing time: " + ex.Message);
                }
            }

            return Task.CompletedTask;
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
                        WriteLog.Info("Kept Margin Userstream Alive: " + MarginListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + MarginListenKey);
                            MiniLog.AddLine("Keep Alive Failed!");
                        }
                    }
                }

                if (SpotListenKey != string.Empty)
                {
                    var o = await BTClient.Local.Spot.UserStream.KeepAliveUserStreamAsync(SpotListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        WriteLog.Info("Kept Spot Userstream Alive: " + SpotListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + SpotListenKey);
                            MiniLog.AddLine("Keep Alive Failed!");
                        }
                    }
                }

                if (IsolatedListenKey != string.Empty)
                {
                    var o = await BTClient.Local.Margin.UserStream.KeepAliveUserStreamAsync(IsolatedListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        WriteLog.Info("Kept Userstream Alive: " + IsolatedListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WriteLog.Info("Keep Alive Failed!" + IsolatedListenKey);
                            MiniLog.AddLine("Keep Alive Failed!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Timing Sink: Keep Alive Failed" + ex.Message);
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
                WriteLog.Error("Failed to update Borrow View Model: " + ex.Message);
                return;
            }
        }

        private async Task UpdateExchangeInfoTask()
        {
            if (ServerTime.Time.Year == 1) { return; }

            if (Static.ShouldUpdateExchangeInfo && (ServerTime.Time.Minute == 30 || ServerTime.Time.Minute == 0))
            {
                ShouldUpdateExchangeInfo = false;
                SearchEnabled = await Task.Run(() => Exchange.ExchangeInfoAllPrices()).ConfigureAwait(false);
                SymbolSearch = SymbolSearch;
            }

            if (!ShouldUpdateExchangeInfo && (ServerTime.Time.Minute != 30 && ServerTime.Time.Minute != 0))
            {
                ShouldUpdateExchangeInfo = true;
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
                WriteLog.Error("General Error Updating Balances: " + ex.Message);
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
            if (BlockOrderUpdate()) { return; }
            try
            {
                ObservableCollection<OrderBase> OrderUpdate = ManageStoredOrders.LoadStoredOrdersCurrentSymbol(Orders);

                Task<WebCallResult<IEnumerable<BinanceAPI.Objects.Spot.SpotData.BinanceOrder>>> webCallResult = null;

                webCallResult = CurrentTradingMode switch
                {
                    TradingMode.Spot => BTClient.Local.Spot.Order.GetOrdersAsync(CurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, receiveWindow: 1500),
                    TradingMode.Margin => BTClient.Local.Margin.Order.GetMarginAccountOrdersAsync(CurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, receiveWindow: 1500),
                    TradingMode.Isolated => BTClient.Local.Margin.Order.GetMarginAccountOrdersAsync(CurrentlySelectedSymbol.SymbolView.Symbol, null, null, null, 200, true, receiveWindow: 1500),
                    _ => null,
                };

                await webCallResult.ConfigureAwait(false);

                if (webCallResult.Result.Success)
                {
                    foreach (var o in webCallResult.Result.Data)
                    {
                        if (BlockOrderUpdate()) { break; }

                        if (!DeletedList.Contains(o.OrderId) && OrderUpdate.FirstOrDefault(f => f.OrderId == o.OrderId) == null)
                        {
                            Invoke.InvokeUI(() =>
                            {
                                OrderUpdate.Add(Order.NewOrder(o, Static.GetCurrentlySelectedSymbol.TradeFee, Static.GetCurrentlySelectedSymbol.InterestRate));
                                ManageStoredOrders.IsUpdatingStoredOrders = true;
                            });
                        }
                    }
                }

                if (ManageStoredOrders.UpdateStoredOrdersAndRefresh(OrderUpdate))
                {
                    Invoke.InvokeUI(() =>
                    {
                        Orders = new ObservableCollection<OrderBase>(OrderUpdate.OrderByDescending(d => d.CreateTime));
                    });
                }

                return;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Failure while getting Orders: ", ex);
            }
        }

        #endregion [ Long Running Tasks ]

        #region [ UserStream Subscription ]

        private async Task OpenUserStreams()
        {
            chartBases = ChartBase.ExchangeInfoSplit(Stored.ExchangeInfo, CurrentlySelectedSymbol.SymbolView.Symbol);

            switch (CurrentTradingMode)
            {
                case TradingMode.Spot:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade/" + chartBases.SymbolMergeTwo + "?layout=pro&theme=dark";

                        var startOkay = await BTClient.Local.Spot.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            MiniLog.AddLine("Got Spot Stream..");
                            WriteLog.Info($"Started Spot User Stream: " + startOkay.Data);
                            SpotListenKey = startOkay.Data.ToString();

                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, OnOrderUpdate, null, OnAccountUpdateSpot, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                await Account.UpdateSpotInformation().ConfigureAwait(false);
                                WriteLog.Info($"Subscribed to Spot User Stream: " + startOkay.Data);
                            }

                            return;
                        }

                        break;
                    }

                case TradingMode.Margin:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade-margin/" + chartBases.SymbolMergeTwo + "?theme=dark&type=cross";

                        var startOkay = await BTClient.Local.Margin.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            MiniLog.AddLine("Got Margin Stream..");
                            WriteLog.Info($"Started Margin User Stream: " + startOkay.Data);
                            MarginListenKey = startOkay.Data.ToString();
                            MainVM.IsMargin = true;
                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, OnOrderUpdate, null, OnAccountUpdateMargin, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                WriteLog.Info($"Subscribed to Margin User Stream: " + startOkay.Data);
                                await Account.UpdateMarginInformation().ConfigureAwait(false);
                                CurrentlySelectedSymbol.InterestRate = Exchange.GetTodaysInterestRate();
                            }

                            return;
                        }

                        break;
                    }

                case TradingMode.Isolated:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade-margin/" + chartBases.SymbolMergeTwo + "?theme=dark&type=isolated";

                        var startOkay = await BTClient.Local.Margin.IsolatedUserStream.StartIsolatedMarginUserStreamAsync(CurrentlySelectedSymbol.SymbolView.Symbol).ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            MiniLog.AddLine("Got Isolated Stream..");
                            WriteLog.Info("Started Isolated User Stream for : " + CurrentlySelectedSymbol.SymbolView.Symbol + " | LK: " + startOkay.Data);
                            LastIsolatedListenKeySymbol = selectedSymbolViewModel.SymbolView.Symbol;
                            IsolatedListenKey = startOkay.Data.ToString();
                            MainVM.IsIsolated = true;

                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, OnOrderUpdate, null, OnAccountUpdateIsolated, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                WriteLog.Info($"Subscribed to Isolated User Stream: " + startOkay.Data);
                                await Account.UpdateIsolatedInformation().ConfigureAwait(false);
                                CurrentlySelectedSymbol.InterestRate = Exchange.GetTodaysInterestRate();
                            }
                            return;
                        }

                        IsSymbolSelected = false;
                        SearchEnabled = await UserStreams.ResetUserStreamsOnError();
                        Static.MessageBox.ShowMessage("Please enable this Isolated Pair on the website or Try Again", "Error Subscribing to Userstream", MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
            }

            // Error
            IsSymbolSelected = false;
            SearchEnabled = await UserStreams.ResetUserStreamsOnError();
            Static.MessageBox.ShowMessage("There was an Error subscribing to a User Stream, Please Try Again", "Error Subscribing to Userstream", MessageBoxButton.OK, MessageBoxImage.Hand);
        }

        private async Task GetUserStreamSubscription()
        {
            if (MainVM.ApiKey == null || MainVM.ApiSecret == null)
            {
                MiniLog.AddLine("No API Keys!");
                _ = Static.MessageBox.ShowMessage($"No Api Keys Found, " +
                    $"Some features won't be available until you enter your API Keys in the Settings.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MiniLog.AddLine("Getting User Stream..");

            await UserStreams.CloseUserStreams().ConfigureAwait(false);

            await OpenUserStreams().ConfigureAwait(false);
        }

        #endregion [ UserStream Subscription ]

        #region [ Change Symbol ]

        private async void ChangeSymbol()
        {
            await Task.Run(async () =>
            {
                await OnChangeSymbol().ConfigureAwait(false);

                _ = UpdateOrdersCurrentSymbolTask();
            }).ConfigureAwait(false);

            MainVM.IsCurrentlyLoading = false;
        }

        private async Task OnChangeSymbol()
        {
            MainVM.IsIsolated = false;
            MainVM.IsMargin = false;
            BlockOrderUpdates = true;

            BorrowVM.Clear();
            SettleVM.Clear();

            CurrentSymbolInfo = Stored.ExchangeInfo.Symbols.SingleOrDefault(r => r.Name == Static.GetCurrentlySelectedSymbol.SymbolView.Symbol);
            IncrementLotSizeMin = Helpers.TrimDecimal(CurrentSymbolInfo.LotSizeFilter.StepSize);
            MinTickSize = Helpers.TrimDecimal(CurrentSymbolInfo.PriceFilter.TickSize);
            CurrentStepSize = IncrementLotSizeMin;

            Socket.CurrentSymbolTicker();

            QuoteVM.TradeAmountBuy = 0;
            QuoteVM.TradeAmountSell = 0;
            QuoteVM.TradePrice = CurrentlySelectedSymbol.SymbolView.LastPrice;
            BorrowVM.SymbolName = CurrentSymbolInfo.Name;
            BorrowVM.BorrowLabelBase = CurrentSymbolInfo.BaseAsset;
            BorrowVM.BorrowLabelQuote = CurrentSymbolInfo.QuoteAsset;

            await GetUserStreamSubscription().ConfigureAwait(false);

            CurrentlySelectedSymbol.TradeFee = Exchange.GetTradeFee();

            ManageStoredOrders.CurrentSymbolModeStoredOrders(DeletedList);

            ResetOrders();

            IsSymbolSelected = true;
        }

        #endregion [ Change Symbol ]

        #region [ Orders ]

        public bool BlockOrderUpdate()
        {
            if (!IsSymbolSelected || CurrentlySelectedSymbol == null) { ResetOrders(); return true; }
            if (BlockOrderUpdates || WaitingForOrderUpdate) { return true; }
            return false;
        }

        private void ResetOrders()
        {
            Orders = new();
            ManageStoredOrders.IsUpdatingStoredOrders = false;
            BlockOrderUpdates = false;
            WaitingForOrderUpdate = false;
        }

        private void SaveAndUpdateOrder(OrderBase order, BinanceStreamOrderUpdate d, decimal convertedPrice)
        {
            WaitingForOrderUpdate = true;
            Invoke.InvokeUI(() =>
            {
                order.QuantityFilled = d.QuantityFilled;
                order.Quantity = Helpers.TrimDecimal(d.Quantity);
                order.Price = convertedPrice;
                order.Status = d.Status;
                order.TimeInForce = d.TimeInForce.ToString();
                order.UpdateTime = d.UpdateTime;

                Orders = new ObservableCollection<OrderBase>(Orders.OrderByDescending(d => d.CreateTime));
            });
            WaitingForOrderUpdate = false;

            ManageStoredOrders.AddSingleOrder(order);
        }

        private void SaveAndAddOrder(DataEvent<BinanceStreamOrderUpdate> data, decimal convertedPrice)
        {
            OrderBase OrderToAdd = Order.AddNewOrderOnUpdate(data, convertedPrice, Static.GetCurrentlySelectedSymbol.TradeFee, Static.GetCurrentlySelectedSymbol.InterestRate);
            WaitAndAddOrder(OrderToAdd);
            ManageStoredOrders.AddSingleOrder(OrderToAdd);
        }

        private void WaitAndAddOrder(OrderBase order)
        {
            WaitingForOrderUpdate = true;
            Invoke.InvokeUI(() =>
            {
                Orders.Add(order);
                Orders = new ObservableCollection<OrderBase>(Orders.OrderByDescending(d => d.CreateTime));
            });
            WaitingForOrderUpdate = false;
        }

        #endregion [ Orders ]

        #region [ Binance Socket Stream Updates ]

        private void OnAccountUpdateSpot(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    MiniLog.AddLine("Got Account Update!");
                    if (Assets.SpotAssets != null)
                    {
                        foreach (var sym in Assets.SpotAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.Asset);
                            if (d != null)
                            {
                                sym.Available = d.Free;
                                sym.Locked = d.Locked;
                                WriteLog.Info("OnAccountUpdate was processed for: " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdate, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        private void OnAccountUpdateMargin(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    MiniLog.AddLine("Got Account Update!");
                    if (Assets.MarginAssets != null)
                    {
                        foreach (var sym in Assets.MarginAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.Asset);
                            if (d != null)
                            {
                                sym.Available = d.Free;
                                sym.Locked = d.Locked;
                                WriteLog.Info("OnAccountUpdateMargin was processed for " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdateMargin, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        private void OnAccountUpdateIsolated(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    MiniLog.AddLine("Got Account Update!");
                    if (Assets.IsolatedAssets != null)
                    {
                        foreach (var sym in Assets.IsolatedAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.QuoteAsset.Asset);
                            if (d != null)
                            {
                                sym.QuoteAsset.Available = d.Free;
                                sym.QuoteAsset.Locked = d.Locked;
                                sym.QuoteAsset.Total = d.Total;
                                WriteLog.Info("OnAccountUpdateIsolated was processed for " + d.Asset);
                            }

                            var f = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.BaseAsset.Asset);
                            if (d != null)
                            {
                                sym.BaseAsset.Available = d.Free;
                                sym.BaseAsset.Locked = d.Locked;
                                sym.BaseAsset.Total = d.Total;
                                WriteLog.Info("OnAccountUpdateIsolated was processed for " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdateIsolated, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        /// <summary>
        /// Updates order list when notification is recieved from the server
        /// <para>Most orders will have 2 Message Parts at Least</para>
        /// If you change this keep in mind both messages might arrive at the same time or before one another
        /// </summary>
        /// <param name="data">The data from the Order Update</param>
        public void OnOrderUpdate(DataEvent<BinanceStreamOrderUpdate> data)
        {
            var d = data.Data;
            try
            {
                // No Symbol Selected
                if (!IsSymbolSelected)
                {
                    WriteLog.Info("Got OnOrderUpdate while no Symbol was Selected: " + d.OrderId +
                        ", This order will be fetched again when the Symbol is selected.");
                    return;
                }
                else
                if (d.Event == "executionReport")
                {
                    BlockOrderUpdates = true;

                    var order = Orders.SingleOrDefault(f => f.OrderId == d.OrderId);
                    decimal rprice = d.Price != 0 ? d.Price : d.LastPriceFilled;
                    decimal convertedPrice = rprice != 0 ? ConvertBySats.ConvertDecimal(rprice, d.Symbol, Stored.ExchangeInfo) : 0;

                    switch (d.Status)
                    {
                        case OrderStatus.New:
                            {
                                // Skip First Message for Market Orders
                                // Ignore First Message For Limit Orders if an update has already been received
                                if (d.Type != OrderType.Market && order == null)
                                {
                                    // New Limit Orders
                                    MiniLog.AddLine("[New] Got Order!");
                                    SaveAndAddOrder(data, convertedPrice);
                                }
                            }
                            break;

                        case OrderStatus.Filled or OrderStatus.PartiallyFilled:
                            {
                                WriteLog.Info("Got OnOrderUpdate from Binance API, Adding Order: " + d.OrderId);
                                if (order == null)
                                {
                                    // Market Orders and Limit Order Updates that outran the first message
                                    MiniLog.AddLine("[Fill] Got New Order!");
                                    SaveAndAddOrder(data, convertedPrice);
                                }
                                else if (order.QuantityFilled < d.QuantityFilled)
                                {
                                    // Limit Order Updates
                                    MiniLog.AddLine("[Fill] Got Order Update!");
                                    SaveAndUpdateOrder(order, d, convertedPrice);
                                }
                            }
                            break;

                        case OrderStatus.Canceled: Deleted.EnumerateDeletedList(Orders, d.OrderId); WriteLog.Info("Added Cancelled Order to Deleted List: " + d.OrderId); break;

                        case OrderStatus.Rejected: WriteLog.Error($"Order: " + d.OrderId + " was Rejected OnOrderUpdate: " + d.RejectReason); break;

                        default: WriteLog.Info("OnOrderUpdate: Invalid: " + d.OrderId + " | ET: " + d.ExecutionType + " | OS: " + d.Status + " | EV: " + d.Event); break;
                    }
                    BlockOrderUpdates = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog.Info("OnOrderUpdate: Error Updating Order: " + d.OrderId + " | ET: " + d.ExecutionType + " | OS: " + d.Status + " | EV: " + d.Event);
                WriteLog.Error(ex.ToString());
            }
        }

        #endregion [ Binance Socket Stream Updates ]

        #region [ UI ]

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
                        SearchEnabled = true;
                        MainVM.IsCurrentlyLoading = false;
                    }).ConfigureAwait(false);
                }

                MiniLog.AddLine("Selected Mode: " + (CurrentTradingMode == TradingMode.Spot ? "Spot" : CurrentTradingMode == TradingMode.Margin ? "Margin" : CurrentTradingMode == TradingMode.Isolated ? "Isolated" : "Unknown"));
            }
        }

        public string SymbolSearch
        {
            get => this.symbolSearchValue;
            set
            {
                this.symbolSearchValue = value; PC();

                if (SymbolSearch is not null and not "")
                {
                    MainVM.AllSymbolsOnUI = Helpers.EmptyCollection;
                    IsSearching = true;
                    MainVM.AllSymbolsOnUI = new ObservableCollection<BinanceSymbolViewModel>(Static.AllPricesFiltered.Where(ap => ap.SymbolView.Symbol.Contains(this.symbolSearchValue.ToUpper())));
                    return;
                }

                IsSearching = false;
                MainVM.AllSymbolsOnUI = Static.AllPricesFiltered;
            }
        }

        public void DeleteRowLocal(object o)
        {
            if (MainVM.IsListValidTarget())
            {
                Deleted.EnumerateDeletedList(Orders, SelectedListItem.OrderId);
            }
        }

        #endregion [ UI ]
    }
}