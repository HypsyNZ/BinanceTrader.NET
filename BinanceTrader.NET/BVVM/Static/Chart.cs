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
using BTNET.BVVM.Log;

namespace BTNET.BV.Base
{
    public class Chart
    {
        public static ChartBase? ExchangeInfoSplit(BinanceExchangeInfo? binanceExchangeInfo, string? symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol) || binanceExchangeInfo == null)
            {
                return null;
            }

            ChartBase cb = new();
            foreach (var exchangeInfo in binanceExchangeInfo.Symbols)
            {
                if (exchangeInfo.Name == symbol)
                {
                    cb.Symbol = exchangeInfo.Name;
                    cb.SymbolLeft = exchangeInfo.BaseAsset;
                    cb.SymbolRight = exchangeInfo.QuoteAsset;
                    cb.SymbolMergeOne = exchangeInfo.BaseAsset + "-" + exchangeInfo.QuoteAsset;
                    cb.SymbolMergeTwo = exchangeInfo.BaseAsset + "_" + exchangeInfo.QuoteAsset;
                    WriteLog.Info("Returning ExchangeInformation for Symbol: " + cb.Symbol
                        + "| Left: " + cb.SymbolLeft + "| Right: " + cb.SymbolRight + "| MergOne: "
                        + cb.SymbolMergeOne + "| MergeTwo: " + cb.SymbolMergeTwo);
                    return cb;
                }
            }
            WriteLog.Error("Couldn't find Exchange Information for Symbol, This probably indicates a deeper issue");
            return null;
        }
    }
}
