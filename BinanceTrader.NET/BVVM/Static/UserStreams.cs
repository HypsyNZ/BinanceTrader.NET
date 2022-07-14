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

using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    internal class UserStreams : ObservableObject
    {
        private const int KEEP_ALIVE_EXPIRE_WARNING_MINS = 30;

        public static DateTime LastUserStreamKeepAlive { get; set; } = new();

        public static Task CloseUserStreamsAsync()
        {
            try
            {
                if (SpotListenKey != string.Empty)
                {
                    _ = Client.Local.Spot.UserStream.StopUserStreamAsync(SpotListenKey).ConfigureAwait(false);
                    SpotListenKey = string.Empty;
                }

                if (MarginListenKey != string.Empty)
                {
                    _ = Client.Local.Margin.UserStream.StopUserStreamAsync(MarginListenKey).ConfigureAwait(false);
                    MarginListenKey = string.Empty;
                }

                if (IsolatedListenKey != string.Empty && LastIsolatedListenKeySymbol != string.Empty)
                {
                    _ = Client.Local.Margin.IsolatedUserStream.CloseIsolatedMarginUserStreamAsync(LastIsolatedListenKeySymbol, IsolatedListenKey).ConfigureAwait(false);
                    IsolatedListenKey = string.Empty;
                }
            }
            catch
            {
                // Fail
            }
            return Task.CompletedTask;
        }

        #region [ UserStream Subscription ]

        public static async Task<bool> OpenUserStreamsAsync()
        {
            bool s = await SpotUserStreamAsync().ConfigureAwait(false);
            bool m = await MarginUserStreamAsync().ConfigureAwait(false);

            return s && m;
        }

        // todo: open stream for all currently active isolated symbols instead of just the selected one
        public static async Task<bool> OpenIsolatedUserStreamAsync(string symbol)
        {
            try
            {
                var startOkay = await Client.Local.Margin.IsolatedUserStream.StartIsolatedMarginUserStreamAsync(symbol).ConfigureAwait(false);
                if (startOkay.Success)
                {
                    WriteLog.Info("Started Isolated User Stream for : " + symbol + " | LK: " + startOkay.Data);
                    LastIsolatedListenKeySymbol = symbol;
                    IsolatedListenKey = startOkay.Data.ToString();

                    var subOkay = await Client.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdateIsolated, null, Socket.OnAccountUpdateIsolated, null).ConfigureAwait(false);
                    if (subOkay.Success)
                    {
                        WriteLog.Info($"Subscribed to Isolated User Stream: " + startOkay.Data);
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        return true;
                    }
                }
            }
            catch
            {
                // Fail
            }
            return false;
        }

        private static async Task<bool> MarginUserStreamAsync()
        {
            try
            {
                var startOkay = await Client.Local.Margin.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                if (startOkay.Success)
                {
                    WriteLog.Info($"Started Margin User Stream: " + startOkay.Data);
                    MarginListenKey = startOkay.Data.ToString();

                    var subOkay = await Client.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdateMargin, null, Socket.OnAccountUpdateMargin, null).ConfigureAwait(false);
                    if (subOkay.Success)
                    {
                        WriteLog.Info($"Subscribed to Margin User Stream: " + startOkay.Data);
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        return true;
                    }
                }
            }
            catch
            {
                // Fail
            }
            return false;
        }

        private static async Task<bool> SpotUserStreamAsync()
        {
            try
            {
                var startOkay = await Client.Local.Spot.UserStream.StartUserStreamAsync().ConfigureAwait(false);
                if (startOkay.Success)
                {
                    WriteLog.Info($"Started Spot User Stream: " + startOkay.Data);
                    SpotListenKey = startOkay.Data.ToString();

                    var subOkay = await Client.SocketClient.Spot.SubscribeToUserDataUpdatesAsync(startOkay.Data, Socket.OnOrderUpdateSpot, null, Socket.OnAccountUpdateSpot, null).ConfigureAwait(false);
                    if (subOkay.Success)
                    {
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info($"Subscribed to Spot User Stream: " + startOkay.Data);
                        return true;
                    }
                }
            }
            catch
            {
                // Fail
            }

            return false;
        }

        public static async Task<bool> GetUserStreamSubscriptionAsync()
        {
            if (!SettingsVM.ApiKeysSet)
            {
                _ = Message.ShowBox($"No Api Keys Found, " +
                    $"Some features won't be available until you enter your API Keys in the Settings.",
                    "Error");
                return false;
            }

            try
            {
                WriteLog.Info("Getting User Stream..");

                await CloseUserStreamsAsync().ConfigureAwait(false);

                var open = await OpenUserStreamsAsync().ConfigureAwait(false);
                if (open)
                {
                    WatchMan.UserStreams.SetWorking();
                    return true;
                }
            }
            catch
            {
                // Fail
            }

            WatchMan.UserStreams.SetError();
            return false;
        }

        public static async Task CheckUserStreamSubscriptionAsync()
        {
            try
            {
                if (GetIsSymbolSelected && DateTime.UtcNow > LastUserStreamKeepAlive + TimeSpan.FromMinutes(KEEP_ALIVE_EXPIRE_WARNING_MINS))
                {
                    WatchMan.UserStreams.SetError();
                    await GetUserStreamSubscriptionAsync().ConfigureAwait(false);
                    LastUserStreamKeepAlive = DateTime.UtcNow;
                    WriteLog.Error("Got new UserStream because the old one wasn't updating anymore");
                }
            }
            catch
            {
                // Fail
            }
        }

        public static async Task KeepAliveKeysAsync()
        {
            try
            {
                if (MarginListenKey != string.Empty)
                {
                    var o = await Client.Local.Margin.UserStream.KeepAliveUserStreamAsync(MarginListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Margin Userstream Alive: " + MarginListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WatchMan.UserStreams.SetError();
                            WriteLog.Info("Keep Alive Failed!" + MarginListenKey);
                        }
                    }
                }

                if (SpotListenKey != string.Empty)
                {
                    var o = await Client.Local.Spot.UserStream.KeepAliveUserStreamAsync(SpotListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Spot Userstream Alive: " + SpotListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WatchMan.UserStreams.SetError();
                            WriteLog.Info("Keep Alive Failed!" + SpotListenKey);
                        }
                    }
                }

                if (IsolatedListenKey != string.Empty)
                {
                    var o = await Client.Local.Margin.UserStream.KeepAliveUserStreamAsync(IsolatedListenKey).ConfigureAwait(false);
                    if (o.Success)
                    {
                        LastUserStreamKeepAlive = DateTime.UtcNow;
                        WriteLog.Info("Kept Userstream Alive: " + IsolatedListenKey);
                    }
                    else
                    {
                        if (WriteLog.ShouldLogResp(o))
                        {
                            WatchMan.UserStreams.SetError();
                            WriteLog.Info("Keep Alive Failed!" + IsolatedListenKey);
                        }
                    }
                }
            }
            catch
            {
                // Fail
            }
        }

        #endregion [ UserStream Subscription ]
    }
}
