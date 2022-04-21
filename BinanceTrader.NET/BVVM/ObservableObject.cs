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

using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.VM.ViewModels;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using TimerSink;

namespace BTNET.BVVM
{
    public class ObservableObject : INotifyPropertyChanged
    {
        #region [PropertyChangedEvent]

        public event PropertyChangedEventHandler PropertyChanged;

        protected void PC([CallerMemberName] string callerName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        #endregion [PropertyChangedEvent]

        public static string Product { get; } = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false)).Product;
        public static string Version { get; } = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute), false)).Version;

        public static BVVM.BT.Timing Timers;
        public TimingSink Sink;

        public string symbolSearchValue;
        private decimal minTickSize, incrementLotSizeMin;
        private bool searchEnabled = false;

        public readonly ApiKeys UserApiKeys = new();

        public decimal MinTickSize
        { get => this.minTickSize; set { this.minTickSize = value; PC(); } }

        public decimal IncrementLotSizeMin
        { get => this.incrementLotSizeMin; set { this.incrementLotSizeMin = value; PC(); } }

        public bool SearchEnabled
        { get => this.searchEnabled; set { this.searchEnabled = value; PC(); } }

        public bool IsSymbolSelected
        { get => Static.symbolSelected; set { Static.symbolSelected = value; PC(); } }

        #region [ Static ]

        public static MainViewModel MainVM { get; set; }
        public static ServerTimeBase ServerTime { get; set; } = new();
        public static OrderBase SelectedListItem { get; set; } = new();
        public static BorrowViewModel BorrowVM { get; set; } = new();
        public static QuoteViewModel QuoteVM { get; set; } = new();
        public static TradeViewModel TradeVM { get; set; } = new();
        public static WatchlistViewModel WatchListVM { get; set; } = new();
        public static RealTimeUpdateViewModel RealTimeVM { get; set; } = new();
        public static SettleViewModel SettleVM { get; set; } = new();
        public static AlertViewModel AlertVM { get; set; } = new();
        public static NotepadViewModel NotepadVM { get; set; } = new();
        public static VisibilityViewModel VisibilityVM { get; set; } = new();

        public static MainOrders Orders { get; set; } = new();

        #endregion [ Static ]
    }
}