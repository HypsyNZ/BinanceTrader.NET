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

using BinanceAPI.Enums;
using BTNET.BV.Enum;
using BTNET.BVVM.Log;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class Trade : ObservableObject
    {
        private const string FAILED = "Failed";

        public static void Buy()
        {
            if (Static.CurrentlySelectedSymbolTab != SelectedTab.Buy || Static.SelectedSymbolViewModel == null)
            {
                WriteLog.Error("Fatal Error in UI");
                _ = Message.ShowBox($"Restart Binance Trader", FAILED);
                return;
            }

            _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity,
                TradeVM.SymbolPriceBuy, Static.CurrentTradingMode, BorrowVM.BorrowBuy, OrderSide.Buy, TradeVM.UseLimitBuyBool);
        }

        public static void Sell()
        {
            if (Static.CurrentlySelectedSymbolTab != SelectedTab.Sell || Static.SelectedSymbolViewModel == null)
            {
                WriteLog.Error("Fatal Error in UI");
                _ = Message.ShowBox($"Restart Binance Trader", FAILED);
                return;
            }

            _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity,
                TradeVM.SymbolPriceSell, Static.CurrentTradingMode, BorrowVM.BorrowSell, OrderSide.Sell, TradeVM.UseLimitSellBool);
        }

        public static async Task PlaceOrderAsync(string? symbol, decimal quantity, decimal price, TradingMode tradingmode, bool borrow, OrderSide side, bool useLimit = false)
        {
            if (symbol == null)
            {
                _ = Message.ShowBox($"Order placing failed: Internal Error, Please Restart Binance Trader", FAILED);
                return;
            }

#if DEBUG || DEBUG_SLOW

            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            decimal? usePrice = type == OrderType.Limit ? price : null;
            await Task.Delay(1);
            WriteLog.Info(
                "TEST ORDER: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | SymbolPrice :" + price +
                " | BorrowBuy :" + borrow +
                " | UseLimit :" + useLimit +
                " | Type: " + type +
                " | Side: " + side +
                " | usePrice: " + usePrice +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
                : " Error")
            );
            return;

#else

            if (quantity == 0 || price == 0)
            {
                _ = Message.ShowBox($"Restart Binance Trader", FAILED);
                return;
            }

            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            decimal? usePrice = type == OrderType.Limit ? price : null;
            TimeInForce? tif = type == OrderType.Limit ? TimeInForce.GoodTillCancel : null;

            await Task.Run(async () =>
            {
                var result = tradingmode switch
                {
                    TradingMode.Spot =>
                    await Client.Local.Spot.Order.PlaceOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, receiveWindow: 3000, timeInForce: tif),

                    TradingMode.Margin =>
                    !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity,
                    price: usePrice, receiveWindow: 3000, timeInForce: tif)

                    : await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice,
                    sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),

                    TradingMode.Isolated =>
                    !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity,
                    price: usePrice, isIsolated: true, receiveWindow: 3000, timeInForce: tif)

                    : await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, isIsolated: true,
                    sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),
                    _ => null,
                };

                if (result != null)
                {
                    if (result.Success)
                    {
                        WriteLog.Info("Order Placed!: Symbol :" + symbol +
                            " | OrderQuantity :" + quantity +
                            " | SymbolPrice :" + price +
                            " | Type: " + type +
                            " | Side: " + side);
                    }
                    else
                    {
                        _ = Message.ShowBox($"Order placing failed: {result.Error?.Message}", FAILED);
                    }
                }
                else
                {
                    _ = Message.ShowBox($"Order placing failed: Internal Error, Please Restart Binance Trader", FAILED);
                }
            }).ConfigureAwait(false);
#endif
        }
    }
}
