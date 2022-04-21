using BTNET.BVVM.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    public class Timing : ObservableObject
    {
        public DateTime IsTimerStillRunningSink = new();

        public Timer Multimedia_Timer_Check_No_Invoke_Timer;

        /// <summary>
        /// Its possible in extremely rare circumstances for a Task to kill the Multimedia Timer running the Sink,
        /// This would generally indicate incorrect error handling in changes you have made or some other extremely rare event out of your control
        /// <para>https://docs.microsoft.com/en-us/windows/win32/multimedia/multimedia-timers</para>
        /// </summary>
        public void Multimedia_Timer_Watchdog()
        {
            _ = Task.Run(() =>
            {
                // Seed values to account for less than exceptional loading times
                IsTimerStillRunningSink = DateTime.Now;

                Multimedia_Timer_Check_No_Invoke_Timer = new(new TimerCallback((o) =>
                {
                    try
                    {
                        DateTime currentTime = DateTime.Now;
                        long sink = currentTime.Millisecond - IsTimerStillRunningSink.Millisecond;
                        if (sink > 1500)
                        {
                            Sink.Start();
                            WriteLog.Error("Timing Sink or Precision Timer may have stopped, Attempting Restart..");

                            if (f > 5)
                            {
                                Static.MessageBox.ShowMessage("Timing Sink or Precision Timer has experienced an error it can't recover from", "Please Restart", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Hand);
                            }

                            f++;
                            return;
                        }
                    }
                    catch
                    {
                    }

                    f = 0;
                }), null, 5000, 250);

                Multimedia_Timer_Check_No_Invoke_Timer.InitializeLifetimeService();
                WriteLog.Info("Timer WatchDog Started Successfully..");
            });
        }

        private int f = 0;

        public void DisposeWatchdog()
        {
            Multimedia_Timer_Check_No_Invoke_Timer?.Dispose();
        }
    }
}