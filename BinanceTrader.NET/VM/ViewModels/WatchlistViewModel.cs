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
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using LoopDelay.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class WatchlistViewModel : ObservableObject
    {
        private const int MAX_ATTEMPTS = 3;

        private const int LOOP_DELAY_MS = 20;
        private const int LOOP_EXPIRE_TIME_MS = 10000;

        private ObservableCollection<WatchlistItem> watchlistitems = new();
        private ObservableCollection<string> allWatchlistSymbols = new();
        private string? selectedWatchlistSymbol;
        private bool watchlistSymbolsEnabled;
        private string? resetText;

        private static bool RestoreWatchListAttempted { get; set; }

        public ICommand? RemoveFromWatchlistCommand { get; set; }
        public ICommand? AddToWatchlistCommand { get; set; }

        private List<string> watchlistitemSymbols = new();

        public void InitializeCommands()
        {
            RemoveFromWatchlistCommand = new DelegateCommand(RemoveWatchlistItem);
            AddToWatchlistCommand = new DelegateCommand(AddWatchlistItem);
        }

        public ObservableCollection<WatchlistItem> WatchListItems
        {
            get => watchlistitems;
            set
            {
                watchlistitems = value;
                PC();
            }
        }

        public List<string> WatchlistitemSymbols
        {
            get => this.watchlistitemSymbols;
            set
            {
                this.watchlistitemSymbols = value;
                PC();
            }
        }

        public ObservableCollection<string> AllWatchlistSymbols
        {
            get => allWatchlistSymbols;
            set
            {
                allWatchlistSymbols = value;
                PC();
            }
        }

        public string? SelectedWatchlistSymbol
        {
            get => selectedWatchlistSymbol;
            set
            {
                selectedWatchlistSymbol = value;
                PC();
            }
        }

        public bool WatchlistSymbolsEnabled
        {
            get => watchlistSymbolsEnabled;
            set
            {
                watchlistSymbolsEnabled = value;
                PC();
            }
        }

        public string? ResetText
        {
            get => resetText;
            set
            {
                resetText = value;
                PC();
            }
        }

        public async Task InitializeWatchListAsync()
        {
            try
            {
                WatchlistSymbolsEnabled = false;
                ResetText = "Loading";

                _ = Task.Run(() =>
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        foreach (var symbol in Static.AllPricesUnfiltered)
                        {
                            AllWatchlistSymbols.Add(symbol.SymbolView.Symbol);
                        }
                    });
                });

                List<string>? StoredwatchlistSymbols = TJson.Load<List<string>>(App.Listofwatchlistsymbols, true);
                WatchMan.Load_Watchlist.SetCompleted();

                if (StoredwatchlistSymbols != null)
                {
                    WriteLog.Info("Loaded [" + StoredwatchlistSymbols.Count + "] Watchlist symbols into Watchlist from File");
                    foreach (string Symbol in StoredwatchlistSymbols)
                    {
                        await AddWatchlistItemAsync(Symbol).ConfigureAwait(false);
                    }
                }

                var startTime = DateTime.UtcNow.Ticks;
                var sw = Stopwatch.StartNew();
                while (await Loop.Delay(startTime, LOOP_DELAY_MS, LOOP_EXPIRE_TIME_MS, () =>
                {
                    WriteLog.Info("Watchlist is taking a long time to connect: [" + sw.ElapsedMilliseconds.ToString() + "ms]");
                }).ConfigureAwait(false))
                {
                    int check = 0;

                    foreach (var item in WatchListItems)
                    {
                        if (item.TickerStatus != Ticker.CONNECTED)
                        {
                            check++;
                        }
                    }

                    if (check == 0)
                    {
                        WriteLog.Info("Loaded Watchlist and Connected Sockets in [" + sw.ElapsedMilliseconds.ToString() + "ms]");
                        break;
                    }
                }

                WatchlistSymbolsEnabled = true;
                ResetText = "";
            }
            catch (Exception ex)
            {
                WatchMan.Load_Watchlist.SetError();
                WriteLog.Error("Watchlist Failed to Initialize: ", ex);
            }
        }

        public void AddWatchlistItem(object o)
        {
            if (SelectedWatchlistSymbol == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                if (!await AddWatchlistItemAsync(SelectedWatchlistSymbol).ConfigureAwait(false))
                {
                    Message.ShowBox("Please enter a valid Symbol", "Invalid Symbol");
                }
            }).ConfigureAwait(false);
        }

        public Task<bool> AddWatchlistItemAsync(string Symbol)
        {
            if (Static.AllPricesUnfiltered.Where(x => x.SymbolView.Symbol == Symbol).FirstOrDefault() != null)
            {
                WatchlistItem watchListItem = new WatchlistItem(Symbol);
                watchListItem.SubscribeWatchListItemSocket();

                InvokeUI.CheckAccess(() =>
                {
                    WatchListItems.Add(watchListItem);
                    WatchListItems = new ObservableCollection<WatchlistItem>(WatchListItems.OrderByDescending(d => d.WatchlistSymbol));
                });

                return Task.FromResult(true);
            }
            else
            {
                WriteLog.Error("Failed to add WatchlistItem");
                return Task.FromResult(false);
            }
        }

        public void RemoveWatchlistItem(object o)
        {
            var watchlistItem = (WatchlistItem)o;

            if (watchlistItem != null)
            {
                Task.Run(() =>
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        WriteLog.Info("Removed Watchlist Symbol: " + watchlistItem.WatchlistSymbol);
                        WatchListItems.Remove(watchlistItem);
                    });
                });
            }
            else
            {
                WriteLog.Error("Error Removing Watchlist Symbol at: [" + DateTime.UtcNow.ToString() + "]");
            }
        }

        public void StoreWatchListItemsSymbols()
        {
            if (WatchListItems != null)
            {
                foreach (WatchlistItem watchlistItem in WatchListItems)
                {
                    var ws = watchlistItem.WatchlistSymbol;
                    if (ws != null)
                    {
                        WatchlistitemSymbols.Add(ws);
                    }
                }

                if (WatchlistitemSymbols.Count > 0)
                {
                    TJson.Save(WatchlistitemSymbols, App.Listofwatchlistsymbols);
                }
            }
        }
    }
}
