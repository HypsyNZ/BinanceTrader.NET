using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BV.Enum;
using BTNET.BVVM.Controls;
using BTNET.BVVM.Helpers;
using BTNET.VM.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM
{
    internal class Search : ObservableObject
    {
        private static bool ShouldDestroy(BinanceSymbol test)
        {
            if (test.Status == SymbolStatus.Close || test.Status == SymbolStatus.Break)
            {
                return true;
            }
            else if (Static.CurrentTradingMode == TradingMode.Spot)
            {
                if (test.IsSpotTradingAllowed == false)
                {
                    return true;
                }
            }
            else if (test.IsMarginTradingAllowed == false)
            {
                return true;
            }

            return false;
        }

        public static async Task<bool> SearchPricesUpdate()
        {
            var result = await BTClient.Local.Spot.Market.GetPricesAsync();
            var ex = Static.ManageExchangeInfo.GetStoredExchangeInfo();
            var allSymbols = Static.AllPrices.ToList();
            Invoke.InvokeUI(() =>
            {
                if (result.Success)
                {
                    var prices = result.Data.Select(r => new BinanceSymbolViewModel(r.Symbol, DecimalLayout.Convert(r.Price, r.Symbol, ex)));
                    Static.AllPricesUnfiltered = new ObservableCollection<BinanceSymbolViewModel>(prices);
                    foreach (var pr in prices)
                    {
                        BinanceSymbolViewModel f = allSymbols.SingleOrDefault(allSymbols => allSymbols.SymbolView.Symbol == pr.SymbolView.Symbol);
                        if (f != null)
                        {
                            BinanceSymbol exSymbol = ex.Symbols.Where(exsym => exsym.Name == f.SymbolView.Symbol).FirstOrDefault();
                            if (ShouldDestroy(exSymbol))
                            {
                                allSymbols.Remove(f);
                            }
                        }
                        else
                        {
                            BinanceSymbol exSymbol = ex.Symbols.Where(exsym => exsym.Name == pr.SymbolView.Symbol).FirstOrDefault();
                            if (!ShouldDestroy(exSymbol))
                            {
                                allSymbols.Add(pr);
                            }
                        }
                    }

                    Static.AllPrices = new ObservableCollection<BinanceSymbolViewModel>(allSymbols);
                    MainVM.AllSymbolsOnUI = Static.AllPrices;
                }
                else
                {
                    _ = Static.MessageBox.ShowMessage($"Error requesting All Price data: {result.Error.Message}", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            return true;
        }
    }
}