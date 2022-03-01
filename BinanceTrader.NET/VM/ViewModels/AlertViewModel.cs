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

using BTNET.Base;
using BTNET.BVVM;
using BTNET.BVVM.HELPERS;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.ViewModels
{
    public class AlertViewModel : ObservableObject
    {
        public ICommand PlaySoundCommand { get; set; }
        public ICommand RepeatAlertCommand { get; set; }
        public ICommand AddAlertCommand { get; set; }
        public ICommand RemoveAlertCommand { get; set; }
        public ICommand ReverseBeforeAlertCommand { get; set; }

        public void InitializeCommands()
        {
            RemoveAlertCommand = new DelegateCommand(RemoveAlert);
            AddAlertCommand = new DelegateCommand(AddAlert);
            PlaySoundCommand = new DelegateCommand(PlaySound);
            RepeatAlertCommand = new DelegateCommand(RepeatAlert);
            ReverseBeforeAlertCommand = new DelegateCommand(ReverseBeforeAlert);
        }

        private string alertsymbol;
        private Intent alertIntent;
        private Direction alertDirection;
        private AlertItem selectedalert;

        private decimal alertprice = 0, alertinterval = 0;

        private bool repeatalert = false, playsound = false, reversefirst = false;

        public string AlertSymbol
        { get => alertsymbol; set { alertsymbol = value; PC(); } }

        public decimal AlertPrice
        { get => alertprice; set { alertprice = value; PC(); } }

        public decimal AlertInterval
        { get => alertinterval; set { alertinterval = value; PC(); } }

        public bool RepeatAlertBool
        { get => repeatalert; set { repeatalert = value; PC(); } }

        public bool PlaySoundBool
        { get => playsound; set { playsound = value; PC(); } }

        public bool ReverseFirstBool
        { get => reversefirst; set { reversefirst = value; PC(); } }

        public Direction AlertDirection
        { get => alertDirection; set { alertDirection = value; PC(); } }

        public Intent AlertIntent
        { get => alertIntent; set { alertIntent = value; PC(); } }

        public AlertItem SelectedAlert
        { get => selectedalert; set { selectedalert = value; PC(); } }

        private ObservableCollection<AlertItem> alerts = new();

        public ObservableCollection<AlertItem> Alerts
        { get => alerts; set { alerts = value; PC("Alerts"); } }

        public void AddAlert(object o)
        {
            if (!RepeatAlertBool) { AlertInterval = 0; }

            Alerts.Add(new AlertItem(AlertPrice,
                AlertSymbol, PlaySoundBool,
                RepeatAlertBool, AlertInterval, ReverseFirstBool,
                AlertIntent, AlertDirection,
                false, null));

            Alerts = new ObservableCollection<AlertItem>(Alerts.OrderByDescending(d => d.AlertSymbol));

            _ = Task.Run(() =>
            {
                MiniLog.AddLine("Added Alert..");
                WriteLog.Alert("-----------------------------Added Alert-----------------------------");
                WriteLog.Alert("Alert Price: " + AlertPrice + "| Alert Symbol: " + AlertSymbol + " | Repeat Interval:" + AlertInterval);
                WriteLog.Alert("Repeat Alert: " + RepeatAlertBool + " | Play Sound: " + PlaySoundBool + " | Reverse First: " + ReverseFirstBool);
                WriteLog.Alert("Intent: " + AlertIntent + " | Direction: " + AlertDirection);
                WriteLog.Alert("-----------------------------Added Alert-----------------------------");
            });
        }

        public void RemoveAlert(object o)
        {
            Invoke.InvokeUI(() =>
            {
                Alerts.Remove(SelectedAlert);
                MiniLog.AddLine("Removed Alert..");
            });
        }

        public void ReverseBeforeAlert(object o)
        {
            ReverseFirstBool = ReverseFirstBool != true && (ReverseFirstBool = true);
        }

        public void RepeatAlert(object o)
        {
            RepeatAlertBool = RepeatAlertBool != true && (RepeatAlertBool = true);
        }

        public void PlaySound(object o)
        {
            PlaySoundBool = PlaySoundBool != true && (PlaySoundBool = true);
        }
    }
}