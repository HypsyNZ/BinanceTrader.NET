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

using BinanceAPI.Objects;
using System;
using System.Threading.Tasks;
using static BTNET.BVVM.ObservableObject;

namespace BTNET.BVVM.Log
{
    public static class WriteLog
    {
        private const int UNKNOWN = -1000;
        private const int TOO_MANY_REQUESTS = -1003;
        private const int SERVER_BUSY = -1004;

        #region [ Static ]

        public static void Alert(string m)
        {
            _ = Task.Run(() =>
            {
                App.LogAlert.Info(m);
                LogVM.LogView = m;
            }).ConfigureAwait(false);
        }

        public static void Info(string m)
        {
            _ = Task.Run(() =>
            {
                App.LogGeneral.Info(m);
                LogVM.LogView = m;
            }).ConfigureAwait(false);
        }

        public static void Error(string m)
        {
            _ = Task.Run(() =>
            {
                App.LogGeneral.Error(m);
                LogVM.LogView = m;
            }).ConfigureAwait(false);
        }

        public static void Error(Exception ex)
        {
            _ = Task.Run(() =>
            {
                App.LogGeneral.Error(ex);
                LogVM.LogView = "Exception: " + ex.Message + "Trace: " + ex.StackTrace + "| Inner: " + ex.InnerException;
            }).ConfigureAwait(false);
        }

        public static void Error(string m, Exception ex)
        {
            _ = Task.Run(() =>
            {
                App.LogGeneral.Error(m, ex);
                LogVM.LogView = m + " | Exception: " + ex.Message + "Trace: " + ex.StackTrace + "| Inner: " + ex.InnerException;
            }).ConfigureAwait(false);
        }

        public static bool ShouldLogResp<T>(WebCallResult<T> d)
        {
            if (d.Error?.Code is (TOO_MANY_REQUESTS) or (SERVER_BUSY) or (UNKNOWN))
            {
                return true;
            }

            return false;
        }

        #endregion [ Static ]
    }
}
