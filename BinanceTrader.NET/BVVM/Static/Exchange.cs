using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BVVM.HELPERS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM
{
    internal class Exchange : ObservableObject
    {
        public static async Task ExchangeInformation()
        {
            try
            {
                var exchangeInfoResult = await BTClient.Local.Spot.System.GetExchangeInfoAsync();
                if (exchangeInfoResult.Success)
                {
                    WriteLog.Info("Updated Exchange Information..");
                    Stored.ExchangeInfo = exchangeInfoResult.Data;
                    Static.ManageExchangeInfo.UpdateAndStoreExchangeInfo(exchangeInfoResult.Data);
                }
                else
                {
                    WriteLog.Info("Error Loading Exchange Info..");

                    if (Stored.ExchangeInfo != null) { return; }

                    StoredExchangeInformation(exchangeInfoResult.Error.Message);
                }
            }
            catch (Exception ex)
            {
                StoredExchangeInformation(ex.Message);
                WriteLog.Info("Exchange Info update failed, This may indicate an issue");
            }
        }

        public static async Task<bool> ExchangeInfoAllPrices()
        {
            Static.ShouldUpdateExchangeInfo = false;

            await ExchangeInformation().ConfigureAwait(false);

            return await Search.SearchPricesUpdate().ConfigureAwait(false);
        }

        public static void StoredExchangeInformation(string errormessage)
        {
            WriteLog.Info("Loading Exchange Info from File, This may indicate an Issue..");
            Stored.ExchangeInfo = Static.ManageExchangeInfo.GetStoredExchangeInfo();

            if (Stored.ExchangeInfo != null)
            {
                _ = Static.MessageBox.ShowMessage($"Loaded Exchange Information From File, " +
                    $"This may indicate an issue and will cause problems over time, " +
                    $"If you consistently see this message please update Binance Trader"
                    , "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                _ = Static.MessageBox.ShowMessage($"Error getting Exchange Information: " +
                    $"{errormessage}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static BinanceTradeFee GetTradeFee()
        {
            var TradeFee = BTClient.Local.Spot.Market.GetTradeFeeAsync(Static.CurrentSymbolInfo.Name);
            if (TradeFee.Result.Success)
            {
                var tf = TradeFee.Result.Data.First();
                if (tf != null)
                {
                    if (tf.TakerFee == tf.MakerFee)
                    {
                        WriteLog.Info("[Main] Trade Fees for " + tf.Symbol + " | Trade Fee: [" + tf.TakerFee + "]" + "(" + (tf.TakerFee * 100) + "%)");
                        MiniLog.AddLine("Trade Fee is: " + tf.TakerFee + "(" + (tf.TakerFee * 100) + "%)");
                    }
                    else
                    {
                        WriteLog.Info("[Main] Trade Fees for " + tf.Symbol + " | Taker Fee: [" + tf.TakerFee + "](" + (tf.TakerFee * 100) + " %) | Maker Fee: [" + tf.MakerFee + "](" + (tf.MakerFee * 100) + " %)");
                        MiniLog.AddLine("Maker Fee is: " + tf.MakerFee + "(" + (tf.MakerFee * 100) + "%)");
                        MiniLog.AddLine("Taker Fee is: " + tf.TakerFee + "(" + (tf.TakerFee * 100) + "%)");
                        MiniLog.AddLine("Trade Fee Mismatch!");
                        MiniLog.AddLine("Min PnL may be wrong!");
                    }

                    return tf;
                }
            }

            return null;
        }

        public static decimal GetTodaysInterestRate()
        {
            decimal today = 0;
            if (!MainVM.IsIsolated && !MainVM.IsMargin) { return 0; }

            var InterestMarginData = BTClient.Local.Margin.GetInterestMarginDataAsync(asset: Static.CurrentSymbolInfo.BaseAsset, receiveWindow: 5000);
            if (InterestMarginData.Result.Success)
            {
                ObservableCollection<BinanceInterestMarginData> interestMarginData = new(InterestMarginData.Result.Data);
                var cir = interestMarginData.First();
                today = cir.DailyInterest * 100;
                WriteLog.Info("[Main] The Interest Rate for [" + cir.Coin + "] today is [" + today + "] ( Yearly Interest Rate: " + (cir.YearlyInterest * 100) + "%)");
            }
            else
            {
                var InterestRateHistory = BTClient.Local.Margin.GetInterestRateHistoryAsync(Static.CurrentSymbolInfo.BaseAsset, receiveWindow: 5000);
                if (InterestRateHistory.Result.Success)
                {
                    ObservableCollection<BinanceInterestRateHistory> binanceInterestRateHistories = new(InterestRateHistory.Result.Data);
                    var cir = binanceInterestRateHistories.First();
                    today = cir.DailyInterest * 100;
                    WriteLog.Info("[Fallback] The Interest Rate for " + cir.Asset + " today is [" + cir.DailyInterest + "](" + today + "%)");
                }
                else
                {
                    var result = BTClient.Local.Margin.GetInterestHistoryAsync(Static.CurrentSymbolInfo.BaseAsset);
                    if (result.Result.Success)
                    {
                        ObservableCollection<BinanceInterestHistory> f = new(result.Result.Data.Rows);

                        today = f[0].InterestRate * 100;
                        WriteLog.Info("[Fallback] The Interest Rate for " + f[0].Asset + " today is [" + f[0].InterestRate + "](" + today + "%)");
                    }
                }
            }

            return today;
        }
    }
}