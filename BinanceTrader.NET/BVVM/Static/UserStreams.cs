using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;
using System.Windows;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    internal class UserStreams : ObservableObject
    {
        public static DateTime lastUserStreamKeepAlive = new();

        public static Task CloseUserStreams()
        {
            if (Static.SpotListenKey != string.Empty)
            {
                _ = BTClient.Local.Spot.UserStream.StopUserStreamAsync(Static.SpotListenKey);
                Static.SpotListenKey = string.Empty;
            }

            if (Static.MarginListenKey != string.Empty)
            {
                _ = BTClient.Local.Margin.UserStream.StopUserStreamAsync(Static.MarginListenKey);
                Static.MarginListenKey = string.Empty;
            }

            if (Static.IsolatedListenKey != string.Empty && Static.LastIsolatedListenKeySymbol != string.Empty)
            {
                _ = BTClient.Local.Margin.IsolatedUserStream.CloseIsolatedMarginUserStreamAsync(Static.LastIsolatedListenKeySymbol, Static.IsolatedListenKey);
                Static.IsolatedListenKey = string.Empty;
            }

            return Task.CompletedTask;
        }

        public static async Task<bool> ResetUserStreamsOnError()
        {
            Static.chartBases = new();

            Static.SpotListenKey = string.Empty;

            MainVM.IsIsolated = false;
            Static.IsolatedListenKey = string.Empty;

            MainVM.IsMargin = false;
            Static.MarginListenKey = string.Empty;

            return await Search.SearchPricesUpdate().ConfigureAwait(false);
        }

        #region [ UserStream Subscription ]

        public static async Task<bool> OpenUserStreams()
        {
            chartBases = ChartBase.ExchangeInfoSplit(Stored.ExchangeInfo, Static.GetCurrentlySelectedSymbol.SymbolView.Symbol);

            switch (CurrentTradingMode)
            {
                case TradingMode.Spot:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade/" + chartBases.SymbolMergeTwo + "?layout=pro&theme=dark";

                        var startOkay = await BTClient.Local.Spot.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            WriteLog.Info($"Started Spot User Stream: " + startOkay.Data);
                            SpotListenKey = startOkay.Data.ToString();

                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdate, null, Socket.OnAccountUpdateSpot, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                lastUserStreamKeepAlive = DateTime.UtcNow;
                                await Account.UpdateSpotInformation().ConfigureAwait(false);
                                WriteLog.Info($"Subscribed to Spot User Stream: " + startOkay.Data);
                            }

                            return true;
                        }

                        break;
                    }

                case TradingMode.Margin:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade-margin/" + chartBases.SymbolMergeTwo + "?theme=dark&type=cross";

                        var startOkay = await BTClient.Local.Margin.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            WriteLog.Info($"Started Margin User Stream: " + startOkay.Data);
                            MarginListenKey = startOkay.Data.ToString();
                            MainVM.IsMargin = true;
                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdate, null, Socket.OnAccountUpdateMargin, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                WriteLog.Info($"Subscribed to Margin User Stream: " + startOkay.Data);
                                lastUserStreamKeepAlive = DateTime.UtcNow;
                                await Account.UpdateMarginInformation().ConfigureAwait(false);
                                Static.GetCurrentlySelectedSymbol.InterestRate = Exchange.GetTodaysInterestRate();
                            }

                            return true;
                        }

                        break;
                    }

                case TradingMode.Isolated:
                    {
                        MainVM.Chart = "https://www.binance.com/en/trade-margin/" + chartBases.SymbolMergeTwo + "?theme=dark&type=isolated";

                        var startOkay = await BTClient.Local.Margin.IsolatedUserStream.StartIsolatedMarginUserStreamAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol).ConfigureAwait(false);
                        if (startOkay.Success)
                        {
                            WriteLog.Info("Started Isolated User Stream for : " + Static.GetCurrentlySelectedSymbol.SymbolView.Symbol + " | LK: " + startOkay.Data);
                            LastIsolatedListenKeySymbol = selectedSymbolViewModel.SymbolView.Symbol;
                            IsolatedListenKey = startOkay.Data.ToString();
                            MainVM.IsIsolated = true;

                            var subOkay = await BTClient.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdate, null, Socket.OnAccountUpdateIsolated, null).ConfigureAwait(false);
                            if (subOkay.Success)
                            {
                                WriteLog.Info($"Subscribed to Isolated User Stream: " + startOkay.Data);
                                lastUserStreamKeepAlive = DateTime.UtcNow;
                                await Account.UpdateIsolatedInformation().ConfigureAwait(false);
                                Static.GetCurrentlySelectedSymbol.InterestRate = Exchange.GetTodaysInterestRate();
                            }
                            return true;
                        }

                        return false;
                    }
            }

            return false;
        }

        public static async Task<bool> GetUserStreamSubscription()
        {
            if (!SettingsVM.ApiKeysSet)
            {
                _ = Static.MessageBox.ShowMessage($"No Api Keys Found, " +
                    $"Some features won't be available until you enter your API Keys in the Settings.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            WriteLog.Info("Getting User Stream..");

            await UserStreams.CloseUserStreams().ConfigureAwait(false);

            var open = await OpenUserStreams().ConfigureAwait(false);
            if (open)
            {
                return true;
            }

            return false;
        }

        public static async Task CheckUserStreamSubscription()
        {
            if (Static.GetIsSymbolSelected && DateTime.UtcNow > lastUserStreamKeepAlive + TimeSpan.FromMinutes(30))
            {
                await GetUserStreamSubscription();
                lastUserStreamKeepAlive = DateTime.UtcNow;
                WriteLog.Error("Got new UserStream because the old one wasn't updating anymore");
            }
        }

        #endregion [ UserStream Subscription ]
    }
}