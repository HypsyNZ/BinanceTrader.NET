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

using BTNET.BV.Abstract;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BTNET.BVVM.Helpers
{
    public class StoreList : ObservableObject
    {
        public static void StoreListLong(List<long> storedListLong, string path)
        {
            try
            {
                StoredListLong store = new(storedListLong);
                if (store.List != null && store.List.Count > 0)
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(store));

                    string CheckValidityOfBackup = File.ReadAllText(path).Normalize();
                    List<long> backup = JsonConvert.DeserializeObject<StoredListLong>(CheckValidityOfBackup).List;

                    if (backup != null && backup.Count > 0)
                    {
                        Backup.SaveBackup(path);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error("Error Storing List: ", ex);
            }
        }

        public static void StoreListString(List<string> storedListString, string path)
        {
            try
            {
                StoredListString store = new(storedListString);
                if (store != null && store.List.Count > 0)
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(store));

                    string CheckValidityOfBackup = File.ReadAllText(path).Normalize();
                    List<string> backup = JsonConvert.DeserializeObject<StoredListString>(CheckValidityOfBackup).List;

                    if (backup != null && backup.Count > 0)
                    {
                        Backup.SaveBackup(path);
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