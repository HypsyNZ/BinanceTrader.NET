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
using System;

namespace BTNET.BV.Base
{
    public class ServerTimeBase : ObservableObject
    {
        private DateTime serverTime;

        public DateTime Time
        { get => this.serverTime; set { this.serverTime = value; PC(); } }

        private long servertimeticks;

        public long ServerTimeTicks
        { get => this.servertimeticks; set { this.servertimeticks = value; PC(); } }

        public int? UsedWeight
        { get => this.usedweight; set { this.usedweight = value; PC(); } }

        private int? usedweight;
    }
}