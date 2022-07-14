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

using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BV.Base
{
    public class StoredExchangeInfo : ObservableObject
    {
        private readonly object ExchangeInfoLock = new object();

        #region [ Load ]

        public Task LoadExchangeInfoFromFileAsync()
        {
            Directory.CreateDirectory(App.SettingsPath);

            BinanceExchangeInfo? ExchangeInfo = TJson.Load<BinanceExchangeInfo>(App.StoredExchangeInfo);

            if (ExchangeInfo != null)
            {
                WriteLog.Info("Loaded [" + ExchangeInfo.Symbols.Count() + "] symbols exchange information from file");
                WatchMan.ExchangeInfo.SetWorking();

                lock (ExchangeInfoLock)
                {
                    Stored.ExchangeInfo = ExchangeInfo;
                }

                return Task.CompletedTask;
            }

            WatchMan.ExchangeInfo.SetError();
            return Task.CompletedTask;
        }

        #endregion [ Load ]

        #region [ Get ]

        public BinanceSymbol? GetStoredSymbolInformation(string symbol)
        {
            if (Stored.ExchangeInfo != null)
            {
                lock (ExchangeInfoLock)
                {
                    return Stored.ExchangeInfo.Symbols.Where(t => t.Name == symbol).FirstOrDefault();
                }
            }

            return null;
        }

        public BinanceExchangeInfo? GetStoredExchangeInfo
        {
            get
            {
                if (Stored.ExchangeInfo != null)
                {
                    lock (ExchangeInfoLock)
                    {
                        return Stored.ExchangeInfo;
                    }
                }

                return null;
            }
        }

        #endregion [ Get ]

        #region [Save]

        public void UpdateAndStoreExchangeInfo(BinanceExchangeInfo exchangeInfoToStore)
        {
            lock (ExchangeInfoLock)
            {
                Stored.ExchangeInfo = exchangeInfoToStore;
            }

            try
            {
                if (exchangeInfoToStore != null)
                {
                    TJson.Save(exchangeInfoToStore, App.StoredExchangeInfo);
                    WriteLog.Info("Updated [" + exchangeInfoToStore.Symbols.Count() + "] symbols Exchange Information and stored it to file");
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error while Storing Exchange Information", ex);
            }
        }

        #endregion [Save]
    }
}
