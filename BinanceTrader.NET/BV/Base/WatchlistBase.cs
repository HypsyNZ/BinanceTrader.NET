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

using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.BT.Args;
using BTNET.BVVM.Log;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BTNET.BV.Base
{
    public class WatchlistItem : ObservableObject
    {
        private string? watchlistSymbol;
        private decimal watchlistPrice;
        private decimal watchListBid;
        private decimal watchlistAsk;
        private decimal watchlistHigh;
        private decimal watchlistLow;
        private decimal watchlistClose;
        private decimal watchlistChange;
        private decimal watchlistVolume;
        private int tickerStatus;

        public WatchlistItem(string watchlistSymbol)
        {
            WatchlistSymbol = watchlistSymbol;
        }

        public string? WatchlistSymbol
        {
            get => watchlistSymbol;
            set
            {
                watchlistSymbol = value;
                PC();
            }
        }

        public decimal WatchlistPrice
        {
            get => watchlistPrice;
            set
            {
                watchlistPrice = value;
                PC();
            }
        }

        public decimal WatchlistBidPrice
        {
            get => watchListBid;
            set
            {
                watchListBid = value;
                PC();
            }
        }

        public decimal WatchlistAskPrice
        {
            get => watchlistAsk;
            set
            {
                watchlistAsk = value;
                PC();
            }
        }

        public decimal WatchlistHigh
        {
            get => watchlistHigh;
            set
            {
                watchlistHigh = value;
                PC();
            }
        }

        public decimal WatchlistLow
        {
            get => watchlistLow;
            set
            {
                watchlistLow = value;
                PC();
            }
        }

        public decimal WatchlistClose
        {
            get => watchlistClose;
            set
            {
                watchlistClose = value;
                PC();
            }
        }

        public decimal WatchlistChange
        {
            get => watchlistChange;
            set
            {
                watchlistChange = value;
                PC();
            }
        }

        public decimal WatchlistVolume
        {
            get => watchlistVolume;
            set
            {
                watchlistVolume = value;
                PC();
            }
        }

        public int TickerStatus
        {
            get => tickerStatus;
            set
            {
                tickerStatus = value;
                PC();
                PC("Status");
            }
        }

        public ImageSource Status
        {
            get
            {
                switch (TickerStatus)
                {
                    case Ticker.CONNECTED:
                        return new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Connection/connection-status-connected.png"));

                    case Ticker.CONNECTING:
                        return new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Connection/connection-status-connecting.png"));

                    default:
                        return new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Connection/connection-status-disconnected.png"));
                }
            }
        }

        public void SubscribeWatchListItemSocket()
        {
            var ticker = Tickers.AddTicker(WatchlistSymbol!);

            if (ticker != null)
            {
                ticker.TickerUpdated += TickerUpdated;
                ticker.StatusChanged += TickerStatusChanged;
            }
            else
            {
                WriteLog.Error("Failed to subscribe: " + WatchlistSymbol!);
            }
        }

        public void TickerUpdated(object sender, TickerResultEventArgs e)
        {
            WatchlistAskPrice = e.BestAsk;
            WatchlistBidPrice = e.BestBid;
        }

        public void TickerStatusChanged(object sender, StatusChangedEventArgs e)
        {
            TickerStatus = e.TickerStatus;
        }
    }
}
