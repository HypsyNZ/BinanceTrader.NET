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

// todo: Check Slow Price of "other" symbols
namespace BTNET.BV.Base
{
    public class AlertItem : ObservableObject
    {
        private AlertStatus alertStatus;

        public string AlertSymbol { get; set; }

        /// <summary>
        /// The Alert Direction
        /// </summary>
        public Direction AlertDirection { get; set; }

        /// <summary>
        /// Whether the Alert was Triggered or not, This is the last thing to return from the Alert, But the Task may still be running.
        /// </summary>
        public bool AlertTriggered { get; set; }

        /// <summary>
        /// The time in Ms when the event was last triggered  (Binance server time)
        /// </summary>
        public long LastTriggered { get; set; }

        /// <summary>
        /// Whether or not the AlertAction should repeat
        /// </summary>
        public bool AlertRepeats { get; set; }

        /// <summary>
        /// If this is enabled the price has to go back below or above AlertPrice for the alert to be active again
        /// </summary>
        public bool ReverseBeforeRepeat { get; set; } = true;

        /// <summary>
        /// Whether or not the Alert should make a sound
        /// </summary>
        public bool AlertHasSound { get; set; }

        /// <summary>
        /// The Alert Price
        /// If your Intent is to sell, This will be compared to the Best Bid Price Available
        /// If your Intent is to buy, This will be compared to the Best Ask Price Available
        /// If Intent is None, This will be compared to the "Price"
        /// </summary>
        public decimal AlertPrice { get; set; }

        /// <summary>
        /// The Current Status of the Alert
        /// </summary>
        public AlertStatus AlertStatus
        {
            get => alertStatus; set
            {
                alertStatus = value;
                PC();
            }
        }

        /// <summary>
        /// How often to repeat the AlertAction in milliseconds
        /// /// </summary>
        public int RepeatInterval { get; set; } = 9999;

        public AlertItem(decimal alertPrice,
            string alertSymbol,
            bool makeSound,
            bool alertRepeats,
            int repeatInterval,
            bool reverseFirst,
            bool triggered,
            long lastTriggered,
            Direction direction = Direction.Down,
            AlertStatus status = AlertStatus.Inactive)
        {
            AlertPrice = alertPrice;
            AlertSymbol = alertSymbol;
            AlertHasSound = makeSound;
            AlertRepeats = alertRepeats;
            RepeatInterval = repeatInterval;
            ReverseBeforeRepeat = reverseFirst;
            AlertDirection = direction;
            AlertTriggered = triggered;
            LastTriggered = lastTriggered;
            AlertStatus = status;
            LastTriggered = 0;
        }
    }
}
