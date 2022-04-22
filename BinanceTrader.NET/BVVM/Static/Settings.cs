using BinanceAPI;
using BinanceAPI.Authentication;
using BinanceAPI.Objects;
using BTNET.BV.Abstract;
using BTNET.BVVM.Log;
using Identity.NET;
using Newtonsoft.Json;
using SimpleLog4.NET;
using System;
using System.IO;

namespace BTNET.BVVM
{
    internal class Settings : ObservableObject
    {
        protected static ApiCredentials credentials;

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

                if (!string.IsNullOrEmpty(keysRaw))
                {
                    var keysDeserialized = JsonConvert.DeserializeObject<ApiKeys>(keysRaw);
                    if (keysDeserialized != null)
                    {
                        credentials = new ApiCredentials(UniqueIdentity.Decrypt(keysDeserialized.ApiKey).ToSecureString(), UniqueIdentity.Decrypt(keysDeserialized.ApiSecret).ToSecureString());
                        SettingsVM.ApiKey = "Key Loaded From File";
                        SettingsVM.ApiSecret = "Key Loaded From File";
                        SettingsVM.ApiKeysSet = true;
                        SettingsVM.Disable();
                    }
                    else
                    {
                        SettingsVM.Enable();
                        WriteLog.Error("API Keys Were Null");
                        return;
                    }

                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ReceiveWindow = TimeSpan.FromSeconds(1),
                        LogPath = Static.logClient,

                        ApiCredentials = credentials,
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
                    return;
                }
            }

            SettingsVM.Enable();

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