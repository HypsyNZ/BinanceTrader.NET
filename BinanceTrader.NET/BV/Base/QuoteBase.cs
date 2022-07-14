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

using BinanceAPI;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.Log;
using System;

namespace BTNET.BV.Base
{
    public class QuoteBase : ObservableObject
    {
        private void TradeAmountSell(decimal price, decimal amount)
        {
            QuoteVM.TradeAmountSell = decimal.Round((amount * price).Normalize(), 8);
            TradeVM.OrderQuantity = amount.Normalize();
        }

        private void TradeAmountBuy(decimal price, decimal amount)
        {
            QuoteVM.TradeAmountBuy = decimal.Round((amount * price).Normalize(), 8);
            TradeVM.OrderQuantity = amount.Normalize();
        }

        private void QuoteAmountSell(decimal price, decimal amount)
        {
            if (price > 0 && amount > 0)
            {
                QuoteVM.ObserveQuoteOrderQuantityLocalSell = (decimal.Round(amount / price, 8)).Normalize();
                TradeVM.OrderQuantity = QuoteVM.ObserveQuoteOrderQuantityLocalSell.Normalize();
            }
        }

        private void QuoteAmountBuy(decimal price, decimal amount)
        {
            if (price > 0 && amount > 0)
            {
                QuoteVM.ObserveQuoteOrderQuantityLocalBuy = (decimal.Round(amount / price, 8)).Normalize();
                TradeVM.OrderQuantity = QuoteVM.ObserveQuoteOrderQuantityLocalBuy.Normalize();
            }
        }

        public void GetQuoteOrderQuantityLocal()
        {
            if (Static.SelectedSymbolViewModel == null)
            {
                return;
            }
            try
            {
                if (Static.CurrentlySelectedSymbolTab == SelectedTab.Sell)
                {
                    if (!TradeVM.UseLimitSellBool)
                    {
                        if (TradeVM.QuoteHasFocus && !TradeVM.BaseHasfocus && !TradeVM.UseBaseForQuoteBoolSell)
                        {
                            QuoteAmountSell(Static.RealTimeUpdate.BestBidPrice, QuoteVM.TradeAmountSell);
                        }
                        else
                        {
                            TradeAmountSell(Static.RealTimeUpdate.BestBidPrice, QuoteVM.ObserveQuoteOrderQuantityLocalSell);
                        }

                        TradeVM.SymbolPriceSell = decimal.Round(Static.RealTimeUpdate.BestBidPrice.Normalize(), 8);
                        return;
                    }

                    TradeAmountSell(TradeVM.SymbolPriceSell, QuoteVM.ObserveQuoteOrderQuantityLocalSell);

                    return;
                }

                if (!TradeVM.UseLimitBuyBool)
                {
                    if (TradeVM.QuoteHasFocus && !TradeVM.BaseHasfocus && !TradeVM.UseBaseForQuoteBoolBuy)
                    {
                        QuoteAmountBuy(Static.RealTimeUpdate.BestAskPrice, QuoteVM.TradeAmountBuy);
                    }
                    else
                    {
                        TradeAmountBuy(Static.RealTimeUpdate.BestAskPrice, QuoteVM.ObserveQuoteOrderQuantityLocalBuy);
                    }

                    TradeVM.SymbolPriceBuy = decimal.Round(Static.RealTimeUpdate.BestAskPrice.Normalize(), 8);
                    return;
                }

                TradeAmountBuy(TradeVM.SymbolPriceBuy, QuoteVM.ObserveQuoteOrderQuantityLocalBuy);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Couldnt get Quote", ex);
            }
        }
    }
}
