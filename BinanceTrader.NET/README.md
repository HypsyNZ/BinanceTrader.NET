### Version 2.2.0.3
- [x] Fixed a bug that could occur if you used search during an exchange info update
- [x] Fixed a bug that sometimes allowed you to select a symbol while the client was starting
- [x] You can no longer select a symbol while `Current Symbol` is loading
- [x] You can no longer select a symbol while `Trading Mode` is changing
- [x] Fixed a UI update issue
- [x] Renamed some things for clarity
- [x] Will now get `All Isolated Pairs` to improve search results
- [x] Will now get `All Margin Pairs` to improve search results
- [x] Use `Symbol Permissions` to improve search results
- [x] Fixed a bug in `BeginInvoke` helper that is unused anyway
- [x] Removed extra null checks from `Invoke` helpers

### Version 2.2.0.2
- [x] Changed when `Trade Fee` is updated for the current symbol
- [x] Changed when `Interest Rate` is updated for the current symbol
- [x] Changed when `Trade Fees` for all symbols is updated
- [x] Changed when `Interest Rates` for all symbols is updated
- [x] Exchange Information will update slightly earlier now
- [x] Made the `Timing Sinks` static
- [x] Improved error correction
- [x] Updated [`TimerSink.NET`](https://www.nuget.org/packages/TimerSink.NET)

### Version 2.1.9.9
- [x] Fixed a bug that was preventing `Running Interest` from updating at times.
- [x] Split some work into a second `TimerSink`
- [x] Add new `Sink` to the watchman
- [x] Trade Fee will update periodically.

### Version 2.1.9.7
- [x] Exchange Information will save/backup correctly again

### Version 2.1.9.6
- [x] Fixed a bug in Exchange Information
- [x] Fixed a bug that was preventing the program from starting correctly.

### Version 2.1.9.5
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET>)
- [x] Websocket messages are now processed faster

### Version 2.1.9.4
- [x] Update Browser [`CefSharp`](<https://cefsharp.github.io/>)
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET>)

### Version 2.1.9.3
- [x] Fixed a bug that could cause the `Real Time Quote` to position incorrectly.

### Version 2.1.9.2
- [x] Changed how `Order Price` is decided - very old orders may display incorrectly
- [x] PnL will use `CumulativeQuoteQuantityFilled` (`Running Total`) if it can
- [x] Interest calculation uses `QuantityFilled` instead of `Quantity` now
- [x] Interest rate for currently selected symbol will update periodically now
- [x] [Added a new Feature `Settle Order` to `Order Actions` in `Order Detail`](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Order-Actions>)
- [x] `Order Tasks` are disabled while `Settle Order` is enabled (for each order)
- [x] `Order Detail` windows now have a `Always on Top` button
- [x] Deleted `Orders` will have certain controls disabled
- [x] Settled `Orders` will have certain controls disabled
- [x] Add `Settle Percent` to `Order Actions` in `Order Detail`
- [x] Add `Settle Mode` to `Order Actions` in `Order Detail`
- [x] Add `Borrow For Settle` to `Order Actions` in `Order Detail`
- [x] Add `Modifier` to `Order Actions` in `Order Detail` - Quantity will step for the selected symbol
- [x] Add `Tooltips` to `Order Actions` in `Order Detail`
- [x] Add `CumulativeQuoteQuantityFilled` to `Order Base`
- [x] Add `Running Total` to `Order Detail`
- [x] You can now see a `Running Total` for each `Order` on the [`Order Panel`](<https://user-images.githubusercontent.com/54571583/176422106-7cc5634e-5f2f-47a1-a848-0ff0a82da89d.png>)
- [x] You can now switch symbols and change modes without losing your `Order Detail`
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET/>)
- [x] Change Flexible Product update time slightly
- [x] Fixed a bug that could occur if you delete the `Orders` or `Settings` folder
- [x] Fixed a bug in `Order Detail` that caused several undesirable things to happen.
- [x] Fixed a bug in how `Exchange Information` was updated `x2`
- [x] Fixed a bug that occurred if you closed the program while it was starting

