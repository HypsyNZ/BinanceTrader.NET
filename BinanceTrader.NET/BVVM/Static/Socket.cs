using BinanceAPI;
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Sockets;
using BTNET.BVVM.Controls;
using BTNET.BVVM.Log;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Socket : ObservableObject
    {
        public static void OnAccountUpdateSpot(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    WriteLog.Info("Got Account Update!");
                    if (Assets.SpotAssets != null)
                    {
                        foreach (var sym in Assets.SpotAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.Asset);
                            if (d != null)
                            {
                                sym.Available = d.Free;
                                sym.Locked = d.Locked;
                                WriteLog.Info("OnAccountUpdate was processed for: " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdate, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        public static void OnAccountUpdateMargin(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    WriteLog.Info("Got Account Update!");
                    if (Assets.MarginAssets != null)
                    {
                        foreach (var sym in Assets.MarginAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.Asset);
                            if (d != null)
                            {
                                sym.Available = d.Free;
                                sym.Locked = d.Locked;
                                WriteLog.Info("OnAccountUpdateMargin was processed for " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdateMargin, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        public static void OnAccountUpdateIsolated(DataEvent<BinanceStreamPositionsUpdate> data)
        {
            if (data != null)
            {
                try
                {
                    WriteLog.Info("Got Account Update!");
                    if (Assets.IsolatedAssets != null)
                    {
                        foreach (var sym in Assets.IsolatedAssets)
                        {
                            var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.QuoteAsset.Asset);
                            if (d != null)
                            {
                                sym.QuoteAsset.Available = d.Free;
                                sym.QuoteAsset.Locked = d.Locked;
                                sym.QuoteAsset.Total = d.Total;
                                WriteLog.Info("OnAccountUpdateIsolated was processed for " + d.Asset);
                            }

                            var f = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.BaseAsset.Asset);
                            if (d != null)
                            {
                                sym.BaseAsset.Available = d.Free;
                                sym.BaseAsset.Locked = d.Locked;
                                sym.BaseAsset.Total = d.Total;
                                WriteLog.Info("OnAccountUpdateIsolated was processed for " + d.Asset);
                            }
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdateIsolated, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        /// <summary>
        /// Updates order list when notification is recieved from the server
        /// <para>Most orders will have 2 Message Parts at Least</para>
        /// If you change this keep in mind both messages might arrive at the same time or before one another
        /// </summary>
        /// <param name="data">The data from the Order Update</param>
        public static void OnOrderUpdate(DataEvent<BinanceStreamOrderUpdate> data)
        {
            var d = data.Data;
            try
            {
                // No Symbol Selected
                if (!Static.GetIsSymbolSelected)
                {
                    WriteLog.Info("Got OnOrderUpdate while no Symbol was Selected: " + d.OrderId +
                        ", This order will be fetched again when the Symbol is selected.");
                    return;
                }
                else
                if (d.Event == "executionReport")
                {
                    var order = MainOrders.GetOrders.SingleOrDefault(f => f.OrderId == d.OrderId);
                    switch (d.Status)
                    {
                        case OrderStatus.New:
                            {
                                // Skip First Message for Market Orders
                                // Ignore First Message For Limit Orders if an update has already been received
                                if (d.Type != OrderType.Market && order == null)
                                {
                                    // New Limit Orders
                                    WriteLog.Info("[New] Got Order!");
                                    Orders.AddNewOrderUpdateEvents(data.Data);
                                }

                                break;
                            }

                        case OrderStatus.Filled or OrderStatus.PartiallyFilled:
                            {
                                WriteLog.Info("Got OnOrderUpdate from Binance API, Adding Order: " + d.OrderId);
                                if (order == null)
                                {
                                    // Market Orders and Limit Order Updates that outran the first message
                                    WriteLog.Info("[Fill] Got New Order!");
                                    Orders.AddNewOrderUpdateEvents(data.Data);
                                }
                                else if (order.QuantityFilled < d.QuantityFilled)
                                {
                                    // Limit Order Updates
                                    WriteLog.Info("[Fill] Got Order Update!");
                                    Orders.AddNewOrderUpdateEvents(data.Data);
                                }

                                break;
                            }

                        case OrderStatus.Canceled: { _ = Deleted.AddToDeletedList(d.OrderId); WriteLog.Info("Added Cancelled Order to Deleted List: " + d.OrderId); break; }

                        case OrderStatus.Rejected: { WriteLog.Error($"Order: " + d.OrderId + " was Rejected OnOrderUpdate: " + d.RejectReason); break; }

                        default: { WriteLog.Info("OnOrderUpdate: Invalid: " + d.OrderId + " | ET: " + d.ExecutionType + " | OS: " + d.Status + " | EV: " + d.Event); break; }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Info("OnOrderUpdate: Error Updating Order: " + d.OrderId + " | ET: " + d.ExecutionType + " | OS: " + d.Status + " | EV: " + d.Event);
                WriteLog.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Low Resolution update for every coin (1 second)
        /// </summary>
        /// <returns></returns>
        public static async Task SubscribeToAllSymbolTickerUpdates()
        {
            // All Prices
            _ = await BTClient.SocketSymbolTicker.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {
                try
                {
                    var exchangeInfo = Static.ManageExchangeInfo.GetStoredExchangeInfo();
                    var pricesFilter = Static.AllPrices.ToList();
                    foreach (var ud in data.Data)
                    {
                        var symbol = pricesFilter.SingleOrDefault(p => p.SymbolView.Symbol == ud.Symbol);
                        if (symbol != null)
                        {
                            symbol.SymbolView = new Binance24HPrice
                            {
                                Symbol = ud.Symbol,
                                LastPrice = DecimalLayout.Convert(ud.LastPrice, ud.Symbol, exchangeInfo),

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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            // Watchlist
            _ = await BTClient.SocketWatchlistTicker.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {
                try
                {
                    if (WatchListVM.WatchListItems != null)
                    {
                        foreach (var ud in data.Data)
                        {
                            var symbolwl = WatchListVM.WatchListItems.SingleOrDefault(p => p.WatchlistSymbol == ud.Symbol);
                            if (symbolwl != null)
                            {
                                symbolwl.WatchlistClose = ud.PrevDayClosePrice;
                                symbolwl.WatchlistChange = ud.PriceChangePercent;
                                symbolwl.WatchlistHigh = ud.HighPrice;
                                symbolwl.WatchlistLow = ud.LowPrice;
                                symbolwl.WatchlistPrice = ud.LastPrice;
                                symbolwl.WatchlistVolume = DecimalLayout.TrimDecimal(ud.BaseVolume);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
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
                    Static.RTUB.BestAskPrice = DecimalLayout.Convert(data.Data.BestAskPrice, Static.CurrentSymbolInfo);
                    Static.RTUB.BestAskQuantity = data.Data.BestAskQuantity;
                    Static.RTUB.BestBidPrice = DecimalLayout.Convert(data.Data.BestBidPrice, Static.CurrentSymbolInfo);
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