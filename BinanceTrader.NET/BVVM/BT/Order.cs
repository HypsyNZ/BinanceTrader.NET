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
using BinanceAPI.Objects.Shared;
using BinanceAPI.Objects.Spot.UserStream;
using BinanceAPI.Objects.Spot.WalletData;
using BTNET.BV.Base;
using BTNET.BVVM.Controls;
using BTNET.VM.ViewModels;
using System;

namespace BTNET.BVVM.BT
{
    internal class Order : ObservableObject
    {
        public static OrderBase NewOrder(BinanceOrderBase o, BinanceTradeFee btf, decimal InterestRate)
        {
            return new OrderBase()
            {
                OrderId = o.OrderId,
                Symbol = o.Symbol,
                QuantityFilled = o.QuantityFilled,
                Quantity = o.Quantity,
                OrderFee = TradeFeeNoInfo(o.Type, o.Status, btf),
                Price = o.Price,
                CreateTime = o.CreateTime,
                UpdateTime = o.UpdateTime,
                Side = o.Side,
                Status = o.Status,
                Type = o.Type,
                TimeInForce = o.TimeInForce.ToString(),
                IsMaker = MakerNoInfo(o.Type, o.Status, btf),
                IPH = InterestRate,
                IPD = InterestRate,
                ITD = (decimal)new TimeSpan(DateTime.UtcNow.Ticks - o.CreateTime.Ticks).TotalHours,
                Helper = new OrderViewModel(o.Side, o.Status)
            };
        }

        public static OrderBase AddNewOrderOnUpdate(BinanceStreamOrderUpdate data, decimal convertedPrice, BinanceTradeFee btf, decimal InterestRate)
        {
            return new OrderBase
            {
                OrderId = data.OrderId,
                Symbol = data.Symbol,
                QuantityFilled = data.QuantityFilled,
                Quantity = DecimalLayout.TrimDecimal(data.Quantity),
                OrderFee = TradeFeeTakerMaker(data.BuyerIsMaker, btf),
                Price = convertedPrice,
                CreateTime = data.CreateTime,
                Status = data.Status,
                Side = data.Side,
                Type = data.Type,
                TimeInForce = data.TimeInForce.ToString(),
                IsMaker = data.BuyerIsMaker,
                IPH = InterestRate,
                IPD = InterestRate,
                ITD = (decimal)new TimeSpan(DateTime.UtcNow.Ticks - data.CreateTime.Ticks).TotalHours,
                Helper = new OrderViewModel(data.Side, data.Status)
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

        // Orders that weren't stored and didn't recieve any OrderUpdates
        // Orders placed outside the app while the app is closed fall into this category
        // Orders that were placed before you started using the app fall into this category
        // Orders that got lost for some reason fall into this category
        private static decimal TradeFeeNoInfo(OrderType o, OrderStatus os, BinanceTradeFee btf)
        {
            // Market orders are always Taker orders
            if (o == OrderType.Market)
            {
                return btf.TakerFee;
            }
            else // Limit Maker can't be Taker
            if (o == OrderType.LimitMaker)
            {
                return btf.MakerFee;
            }

            // This leaves filled orders and orders with an unknowable state
            return btf.TakerFee >= btf.MakerFee ? btf.TakerFee : btf.MakerFee;
        }

        /// <summary>
        /// When you retrieve orders from the server manually it doesn't tell you if they were maker or taker orders, This information only comes from OnOrderUpdates
        /// Like the trade fee if there is no OnOrderUpdate information for this order then it is a best guess.
        /// In the case that we can't guess it will select the higher fee
        /// </summary>
        /// <param name="o">Order Type for the Order</param>
        /// <param name="os">Order Status for the Order</param>
        /// <returns></returns>
        private static bool MakerNoInfo(OrderType o, OrderStatus os, BinanceTradeFee btf)
        {
            // Market orders are always Taker orders
            if (o == OrderType.Market)
            {
                return false;
            }
            else // Limit Maker can't be Taker
            if (o == OrderType.LimitMaker)
            {
                return true;
            }

            // This leaves filled orders and orders with an unknowable state
            return btf.TakerFee >= btf.MakerFee ? false : true;
        }
    }
}