### Version 2.1.9.1
- [x] Fixed a bug that made it look like you could cancel a market order (you can't obviously)

### Version 2.1.8.9
- [x] Fixed Watchlist `Connecting..` message that appears during startup
- [x] Limit Price will fill correctly now on load to the last value you entered
- [x] The last limit price you entered is part of the `StoredQuote`
- [x] The last quantity you enter for a symbol will be remembered as a `StoredQuote` and loaded where possible
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET/>)

### Version 2.1.8.7
- [x] Resize `Change %` in `WatchList` to fix a display issue.

### Version 2.1.8.6
- [x] Change `TabIndex` of `Real Time Quote` to simplify usage

### Version 2.1.8.5
- [x] Fixed a bug in Real Time Quote that caused `Quote` box to be enabled when it shouldn't be
- [x] `StoredQuotes` first pass this is a work in progress and will only work sometimes

### Version 2.1.8.4
- [x] Real Time Quote will now remember the settings `Base`, `Borrow`, `Limit` for the `Buy` and `Sell` tabs between loads

### Version 2.1.8.3
- [x] Add `Connection Status` to `WatchList`
- [x] `Watchlist` Connection Status is `Green` when connected
- [x] `Watchlist` Connection Status is `Yellow` when reconnecting
- [x] `Watchlist` Connection Status is `Gray` when disconnected
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET/>)

### Version 2.1.8.1
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET/>)

