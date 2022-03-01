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

using BinanceAPI.Objects.Spot.MarketData;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BVVM;

namespace BTNET.ViewModels
{
    public class BinanceSymbolViewModel : ObservableObject
    {
        private BinanceTradeFee tradeFee = null;
        private Binance24HPrice symbolView = new();
        private decimal interestRate, one, two;

        public Binance24HPrice SymbolView
        {
            get => symbolView; set
            {
                symbolView = value; PC();
                One = SymbolView.LastPrice;
                Two = SymbolView.LastPrice;
            }
        }

        public BinanceTradeFee TradeFee
        { get => tradeFee; set { this.tradeFee = value; PC(); } }

        public decimal InterestRate
        { get => this.interestRate; set { this.interestRate = value; PC(); } }

        public decimal One
        {
            get => this.one / 10; set
            {
                this.one = value; PC();
            }
        }

        public decimal Two
        {
            get => this.two / 10 * 2; set
            {
                this.two = value; PC();
            }
        }

        public BinanceSymbolViewModel(string symbol, decimal price)
        {
            SymbolView.Symbol = symbol;
            SymbolView.LastPrice = price;
            InterestRate = interestRate;
            TradeFee = tradeFee;
            One = one;
            Two = two;
        }
    }
}