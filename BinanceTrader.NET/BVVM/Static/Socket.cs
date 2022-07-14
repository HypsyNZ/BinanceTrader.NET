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

using BinanceAPI;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.MarketStream;
using BinanceAPI.Objects.Spot.SpotData;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Sockets;
using BTNET.BV.Enum;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
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
                    WriteLog.Info("Got Spot Account Update!");
                    if (Assets.SpotAssets != null)
                    {
                        foreach (var bal in data.Data.Balances)
                        {
                            lock (Assets.SpotAssetLock)
                            {
                                var asset = Assets.SpotAssets.SingleOrDefault(x => x.Asset == bal.Asset);
                                if (asset != null)
                                {
                                    asset.Available = bal.Free;
                                    asset.Locked = bal.Locked;
                                    WriteLog.Info("[Update] AccountUpdate was processed for: " + asset.Asset);
                                }
                                else
                                {
                                    Assets.SpotAssets.Add(new BinanceBalance()
                                    {
                                        Asset = bal.Asset,
                                        Available = bal.Free,
                                        Locked = bal.Locked
                                    });

                                    WriteLog.Info("[Add] AccountUpdate was processed for: " + bal.Asset);
                                }
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
                    WriteLog.Info("Got Margin Account Update!");
                    if (Assets.MarginAssets != null)
                    {
                        foreach (var bal in data.Data.Balances)
                        {
                            lock (Assets.MarginAssetLock)
                            {
                                var asset = Assets.MarginAssets.SingleOrDefault(x => x.Asset == bal.Asset);
                                if (asset != null)
                                {
                                    asset.Available = bal.Free;
                                    asset.Locked = bal.Locked;
                                    WriteLog.Info("[Update] AccountUpdateMargin was processed for " + asset.Asset);
                                }
                                else
                                {
                                    Assets.MarginAssets.Add(new BinanceMarginBalance()
                                    {
                                        Asset = bal.Asset,
                                        Locked = bal.Locked,
                                        Available = bal.Free
                                    });

                                    WriteLog.Info("[Add] AccountUpdateMargin was processed for " + bal.Asset);
                                }
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
                    WriteLog.Info("Got Isolated Account Update!");
                    if (Assets.IsolatedAssets != null)
                    {
                        string name = "";
                        foreach (var asset in data.Data.Balances)
                        {
                            if (asset.Asset == "BNB")
                            {
                                continue;
                            }

                            name += asset.Asset;
                        }

                        string nameReverse = "";
                        foreach (var asset in data.Data.Balances.Reverse())
                        {
                            if (asset.Asset == "BNB")
                            {
                                continue;
                            }

                            nameReverse += asset.Asset;
                        }

                        bool match = false;

                        lock (Assets.IsolatedAssetLock)
                        {
                            foreach (var sym in Assets.IsolatedAssets)
                            {
                                if (name == sym.Symbol || nameReverse == sym.Symbol)
                                {
                                    match = true;
                                    var d = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.QuoteAsset.Asset);
                                    if (d != null)
                                    {
                                        sym.QuoteAsset.Available = d.Free;
                                        sym.QuoteAsset.Locked = d.Locked;
                                        sym.QuoteAsset.Total = d.Total;
                                        WriteLog.Info("Isolated AccountUpdate was processed for " + d.Asset);
                                    }

                                    var f = data.Data.Balances.SingleOrDefault(o => o.Asset == sym.BaseAsset.Asset);
                                    if (f != null)
                                    {
                                        sym.BaseAsset.Available = f.Free;
                                        sym.BaseAsset.Locked = f.Locked;
                                        sym.BaseAsset.Total = f.Total;
                                        WriteLog.Info("Isolated AccountUpdate was processed for " + f.Asset);
                                    }
                                }
                            }
                        }

                        if (!match)
                        {
                            WriteLog.Info("Couldn't find a match for the Isolated AccountUpdate");
                            _ = Account.UpdateIsolatedInformationAsync();
                        }
                    }
                }
                catch
                {
                    WriteLog.Error("There was an error processing OnAccountUpdateIsolated, Event Time for Reference: " + data.Data.EventTime);
                }
            }
        }

        public static void OnOrderUpdateSpot(DataEvent<BinanceStreamOrderUpdate> data)
        {
            OnOrderUpdateDigest(data, TradingMode.Spot);
        }

        public static void OnOrderUpdateMargin(DataEvent<BinanceStreamOrderUpdate> data)
        {
            OnOrderUpdateDigest(data, TradingMode.Margin);
        }

        public static void OnOrderUpdateIsolated(DataEvent<BinanceStreamOrderUpdate> data)
        {
            OnOrderUpdateDigest(data, TradingMode.Isolated);
        }

        /// <summary>
        /// Updates order list when notification is recieved from the server
        /// <para>Most orders will have 2 Message Parts at Least</para>
        /// If you change this keep in mind both messages might arrive at the same time or before one another
        /// </summary>
        /// <param name="data">The data from the Order Update</param>
        public static void OnOrderUpdateDigest(DataEvent<BinanceStreamOrderUpdate> data, TradingMode tradingMode)
        {
            var d = data.Data;
            try
            {
                if (d.Event == "executionReport")
                {
                    Orders.AddNewOrderUpdateEventsToQueue(data.Data, tradingMode);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
                WriteLog.Info("OnOrderUpdate: Error Updating Order: " + d.OrderId + " | ET: " + d.ExecutionType + " | OS: " + d.Status + " | EV: " + d.Event);
            }
        }

        /// <summary>
        /// Low Resolution update for every coin (1 second)
        /// </summary>
        /// <returns></returns>
        public static async Task SubscribeToAllSymbolTickerUpdatesAsync()
        {
            // All Prices
            var allpricesResult = await Client.SocketSymbolTicker.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {
                WatchMan.AllPricesTicker.SetWorking();
                try
                {
                    var exchangeInfo = Static.ManageExchangeInfo.GetStoredExchangeInfo;
                    if (exchangeInfo != null)
                    {
                        var pricesFilter = Static.AllPrices.ToList();
                        foreach (var ud in data.Data)
                        {
                            var symbol = pricesFilter.SingleOrDefault(p => p.SymbolView.Symbol == ud.Symbol);
                            if (symbol != null)
                            {
                                symbol.SymbolView = new BinanceStreamTick
                                {
                                    Symbol = ud.Symbol,
                                    LastPrice = ud.LastPrice.Normalize(),
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
                    else
                    {
                        WriteLog.Error("Exchange Information was missing when subscribing to tickers");
                    }
                }
                catch (Exception ex)
                {
                    WatchMan.AllPricesTicker.SetError();
                    WriteLog.Error(ex);
                }
            }).ConfigureAwait(false);

            if (!allpricesResult.Success)
            {
                WatchMan.ExceptionWhileStarting.SetError();
                WatchMan.AllPricesTicker.SetError();
                WriteLog.Error("Exception: " + allpricesResult.Error!.Message);
            }

            // Watchlist
            var watchlistResult = await Client.SocketWatchlistTicker.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data =>
            {
                WatchMan.WatchlistAllPricesTicker.SetWorking();
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
                                symbolwl.WatchlistVolume = ud.BaseVolume.Normalize();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WatchMan.WatchlistAllPricesTicker.SetError();
                    WriteLog.Error(ex);
                }
            }).ConfigureAwait(false);

            if (!watchlistResult.Success)
            {
                WatchMan.ExceptionWhileStarting.SetError();
                WatchMan.WatchlistAllPricesTicker.SetError();
                WriteLog.Error("Exception: " + watchlistResult.Error!.Message);
            }
        }

        public static void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            Static.RealTimeUpdate.BestAskPrice = e.BestAsk.Normalize();
            Static.RealTimeUpdate.BestAskQuantity = e.BestAskQuantity;
            Static.RealTimeUpdate.BestBidPrice = e.BestBid.Normalize();
            Static.RealTimeUpdate.BestBidQuantity = e.BestBidQuantity;
        }

        private static Ticker? currentSymbol;

        public static Task CurrentSymbolTickerAsync()
        {
            if (currentSymbol != null)
            {
                currentSymbol.TickerUpdated -= TickerUpdated;
            }

            currentSymbol = Tickers.AddTicker(Static.SelectedSymbolViewModel!.SymbolView.Symbol);

            currentSymbol.TickerUpdated += TickerUpdated;

            return Task.CompletedTask;
        }
    }
}
