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

using BinanceAPI.Enums;
using BinanceAPI.Objects;
using BTNET.BV.Enum;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM.BT
{
    internal class Trade : ObservableObject
    {
        public static void Buy(TradeViewModel TP, BorrowViewModel BVM)
        {
            if (Static.CurrentlySelectedSymbolTab != SelectedTab.Buy)
            {
                WriteLog.Error("Fatal Error in UI");
                _ = Static.MessageBox.ShowMessage($"Restart Binance Trader", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PlaceOrder(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, TP.OrderQuantity, TP.SymbolPrice, Static.CurrentTradingMode, BVM.BorrowBuy, OrderSide.Buy, TP.UseLimitBuyBool);
        }

        public static void Sell(TradeViewModel TP, BorrowViewModel BVM)
        {
            if (Static.CurrentlySelectedSymbolTab != SelectedTab.Sell)
            {
                WriteLog.Error("Fatal Error in UI");
                _ = Static.MessageBox.ShowMessage($"Restart Binance Trader", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PlaceOrder(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, TP.OrderQuantity, TP.SymbolPrice, Static.CurrentTradingMode, BVM.BorrowSell, OrderSide.Sell, TP.UseLimitSellBool);
        }

        public static Task PlaceOrder(string symbol, decimal quantity, decimal price, TradingMode tradingmode, bool borrow, OrderSide side, bool useLimit = false)
        {
#if DEBUG || DEBUG_SLOW

            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            decimal? usePrice = type == OrderType.Limit ? price : null;

            WriteLog.Info(
                "TEST ORDER: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | SymbolPrice :" + price +
                " | BorrowBuy :" + borrow +
                " | UseLimit :" + useLimit +
                " | Type: " + type +
                " | Side: " + side +
                " | usePrice: " + usePrice +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy" : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell" : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle" : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot" : Static.CurrentTradingMode == TradingMode.Margin ? " Margin" : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated" : " Error")
            );
            return Task.CompletedTask;

#else

            if (quantity == 0 || price == 0)
            {
                _ = Static.MessageBox.ShowMessage($"Restart Binance Trader", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.CompletedTask;
            }

            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            decimal? usePrice = type == OrderType.Limit ? price : null;
            TimeInForce? tif = type == OrderType.Limit ? TimeInForce.GoodTillCancel : null;

            _ = Task.Run(async () =>
            {
                Task<WebCallResult<BinanceAPI.Objects.Spot.SpotData.BinancePlacedOrder>> result = null;

                result = tradingmode switch
                {
                    TradingMode.Spot => BTClient.Local.Spot.Order.PlaceOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, receiveWindow: 3000, timeInForce: tif),
                    TradingMode.Margin => !borrow
                    ? BTClient.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, receiveWindow: 3000, timeInForce: tif)
                    : BTClient.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),
                    TradingMode.Isolated => !borrow
                    ? BTClient.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, isIsolated: true, receiveWindow: 3000, timeInForce: tif)
                    : BTClient.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),
                    _ => null,
                };

                await result;

                if (result.Result.Success)
                {
                    WriteLog.Info("Order Placed!: Symbol :" + symbol +
                        " | OrderQuantity :" + quantity +
                        " | SymbolPrice :" + price +
                        " | Type: " + type +
                        " | Side: " + side);
                }
                else
                {
                    _ = Static.MessageBox.ShowMessage($"Order placing failed: {result.Result.Error.Message}", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
#endif
        }
    }
}