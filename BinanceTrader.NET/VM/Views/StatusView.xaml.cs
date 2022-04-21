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
using System.Windows;
using System.Windows.Input;

namespace BTNET.VM.Views
{
    /// <summary>
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : Window
    {
        public StatusView(MainContext datacontext)
        {
            InitializeComponent();
            DataContext = datacontext;
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            this.DragMove();
        }
    }
}