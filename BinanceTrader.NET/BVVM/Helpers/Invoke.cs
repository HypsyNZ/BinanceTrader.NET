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

using System;
using System.Windows;

namespace BTNET.BVVM.HELPERS
{
    public static class Invoke
    {
        public static void InvokeUI(Action action)
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess()) { action(); return; }

                Application.Current.Dispatcher.Invoke(delegate { action(); });
            }
            catch (Exception e)
            {
                WriteLog.Info("Invoke Error Reason: " + e.Message + "Inner Exception: " + e.InnerException + " | Stack Trace: " + e.StackTrace + " | HResult: " + e.HResult);
            }
        }

        public static void BeginInvokeUI(Action action)
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess()) { action(); return; }

                Application.Current.Dispatcher.BeginInvoke((Action)delegate { action(); });
            }
            catch (Exception e)
            {
                WriteLog.Info("Begin Invoke Error Reason: " + e.Message + "Inner Exception: " + e.InnerException + " | Stack Trace: " + e.StackTrace + " | HResult: " + e.HResult);
            }
        }
    }
}