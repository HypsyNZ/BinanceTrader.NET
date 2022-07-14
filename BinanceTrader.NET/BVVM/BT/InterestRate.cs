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

using BinanceAPI.Objects.Spot.MarginData;
using BTNET.BVVM.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class InterestRate : ObservableObject
    {
        private const int ONE_HUNDRED = 100;
        private const int ONE_YEAR = 365;
        private const int RECV_WINDOW = 5000;

        public static IEnumerable<BinanceInterestMarginData>? InterestRatesCrossMargin { get; set; }

        public static IEnumerable<BinanceInterestIsolatedMarginData>? InterestRatesIsolatedMargin { get; set; }

        /// <summary>
        /// Get Daily Interest Rate for Margin Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the rate for the base asset should be returned, false for quote asset</param>
        /// <returns>Daily Interest Rate for the Asset</returns>
        public static decimal GetDailyInterestRate(string symbol, bool baseAsset = true)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return 0;
            }

            var exInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);

            if (exInfo == default)
            {
                WriteLog.Info("Exchange Info was null when getting interest rate");
                return 0;
            }

            return InterestRatesCrossMargin?.Where(t => t.Coin == (baseAsset ? exInfo.BaseAsset : exInfo.QuoteAsset)).FirstOrDefault().DailyInterest * ONE_HUNDRED ?? 0;
        }

        /// <summary>
        /// Get Yearly Interest Rate for Margin Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the rate for the base asset should be returned, false for quote asset</param>
        /// <returns>Yearly Interest Rate for the Asset</returns>
        public static decimal? GetYearlyInterestRate(string symbol, bool baseAsset = true)
        {
            var exInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);

            return InterestRatesCrossMargin?.Where(t => t.Coin == (baseAsset ? exInfo?.BaseAsset : exInfo?.QuoteAsset)).FirstOrDefault().YearlyInterest * ONE_HUNDRED;
        }

        /// <summary>
        /// Get Borrow Limit for Margin Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the limit for the base asset should be returned, false for quote asset</param>
        /// <returns>The Borrow Limit for the Asset</returns>
        public static decimal? GetBorrowLimit(string symbol, bool baseAsset = true)
        {
            var exInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);

            return InterestRatesCrossMargin?.Where(t => t.Coin == (baseAsset ? exInfo?.BaseAsset : exInfo?.QuoteAsset)).FirstOrDefault().BorrowLimit * ONE_HUNDRED;
        }

        /// <summary>
        /// Get Borrowable Status for Margin Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the borrowable status for the base asset should be returned, false for quote asset</param>
        /// <returns>True if the symbol is borrowable</returns>
        public static bool GetIsBorrowable(string symbol, bool baseAsset = true)
        {
            var exInfo = Static.ManageExchangeInfo.GetStoredSymbolInformation(symbol);

            return InterestRatesCrossMargin.Where(t => t.Coin == (baseAsset ? exInfo?.BaseAsset : exInfo?.QuoteAsset)).FirstOrDefault().Borrowable;
        }

        /// <summary>
        /// Get Daily Interest Rate from local collection for Isolated Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the rate for the base asset should be returned, false for quote asset</param>
        /// <returns>Daily Interest for the Isolated Asset</returns>
        public static decimal GetDailyInterestRateIsolated(string symbol, bool baseAsset = true)
        {
            if (baseAsset)
            {
                //ElementAt(0) Base Asset
                return InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(0).DailyInterest * ONE_HUNDRED;
            }

            //ElementAt(1) Quote Asset
            return InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(1).DailyInterest * ONE_HUNDRED;
        }

        /// <summary>
        /// Get Yearly Interest Rate from local collection for Isolated Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the rate for the base asset should be returned, false for quote asset</param>
        /// <returns>Daily Interest for the Isolated Asset</returns>
        public static decimal GetYearlyInterestRateIsolated(string symbol, bool baseAsset = true)
        {
            if (baseAsset)
            {
                //ElementAt(0) Base Asset
                return (InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(0).DailyInterest * ONE_YEAR) * ONE_HUNDRED;
            }

            //ElementAt(1) Quote Asset
            return (InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(1).DailyInterest * ONE_YEAR) * ONE_HUNDRED;
        }

        /// <summary>
        /// Get Borrow Limit from local collection for Isolated Pair
        /// </summary>
        /// <param name="symbol">The Symbol</param>
        /// <param name="baseAsset">True if the limit for the base asset should be returned, false for quote asset</param>
        /// <returns>The Borrow Limit for the Isolated Asset</returns>
        public static decimal GetBorrowLimitIsolated(string symbol, bool baseAsset = true)
        {
            if (baseAsset)
            {
                //ElementAt(0) Base Asset
                return InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(0).BorrowLimit * ONE_HUNDRED;
            }

            //ElementAt(1) Quote Asset
            return InterestRatesIsolatedMargin.Where(t => t.Coin == symbol).FirstOrDefault().Data.ElementAt(1).BorrowLimit * ONE_HUNDRED;
        }

        /// <summary>
        /// Get Interest Rates for all Pairs from the Server
        /// </summary>
        public static async Task GetAllInterestRatesAsync()
        {
            var result = await Client.Local.Margin.GetInterestMarginDataAsync(receiveWindow: RECV_WINDOW);
            if (result.Success)
            {
                InterestRatesCrossMargin = result.Data;
                WatchMan.Load_InterestMargin.SetCompleted();
                WriteLog.Info("Loaded Margin Interest Rates for [" + result.Data.Count() + "] Symbols");
            }
            else
            {
                WriteLog.Error("Failed to Get Margin Interest Rates: " + result.Error?.Message);
                WatchMan.Load_InterestMargin.SetError();
            }

            var resultIsolated = Client.Local.Margin.GetInterestIsolatedMarginDataAsync(receiveWindow: RECV_WINDOW).Result;
            if (resultIsolated.Success)
            {
                WriteLog.Info("Loaded Isolated Interest Rates for [" + resultIsolated.Data.Count() + "] Symbols");
                InterestRatesIsolatedMargin = resultIsolated.Data;
                WatchMan.Load_InterestIsolated.SetCompleted();
            }
            else
            {
                WriteLog.Error("Failed to Get Isolated Interest Rates: " + result.Error?.Message);
                WatchMan.Load_InterestIsolated.SetError();
            }
        }
    }
}
