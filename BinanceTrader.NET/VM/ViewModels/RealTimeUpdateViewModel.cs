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

namespace BTNET.ViewModels
{
    public class RealTimeUpdateViewModel : ObservableObject
    {
        private decimal askprice, askquantity, bidprice, bidquantity;

        public decimal AskPrice
        { get => this.askprice; set { this.askprice = value; PC(); } }

        public decimal AskQuantity
        { get => this.askquantity; set { this.askquantity = value; PC(); } }

        public decimal BidPrice
        { get => this.bidprice; set { this.bidprice = value; PC(); } }

        public decimal BidQuantity
        { get => this.bidquantity; set { this.bidquantity = value; PC(); } }
    }
}