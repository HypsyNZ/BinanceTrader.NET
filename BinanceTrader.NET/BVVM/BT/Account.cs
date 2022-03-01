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

using BinanceAPI.Objects.Spot.IsolatedMarginData;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Enum;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class Account : ObservableObject
    {
        public static async Task UpdateAccountInformation()
        {
            switch (Static.CurrentTradingMode)
            {
                case TradingMode.Margin:
                    await Account.UpdateMarginInformation().ConfigureAwait(false); break;
                case TradingMode.Isolated:
                    await Account.UpdateIsolatedInformation().ConfigureAwait(false); break;
                default:
                    await Account.UpdateSpotInformation().ConfigureAwait(false); break;
            }
        }

        public static async Task UpdateSpotInformation()
        {
            try
            {
                var spotInformation = await BTClient.Local.General.GetAccountInfoAsync().ConfigureAwait(false);
                if (spotInformation.Success)
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
            catch (Exception ex)
            {
                WriteLog.Error("Error Getting Spot Balances: " + ex.Message);
            }
        }

        public static async Task UpdateIsolatedInformation()
        {
            try
            {
                var isolatedInformation = await BTClient.Local.Margin.GetIsolatedMarginAccountAsync().ConfigureAwait(false);
                if (isolatedInformation.Success)
                {
                    Assets.IsolatedAssets = new ObservableCollection<BinanceIsolatedMarginAccountSymbol>(isolatedInformation.Data.Assets.OrderByDescending(d => d.Symbol).Select(o => new BinanceIsolatedMarginAccountSymbol()
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
                else
                {
                    if (WriteLog.ShouldLogResp(isolatedInformation))
                    {
                        WriteLog.Error("Error Getting Isolated Balances: " + isolatedInformation.Error.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Getting Isolated Balances: " + ex.Message);
            }
        }

        public static async Task UpdateMarginInformation()
        {
            try
            {
                var marginInformation = await BTClient.Local.Margin.GetMarginAccountInfoAsync().ConfigureAwait(false);

                if (marginInformation.Success)
                {
                    var account = marginInformation.Data;

                    Assets.MarginAssets = new ObservableCollection<BinanceMarginBalance>(account.Balances.OrderByDescending(d => d.Asset).Select(o => new BinanceMarginBalance()
                    {
                        Asset = o.Asset,
                        NetAsset = o.Total,
                        Available = o.Available,
                        Locked = o.Locked,
                        Borrowed = o.Borrowed,
                        Interest = o.Interest
                    }));

                    Static.MarginAccount = new BinanceMarginAccount
                    {
                        BorrowEnabled = account.BorrowEnabled,
                        TradeEnabled = account.TradeEnabled,
                        TotalAssetOfBtc = account.TotalAssetOfBtc,
                        TotalLiabilityOfBtc = account.TotalLiabilityOfBtc,
                        TotalNetAssetOfBtc = account.TotalNetAssetOfBtc,
                        MarginLevel = account.MarginLevel
                    };
                }
                else
                {
                    if (WriteLog.ShouldLogResp(marginInformation))
                    {
                        WriteLog.Error("[1] Error Getting Margin Balances: " + marginInformation.Error.Message);
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