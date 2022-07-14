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

using BTNET.BV.Base;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Deleted : ObservableObject
    {
        public const int MAX_ATTEMPTS = 3;

        public static bool IsDeletedTriggered { get; set; }

        public static bool RestoreDeletedListAttempted { get; set; }

        public static Task InitializeDeletedListAsync()
        {
            Static.DeletedList = TJson.Load<List<long>>(App.Listofdeletedorders, true) ?? new();
            var c = Static.DeletedList.Count;
            if (c > 0)
            {
                WriteLog.Info("Loaded [" + c + "] Order Ids into Deleted List from File");
            }

            WatchMan.Load_Deletedlist.SetCompleted();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds Order to the deleted list and stores it or ignores it if the order is already on the list
        /// </summary>
        /// <param name="id"></param>
        public static Task AddToDeletedListAsync(long id = 0)
        {
            if (id != 0)
            {
                if (Static.DeletedList != null && !Static.DeletedList.Contains(id))
                {
                    Static.DeletedList.Add(id);
                    TJson.Save(Static.DeletedList, App.Listofdeletedorders);
                }
            }

            IsDeletedTriggered = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enumerate the Deleted List
        /// </summary>
        /// <param name="Orders"></param>
        /// <param name="id"></param>
        public static Task EnumerateDeletedListAsync(ObservableCollection<OrderBase>? Orders)
        {
            if (Orders == null)
            {
                return Task.CompletedTask;
            }

            foreach (var r in Orders.ToList())
            {
                if (Static.DeletedList != null && Static.DeletedList.Contains(r.OrderId))
                {
                    Invoke.InvokeUI(() =>
                    {
                        if (r.Helper != null)
                        {
                            r.Helper.SettleOrderEnabled = false;
                            r.Helper.SettleControlsEnabled = false;
                        }

                        _ = Orders.Remove(r);
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
