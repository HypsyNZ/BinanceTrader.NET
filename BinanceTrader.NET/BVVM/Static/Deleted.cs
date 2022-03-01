using BTNET.Abstract;
using BTNET.Base;
using BTNET.BVVM.HELPERS;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace BTNET.BVVM
{
    internal class Deleted : ObservableObject
    {
        public static void InitializeDeletedList()
        {
            Directory.CreateDirectory(Static.SettingsPath);

            if (File.Exists(Static.listofdeletedorders))
            {
                string deletedOrder = File.ReadAllText(Static.listofdeletedorders);
                if (deletedOrder != null)
                {
                    MiniLog.AddLine("Loaded Deleted Order List..");
                    Static.DeletedList = JsonConvert.DeserializeObject<StoredListLong>(deletedOrder).List;
                }
            }
        }

        public static void EnumerateDeletedList(ObservableCollection<OrderBase> Orders, long id = 0)
        {
            if (!Static.WaitingForOrderUpdate)
            {
                Static.BlockOrderUpdates = true;

                if (id != 0)
                {
                    if (!Static.DeletedList.Contains(id))
                    {
                        Static.DeletedList.Add(id);
                        StoreList.StoreListLong(Static.DeletedList, Static.listofdeletedorders);
                    }
                }

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
            }
            Static.BlockOrderUpdates = false;
        }
    }
}