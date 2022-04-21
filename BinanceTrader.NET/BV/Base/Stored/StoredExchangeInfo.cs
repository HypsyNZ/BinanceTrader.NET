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

using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BTNET.BV.Base
{
    public class StoredExchangeInfo : ObservableObject
    {
        #region [ Load ]

        public BinanceExchangeInfo LoadExchangeInfoFromFile()
        {
            Directory.CreateDirectory("c:\\BNET\\Settings\\");

            var ExchangeInfo = LoadStoredExchangeInfo(Stored.storedExchangeInfo);

            if (ExchangeInfo != null)
            {
                WriteLog.Info("Loaded Exchange Informaiton from File");
                return ExchangeInfo;
            }

            return null;
        }

        private BinanceExchangeInfo LoadStoredExchangeInfo(string storedExchangeInfoString)
        {
            try
            {
                if (File.Exists(storedExchangeInfoString))
                {
                    string _storedExchangeInfo = File.ReadAllText(storedExchangeInfoString);
                    if (_storedExchangeInfo != null)
                    {
                        try
                        {
                            var _dstoredExchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(_storedExchangeInfo);

                            if (_dstoredExchangeInfo != null)
                            {
                                return _dstoredExchangeInfo;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (JsonSerializationException)
                        {
                            WriteLog.Error("Stored ExchangeInfo failed to deserialize and was deleted, This is usually caused by incorrect closing of BinanceTrader");
                            File.Delete(storedExchangeInfoString);

                            if (!ExchangeInfoRestoreAttempted)
                            {
                                ExchangeInfoRestoreAttempted = Backup.RestoreBackup(storedExchangeInfoString, "ExchangeInfo");

                                return LoadStoredExchangeInfo(storedExchangeInfoString);
                            }
                            else
                            {
                                File.Delete(storedExchangeInfoString + ".bak");
                                WriteLog.Error("Exchange Info Backup was damaged and was deleted");
                            }

                            return null;
                        }
                    }
                    else
                    {
                        WriteLog.Info("ExchangeInfo: Exchange Information couldn't be read or doesn't exist");
                        return null;
                    }
                }
                else
                {
                    if (!ExchangeInfoRestoreAttempted)
                    {
                        ExchangeInfoRestoreAttempted = Backup.RestoreBackup(storedExchangeInfoString, "ExchangeInfo");

                        return LoadStoredExchangeInfo(storedExchangeInfoString);
                    }

                    return null;
                }
            }
            catch (System.Security.SecurityException)
            {
                _ = Static.MessageBox.ShowMessage($"Why did you do this", "Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Stored Exchange Info: ", ex);
                return null;
            }
        }

        private static bool ExchangeInfoRestoreAttempted { get; set; } = false;

        #endregion [ Load ]

        #region [ Get ]

        public BinanceExchangeInfo GetStoredExchangeInfo()
        {
            if (Stored.ExchangeInfo != null)
            {
                return Stored.ExchangeInfo;
            }

            return null;
        }

        #endregion [ Get ]

        #region [ Set ]

        public void UpdateAndStoreExchangeInfo(BinanceExchangeInfo exchangeInfo)
        {
            if (exchangeInfo == null) { return; }

            Stored.ExchangeInfo = exchangeInfo;

            StoreExchangeInfo(Stored.ExchangeInfo, Stored.storedExchangeInfo);
        }

        #endregion [ Set ]

        #region [ Save ]

        private bool StoreExchangeInfo(BinanceExchangeInfo exchangeInfoToStore, string exchangeInfoToStoreFile)
        {
            try
            {
                if (exchangeInfoToStore != null)
                {
                    var convert = JsonConvert.SerializeObject(exchangeInfoToStore, Formatting.Indented, new JsonSerializerSettings());

                    File.WriteAllText(exchangeInfoToStoreFile, convert);

                    var backup = exchangeInfoToStoreFile + ".bak";

                    if (File.Exists(exchangeInfoToStoreFile))
                    {
                        var ExchangeInfo = LoadStoredExchangeInfo(Stored.storedExchangeInfo);
                        if (ExchangeInfo != null)
                        {
                            Backup.SaveBackup(exchangeInfoToStoreFile);
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error while Storing Exchange Information", ex);
                return false;
            }
        }

        #endregion [ Save ]
    }
}