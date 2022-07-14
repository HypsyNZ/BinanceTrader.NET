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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Assets : ObservableObject
    {
        public static readonly object SpotAssetLock = new object();
        public static readonly object MarginAssetLock = new object();
        public static readonly object IsolatedAssetLock = new object();

        public static ObservableCollection<BinanceBalance> SpotAssets { get; set; } = new();
        public static ObservableCollection<BinanceBalance> SpotAssetsLending { get; set; } = new();
        public static ObservableCollection<BinanceMarginBalance> MarginAssets { get; set; } = new();
        public static ObservableCollection<BinanceIsolatedMarginAccountSymbol> IsolatedAssets { get; set; } = new();

        public static async Task SelectedMarginAssetUpdateAsync()
        {
            var pair = await Client.Local.Margin.Market.GetMarginPairAsync(Static.CurrentSymbolInfo!.Name).ConfigureAwait(false);
            if (pair.Success)
            {
                ObservableCollection<BinanceMarginBalance>? assets;

                lock (Assets.MarginAssetLock)
                {
                    assets = Assets.MarginAssets;
                }

                if (assets != null)
                {
                    foreach (var s in assets)
                    {
                        if (s.Asset == pair.Data.BaseAsset)
                        {
                            BorrowVM.BorrowLabelBase = s.Asset;
                            BorrowVM.BorrowedBase = s.Borrowed;
                            BorrowVM.InterestBase = s.Interest;
                            BorrowVM.LockedBase = s.Locked;
                            BorrowVM.FreeBase = s.Available;
                        }
                        if (s.Asset == pair.Data.QuoteAsset)
                        {
                            BorrowVM.BorrowLabelQuote = s.Asset;
                            BorrowVM.BorrowedQuote = s.Borrowed;
                            BorrowVM.InterestQuote = s.Interest;
                            BorrowVM.LockedQuote = s.Locked;
                            BorrowVM.FreeQuote = s.Available;
                        }
                    }
                }

                BorrowVM.MarginLevel = Static.MarginAccount.MarginLevel;
                BorrowVM.TotalAssetOfBtc = Static.MarginAccount.TotalAssetOfBtc;
                BorrowVM.TotalLiabilityOfBtc = Static.MarginAccount.TotalLiabilityOfBtc;
                BorrowVM.TotalNetAssetOfBtc = Static.MarginAccount.TotalNetAssetOfBtc;

                BorrowVM.ShowBreakdown = SettingsVM.ShowBreakDownInfoIsChecked ?? false;
            }
        }

        public static Task SelectedIsolatedAssetUpdateAsync()
        {
            BinanceIsolatedMarginAccountSymbol? isolatedSymbol;

            lock (Assets.IsolatedAssetLock)
            {
                isolatedSymbol = Assets.IsolatedAssets.SingleOrDefault(p => p.Symbol == Static.CurrentSymbolInfo!.Name);
            }

            if (isolatedSymbol != null)
            {
                BorrowVM.MarginLevel = isolatedSymbol.MarginLevel;
                BorrowVM.LiquidationPrice = isolatedSymbol.LiquidatePrice;

                BorrowVM.BorrowLabelBase = isolatedSymbol.BaseAsset.Asset;
                BorrowVM.BorrowedBase = isolatedSymbol.BaseAsset.Borrowed;
                BorrowVM.InterestBase = isolatedSymbol.BaseAsset.Interest;
                BorrowVM.LockedBase = isolatedSymbol.BaseAsset.Locked;
                BorrowVM.FreeBase = isolatedSymbol.BaseAsset.Available;

                BorrowVM.BorrowLabelQuote = isolatedSymbol.QuoteAsset.Asset;
                BorrowVM.BorrowedQuote = isolatedSymbol.QuoteAsset.Borrowed;
                BorrowVM.InterestQuote = isolatedSymbol.QuoteAsset.Interest;
                BorrowVM.LockedQuote = isolatedSymbol.QuoteAsset.Locked;
                BorrowVM.FreeQuote = isolatedSymbol.QuoteAsset.Available;
            }

            BorrowVM.ShowBreakdown = SettingsVM.ShowBreakDownInfoIsChecked ?? false;

            return Task.CompletedTask;
        }

        public static Task SelectedSpotAssetUpdateAsync()
        {
            BorrowVM.ShowBreakdown = SettingsVM.ShowBreakDownInfoIsChecked ?? false;

            BinanceBalance? spotBase;
            BinanceBalance? spotQuote;

            lock (Assets.SpotAssetLock)
            {
                spotBase = Assets.SpotAssets.SingleOrDefault(p => p.Asset == Static.ChartBases.SymbolLeft);
                spotQuote = Assets.SpotAssets.SingleOrDefault(p => p.Asset == Static.ChartBases.SymbolRight);
            }

            if (spotBase != null)
            {
                BorrowVM.BorrowLabelBase = spotBase.Asset;
                BorrowVM.LockedBase = spotBase.Locked;
                BorrowVM.FreeBase = spotBase.Available;
                BorrowVM.TotalBase = spotBase.Total;
            }

            if (spotQuote != null)
            {
                BorrowVM.BorrowLabelQuote = spotQuote.Asset;
                BorrowVM.LockedQuote = spotQuote.Locked;
                BorrowVM.FreeQuote = spotQuote.Available;
                BorrowVM.TotalQuote = spotQuote.Total;
            }

            return Task.CompletedTask;
        }
    }
}
