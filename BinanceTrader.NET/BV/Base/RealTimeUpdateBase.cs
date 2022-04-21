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

namespace BTNET.BV.Base
{
    public class RealTimeUpdateBase : ObservableObject
    {
        private decimal bestAsk, bestAskQ, bestBid, bestBidQ;

        public decimal BestAskPrice
        { get => this.bestAsk; set { this.bestAsk = value; PC(); } }

        public decimal BestAskQuantity
        { get => this.bestAskQ; set { this.bestAskQ = value; PC(); } }

        public decimal BestBidPrice
        { get => this.bestBid; set { this.bestBid = value; PC(); } }

        public decimal BestBidQuantity
        { get => this.bestBidQ; set { this.bestBidQ = value; PC(); } }
    }
}