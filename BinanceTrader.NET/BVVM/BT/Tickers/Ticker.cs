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
using BinanceAPI.ClientHosts;
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Sockets;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.Log;
using System;
using System.Linq;
using System.Threading;

namespace BTNET.BVVM.BT
{
    public class Ticker
    {
        public const int CONNECTED = 1;
        public const int CONNECTING = 2;
        public const int DISCONNECTED = 3;

        private readonly SocketClientHost TickerSocket = new SocketClientHost();
        private CancellationTokenSource TickerCancellationToken;

        private BinanceSymbol? TickerSymbol { get; set; }
        private UpdateSubscription? TickerUpdateSubscription { get; set; }

        public TickerResultEventArgs TickerResult { get; set; }

        public StatusChangedEventArgs CurrentStatus { get; set; }

        /// <summary>
        /// Fires when updates for the Ticker are received
        /// <para>TickerUpdated += TickerUpdated</para>
        /// </summary>
        public EventHandler<TickerResultEventArgs>? TickerUpdated;

        /// <summary>
        /// Ticker Status Changed
        /// </summary>
        public EventHandler<StatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// The Symbol for the current Ticker
        /// </summary>
        public string Symbol { get; set; }

        public Ticker(string symbol)
        {
            TickerResult = new TickerResultEventArgs();
            CurrentStatus = new StatusChangedEventArgs(DISCONNECTED);
            TickerCancellationToken = new CancellationTokenSource();
            Symbol = symbol;
            StartTicker(symbol);
        }

        /// <summary>
        /// Stop the Ticker
        /// </summary>
        /// <returns>True if the Ticker Stopped</returns>
        public bool StopTicker()
        {
            if (TickerUpdateSubscription != null)
            {
                _ = TickerSocket.UnsubscribeAsync(TickerUpdateSubscription);
                TickerSymbol = null;
                TickerResult = new TickerResultEventArgs();
                CurrentStatus = new StatusChangedEventArgs(DISCONNECTED);
                TickerUpdateSubscription = null;
                Symbol = "";
                TickerCancellationToken.Cancel();
                return true;
            }

            return false;
        }

        private void StartTicker(string symbol)
        {
            TickerCancellationToken = new CancellationTokenSource();
            try
            {
                TickerResult = new TickerResultEventArgs();
                CurrentStatus = new StatusChangedEventArgs();
                TickerSymbol = Stored.ExchangeInfo?.Symbols.SingleOrDefault(r => r.Name == symbol);

                if (TickerUpdateSubscription != null)
                {
                    _ = TickerSocket.UnsubscribeAsync(TickerUpdateSubscription);
                    TickerUpdateSubscription.StatusChanged -= Subscription_StatusChanged;
                    TickerUpdateSubscription = null;
                }

                if (TickerSymbol != null)
                {
                    TickerUpdateSubscription = TickerSocket.Spot.SubscribeToBookTickerUpdatesAsync(symbol, data =>
                    {
                        try
                        {
                            TickerResult.BestAsk = data.Data.BestAskPrice.Normalize();
                            TickerResult.BestAskQuantity = data.Data.BestAskQuantity;
                            TickerResult.BestBid = data.Data.BestBidPrice.Normalize();
                            TickerResult.BestBidQuantity = data.Data.BestBidQuantity;
                            TickerUpdated?.Invoke(this, TickerResult);
                        }
                        catch
                        {
                            // Ignore
                        }
                    }).Result.Data;

                    TickerUpdateSubscription.StatusChanged += Subscription_StatusChanged;
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        private void Subscription_StatusChanged(ConnectionStatus obj)
        {
            if (obj == ConnectionStatus.Connected)
            {
                if (CurrentStatus.TickerStatus != CONNECTED)
                {
                    CurrentStatus.TickerStatus = CONNECTED;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
            else if (obj == ConnectionStatus.Connecting || obj == ConnectionStatus.Waiting)
            {
                if (CurrentStatus.TickerStatus != CONNECTING)
                {
                    CurrentStatus.TickerStatus = CONNECTING;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
            else
            {
                if (CurrentStatus.TickerStatus != DISCONNECTED)
                {
                    CurrentStatus.TickerStatus = DISCONNECTED;
                    StatusChanged?.Invoke(null, CurrentStatus);
                }
            }
        }
    }
}
