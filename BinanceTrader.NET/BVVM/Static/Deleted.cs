using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Deleted : ObservableObject
    {
        public static bool IsDeletedTriggered { get; set; } = false;

        public static bool RestoreDeletedListAttempted { get; set; } = false;

        public static void InitializeDeletedList()
        {
            Directory.CreateDirectory(Static.SettingsPath);

            if (File.Exists(Static.listofdeletedorders))
            {
                string deletedOrder = File.ReadAllText(Static.listofdeletedorders);
                if (deletedOrder != null)
                {
                    WriteLog.Info("Loaded Deleted Order List..");
                    try
                    {
                        Static.DeletedList = JsonConvert.DeserializeObject<StoredListLong>(deletedOrder).List;
                    }
                    catch (JsonSerializationException)
                    {
                        File.Delete(Static.listofdeletedorders);

                        if (!RestoreDeletedListAttempted)
                        {
                            RestoreDeletedListAttempted = Backup.RestoreBackup(Static.listofdeletedorders, "DeletedList");
                            InitializeDeletedList();

                            WriteLog.Error("DeletedList failed to deserialize and was restored");
                        }
                        else
                        {
                            File.Delete(Static.listofdeletedorders + ".bak");
                            WriteLog.Error("The backup for DeletedList was damaged so it was deleted!");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds Order to the deleted list and stores it or ignores it if the order is already on the list
        /// </summary>
        /// <param name="id"></param>
        public static Task AddToDeletedList(long id = 0)
        {
            if (id != 0)
            {
                if (!Static.DeletedList.Contains(id))
                {
                    Static.DeletedList.Add(id);
                    StoreList.StoreListLong(Static.DeletedList, Static.listofdeletedorders);
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
        public static Task EnumerateDeletedList(ObservableCollection<OrderBase> Orders)
        {
            MainOrders.BlockOrderUpdates = true;
            foreach (var r in Orders.ToList())
            {
                if (Static.DeletedList.Contains(r.OrderId))
                {
                    Invoke.InvokeUI(() =>
                    {
                        _ = Orders.Remove(r);
                    });
                }
            }

            MainOrders.BlockOrderUpdates = false;
            return Task.CompletedTask;
        }
    }
}