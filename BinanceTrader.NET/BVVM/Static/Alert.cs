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
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BV.Base
{
    public static class Alert
    {
        private const int ALERT_TICKER_ADD_DELAY = 200;

        private static Collection<Ticker> TickerList = new Collection<Ticker>();

        public static void CheckAlertItem(AlertItem alert)
        {
            if (alert.AlertPrice == 0)
            {
                alert.AlertStatus = AlertStatus.Inactive;
                return;
            }

            Ticker? f = Tickers.SymbolTickers.Where(t => t.Symbol == alert.AlertSymbol).FirstOrDefault();
            if (f != null)
            {
                if (f.TickerResult == null)
                {
                    alert.AlertStatus = AlertStatus.Inactive;
                    return;
                }

                if (f.TickerResult.BestBid == 0 || f.TickerResult.BestAsk == 0)
                {
                    alert.AlertStatus = AlertStatus.Inactive;
                    return;
                }

                if (!alert.AlertTriggered)
                {
                    alert.AlertStatus = AlertStatus.Active;
                    switch (alert.AlertDirection)
                    {
                        case Direction.Up:
                            if (f.TickerResult?.BestBid >= alert.AlertPrice)
                            {
                                RunAlert(alert);
                            }
                            return;

                        case Direction.Down:
                            if (f.TickerResult?.BestAsk <= alert.AlertPrice)
                            {
                                RunAlert(alert);
                            }
                            return;

                        default: return;
                    }
                }
                else if (alert.AlertRepeats)
                {
                    alert.AlertStatus = AlertStatus.Repeating;

                    if (!alert.ReverseBeforeRepeat)
                    {
                        CheckIntervalRunAlert(alert);
                        return;
                    }

                    ShouldReverseBeforeRun(alert, f!);
                    return;
                }

                alert.AlertStatus = AlertStatus.Triggered;
                return;
            }

            alert.AlertStatus = AlertStatus.Inactive;
        }

        public static async Task StartAlertSymbolTickersAsync()
        {
            foreach (var item in ObservableObject.AlertVM.Alerts)
            {
                await Task.Delay(ALERT_TICKER_ADD_DELAY).ConfigureAwait(false);
                Tickers.AddTicker(item.AlertSymbol);
            }
        }

        private static void ShouldReverseBeforeRun(AlertItem alert, Ticker t)
        {
            alert.AlertStatus = AlertStatus.Waiting;
            // Check if Price has reversed
            switch (alert.AlertDirection)
            {
                case Direction.Up:
                    if (alert.AlertTriggered && t.TickerResult!.BestBid < alert.AlertPrice)
                    {
                        alert.AlertTriggered = false;
                    }
                    break;

                case Direction.Down:
                    if (alert.AlertTriggered && t.TickerResult!.BestAsk > alert.AlertPrice)
                    {
                        alert.AlertTriggered = false;
                    }
                    break;

                default: return;
            }

            if (alert.AlertTriggered)
            {
                // Not Reversed Yet
                return;
            }

            CheckIntervalRunAlert(alert);
        }

        private static void CheckIntervalRunAlert(AlertItem alert)
        {
            if ((alert.LastTriggered + (alert.RepeatInterval * App.TEN_THOUSAND_TICKS)) - DateTime.Now.Ticks <= 0)
            {
                RunAlert(alert);
            }
        }

        private static void RunAlert(AlertItem alert)
        {
            alert.AlertStatus = AlertStatus.Running;
            alert.AlertTriggered = true;
            alert.LastTriggered = DateTime.Now.Ticks;

            _ = Task.Run(() =>
            {
                if (alert.AlertHasSound)
                {
                    Sound.PlaySound();
                }

                WriteLog.Alert("[x_o] Alert Price: " + alert.AlertPrice + "| Alert Symbol: " + alert.AlertSymbol + " | Repeat Interval:" + alert.RepeatInterval);
            });
        }
    }
}
