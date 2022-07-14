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

using BTNET.BVVM.BT;
using BTNET.BVVM.Log;
using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BTNET.BVVM.Helpers
{
    internal class Browser
    {
        public static Task ConfigureBrowserAsync()
        {
            WatchMan.Load_Browser.SetWorking();

            try
            {
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
                Cef.EnableHighDPISupport();
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            }
            catch (Exception ex)
            {
                WatchMan.Load_Browser.SetError();
                WriteLog.Error(ex);
            }

            WatchMan.Load_Browser.SetCompleted();

            return Task.CompletedTask;
        }
    }
}
