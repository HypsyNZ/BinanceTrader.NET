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
using BinanceAPI.ClientBase;
using BinanceAPI.ClientHosts;
using BinanceAPI.Objects;
using BTNET.BV.Abstract;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using Identity.NET;
using Newtonsoft.Json;
using SimpleLog4.NET;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Settings : ObservableObject
    {
        private const int SERVER_TIME_UPDATE_MS_RELEASE = 125;
        private const int SERVER_TIME_START_MS_RELEASE = 1;

        private const int SERVER_TIME_UPDATE_MS_DEFAULT = 300;
        private const int SERVER_TIME_START_MS_DEFAULT = 1;

        private const int SERVER_TIME_READY_DELAY_MS = 50;
        private const int SERVER_TIME_READY_EXPIRE_MS = 5000;

        public static bool KeysLoaded { get; set; }

        private static void InitializeIdentity()
        {
            UniqueIdentity.Initialize(password: "BinanceTrader.NET", pathToIdentity: @"HKEY_LOCAL_MACHINE\SOFTWARE\BinanceTrader.NET");

            const string testIdentity = "Test";
            string enc = UniqueIdentity.Encrypt(testIdentity);
            string dec = UniqueIdentity.Decrypt(enc);

            if (dec != testIdentity)
            {
                Message.ShowBox("Identity isn't functioning properly, So Binance Trader can't start", "Not Working", waitForReply: true, exit: true, shutdownOrExit: false);
            }
        }

        public static async Task LoadSettingsAsync()
        {
            Directory.CreateDirectory(App.SettingsPath);
            WriteLog.Info("Loading Settings");

            InitializeIdentity();

            var options = new BinanceClientHostOptions()
            {
                ReceiveWindow = TimeSpan.FromSeconds(1),
                LogPath = App.LogClient,
#if RELEASE
                LogLevel = LogLevel.Error,
#else
                LogLevel = LogLevel.Debug,
#endif
            };

            BinanceClientHost.SetDefaultOptions(options);

            SocketClientHost.SetDefaultOptions(new SocketClientHostOptions()
            {
                LogPath = App.LogSocket,
#if RELEASE
                LogLevel = LogLevel.Error,
#else
                LogLevel = LogLevel.Debug,
#endif
            });

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (File.Exists(App.KeyFile))
            {
                string keysRaw = File.ReadAllText(App.KeyFile);

                if (!string.IsNullOrEmpty(keysRaw))
                {
                    var keysDeserialized = JsonConvert.DeserializeObject<ApiKeys>(keysRaw);

                    if (keysDeserialized != null)
                    {
                        // Decrypt keys and store them as a Secure String
                        BaseClient.SetAuthentication(UniqueIdentity.Decrypt(keysDeserialized.ApiKey), UniqueIdentity.Decrypt(keysDeserialized.ApiSecret));
                        // Ensure the keys aren't in the UI.
                        SettingsVM.ApiKey = "Key Loaded From File";
                        SettingsVM.ApiSecret = "Key Loaded From File";
                        SettingsVM.ApiKeysSet = true;

                        // Disable the UI Controls where the keys were set
                        SettingsVM.Disable();
                        KeysLoaded = true;
                    }
                    else
                    {
                        KeysLoaded = false;
                        SettingsVM.Enable();
                        WriteLog.Error("API Keys Were Null");
                        return;
                    }
                }
            }
            else
            {
                KeysLoaded = false;
                BaseClient.SetAuthentication("Public", "Public");
                SettingsVM.Enable();
                Message.ShowBox("Some features won't work until you enter your API Keys in the Settings", "Not Configured");
            }

            try
            {
                Client = new BTClient(cts.Token);
                await ServerTimeClient.Start(options, cts.Token).ConfigureAwait(false);
            }
            catch (Exception cr)
            {
                WriteLog.Error("Failed to Start Server Time Client: " + cr);
                Message.ShowBox("Server Time Client failed to start, So Binance Trader can't start", "Not Working", waitForReply: true, exit: true);
            }

            if (File.Exists(App.Settings))
            {
                string settings = File.ReadAllText(App.Settings);

                if (settings != null)
                {
                    SettingsObject? results = JsonConvert.DeserializeObject<SettingsObject>(settings);

                    if (results != null)
                    {
                        SettingsVM.ShowBorrowInfoIsChecked = results.ShowBorrowInfoIsChecked;
                        SettingsVM.ShowSymbolInfoIsChecked = results.ShowSymbolInfoIsChecked;
                        SettingsVM.ShowMarginInfoIsChecked = results.ShowMarginInfoIsChecked;
                        SettingsVM.ShowIsolatedInfoIsChecked = results.ShowIsolatedInfoIsChecked;
                        SettingsVM.ShowBreakDownInfoIsChecked = results.ShowBreakDownInfoIsChecked;
                        SettingsVM.StretchBrowserIsChecked = results.StretchBrowserIsChecked;
                        SettingsVM.OrderOpacity = results.OrderOpacity;
                        SettingsVM.CheckForUpdatesIsChecked = results.CheckForUpdates;

                        TradeVM.UseBaseForQuoteBoolBuy = results.BuyBaseChecked ?? false;
                        TradeVM.UseBaseForQuoteBoolSell = results.SellBaseChecked ?? false;
                        TradeVM.EnableQuotePriceBuy = !TradeVM.UseLimitBuyBool && !TradeVM.UseBaseForQuoteBoolBuy;
                        TradeVM.EnableQuotePriceSell = !TradeVM.UseLimitSellBool && !TradeVM.UseBaseForQuoteBoolSell;

                        TradeVM.UseLimitBuyBool = results.BuyLimitChecked ?? false;
                        TradeVM.UseLimitSellBool = results.SellLimitChecked ?? false;

                        BorrowVM.BorrowBuy = results.BuyBorrowChecked ?? false;
                        BorrowVM.BorrowSell = results.SellBorrowChecked ?? false;
                    }
                }
            }

            SettingsVM.IsUpToDate = await General.CheckUpTodateAsync().ConfigureAwait(false);
        }
    }
}
