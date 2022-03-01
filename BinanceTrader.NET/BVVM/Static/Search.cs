using BinanceAPI.Enums;
using BTNET.BV.Enum;
using BTNET.BVVM.HELPERS;
using BTNET.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM
{
    internal class Search : ObservableObject
    {
        public static async Task<bool> SearchPricesUpdate()
        {
            var result = await BTClient.Local.Spot.Market.GetPricesAsync().ConfigureAwait(false);
            if (result.Success)
            {
                var prices = result.Data.Select(r => new BinanceSymbolViewModel(r.Symbol, ConvertBySats.ConvertDecimal(r.Price, r.Symbol, Stored.ExchangeInfo)));

                Static.AllPrices = new ObservableCollection<BinanceSymbolViewModel>(prices);
                Static.AllPricesFiltered = new ObservableCollection<BinanceSymbolViewModel>(prices);

                foreach (var ExSymbol in Stored.ExchangeInfo.Symbols)
                {
                    var temp = Static.AllPricesFiltered.SingleOrDefault(o => o.SymbolView.Symbol == ExSymbol.Name);
                    if (temp != null)
                    {
                        if (ExSymbol.Status == SymbolStatus.Close || ExSymbol.Status == SymbolStatus.Break)
                        {
                            _ = Static.AllPricesFiltered.Remove(temp);
                        }
                        else
                        if (Static.CurrentTradingMode == TradingMode.Spot)
                        {
                            if (ExSymbol.IsSpotTradingAllowed == false)
                            {
                                _ = Static.AllPricesFiltered.Remove(temp);
                            }
                        }
                        else if (ExSymbol.IsMarginTradingAllowed == false)
                        {
                            _ = Static.AllPricesFiltered.Remove(temp);
                        }
                    }
                }

                MainVM.AllSymbolsOnUI = Static.AllPricesFiltered;
                WriteLog.Info("Filtered Search: " + (Static.CurrentTradingMode == TradingMode.Spot ? "All Tradable Coins" : (Static.CurrentTradingMode == TradingMode.Margin || Static.CurrentTradingMode == TradingMode.Isolated) ? "Margin/Isolated" : "Unknown"));
            }
            else
            {
                _ = Static.MessageBox.ShowMessage($"Error requesting All Price data: {result.Error.Message}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }
    }
}