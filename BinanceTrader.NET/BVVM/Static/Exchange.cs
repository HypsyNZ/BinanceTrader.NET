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
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Exchange : ObservableObject
    {
        public static DateTime ExchangeInfoUpdateTime { get; set; }

        public static async Task ExchangeInformationAsync()
        {
            try
            {
                var exchangeInfoResult = await Client.Local.Spot.System.GetExchangeInfoAsync();
                if (exchangeInfoResult != null)
                {
                    if (exchangeInfoResult.Success)
                    {
                        WriteLog.Info("Updated Exchange Information..");
                        Static.ManageExchangeInfo.UpdateAndStoreExchangeInfo(exchangeInfoResult.Data);
                        ExchangeInfoUpdateTime = DateTime.Now;
                        WatchMan.ExchangeInfo.SetWorking();
                    }
                    else
                    {
                        WriteLog.Info("Error Loading Exchange Info..");
                        WatchMan.ExchangeInfo.SetError();

                        if (Stored.ExchangeInfo != null)
                        {
                            return;
                        }

                        StoredExchangeInformation(exchangeInfoResult.Error?.Message);
                    }
                }
                else
                {
                    WriteLog.Info("Exchange Info: Internal Error");
                }
            }
            catch (Exception ex)
            {
                StoredExchangeInformation(ex.Message);
                WriteLog.Info("Exchange Info update failed, This may indicate an issue");
            }
        }

        public static async Task ExchangeInfoAllPricesAsync()
        {
            InvokeUI.CheckAccess(() =>
            {
                MainVM.SymbolSelectionHitTest = false;
                MainVM.SearchEnabled = false;
            });

            await ExchangeInformationAsync().ConfigureAwait(false);

            await MarginPairs.AllMarginPairInformation().ConfigureAwait(false);

            await Search.SearchPricesUpdateAsync().ConfigureAwait(false);
        }

        public static void StoredExchangeInformation(string? errormessage)
        {
            errormessage ??= "None";

            WriteLog.Info("Loading Exchange Info from File, This may indicate an Issue..");

            ExchangeInfoUpdateTime = DateTime.Now;
            Static.ManageExchangeInfo.LoadExchangeInfoFromFileAsync();

            if (Stored.ExchangeInfo != null)
            {
                WatchMan.ExchangeInfo.SetWorking();
                _ = Message.ShowBox($"Loaded Exchange Information From File, " +
                    $"This may indicate an issue and will cause problems over time, " +
                    $"If you consistently see this message please update Binance Trader"
                    , "error");
            }
            else
            {
                WatchMan.ExchangeInfo.SetError();
                _ = Message.ShowBox($"Error getting Exchange Information: " +
                    $"{errormessage}", "error");
            }
        }
    }
}
