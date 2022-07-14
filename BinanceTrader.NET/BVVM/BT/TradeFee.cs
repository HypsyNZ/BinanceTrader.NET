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

using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BVVM.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class TradeFee : ObservableObject
    {
        private const int RECV_WINDOW = 5000;

        public static IEnumerable<BinanceTradeFee>? TradeFeesAllSymbols { get; set; }

        public static BinanceTradeFee GetTradeFee(string symbol)
        {
            var r = TradeFeesAllSymbols?.Where(x => x.Symbol == symbol).FirstOrDefault();
            if (r == default)
            {
                WriteLog.Info("Error getting trade fee");
                return new();
            }

            return r;
        }

        public static async Task GetAllTradeFeesAsync()
        {
            try
            {
                var result = await Client.Local.Spot.Market.GetTradeFeeAsync(receiveWindow: RECV_WINDOW);
                if (result.Success)
                {
                    TradeFeesAllSymbols = result.Data;
                    WatchMan.Load_TradeFee.SetCompleted();
                    WriteLog.Info("Loaded Trade Fee for [" + result.Data.Count() + "] Symbols");
                }
                else
                {
                    WatchMan.Load_TradeFee.SetError();
                    WriteLog.Error("Failed to Get Trade Fees: " + result.Error);
                }
            }
            catch
            {
                WatchMan.Load_TradeFee.SetError();
                WriteLog.Error("Failed to get Trade Fees");
            }
        }
    }
}
