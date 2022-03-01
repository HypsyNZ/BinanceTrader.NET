using BinanceAPI;
using BinanceAPI.Objects.Spot.MarketData;
using BTNET.BVVM.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Socket : ObservableObject
    {
        public static async Task SubscribeToAllSymbolTickerUpdates()
        {
            // Low Resolution update for every coin (1 second)
            _ = await BTClient.SocketSymbolTicker.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {
                var copy = Static.AllPricesFiltered.ToList();
                foreach (var ud in data.Data)
                {
                    var symbol = copy.SingleOrDefault(p => p.SymbolView.Symbol == ud.Symbol);
                    if (symbol != null)
                    {
                        symbol.SymbolView = new Binance24HPrice
                        {
                            Symbol = ud.Symbol,
                            LastPrice = ConvertBySats.ConvertDecimal(ud.LastPrice, symbol.SymbolView.Symbol, Stored.ExchangeInfo),

                            OpenTime = ud.OpenTime,
                            OpenPrice = ud.OpenPrice,
                            LastQuantity = ud.LastQuantity,
                            LastTradeId = ud.LastTradeId,
                            HighPrice = ud.HighPrice,
                            LowPrice = ud.LowPrice,
                            PrevDayClosePrice = ud.PrevDayClosePrice,
                            PriceChange = ud.PriceChange,
                            PriceChangePercent = ud.PriceChangePercent,
                            BaseVolume = ud.BaseVolume,
                            QuoteVolume = ud.QuoteVolume,
                            CloseTime = ud.CloseTime,
                            TotalTrades = ud.TotalTrades,
                            WeightedAveragePrice = ud.WeightedAveragePrice,
                        };
                    }

                    if (WatchListVM.WatchListItems != null)
                    {
                        var symbolwl = WatchListVM.WatchListItems.SingleOrDefault(p => p.WatchlistSymbol == ud.Symbol);
                        if (symbolwl != null)
                        {
                            symbolwl.WatchlistClose = ud.PrevDayClosePrice;
                            symbolwl.WatchlistChange = ud.PriceChangePercent;
                            symbolwl.WatchlistHigh = ud.HighPrice;
                            symbolwl.WatchlistLow = ud.LowPrice;
                            symbolwl.WatchlistPrice = ud.LastPrice;
                            symbolwl.WatchlistVolume = Helpers.TrimDecimal(ud.BaseVolume);
                        }
                    }
                }

                if (!Static.IsSearching)
                {
                    MainVM.AllSymbolsOnUI = Static.AllPricesFiltered;
                }
            }).ConfigureAwait(false);
        }

        public static void CurrentSymbolTicker()
        {
            var socketClientTicker = new BinanceSocketClient();

            if (Static.CurrentSymbolTickerUpdateSubscription != null)
            {
                _ = socketClientTicker.UnsubscribeAsync(Static.CurrentSymbolTickerUpdateSubscription);
            }

            // High Resolution update for selected coin
            var SymbolUpdateSubscription = socketClientTicker.Spot.SubscribeToBookTickerUpdatesAsync(Static.GetCurrentlySelectedSymbol.SymbolView.Symbol, data =>
            {
                if (Static.GetCurrentlySelectedSymbol == null) { return; }
                try
                {
                    Static.RTUB.BestAskPrice = ConvertBySats.ConvertDecimal(data.Data.BestAskPrice, Static.CurrentSymbolInfo);
                    Static.RTUB.BestAskQuantity = data.Data.BestAskQuantity;
                    Static.RTUB.BestBidPrice = ConvertBySats.ConvertDecimal(data.Data.BestBidPrice, Static.CurrentSymbolInfo);
                    Static.RTUB.BestBidQuantity = data.Data.BestBidQuantity;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Real Time Symbol Ticker Exception: ", ex);
                }
            });

            Static.CurrentSymbolTickerUpdateSubscription = SymbolUpdateSubscription.Result.Data;
        }
    }
}