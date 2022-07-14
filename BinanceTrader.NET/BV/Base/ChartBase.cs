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

namespace BTNET.BV.Base
{
    public class ChartBase
    {
        /// <summary>
        /// Currently Selected Symbol | ex: BTCUSDT
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Currently Selected Symbol Base Asset | ex: BTC
        /// </summary>
        public string? SymbolLeft { get; set; }

        /// <summary>
        /// Currently Selected Symbol Quote Asset | ex: USDT
        /// </summary>
        public string? SymbolRight { get; set; }

        /// <summary>
        /// Currently Selected Symbol - Split | ex: BTC-USDT
        /// </summary>
        public string? SymbolMergeOne { get; set; }

        /// <summary>
        /// Currently Selected Symbol _ Split | ex: BTC_USDT
        /// </summary>
        public string? SymbolMergeTwo { get; set; }
    }
}
