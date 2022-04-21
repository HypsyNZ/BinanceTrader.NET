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

using BTNET.BVVM;

namespace BTNET.VM.ViewModels
{
    public class QuoteViewModel : ObservableObject
    {
        public QuoteViewModel()
        {
        }

        private decimal tradeAmountbuy; public decimal TradeAmountBuy
        {
            get => this.tradeAmountbuy;
            set
            {
                this.tradeAmountbuy = value; PC();
            }
        }

        private decimal tradeAmountsell; public decimal TradeAmountSell
        {
            get => this.tradeAmountsell;
            set
            {
                this.tradeAmountsell = value; PC();
            }
        }

        private decimal tradePrice; public decimal TradePrice
        { get => this.tradePrice; set { this.tradePrice = value; PC(); } }

        private decimal observeQuoteBuy; public decimal ObserveQuoteOrderQuantityLocalBuy
        {
            get => this.observeQuoteBuy;
            set
            {
                this.observeQuoteBuy = value; PC();
            }
        }

        private decimal observeQuoteSell; public decimal ObserveQuoteOrderQuantityLocalSell
        {
            get => this.observeQuoteSell;
            set
            {
                this.observeQuoteSell = value; PC();
            }
        }
    }
}