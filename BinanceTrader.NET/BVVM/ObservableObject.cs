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

using BinanceAPI;
using BTNET.BV.Abstract;
using BTNET.BV.Base;
using BTNET.BVVM.BT;
using BTNET.VM.ViewModels;
using Newtonsoft.Json;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void PC([CallerMemberName] string callerName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }

        #endregion [PropertyChangedEvent]

        private decimal minTickSize;
        private decimal minPrice;
        private decimal quantityMin;
        private decimal quantityMax;
        private decimal priceMax;
        private decimal priceTickSize;
        private readonly ApiKeys userApiKeys = new();

        [JsonIgnore]
        public ApiKeys UserApiKeys => this.userApiKeys;

        [JsonIgnore]
        public static TimingSink? Sink { get; set; }

        [JsonIgnore]
        public static TimingSink? SinkTwo { get; set; }

        [JsonIgnore]
        public string SymbolSearchValue { get; set; } = "";

        [JsonIgnore]
        public static string Product { get; } = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(typeof(App).Assembly,
            typeof(AssemblyProductAttribute), false)).Product;

        [JsonIgnore]
        public static string Version { get; } = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(App).Assembly,
            typeof(AssemblyFileVersionAttribute), false)).Version;

        [JsonIgnore]
        public decimal QuantityTickSize
        {
            get => minTickSize;
            set
            {
                minTickSize = value.Normalize();
                PC();
            }
        }

        [JsonIgnore]
        public decimal QuantityMin
        {
            get => quantityMin;
            set
            {
                quantityMin = value.Normalize();
                PC();
            }
        }

        [JsonIgnore]
        public decimal QuantityMax
        {
            get => quantityMax;
            set
            {
                quantityMax = value.Normalize();
                PC();
            }
        }

        [JsonIgnore]
        public decimal PriceTickSize
        {
            get => priceTickSize;
            set
            {
                priceTickSize = value.Normalize();
                PC();
            }
        }

        [JsonIgnore]
        public decimal PriceMin
        {
            get => minPrice;
            set
            {
                minPrice = value.Normalize();
                PC();
            }
        }

        [JsonIgnore]
        public decimal PriceMax
        {
            get => priceMax; set
            {
                priceMax = value.Normalize();

                PC();
            }
        }

        #region [ Static ]

        public static MainViewModel MainVM { get; set; } = new(null!);
        public static SettingsViewModel SettingsVM { get; set; } = new();
        public static ServerTimeViewModel ServerTimeVM { get; set; } = new();
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
        public static LogViewModel LogVM { get; set; } = new();

        public static FlexibleViewModel FlexibleVM { get; set; } = new();

        public static Timing? Timers { get; set; }
        public static MainOrders Orders { get; set; } = new();

#pragma warning disable CS8618 // Won't be null
        public static BTClient Client { get; set; }
#pragma warning restore CS8618 // Won't be null

        #endregion [ Static ]
    }
}
