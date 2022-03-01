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
using BTNET.Base;
using System.Collections.ObjectModel;

namespace BTNET.BVVM
{
    public class Stored : ObservableObject
    {
        public static readonly string storedExchangeInfo = Static.SettingsPath + @"ExchangeInfoCopy.json";
        public static readonly string storedSpotOrders = Static.SystemDrive + @"BNET\Orders\SpotOrders.json";
        public static readonly string storedMarginOrders = Static.SystemDrive + @"BNET\Orders\MarginOrders.json";
        public static readonly string storedIsolatedOrders = Static.SystemDrive + @"BNET\Orders\IsolatedOrders.json";

        public static BinanceExchangeInfo ExchangeInfo { get; set; }

        public static ObservableCollection<OrderBase> ToS { get; set; }
        public static ObservableCollection<OrderBase> ToM { get; set; }
        public static ObservableCollection<OrderBase> ToI { get; set; }
        public static ObservableCollection<OrderBase> TempOrders { get; set; } = null;
    }
}