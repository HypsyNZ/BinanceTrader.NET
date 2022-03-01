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

using CefSharp;
using CefSharp.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace BTNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string Product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false)).Product;
        public static readonly string Version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute), false)).Version;

        private App()
        {
            using (Process p = Process.GetCurrentProcess())
            {
                int affinity = p.ProcessorAffinity.ToInt32();
                p.PriorityClass = ProcessPriorityClass.RealTime;
                p.ProcessorAffinity = (IntPtr)(affinity / 24);
            }

            var settings = new CefSettings()
            {
                UserAgent = Product + " (" + Version + ")",
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
    }
}