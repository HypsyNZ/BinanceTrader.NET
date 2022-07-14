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

using BTNET.Toolkit.Primitives.Enum;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace BTNET.Toolkit
{
    public abstract class CommonNumericUpDown<T> : NumericUpDown<T?> where T : struct, IFormattable, IComparable<T>
    {
        private readonly FromText _fromText;
        private readonly FromDecimal _fromDecimal;
        private readonly Func<T, T, bool> _fromLowerThan;
        private readonly Func<T, T, bool> _fromGreaterThan;

        protected delegate bool FromText(string s, NumberStyles style, IFormatProvider provider, out T result);

        protected delegate T FromDecimal(decimal d);

        internal static readonly DependencyProperty IsInvalidProperty = DependencyProperty.Register("IsInvalid", typeof(bool), typeof(CommonNumericUpDown<T>), new UIPropertyMetadata(false));

        internal bool IsInvalid
        {
            get
            {
                return (bool)GetValue(IsInvalidProperty);
            }
            private set
            {
                SetValue(IsInvalidProperty, value);
            }
        }

        public static readonly DependencyProperty ParsingNumberStyleProperty = DependencyProperty.Register("ParsingNumberStyle", typeof(NumberStyles), typeof(CommonNumericUpDown<T>), new UIPropertyMetadata(NumberStyles.Any));

        public NumberStyles ParsingNumberStyle
        {
            get { return (NumberStyles)GetValue(ParsingNumberStyleProperty); }
            set { SetValue(ParsingNumberStyleProperty, value); }
        }

        protected CommonNumericUpDown(FromText fromText, FromDecimal fromDecimal, Func<T, T, bool> fromLowerThan, Func<T, T, bool> fromGreaterThan)
        {
            _fromText = fromText ?? throw new ArgumentNullException("tryParseMethod");
            _fromDecimal = fromDecimal ?? throw new ArgumentNullException("fromDecimal");
            _fromLowerThan = fromLowerThan ?? throw new ArgumentNullException("fromLowerThan");
            _fromGreaterThan = fromGreaterThan ?? throw new ArgumentNullException("fromGreaterThan");
        }

        protected override void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;

            if ((Increment != null) && !IsReadOnly)
            {
                if (IsLowerThan(Value, Maximum) || !Value.HasValue || !Maximum.HasValue)
                    validDirections |= ValidSpinDirections.Increase;

                if (IsGreaterThan(Value, Minimum) || !Value.HasValue || !Minimum.HasValue)
                    validDirections |= ValidSpinDirections.Decrease;
            }

            if (Spinner != null)
                Spinner.ValidSpinDirection = validDirections;
        }

        internal bool HandleNullSpin()
        {
            if (TextBox != null)
            {
                var hasValue = UpdateValueOnEnterKey
                               ? (ConvertTextToValue(TextBox.Text) != null)
                               : Value.HasValue;

                if (!hasValue)
                {
                    var forcedValue = DefaultValue ?? default;
                    var newValue = CoerceValueMinMax(forcedValue);

                    if (newValue.HasValue)
                    {
                        if (UpdateValueOnEnterKey)
                        {
                            TextBox.Text = newValue.Value.ToString(FormatString, CultureInfo);
                        }
                        else
                        {
                            Value = newValue;
                        }
                        return true;
                    }
                }
                else if (!Increment.HasValue)
                {
                    return true;
                }
            }

            return false;
        }

        internal static void UpdateMetadataCommon(Type type, T? increment, T? minValue, T? maxValue)
        {
            IncrementProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(increment));
            MaximumProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(maxValue));
            MinimumProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(minValue));
        }

        protected static void UpdateMetadata(Type type, T? increment, T? minValue, T? maxValue)
        {
            DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
            UpdateMetadataCommon(type, increment, minValue, maxValue);
        }

        protected override void OnIncrement()
        {
            if (!HandleNullSpin())
            {
                if (UpdateValueOnEnterKey)
                {
                    if (TextBox != null)
                    {
                        var currentValue = ConvertTextToValue(TextBox.Text);
                        if (currentValue.HasValue && Increment.HasValue)
                        {
                            var result = IncrementValue(currentValue.Value, Increment.Value);

                            var newValue = CoerceValueMinMax(result);
                            if (newValue.HasValue)
                            {
                                TextBox.Text = newValue.Value.ToString(FormatString, CultureInfo);
                            }
                        }
                    }
                }
                else
                {
                    if (Value.HasValue && Increment.HasValue)
                    {
                        var result = IncrementValue(Value.Value, Increment.Value);
                        Value = CoerceValueMinMax(result);
                    }
                }
            }
        }

        protected override void OnDecrement()
        {
            if (!HandleNullSpin())
            {
                if (UpdateValueOnEnterKey)
                {
                    if (TextBox != null)
                    {
                        var currentValue = ConvertTextToValue(TextBox.Text);
                        if (currentValue.HasValue && Increment.HasValue)
                        {
                            var result = DecrementValue(currentValue.Value, Increment.Value);
                            var newValue = CoerceValueMinMax(result);
                            if (newValue.HasValue)
                            {
                                TextBox.Text = newValue.Value.ToString(FormatString, CultureInfo);
                            }
                        }
                    }
                }
                else
                {
                    if (Value.HasValue && Increment.HasValue)
                    {
                        var result = DecrementValue(Value.Value, Increment.Value);
                        Value = CoerceValueMinMax(result);
                    }
                }
            }
        }

        protected override void OnMinimumChanged(T? oldValue, T? newValue)
        {
            base.OnMinimumChanged(oldValue, newValue);

            if (Value.HasValue)
            {
                Value = CoerceValueMinMax(Value.Value);
            }
        }

        protected override void OnMaximumChanged(T? oldValue, T? newValue)
        {
            base.OnMaximumChanged(oldValue, newValue);

            if (Value.HasValue)
            {
                Value = CoerceValueMinMax(Value.Value);
            }
        }

        protected override T? ConvertTextToValue(string text)
        {
            T? result = null;

            if (string.IsNullOrEmpty(text))
                return result;

            string currentValueText = ConvertValueToText();
            if (Equals(currentValueText, text))
            {
                IsInvalid = false;
                return Value;
            }

            result = ConvertTextToValueCore(currentValueText, text);

            return CoerceValueMinMax(result);
        }

        protected override string ConvertValueToText()
        {
            if (Value == null)
                return string.Empty;

            IsInvalid = false;

            if (FormatString.Contains("{0"))
                return string.Format(CultureInfo, FormatString, Value.Value);

            return Value.Value.ToString(FormatString, CultureInfo);
        }

        internal T? ConvertTextToValueCore(string currentValueText, string text)
        {
            T? result;

            if (IsPercent(FormatString))
            {
                result = _fromDecimal(ParsePercent(text, CultureInfo));
            }
            else
            {
                T outputValue = new T();
                if (!_fromText(text, ParsingNumberStyle, CultureInfo, out outputValue))
                {
                    if (!_fromText(currentValueText, ParsingNumberStyle, CultureInfo, out var currentValueTextOutputValue))
                    {
                        var currentValueTextSpecialCharacters = currentValueText.Where(c => !char.IsDigit(c));
                        if (currentValueTextSpecialCharacters.Count() > 0)
                        {
                            var textSpecialCharacters = text.Where(c => !char.IsDigit(c));
                            if (currentValueTextSpecialCharacters.Except(textSpecialCharacters).ToList().Count == 0)
                            {
                                foreach (var character in textSpecialCharacters)
                                {
                                    text = text.Replace(character.ToString(), string.Empty);
                                }

                                _fromText(text, ParsingNumberStyle, CultureInfo, out outputValue);
                            }
                        }
                    }
                }
                result = outputValue;
            }
            return result;
        }

        internal bool IsPercent(string stringToTest)
        {
            int PIndex = stringToTest.IndexOf("P");
            if (PIndex >= 0)
            {
                bool isText = (stringToTest.Substring(0, PIndex).Contains("'")
                              && stringToTest.Substring(PIndex, FormatString.Length - PIndex).Contains("'"));

                return !isText;
            }
            return false;
        }

        internal bool IsLowerThan(T? value1, T? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return _fromLowerThan(value1.Value, value2.Value);
        }

        internal bool IsGreaterThan(T? value1, T? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return _fromGreaterThan(value1.Value, value2.Value);
        }

        internal T? CoerceValueMinMax(T? value)
        {
            if (IsLowerThan(value, Minimum))
                return Minimum;
            else if (IsGreaterThan(value, Maximum))
                return Maximum;
            else
                return value;
        }

        protected abstract T IncrementValue(T value, T increment);

        protected abstract T DecrementValue(T value, T increment);
    }
}
