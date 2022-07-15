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

using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BTNET.VM.Views
{
    public partial class AlertsView : Window
    {
        private const int COLUMN_ONE = 0;
        private const int COLUMN_TWO = 1;
        private const int COLUMN_THREE = 2;
        private const int COLUMN_FOUR = 3;
        private const int COLUMN_FIVE = 4;
        private const int COLUMN_SIX = 5;

        private const int INTERVAL_DEFAULT_WIDTH = 65;
        private const int SETTINGS_DEFAULT_WIDTH = 50;
        private const int DIRECTION_DEFAULT_WIDTH = 50;
        private const int STATUS_DEFAULT_WIDTH = 63;

        private const int WIDTH_OFFSET = 10;

        private const int HALF_WIDTH = 2;
        private const int ANIMATION_DELAY_MS = 610;
        private const int SIDE_MENU_RESET_TIME_MS = 1250;
        private const int SIDE_MENU_OPEN = 100;
        private const int SIDE_MENU_CLOSED = 0;

        public AlertsView(MainContext datacontext)
        {
            AllowsTransparency = true;
            DataContext = datacontext;

            InitializeComponent();
            cbDirection.ItemsSource = Enum.GetValues(typeof(Direction)).Cast<Direction>();
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == App.RAPID_CLICKS_TO_MAXIMIZE_WINDOW)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }

            MainContext.BorderAdjustment(WindowState, true);
            DragMove();

            _ = !IsMenuOpen() ? CloseSideMenuAsync().ConfigureAwait(false) : OpenSideMenuAsync().ConfigureAwait(false);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderBrush = Brushes.Transparent;
            BorderThickness = MainContext.BorderAdjustment(WindowState, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ObservableObject.AlertVM.ToggleAlertEnabled = false;

            if (IsMenuOpen())
            {
                ObservableObject.AlertVM.ToggleAlertSideMenu = SIDE_MENU_CLOSED;
                Task.Run(async () =>
                {
                    await Task.Delay(ANIMATION_DELAY_MS).ConfigureAwait(false);
                    await CloseSideMenuAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else
            {
                ObservableObject.AlertVM.ToggleAlertSideMenu = SIDE_MENU_OPEN;
                Task.Run(async () =>
                {
                    await Task.Delay(ANIMATION_DELAY_MS).ConfigureAwait(false);
                    await OpenSideMenuAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }

            Task.Run(async () =>
            {
                await Task.Delay(SIDE_MENU_RESET_TIME_MS).ConfigureAwait(false);
                ObservableObject.AlertVM.ToggleAlertEnabled = true;
            }).ConfigureAwait(false);
        }

        private bool IsMenuOpen()
        {
            if (ObservableObject.AlertVM.ToggleAlertSideMenu == SIDE_MENU_OPEN)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Task CloseSideMenuAsync()
        {
            var RealWidth = AlertListView.ActualWidth;
            InvokeUI.CheckAccess(() =>
            {
                var contents = AlertListView.View as GridView;

                if (contents != null)
                {
                    contents.Columns[COLUMN_THREE].Width = INTERVAL_DEFAULT_WIDTH + WIDTH_OFFSET; // Interval
                    RealWidth -= contents.Columns[COLUMN_THREE].Width;

                    contents.Columns[COLUMN_FOUR].Width = DIRECTION_DEFAULT_WIDTH + WIDTH_OFFSET; // Direction
                    RealWidth -= contents.Columns[COLUMN_FOUR].Width;

                    contents.Columns[COLUMN_FIVE].Width = STATUS_DEFAULT_WIDTH + WIDTH_OFFSET; // Status
                    RealWidth -= contents.Columns[COLUMN_FIVE].Width;

                    contents.Columns[COLUMN_SIX].Width = SETTINGS_DEFAULT_WIDTH + WIDTH_OFFSET; // Settings
                    RealWidth -= contents.Columns[COLUMN_SIX].Width;

                    var diff = RealWidth / HALF_WIDTH;

                    contents.Columns[COLUMN_ONE].Width = diff; // Symbol
                    contents.Columns[COLUMN_TWO].Width = diff; // Price
                }
            });

            return Task.CompletedTask;
        }

        private Task OpenSideMenuAsync()
        {
            InvokeUI.CheckAccess(() =>
            {
                var RealWidth = AlertListView.ActualWidth;
                var contents = AlertListView.View as GridView;

                if (contents != null)
                {
                    contents.Columns[COLUMN_THREE].Width = INTERVAL_DEFAULT_WIDTH; // Interval
                    RealWidth -= contents.Columns[COLUMN_THREE].Width;

                    contents.Columns[COLUMN_FOUR].Width = DIRECTION_DEFAULT_WIDTH; // Direction
                    RealWidth -= contents.Columns[COLUMN_FOUR].Width;

                    contents.Columns[COLUMN_FIVE].Width = STATUS_DEFAULT_WIDTH; // Status
                    RealWidth -= contents.Columns[COLUMN_FIVE].Width;

                    contents.Columns[COLUMN_SIX].Width = SETTINGS_DEFAULT_WIDTH; // Settings
                    RealWidth -= contents.Columns[COLUMN_SIX].Width;

                    var diff = RealWidth / HALF_WIDTH;

                    contents.Columns[COLUMN_ONE].Width = diff; // Symbol
                    contents.Columns[COLUMN_TWO].Width = diff; // Price
                }
            });

            return Task.CompletedTask;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var s = sender as Image;
            if (s != null)
            {
                s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-pressed.png"));
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleButton(sender);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButton(sender);
        }

        internal void ToggleButton(object sender)
        {
            var s = sender as Image;
            if (!this.Topmost)
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top.png"));
                }
            }
            else
            {
                if (s != null)
                {
                    s.Source = new BitmapImage(new Uri("pack://application:,,,/BV/Resources/Top/top-mouseover.png"));
                }
            }
        }
    }
}
