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

using BTNET.BV.Abstract;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal class General : ObservableObject
    {
        private const int AFFINITY_MASK = 24;
        private const int EXIT_REASON = 7;

        /// <summary>
        /// Use the Github API to Check for Updates
        /// </summary>
        /// <returns></returns>
        public static Task<string> CheckUpTodateAsync()
        {
            try
            {
                if (SettingsVM.CheckForUpdatesIsChecked ?? false)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/HypsyNZ/BinanceTrader.NET/releases");
                    request.UserAgent = new Random(new Random().Next()).ToString();

                    var response = request.GetResponse();
                    if (response != null)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());

                        string readerOutput = reader.ReadToEnd();

                        List<Tag>? deserializedString = JsonConvert.DeserializeObject<List<Tag>>(readerOutput);
                        if (deserializedString != null && deserializedString.Count > 0)
                        {
                            Tag tagOnFirstRelease = deserializedString.FirstOrDefault();
                            if (tagOnFirstRelease != null)
                            {
                                if ("v" + ObservableObject.Version == tagOnFirstRelease.TagName)
                                {
                                    return Task.FromResult("You are using the most recent version");
                                }
                                else
                                {
                                    return Task.FromResult("Update: " + tagOnFirstRelease.TagName);
                                }
                            }
                        }
                    }
                }
                else
                {
                    return Task.FromResult("Checking for Updates is Disabled");
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Checking Version: ", ex);
                return Task.FromResult("Error While Checking Version");
            }

            return Task.FromResult("Couldn't check for Updates, Network Error");
        }

        /// <summary>
        /// Sets Process Priority to Real Time
        /// </summary>
        /// <returns></returns>
        public static async Task ProcessPriorityAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using Process p = Process.GetCurrentProcess();
                    p.PriorityClass = ProcessPriorityClass.RealTime;
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Something went wrong setting Process Priority", ex);
                }
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Sets Processor Affinity to use every other core
        /// (uses 8 cores on 20 core machine)
        /// </summary>
        /// <returns></returns>
        public static async Task ProcessAffinityAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using Process p = Process.GetCurrentProcess();
                    int affinity = p.ProcessorAffinity.ToInt32();
                    p.ProcessorAffinity = (IntPtr)(affinity / AFFINITY_MASK);
                }
                catch (Exception ex)
                {
                    WriteLog.Error("Something went wrong setting Process Affinity", ex);
                }
            });
        }

        /// <summary>
        /// Determines if the current user has the Administrator Role
        /// </summary>
        /// <returns>True if the user has the Administrator Role</returns>
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Exits if the specified number of instances of the program are already running
        /// </summary>
        /// <param name="processName"></param>
        public static void LimitInstances(string processName, int maxNumber)
        {
#if RELEASE
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > maxNumber)
            {
                Message.ShowBox("Binance Trader is Already Running [" + maxNumber + "] Instance, which is the Maximum", "Already Running", waitForReply: true, exit: true);
            }
#endif
        }
    }
}
