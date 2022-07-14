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

using BTNET.BVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BTNET.VM.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            //this.Topmost = true;
            this.Focus();
            this.BringIntoView();
        }

        private void SortableListViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader f)
            {
                ((Controls.SortableListView)sender).GridViewColumnHeaderClicked(f);
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == App.RAPID_CLICKS_TO_MAXIMIZE_WINDOW)
            {
                var state = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                WindowState = state;

                switch (state)
                {
                    case WindowState.Normal:
                        ObservableObject.MainVM.ListViewControlHeightOffset = App.ORDER_LIST_MAX_HEIGHT_OFFSET_NORMAL;
                        break;

                    default:
                        ObservableObject.MainVM.ListViewControlHeightOffset = App.ORDER_LIST_MAX_HEIGHT_OFFSET_MAXIMIZED;
                        break;
                }

                _ = MainContext.ResetControlPositionsAsync();
                _ = MainContext.PaddingWidthAsync();
            }

            BorderThickness = MainContext.BorderAdjustment(WindowState);
            DragMove();
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderBrush = Brushes.Transparent;
            BorderThickness = MainContext.BorderAdjustment(WindowState);
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var s = sender as Image;

            if (s != null)
            {
                if (closed)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/open-side-menu-pressed.png"));
                }
                else
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/close-side-menu-pressed.png"));
                }
            }
        }

        private void Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var s = sender as Image;

            if (s != null)
            {
                if (closed)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/open-side-menu.png"));
                }
                else
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Side/close-side-menu.png"));
                }
            }
        }

        private bool closed;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            closed = !closed;
        }
    }
}
