using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.UserStream;
using ExchangeAPI.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTNET.BVVM.Test
{
    /// <summary>
    /// Not A Real Test
    /// v1.0.1 Pass
    /// </summary>
    internal class DummyTestOrder
    {
        private void TestOrder()
        {
            Task.Run(async () =>
            {
                await Task.Delay(20000);

                _ = Task.Run(() =>
                {
                    BinanceStreamOrderUpdate data1 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 10, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data2 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 15, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data3 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 26, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data4 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 23, Price = 1, Quantity = 100.0M, QuantityFilled = 47, Event = "executionReport", Status = OrderStatus.New, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data5 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 65, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data6 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 68, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data7 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 23, Price = 1, Quantity = 100.0M, QuantityFilled = 69, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    TestUpdate(data1);
                    TestUpdate(data2);
                    TestUpdate(data3);
                    TestUpdate(data4);
                    TestUpdate(data5);
                    TestUpdate(data6);
                    TestUpdate(data7);
                });

                _ = Task.Run(() =>
                {
                    BinanceStreamOrderUpdate data8 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 23, Price = 1, Quantity = 100.0M, QuantityFilled = 70, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data9 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 80, Event = "executionReport", Status = OrderStatus.PartiallyFilled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Limit, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data10 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 22, Price = 1, Quantity = 100.0M, QuantityFilled = 100, Event = "executionReport", Status = OrderStatus.Filled, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data11 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 24, Price = 1, Quantity = 0M, QuantityFilled = 100.0M, Event = "executionReport", Status = OrderStatus.New, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Market, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data13 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 24, Price = 1, Quantity = 0M, QuantityFilled = 100.0M, Event = "executionReport", Status = OrderStatus.New, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Market, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data12 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 24, Price = 1, Quantity = 99.0M, QuantityFilled = 99.0M, Event = "executionReport", Status = OrderStatus.Filled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Market, BuyerIsMaker = false };
                    BinanceStreamOrderUpdate data14 = new BinanceStreamOrderUpdate() { Symbol = "DOGEUSDT", OrderId = 25, Price = 1, Quantity = 98.0M, QuantityFilled = 98.0M, Event = "executionReport", Status = OrderStatus.Filled, CreateTime = DateTime.UtcNow, Side = OrderSide.Buy, Type = OrderType.Market, BuyerIsMaker = false };
                    TestUpdate(data8);
                    TestUpdate(data9);
                    TestUpdate(data10);
                    TestUpdate(data11);
                    TestUpdate(data13);
                    TestUpdate(data12);
                    TestUpdate(data14);
                });
            });
        }

        private void TestUpdate(BinanceStreamOrderUpdate order)
        {
            DataEvent<BinanceStreamOrderUpdate> update = new DataEvent<BinanceStreamOrderUpdate>(order, DateTime.UtcNow);
            OnOrderUpdate(update);
        }
    }
}