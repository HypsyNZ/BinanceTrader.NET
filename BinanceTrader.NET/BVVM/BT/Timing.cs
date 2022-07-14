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

using BTNET.BVVM.Log;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimerSink;

namespace BTNET.BVVM.BT
{
    public class Timing
    {
        /// <summary>
        /// Occurs when a Sink is Missing
        /// </summary>
        internal EventHandler<int>? SinkMissing;

        private const int MAX_FAIL_COUNT = 5;

        private const int WATCHDOG_START_DELAY_MS = 5000;
        private const int WATCHDOG_UPDATE_INTERVAL_MS = 250;

        private int FailCountSinkOne;
        private int FailCountSinkTwo;
        private int WatchDogCount;

        public Timer? Multimedia_Timer_Check_No_Invoke_Timer { get; set; }

        /// <summary>
        /// Its possible in extremely rare circumstances for a Task to kill the Multimedia Timer running the Sink,
        /// This would generally indicate incorrect error handling in changes you have made or some other extremely rare event out of your control
        /// <para>https://docs.microsoft.com/en-us/windows/win32/multimedia/multimedia-timers</para>
        /// </summary>
        public void Multimedia_Timer_Watchdog()
        {
            _ = Task.Run(() =>
            {
                Multimedia_Timer_Check_No_Invoke_Timer = new(new TimerCallback(async (o) =>
                {
                    if (ObservableObject.Sink != null)
                    {
                        FailCountSinkOne = await TestSinkAsync(ObservableObject.Sink, FailCountSinkOne).ConfigureAwait(false);
                    }
                    else
                    {
                        WriteLog.Error("Sink One has disappeared, Attempting to recover from event by recreating sink");
                        SinkMissing?.Invoke(null, 1);
                        return;
                    }

                    if (ObservableObject.SinkTwo != null)
                    {
                        FailCountSinkTwo = await TestSinkAsync(ObservableObject.SinkTwo, FailCountSinkTwo).ConfigureAwait(false);
                    }
                    else
                    {
                        WriteLog.Error("Sink Two has disappeared, Attempting to recover from event by recreating sink");
                        SinkMissing?.Invoke(null, 2);
                        return;
                    }

                }), null, WATCHDOG_START_DELAY_MS, WATCHDOG_UPDATE_INTERVAL_MS);

                WatchDogCount++;
                WriteLog.Info("Timer WatchDog [" + WatchDogCount + "] Started Successfully..");
            });
        }

        private Task<int> TestSinkAsync(TimingSink timingSink, int failCount)
        {
            if (timingSink.SinkFaulted)
            {
                try
                {
                    timingSink.Stop();
                    timingSink.Start();

                    WriteLog.Error("Timing Sink Faulted but isn't null, Attempting Restart..");

                    if (failCount > MAX_FAIL_COUNT)
                    {
                        Panic();
                    }

                    failCount++;
                }
                catch (Exception ex)
                {
                    WriteLog.Error(ex);
                }

                return Task.FromResult(failCount);
            }

            return Task.FromResult(0);
        }

        public static void Panic()
        {
            Message.ShowBox("Timing Sink or Precision Timer has experienced an error it can't recover from", "Please Restart", waitForReply: true, exit: true);
        }

        public void DisposeWatchdog()
        {
            Multimedia_Timer_Check_No_Invoke_Timer?.Dispose();
        }
    }
}
