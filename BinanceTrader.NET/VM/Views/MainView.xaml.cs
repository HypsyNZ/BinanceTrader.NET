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
using System.Windows.Controls;

namespace BTNET.VM.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            //this.Topmost = true;
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
                _ = MainContext.ResetControlPositions();
                _ = MainContext.PaddingWidth();
            }

            DragMove();
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderThickness = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
        }
    }
}