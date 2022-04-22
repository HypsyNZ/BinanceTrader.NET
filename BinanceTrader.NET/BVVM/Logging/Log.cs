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

using BinanceAPI.Objects;
using SimpleLog4.NET;
using System;
using System.Threading.Tasks;

namespace BTNET.BVVM.Log
{
    public static class WriteLog
    {
        private static SimpleLog4.NET.Log LogGeneral = new(@"C:\\BNET\\log.txt", true, logLevel: LogLevel.Information);
        private static SimpleLog4.NET.Log LogAlert = new(@"C:\\BNET\\alerts.txt", true, logLevel: LogLevel.Information);

        #region [ Static ]

        public static void Alert(string m)
        {
            LogAlert.Info(m);
        }

        public static void Info(string m)
        {
            LogGeneral.Info(m);
        }

        public static void Error(string m)
        {
            LogGeneral.Error(m);
        }

        public static void Error(Exception ex)
        {
            LogGeneral.Error(ex);
        }

        public static void Error(string m, Exception ex)
        {
            LogGeneral.Error(m, ex);
        }

        // -1000 UNKNOWN
        // -1003 TOO_MANY_REQUESTS
        // -1004 SERVER_BUSY

        public static bool ShouldLogResp<T>(WebCallResult<T> d)
        {
            if (d.Error.Code is (-1003) or (-1004) or (-1000))
            {
                return true;
            }

            return false;
        }

        public static bool ShouldLogResp<T>(Task<WebCallResult<T>> d)
        {
            if (d.Result.Error.Code is (-1003) or (-1004) or (-1000))
            {
                return true;
            }

            return false;
        }

        #endregion [ Static ]
    }
}