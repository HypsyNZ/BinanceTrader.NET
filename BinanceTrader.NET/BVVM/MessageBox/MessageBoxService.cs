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
using System;
using System.Windows;

namespace BTNET.BVVM.MessageBox
{
    public class MessageBoxService : IMessageBoxService
    {
        public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton messageButtons, MessageBoxImage messageIcon)
        {
            // Log Message
            WriteLog.Info(text);

            // Console Logger
            Console.WriteLine(text);

            // Display Message
            return System.Windows.MessageBox.Show(text, caption, messageButtons, messageIcon);
        }
    }
}