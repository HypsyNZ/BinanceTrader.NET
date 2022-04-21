<a href="https://github.com/HypsyNZ/BinanceTrader.NET/releases/"><img src="https://i.imgur.com/5HBH8IM.png" align="left" height="64" width="64" ></a><h1> BinanceTrader.NET </h1>

[![CodeFactor](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/badge/main)](https://www.codefactor.io/repository/github/hypsynz/binancetrader.net/overview/main)
 
Real Time Market Trading App for Binance.com - [Download Here](https://github.com/HypsyNZ/BinanceTrader.NET/releases/)

![UI](https://i.ibb.co/xzZXmnB/preview.gif)


# Basics

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
- [x] Automatic backup/restore of critical information

# Real Time Quote

![Real Time Quote](https://i.ibb.co/sCMx68v/real-time-quote-order.gif)

- [x] The quote updates automatically based on price and which box last had focus by default
- [x] Tick "Borrow" to automatically borrow where available
- [x] Tick "Base" to lock quote to an amount of Base Asset
- [x] You can scroll your mouse to increment/decrement the amount by the expected minimum step for the selected coin
- [x] Tick "Limit" to place Limit Orders instead, This will unlock the "Purchase Price" and the "Quote Price" will update automatically.

# Running Interest

![Running Interest](https://i.ibb.co/6F1KCP6/interest-preview.gif)

- [x] Updates in Real Time
- [x] Running Interest To Date in Quote Price
- [x] Running Interest To Date in Base Price
- [x] Reset Running Interest for current Order 
- [x] Interest Per Hour
- [x] Interest Per Day

# Real Time Asset Update

![https://i.ibb.co/tYBxv24/asset-preview.gif](https://i.ibb.co/tYBxv24/asset-preview.gif)

- [x] Track account information in nearly real time
- [x] Changes based on what mode you have selected
- [x] Can be moved around the screen if you want to position them somewhere else 

# One Click Settle

![One Click Settle](https://i.imgur.com/uiUpzH6.png)

- [x] One Click Settle your Borrowing where available
- [x] Settle Max Free Amount of Quote Asset
- [x] Settle Max Free Amount of Base Asset
- [x] Settle Max Free Amount of Both Symbols (Default)
- [x] Enabled/Disabled based on availability

# Orders

![https://i.imgur.com/mZtPmU2.png](https://i.imgur.com/mZtPmU2.png)

- [x] All Orders are saved to file in valid JSON (and loaded on application open)
- [x] All saved orders are backed up by `.bak` files that can be restored in case of file corruption
- [x] Copy Order to the clipboard (`Ctrl+C` while an order is highlighted) 
- [x] Locally hide order (`Delete` key while order is highlighted)

# 24 Hour Stats

![24 Hour Stats Preview](https://i.imgur.com/3V4qVNN.png)

- [x] Always available at the top of the screen
- [x] 24 Hour rolling statistics (Change/High/Low/Close/Volume)
- [x] Tells you the server time in real time 

# Side Menu

![Side Menu Update Preview](https://i.ibb.co/3hxd3Sz/menu-update-preview.gif)

- [x] Updates Every Second
- [x] All Tradable coins for the current mode
- [x] Can be hidden by clicking Hide Side Menu in the top right

# Watchlist

![Watchlist Preview](https://i.ibb.co/RT928nj/watchlist-resize-preview.gif)

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

- [x] Monitor as many symbols as your internet connection/processor can handle in Real Time
- [x] Can be resized vertically to display more symbols

_You may experience latency if you add too many symbols._

# Alerts

![Alerts Window](https://i.imgur.com/zSFQh38.png)

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

# Per Order Tasks

![Order Tasks](https://i.imgur.com/yfrJ0zQ.png)

You can now settle an order instantly at market price with one click

| Name  | Button | Description |
| ------------- | ------------- | ------------ |
| Buy | B | Buy Filled Amount, Will NOT Settle |
| Buy and Settle  | BS  | Buy Filled Amount, Settle Max Free Amount of Base Asset |
| Buy, Borrow and Settle  | BBS  | Buy Filled Amount, Borrow If Required, Settle Max Free Amount of Base Asset |
| Sell | S | Sell Filled Amount, Will NOT Settle |
| Sell and Settle | SS | Sell Filled Amount, Settle Max Free Amount of Base Asset |
| Sell, Borrow and Settle | SBS | Sell Filled Amount, Borrow If Required, Settle Max Free Amount of Base Asset|

You can also reset the running interest for each order

![Reset Running Interest](https://i.imgur.com/1oMJAfC.png)

# Notepad

![Notepad Demo](https://i.ibb.co/7nF5d72/notepad-preview.gif)

- [x] Resize with corner gripper
- [x] Save to file by clicking save icon
- [x] Automatically loads saved notes during startup
- [x] Notes will save automatically on program close

# Dependencies

All dependencies that can't be downloaded as a package are included in the `References` directory.

They are also available in my other repos if you want to build them yourself.

|Name                    |Framework       |Link                                           | [Package](https://www.nuget.org/) |
|------------------------|----------------|-----------------------------------------------|-------|
| TimerSink.NET.dll      | .NET	4.8       |https://github.com/HypsyNZ/Timer-Sink.NET      | [Yes](https://www.nuget.org/packages/TimerSink.NET/)   |
| PrecisionTimer.NET.dll | .NET	4.8       |https://github.com/HypsyNZ/Precision-Timer.NET | [Yes](https://www.nuget.org/packages/PrecisionTimer.NET/)   |
| LoopDelay.NET.dll      | netstandard2.0 |https://github.com/HypsyNZ/LoopDelay.NET       | [Yes](https://www.nuget.org/packages/LoopDelay.NET/)   |
| SimpleLog4.NET.dll     | netstandard2.0 |https://github.com/HypsyNZ/SimpleLog4.NET      | [Yes](https://www.nuget.org/packages/SimpleLog4.NET)|
| BinanceAPI.dll         | netstandard2.0 |https://github.com/HypsyNZ/Binance-API         | No    |
| CandyShop.dll          | .NET       4.8 |https://github.com/HypsyNZ/CandyShop           | No    |

You may experience issues if you use other versions.

# Tips

If you found this useful pleases consider leaving a tip

|     [![Donate with Bitcoin](https://en.cryptobadges.io/badge/micro/1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm)](https://en.cryptobadges.io/donate/1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm) | 1NXUg88UvRWYn1WTnikVNn2fbbEtuTeXzm  |
| --------- | ----------- |
|  [![Donate with Ethereum](https://en.cryptobadges.io/badge/micro/0x50740d132481be4721b1742670031baee3655ec2)](https://en.cryptobadges.io/donate/0x50740d132481be4721b1742670031baee3655ec2)	  | 0x50740d132481be4721b1742670031baee3655ec2 |
|[![Donate with Litecoin](https://en.cryptobadges.io/badge/micro/Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES)](https://en.cryptobadges.io/donate/Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES) |Lbd3oMKeokyXUQaxBDJpMMNVUws5wYhQES|