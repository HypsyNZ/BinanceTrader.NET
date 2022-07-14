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
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.Log;
using LoopDelay.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class InternalOrderTasks : ObservableObject
    {
        private const int DELAY_MS = 3;
        private const int EXPIRE_TIME = 3000;

        private const string S = " | ";
        private const string TRACE = "| Trace: ";
        private const string OUTER = "SettleQuote: OUTER: ";
        private const string INNER = "| INNER: ";
        private const string SELECT_SYMBOL_AGAIN = "Select Symbol Again";
        private const string TRADING_MODE_EXPECTED = "Trading Mode was Expected: ";
        private const string ZERO = "SettleWidget: Settle Amount was Zero";
        private const string BORROW_ZERO = "Borrow Was Zero";

        private const string SUCCESS = "Repay was Successful, Settled: ";
        private const string REQUIRED_VALUE_ZERO = "Settle Failed: A required value was zero";
        private const string DEUG_SETTLE = "DEBUG SETTLE: Asset :";

        private const string FREE_AMOUNT = " | freeAmount :";
        private const string BORROW_AMOUNT = " | borrowedAmount :";
        private const string SYMBOL = " | Symbol :";
        private const string ISOLATED = "| Isolated: ";

        /// <summary>
        /// Creates a discardable <see cref="Task"/> and Settles Requested Asset
        /// <para>Task will be awaited if you use the <see cref="Task{TResult}"/></para>
        /// </summary>
        /// <param name="freeAmount">The current free amount of Asset</param>
        /// <param name="borrowedAmount">The current borrowed amount of Asset</param>
        /// <param name="Asset">The Asset</param>
        /// <param name="Symbol">The Symbol for the Asset</param>
        /// <returns>Boolean indicating success or failure</returns>
        public static async Task<bool> SettleAssetAsync(decimal freeAmount, decimal borrowedAmount, string Asset, string Symbol, bool isolated)
        {
#if DEBUG || DEBUG_SLOW

            WriteLog.Info(
                "TEST SETTLE: Asset :" + Asset +
                " | freeAmount :" + freeAmount +
                " | borrowedAmount :" + borrowedAmount +
                " | Symbol :" + Symbol +
                " | isolated :" + isolated
            );

            await Task.CompletedTask;
            return true;
#else

            bool SettleBool = await Task.Run(() =>
            {
                try
                {
                    if (freeAmount == 0 || borrowedAmount == 0)
                    {
                        WriteLog.Error(REQUIRED_VALUE_ZERO);
                        App.OrderTaskFailed?.Invoke(REQUIRED_VALUE_ZERO, null);
                        return false;
                    }

                    var resultQ = Client.Local.Margin.RepayAsync(Asset, freeAmount, isolated, Symbol, 2000);

                    if (resultQ.Result.Success)
                    {
                        WriteLog.Info(SUCCESS + Asset + S + resultQ.Result.Data.TransactionId + ISOLATED + isolated);
                        return true;
                    }
                    else
                    {
                        WriteLog.Error(resultQ.Result.Error?.Message + ISOLATED + isolated);
                        WriteLog.Info(DEUG_SETTLE + Asset + FREE_AMOUNT + freeAmount + BORROW_AMOUNT + borrowedAmount + SYMBOL + Symbol + ISOLATED + isolated);
                        App.OrderTaskFailed?.Invoke(resultQ.Result.Error?.Message + ISOLATED + isolated, null);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    App.OrderTaskFailed?.Invoke(OUTER + ex.Message + INNER + ex.InnerException + TRACE + ex.StackTrace, null);
                    WriteLog.Error(OUTER + ex.Message + INNER + ex.InnerException + TRACE + ex.StackTrace);
                    return false;
                }
            }).ConfigureAwait(false);

            return SettleBool;
#endif
        }

        /// <summary>
        /// Available amount of asset
        /// </summary>
        /// <typeparam name="T">Accepts collection of Assets that must contain IBinanceBalance</typeparam>
        /// <param name="asset">Asset to return the Available amount for</param>
        /// <param name="assetCollection">Collection of Assets</param>
        /// <returns></returns>
        private static decimal AssetFreeAmount(string asset, ObservableCollection<BinanceBalance> assetCollection)
        {
            var s = assetCollection.SingleOrDefault(p => p.Asset == asset);
            if (s != null)
            {
                return s.Available;
            }

            return 0;
        }

        /// <summary>
        /// Available amount of asset
        /// </summary>
        /// <typeparam name="T">Accepts collection of Assets that must contain IBinanceBalance</typeparam>
        /// <param name="asset">Asset to return the Available amount for</param>
        /// <param name="assetCollection">Collection of Assets</param>
        /// <returns></returns>
        private static decimal AssetFreeAmount(string asset, ObservableCollection<BinanceMarginBalance> assetCollection)
        {
            var s = assetCollection.SingleOrDefault(p => p.Asset == asset);
            if (s != null)
            {
                return s.Available;
            }

            return 0;
        }

        /// <summary>
        /// Current Available Amount based on selected TradingMode
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>Current Available Free Amount of provided Asset based on TradingMode</returns>
        private static decimal CurrentAvailableAmount(BinanceSymbol asset, TradingMode tradingMode)
        {
            switch (tradingMode)
            {
                case TradingMode.Spot:
                    lock (Assets.SpotAssetLock)
                    {
                        return AssetFreeAmount(asset.BaseAsset, Assets.SpotAssets);
                    }

                case TradingMode.Margin:
                    lock (Assets.MarginAssetLock)
                    {
                        return AssetFreeAmount(asset.BaseAsset, Assets.MarginAssets);
                    }

                case TradingMode.Isolated:
                    lock (Assets.IsolatedAssetLock)
                    {
                        return Assets.IsolatedAssets.FirstOrDefault(x => x.Symbol == asset.Name).BaseAsset.Available;
                    }
                default:
                    Message.ShowBox(TRADING_MODE_EXPECTED, SELECT_SYMBOL_AGAIN);
                    WriteLog.Error(TRADING_MODE_EXPECTED + asset.BaseAsset);
                    MainContext.ResetSymbol();
                    return 0;
            }
        }

        /// <summary>
        /// Process Automatic Order / Settle via User Interface
        /// </summary>
        /// <param name="order">The Order</param>
        /// <param name="orderSide">The OrderSide</param>
        /// <param name="borrow">true if the newly placed order should borrow where available</param>
        /// <param name="tradingMode">Trading mode for the order being processed</param>
        /// <param name="quantitypadding">Additional quantity to be added to the order (default 0)</param>
        public static void ProcessOrder(OrderBase order, OrderSide orderSide, bool borrow, TradingMode tradingMode, bool settle = true, decimal quantitypadding = 0)
        {
#if DEBUG
            WriteLog.Info("Symbol: " + order.Symbol + " | Filled: " + order.QuantityFilled + " | OrderPadding: " + quantitypadding + " | Price: " + order.Price + " | Mode: " + tradingMode + " | Borrow: " + borrow + " | Side: " + orderSide);
#endif

            Task.Run(async () =>
            {
                var symbolInfo = Stored.ExchangeInfo!.Symbols.SingleOrDefault(r => r.Name == order.Symbol);

                decimal settleAmount = 0;
                decimal amountBeforeOrder = CurrentAvailableAmount(symbolInfo, tradingMode);

#if DEBUG
                WriteLog.Info("Settle Amount Before Order: " + amountBeforeOrder);
#endif

                var newOrderQuantity = order.QuantityFilled + quantitypadding;

                await Trade.PlaceOrderAsync(order.Symbol, newOrderQuantity, order.Price, tradingMode, borrow, orderSide).ConfigureAwait(false);

                if (!settle)
                {
                    return;
                }

                var startTime = DateTime.UtcNow.Ticks;
                while (await Loop.Delay(startTime, DELAY_MS, EXPIRE_TIME).ConfigureAwait(false))
                {
                    settleAmount = CurrentAvailableAmount(symbolInfo, tradingMode);
                    if (settleAmount != amountBeforeOrder)
                    {
                        break;
                    }
                }

#if DEBUG
                WriteLog.Info("Settle Amount After Order: " + settleAmount);
#endif

                if (amountBeforeOrder + newOrderQuantity == settleAmount)
                {
#if DEBUG
                    WriteLog.Info("Defer Loop");
#endif
                    settleAmount = await DeferAsync(startTime, settleAmount, tradingMode, symbolInfo).ConfigureAwait(false);
                }

#if DEBUG
                WriteLog.Info("Settle Amount After Defer: " + settleAmount);
#endif

                if (settleAmount != 0)
                {
                    decimal borrowed = 0;

                    if (tradingMode == TradingMode.Margin)
                    {
                        lock (Assets.MarginAssetLock)
                        {
                            borrowed = Assets.MarginAssets.Where(t => t.Asset == symbolInfo.BaseAsset).FirstOrDefault().Borrowed;
                        }
                    }
                    else
                    {
                        lock (Assets.IsolatedAssetLock)
                        {
                            borrowed = Assets.IsolatedAssets.Where(t => t.Symbol == order.Symbol).FirstOrDefault().BaseAsset.Borrowed;
                        }
                    }

#if DEBUG
                    WriteLog.Info("Borrow Amount Before Settle: " + borrowed);
#endif

                    if (borrowed != 0)
                    {
                        await SettleAssetAsync(settleAmount, borrowed, symbolInfo.BaseAsset, symbolInfo.Name, tradingMode == TradingMode.Isolated).ConfigureAwait(false);
                    }
                    else
                    {
                        App.OrderTaskFailed?.Invoke(BORROW_ZERO, order);
                        WriteLog.Error(BORROW_ZERO);
                    }
                }
                else
                {
                    App.OrderTaskFailed?.Invoke(ZERO, order);
                    WriteLog.Info(ZERO);
                }
            }).ConfigureAwait(false);
        }

        private static async Task<decimal> DeferAsync(long startTime, decimal amountBeforeDefer, TradingMode tradingMode, BinanceSymbol baseAsset)
        {
            var deferAmount = amountBeforeDefer;
            while (await Loop.Delay(startTime, DELAY_MS, EXPIRE_TIME).ConfigureAwait(false))
            {
                deferAmount = CurrentAvailableAmount(baseAsset, tradingMode);
                if (amountBeforeDefer != deferAmount)
                {
                    break;
                }
            }

            return deferAmount;
        }
    }
}
