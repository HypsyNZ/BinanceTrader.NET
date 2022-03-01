using BinanceAPI;
using BinanceAPI.Objects;
using BTNET.Abstract;
using ExchangeAPI.Authentication;
using ExchangeAPI.Objects;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BTNET.BVVM
{
    internal class Settings : ObservableObject
    {
        public static void LoadSettings()
        {
            MiniLog.AddLine("Loading Settings");

            BinanceSocketClientOptions binanceSocketClientOptions = new()
            {
#if DEBUG
                LogLevel = LogLevel.Debug,
#endif
#if RELEASE
                LogLevel = LogLevel.Error,
#endif
            };

            BinanceSocketClient.SetDefaultOptions(binanceSocketClientOptions);

            if (File.Exists(Static.settingsfile))
            {
                string j = File.ReadAllText(Static.settingsfile);

                if (j != null)
                {
                    var j2 = JsonConvert.DeserializeObject<ApiKeys>(j);

                    if (j2 != null)
                    {
                        MainVM.ApiKey = j2.ApiKey;
                        MainVM.ApiSecret = j2.ApiSecret;
                    }

                    if (MainVM.ApiKey == null | MainVM.ApiSecret == null)
                    {
                        WriteLog.Error("API Keys Were Null");
                        return;
                    }

                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ReceiveWindow = new TimeSpan(0, 0, 0, 0, 1000),
                        AutoTimestamp = true,
                        ApiCredentials = new ApiCredentials(MainVM.ApiKey, MainVM.ApiSecret),
                        //       UserAgent = App.Product + " (" + App.Version + ")",
#if DEBUG
                        LogLevel = LogLevel.Debug,
#endif
#if RELEASE
                        LogLevel = LogLevel.Error,
#endif
                    });
                }
                else
                {
                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ReceiveWindow = new TimeSpan(0, 0, 0, 0, 1000),
                        //   UserAgent = App.Product + " (" + App.Version + ")",
#if DEBUG
                        LogLevel = LogLevel.Debug,
                        AutoTimestamp = true,
#endif
#if RELEASE
                        LogLevel = LogLevel.Error,
#endif
                    });
                }
            }
        }
    }
}