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

using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.Controls
{
    /// <summary>
    /// Interaction logic for CanvasControl.xaml
    /// </summary>
    public partial class CanvasControl : UserControl
    {
        private object? movingObject;
        private double firstXPos;
        private double firstYPos;

        public static double RealTimeLeft { get; set; }
        public static double RealTimeTop { get; set; }

        public static double BorrowBoxLeft { get; set; }
        public static double BorrowBoxTop { get; set; }

        public static double MarginBoxLeft { get; set; }
        public static double MarginBoxTop { get; set; }

        public static double CanvasActualWidth { get; set; }

        public static double CanvasActualHeight { get; set; }

        public CanvasControl()
        {
            InitializeComponent();
        }

        private void StopMoving(object sender, MouseButtonEventArgs e)
        {
            movingObject = null;
        }

        private void BreakDownBoxDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(BreakdownBox).X;
            firstYPos = e.GetPosition(BreakdownBox).Y;

            movingObject = sender;
        }

        private void BreakDownBoxMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                double newLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;

                BreakdownBox.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                BreakdownBox.SetValue(Canvas.TopProperty, newTop);
            }
        }

        private void InfoBoxDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(InfoBox).X;
            firstYPos = e.GetPosition(InfoBox).Y;

            movingObject = sender;
        }

        private void InfoBoxMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                double newLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;

                InfoBox.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                InfoBox.SetValue(Canvas.TopProperty, newTop);
            }
        }

        private void RealTimeBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(RealTimeBox).X;
            firstYPos = e.GetPosition(RealTimeBox).Y;

            movingObject = sender;
        }

        private void RealTimeBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                RealTimeLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;
                RealTimeTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                RealTimeBox.SetValue(Canvas.LeftProperty, RealTimeLeft);
                RealTimeBox.SetValue(Canvas.TopProperty, RealTimeTop);
            }
        }

        private void BorrowBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(BorrowBox).X;
            firstYPos = e.GetPosition(BorrowBox).Y;

            movingObject = sender;
        }

        private void BorrowBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                BorrowBoxLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;
                BorrowBoxTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                BorrowBox.SetValue(Canvas.LeftProperty, BorrowBoxLeft);
                BorrowBox.SetValue(Canvas.TopProperty, BorrowBoxTop);
            }
        }

        private void MarginBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(MarginBox).X;
            firstYPos = e.GetPosition(MarginBox).Y;

            movingObject = sender;
        }

        private void MarginBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                MarginBoxLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;
                MarginBoxTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                MarginBox.SetValue(Canvas.LeftProperty, MarginBoxLeft);
                MarginBox.SetValue(Canvas.TopProperty, MarginBoxTop);
            }
        }

        private void CanvasC_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            CanvasActualWidth = CanvasC.ActualWidth;
            CanvasActualHeight = CanvasC.ActualHeight;
        }
    }
}
