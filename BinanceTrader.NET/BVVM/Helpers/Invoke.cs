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
using System.Windows.Threading;

namespace BTNET.BVVM.Helpers
{
    public static class Invoke
    {
        public static void InvokeUI(Action action)
        {
            try
            {
                Dispatcher dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                {
                    if (dispatcher.CheckAccess()) { action(); return; }

                    dispatcher.Invoke(delegate
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            WriteLog.Error("Invoke Error: ", ex);
                        }
                    });
                }
            }
            catch
            {
            }
        }

        public static void BeginInvokeUI(Action action)
        {
            try
            {
                Dispatcher dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                {
                    if (dispatcher.CheckAccess()) { action(); return; }

                    dispatcher.BeginInvoke((Action)delegate
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            WriteLog.Error("BeginInvoke Error: ", ex);
                        }
                    });
                }
            }
            catch
            {
            }
        }
    }
}