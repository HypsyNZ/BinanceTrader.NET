using BinanceAPI;
using BinanceAPI.Authentication;
using BinanceAPI.Objects;
using BTNET.BV.Abstract;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using SimpleLog4.NET;
using System;
using System.IO;

namespace BTNET.BVVM
{
    internal class Settings : ObservableObject
    {
        public static void LoadSettings()
        {
            WriteLog.Info("Loading Settings");

            BinanceSocketClientOptions binanceSocketClientOptions = new()
            {              
                LogPath = Static.logSocket,
#if RELEASE
                LogLevel = LogLevel.Error,
#else
                LogLevel = LogLevel.Debug,
#endif
            };

            BinanceSocketClient.SetDefaultOptions(binanceSocketClientOptions);

            if (File.Exists(Static.settingsfile))
            {
                string keysRaw = File.ReadAllText(Static.settingsfile);

                if (keysRaw != null || keysRaw != "")
                {
                    var keysDeserialized = JsonConvert.DeserializeObject<ApiKeys>(keysRaw);

                    if (keysDeserialized != null)
                    {
                        MainVM.ApiKey = keysDeserialized.ApiKey;
                        MainVM.ApiSecret = keysDeserialized.ApiSecret;
                    }

                    if (MainVM.ApiKey == "" | MainVM.ApiSecret == "")
                    {
                        WriteLog.Error("API Keys Were Null");
                        return;
                    }

                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ReceiveWindow = TimeSpan.FromSeconds(1),
                        LogPath = Static.logClient,
                        ApiCredentials = new ApiCredentials(MainVM.ApiKey, MainVM.ApiSecret),

#if RELEASE
                        ServerTimeStartTime = 1000,
                        ServerTimeUpdateTime = 125,
                        LogLevel = LogLevel.Error,
#else
                        ServerTimeStartTime = 1000,
                        ServerTimeUpdateTime = 60000,
                        LogLevel = LogLevel.Debug,
#endif
                    });
                }
                else
                {
                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ReceiveWindow = TimeSpan.FromSeconds(1),
                        ServerTimeStartTime = 1000,
                        ServerTimeUpdateTime = 300,
                        LogPath = Static.logClient,
                        LogLevel = LogLevel.Error,
                    });
                }
            }
        }
    }
}