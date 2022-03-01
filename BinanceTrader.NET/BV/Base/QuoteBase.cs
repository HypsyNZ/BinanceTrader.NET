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

using BTNET.BV.Enum;
using BTNET.BVVM;
using System;

namespace BTNET.Base
{
    public class QuoteBase : ObservableObject
    {
        private void TradeAmountSell(decimal price, decimal amount)
        {
            QuoteVM.TradeAmountSell = decimal.Round(amount * price, Static.CurrentStepSize.Scale);
            TradeVM.OrderQuantity = amount;
        }

        private void TradeAmountBuy(decimal price, decimal amount)
        {
            QuoteVM.TradeAmountBuy = decimal.Round(amount * price, Static.CurrentStepSize.Scale);
            TradeVM.OrderQuantity = amount;
        }

        private void QuoteAmountSell(decimal price, decimal amount)
        {
            QuoteVM.ObserveQuoteOrderQuantityLocalSell = decimal.Round(amount / price, Static.CurrentStepSize.Scale);
            TradeVM.OrderQuantity = QuoteVM.ObserveQuoteOrderQuantityLocalSell;
        }

        private void QuoteAmountBuy(decimal price, decimal amount)
        {
            QuoteVM.ObserveQuoteOrderQuantityLocalBuy = decimal.Round(amount / price, Static.CurrentStepSize.Scale);
            TradeVM.OrderQuantity = QuoteVM.ObserveQuoteOrderQuantityLocalBuy;
        }

        public void GetQuoteOrderQuantityLocal()
        {
            if (Static.GetCurrentlySelectedSymbol == null) { return; }
            try
            {
                if (Static.CurrentlySelectedSymbolTab == SelectedTab.Sell)
                {
                    if (!TradeVM.UseLimitSellBool)
                    {
                        if (TradeVM.QuoteHasFocus && !TradeVM.BaseHasfocus && !TradeVM.UseBaseForQuoteBoolSell)
                        {
                            QuoteAmountSell(Static.RTUB.BestBidPrice, QuoteVM.TradeAmountSell);
                        }
                        else
                        {
                            TradeAmountSell(Static.RTUB.BestBidPrice, QuoteVM.ObserveQuoteOrderQuantityLocalSell);
                        }

                        TradeVM.SymbolPrice = Static.RTUB.BestBidPrice;
                        return;
                    }

                    TradeAmountSell(TradeVM.SymbolPrice, QuoteVM.ObserveQuoteOrderQuantityLocalSell);

                    return;
                }

                if (!TradeVM.UseLimitBuyBool)
                {
                    if (TradeVM.QuoteHasFocus && !TradeVM.BaseHasfocus && !TradeVM.UseBaseForQuoteBoolBuy)
                    {
                        QuoteAmountBuy(Static.RTUB.BestAskPrice, QuoteVM.TradeAmountBuy);
                    }
                    else
                    {
                        TradeAmountBuy(Static.RTUB.BestAskPrice, QuoteVM.ObserveQuoteOrderQuantityLocalBuy);
                    }

                    TradeVM.SymbolPrice = Static.RTUB.BestAskPrice;
                    return;
                }

                TradeAmountBuy(TradeVM.SymbolPrice, QuoteVM.ObserveQuoteOrderQuantityLocalBuy);
            }
            catch (Exception ex)
            {
                WriteLog.Error("Couldnt get Quote", ex);
            }
        }
    }
}