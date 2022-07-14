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
using BTNET.BV.Base;
using BTNET.BV.Converters;
using System;

namespace BTNET.BVVM.BT.Orders
{
    internal static class OrderHelper
    {
        private const int ONE_HUNDRED = 100;
        private const int HOURS = 24;

        public static decimal InterestPerHour(decimal quantity, decimal interestRate)
        {
            return decimal.Round((((interestRate / HOURS) * quantity) / ONE_HUNDRED), App.DEFAULT_ROUNDING_PLACES);
        }

        public static decimal InterestPerDay(decimal quantity, decimal interestRate)
        {
            return decimal.Round((interestRate * quantity) / ONE_HUNDRED, App.DEFAULT_ROUNDING_PLACES);
        }

        public static decimal InterestToDate(decimal interestPerHour, long ticks)
        {
            return decimal.Round((decimal)new TimeSpan(DateTime.UtcNow.Ticks - ticks).TotalHours * interestPerHour, App.DEFAULT_ROUNDING_PLACES);
        }

        public static decimal InterestToDateQuote(decimal interestToDate, decimal price)
        {
            return decimal.Round(interestToDate * price, App.DEFAULT_ROUNDING_PLACES);
        }

        public static decimal Fee(decimal fee, decimal price, decimal quantityFilled)
        {
            decimal t = 0;
            if (fee > 0)
            {
                var f = price * quantityFilled / ONE_HUNDRED;
                t = f * (fee * ONE_HUNDRED);
            }

            return t;
        }

        public static decimal PnL(OrderBase order, decimal askPrice, decimal bidPrice)
        {
            if (order.Status == OrderStatus.Canceled)
            {
                return 0;
            }

            decimal price = (order.Side == OrderSide.Sell) ? askPrice : bidPrice;

            if (order.CumulativeQuoteQuantityFilled != 0)
            {
                if (order.Side == OrderSide.Sell)
                {
                    return order.CumulativeQuoteQuantityFilled - (price * order.QuantityFilled);
                }
                else
                {
                    return (price * order.QuantityFilled) - order.CumulativeQuoteQuantityFilled;
                }
            }
            else
            {
                if (order.Side == OrderSide.Sell)
                {
                    return (order.Price * order.QuantityFilled) - (price * order.QuantityFilled);
                }
                else
                {
                    return (price * order.QuantityFilled) - (order.Price * order.QuantityFilled);
                }
            }
        }

        public static string TIF(string? tif)
        {
            return tif == "GoodTillCancel"
                    ? "GTCancel" : tif == "ImmediateOrCancel"
                    ? "IOCancel" : tif == "FillOrKill"
                    ? "FOKill" : tif == "GoodTillCrossing"
                    ? "GTCross" : tif == "GoodTillExpiredOrCanceled"
                    ? "GTExpire" : "NaN";
        }

        public static string Fulfilled(decimal quantity, decimal quantityFilled)
        {
            return NumericFieldConverter.ConvertBasic(quantityFilled) + "/" + NumericFieldConverter.ConvertBasic(quantity);
        }
    }
}
