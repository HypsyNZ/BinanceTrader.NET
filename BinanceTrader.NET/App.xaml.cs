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

using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using CefSharp;
using CefSharp.Wpf;
using PrecisionTiming;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
#if RELEASE
            Process[] processes = Process.GetProcessesByName("BinanceTrader.NET");
            if (processes.Length > 1) { System.Environment.Exit(7); }
#endif
            if (IsAdministrator())
            {
                ServicePointManager.DefaultConnectionLimit = 50;

                TimingSettings.SetMinimumTimerResolution(1);

                _ = Task.Run(async () =>
                {
                    await General.SyncLocalTime().ConfigureAwait(false);
                }).ConfigureAwait(false);

                _ = Task.Run(async () =>
                {
                    await General.ProcessPriority().ConfigureAwait(false);
                    await General.ProcessAffinity().ConfigureAwait(false);
                }).ConfigureAwait(false);

                var settings = new CefSettings()
                {
                    UserAgent = ObservableObject.Product + " (" + ObservableObject.Version + ")",
                    PersistSessionCookies = true,
                    PersistUserPreferences = true,
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                };

                settings.WindowlessRenderingEnabled = true;
                settings.LogSeverity = LogSeverity.Disable;
                settings.CefCommandLineArgs.Add("disable-preconnect");
                settings.CefCommandLineArgs.Add("disable-background-networking");
                settings.CefCommandLineArgs.Add("disable-webaudio");
                settings.CefCommandLineArgs.Add("dns-prefetch-disable");
                settings.CefCommandLineArgs.Add("no-pings");
                settings.CefCommandLineArgs.Add("ui-enable-zero-copy");
                settings.CefCommandLineArgs.Add("enable-zero-copy");
                settings.CefCommandLineArgs.Add("enable-oop-rasterization");
                settings.SetOffScreenRenderingBestPerformanceArgs();
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

                InitializeComponent();
            }
            else
            {
                MessageBox.Show("Binance Trader must run as Administrator so it can run in Real Time and Sync the Local Time!", "Please Restart As Administrator!", MessageBoxButton.OK, MessageBoxImage.Hand);
                {
                    System.Environment.Exit(7);
                }
            }
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}