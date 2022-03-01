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

using ExchangeAPI.Objects;
using log4net;
using System;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    public static class WriteLog
    {
        private static readonly ILog log = LogManager.GetLogger("General");
        private static readonly ILog loga = LogManager.GetLogger("Alerts");

        #region [ Static ]

        public static void Alert(object message)
        {
            loga.Info(message);
        }

        public static void Info(object message)
        {
            log.Info(message);
        }

        public static void Info(object message, Exception ex)
        {
            log.Info(message, ex);
        }

        public static void Error(object message)
        {
            log.Error(message);
        }

        public static void Error(object message, Exception ex)
        {
            log.Error(message, ex);
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