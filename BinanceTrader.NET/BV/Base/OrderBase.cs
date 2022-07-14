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

using BinanceAPI.Enums;
using BTNET.BVVM;
using BTNET.VM.ViewModels;
using Newtonsoft.Json;
using System;

namespace BTNET.BV.Base
{
    public class OrderBase : ObservableObject
    {
        private OrderViewModel? helper;

        private DateTime resetTime = DateTime.MinValue;
        private DateTime time;
        private DateTime? updateTime;

        private OrderStatus status;
        private OrderSide side;
        private OrderType type;

        private long id;
        private bool isMaker;

        private string symbol = "";
        private string fulfilled = "";
        private string timeinforce = "";

        private decimal pnl;
        private decimal orderFee;
        private decimal originalQuantity;
        private decimal executedQuantity;
        private decimal price;
        private decimal fee;
        private decimal minPos;
        private decimal iph;
        private decimal ipd;
        private decimal itd;
        private decimal itdq;
        private decimal cumulativeQuoteQuantityFilled;

        public long OrderId
        {
            get => this.id;
            set
            {
                this.id = value;
                PC();
            }
        }

        public string Symbol
        {
            get => this.symbol;
            set
            {
                this.symbol = value;
                PC();
            }
        }

        public decimal OrderFee
        {
            get => orderFee;
            set
            {
                orderFee = value;
                PC();
            }
        }

        public decimal Quantity
        {
            get => this.originalQuantity;
            set
            {
                this.originalQuantity = value;
                PC();
            }
        }

        public decimal QuantityFilled
        {
            get => this.executedQuantity;
            set
            {
                this.executedQuantity = value;
                PC();
                CanCancel = CanCancel; // PC();
            }
        }

        public decimal CumulativeQuoteQuantityFilled
        {
            get => cumulativeQuoteQuantityFilled;
            set
            {
                cumulativeQuoteQuantityFilled = value;
                PC();
            }
        }

        public decimal Price
        {
            get => this.price;
            set
            {
                this.price = value;
                PC();
            }
        }

        public DateTime CreateTime
        {
            get => this.time;
            set
            {
                this.time = value;
                PC();
            }
        }

        public DateTime ResetTime
        {
            get => this.resetTime;
            set
            {
                this.resetTime = value;
                PC();
            }
        }

        public DateTime? UpdateTime
        {
            get => this.updateTime;
            set
            {
                this.updateTime = value;
                PC();
            }
        }

        public OrderStatus Status
        {
            get => this.status;
            set
            {
                this.status = value;
                PC();
            }
        }

        public OrderSide Side
        {
            get => this.side;
            set
            {
                this.side = value;
                PC();
            }
        }

        public OrderType Type
        {
            get => this.type;
            set
            {
                this.type = value;
                PC();
            }
        }

        /// <summary>
        /// Maker or Taker Order
        /// </summary>
        public bool IsMaker
        {
            get => isMaker;
            set
            {
                isMaker = value;
                PC();
            }
        }

        /// <summary>
        /// Order Fee For This Order
        /// </summary>
        public decimal Fee
        {
            get => this.fee;
            set
            {
                this.fee = value;
                PC();
            }
        }

        /// <summary>
        /// Min profit indicator (Order Fee x 5)
        /// </summary>
        public decimal MinPos
        {
            get => this.minPos;
            set
            {
                this.minPos = value;
                PC();
            }
        }

        /// <summary>
        /// Interest Per Hour
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestPerHour
        {
            get => this.iph;
            set
            {
                this.iph = value;
                PC();
            }
        }

        /// <summary>
        /// Interest Per Day
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestPerDay
        {
            get => this.ipd;
            set
            {
                this.ipd = value;
                PC();
            }
        }

        /// <summary>
        /// Interest to Date in Base Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestToDate
        {
            get => this.itd;
            set
            {
                this.itd = value;
                PC();
            }
        }

        /// <summary>
        /// Interest to Date in Quote Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal InterestToDateQuote
        {
            get => itdq;
            set
            {
                itdq = value;
                PC();
            }
        }

        /// <summary>
        /// Running Profit and Loss indicator in Quote Price
        /// </summary>
        public decimal Pnl
        {
            get => this.pnl;
            set
            {
                this.pnl = value;
                PC();
            }
        }

        public string TimeInForce
        {
            get => this.timeinforce;
            set
            {
                this.timeinforce = value;
                PC();
            }
        }

        public string Fulfilled
        {
            get => fulfilled;
            set
            {
                fulfilled = value;
                PC();
            }
        }

        [JsonIgnore]
        public bool CanCancel
        {
            get { return Status is OrderStatus.New or OrderStatus.PartiallyFilled && Type != OrderType.Market; }
            set
            {
                PC();
            }
        }

        [JsonIgnore]
        public OrderViewModel? Helper
        {
            get => helper;
            set
            {
                helper = value;
                PC();
            }
        }
    }
}