### Version 2.1.7.9
- [x] Fixes bug in User Subscriptions caused by an [error](<https://github.com/dotnet/runtime/blob/7cbf0a7011813cb84c6c858ef19acb770daa777e/src/libraries/Common/src/System/Net/WebSockets/ManagedWebSocket.cs#L525>) in the `.NET Runtime`

### Version 2.1.7.8
- [x] Fixed a bug that was causing multiple issues

### Version 2.1.7.7
- [x] Update [`BinanceAPI.NET`](<https://www.nuget.org/packages/BinanceAPI.NET/>) to account for Exchange Information changes

### Version 2.1.7.5
- [x] Fixed a bug in the updater that stopped the status from displaying in `About`
- [x] Reorganize `MainContext`

### Version 2.1.7.4
- [x] Removed `LocalTimeSync` - The [underlying library](<https://www.nuget.org/packages/BinanceAPI.NET/>) doesn't care about the `Local Time` anymore.

### Version 2.1.7.3
- [x] Update Packages
- [x] Fixed a bug that caused Time to go out of Sync

### Version 2.1.7.2
- [x] Fixed a bug due to testing that caused Checking for Updates

### Version 2.1.7.1
- [x] New Setting: Check for Updates on Startup (Uses Github API, Disabled by default)
- [x] Reset Head

### Version 2.1.6.2
- [x] Update Package [`BinanceAPI`](<https://www.nuget.org/packages/BinanceAPI.NET/>)
- [x] Fixed a bug that prevented Sockets from Reconnecting on Failure

### Version 2.1.5
- [x] [`LockedTask.NET`](<https://www.nuget.org/packages/LockedTask.NET/>) - Async Locks

### Version 2.1.3
- [x] Fixed a bug in `OrderUpdate` that was introduced in `Version 2.1.2`

### Version 2.1.2
- [x] Fixed some small bugs that affect performance
- [x] Fixed a bug that caused an `Order Update` to sit in the queue.
- [x] Updated Packages for Socket Improvements

### [Version 2.1.1](<https://i.imgur.com/X3rLtet.png>)
- [x] New Order Settings Menu
- [x] Animate new [Order Settings Menu](<https://i.imgur.com/zhM00fm.gif>)
- [x] Order Opacity Slider now affects Order Settings.
- [x] Order Update Improvements
- [x] Order Storage/Loading Improvements
- [x] Exchange Info Storage/Loading Improvements
- [x] Alert Storage/Loading Improvements
- [x] Fixed a bug that stopped Fee/MinPnL appearing in Order Detail for Market Orders.
- [x] Fixed a bug that stopped Orders from updating when you changed Symbols/Modes to quickly
- [x] Fixed a bug that occured when you change Order Settings and click Buy or Sell

### Version 2.1.0
- [x] Fixed new user experience
- [x] [Watchlist UI](<https://i.imgur.com/U2MuRo7.png>) Changes
- [x] Fixed an occurance of hard coded `C` drive
- [x] Add error message when max instances are already running
- [x] More locks

### Version 2.0.9
- [x] Order List: [Change Resize](<https://i.imgur.com/b1kImsh.mp4>)
- [x] Fixed a bug that could cause a crash while resizing

### Version 2.0.8
- [x] New Setting: Stretch Browser to Fit
- [x] [New Setting](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#settings>): Order Display Opacity
- [x] [New Settings](<https://i.imgur.com/Z73F7ri.gif>): Hide/Show Info Panels
- [x] Settings: Saved/Loaded when required
- [x] New [Symbol Info Panel](<https://i.imgur.com/taqIq5k.gif>) (Margin/Isolated)
- [x] New Theme: Everything is Dark
- [x] [Real Time Quote](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#real-time-quote>) Improvements
- [x] Quote: You can now use all `Numpad+` and `Numpad-` to change the price/quantity
- [x] Quote: You can now use `+` and `-` to change the price/quantity
- [x] [Alerts](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#alerts>): UI Improvements
- [x] Alerts: same as above for the repeat interval
- [x] Alerts: same as above for the price
- [x] Alerts: Prevent New Alerts from running instantly in some situations.
- [x] Alerts: Unchecking `Reverse First` will allow a New Alert to run instantly.
- [x] Alerts: Add `AlertStatus` so you can clearly see what an Alert is doing
- [x] Alerts: Alert are now active without any symbol selected (WIP)
- [x] Alerts: [Add Toggle Window on Top](<https://i.imgur.com/1hXFYO8.gif>)
- [x] Decimal Improvements
- [x] Fixed a bug that prevented Bulk Order update from server after an error
- [x] Browser: change how the browser is loaded (faster)
- [x] Fixed a bug that stopped the correct `Fulfilled` value being stored for limit orders that had updates
- [x] Fixed a bug that caused Order Detail to update incorrectly
- [x] Fixed a bug that could cause loading/storing Orders to fail
- [x] New Menu <https://i.imgur.com/uGFCeAm.gif>
- [x] New Exit Button
- [x] New Hide Side Menu Button <https://i.imgur.com/l5CbCfn.gif>
- [x] Add a warning message that displays when no API Keys are loaded
- [x] [Watchlist](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#watchlist>): Fix Resize
- [x] Watchlist: Increase `Change %` Size
- [x] Update and Resize Icons
- [x] Locks for Safety
- [x] Improvements to Account Updates
- [x] [Order Detail](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#order-details>): Fix error that occurred when cancelling an order after changing symbols

### Version 1.7.3
- [x] General UI Layout Improvements
- [x] Fixed Main Window not focusing occasionally
- [x] Watchlist: Fix symbols being empty occasionally
- [x] Watchlist: Symbol Autocomplete
- [x] Watchlist: Improve Feedback
- [x] Watchlist: Fix Percent Change display
- [x] [Flexible Savings](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#flexible-savings-products>): You can now specify a Redeem Amount
- [x] Flexible Savings: You can now specify a Subscribe Amount
- [x] Flexible Savings: Improve Feedback
- [x] Flexible Savings: Display Annual Rates for each `Tier`
- [x] You can now subscribe to Flexible Savings products
- [x] You can now fast redeem Flexible Savings products.
- [x] Socket Improvements
- [x] Userstream Improvements
- [x] [Quote](<https://github.com/HypsyNZ/BinanceTrader.NET/wiki/Additional-Information#real-time-quote>) price no longer gets rounded: <https://i.imgur.com/A6ZD01n.gif>
- [x] Replace logging console with a simple log view
- [x] Fixed a bug that could cause the order backup to be overwritten with garbage.
- [x] Order Context Switching
- [x] MinPnl is now located in Order Details*
- [x] Fee x2 is now located in Order Details*
- [x] Change layout widths* <https://i.imgur.com/MtAKlLf.jpg>
- [x] Changes how Orders are Loaded
- [x] Changes how Orders are Stored
- [x] Changes how Orders are Updated
- [x] Improve getting Trade Fees
- [x] Improve getting Interest Rates
- [x] Improvement to UserStreams
- [x] Improvement to Loading Times
- [x] Update Real Time Quote: <https://i.imgur.com/aFzFQ55.gif>
- [x] Improve Order Details View: <https://i.imgur.com/y6m8B2h.png>
- [x] Order ID is now displayed as the title for Order Details (taskbar)
- [x] Change to how tickers are updated
- [x] Added exit button to Order Details
- [x] Added Quick Order Tasks to Order Details
- [x] Fix bug that occured when reopening a window closed from the task bar.
- [x] Small change to Order Tasks: <https://i.imgur.com/hZtrOgE.gif>
- [x] Alerts are now saved to file
- [x] Alerts are now backed up to a .bak file
- [x] Alerts backup will be restored automatically.
- [x] Fixed app not closing sometimes after onclosing
- [x] License

### Version 1.5.0
- [x] Dispose Clients
- [x] Fixed small regression in `Alerts` that make Alert Direction not appear on the UI.
- [x] Binded Alert Intent to the UI again (for reference only)
- [x] Update BinanceAPI.NET to Nuget Package
- [x] Small Improvement to API Key handling
- [x] Small Improvement to error handling
- [x] Reposition 24 Hour Stats
- [x] Refactoring
- [x] Fixed a bug stopping `Isolated` account information from updating properly.
- [x] Fixed an issue with `Price` display on orders for some coins
- [x] Refactoring
- [x] Fix an issue with the Icon 
- [x] Secure API Keys: Once you save the API Keys they will only be usable on the Current PC
- [x] Better handling of API Keys
- [x] API Keys are no longer displayed when opening the settings 
- [x] Improvements to how order updates are processed
- [x] Update Packages
- [x] Improve logging
- [x] Notepad will now save when you close it
- [x] Add About Window
- [x] Change Default Connection Limit
- [x] Change Default Window Sizes
- [x] Watchlist can now be resized vertically: <https://i.ibb.co/RT928nj/watchlist-resize-preview.gif>
- [x] Watchlist can now handle more symbols (Limited by your CPU/Internet Connection)
- [x] Improvements to backups to ensure backups don't get overwritten with garbage
- [x] Some small model changes and general clean up
- [x] Improvements to how orders are stored
- [x] Interest information is now hidden in spot mode: <https://i.ibb.co/1qhHx8f/hide-interest-preview.gif>
- [x] Order tasks that can't be completed are hidden in spot mode
- [x] Settle tab is now hidden in spot mode
- [x] Notepad is now always on top
- [x] Adds a basic notepad: <https://i.ibb.co/7nF5d72/notepad-preview.gif>
- [x] Resize notepad with corner gripper
- [x] Save notes to file by clicking save icon
- [x] Automatically loads saved notes during startup
- [x] Notes will save automatically on program close
- [x] Add Strong Name Requirement
- [x] Main Window can now be resized
- [x] Added corner gripper
- [x] Added close price to 24 Stats
- [x] Major UI Improvements (Still WIP)
- [x] Remove Search Button
- [x] Remove Mini-log (use console logger/file)
- [x] Change global font
- [x] Other small bug fixes
- [x] UI Improvements
- [x] Makes several UI elements semi-transparent: <https://i.imgur.com/263U0xE.gif>
- [x] Window now resizes as you would expect
- [x] Floating items will reset during window resize
- [x] Floating items that get lost when you close the side bar will reset location
- [x] Real Time Quote / Order Widget is now semi-transparent
- [x] Added Option to Buy/Sell the filled amount of an Order with 1 Click (See Readme)
- [x] Fix a bug in Order Update that froze the UI
- [x] Small changes to improve stability
- [x] Fix console icon
- [x] Optimize/Compress images
- [x] Changed how windows open and close
- [x] Updated most icons
- [x] Status window boiler plating for upcoming updating
- [x] You can now reset the running interest for an order
- [x] Watchlist will now restore from backup automatically
- [x] Deleted List will now restore from backup automatically
- [x] Fixed restoring from backup
