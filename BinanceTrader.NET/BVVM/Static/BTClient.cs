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

using BinanceAPI.ClientHosts;
using System.Threading;

namespace BTNET.BVVM
{
    public class BTClient
    {
        public SocketClientHost SocketWatchlistTicker { get; set; }
        public SocketClientHost SocketSymbolTicker { get; set; }
        public SocketClientHost SocketClient { get; set; }
        public BinanceClientHost Local { get; set; }

        public BTClient(CancellationToken waitToken = default)
        {
            Local = new(waitToken);
            SocketWatchlistTicker = new();
            SocketSymbolTicker = new();
            SocketClient = new();
        }

        public void DisposeClients()
        {
            _ = SocketClient?.UnsubscribeAllAsync();
            _ = SocketSymbolTicker?.UnsubscribeAllAsync();
            _ = SocketWatchlistTicker?.UnsubscribeAllAsync();

            Local?.Dispose();
            SocketClient?.Dispose();
            SocketSymbolTicker?.Dispose();
            SocketWatchlistTicker?.Dispose();
        }
    }
}
