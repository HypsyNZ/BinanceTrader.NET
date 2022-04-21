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

using BTNET.BV.Enum;
using BTNET.BVVM;
using System.Threading.Tasks;

namespace BTNET.VM.ViewModels
{
    public class VisibilityViewModel : ObservableObject
    {
        private double interestWidth = 80;
        private double paddingWidth = 100;
        private double resetInterestWidth = 55;
        private bool hideSettleTab = false;

        public bool HideSettleTab
        { get => hideSettleTab; set { hideSettleTab = value; PC(); } }

        public double InterestWidth
        { get => interestWidth; set { interestWidth = value; PC(); } }

        public double PaddingWidth
        { get => paddingWidth; set { paddingWidth = value; PC(); } }

        public double ResetInterestWidth
        { get => resetInterestWidth; set { resetInterestWidth = value; PC(); } }

        public Task AdjustWidth(TradingMode currentMode)
        {
            PaddingWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            if (currentMode == TradingMode.Spot)
            {
                InterestWidth = 0;
                ResetInterestWidth = 0;
                return Task.CompletedTask;
            }

            InterestWidth = 80;
            ResetInterestWidth = 55;

            return Task.CompletedTask;
        }
    }
}