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

using BinanceAPI;
using BinanceAPI.Sockets;
using BTNET.BVVM;
using BTNET.BVVM.Log;
using System;

namespace BTNET.BV.Base
{
    public class WatchlistItem : ObservableObject
    {
        private string watchlistSymbol;

        private decimal watchlistPrice, watchListBid, watchlistAsk, watchlistHigh, watchlistLow, watchlistClose, watchlistChange, watchlistVolume;

        public string WatchlistSymbol
        { get => watchlistSymbol; set { watchlistSymbol = value; PC(); } }

        public decimal WatchlistPrice
        { get => watchlistPrice; set { watchlistPrice = value; PC(); } }

        public decimal WatchlistBidPrice
        { get => watchListBid; set { watchListBid = value; PC(); } }

        public decimal WatchlistAskPrice
        { get => watchlistAsk; set { watchlistAsk = value; PC(); } }

        public decimal WatchlistHigh
        { get => watchlistHigh; set { watchlistHigh = value; PC(); } }

        public decimal WatchlistLow
        { get => watchlistLow; set { watchlistLow = value; PC(); } }

        public decimal WatchlistClose
        { get => watchlistClose; set { watchlistClose = value; PC(); } }

        public decimal WatchlistChange
        { get => watchlistChange; set { watchlistChange = value; PC(); } }

        public decimal WatchlistVolume
        { get => watchlistVolume; set { watchlistVolume = value; PC(); } }

        /// <summary>
        /// The UpdateSubscription for this WatchlistItem
        /// </summary>
        private UpdateSubscription WatchlistSymbolTickerUpdateSubscription = null;

        /// <summary>
        /// Unsubscribes from the Real Time Websocket Updates for the WatchlistSymbol in this WatchlistItem
        /// </summary>
        public void UnsubscribeWatchListItemSocket()
        {
            BinanceSocketClient unsub = new BinanceSocketClient();
            if (this.WatchlistSymbolTickerUpdateSubscription != null)
            {
                var result = unsub.UnsubscribeAsync(this.WatchlistSymbolTickerUpdateSubscription);
            }
        }

        /// <summary>
        /// Subscribes to the Real Time Websocket Updates for the WatchlistSymbol in this WatchlistItem
        /// </summary>
        public void SubscribeWatchListItemSocket()
        {
            BinanceSocketClient socketClientTicker = new BinanceSocketClient();

            if (this.WatchlistSymbolTickerUpdateSubscription != null)
            {
                WriteLog.Info("WatchlistItem is already running");
                return;
            }

            var SymbolUpdateSubscription = socketClientTicker.Spot.SubscribeToBookTickerUpdatesAsync(WatchlistSymbol, data =>
            {
                try
                {
                    WatchlistAskPrice = data.Data.BestAskPrice;
                    WatchlistBidPrice = data.Data.BestBidPrice;
                }
                catch (Exception ex)
                {
                    WriteLog.Error(WatchlistSymbol + " | WatchlistItem: Real Time Symbol Ticker Exception: ", ex);
                }
            });

            this.WatchlistSymbolTickerUpdateSubscription = SymbolUpdateSubscription.Result.Data;
        }

        public WatchlistItem()
        { }
    }
}