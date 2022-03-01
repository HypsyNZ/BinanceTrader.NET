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

using BTNET.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BTNET.BVVM.HELPERS
{
    public class StoreList : ObservableObject
    {
        private static readonly StoredListLong TempListLong = new();
        private static readonly StoredListString TempListString = new();

        public static void StoreListLong(List<long> list, string path)
        {
            try
            {
                if (list != null)
                {
                    if (File.Exists(path))
                    {
                        string alreadyStoredList = File.ReadAllText(path);

                        if (alreadyStoredList != null)
                        {
                            List<long> compareList = JsonConvert.DeserializeObject<StoredListLong>(alreadyStoredList).List;

                            if (list == compareList)
                            {
                                return;
                            }
                        }
                    }

                    TempListLong.List = list;
                    if (TempListLong.List != null)
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(TempListLong));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Storing List: ", ex);
            }
        }

        public static void StoreListString(List<string> list, string path)
        {
            try
            {
                if (list != null)
                {
                    if (File.Exists(path))
                    {
                        string alreadyStoredList = File.ReadAllText(path);

                        if (alreadyStoredList != null && alreadyStoredList != "")
                        {
                            List<string> compareList = JsonConvert.DeserializeObject<StoredListString>(alreadyStoredList).List;

                            if (list == compareList)
                            {
                                return;
                            }
                        }
                    }

                    TempListString.List = list;
                    if (TempListString.List != null)
                    {
                        File.WriteAllText(path, JsonConvert.SerializeObject(TempListString));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Storing List: ", ex);
            }
        }
    }
}