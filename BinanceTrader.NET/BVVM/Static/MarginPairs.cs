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
using BTNET.BVVM.Log;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    public class MarginPairs : ObservableObject
    {
        private static readonly object cml = new object();
        private static readonly object iml = new object();
        private static IEnumerable<BinanceMarginPair>? allCrossMarginPairs;
        private static IEnumerable<BinanceIsolatedMarginSymbol>? allIsolatedMarginPairs;

        public static IEnumerable<BinanceMarginPair>? AllCross
        {
            get
            {
                lock (cml)
                {
                    return allCrossMarginPairs;
                }
            }
            set
            {
                lock (cml)
                {
                    allCrossMarginPairs = value;
                }
            }
        }

        public static IEnumerable<BinanceIsolatedMarginSymbol>? AllIsolated
        {
            get
            {
                lock (iml)
                {
                    return allIsolatedMarginPairs;
                }
            }
            set
            {
                lock (iml)
                {
                    allIsolatedMarginPairs = value;
                }
            }
        }

        public static async Task AllMarginPairInformation()
        {
            var cross = await Client.Local.Margin.Market.GetMarginPairsAsync().ConfigureAwait(false);
            if (cross.Success)
            {
                MarginPairs.AllCross = cross.Data;
            }
            else
            {
                WriteLog.Error("Failed to update Cross Margin Pairs, Search may have some incorrect results");
            }

            var isolated = await Client.Local.Margin.Market.GetIsolatedMarginSymbolsAsync().ConfigureAwait(false);
            if (isolated.Success)
            {
                MarginPairs.AllIsolated = isolated.Data;
            }
            else
            {
                WriteLog.Error("Failed to update Isolated Margin Pairs, Search may have some incorrect results");
            }
        }
    }
}