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
using System.Windows;
using System.Windows.Media.Animation;

namespace BTNET.Converters
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register(
                "To", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(
            object defaultOriginValue, object defaultDestinationValue,
            AnimationClock animationClock)
        {
            var from = (GridLength)defaultOriginValue;

            if (from.GridUnitType != To.GridUnitType ||
                !animationClock.CurrentProgress.HasValue)
            {
                return from;
            }

            double p = animationClock.CurrentProgress.Value;

            return new GridLength(((1d - p) * from.Value) + (p * To.Value), from.GridUnitType);
        }
    }
}