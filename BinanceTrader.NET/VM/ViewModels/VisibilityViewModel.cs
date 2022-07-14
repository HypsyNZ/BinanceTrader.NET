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

using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class VisibilityViewModel : ObservableObject
    {
        private const int INTEREST_WIDTH_DEFAULT = 105;
        private const int ORDER_SETTINGS_WIDTH_DEFAULT = 67;

        private double interestWidth = INTEREST_WIDTH_DEFAULT;
        private double paddingWidth = 100;
        private bool hideSettleTab;

        private double orderSettingsWidthFrom = 0;
        private double orderSettingsWidthTo = ORDER_SETTINGS_WIDTH_DEFAULT;
        private bool orderSettingsVisibility = true;

        public ICommand? OrderSettingsCommand { get; set; }

        public void InitializeCommands()
        {
            OrderSettingsCommand = new DelegateCommand(OrderSettingsToggle);
        }

        public bool HideSettleTab
        {
            get => hideSettleTab;
            set
            {
                hideSettleTab = value;
                PC();
            }
        }

        public double InterestWidth
        {
            get => interestWidth;
            set
            {
                interestWidth = value;
                PC();
            }
        }

        public double PaddingWidth
        {
            get => paddingWidth;
            set
            {
                paddingWidth = value;
                PC();
            }
        }

        public double OrderSettingsWidthFrom
        {
            get => orderSettingsWidthFrom;
            set
            {
                orderSettingsWidthFrom = value;
                PC();
            }
        }

        public double OrderSettingsWidthTo
        {
            get => orderSettingsWidthTo;
            set
            {
                orderSettingsWidthTo = value;
                PC();
            }
        }

        public bool OrderSettingsVisibility
        {
            get => orderSettingsVisibility;
            set
            {
                orderSettingsVisibility = value;
                PC();
            }
        }

        public void OrderSettingsOnTabChanged(object sender, EventArgs args)
        {
            if (Static.CurrentlySelectedSymbolTab == SelectedTab.Settle)
            {
                OrderSettingsVisibility = false;
                return;
            }

            OrderSettingsVisibility = true;
        }

        public void OrderSettingsToggle(object o)
        {
            OrderSettingsWidthFrom = (OrderSettingsWidthFrom == ORDER_SETTINGS_WIDTH_DEFAULT ? 0 : ORDER_SETTINGS_WIDTH_DEFAULT);
            OrderSettingsWidthTo = (OrderSettingsWidthTo == ORDER_SETTINGS_WIDTH_DEFAULT ? 0 : ORDER_SETTINGS_WIDTH_DEFAULT);
        }

        public Task AdjustWidthAsync(TradingMode currentMode)
        {
            PaddingWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            if (currentMode == TradingMode.Spot)
            {
                InterestWidth = 0;
                return Task.CompletedTask;
            }

            InterestWidth = INTEREST_WIDTH_DEFAULT;

            return Task.CompletedTask;
        }
    }
}
