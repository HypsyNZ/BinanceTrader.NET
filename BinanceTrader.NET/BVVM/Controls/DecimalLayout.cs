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
using BTNET.Abstract;
using System.Globalization;
using System.Linq;

namespace BTNET.BVVM.Controls
{
    internal static class DecimalLayout
    {
        /// <summary>
        /// Convert Decimal to its Appropriate Representation using <see cref="BinanceSymbol"/> directly
        /// </summary>
        /// <param name="d">Decimal to Convert</param>
        /// <param name="binanceSymbol">Binance Symbol Info Object</param>
        /// <returns></returns>
        public static decimal Convert(decimal d, BinanceSymbol binanceSymbol)
        {
            DecimalHelper dd = TrimDecimal(binanceSymbol != null ? binanceSymbol.PriceFilter.MinPrice : 0);
            return CSats(d, dd);
        }

        /// <summary>
        /// Convert Decimal to its Appropriate Representation by accessing <see cref="BinanceSymbol"/> from <see cref="BinanceExchangeInfo"/>
        /// </summary>
        /// <param name="d">Decimal to convert</param>
        /// <param name="symbol">Current Symbol</param>
        /// <param name="BinanceExchangeInfo">Binance Exchange Info</param>
        /// <returns></returns>
        public static decimal Convert(decimal d, string symbol, BinanceExchangeInfo BinanceExchangeInfo)
        {
            var res = BinanceExchangeInfo.Symbols.SingleOrDefault(r => r.Name == symbol);
            DecimalHelper dd = TrimDecimal(res != null ? res.PriceFilter.MinPrice : 0);
            return CSats(d, dd);
        }

        /// <summary>
        /// Convert Decimal to its Appropriate Representation using Decimal Scale
        /// </summary>
        /// <param name="d">Decimal to Convert</param>
        /// <param name="dd"><see cref="DecimalHelper"/> Object</param>
        /// <returns>Appropriate Representation</returns>
        private static decimal CSats(decimal d, DecimalHelper dd)
        {
            double val = System.Convert.ToDouble(d);
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string finalString;

            int count = dd.Scale;
            switch (count)
            {
                case 1:
                    finalString = val.ToString("#,0.0###########", nfi); break;
                case 2:
                    finalString = val.ToString("#,0.00##########", nfi); break;
                case 3:
                    finalString = val.ToString("#,0.000#########", nfi); break;
                case 4:
                    finalString = val.ToString("#,0.0000########", nfi); break;
                case 5:
                    finalString = val.ToString("#,0.00000#######", nfi); break;
                case 6:
                    finalString = val.ToString("#,0.000000#######", nfi); break;
                case 7:
                    finalString = val.ToString("#,0.0000000#####", nfi); break;
                case 8:
                    finalString = val.ToString("#,0.00000000#####", nfi); break;
                case 9:
                    finalString = val.ToString("#,0.000000000####", nfi); break;
                case 10:
                    finalString = val.ToString("#,0.0000000000###", nfi); break;
                default:
                    finalString = val.ToString("#,0.0############", nfi); break;
            }

            bool f = decimal.TryParse(finalString, out decimal outD);
            return f ? outD : 0;
        }

        /// <summary>
        /// Trim the zeros from the end of a <seealso cref="decimal"/>
        /// </summary>
        /// <param name="value"><seealso cref="decimal"/> to trim</param>
        /// <returns></returns>
        public static decimal TrimDecimal(decimal value)
        {
            if (value != 0)
            {
                string text = value.ToString(CultureInfo.InvariantCulture).TrimEnd('0');

                bool convertback = decimal.TryParse(text, out decimal outD);

                return convertback ? outD : 0;
            }
            return 0;
        }
    }
}