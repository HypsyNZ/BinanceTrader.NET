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

using BTNET.BVVM.Log;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM
{
    public class Message
    {
        /// <summary>
        /// Show a message to the user with an OK button
        /// <para>This message box does not wait for user input</para>
        /// </summary>
        /// <param name="text">The text for the message box</param>
        /// <param name="caption">The title caption for the message box</param>
        /// <param name="waitForReply">True if you need the response from the user</param>
        /// <param name="exit">True if program should exit when user responds, waitForReply must be true</param>
        /// <param name="shutdownOrExit">True if Environment.Exit should be used, False for Current.Shutdown()</param>
        /// <returns>Message Box Result None</returns>
        public static MessageBoxResult ShowBox(string text, string caption, MessageBoxButton messageBoxButton = MessageBoxButton.OK, MessageBoxImage messageBoxImage = MessageBoxImage.Hand, bool waitForReply = false, bool exit = false, bool shutdownOrExit = true)
        {
            if (waitForReply)
            {
                return Show(text, caption, messageBoxButton, messageBoxImage, exit, shutdownOrExit);
            }
            else
            {
                _ = Task.Run(() =>
                {
                    // Log Message
                    WriteLog.Info(text);

                    // Display Message with OK button since no other option exists
                    return MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Hand);
                }).ConfigureAwait(false);
                return MessageBoxResult.None;
            }
        }

        private static MessageBoxResult Show(string text, string caption, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, bool exit, bool shutdownOrExit)
        {
            var result = MessageBox.Show(text, caption, messageBoxButton, messageBoxImage);

            if (exit)
            {
                if (shutdownOrExit)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Application.Current.Shutdown(0);
                }
            }

            return result;
        }
    }
}
