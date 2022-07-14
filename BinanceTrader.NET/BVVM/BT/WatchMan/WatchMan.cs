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

using LoopDelay.NET;
using System;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    public static class WatchMan
    {
        private const int STARTUP_CHECK_DELAY_MS = 50;
        private const int STARTUP_MAX_TIME_MS = 30000;
        private const int STARTUP_EXPIRE_TIME_MULTI = 2;
        private const int STARTUP_EXPIRE_TIME = STARTUP_MAX_TIME_MS * STARTUP_EXPIRE_TIME_MULTI;

        public static State Load_InterestMargin = new();
        public static State Load_InterestIsolated = new();
        public static State Load_TradeFee = new();
        public static State Load_Alerts = new();
        public static State Load_Watchlist = new();
        public static State Load_Deletedlist = new();
        public static State Load_Browser = new();

        public static State UserStreams = new();
        public static State ExchangeInfo = new();
        public static State SearchPrices = new();

        public static State ExceptionWhileStarting = new();

        public static State AllPricesTicker = new();
        public static State WatchlistAllPricesTicker = new();

        public static State Task_One = new();
        public static State Task_Two = new();
        public static State Task_Three = new();
        public static State Task_Four = new();

        public static bool LoadCompleted()
        {
            if (!Load_InterestMargin.IsCompleted())
            {
                if (!Load_InterestIsolated.IsWaiting())
                {
                    return false;
                }
            }

            if (!Load_InterestIsolated.IsCompleted())
            {
                if (!Load_InterestIsolated.IsWaiting())
                {
                    return false;
                }
            }

            if (!Load_TradeFee.IsCompleted())
            {
                if (!Load_TradeFee.IsWaiting())
                {
                    return false;
                }
            }

            if (!Load_Alerts.IsCompleted())
            {
                return false;
            }

            if (!Load_Watchlist.IsCompleted())
            {
                return false;
            }

            if (!Load_Browser.IsCompleted())
            {
                return false;
            }

            if (!Load_Deletedlist.IsCompleted())
            {
                return false;
            }

            if (!Task_One.IsCompleted())
            {
                return false;
            }

            if (!Task_Two.IsCompleted())
            {
                return false;
            }

            if (!Task_Three.IsCompleted())
            {
                return false;
            }

            if (!Task_Four.IsCompleted())
            {
                return false;
            }

            if (!UserStreams.IsWorking())
            {
                if (!UserStreams.IsWaiting())
                {
                    return false;
                }
            }

            if (!ExchangeInfo.IsWorking())
            {
                return false;
            }

            if (!SearchPrices.IsWorking())
            {
                return false;
            }

            if (ExceptionWhileStarting.IsError())
            {
                return false;
            }

            return true;
        }

        public static bool RunningWhileExceptional()
        {
            if (Load_InterestMargin.IsError())
            {
                return true;
            }

            if (Load_InterestIsolated.IsError())
            {
                return true;
            }

            if (Load_TradeFee.IsError())
            {
                return true;
            }

            if (Load_Alerts.IsError())
            {
                return true;
            }

            if (Load_Watchlist.IsError())
            {
                return true;
            }

            if (Load_Browser.IsError())
            {
                return true;
            }

            if (Task_One.IsError())
            {
                return true;
            }

            if (Task_Two.IsError())
            {
                return true;
            }

            if (Task_Three.IsError())
            {
                return true;
            }

            if (!Task_Four.IsError())
            {
                return false;
            }

            if (UserStreams.IsError())
            {
                return true;
            }

            if (ExchangeInfo.IsError())
            {
                return true;
            }

            if (SearchPrices.IsError())
            {
                return true;
            }

            if (WatchlistAllPricesTicker.IsError())
            {
                return true;
            }

            if (AllPricesTicker.IsError())
            {
                return true;
            }

            return false;
        }

        public static async Task StartUpMonitorAsync(DateTime startTime)
        {
            while (await Loop.Delay(startTime.Ticks, STARTUP_CHECK_DELAY_MS, STARTUP_EXPIRE_TIME, (() =>
            {
                Message.ShowBox("Failed to Start after [" + STARTUP_EXPIRE_TIME + "ms] and will now exit", "Please Restart", waitForReply: true, exit: true);
            })))
            {
                if (LoadCompleted())
                {
                    App.ApplicationStarted?.Invoke(null, null);
                    return;
                }

                if (startTime + TimeSpan.FromMilliseconds(STARTUP_MAX_TIME_MS) < DateTime.UtcNow)
                {
                    Message.ShowBox("Failed to Start within [" + STARTUP_MAX_TIME_MS + "ms] and will now exit", "Please Restart", waitForReply: true, exit: true);
                    return;
                }
            }
        }
    }
}
