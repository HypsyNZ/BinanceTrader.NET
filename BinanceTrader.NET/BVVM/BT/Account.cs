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

using BinanceAPI.Objects.Spot.IsolatedMarginData;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BVVM.Log;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class Account : ObservableObject
    {
        public static async Task UpdateSpotInformationAsync()
        {
            try
            {
                var spotInformation = await Client.Local.General.GetAccountInfoAsync().ConfigureAwait(false);
                if (spotInformation.Success)
                {
                    lock (Assets.SpotAssetLock)
                    {
                        Assets.SpotAssets = new ObservableCollection<BinanceBalance>(
                            spotInformation.Data.Balances.Where(b => b.Available != 0 || b.Locked != 0)
                            .Select(b => new BinanceBalance()
                            {
                                Asset = b.Asset,
                                Available = b.Available,
                                Locked = b.Locked
                            }
                            ).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Getting Spot Balances: " + ex.Message);
            }
        }

        public static async Task UpdateIsolatedInformationAsync()
        {
            try
            {
                var isolatedInformation = await Client.Local.Margin.GetIsolatedMarginAccountAsync().ConfigureAwait(false);
                if (isolatedInformation.Success)
                {
                    lock (Assets.IsolatedAssetLock)
                    {
                        Assets.IsolatedAssets = new ObservableCollection<BinanceIsolatedMarginAccountSymbol>(isolatedInformation.Data.Assets.OrderByDescending(d => d.Symbol)
                            .Select(o => new BinanceIsolatedMarginAccountSymbol()
                            {
                                Symbol = o.Symbol,
                                TradeEnabled = o.TradeEnabled,
                                BaseAsset = o.BaseAsset,
                                QuoteAsset = o.QuoteAsset,
                                IndexPrice = o.IndexPrice,
                                LiquidatePrice = o.LiquidatePrice,
                                LiquidateRate = o.LiquidateRate,
                                MarginLevel = o.MarginLevel,
                                MarginLevelStatus = o.MarginLevelStatus,
                                MarginRatio = o.MarginRatio,
                                IsolatedCreated = o.IsolatedCreated
                            }));
                    }
                }
                else
                {
                    if (WriteLog.ShouldLogResp(isolatedInformation))
                    {
                        WriteLog.Error("Error Getting Isolated Balances: " + isolatedInformation.Error?.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Getting Isolated Balances: " + ex.Message);
            }
        }

        public static async Task UpdateMarginInformationAsync()
        {
            try
            {
                var marginInformation = await Client.Local.Margin.GetMarginAccountInfoAsync().ConfigureAwait(false);
                if (marginInformation != null)
                {
                    if (marginInformation.Success)
                    {
                        var account = marginInformation.Data;

                        Static.MarginAccount = new BinanceMarginAccount
                        {
                            BorrowEnabled = account.BorrowEnabled,
                            TradeEnabled = account.TradeEnabled,
                            TotalAssetOfBtc = account.TotalAssetOfBtc,
                            TotalLiabilityOfBtc = account.TotalLiabilityOfBtc,
                            TotalNetAssetOfBtc = account.TotalNetAssetOfBtc,
                            MarginLevel = account.MarginLevel
                        };

                        lock (Assets.MarginAssetLock)
                        {
                            Assets.MarginAssets = new ObservableCollection<BinanceMarginBalance>(account.Balances.OrderByDescending(d => d.Asset).Select(o => new BinanceMarginBalance()
                            {
                                Asset = o.Asset,
                                NetAsset = o.Total,
                                Available = o.Available,
                                Locked = o.Locked,
                                Borrowed = o.Borrowed,
                                Interest = o.Interest
                            }));
                        }
                    }
                    else if (marginInformation.Error != null)
                    {
                        if (WriteLog.ShouldLogResp(marginInformation))
                        {
                            WriteLog.Error("Error Getting Margin Balances: " + marginInformation.Error.Message);
                        }
                    }
                    else
                    {
                        WriteLog.Error("Error Getting Margin Balances: Internal Error");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("[2] Error Getting Margin Balances: " + ex.Message);
            }
        }
    }
}
