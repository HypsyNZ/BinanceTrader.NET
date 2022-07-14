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

using BinanceAPI.Objects.Spot.LendingData;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BVVM;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using LoopDelay.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class FlexibleViewModel : ObservableObject
    {
        private const int LOOP_EXPIRE_MS = 2500;
        private const int LOOP_DELAY_MS = 250;
        private const int INTEREST_ROUNDING_PLACES = 5;
        private const int ONE_HUNDRED = 100;
        private const int RECV_WINDOW = 3000;

        public ICommand? AddPositionCommand { get; set; }

        public ICommand? RedeemSelectedPositionCommand { get; set; }

        private readonly object PositionLock = new object();
        private readonly object ProductLock = new object();

        private decimal subscribeAmount;
        private decimal? possibleSubscribeAmount;
        private decimal? redeemAmount;
        private bool subscribeEnabled;
        private bool redeemEnabled;
        private bool productSelectionEnabled;
        private bool redeemAmountVisible;
        private bool redeemAmountEnabled;
        private bool subscribeAmountVisible;
        private string? selectedProduct = "";
        private string? selectedPositionString = "";
        private string? resetText;
        private string? possibleRatesCurrentAsset;

        private BinanceFlexibleProductPosition? selectedPosition = null;
        private BinanceSavingsProduct? selectedSavingsProduct = null;
        private ObservableCollection<string> allProductsSelection = new ObservableCollection<string>();

        private IEnumerable<BinanceSavingsProduct> allProducts = new Collection<BinanceSavingsProduct>();
        private IEnumerable<BinanceFlexibleProductPosition> allPositions = new Collection<BinanceFlexibleProductPosition>();

        public void InitializeCommands()
        {
            AddPositionCommand = new DelegateCommand(AddPosition);
            RedeemSelectedPositionCommand = new DelegateCommand(FastRedeemPosition);
        }

        public ObservableCollection<string> AllProductsSelection
        {
            get => allProductsSelection; set
            {
                allProductsSelection = value;
                PC();
            }
        }

        private BinanceSavingsProduct? SelectedSavingsProduct
        {
            get => selectedSavingsProduct;
            set
            {
                selectedSavingsProduct = value;
                PC();
            }
        }

        public IEnumerable<BinanceSavingsProduct> AllProducts
        {
            get => allProducts; set
            {
                allProducts = value;
                PC();
            }
        }

        public IEnumerable<BinanceFlexibleProductPosition> AllPositions
        {
            get => allPositions;
            set
            {
                allPositions = value;
                PC();
            }
        }

        public BinanceFlexibleProductPosition? SelectedPosition
        {
            get => selectedPosition;
            set
            {
                selectedPosition = value;
                PC();

                if (value != null)
                {
                    RedeemEnabled = value.CanRedeem;
                    RedeemAmountEnabled = value.CanRedeem;
                    RedeemAmountVisible = value.CanRedeem;

                    SelectedPositionString = value.ProductName;

                    RedeemAmount = GetPositionFullAmount(value.Asset);
                }
                else
                {
                    RedeemEnabled = false;
                    RedeemAmountEnabled = false;
                    RedeemAmountVisible = false;
                    SelectedPositionString = null;
                }
            }
        }

        public string? SelectedProduct
        {
            get => selectedProduct; set
            {
                selectedProduct = value;
                PC();

                SelectedSavingsProduct = GetProduct(value);

                if (SelectedSavingsProduct != null)
                {
                    SubscribeEnabled = SelectedSavingsProduct.CanPurchase;
                    SubscribeAmountVisible = SelectedSavingsProduct.CanPurchase;
                    PossibleSubscribeAmount = GetPossibleSubAmount();
                    PossibleRatesCurrentAsset = GetPossibleRates();
                }
                else
                {
                    SubscribeAmountVisible = false;
                    SubscribeEnabled = false;
                    PossibleSubscribeAmount = 0;
                    PossibleRatesCurrentAsset = null;
                }
            }
        }

        public bool SubscribeEnabled
        {
            get => subscribeEnabled;
            set
            {
                subscribeEnabled = value;
                PC();
            }
        }

        public bool RedeemEnabled
        {
            get => redeemEnabled;
            set
            {
                redeemEnabled = value;
                PC();
            }
        }

        public string? PossibleRatesCurrentAsset
        {
            get => possibleRatesCurrentAsset;
            set
            {
                possibleRatesCurrentAsset = value;
                PC();
            }
        }

        public string? SelectedPositionString
        {
            get => selectedPositionString;
            set
            {
                selectedPositionString = value;
                PC();
            }
        }

        public decimal SubscribeAmount
        {
            get => subscribeAmount;
            set
            {
                subscribeAmount = value;
                PC();
            }
        }

        public bool SubscribeAmountVisible
        {
            get => subscribeAmountVisible;
            set
            {
                subscribeAmountVisible = value;
                PC();
            }
        }

        public decimal? PossibleSubscribeAmount
        {
            get => possibleSubscribeAmount;
            set
            {
                possibleSubscribeAmount = value;
                PC();

                SubscribeAmount = (decimal)possibleSubscribeAmount!;
            }
        }

        public bool ProductSelectionEnabled
        {
            get => productSelectionEnabled;
            set
            {
                productSelectionEnabled = value;
                PC();
            }
        }

        public string? ResetText
        {
            get => resetText;
            set
            {
                resetText = value;
                PC();
            }
        }

        public bool RedeemAmountEnabled
        {
            get => redeemAmountEnabled;
            set
            {
                redeemAmountEnabled = value;
                PC();
            }
        }

        public bool RedeemAmountVisible
        {
            get => redeemAmountVisible;
            set
            {
                redeemAmountVisible = value;
                PC();
            }
        }

        public decimal? RedeemAmount
        {
            get => redeemAmount;
            set
            {
                redeemAmount = value;
                PC();
            }
        }

        /// <summary>
        /// Gets/Updates all current Flexible Products
        /// </summary>
        public async Task GetAllProductsAsync()
        {
            ResetText = "Loading";
            ProductSelectionEnabled = false;

            var update = await Flexible.GetAllFlexibleProductsListAsync().ConfigureAwait(false);
            if (update.Count() > 0)
            {
                lock (ProductLock)
                {
                    AllProducts = update;
                }

                AllProductsSelection = new();
                ClearSelected();

                foreach (var product in AllProducts)
                {
                    AllProductsSelection.Add(product.Asset);
                }
            }

            ProductSelectionEnabled = true;
            ResetText = "";
        }

        /// <summary>
        /// Gets/Updates all current Flexible Product Positions
        /// </summary>
        /// <param name="expect">true if the result is expected to have changed</param>
        /// <returns></returns>
        public async Task GetAllPositionsAsync(bool expect)
        {
            var positions = AllPositions;
            var start = DateTime.UtcNow.Ticks;
            while (await Loop.Delay(start, LOOP_DELAY_MS, LOOP_EXPIRE_MS, () =>
            {
                _ = GetAllPositionsAsync(false).ConfigureAwait(false);
            }))
            {
                var result = await Client.Local.Lending.GetFlexibleProductPositionAsync().ConfigureAwait(false);
                if (result != null)
                {
                    if (expect)
                    {
                        if (positions.Count() != result.Data.Count())
                        {
                            lock (PositionLock)
                            {
                                AllPositions = result.Data;
                                break;
                            }
                        }
                    }
                    else
                    {
                        lock (PositionLock)
                        {
                            AllPositions = result.Data;
                            break;
                        }
                    }
                }
            }
        }

        public void ClearSelected()
        {
            SelectedProduct = null;
            SelectedSavingsProduct = null;
            SelectedPosition = null;
            RedeemEnabled = false;
            SubscribeEnabled = false;

            RedeemAmount = 0;
            RedeemAmountEnabled = false;
            RedeemAmountVisible = false;

            SubscribeAmount = 0;
        }

        internal async Task RefreshAsync()
        {
            ProductSelectionEnabled = false;
            SubscribeEnabled = false;
            RedeemEnabled = false;

            ClearSelected();

            await GetAllPositionsAsync(true).ConfigureAwait(false);

            ResetText = "";
            ProductSelectionEnabled = true;
        }

        /// <summary>
        ///  Adds a Flexible Product Position
        /// </summary>
        public void AddPosition(object o)
        {
            if (SelectedSavingsProduct == null || SelectedProduct == null)
            {
                return;
            }

            SubscribeEnabled = false;
            RedeemEnabled = false;
            ProductSelectionEnabled = false;
            RedeemAmountEnabled = false;

            WriteLog.Info("Selected Flexible Product: " + SelectedProduct + " | Subscribe Amount: " + SubscribeAmount);

            Task.Run(async () =>
            {
                var id = SelectedSavingsProduct.ProductId;
                if (id == null)
                {
                    WriteLog.Error("Couldn't find product ID for selected flexible product");
                    return;
                }

                if (SubscribeAmount == 0)
                {
                    WriteLog.Error("Subscribe amount was zero so we didn't bother sending a request, updated positions instead");
                    await GetAllPositionsAsync(false).ConfigureAwait(false);
                    return;
                }

                if (SubscribeAmount > SelectedSavingsProduct.MinimalPurchaseAmount)
                {
                    ResetText = "Purchasing";
                    var result = await Client.Local.Lending.PurchaseFlexibleProductAsync(id, SubscribeAmount, receiveWindow: RECV_WINDOW).ConfigureAwait(false);
                    if (!result.Success)
                    {
                        WriteLog.Error("Flexible Exception: " + (result.Error?.Message ?? "Unknown Exception"));
                    }
                }
                else
                {
                    Message.ShowBox("The Subscribe amount is below the Minimum Amount allowed", "Not Enough");
                    WriteLog.Error("Flexible Exception: Amount is below the minimum purchase amount");
                }

                await RefreshAsync().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Redeems a Flexible Product Position in Fast Mode
        /// </summary>
        /// <param name="o"></param>
        public void FastRedeemPosition(object o)
        {
            if (SelectedPosition == null)
            {
                return;
            }

            SubscribeEnabled = false;
            RedeemEnabled = false;
            RedeemAmountEnabled = false;
            ProductSelectionEnabled = false;
            ResetText = "Redeeming";

            WriteLog.Info("Redeem Name: " + SelectedPosition.ProductName + "| ProductID: " + SelectedPosition.ProductId + "| Redeem Amount: " + RedeemAmount);

            Task.Run(async () =>
            {
                if (RedeemAmount != 0)
                {
                    var result = await Client.Local.Lending.RedeemFlexibleProductAsync(SelectedPosition.ProductId, RedeemAmount!.Value, BinanceAPI.Enums.RedeemType.Fast, receiveWindow: RECV_WINDOW).ConfigureAwait(false);
                    if (!result.Success)
                    {
                        WriteLog.Error("Flexible Exception: " + (result.Error?.Message ?? "Unknown Exception"));
                    }
                }
                else
                {
                    WriteLog.Error("Redeem Amount was Zero");
                }

                await RefreshAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        internal decimal? GetPossibleSubAmount()
        {
            BinanceBalance? subAmount;

            lock (Assets.SpotAssetLock)
            {
                subAmount = Assets.SpotAssets.Where(t => t.Asset == SelectedSavingsProduct?.Asset).FirstOrDefault();
            }

            if (subAmount != null)
            {
                return subAmount.Available;
            }
            else
            {
                return 0;
            }
        }

        internal string? GetPossibleRates()
        {
            if (SelectedSavingsProduct != null)
            {
                if (SelectedSavingsProduct.TierAnnualInterestRate.Count > 0)
                {
                    var Tier1 = SelectedSavingsProduct.TierAnnualInterestRate.ElementAtOrDefault(1);
                    var Tier2 = SelectedSavingsProduct.TierAnnualInterestRate.ElementAtOrDefault(0);
                    var Tier3 = SelectedSavingsProduct.TierAnnualInterestRate.ElementAtOrDefault(2);

                    var tier = "";

                    if (Tier1.Key != null)
                    {
                        tier = "Annual Interest Rates: " + Tier1.Key + "  " + (decimal.Round(Tier1.Value, INTEREST_ROUNDING_PLACES) * ONE_HUNDRED) + "%";
                    }

                    if (Tier2.Key != null)
                    {
                        tier = tier + " || " + Tier2.Key + "  " + (decimal.Round(Tier2.Value, INTEREST_ROUNDING_PLACES) * ONE_HUNDRED) + "%";
                    }

                    if (Tier3.Key != null)
                    {
                        tier = tier + " || " + Tier3.Key + "  " + (decimal.Round(Tier3.Value, INTEREST_ROUNDING_PLACES) * ONE_HUNDRED) + "%";
                    }

                    return tier;
                }
                else
                {
                    return "Annual Interest Rate: " + (decimal.Round(SelectedSavingsProduct.AverageAnnualInterestRate, INTEREST_ROUNDING_PLACES) * ONE_HUNDRED).ToString() + "%";
                }
            }
            return null;
        }

        internal BinanceSavingsProduct? GetProduct(string? asset)
        {
            if (asset == null)
            {
                return null;
            }

            lock (ProductLock)
            {
                return AllProducts.Where(t => t.Asset == asset).FirstOrDefault();
            }
        }

        internal decimal? GetPositionFullAmount(string? asset)
        {
            if (asset == null)
            {
                return null;
            }

            lock (PositionLock)
            {
                return AllPositions.Where(t => t.Asset == asset).FirstOrDefault().TotalAmount;
            }
        }
    }
}
