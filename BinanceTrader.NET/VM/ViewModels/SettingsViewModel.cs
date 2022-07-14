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

using BinanceAPI.ClientBase;
using BTNET.BV.Abstract;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using Identity.NET;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private const int BROWSER_STRETCH = 3;
        private const int BROWSER_FILL = 2;

        #region [Commands]

        public void InitializeCommands()
        {
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
            ChangeSettingsCommand = new DelegateCommand(ChangeSettings);
            ShowBorrowInfoCommand = new DelegateCommand(ShowBorrowInfo);
            ShowMarginInfoCommand = new DelegateCommand(ShowMarginInfo);
            ShowSymbolInfoCommand = new DelegateCommand(ShowSymbolInfo);
            ShowIsolatedInfoCommand = new DelegateCommand(ShowIsolatedInfo);
            ShowBreakDownInfoCommand = new DelegateCommand(ShowBreakDownInfo);
            StretchBrowserCommand = new DelegateCommand(StretchBrowserToFit);
            CheckForUpdatesCommand = new DelegateCommand(CheckForUpdate);
        }

        public ICommand? StretchBrowserCommand { get; set; }

        public ICommand? ShowIsolatedInfoCommand { get; set; }
        public ICommand? ShowSymbolInfoCommand { get; set; }
        public ICommand? ShowBorrowInfoCommand { get; set; }
        public ICommand? ShowMarginInfoCommand { get; set; }

        public ICommand? ShowBreakDownInfoCommand { get; set; }

        public ICommand? SaveSettingsCommand { get; set; }
        public ICommand? ChangeSettingsCommand { get; set; }

        public ICommand? CheckForUpdatesCommand { get; set; }

        #endregion [Commands]

        internal bool ApiKeysSet;
        private bool saveEnabled;
        private bool changeEnabled;
        private bool apiKeyEnabled = true;
        private bool apiSecretEnabled = true;
        private bool? showSymbolInfoIsChecked = true;
        private bool? showBorrowInfoIsChecked = true;
        private bool? showMarginInfoIsChecked = true;
        private bool? showIsolatedInfoIsChecked = true;
        private bool? showBreakDownInfoIsChecked = true;
        private bool? stretchBrowserIsChecked = true;
        private bool? checkForUpdatesIsChecked = false;
        private double? orderOpacity = 0.7;
        private int stretchBrowser = 3;
        private bool checkForUpdateCheckBoxEnabled;
        private string isUpToDate = "";

        public string IsUpToDate
        {
            get => isUpToDate;
            set
            {
                isUpToDate = value;
                PC();
            }
        }

        public string ApiSecret
        {
            get => UserApiKeys.ApiSecret;
            set
            {
                UserApiKeys.ApiSecret = value;
                PC();
            }
        }

        public string ApiKey
        {
            get => UserApiKeys.ApiKey;
            set
            {
                UserApiKeys.ApiKey = value;
                PC();
            }
        }

        public bool ApiKeyEnabled
        {
            get => apiKeyEnabled;
            set
            {
                apiKeyEnabled = value;
                PC();
            }
        }

        public bool ApiSecretEnabled
        {
            get => apiSecretEnabled;
            set
            {
                apiSecretEnabled = value;
                PC();
            }
        }

        public bool SaveEnabled
        {
            get => saveEnabled;
            set
            {
                saveEnabled = value;
                PC();
            }
        }

        public bool ChangeEnabled
        {
            get => changeEnabled;
            set
            {
                changeEnabled = value;
                PC();
            }
        }

        public bool? ShowSymbolInfoIsChecked
        {
            get => showSymbolInfoIsChecked;
            set
            {
                showSymbolInfoIsChecked = value;
                PC();
            }
        }

        public bool? ShowBorrowInfoIsChecked
        {
            get => showBorrowInfoIsChecked;
            set
            {
                showBorrowInfoIsChecked = value;
                PC();
            }
        }

        public bool? ShowMarginInfoIsChecked
        {
            get => showMarginInfoIsChecked;
            set
            {
                showMarginInfoIsChecked = value;
                PC();
            }
        }

        public bool? ShowIsolatedInfoIsChecked
        {
            get => showIsolatedInfoIsChecked;
            set
            {
                showIsolatedInfoIsChecked = value;
                PC();
            }
        }

        public bool? ShowBreakDownInfoIsChecked
        {
            get => showBreakDownInfoIsChecked;
            set
            {
                showBreakDownInfoIsChecked = value;
                PC();
            }
        }

        public bool? StretchBrowserIsChecked
        {
            get => stretchBrowserIsChecked;
            set
            {
                stretchBrowserIsChecked = value;
                PC();
            }
        }

        public bool? CheckForUpdatesIsChecked
        {
            get => checkForUpdatesIsChecked;
            set
            {
                checkForUpdatesIsChecked = value;
                PC();
            }
        }

        public bool CheckForUpdateCheckBoxEnabled
        {
            get => checkForUpdateCheckBoxEnabled;
            set
            {
                checkForUpdateCheckBoxEnabled = value;
                PC();
            }
        }

        public double? OrderOpacity
        {
            get => orderOpacity;
            set
            {
                orderOpacity = value;
                PC();
            }
        }

        public int StretchBrowser
        {
            get => stretchBrowser;
            set
            {
                stretchBrowser = value;
                PC();
            }
        }

        private void StretchBrowserToFit(object o)
        {
            if (StretchBrowser == BROWSER_STRETCH)
            {
                StretchBrowserIsChecked = false;
                StretchBrowser = BROWSER_FILL;
            }
            else
            {
                StretchBrowserIsChecked = true;
                StretchBrowser = BROWSER_STRETCH;
            }
        }

        private void CheckForUpdate(object o)
        {
            CheckForUpdatesIsChecked = !CheckForUpdatesIsChecked;
            //   CheckForUpdates = CheckForUpdatesIsChecked ?? false;
            PC("CheckForUpdatesIsChecked");
        }

        private void ShowBreakDownInfo(object o)
        {
            ShowBreakDownInfoIsChecked = !ShowBreakDownInfoIsChecked;
            BorrowVM.ShowBreakdown = ShowBreakDownInfoIsChecked ?? false;
            PC("ShowBreakDownIsChecked");
        }

        private void ShowIsolatedInfo(object o)
        {
            ShowIsolatedInfoIsChecked = !ShowIsolatedInfoIsChecked;
            BorrowVM.IsolatedInfoVisible = showMarginInfoIsChecked ?? false;
            PC("ShowIsolatedInfoIsChecked");
        }

        private void ShowMarginInfo(object o)
        {
            ShowMarginInfoIsChecked = !ShowMarginInfoIsChecked;
            BorrowVM.MarginInfoVisible = ShowMarginInfoIsChecked ?? false;
            PC("ShowMarginInfoIsChecked");
        }

        private void ShowBorrowInfo(object o)
        {
            ShowBorrowInfoIsChecked = !ShowBorrowInfoIsChecked;
            BorrowVM.BorrowInfoVisible = ShowBorrowInfoIsChecked ?? false;
            PC("ShowBorrowInfoIsChecked");
        }

        private void ShowSymbolInfo(object o)
        {
            ShowSymbolInfoIsChecked = !ShowSymbolInfoIsChecked;
            PC("ShowSymbolInfoIsChecked");
        }

        private void ChangeSettings(object o)
        {
            Enable();
        }

        public void SaveOnClose()
        {
            SettingsObject settings = new(
                ShowBorrowInfoIsChecked ?? null,
                ShowSymbolInfoIsChecked ?? null,
                ShowBreakDownInfoIsChecked ?? null,
                ShowMarginInfoIsChecked ?? null,
                ShowIsolatedInfoIsChecked ?? null,
                OrderOpacity ?? null,
                StretchBrowserIsChecked ?? null,
                CheckForUpdatesIsChecked ?? null,
                TradeVM.UseBaseForQuoteBoolSell,
                TradeVM.UseLimitSellBool,
                BorrowVM.BorrowSell,
                TradeVM.UseBaseForQuoteBoolBuy,
                BorrowVM.BorrowBuy,
                TradeVM.UseLimitBuyBool
                );
            if (settings != null)
            {
                File.WriteAllText(App.Settings, JsonConvert.SerializeObject(settings));
            }
        }

        private void SaveSettings(object o)
        {
            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
            {
                if (ApiKey == "Key Loaded From File" || ApiSecret == "Key Loaded From File")
                {
                    return;
                }

                Directory.CreateDirectory(App.SettingsPath);

                // Encrypt API Keys with Identity
                ApiKey = UniqueIdentity.Encrypt(ApiKey);
                ApiSecret = UniqueIdentity.Encrypt(ApiSecret);

                // Write Encrypted keys to File
                File.WriteAllText(App.KeyFile, JsonConvert.SerializeObject(UserApiKeys));

                // Decrypt API Keys with Identity and store them as a Secure String (ID Test completed)
                BaseClient.SetAuthentication(UniqueIdentity.Decrypt(ApiKey), UniqueIdentity.Decrypt(ApiSecret));

                // Ensure API Keys aren't in the UI
                ApiKey = "Key Saved To File";
                ApiSecret = "Key Saved To File";

                MessageBox.Show(@"API Key and API Secret were saved to File [C:\BNET\Settings\keys.json]", "Keys Encrypted");

                // Disable UI Controls where keys were Set
                Disable();

                Settings.KeysLoaded = true;
                _ = MainContext.StartUserStreamsAsync().ConfigureAwait(false);
            }
            else
            {
                Settings.KeysLoaded = false;
                MessageBox.Show("Please enter your API Key and your API Secret", "API Key Missing");
                Enable();
            }
        }

        internal void Enable()
        {
            ApiKey = "";
            ApiSecret = "";
            ApiKeyEnabled = true;
            ApiSecretEnabled = true;
            ApiKeysSet = false;
            SaveEnabled = true;
            ChangeEnabled = false;
        }

        internal void Disable()
        {
            ApiKeyEnabled = false;
            ApiSecretEnabled = false;
            ApiKeysSet = true;
            SaveEnabled = false;
            ChangeEnabled = true;
        }
    }
}
