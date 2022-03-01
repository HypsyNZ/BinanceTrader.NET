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
using BinanceAPI.Interfaces;
using BinanceAPI.Objects.Spot.IsolatedMarginData;
using BTNET.Base;
using BTNET.BV.Enum;
using LoopDelay.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class Settle : ObservableObject
    {
        /// <summary>
        /// Creates a discardable <see cref="Task"/> and Settles Requested Asset
        /// <para>Task will be awaited if you use the <see cref="Task{TResult}"/></para>
        /// </summary>
        /// <param name="freeAmount">The current free amount of Asset</param>
        /// <param name="borrowedAmount">The current borrowed amount of Asset</param>
        /// <param name="Asset">The Asset</param>
        /// <param name="Symbol">The Symbol for the Asset</param>
        /// <returns>Boolean indicating success or failure</returns>
        public static async Task<bool> SettleAsset(decimal freeAmount, decimal borrowedAmount, string Asset, string Symbol, bool isolated)
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
                        WriteLog.Error("Settle Failed: A required value was zero");
                        return false;
                    }

                    var resultQ = BTClient.Local.Margin.RepayAsync(Asset, freeAmount, isolated, Symbol, 2000);

                    if (resultQ.Result.Success)
                    {
                        MiniLog.AddLine("Settled: " + Asset);
                        WriteLog.Info("Repay was Successful, Settled: " + Asset + " | " + resultQ.Result.Data.TransactionId + "| Isolated: " + isolated);
                        return true;
                    }
                    else
                    {
                        MiniLog.AddLine("Settle Failed!");
                        WriteLog.Error(resultQ.Result.Error.Message + " | Isolated: " + isolated);
                        WriteLog.Info("DEBUG SETTLE: Asset :" + Asset + " | freeAmount :" + freeAmount + " | borrowedAmount :" + borrowedAmount + " | Symbol :" + Symbol + " | isolated :" + isolated);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("SettleQuote: OUTER: " + ex.Message + "| INNER: " + ex.InnerException + "| ST: " + ex.StackTrace);
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
        private static decimal AssetFreeAmount<T>(string asset, ObservableCollection<T> assetCollection) where T : IBinanceBalance
        {
            T s = assetCollection.ToList().SingleOrDefault(p => p.Asset == asset);
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
        private static decimal CurrentAvailableAmount(string asset)
        {
            decimal availableAmount = 0;

            switch (Static.CurrentTradingMode)
            {
                case TradingMode.Spot:
                    availableAmount = AssetFreeAmount(asset, Assets.SpotAssets);
                    break;

                case TradingMode.Margin:
                    availableAmount = AssetFreeAmount(asset, Assets.MarginAssets);
                    break;

                case TradingMode.Isolated:
                    {
                        BinanceIsolatedMarginAccountAsset settleIsolated = Assets.IsolatedAssets.ToList().FirstOrDefault(x => x.Symbol == asset).BaseAsset;
                        availableAmount = settleIsolated.Available;
                        break;
                    }
            }

            return availableAmount;
        }

        /// <summary>
        /// Process Automatic Order / Settle via User Interface
        /// </summary>
        /// <param name="order">The Order</param>
        /// <param name="orderSide">The OrderSide</param>
        /// <param name="borrow">true if the newly placed order should borrow where available
        /// </param>
        public static void ProcessOrder(OrderBase order, OrderSide orderSide, bool borrow)
        {
            Task.Run(async () =>
            {
                decimal settleAmount = 0;
                decimal amountBeforeOrder = CurrentAvailableAmount(Static.CurrentSymbolInfo.BaseAsset);

                await Trade.PlaceOrder(order.Symbol, order.QuantityFilled, order.Price, Static.CurrentTradingMode, borrow, orderSide).ConfigureAwait(false);

                var startTime = DateTime.UtcNow.Ticks;
#if DEBUG
                MiniLog.AddLine("Before: " + amountBeforeOrder);
                int t = 0;
#endif
                while (await Loop.Delay(startTime, 3, 3000).ConfigureAwait(false))
                {
                    settleAmount = CurrentAvailableAmount(Static.CurrentSymbolInfo.BaseAsset);
                    if (settleAmount != amountBeforeOrder) { break; }
#if DEBUG
                    MiniLog.AddLine("t1: " + t); t++;
#endif
                }

                if (amountBeforeOrder + order.QuantityFilled == settleAmount)
                {
#if DEBUG
                    MiniLog.AddLine("Defer");
#endif
                    settleAmount = await Defer(startTime, settleAmount).ConfigureAwait(false);
                }

                if (settleAmount != 0)
                {
                    await Settle.SettleAsset(settleAmount, BorrowVM.BorrowedBase, Static.CurrentSymbolInfo.BaseAsset, Static.CurrentSymbolInfo.Name, MainVM.IsIsolated && !MainVM.IsMargin).ConfigureAwait(false);
                }
                else
                {
                    WriteLog.Info("SettleWidget: No Reason to Settle");
                }
            }).ConfigureAwait(false);
        }

        private static async Task<decimal> Defer(long startTime, decimal amountBeforeDefer)
        {
            var deferAmount = amountBeforeDefer;
#if DEBUG
            int t = 0;
#endif
            while (await Loop.Delay(startTime, 3, 2000).ConfigureAwait(false))
            {
                deferAmount = CurrentAvailableAmount(Static.CurrentSymbolInfo.BaseAsset);
                if (amountBeforeDefer != deferAmount) { break; }
#if DEBUG
                MiniLog.AddLine("t1: " + t); t++;
#endif
            }

            return deferAmount;
        }
    }
}