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
using BinanceAPI.Authentication;
using BinanceAPI.Objects;
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
        public SettingsViewModel()
        {
        }

        #region [Commands]

        public void InitializeCommands()
        {
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
            ChangeSettingsCommand = new DelegateCommand(ChangeSettings);
        }

        public ICommand SaveSettingsCommand { get; set; }
        public ICommand ChangeSettingsCommand { get; set; }

        #endregion [Commands]

        internal bool ApiKeysSet = false;
        private bool saveEnabled;
        private bool changeEnabled;
        private bool apiKeyEnabled = true;
        private bool apiSecretEnabled = true;

        public string ApiSecret
        { get => this.UserApiKeys.ApiSecret; set { this.UserApiKeys.ApiSecret = value; PC(); } }

        public string ApiKey
        { get => this.UserApiKeys.ApiKey; set { this.UserApiKeys.ApiKey = value; PC(); } }

        public bool ApiKeyEnabled
        { get => apiKeyEnabled; set { apiKeyEnabled = value; PC(); } }

        public bool ApiSecretEnabled
        { get => apiSecretEnabled; set { apiSecretEnabled = value; PC(); } }

        public bool SaveEnabled
        { get => saveEnabled; set { saveEnabled = value; PC(); } }

        public bool ChangeEnabled
        { get => changeEnabled; set { changeEnabled = value; PC(); } }

        private void ChangeSettings(object o)
        {
            Enable();
        }

        private void SaveSettings(object o)
        {
            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
            {
                if (ApiKey == "Key Loaded From File" || ApiSecret == "Key Loaded From File") { return; }

                Directory.CreateDirectory(Static.SettingsPath);

                ApiKey = UniqueIdentity.Encrypt(ApiKey);
                ApiSecret = UniqueIdentity.Encrypt(ApiSecret);

                File.WriteAllText(Static.settingsfile, JsonConvert.SerializeObject(this.UserApiKeys));

                BinanceClient.SetDefaultOptions(new BinanceClientOptions() { ApiCredentials = new ApiCredentials(UniqueIdentity.Decrypt(ApiKey).ToSecureString(), UniqueIdentity.Decrypt(ApiSecret).ToSecureString()) });

                ApiKey = "Key Saved To File";
                ApiSecret = "Key Saved To File";

                Static.MessageBox.ShowMessage(@"API Key and API Secret were saved to File [C:\BNET\Settings\keys.json]", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Disable();
            }
            else
            {
                Static.MessageBox.ShowMessage("Please enter your API Key and your API Secret", "API Key Missing", MessageBoxButton.OK, MessageBoxImage.Error);
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