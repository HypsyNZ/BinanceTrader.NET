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

using System;
using System.Globalization;
using System.Windows.Data;

namespace BTNET.BV.Converters
{
    public class NumericFieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal val = System.Convert.ToDecimal(value);
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string finalString = val.ToString("#,0.############", nfi);
            return finalString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBasic(object value)
        {
            decimal val = System.Convert.ToDecimal(value);
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string finalString = val.ToString("#,0.############", nfi);
            return finalString;
        }

        public decimal ConvertDecimal(decimal value, string mask)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ",";
            string finalString = value.ToString("#,0." + mask, nfi);
            bool d = decimal.TryParse(finalString, out decimal outD);

            if (d) return outD;
            return 0;
        }
    }
}