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
using Newtonsoft.Json;
using System;
using System.IO;
using static BTNET.BVVM.Stored;

namespace BTNET.Base
{
    public class StoredExchangeInfo : ObservableObject
    {
        #region [ Load ]

        public BinanceExchangeInfo LoadExchangeInfoFromFile()
        {
            Directory.CreateDirectory("c:\\BNET\\Settings\\");

            ExchangeInfo = LoadStoredExchangeInfo(storedExchangeInfo);

            if (ExchangeInfo != null)
            {
                MiniLog.AddLine("Loaded Exchange Informaiton");
                return ExchangeInfo;
            }

            return null;
        }

        private BinanceExchangeInfo LoadStoredExchangeInfo(string storedExchangeInfoString)
        {
            if (File.Exists(storedExchangeInfoString))
            {
                try
                {
                    string _storedExchangeInfo = File.ReadAllText(storedExchangeInfoString);
                    if (_storedExchangeInfo != null)
                    {
                        var _dstoredExchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(_storedExchangeInfo);

                        if (_dstoredExchangeInfo != null)
                        {
                            return _dstoredExchangeInfo;
                        }
                        else
                        {
                            WriteLog.Error("ExchangeInfo: Exchange Information couldn't be deserialized");
                            return null;
                        }
                    }
                    else
                    {
                        WriteLog.Error("ExchangeInfo: Exchange Information couldn't be read or doesn't exist");
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

            return null;
        }

        #endregion [ Load ]

        #region [ Get ]

        public BinanceExchangeInfo GetStoredExchangeInfo()
        {
            if (ExchangeInfo != null)
            {
                return ExchangeInfo;
            }

            return null;
        }

        #endregion [ Get ]

        #region [ Set ]

        public void UpdateAndStoreExchangeInfo(BinanceExchangeInfo exchangeInfo)
        {
            if (exchangeInfo == null) { return; }

            ExchangeInfo = exchangeInfo;

            StoreExchangeInfo(ExchangeInfo, storedExchangeInfo);
        }

        #endregion [ Set ]

        #region [ Save ]

        private bool StoreExchangeInfo(BinanceExchangeInfo exchangeInfoToStore, string exchangeInfoToStoreFile)
        {
            if (ExchangeInfo != null)
            {
                var convert = JsonConvert.SerializeObject(ExchangeInfo, Formatting.Indented, new JsonSerializerSettings());
                File.WriteAllText(storedExchangeInfo, convert);

                return true;
            }

            return false;
        }

        #endregion [ Save ]
    }
}