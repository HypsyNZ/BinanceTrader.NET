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

using BTNET.BVVM.Log;
using System.Threading.Tasks;
using System.Windows;

namespace BTNET.BVVM.MessageBox
{
    public class MessageBoxService : IMessageBoxService
    {
        /// <summary>
        /// Show a message to the user
        /// <para>This message box does not wait for user input by default (Buttons won't do anything)</para>
        /// <para>Please set waitForReply to true if you need to wait for the users reply</para>
        /// </summary>
        /// <param name="text">The text for the message box</param>
        /// <param name="caption">The title caption for the message box</param>
        /// <param name="messageButtons">Which buttons the message box should have</param>
        /// <param name="messageIcon">Icon for the message box</param>
        /// <param name="waitForReply">True if you need the response from the user</param>
        /// <returns></returns>
        public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton messageButtons, MessageBoxImage messageIcon, bool waitForReply = false)
        {
            if (waitForReply)
            {
                return System.Windows.MessageBox.Show(text, caption, messageButtons, messageIcon);
            }
            else
            {
                _ = Task.Run(() =>
                {
                    // Log Message
                    WriteLog.Info(text);

                    // Display Message with OK button since no other option exists
                    return System.Windows.MessageBox.Show(text, caption, MessageBoxButton.OK, messageIcon);
                }).ConfigureAwait(false);
                return MessageBoxResult.None;
            }
        }
    }
}