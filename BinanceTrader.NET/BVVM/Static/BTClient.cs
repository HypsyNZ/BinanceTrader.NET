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

namespace BTNET.BVVM
{
    internal class BTClient : ObservableObject
    {
        private static BinanceClient local = new();
        private static BinanceSocketClient socketClient = new();
        private static BinanceSocketClient socketSymbolTicker = new();

        public static BinanceSocketClient SocketSymbolTicker { get => socketSymbolTicker; set => socketSymbolTicker = value; }
        public static BinanceSocketClient SocketClient { get => socketClient; set => socketClient = value; }
        public static BinanceClient Local { get => local; set => local = value; }

        public static void DisposeClients()
        {
            if (SocketClient != null)
            {
                _ = SocketClient.UnsubscribeAllAsync();
            }

            if (SocketSymbolTicker != null)
            {
                _ = SocketSymbolTicker.UnsubscribeAllAsync();
            }

            if (Local != null)
            {
                Local.Dispose();
            }
        }
    }
}