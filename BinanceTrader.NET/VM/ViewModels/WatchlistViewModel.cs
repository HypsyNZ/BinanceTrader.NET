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
using BTNET.Base;
using BTNET.BVVM;
using BTNET.BVVM.HELPERS;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class WatchlistViewModel : ObservableObject
    {
        public ICommand RemoveFromWatchlistCommand { get; set; }
        public ICommand AddToWatchlistCommand { get; set; }

        public void InitializeCommands()
        {
            RemoveFromWatchlistCommand = new DelegateCommand(RemoveWatchlistItem);
            AddToWatchlistCommand = new DelegateCommand(AddWatchlistItem);
        }

        public ObservableCollection<WatchlistItem> WatchListItems
        { get => watchlistitems; set { watchlistitems = value; PC(); } }

        private List<string> watchlistitemSymbols = new();

        public List<string> WatchlistitemSymbols
        { get => this.watchlistitemSymbols; set { this.watchlistitemSymbols = value; PC(); } }

        private WatchlistItem selectedWatchListItem;

        public WatchlistItem SelectedWatchlistItem
        { get => selectedWatchListItem; set { selectedWatchListItem = value; PC(); } }

        private ObservableCollection<WatchlistItem> watchlistitems = new();

        private string watchlistSymbol;

        public string WatchlistSymbol
        { get => watchlistSymbol; set { watchlistSymbol = value; PC(); } }

        public Task InitializeWatchList()
        {
            Directory.CreateDirectory(Static.SettingsPath);

            if (File.Exists(Static.listofwatchlistsymbols))
            {
                string watchlistSymbols = File.ReadAllText(Static.listofwatchlistsymbols);
                if (watchlistSymbols != null && watchlistSymbols != "")
                {
                    List<string> StoredwatchlistSymbols = JsonConvert.DeserializeObject<StoredListString>(watchlistSymbols).List;
                    if (StoredwatchlistSymbols != null)
                    {
                        foreach (string Symbol in StoredwatchlistSymbols)
                        {
                            AddWatchlistItem(Symbol);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        public void AddWatchlistItem(object o)
        {
            Task.Run(() =>
            {
                if (Static.AllPrices.Where(x => x.SymbolView.Symbol == WatchlistSymbol).FirstOrDefault() != null)
                {
                    WatchlistItem watchListItem = new WatchlistItem();
                    watchListItem.WatchlistSymbol = WatchlistSymbol;

                    watchListItem.SubscribeWatchListItemSocket();
                    Invoke.InvokeUI(() =>
                    {
                        WatchListItems.Add(watchListItem);

                        WatchListItems = new ObservableCollection<WatchlistItem>(WatchListItems.OrderByDescending(d => d.WatchlistSymbol));
                    });
                }
                else
                {
                    Static.MessageBox.ShowMessage("Please enter a valid Symbol", "Invalid Symbol", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            });
        }

        public void AddWatchlistItem(string Symbol)
        {
            if (Static.AllPrices.Where(x => x.SymbolView.Symbol == Symbol) != null)
            {
                WatchlistItem watchListItem = new WatchlistItem();
                watchListItem.WatchlistSymbol = Symbol;

                watchListItem.SubscribeWatchListItemSocket();
                Invoke.InvokeUI(() =>
                {
                    WatchListItems.Add(watchListItem);
                    WatchListItems = new ObservableCollection<WatchlistItem>(WatchListItems.OrderByDescending(d => d.WatchlistSymbol));
                });
            }
            else
            {
                WriteLog.Error("Failed to add WatchlistItem");
            }
        }

        public void StoreWatchListItemsSymbols()
        {
            if (WatchListItems != null)
            {
                foreach (WatchlistItem watchlistItem in WatchListItems)
                {
                    WatchlistitemSymbols.Add(watchlistItem.WatchlistSymbol);
                }

                if (WatchlistitemSymbols.Count > 0)
                {
                    StoreList.StoreListString(WatchlistitemSymbols, Static.listofwatchlistsymbols);
                }
            }
        }

        public void RemoveWatchlistItem(object o)
        {
            Task.Run(() =>
            {
                if (SelectedWatchlistItem != null)
                {
                    SelectedWatchlistItem.UnsubscribeWatchListItemSocket();

                    Invoke.InvokeUI(() =>
                    {
                        WatchListItems.Remove(SelectedWatchlistItem);
                        MiniLog.AddLine("Removed Watchlist Item..");
                    });
                }
                else
                {
                    Static.MessageBox.ShowMessage("Please select a Symbol to Remove", "No Symbol Selected", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            });
        }
    }
}