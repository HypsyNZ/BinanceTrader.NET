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

using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using BTNET.VM.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const int DELAY = 200;

        private const int MAIN_MAX_OPACITY = 100;
        private const int MAIN_MIN_OPACITY = 0;

        private const int CANVAS_OVERFLOW = 200;
        private const int CANVAS_UNDERFLOW = -200;
        private const int CANVAS_OVERFLOW_SMALL = 20;
        private const int CANVAS_UNDERFLOW_SMALL = -5;

        private const int RESET_MARGIN_INFO_LEFT = 113;
        private const int RESET_MARGIN_INFO_TOP = -170;

        private const int RESET_REAL_TIME_LEFT = 1055;
        private const int RESET_REAL_TIME_TOP = -400;

        private const int RESET_BORROW_BOX_LEFT = 10;
        private const int RESET_BORROW_BOX_TOP = -170;

        private const int ORDER_PANEL_HEIGHT_RESET = 80;

        private int showMain = 0;

        private bool searchEnabled;
        private static bool isSymbolSelected;

        private int listViewControlHeightOffset = App.ORDER_LIST_MAX_HEIGHT_OFFSET_NORMAL;
        private int hidesidemenu;
        private string? chart;
        private bool isIsolated;
        private bool isMargin;
        private bool isWatchlistStillLoading;
        private bool isCurrentlyLoading;
        private bool shouldSuspendSymbolSelection = true;
        private bool alertsReady = true;

        private double listViewControlMaxHeight = ORDER_PANEL_HEIGHT_RESET;
        private double resetRealTimeLeft = RESET_REAL_TIME_LEFT;
        private double resetRealTimeTop = RESET_REAL_TIME_TOP;
        private double resetBorrowBoxLeft = RESET_BORROW_BOX_LEFT;
        private double resetBorrowBoxTop = RESET_BORROW_BOX_TOP;
        private double resetMarginInfoLeft = RESET_MARGIN_INFO_LEFT;
        private double resetMarginInfoTop = RESET_MARGIN_INFO_TOP;

        private GridLength orderPositionReset = new(ORDER_PANEL_HEIGHT_RESET);

        private readonly MainContext? M;
        private ObservableCollection<BinanceSymbolViewModel>? allPrices;
        private bool savingsEnabled;
        private bool buyButtonEnabled;
        private bool sellButtonEnabled;
        private bool notepadReady = false;

        public MainViewModel(MainContext m) => M = m;

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

            ToggleFlexibleCommand = new DelegateCommand(ToggleFlexibleView);
            ToggleSettingsCommand = new DelegateCommand(ToggleSettingsView);
            ToggleWatchlistCommand = new DelegateCommand(ToggleWatchlistView);
            ToggleAboutViewCommand = new DelegateCommand(ToggleAboutView);
            ToggleNotepadViewCommand = new DelegateCommand(ToggleNotepadView);
            ToggleAlertsCommand = new DelegateCommand(ToggleAlertsView);
            ToggleLogCommand = new DelegateCommand(ToggleLogView);
            HideMenuCommand = new DelegateCommand(HideSymbolMenu);
        }

        public ICommand? ToggleFlexibleCommand { get; set; }
        public ICommand? ToggleLogCommand { get; set; }
        public ICommand? ToggleSettingsCommand { get; set; }
        public ICommand? ToggleAlertsCommand { get; set; }
        public ICommand? ToggleWatchlistCommand { get; set; }
        public ICommand? ToggleStratViewCommand { get; set; }
        public ICommand? ToggleAboutViewCommand { get; set; }
        public ICommand? ToggleNotepadViewCommand { get; set; }

        public ICommand? HideMenuCommand { get; set; }

        public ICommand? BuyCommand { get; set; }
        public ICommand? SellCommand { get; set; }
        public ICommand? SaveSettingsCommand { get; set; }

        public ICommand? CloseWindowCommand { get; set; }
        public ICommand? LostFocusCommand { get; set; }
        public ICommand? GotFocusCommand { get; set; }
        public ICommand? CopySelectedItemToClipboardCommand { get; set; }
        public ICommand? ExitMainWindowCommand { get; set; }

        #endregion [Commands]

        #region [ Loading ]

        public string LoadingText
        {
            get
            {
                if (isCurrentlyLoading && IsWatchlistStillLoading)
                {
                    return "Loading..";
                }

                if (IsWatchlistStillLoading)
                {
                    return "Connecting..";
                }

                if ((SettingsVM.CheckForUpdatesIsChecked ?? false) == true)
                {
                    if (SettingsVM.IsUpToDate != "You are using the most recent version")
                    {
                        return SettingsVM.IsUpToDate;
                    }
                }

                return "Loading..";
            }

            set
            {
                PC();
            }
        }

        public bool ShouldSuspendSymbolSelection
        {
            get => shouldSuspendSymbolSelection;
            set
            {
                shouldSuspendSymbolSelection = value;
                PC();
            }
        }

        public bool IsCurrentlyLoading
        {
            get => isCurrentlyLoading || IsWatchlistStillLoading || SettingsVM.IsUpToDate.Contains("Update:");
            set
            {
                isCurrentlyLoading = value;
                PC();
                LoadingText = "";
            }
        }

        public bool IsWatchlistStillLoading
        {
            get => isWatchlistStillLoading;
            set
            {
                isWatchlistStillLoading = value;
                PC();
                LoadingText = "";
                PC("IsCurrentlyLoading");
            }
        }

        public bool SearchEnabled
        {
            get => this.searchEnabled;
            set
            {
                this.searchEnabled = value;
                PC();
            }
        }

        public bool IsSymbolSelected
        {
            get => isSymbolSelected;
            set
            {
                MainVM.ShowMain = !value ? MAIN_MIN_OPACITY : MAIN_MAX_OPACITY;
                isSymbolSelected = value;
                Static.GetIsSymbolSelected = value;
                PC();
            }
        }

        public bool AlertsReady
        {
            get => alertsReady;
            set
            {
                alertsReady = value;
                PC();
            }
        }

        public bool NotepadReady
        {
            get => notepadReady;
            set
            {
                notepadReady = value;
                PC();
            }
        }

        #endregion [ Loading ]

        public double ListViewControlMaxHeight
        {
            get => listViewControlMaxHeight;
            set
            {
                listViewControlMaxHeight = value;
                PC();
            }
        }

        public int ListViewControlHeightOffset
        {
            get => listViewControlHeightOffset;
            set
            {
                listViewControlHeightOffset = value;
                PC();
            }
        }

        public bool BuyButtonEnabled
        {
            get => buyButtonEnabled;
            set
            {
                buyButtonEnabled = value;
                PC();
            }
        }

        public bool SellButtonEnabled
        {
            get => sellButtonEnabled;
            set
            {
                sellButtonEnabled = value;
                PC();
            }
        }

        public bool SavingsEnabled
        {
            get => savingsEnabled; set
            {
                savingsEnabled = value;
                PC();
            }
        }

        public int ShowMain
        {
            get => showMain;
            set
            {
                showMain = value;
                PC();
            }
        }

        public bool IsIsolated
        {
            get => isIsolated;
            set
            {
                isIsolated = value;
                PC();
            }
        }

        public bool IsMargin
        {
            get => isMargin;
            set
            {
                isMargin = value;
                PC();
            }
        }

        public string? Chart
        {
            get => this.chart;
            set
            {
                this.chart = value;
                PC();
            }
        }

        public int HideSideMenu
        {
            get => this.hidesidemenu;
            set
            {
                this.hidesidemenu = value;
                PC();
            }
        }

        public ObservableCollection<BinanceSymbolViewModel>? AllSymbolsOnUI
        {
            get => this.allPrices;
            set
            {
                this.allPrices = value;
                PC();
            }
        }

        public int SelectedTabUI
        {
            get => (int)Static.CurrentlySelectedSymbolTab;
            set
            {
                Static.CurrentlySelectedSymbolTab = (SelectedTab)value;
                PC();

                App.TabChanged?.Invoke(null, null);
            }
        }

        #region [Closing]

        private void OnClosing(object o)
        {
            try
            {
                WriteLog.Info("Shutting Down..");

                foreach (Window w in Application.Current.Windows)
                {
                    w.Hide();
                    IsSymbolSelected = false;
                }

                try
                {
                    Timers?.DisposeWatchdog();
                    Sink?.Stop();
                    NotepadVM?.SaveNotes();
                }
                catch
                {
                    // Shutting Down
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        Invoke.InvokeUI(() =>
                        {
                            CefSharp.Cef.ShutdownWithoutChecks();
                        });
                    }
                    catch
                    {
                        // Shutting Down
                    }
                });

                try
                {
                    if (Static.IsStarted)
                    {
                        WriteLog.Info("Saving Settings");
                        Directory.CreateDirectory(App.SettingsPath);
                        Directory.CreateDirectory(App.OrderPath);
                        SettingsVM.SaveOnClose();

                        WriteLog.Info("Saving Alerts..");
                        TJson.Save(AlertVM.Alerts.ToList(), App.StoredAlerts);

                        WriteLog.Info("Storing Orders..");
                        TJson.Save(Static.DeletedList, App.Listofdeletedorders);
                        Static.ManageStoredOrders.AddOrderUpdatesToMemoryStorage(Orders.Current.ToList());
                        Static.ManageStoredOrders.WriteAllOrderCollectionsToFileStorage();

                        WriteLog.Info("Storing Quotes..");
                        Quotes.AddStoredQuote();
                        TJson.Save(Stored.Quotes, App.StoredQuotes);

                        if (!IsWatchlistStillLoading)
                        {
                            WriteLog.Info("Saving Watchlist..");
                            WatchListVM.StoreWatchListItemsSymbols();
                        }
                    }
                }
                catch
                {
                    // Shutting Down
                }

                try
                {
                    WriteLog.Info("Unsubscribing Sockets..");
                    Client?.DisposeClients();
                }
                catch
                {
                    // Shutting Down
                }
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
            if (Message.ShowBox("Are you Sure you want to Exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning, waitForReply: true) == MessageBoxResult.Yes)
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
            // Buy
            Trade.Buy();
        }

        /// <summary>
        /// Place an Order to Sell to sell manually with the Sell button
        /// </summary>
        /// <param name="o"></param>
        public void Sell(object o)
        {
            // Sell
            Trade.Sell();
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
                    " | IPD: " + s.InterestPerDay +
                    " | IPH: " + s.InterestPerHour +
                    " | ITD: " + s.InterestToDate;

                Clipboard.SetText(copy);
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

        private void ToggleAlertsView(object o)
        {
            if (AlertsView == null)
            {
                AlertsView = new AlertsView(M!);
                AlertsView.Show();
            }
            else
            {
                if (!AlertsView.IsLoaded)
                {
                    AlertsView = new AlertsView(M!);
                    AlertsView.Show();
                    return;
                }

                AlertVM.ToggleAlertSideMenu = 0;
                AlertsView?.Close();
                AlertsView = null;
            }
        }

        private void ToggleSettingsView(object o)
        {
            if (SettingsView == null)
            {
                SettingsView = new SettingsView(M!);
                SettingsView.Show();
            }
            else
            {
                if (!SettingsView.IsLoaded)
                {
                    SettingsView = new SettingsView(M!);
                    SettingsView.Show();
                    return;
                }

                SettingsView?.Close();
                SettingsView = null;
            }
        }

        private void ToggleFlexibleView(object o)
        {
            if (FlexibleView == null)
            {
                FlexibleView = new FlexibleView(M!);

                _ = Task.Run(async () =>
                {
                    await FlexibleVM.GetAllProductsAsync().ConfigureAwait(false);
                    await FlexibleVM.GetAllPositionsAsync(false).ConfigureAwait(false);
                });

                FlexibleView.Show();
            }
            else
            {
                if (!FlexibleView.IsLoaded)
                {
                    FlexibleView = new FlexibleView(M!);
                    FlexibleView.Show();
                    return;
                }

                FlexibleView?.Close();
                FlexibleView = null;
            }

            FlexibleVM.ClearSelected();
        }

        private void ToggleWatchlistView(object o)
        {
            if (WatchlistView == null)
            {
                WatchlistView = new WatchlistView(M!);
                WatchlistView.Show();
            }
            else
            {
                if (!WatchlistView.IsLoaded)
                {
                    WatchlistView = new WatchlistView(M!);
                    WatchlistView.Show();
                    return;
                }

                WatchlistView?.Close();
                WatchlistView = null;
            }
        }

        private void ToggleAboutView(object o)
        {
            if (AboutView == null)
            {
                AboutView = new AboutView(M!);
                AboutView.Show();
            }
            else
            {
                if (!AboutView.IsLoaded)
                {
                    AboutView = new AboutView(M!);
                    AboutView.Show();
                    return;
                }

                AboutView?.Close();
                AboutView = null;
            }
        }

        private void ToggleNotepadView(object o)
        {
            if (NotepadView == null)
            {
                NotepadView = new NotepadView(M!);
                NotepadView.Show();
            }
            else
            {
                if (!NotepadView.IsLoaded)
                {
                    NotepadView = new NotepadView(M!);
                    NotepadView.Show();
                    return;
                }

                NotepadView?.Close();
                NotepadView = null;
                NotepadVM.SaveNotes();
            }
        }

        private void ToggleLogView(object o)
        {
            if (LogView == null)
            {
                LogView = new LogView(M!);
                LogView.Show();
            }
            else
            {
                if (!LogView.IsLoaded)
                {
                    LogView = new LogView(M!);
                    LogView.Show();
                    return;
                }

                LogView?.Close();
                LogView = null;
            }
        }

        public void HideSymbolMenu(object o)
        {
            if (HideSideMenu == CANVAS_OVERFLOW)
            {
                HideSideMenu = 0;

                if ((Controls.CanvasControl.BorrowBoxLeft + CANVAS_OVERFLOW) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.BorrowBoxLeft < CANVAS_UNDERFLOW)
                {
                    _ = ResetBorrowBoxAsync().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.MarginBoxLeft + CANVAS_OVERFLOW) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.MarginBoxLeft < CANVAS_UNDERFLOW)
                {
                    _ = ResetMarginInfoAsync().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.RealTimeLeft + CANVAS_OVERFLOW) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.RealTimeLeft < CANVAS_UNDERFLOW_SMALL)
                {
                    _ = ResetRealTimeAsync().ConfigureAwait(false);
                }
            }
            else
            {
                if ((Controls.CanvasControl.BorrowBoxLeft + CANVAS_OVERFLOW_SMALL) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.BorrowBoxLeft < CANVAS_UNDERFLOW)
                {
                    _ = ResetBorrowBoxAsync().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.MarginBoxLeft + CANVAS_OVERFLOW_SMALL) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.MarginBoxLeft < CANVAS_UNDERFLOW)
                {
                    _ = ResetMarginInfoAsync().ConfigureAwait(false);
                }

                if ((Controls.CanvasControl.RealTimeLeft + CANVAS_OVERFLOW_SMALL) > Controls.CanvasControl.CanvasActualWidth || Controls.CanvasControl.RealTimeLeft < CANVAS_UNDERFLOW_SMALL)
                {
                    _ = ResetRealTimeAsync().ConfigureAwait(false);
                }

                HideSideMenu = CANVAS_OVERFLOW;
            }
        }

        public Task ResetListViewMaxHeightAsync()
        {
            Invoke.InvokeUI(() =>
            {
                var d = App.Current.MainWindow.ActualHeight - ListViewControlHeightOffset;
                switch (d)
                {
                    case >= 0:
                        ListViewControlMaxHeight = d;
                        break;

                    default:
                        d = App.Current.MainWindow.ActualHeight;
                        if (d > 0)
                        {
                            ListViewControlMaxHeight = d;
                        }
                        break;
                }
            });

            return Task.CompletedTask;
        }

        public async Task ResetControlPositionsAsync()
        {
            await Task.Delay(DELAY).ConfigureAwait(false);
            _ = ResetRealTimeAsync().ConfigureAwait(false);
            _ = ResetBorrowBoxAsync().ConfigureAwait(false);
            _ = ResetMarginInfoAsync().ConfigureAwait(false);
            _ = ResetListViewMaxHeightAsync().ConfigureAwait(false);

            OrderPositionReset = new GridLength(ORDER_PANEL_HEIGHT_RESET);
        }

        public async Task ResetRealTimeAsync()
        {
            await Task.Delay(DELAY).ConfigureAwait(false);
            ResetRealTimeLeft = RESET_REAL_TIME_LEFT;
            ResetRealTimeTop = RESET_REAL_TIME_TOP;
        }

        public async Task ResetBorrowBoxAsync()
        {
            await Task.Delay(DELAY).ConfigureAwait(false);
            ResetBorrowBoxLeft = RESET_BORROW_BOX_LEFT;
            ResetBorrowBoxTop = RESET_BORROW_BOX_TOP;
        }

        public async Task ResetMarginInfoAsync()
        {
            await Task.Delay(DELAY).ConfigureAwait(false);
            ResetMarginInfoLeft = RESET_MARGIN_INFO_LEFT;
            ResetMarginInfoTop = RESET_MARGIN_INFO_TOP;
        }

        public double ResetRealTimeLeft
        {
            get => resetRealTimeLeft;
            set
            {
                resetRealTimeLeft = value;
                PC();
            }
        }

        public double ResetRealTimeTop
        {
            get => resetRealTimeTop;
            set
            {
                resetRealTimeTop = value;
                PC();
            }
        }

        public double ResetBorrowBoxLeft
        {
            get => resetBorrowBoxLeft;
            set
            {
                resetBorrowBoxLeft = value;
                PC();
            }
        }

        public double ResetBorrowBoxTop
        {
            get => resetBorrowBoxTop;
            set
            {
                resetBorrowBoxTop = value;
                PC();
            }
        }

        public double ResetMarginInfoLeft
        {
            get => resetMarginInfoLeft;
            set
            {
                resetMarginInfoLeft = value;
                PC();
            }
        }

        public double ResetMarginInfoTop
        {
            get => resetMarginInfoTop;
            set
            {
                resetMarginInfoTop = value;
                PC();
            }
        }

        public GridLength OrderPositionReset
        {
            get => orderPositionReset;
            set
            {
                orderPositionReset = value;
                PC();
            }
        }

        public AlertsView? AlertsView { get; set; }
        public WatchlistView? WatchlistView { get; set; }
        public SettingsView? SettingsView { get; set; }
        public NotepadView? NotepadView { get; set; }
        public AboutView? AboutView { get; set; }
        public FlexibleView? FlexibleView { get; set; }
        public LogView? LogView { get; set; }

        #endregion [Controls]
    }
}
