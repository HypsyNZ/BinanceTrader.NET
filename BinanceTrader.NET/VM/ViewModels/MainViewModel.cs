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

using BinanceAPI;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public AlertsView AlertsView;
        public WatchlistView WatchlistView;
        public SettingsView SettingsView;
        public StatusView StatusView;
        public NotepadView NotepadView;
        public AboutView AboutView;

        private int hidesidemenu = 0;
        private string chart;
        private bool isIsolated = false, isMargin = false, isWatchlistStillLoading = false, isCurrentlyLoading = false, shouldSuspendSymbolSelection = true;

        private MainContext M = null;
        private ObservableCollection<BinanceSymbolViewModel> allPrices;

        public MainViewModel(MainContext m)
        {
            M = m;
        }

        #region [Commands]

        public void InitializeCommands()
        {
            CloseWindowCommand = new DelegateCommand(OnClosing);
            LostFocusCommand = new DelegateCommand(LostFocus);
            GotFocusCommand = new DelegateCommand(GotFocus);
            CopySelectedItemToClipboardCommand = new DelegateCommand(CopySelectedItemToClipboard);
            ExitMainWindowCommand = new DelegateCommand(OnExitMainWindow);
            BuyCommand = new DelegateCommand(Buy);
            SellCommand = new DelegateCommand(Sell);

            ToggleSettingsCommand = new DelegateCommand(ToggleSettings);
            ToggleWatchlistCommand = new DelegateCommand(ToggleWatchlist);
            ToggleStratViewCommand = new DelegateCommand(ToggleStatusView);
            ToggleAboutViewCommand = new DelegateCommand(ToggleAboutView);
            ToggleNotepadViewCommand = new DelegateCommand(ToggleNotepadView);
            ToggleAlertsCommand = new DelegateCommand(ToggleAlerts);
            ToggleConsoleCommand = new DelegateCommand(ToggleConsole);

            HideMenuCommand = new DelegateCommand(HideSymbolMenu);
        }

        public ICommand ToggleConsoleCommand { get; set; }
        public ICommand ToggleSettingsCommand { get; set; }
        public ICommand ToggleAlertsCommand { get; set; }
        public ICommand ToggleWatchlistCommand { get; set; }
        public ICommand ToggleStratViewCommand { get; set; }
        public ICommand ToggleAboutViewCommand { get; set; }
        public ICommand ToggleNotepadViewCommand { get; set; }

        public ICommand HideMenuCommand { get; set; }

        public ICommand BuyCommand { get; set; }
        public ICommand SellCommand { get; set; }
        public ICommand SaveSettingsCommand { get; set; }

        public ICommand CloseWindowCommand { get; set; }
        public ICommand LostFocusCommand { get; set; }
        public ICommand GotFocusCommand { get; set; }
        public ICommand CopySelectedItemToClipboardCommand { get; set; }
        public ICommand ExitMainWindowCommand { get; set; }

        #endregion [Commands]

        #region [ Loading ]

        public string LoadingText
        { get => (isCurrentlyLoading && IsWatchlistStillLoading) ? "Loading: Please Wait..." : IsWatchlistStillLoading ? "Watchlist: Still Connecting..." : "Loading: Please Wait..."; set { PC(); } }

        public bool ShouldSuspendSymbolSelection
        { get => shouldSuspendSymbolSelection; set { shouldSuspendSymbolSelection = value; PC(); } }

        public bool IsCurrentlyLoading
        { get => isCurrentlyLoading || IsWatchlistStillLoading; set { isCurrentlyLoading = value; LoadingText = ""; PC(); } }

        public bool IsWatchlistStillLoading
        { get => isWatchlistStillLoading; set { isWatchlistStillLoading = value; PC(); PC("IsCurrentlyLoading"); } }

        #endregion [ Loading ]

        public bool IsIsolated
        { get => isIsolated; set { isIsolated = value; PC(); } }

        public bool IsMargin
        { get => isMargin; set { isMargin = value; PC(); } }

        public string Chart
        { get => this.chart; set { this.chart = value; PC(); } }

        public int HideSideMenu
        { get => this.hidesidemenu; set { this.hidesidemenu = value; PC(); } }

        public ObservableCollection<BinanceSymbolViewModel> AllSymbolsOnUI
        { get => this.allPrices; set { this.allPrices = value; PC(); } }

        public int SelectedTabUI
        { get => (int)Static.CurrentlySelectedSymbolTab; set { Static.CurrentlySelectedSymbolTab = (SelectedTab)value; PC(); } }

        #region [Closing]

        private async void OnClosing(object o)
        {
            WriteLog.Info("Shutting Down..");

            try
            {
                Timers?.DisposeWatchdog();
                Sink?.Stop();
                NotepadVM?.SaveNotes();

                foreach (Window w in Application.Current.Windows)
                {
                    w.Hide();
                    IsSymbolSelected = false;
                }

                await Task.Run(() =>
                {
                    WriteLog.Info("Storing Orders..");
                    StoreList.StoreListLong(Static.DeletedList, Static.listofdeletedorders);
                    Static.ManageStoredOrders.AddCurrentOrdersToCollections(MainOrders.GetOrders);
                    Static.ManageStoredOrders.StoreAllOrderCollections();

                    if (!IsWatchlistStillLoading)
                    {
                        WriteLog.Info("Saving Watchlist..");
                        WatchListVM.StoreWatchListItemsSymbols();
                    }
                }).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    WriteLog.Info("Unsubscribing Sockets..");
                    BTClient.DisposeClients();
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLog.Error("OnClosingError: ", ex);
            }
            finally
            {
                WriteLog.Info("Exiting, Goodbye..");
                Environment.Exit(0);
            }
        }

        private void OnExitMainWindow(object o)
        {
            if (Static.MessageBox.ShowMessage("Are you Sure you want to Exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning, true) == MessageBoxResult.Yes)
            {
                Application.Current.MainWindow.Close();
            }
        }

        #endregion [Closing]

        #region [Controls]

        /// <summary>
        /// Place an Order to Buy manually with the Buy button
        /// </summary>
        /// <param name="o"></param>
        public void Buy(object o)
        {
            Trade.Buy(TradeVM, BorrowVM);
        }

        /// <summary>
        /// Place an Order to Sell to sell manually with the Sell button
        /// </summary>
        /// <param name="o"></param>
        public void Sell(object o)
        {
            Trade.Sell(TradeVM, BorrowVM);
        }

        private void CopySelectedItemToClipboard(object o)
        {
            if (IsListValidTarget())
            {
                var s = SelectedListItem;

                string copy =
                    "SIDE: " + s.Side +
                    " | PRICE: " + s.Price +
                    " | FILL: " + s.QuantityFilled + "/" + s.Quantity +
                    " | TYPE: " + s.Type +
                    " | PNL: " + s.Pnl +
                    " | OID: " + s.OrderId +
                    " | TIME: " + s.CreateTime +
                    " | STAT: " + s.Status +
                    " | TIF: " + s.TimeInForce.ToString() +
                    " | IPD: " + s.IPD +
                    " | IPH: " + s.IPH +
                    " | ITD: " + s.ITD;

                Clipboard.SetText(copy);
            }
        }

        private void ToggleConsole(object o)
        {
            if (!ConsoleAllocator.ConsoleWindowOpen)
            {
                ConsoleAllocator.ShowConsoleWindow();
            }
            else
            {
                ConsoleAllocator.HideConsoleWindow();
            }
        }

        private void GotFocus(object o)
        {
            Static.IsListFocus = true;
        }

        private void LostFocus(object o)
        {
            Static.IsListFocus = false;
        }

        public bool IsListValidTarget()
        {
            return SelectedListItem != null && Static.IsListFocus;
        }

        private void ToggleAlerts(object o)
        {
            if (AlertsView == null)
            {
                AlertsView = new AlertsView(M);
                AlertsView.Show();
            }
            else
            {
                AlertsView?.Close();
                AlertsView = null;
            }
        }

        private void ToggleSettings(object o)
        {
            if (SettingsView == null)
            {
                SettingsView = new SettingsView(M);
                SettingsView.Show();
            }
            else
            {
                SettingsView?.Close();
                SettingsView = null;
            }
        }

        private void ToggleWatchlist(object o)
        {
            if (WatchlistView == null)
            {
                WatchlistView = new WatchlistView(M);
                WatchlistView.Show();
            }
            else
            {
                WatchlistView?.Close();
                WatchlistView = null;
            }
        }

        private void ToggleStatusView(object o)
        {
            if (StatusView == null)
            {
                StatusView = new StatusView(M);
                StatusView.Show();
            }
            else
            {
                StatusView?.Close();
                StatusView = null;
            }
        }

        private void ToggleAboutView(object o)
        {
            if (AboutView == null)
            {
                AboutView = new AboutView(M);
                AboutView.Show();
            }
            else
            {
                AboutView?.Close();
                AboutView = null;
            }
        }

        private void ToggleNotepadView(object o)
        {
            if (NotepadView == null)
            {
                NotepadView = new NotepadView(M);
                NotepadView.Show();
            }
            else
            {
                NotepadView?.Close();
                NotepadView = null;
                NotepadVM.SaveNotes();
            }
        }

        public void HideSymbolMenu(object o)
        {
            if (HideSideMenu == 200)
            {
                HideSideMenu = 0;

                if ((Controls.CanvasControl.BorrowBoxLeft + 200) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.BorrowBoxLeft < -200)
                {
                    _ = ResetBorrowBox().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.MarginBoxLeft + 200) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.MarginBoxLeft < -200)
                {
                    _ = ResetMarginInfo().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.RealTimeLeft + 200) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.RealTimeLeft < -5)
                {
                    _ = ResetRealTime().ConfigureAwait(false);
                }
            }
            else
            {
                if ((Controls.CanvasControl.BorrowBoxLeft + 20) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.BorrowBoxLeft < -200)
                {
                    _ = ResetBorrowBox().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.MarginBoxLeft + 20) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.MarginBoxLeft < -200)
                {
                    _ = ResetMarginInfo().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.RealTimeLeft + 20) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.RealTimeLeft < -5)
                {
                    _ = ResetRealTime().ConfigureAwait(false);
                }

                HideSideMenu = 200;
            }
        }

        public Task ResetControlPositions()
        {
            _ = ResetRealTime().ConfigureAwait(false);
            _ = ResetBorrowBox().ConfigureAwait(false);
            _ = ResetMarginInfo().ConfigureAwait(false);
            OrderPositionReset = new GridLength(80);

            return Task.CompletedTask;
        }

        public async Task ResetRealTime()
        {
            await Task.Delay(500);
            ResetRealTimeLeft = 1055;
            ResetRealTimeTop = -400;
        }

        public async Task ResetBorrowBox()
        {
            await Task.Delay(500);
            ResetBorrowBoxLeft = 10;
            ResetBorrowBoxTop = -170;
        }

        public async Task ResetMarginInfo()
        {
            await Task.Delay(500);
            ResetMarginInfoLeft = 110;
            ResetMarginInfoTop = -170;
        }

        public double resetRealTimeLeft = 1055;
        public double resetRealTimeTop = -400;

        public double ResetRealTimeLeft
        { get => resetRealTimeLeft; set { resetRealTimeLeft = value; PC(); } }

        public double ResetRealTimeTop
        { get => resetRealTimeTop; set { resetRealTimeTop = value; PC(); } }

        public double resetBorrowBoxLeft = 10;
        public double resetBorrowBoxTop = -170;

        public double ResetBorrowBoxLeft
        { get => resetBorrowBoxLeft; set { resetBorrowBoxLeft = value; PC(); } }

        public double ResetBorrowBoxTop
        { get => resetBorrowBoxTop; set { resetBorrowBoxTop = value; PC(); } }

        private double resetMarginInfoLeft = 110;
        private double resetMarginInfoTop = -170;

        public double ResetMarginInfoLeft
        { get => resetMarginInfoLeft; set { resetMarginInfoLeft = value; PC(); } }

        public double ResetMarginInfoTop
        { get => resetMarginInfoTop; set { resetMarginInfoTop = value; PC(); } }

        public GridLength orderPositionReset = new GridLength(80);

        public GridLength OrderPositionReset
        { get => orderPositionReset; set { orderPositionReset = value; PC(); } }

        #endregion [Controls]
    }
}