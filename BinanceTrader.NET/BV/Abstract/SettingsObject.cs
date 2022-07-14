/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

namespace BTNET.BV.Abstract
{
    internal class SettingsObject
    {
        public SettingsObject(bool? showBorrowInfoIsChecked, bool? showSymbolInfoIsChecked, bool? showBreakDownInfoIsChecked,
            bool? showMarginInfoIsChecked, bool? showIsolatedInfoIsChecked, double? orderOpacity, bool? stretchBrowserIsChecked, bool? checkForUpdates,
            bool? sellBaseChecked, bool? sellLimitChecked, bool? sellBorrowChecked, bool? buyBaseChecked, bool? buyBorrowChecked, bool? buyLimitChecked)
        {
            ShowBorrowInfoIsChecked = showBorrowInfoIsChecked;
            ShowSymbolInfoIsChecked = showSymbolInfoIsChecked;
            ShowBreakDownInfoIsChecked = showBreakDownInfoIsChecked;
            ShowMarginInfoIsChecked = showMarginInfoIsChecked;
            ShowIsolatedInfoIsChecked = showIsolatedInfoIsChecked;
            OrderOpacity = orderOpacity;
            StretchBrowserIsChecked = stretchBrowserIsChecked;
            CheckForUpdates = checkForUpdates;

            SellBaseChecked = sellBaseChecked;
            BuyBaseChecked = buyBaseChecked;
            SellLimitChecked = sellLimitChecked;
            BuyLimitChecked = buyLimitChecked;
            SellBorrowChecked = sellBorrowChecked;
            BuyBorrowChecked = buyBorrowChecked;
        }

        public bool? ShowBorrowInfoIsChecked { get; set; }
        public bool? ShowSymbolInfoIsChecked { get; set; }
        public bool? ShowBreakDownInfoIsChecked { get; set; }
        public bool? ShowMarginInfoIsChecked { get; set; }
        public bool? ShowIsolatedInfoIsChecked { get; set; }
        public double? OrderOpacity { get; set; }
        public bool? StretchBrowserIsChecked { get; set; }
        public bool? CheckForUpdates { get; set; }

        public bool? SellBaseChecked { get; set; }
        public bool? SellLimitChecked { get; set; }
        public bool? SellBorrowChecked { get; set; }

        public bool? BuyBaseChecked { get; set; }
        public bool? BuyLimitChecked { get; set; }
        public bool? BuyBorrowChecked { get; set; }
    }
}
