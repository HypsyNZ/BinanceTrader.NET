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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BTNET.VM.Views
{
    public partial class WatchlistView : Window
    {
        private const int COLUMN_ONE = 0;
        private const int COLUMN_TWO = 1;
        private const int COLUMN_THREE = 2;
        private const int COLUMN_FOUR = 3;
        private const int COLUMN_FIVE = 4;
        private const int COLUMN_SIX = 5;
        private const int COLUMN_SEVEN = 6;
        private const int COLUMN_EIGHT = 7;
        private const int COLUMN_NINE = 8;
        private const int COLUMN_TEN = 9;
        private const int COLUMN_ELEVEN = 10;
        private const int DEFAULT_PRICE_WIDTH = 88;
        private const int DEFAULT_CHANGE_WIDTH = 65;
        private const int DEFAULT_REMOVE_WIDTH = 30;
        private const int DEFAULT_STATUS_WIDTH = 20;

        private const double STACK_PANEL_OPACITY_OFFSET = 0.25;
        private const double REMOVE_BUTTONS_OPACITY_OFFSET = 0.25;
        private const double DEFAULT_STACK_PANEL_OPACITY = 0.9;
        private const double DEFAULT_LIST_VIEW_OPACITY = 0.65;

        public WatchlistView(MainContext datacontext)
        {
            AllowsTransparency = true;

            InitializeComponent();
            combobox.ItemsPanel = new ItemsPanelTemplate();
            var stackPanelTemplate = new FrameworkElementFactory(typeof(VirtualizingStackPanel));
            combobox.ItemsPanel.VisualTree = stackPanelTemplate;
            DataContext = datacontext;
            sortableListView.Opacity = DEFAULT_LIST_VIEW_OPACITY;
            stackPanel.Opacity = DEFAULT_STACK_PANEL_OPACITY;
            Topmost = true;
        }

        private void DragWindowOrMaximize(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == App.RAPID_CLICKS_TO_MAXIMIZE_WINDOW)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }

            BorderBrush = Brushes.Transparent;
            MainContext.BorderAdjustment(WindowState, true);
            DragMove();
            ResizeColumns();
        }

        private void ResizeColumns()
        {
            var view = sortableListView.View as GridView;
            var RealWidth = sortableListView.ActualWidth;

            if (view != null)
            {
                // Status
                view.Columns[COLUMN_ONE].Width = DEFAULT_STATUS_WIDTH;
                RealWidth -= view.Columns[COLUMN_ONE].Width;

                // Symbol
                view.Columns[COLUMN_TWO].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_TWO].Width;

                // Price
                view.Columns[COLUMN_THREE].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_THREE].Width;

                // Bid
                view.Columns[COLUMN_FOUR].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_FOUR].Width;

                // Ask
                view.Columns[COLUMN_FIVE].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_FIVE].Width;

                // High
                view.Columns[COLUMN_SIX].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_SIX].Width;

                // Low
                view.Columns[COLUMN_SEVEN].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_SEVEN].Width;

                // Close
                view.Columns[COLUMN_EIGHT].Width = DEFAULT_PRICE_WIDTH;
                RealWidth -= view.Columns[COLUMN_EIGHT].Width;

                // Change
                view.Columns[COLUMN_NINE].Width = DEFAULT_CHANGE_WIDTH;
                RealWidth -= view.Columns[COLUMN_NINE].Width;

                // Remove
                view.Columns[COLUMN_ELEVEN].Width = DEFAULT_REMOVE_WIDTH;
                RealWidth -= view.Columns[COLUMN_ELEVEN].Width;

                // Volume
                view.Columns[COLUMN_TEN].Width = RealWidth;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderBrush = Brushes.Transparent;
            BorderThickness = MainContext.BorderAdjustment(WindowState, true);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sortableListView != null)
            {
                sortableListView.Opacity = e.NewValue;
                stackPanel.Opacity = e.NewValue + STACK_PANEL_OPACITY_OFFSET;
            }
        }
    }
}
