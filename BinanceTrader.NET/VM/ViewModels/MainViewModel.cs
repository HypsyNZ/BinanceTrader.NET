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
using BinanceAPI.Objects;
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.HELPERS;
using BTNET.Views;
using ExchangeAPI.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public AlertsView AlertsView;
        public WatchlistView WatchlistView;
        public SettingsView SettingsView;

        private string chart;
        private bool isCurrentlyLoading = false;
        private bool isWatchlistStillLoading = false;

        private Main M = null;

        public MainViewModel(Main m)
        {
            M = m;
        }

        #region [Commands]

        public void InitializeCommands()
        {
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
            CloseWindowCommand = new DelegateCommand(OnClosing);
            LostFocusCommand = new DelegateCommand(LostFocus);
            GotFocusCommand = new DelegateCommand(GotFocus);
            CopySelectedItemToClipboardCommand = new DelegateCommand(CopySelectedItemToClipboard);
            ExitMainWindowCommand = new DelegateCommand(OnExitMainWindow);
            BuyCommand = new DelegateCommand(Buy);
            SellCommand = new DelegateCommand(Sell);

            SettingsCommand = new DelegateCommand(Settings);
            WatchlistCommand = new DelegateCommand(ShowWatchlist);
            AlertsCommand = new DelegateCommand(ShowAlerts);

            CloseSettingsCommand = new DelegateCommand(CloseSettings);
            CloseAlertsCommand = new DelegateCommand(CloseAlerts);
            CloseWatchlistCommand = new DelegateCommand(CloseWatchlist);
            HideMenuCommand = new DelegateCommand(HideSymbolMenu);
        }

        public ICommand SettingsCommand { get; set; }
        public ICommand AlertsCommand { get; set; }
        public ICommand WatchlistCommand { get; set; }
        public ICommand CloseSettingsCommand { get; set; }
        public ICommand CloseAlertsCommand { get; set; }
        public ICommand CloseWatchlistCommand { get; set; }
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

        public bool IsCurrentlyLoading
        { get => isCurrentlyLoading || IsWatchlistStillLoading; set { isCurrentlyLoading = value; LoadingText = ""; PC(); } }

        public bool IsWatchlistStillLoading
        { get => isWatchlistStillLoading; set { isWatchlistStillLoading = value; PC(); PC("IsCurrentlyLoading"); } }

        #endregion [ Loading ]

        private int hidesidemenu = 0;
        private bool isIsolated = false, isMargin = false;

        public bool IsIsolated
        { get => isIsolated; set { isIsolated = value; PC(); } }

        public bool IsMargin
        { get => isMargin; set { isMargin = value; PC(); } }

        public string MiniLogOut
        { get => MiniLog.MiniLogString; set { PC(); } }

        public string Chart
        { get => this.chart; set { this.chart = value; PC(); } }

        public int HideSideMenu
        { get => this.hidesidemenu; set { this.hidesidemenu = value; PC(); } }

        public string ApiSecret
        { get => this.UserApiKeys.ApiSecret; set { this.UserApiKeys.ApiSecret = value; PC(); } }

        public string ApiKey
        { get => this.UserApiKeys.ApiKey; set { this.UserApiKeys.ApiKey = value; PC(); } }

        private ObservableCollection<BinanceSymbolViewModel> allPrices;

        public ObservableCollection<BinanceSymbolViewModel> AllSymbolsOnUI
        { get => this.allPrices; set { this.allPrices = value; PC(); } }

        public static int SelectedTabUI
        { get => (int)Static.CurrentlySelectedSymbolTab; set { Static.CurrentlySelectedSymbolTab = (SelectedTab)value; } }

        private void GotFocus(object o)
        {
            Static.IsListFocus = true;
        }

        private void LostFocus(object o)
        {
            Static.IsListFocus = false;
        }

        private void SaveSettings(object o)
        {
            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
            {
                Directory.CreateDirectory(Static.SettingsPath);
                File.WriteAllText(Static.settingsfile, JsonConvert.SerializeObject(this.UserApiKeys));
                BinanceClient.SetDefaultOptions(new BinanceClientOptions() { ApiCredentials = new ApiCredentials(ApiKey, ApiSecret) });

                BTClient.Local.SetApiCredentials(ApiKey, ApiSecret);
                BTClient.SocketClient.SetApiCredentials(ApiKey, ApiSecret);
                BTClient.SocketSymbolTicker.SetApiCredentials(ApiKey, ApiSecret);

                Static.MessageBox.ShowMessage(@"API Key and API Secret were saved to File [C:\BNET\Settings\keys.json]", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Static.MessageBox.ShowMessage("Please enter your API Key and your API Secret", "API Key Missing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region [Closing]

        private async void OnClosing(object o)
        {
            WriteLog.Info("Shutting Down..");

            Sink.Stop();
            SinkRT.Stop();

            try
            {
                foreach (Window w in Application.Current.Windows)
                {
                    w.Hide();
                    IsSymbolSelected = false;
                }

                await Task.Run(() =>
                {
                    WriteLog.Info("Storing Orders..");
                    StoreList.StoreListLong(Static.DeletedList, Static.listofdeletedorders);
                    Static.ManageStoredOrders.AddCurrentOrdersToCollections(Static.GetOrders);
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
            var ms = Static.MessageBox.ShowMessage("Are you Sure you want to Exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (ms == MessageBoxResult.Yes)
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

        public bool IsListValidTarget()
        {
            return SelectedListItem != null && Static.IsListFocus;
        }

        private void ShowAlerts(object o)
        {
            if (AlertsView == null)
            {
                AlertsView = new AlertsView(M);
                AlertsView.Show();
            }
        }

        private void Settings(object o)
        {
            SettingsView = new SettingsView(M);
            _ = SettingsView.ShowDialog();
        }

        private void ShowWatchlist(object o)
        {
            if (WatchlistView == null)
            {
                WatchlistView = new WatchlistView(M);
                WatchlistView.Show();
            }
        }

        public void HideSymbolMenu(object o)
        {
            if (HideSideMenu == 200) { HideSideMenu = 0; } else { HideSideMenu = 200; PC(); }
        }

        private void CloseAlerts(object o)
        {
            AlertsView?.Close();
            AlertsView = null;
        }

        private void CloseSettings(object o)
        {
            SettingsView?.Close();
            SettingsView = null;
        }

        private void CloseWatchlist(object o)
        {
            WatchlistView?.Close();
            WatchlistView = null;
        }

        #endregion [Controls]
    }
}