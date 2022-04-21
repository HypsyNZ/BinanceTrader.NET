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

using BTNET.BVVM.Log;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal class General : ObservableObject
    {
        /// <summary>
        /// Synchronizes the local time with the default Time Server you have set on your OS
        /// This isn't the same as manually clicking "Sync Now" its better
        /// </summary>
        /// <returns></returns>
        public static Task SyncLocalTime()
        {
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var automaticSyncTime = await Command.Run("sc config W32Time start=auto").ConfigureAwait(false);

                    var returnedSuccess = await Command.Run("sc start W32Time").ConfigureAwait(false);

                    if (returnedSuccess)
                    {
                        var syncedTime = await Command.Run("w32tm /resync").ConfigureAwait(false);
                        if (syncedTime)
                        {
                            WriteLog.Info("W32API: Synchronized The Local Time..");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Something went wrong Synchronizing local time : ", ex);
                }
            }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets Process Priority to Real Time
        /// </summary>
        /// <returns></returns>
        public static Task ProcessPriority()
        {
            try
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    p.PriorityClass = ProcessPriorityClass.RealTime;
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Something went wrong setting Process Priority", ex);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets Processor Affinity to use every other core
        /// (uses 8 cores on 20 core machine)
        /// </summary>
        /// <returns></returns>
        public static Task ProcessAffinity()
        {
            try
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    int affinity = p.ProcessorAffinity.ToInt32();
                    p.ProcessorAffinity = (IntPtr)(affinity / 24);
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Something went wrong setting Process Affinity", ex);
            }
            return Task.CompletedTask;
        }
    }
}