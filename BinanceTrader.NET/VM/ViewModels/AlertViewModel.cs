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
using BTNET.BV.Enum;
using BTNET.BVVM;
using BTNET.BVVM.BT;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class AlertViewModel : ObservableObject
    {
        public ICommand? PlaySoundCommand { get; set; }
        public ICommand? RepeatAlertCommand { get; set; }
        public ICommand? AddAlertCommand { get; set; }
        public ICommand? RemoveAlertCommand { get; set; }
        public ICommand? ReverseBeforeAlertCommand { get; set; }

        public void InitializeCommands()
        {
            RemoveAlertCommand = new DelegateCommand(RemoveAlert);
            AddAlertCommand = new DelegateCommand(AddAlert);
            PlaySoundCommand = new DelegateCommand(PlaySound);
            RepeatAlertCommand = new DelegateCommand(RepeatAlert);
            ReverseBeforeAlertCommand = new DelegateCommand(ReverseBeforeAlert);
        }

        private string alertsymbol = "";
        private Direction alertDirection;
        private AlertItem? selectedalert;

        private decimal alertprice;
        private int alertinterval;

        private bool repeatalert;
        private bool playsound;
        private bool reversefirst;

        private ObservableCollection<AlertItem> alerts = new();
        private int toggleAlertSideMenu = 0;
        private bool toggleAlertEnabled = true;
        private AlertStatus alertStatus;

        public string AlertSymbol
        {
            get => alertsymbol;
            set
            {
                alertsymbol = value;
                PC();
            }
        }

        public decimal AlertPrice
        {
            get => alertprice;
            set
            {
                alertprice = value;
                PC();
            }
        }

        public int AlertInterval
        {
            get => alertinterval;
            set
            {
                alertinterval = value;
                PC();
            }
        }

        public bool RepeatAlertBool
        {
            get => repeatalert;
            set
            {
                repeatalert = value;
                PC();
            }
        }

        public bool PlaySoundBool
        {
            get => playsound;
            set
            {
                playsound = value;
                PC();
            }
        }

        public bool ReverseFirstBool
        {
            get => reversefirst;
            set
            {
                reversefirst = value;
                PC();
            }
        }

        public Direction AlertDirection
        {
            get => alertDirection;
            set
            {
                alertDirection = value;
                PC();
            }
        }

        public AlertStatus AlertStatus
        {
            get => alertStatus;
            set
            {
                alertStatus = value;
                PC();
            }
        }

        public AlertItem? SelectedAlert
        {
            get => selectedalert;
            set
            {
                selectedalert = value;
                PC();
            }
        }

        public ObservableCollection<AlertItem> Alerts
        {
            get => alerts;
            set
            {
                alerts = value;
                PC();
            }
        }

        public int ToggleAlertSideMenu
        {
            get => toggleAlertSideMenu;
            set
            {
                toggleAlertSideMenu = value;
                PC();
            }
        }

        public bool ToggleAlertEnabled
        {
            get => toggleAlertEnabled;
            set
            {
                toggleAlertEnabled = value;
                PC();
            }
        }

        public void AddAlert(object o)
        {
            if (!RepeatAlertBool)
            {
                AlertInterval = 0;
            }

            AlertItem item = new AlertItem(AlertPrice, AlertSymbol, PlaySoundBool, RepeatAlertBool, AlertInterval, ReverseFirstBool, false, 0, AlertDirection);

            AddAlert(item);
        }

        internal void AddAlert(AlertItem item)
        {
            Alerts.Add(item);

            Alerts = new ObservableCollection<AlertItem>(Alerts.OrderByDescending(d => d.AlertSymbol));

            _ = Task.Run(() =>
            {
                Tickers.AddTicker(AlertSymbol);

                WriteLog.Alert("-----------------------------Added Alert-----------------------------" + "\n" +
                               "Alert Price: " + AlertPrice + "| Alert Symbol: " + AlertSymbol + " | Repeat Interval:" + AlertInterval + "\n" +
                               "Repeat Alert: " + RepeatAlertBool + " | Play Sound: " + PlaySoundBool + " | Reverse First: " + ReverseFirstBool + "\n" +
                               "Direction: " + AlertDirection + "\n" +
                               "-----------------------------Added Alert-----------------------------");
            });
        }

        public void RemoveAlert(object o)
        {
            var alert = o as AlertItem;
            if (alert != null)
            {
                Invoke.InvokeUI(() =>
                {
                    WriteLog.Info("Removed Alert for: " + alert.AlertSymbol + " at " + alert.AlertPrice);
                    Alerts.Remove(alert);
                });
            }
        }

        public void ReverseBeforeAlert(object o)
        {
            ReverseFirstBool = !ReverseFirstBool;
        }

        public void RepeatAlert(object o)
        {
            RepeatAlertBool = !RepeatAlertBool;
        }

        public void PlaySound(object o)
        {
            PlaySoundBool = !PlaySoundBool;
        }
    }
}
