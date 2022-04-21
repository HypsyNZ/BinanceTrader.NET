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
using BTNET.BVVM.Log;

namespace BTNET.BV.Base
{
    public class ChartBase
    {
        private string symbol;
        private string symbolLeft;
        private string symbolRight;
        private string symbolMergeOne;
        private string symbolMergeTwo;

        /// <summary>
        /// Currently Selected Symbol | ex: BTCUSDT
        /// </summary>
        public string Symbol { get => symbol; set => symbol = value; }

        /// <summary>
        /// Currently Selected Symbol Base Asset | ex: BTC
        /// </summary>
        public string SymbolLeft { get => symbolLeft; set => symbolLeft = value; }

        /// <summary>
        /// Currently Selected Symbol Quote Asset | ex: USDT
        /// </summary>
        public string SymbolRight { get => symbolRight; set => symbolRight = value; }

        /// <summary>
        /// Currently Selected Symbol - Split | ex: BTC-USDT
        /// </summary>
        public string SymbolMergeOne { get => symbolMergeOne; set => symbolMergeOne = value; }

        /// <summary>
        /// Currently Selected Symbol _ Split | ex: BTC_USDT
        /// </summary>
        public string SymbolMergeTwo { get => symbolMergeTwo; set => symbolMergeTwo = value; }

        /// <summary>
        /// Returns common names that can be used for Chart Links
        /// </summary>
        /// <param name="binanceExchangeInfo">BinanceExchangeInfo</param>
        /// <param name="symbol">Currently Selected Symbol</param>
        /// <returns>SymbolMergeOne // BTC-USDT || SymbolMergeTwo // BTC_USDT  </returns>
        public static ChartBase ExchangeInfoSplit(BinanceExchangeInfo binanceExchangeInfo, string symbol)
        {
            ChartBase cb = new();
            foreach (var exchangeInfo in binanceExchangeInfo.Symbols)
            {
                if (exchangeInfo.Name == symbol)
                {
                    cb.symbol = exchangeInfo.Name;
                    cb.symbolLeft = exchangeInfo.BaseAsset;
                    cb.symbolRight = exchangeInfo.QuoteAsset;
                    cb.SymbolMergeOne = exchangeInfo.BaseAsset + "-" + exchangeInfo.QuoteAsset;
                    cb.SymbolMergeTwo = exchangeInfo.BaseAsset + "_" + exchangeInfo.QuoteAsset;
                    WriteLog.Info("Returning ExchangeInformation for Symbol: " + cb.symbol + "| Left: " + cb.symbolLeft + "| Right: " + cb.symbolRight + "| MergOne: " + cb.SymbolMergeOne + "| MergeTwo: " + cb.SymbolMergeTwo);
                    return cb;
                }
            }
            WriteLog.Error("Couldn't find Exchange Information for Symbol, This probably indicates a deeper issue");
            return null;
        }

        public ChartBase()
        {
            this.Symbol = symbol; // BTCUSDT
            this.SymbolLeft = symbolLeft; // BTC
            this.SymbolRight = symbolRight; // USDT
            this.SymbolMergeOne = SymbolMergeOne; // BTC-USDT
            this.SymbolMergeTwo = SymbolMergeTwo; // BTC_USDT
        }
    }
}