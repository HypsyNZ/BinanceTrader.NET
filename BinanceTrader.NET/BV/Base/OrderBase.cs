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

using BinanceAPI.Enums;
using BTNET.BV.Converters;
using BTNET.BVVM;
using BTNET.VM.ViewModels;
using System;

namespace BTNET.BV.Base
{
    public class OrderBase : ObservableObject
    {
        private readonly NumericFieldConverter n = new();

        private OrderViewModel helper;

        private DateTime resetTime = DateTime.MinValue;
        private DateTime time;
        private DateTime? updateTime;
        private OrderStatus status;
        private OrderSide side;
        private OrderType type;
        private long id;
        private string symbol, timeinforce;
        private bool isMaker = false;
        private decimal orderFee, originalQuantity, executedQuantity, price, fee, minPos, iph, ipd, itd;

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
            set { orderFee = value; PC(); }
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
            }
        }

        public decimal Price
        {
            get => this.price;
            set
            {
                this.price = value;
                PC();

                Fee = value;
                MinPos = value;
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
            set { isMaker = value; PC(); }
        }

        /// <summary>
        /// Order Fee For This Order
        /// </summary>
        public decimal Fee
        {
            get => this.fee;
            set
            {
                double t = 0;
                if (OrderFee > 0)
                {
                    t = (((double)Price * (double)QuantityFilled / 100) * ((double)OrderFee * 100)) * 2;
                }

                this.fee = decimal.Round((decimal)t, 8);
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
                double t = 0;
                if (OrderFee > 0)
                {
                    t = (((double)Price * (double)QuantityFilled / 100) * ((double)OrderFee * 100)) * 5;
                }

                this.minPos = decimal.Round((decimal)t, 8);
                PC();
            }
        }

        /// <summary>
        /// Interest Per Hour
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal IPH
        {
            get => this.iph;
            set { this.iph = decimal.Round(value / 24 * QuantityFilled / 100, 8); PC(); }
        }

        /// <summary>
        /// Interest Per Day
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal IPD
        {
            get => this.ipd;
            set { this.ipd = decimal.Round(value * QuantityFilled / 100, 8); PC(); }
        }

        /// <summary>
        /// Interest to Date in Base Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal ITD
        {
            get => this.itd;
            set { this.itd = decimal.Round(value * this.iph, 8); PC(); }
        }

        /// <summary>
        /// Interest to Date in Quote Price
        /// This can become inaccurate if interest rates change after you open the order
        /// </summary>
        public decimal ITDQ
        {
            get => ITD * Price;
            set => PC();
        }

        private decimal pnl;

        /// <summary>
        /// Running Profit and Loss indicator in Quote Price
        /// </summary>
        public decimal Pnl
        {
            get => this.pnl;
            set
            {
                this.pnl = decimal.Round(value, 8);
                PC();
            }
        }

        public string TimeInForce
        {
            get => this.timeinforce == "GoodTillCancel" ? "GTCancel" : this.timeinforce
                == "ImmediateOrCancel" ? "IOCancel" : this.timeinforce
                == "FillOrKill" ? "FOKill" : this.timeinforce
                == "GoodTillCrossing" ? "GTCross" : this.timeinforce
                == "GoodTillExpiredOrCanceled" ? "GTExpire" : "NaN";
            set
            {
                this.timeinforce = value;
                PC();
            }
        }

        public string Fulfilled
        { get => this.n.ConvertBasic(QuantityFilled) + "/" + this.n.ConvertBasic(Quantity); set { PC(); } }

        public OrderViewModel Helper
        { get => helper; set { helper = value; PC(); } }
    }
}