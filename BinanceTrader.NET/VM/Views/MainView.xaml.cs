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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace BTNET.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
#if RELEASE
            Process[] processes = Process.GetProcessesByName("BinanceTrader.NET");
            if (processes.Length > 1) { System.Environment.Exit(4); }
#endif

            _ = WinApi.TimeBeginPeriod(1);
            InitializeComponent();
            //this.Topmost = true;
        }

        // Punish bad apps by setting a global
        public static class WinApi
        {
            /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>
            [SuppressUnmanagedCodeSecurity]
            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
            public static extern uint TimeBeginPeriod(uint uMilliseconds);

            /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>
            [SuppressUnmanagedCodeSecurity]
            [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
            public static extern uint TimeEndPeriod(uint uMilliseconds);
        }

        private void SortableListViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            ((Controls.SortableListView)sender).GridViewColumnHeaderClicked(e.OriginalSource as GridViewColumnHeader);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }

            DragMove();
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderThickness = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
        }

        private void Minilog_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Minilog.ScrollToEnd();
        }
    }
}