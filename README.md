<a href="https://github.com/HypsyNZ/BinanceTrader.NET/releases/"><img src="https://i.imgur.com/5HBH8IM.png" align="left" height="64" width="64" ></a><h1> BinanceTrader.NET </h1>


| Main  | Beta |
| ------------- | ------------- |
| [![CodeFactor](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/badge/main)](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/overview/main)  | [![CodeFactor](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/badge/beta)](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/overview/beta)  |

Real Time Market Trading App for Binance.com - [Download Here](https://github.com/HypsyNZ/BinanceTrader.NET/releases/)

<h1></h1>

![Real Time Price Ticker Demo](https://i.ibb.co/0fw79cs/real-time-update1.gif) ![https://i.ibb.co/SynL7Zz/demo.gif](https://i.ibb.co/SynL7Zz/demo.gif)

- [x] Spot/ Margin / Isolated
- [x] Buy / Sell Market Orders
- [x] Buy / Sell Limit Orders
- [x] One Click Borrow
- [x] One Click Settle
- [x] Settle Widget for "Per Order" decisions 
- [x] Direction Based Alerts
- [x] Real Time Quotes
- [x] Chart using Embedded Browser (Binance / TradingView)
- [x] Real Time Watchlist
- [x] Real Time Bid/Ask
- [x] Real Time Running Interest
- [x] Real Time Profit & Loss (PnL)
- [x] Track Fee for Orders (x2)

# Real Time Quote

![Real Time Quote](https://i.ibb.co/ZY6bXVW/demo-real-time-quote.gif)

- [x] The quote updates automatically based on price and which box last had focus by default
- [x] Tick "Borrow" to automatically borrow where available
- [x] Tick "Base" to lock quote to an amount of Base Asset
- [x] You can scroll your mouse to increment/decrement the amount by the expected minimum step for the selected coin
- [x] Tick "Limit" to place Limit Orders instead, This will unlock the "Purchase Price" and the "Quote Price" will update automatically.

# Orders

![https://i.imgur.com/G6U2Dd7.png](https://i.imgur.com/G6U2Dd7.png)

- [x] All Orders are saved to file in valid JSON (and loaded on application open)
- [x] Copy Order to the clipboard (`Ctrl+C` while an order is highlighted) 
- [x] Locally hide order (`Delete` key while order is highlighted)

# Watchlist

![Watchlist Demo](https://i.ibb.co/2KWrKtT/watchlist.gif)

| Value     | Description   | Speed      |
| --------- | -----------   | ---------- |
| Bid	  	  | The Best Bid  |  Websocket |
| Ask	  	  | The Best Ask  |  Websocket |
| Price  	  | Current Price |  1 Second  |
| High  	  | 24 Hour High  |  1 Second  |
| Low   	  | 24 Hour Low   |  1 Second  |
| Close 	  | 24 Hour Close |  1 Second  |
| Change	  | 24 Hour Change|  1 Second  |
| Volume      | 24 Hour Volume|  1 Second  |

- [x] Monitor up to 15 coins at once in Real Time, You may experience latency if you add more than this

# Alerts

![Alerts Window](https://i.imgur.com/4AMkYZO.png)

| Value           | Description                                                                           |
| --------------  | ------------------------------------------------------------------------------------- | 
| Alert Symbol    | The coin for the Alert                                                                |
| Alert Price     | The price the alert should trigger at                                                 |
| Play Sound      | Tick if the Alert should play a sound (Otherwise it just shows in the Minilog)        |
| Repeat Alert    | If the Alert should trigger more than once                                            |
| Repeat Interval | If the Alert should repeat how often should it go off                                 |
| Reverse First   | If the price should go back under/over the alert price before being allowed to repeat |
| Intent          | If you intend to buy/sell (Doesn't do anything right now)                             |
| Direction       | When to trigger the alert (Above or Below the alert price)                            |

- [x] Alerts can have `Actions` if you make them programmatically, I will eventually be making an action builder so it can be done from the UI.
- [x]  Only alerts for the currently selected symbol will trigger, This will change eventually.

# One Click Settle

![One Click Settle](https://i.imgur.com/MKjm1Lz.png)

- [x] One Click Settle your Borrowing where available
- [x] Settle Max Free Amount of Quote Asset
- [x] Settle Max Free Amount of Base Asset
- [x] Settle Max Free Amount of Both Symbols (Default)

![One Click Settle Per Order](https://i.imgur.com/IMxx2Cy.png)

Each listed Order now has 2 new buttons which allow you to settle an order instantly at market price with one click

| Name  | Button | Description |
| ------------- | ------------- | ------------ |
| Buy and Settle  | BS  | Buy Filled Amount, Settle Max Free Amount of Base Asset |
| Buy, Borrow and Settle  | BBS  | Buy Filled Amount, Borrow If Required, Settle Max Free Amount of Base Asset |
| Sell and Settle | SS | Sell Filled Amount, Settle Max Free Amount of Base Asset |
| Sell, Borrow and Settle | SBS | Sell Filled Amount, Borrow If Required, Settle Max Free Amount of Base Asset|


# Running Interest

![Running Interest](https://i.ibb.co/7RjXX42/ITD.gif)

- [x] Updates in Real Time
- [x] Interest To Date in Quote Price
- [x] Interest To Date in Base Price
- [x] Interest Per Hour
- [x] Interest Per Day

# Dependencies

All dependencies are included in the `References` directory.

They are also available in my other repos if you want to build them yourself.

|Name                    |Framework       |Link                                           |
|------------------------|----------------|-----------------------------------------------|
| TimerSink.NET.dll      | .NET	4.8       |https://github.com/HypsyNZ/Timer-Sink.NET      |
| PrecisionTimer.NET.dll | .NET	4.8       |https://github.com/HypsyNZ/Precision-Timer.NET |
| LoopDelay.NET.dll      | netstandard2.0 |https://github.com/HypsyNZ/LoopDelay.NET       |
| BinanceAPI.dll         | netstandard2.0 |https://github.com/HypsyNZ/Binance-API         |
| ExchangeAPI.dll        | netstandard2.0 |https://github.com/HypsyNZ/Exchange-API        |

You may experience issues if you use other versions.

# Tips

If you found this useful pleases consider leaving a tip

|     [![Donate with Bitcoin](https://en.cryptobadges.io/badge/micro/1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm)](https://en.cryptobadges.io/donate/1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm) | 1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm  |
| --------- | ----------- |
|  [![Donate with Ethereum](https://en.cryptobadges.io/badge/micro/0x50740d132481be4721b1742670031baee3655ec2)](https://en.cryptobadges.io/donate/0x50740d132481be4721b1742670031baee3655ec2)	  | 0x50740d132481be4721b1742670031baee3655ec2 |
|[![Donate with Litecoin](https://en.cryptobadges.io/badge/micro/Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES)](https://en.cryptobadges.io/donate/Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES) |Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES|
