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

using System.Windows.Controls;
using System.Windows.Input;

namespace BTNET.VM.Controls
{
    /// <summary>
    /// Interaction logic for CanvasControl.xaml
    /// </summary>
    public partial class CanvasControl : UserControl
    {
        public static double RealTimeLeft { get; set; } = 0;
        public static double RealTimeTop { get; set; } = 0;

        public static double BorrowBoxLeft { get; set; } = 0;
        public static double BorrowBoxTop { get; set; } = 0;

        public static double MarginBoxLeft { get; set; } = 0;
        public static double MarginBoxTop { get; set; } = 0;

        public static double CanvasActualWidth { get; set; } = 0;

        public static double CanvasActualHeight { get; set; } = 0;

        public CanvasControl()
        {
            InitializeComponent();
        }

        private object movingObject;

        private double firstXPos, firstYPos;

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