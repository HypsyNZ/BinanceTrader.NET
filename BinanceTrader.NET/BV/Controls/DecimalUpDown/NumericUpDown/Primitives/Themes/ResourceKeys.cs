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

using System.Windows;

namespace BTNET.Toolkit.Themes
{
    public static class ResourceKeys
    {
        #region Brush Keys

        public static readonly ComponentResourceKey ControlNormalBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlNormalBackgroundKey");
        public static readonly ComponentResourceKey ControlDisabledBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlDisabledBackgroundKey");
        public static readonly ComponentResourceKey ControlNormalBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlNormalBorderKey");
        public static readonly ComponentResourceKey ControlMouseOverBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlMouseOverBorderKey");
        public static readonly ComponentResourceKey ControlSelectedBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlSelectedBorderKey");
        public static readonly ComponentResourceKey ControlFocusedBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ControlFocusedBorderKey");

        public static readonly ComponentResourceKey ButtonNormalOuterBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonNormalOuterBorderKey");
        public static readonly ComponentResourceKey ButtonNormalInnerBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonNormalInnerBorderKey");
        public static readonly ComponentResourceKey ButtonNormalBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonNormalBackgroundKey");

        public static readonly ComponentResourceKey ButtonMouseOverBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonMouseOverBackgroundKey");
        public static readonly ComponentResourceKey ButtonMouseOverOuterBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonMouseOverOuterBorderKey");
        public static readonly ComponentResourceKey ButtonMouseOverInnerBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonMouseOverInnerBorderKey");

        public static readonly ComponentResourceKey ButtonPressedOuterBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonPressedOuterBorderKey");
        public static readonly ComponentResourceKey ButtonPressedInnerBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonPressedInnerBorderKey");
        public static readonly ComponentResourceKey ButtonPressedBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonPressedBackgroundKey");

        public static readonly ComponentResourceKey ButtonFocusedOuterBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonFocusedOuterBorderKey");
        public static readonly ComponentResourceKey ButtonFocusedInnerBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonFocusedInnerBorderKey");
        public static readonly ComponentResourceKey ButtonFocusedBackgroundKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonFocusedBackgroundKey");

        public static readonly ComponentResourceKey ButtonDisabledOuterBorderKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonDisabledOuterBorderKey");
        public static readonly ComponentResourceKey ButtonInnerBorderDisabledKey = new ComponentResourceKey(typeof(ResourceKeys), "ButtonInnerBorderDisabledKey");

        #endregion Brush Keys

        public static readonly ComponentResourceKey SpinButtonCornerRadiusKey = new ComponentResourceKey(typeof(ResourceKeys), "SpinButtonCornerRadiusKey");

        public static readonly ComponentResourceKey SpinnerButtonStyleKey = new ComponentResourceKey(typeof(ResourceKeys), "SpinnerButtonStyleKey");
    }
}
