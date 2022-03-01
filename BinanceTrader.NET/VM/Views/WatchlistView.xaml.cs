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

using BTNET.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace BTNET.Views
{
    /// <summary>
    /// Interaction logic for WatchlistView.xaml
    /// </summary>
    public partial class WatchlistView : Window
    {
        public WatchlistView(Main datacontext)
        {
            InitializeComponent();
            this.Topmost = true;
            DataContext = datacontext;
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            }

            this.DragMove();
        }
    }
}