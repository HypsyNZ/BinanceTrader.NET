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

using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using System.Threading.Tasks;
using static BTNET.BVVM.Static;

namespace BTNET.BVVM
{
    internal class ChangeMode : ObservableObject
    {
        public static async Task<bool> ChangeSelectedSymbolModeAsync(string symbol)
        {
            ChartBases = Chart.ExchangeInfoSplit(Stored.ExchangeInfo, symbol)!;

            switch (CurrentTradingMode)
            {
                case TradingMode.Spot:
                    {
                        await SpotAsync().ConfigureAwait(false);
                        return true;
                    }

                case TradingMode.Margin:
                    {
                        await MarginAsync().ConfigureAwait(false);
                        return true;
                    }

                case TradingMode.Isolated:
                    {
                        bool r = await UserStreams.OpenIsolatedUserStreamAsync(symbol).ConfigureAwait(false);
                        if (!r)
                        {
                            return false;
                        }

                        await IsolatedAsync().ConfigureAwait(false);
                        return true;
                    }
                default: return false;
            }
        }

        private static async Task IsolatedAsync()
        {
            MainVM.Chart = "https://www.binance.com/en/trade-margin/" + ChartBases.SymbolMergeTwo + "?theme=dark&type=isolated";
            MainVM.IsIsolated = true;

            await Account.UpdateIsolatedInformationAsync().ConfigureAwait(false);

            SelectedSymbolViewModel!.InterestRate = InterestRate.GetDailyInterestRateIsolated(SelectedSymbolViewModel!.SymbolView.Symbol);
            WriteLog.Info("The Interest Rate for [" + Static.CurrentSymbolInfo!.BaseAsset + "] today is [" + SelectedSymbolViewModel.InterestRate + "]");
        }

        private static async Task MarginAsync()
        {
            MainVM.Chart = "https://www.binance.com/en/trade-margin/" + ChartBases.SymbolMergeTwo + "?theme=dark&type=cross";
            MainVM.IsMargin = true;

            await Account.UpdateMarginInformationAsync().ConfigureAwait(false);

            SelectedSymbolViewModel!.InterestRate = InterestRate.GetDailyInterestRate(SelectedSymbolViewModel!.SymbolView.Symbol);
            WriteLog.Info("The Interest Rate for [" + Static.CurrentSymbolInfo!.BaseAsset + "] today is [" + SelectedSymbolViewModel.InterestRate + "]");
        }

        private static async Task SpotAsync()
        {
            MainVM.Chart = "https://www.binance.com/en/trade/" + ChartBases.SymbolMergeTwo + "?layout=pro&theme=dark";
            await Account.UpdateSpotInformationAsync().ConfigureAwait(false);
        }
    }
}
