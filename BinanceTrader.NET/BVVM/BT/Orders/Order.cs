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
using BinanceAPI.Objects.Shared;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Log;
using BTNET.VM.ViewModels;

namespace BTNET.BVVM.BT
{
    internal class Order : ObservableObject
    {
        public static OrderBase AddNewOrderFromServer(BinanceOrderBase o, TradingMode tradingMode)
        {
            BinanceTradeFee fee = TradeFee.GetTradeFee(o.Symbol);
            var baseFee = TradeFeeNoInfo(o.Type, fee);
            var orderFee = OrderHelper.Fee(TradeFeeNoInfo(o.Type, fee), o.Price, o.QuantityFilled);
            WriteLog.Info("Add: FeeT: " + fee.TakerFee + "| FeeM: " + fee.MakerFee + " | baseFee: " + baseFee + "| orderFee: " + orderFee);

            decimal orderPrice;
            if (o.AverageFillPrice > 0)
            {
                // Average Price (Actual Price for this order)
                orderPrice = (decimal)o.AverageFillPrice;
            }
            else
            {
                // Order filled at one price / couldnt get real price etc
                orderPrice = o.Price;
            }

            return new OrderBase()
            {
                OrderId = o.OrderId,
                Symbol = o.Symbol,
                QuantityFilled = o.QuantityFilled,
                Quantity = o.Quantity,
                CumulativeQuoteQuantityFilled = o.QuoteQuantityFilled,
                OrderFee = baseFee,
                Price = orderPrice,
                CreateTime = o.CreateTime,
                UpdateTime = o.UpdateTime,
                Side = o.Side,
                Status = o.Status,
                Type = o.Type,
                IsMaker = MakerNoInfo(o.Type, fee),
                Fee = orderFee * App.FEE_MULTIPLIER,
                MinPos = orderFee * App.MIN_PNL_FEE_MULTIPLIER,
                TimeInForce = OrderHelper.TIF(o.TimeInForce.ToString()),
                Fulfilled = OrderHelper.Fulfilled(o.Quantity, o.QuantityFilled),
                Helper = new OrderViewModel(o.Side, tradingMode, o.Symbol)
            };
        }

        public static OrderBase AddNewOrderOnUpdate(BinanceStreamOrderUpdate data, decimal convertedPrice, TradingMode tradingmode)
        {
            BinanceTradeFee fee = TradeFee.GetTradeFee(data.Symbol);
            decimal baseFee = TradeFeeTakerMaker(data.BuyerIsMaker, fee);
            decimal orderFee = OrderHelper.Fee(baseFee, convertedPrice, data.QuantityFilled);
            WriteLog.Info("Update: FeeT: " + fee.TakerFee + "| FeeM: " + fee.MakerFee + " | baseFee: " + baseFee + "| orderFee: " + orderFee);
            return new OrderBase()
            {
                OrderId = data.OrderId,
                Symbol = data.Symbol,
                QuantityFilled = data.QuantityFilled,
                Quantity = data.Quantity,
                CumulativeQuoteQuantityFilled = data.QuoteQuantityFilled,
                OrderFee = baseFee,
                Price = convertedPrice,
                CreateTime = data.CreateTime,
                Status = data.Status,
                Side = data.Side,
                Type = data.Type,
                IsMaker = data.BuyerIsMaker,
                Fee = orderFee * App.FEE_MULTIPLIER,
                MinPos = orderFee * App.MIN_PNL_FEE_MULTIPLIER,
                TimeInForce = OrderHelper.TIF(data.TimeInForce.ToString()),
                Fulfilled = OrderHelper.Fulfilled(data.Quantity, data.QuantityFilled),
                Helper = new OrderViewModel(data.Side, tradingmode, data.Symbol)
            };
        }

        // Use OnOrderUpdate to decide if order is maker/taker
        private static decimal TradeFeeTakerMaker(bool buyerIsMaker, BinanceTradeFee btf)
        {
            if (buyerIsMaker)
            {
                return btf.MakerFee;
            }

            return btf.TakerFee;
        }

        /// <summary>
        /// Orders that weren't stored and didn't recieve any OrderUpdates
        /// Orders placed outside the app while the app is closed fall into this category
        /// Orders that were placed before you started using the app fall into this category
        /// Orders that got lost for some reason fall into this category
        /// </summary>
        /// <param name="o"></param>
        /// <param name="btf"></param>
        /// <returns></returns>
        private static decimal TradeFeeNoInfo(OrderType o, BinanceTradeFee btf)
        {
            return o switch
            {
                OrderType.Market => btf.TakerFee,
                OrderType.LimitMaker => btf.MakerFee,
                _ => btf.TakerFee >= btf.MakerFee ? btf.TakerFee : btf.MakerFee,
            };
        }

        /// <summary>
        /// When you retrieve orders from the server manually it doesn't tell you if they were maker or taker orders, This information only comes from OnOrderUpdates
        /// Like the trade fee if there is no OnOrderUpdate information for this order then it is a best guess.
        /// In the case that we can't guess it will select the higher fee
        /// </summary>
        /// <param name="o">Order Type for the Order</param>
        /// <returns></returns>
        private static bool MakerNoInfo(OrderType o, BinanceTradeFee btf)
        {
            return o switch
            {
                OrderType.Market => false,
                OrderType.LimitMaker => true,
                _ => btf.TakerFee >= btf.MakerFee,
            };
        }
    }
}
