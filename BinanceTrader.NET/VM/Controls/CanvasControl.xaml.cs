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

namespace BTNET.Controls
{
    /// <summary>
    /// Interaction logic for CanvasControl.xaml
    /// </summary>
    public partial class CanvasControl : UserControl
    {
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
                double newLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;

                RealTimeBox.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                RealTimeBox.SetValue(Canvas.TopProperty, newTop);
            }
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

        private void BorrowBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(BorrowBox).X;
            firstYPos = e.GetPosition(BorrowBox).Y;

            movingObject = sender;
        }

        private void MarginBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                double newLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;

                MarginBox.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                MarginBox.SetValue(Canvas.TopProperty, newTop);
            }
        }

        private void MarginBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstXPos = e.GetPosition(MarginBox).X;
            firstYPos = e.GetPosition(MarginBox).Y;

            movingObject = sender;
        }

        private void BorrowBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                double newLeft = e.GetPosition(canvas).X - firstXPos - canvas.Margin.Left;

                BorrowBox.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - firstYPos - canvas.Margin.Top;

                BorrowBox.SetValue(Canvas.TopProperty, newTop);
            }
        }
    }
}