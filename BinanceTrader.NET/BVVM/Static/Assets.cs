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

using BinanceAPI.Objects.Spot.IsolatedMarginData;
using BinanceAPI.Objects.Spot.MarginData;
using BinanceAPI.Objects.Spot.SpotData;
using System.Collections.ObjectModel;

namespace BTNET.BVVM
{
    internal class Assets
    {
        public static ObservableCollection<BinanceBalance> SpotAssets { get; set; } = new();
        public static ObservableCollection<BinanceMarginBalance> MarginAssets { get; set; } = new();
        public static ObservableCollection<BinanceIsolatedMarginAccountSymbol> IsolatedAssets { get; set; } = new();
    }
}