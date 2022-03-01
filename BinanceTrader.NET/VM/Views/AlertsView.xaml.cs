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

using BTNET.Base;
using BTNET.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BTNET.Views
{
    /// <summary>
    /// Interaction logic for AlertsView.xaml
    /// </summary>
    public partial class AlertsView : Window
    {
        public AlertsView(Main datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
            cbDirection.ItemsSource = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            cbIntent.ItemsSource = Enum.GetValues(typeof(Intent)).Cast<Intent>();
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            this.DragMove();
        }
    }
}