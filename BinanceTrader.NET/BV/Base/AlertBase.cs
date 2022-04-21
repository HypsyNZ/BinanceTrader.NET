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
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;

// todo: Check Slow Price of "other" symbols
// todo: Add "Retrigger Price"
// todo: Make "Order Builder" so Alerts can have Actions
namespace BTNET.BV.Base
{
    public class AlertItem : ObservableObject
    {
        private decimal alertPrice = 0;
        private bool alertHasAction = false;
        private bool alertHasSound = false;
        private bool alertRepeats = false;

        private bool alertReverseBeforeRepeat = true;
        private decimal alertRepeatInterval = 9999;

        private Intent alertIntent = Intent.None;
        private Direction alertDirection = Direction.Down;

        private Task alerttask = null;
        private Action action = null;

        private long alertLastTriggered = 0;
        private bool alertHasBeenTriggered = false;
        private string alertsymbol;

        public string AlertSymbol
        { get => alertsymbol; set { alertsymbol = value; PC(); } }

        public Intent AlertIntent { get => alertIntent; set => alertIntent = value; }
        public Direction AlertDirection { get => alertDirection; set => alertDirection = value; }

        /// <summary>
        /// Whether the Alert was Triggered or not, This is the last thing to return from the Alert, But the Task may still be running.
        /// </summary>
        private bool AlertTriggered
        { get => alertHasBeenTriggered; set { alertHasBeenTriggered = value; PC(); } }

        /// <summary>
        /// The time in Ms when the event was last triggered  (Binance server time)
        /// </summary>
        private long LastTriggered
        { get => alertLastTriggered; set { alertLastTriggered = value; PC(); } }

        /// <summary>
        /// The whole "Task" that was assosiated with this alert
        /// </summary>
        public Task AlertTask
        { get => alerttask; set { alerttask = value; PC(); } }

        /// <summary>
        /// The Action to take when the Alert is triggered
        /// </summary>
        public Action AlertAction
        { get => action; set { action = value; PC(); } }

        /// <summary>
        /// Whether or not to start a new Task and attempt the AlertAction
        /// </summary>
        public bool AlertHasAction
        { get => alertHasAction; set { alertHasAction = value; PC(); } }

        /// <summary>
        /// Whether or not the AlertAction should repeat
        /// </summary>
        public bool AlertRepeats
        { get => alertRepeats; set { alertRepeats = value; PC(); } }

        /// <summary>
        /// If this is enabled the price has to go back below or above AlertPrice for the alert to be active again
        /// </summary>
        public bool ReverseBeforeRepeat
        { get => alertReverseBeforeRepeat; set { alertReverseBeforeRepeat = value; PC(); } }

        /// <summary>
        /// Whether or not the Alert should make a sound
        /// </summary>
        public bool AlertHasSound
        { get => alertHasSound; set { alertHasSound = value; PC(); } }

        /// <summary>
        /// The Alert Price
        /// If your Intent is to sell, This will be compared to the Best Bid Price Available
        /// If your Intent is to buy, This will be compared to the Best Ask Price Available
        /// If Intent is None, This will be compared to the "Price"
        /// </summary>
        public decimal AlertPrice
        { get => alertPrice; set { alertPrice = value; PC(); } }

        /// <summary>
        /// How often to repeat the AlertAction in milliseconds
        /// /// </summary>
        public decimal RepeatInterval
        { get => alertRepeatInterval; set { alertRepeatInterval = value; PC(); } }

        /// <summary>
        /// Checks if an Alert should fire and returns true when it fires,
        /// This should be looped or connected to a timer of some sort, It won't automatically fire.
        /// </summary>
        /// <param name="b">Binance Symbol View Model</param>
        /// <returns>True when the Alert has been Triggered, This doesn't mean the Task is completed.</returns>
        public void CheckAlert()
        {
            // Wrong symbol, Skip everything
            if (Static.GetCurrentlySelectedSymbol.SymbolView.Symbol != AlertSymbol) { return; }

            if (!AlertTriggered)
            {
                switch (AlertDirection)
                {
                    case Direction.Up:
                        if (Static.RTUB.BestBidPrice >= AlertPrice)
                        {
                            RunAlert();
                        }
                        break;

                    case Direction.Down:
                        if (Static.RTUB.BestAskPrice <= AlertPrice)
                        {
                            RunAlert();
                        }
                        break;
                }
            }
            else if (AlertRepeats)
            {
                if (!ReverseBeforeRepeat) { CheckIntervalRunAlert(); return; }

                ShouldReverseBeforeRun();
            }
        }

        /// <summary>
        /// This means AlertAction Task has completed, This doesn't mean the alert has returned.
        /// </summary>
        public bool CheckAlertTaskCompleted()
        {
            return AlertTask.IsCompleted;
        }

        /// <summary>
        /// Should the Alert be triggered
        /// </summary>
        /// <returns></returns>
        private void ShouldReverseBeforeRun()
        {
            // Check if Price has reversed
            switch (AlertDirection)
            {
                case Direction.Up: if (AlertTriggered && Static.RTUB.BestBidPrice < AlertPrice) { AlertTriggered = false; } break;
                case Direction.Down: if (AlertTriggered && Static.RTUB.BestAskPrice > AlertPrice) { AlertTriggered = false; } break;
            }

            // Not Reversed Yet
            if (AlertTriggered) { return; }

            CheckIntervalRunAlert();
        }

        /// <summary>
        /// Checks the Interval since the LastTriggered Time and Runs the Alert if it has Elapsed
        /// </summary>
        private void CheckIntervalRunAlert()
        {
            if ((LastTriggered + (RepeatInterval * 10000)) - DateTime.Now.Ticks <= 0)
            {
                RunAlert();
            }
        }

        /// <summary>
        /// The Alert
        /// </summary>
        private void RunAlert()
        {
            AlertTriggered = true;
            LastTriggered = DateTime.Now.Ticks;

            _ = Task.Run(() =>
            {
                if (AlertHasSound)
                {
                    Sound.PlaySound();
                }

                //if (AlertHasAction && AlertAction != null)
                //{
                //    // Do literally any task when conditions are met
                //AlertTask = Task.Run(() => AlertAction);
                //}

                WriteLog.Alert("[x_o] Alert Price: " + AlertPrice + "| Alert Symbol: " + AlertSymbol + " | Repeat Interval:" + RepeatInterval);
            });

            return;
        }

        public AlertItem(decimal alertPrice,
            string alertSymbol,
            bool makeSound,
            bool alertRepeats,
            decimal repeatInterval,
            bool reverseFirst,
            Intent intent,
            Direction direction,
            bool alertHasAction,
            Action action)
        {
            this.AlertPrice = alertPrice;
            this.AlertSymbol = alertSymbol;
            this.AlertHasSound = makeSound;
            this.AlertRepeats = alertRepeats;
            this.RepeatInterval = repeatInterval;
            this.ReverseBeforeRepeat = reverseFirst;
            this.AlertIntent = intent;
            this.AlertDirection = direction;
            this.AlertHasAction = alertHasAction;
            this.AlertAction = action;
        }
    }
}