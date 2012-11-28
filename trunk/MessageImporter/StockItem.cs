using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace MessageImporter
{
    public enum StockItemState
    {
        Paired,
        Unpaired,
        Waiting,
        PermanentStorage
    }

    /// <summary>
    /// Polozka objednavky
    /// </summary>
    public class StockItem : ICloneable
    {
        private InvoiceItem pairProd;
        internal InvoiceItem PairProduct 
        {
            get
            {
                return pairProd;
            }

            set
            {
                pairProd = value;
                if (pairProd.Parent.Cancelled)
                    State = StockItemState.PermanentStorage;    // ak je objednavka zrusena produkt ide na permanent storage
            }
        }

        [System.ComponentModel.DisplayName("Icon")]
        public Image Icon
        {
            get
            {
                if (pairProd == null)
                    return Icons.NonComplete;

                if (pairProd != null && state == StockItemState.PermanentStorage)
                    return Icons.Warning;

                if (state == StockItemState.Waiting)
                    return Icons.Waiting;

                return Icons.Complete;
            }
        }

        // polozka bola nacitana z DB
        internal bool IsFromDB { get; set; }

        internal StockItemState? PreviousState { get; set; }
        private StockItemState state;
        [System.ComponentModel.DisplayName("Stav")]
        public StockItemState State
        {
            get
            {
                // refresh stavu
                if (pairProd == null)
                    state = StockItemState.Unpaired;

                else if (pairProd != null && pairProd.Parent.Cancelled)
                {
                    state = StockItemState.PermanentStorage;
                    if (string.IsNullOrEmpty(Sklad))
                        Sklad = "02";
                }
                else if (pairProd != null && state != StockItemState.Waiting)
                {
                    state = StockItemState.Paired;

                    if (string.IsNullOrEmpty(Sklad))
                        Sklad = "01";
                }

                return state;
            }

            set
            {
                PreviousState = state;
                state = value;
            }
        }

        [System.ComponentModel.DisplayName("Obj. vybavená")]
        public bool EquippedInv
        {
            get
            {
                if (pairProd == null)
                    return false;

                return pairProd.Parent.Equipped;
            }
        }

        [System.ComponentModel.DisplayName("SKU")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Zoznam stringov na nahradenie budu sa plnit z main okna
        /// </summary>
        internal static BindingList<ReplacementPair> Replacements { get; set; }

        private string description;
        [System.ComponentModel.DisplayName("Popis")]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;

                if (Replacements != null)
                {
                    foreach (var rep in Replacements)
                    {
                        if (description.Contains(rep.ValueToFind))
                        {
                            description = description.Replace(rep.ValueToFind, rep.ValueToReplace == "<empty>" ? "" : rep.ValueToReplace);
                        }
                    }
                }
            }
        }

        private string itemNameInv;
        [System.ComponentModel.DisplayName("Popis z objednávky")]
        internal string ItemNameInv
        {
            get
            {
                if (itemNameInv == null && pairProd != null)
                    itemNameInv = PairProduct.ItemName;

                return itemNameInv;
            }

            set
            {
                itemNameInv = value;
            }
        }


        private string sellPriceInv;
        [System.ComponentModel.DisplayName("Predajná cena")]
        public string SellPriceInv
        {
            get
            {
                if (string.IsNullOrEmpty(sellPriceInv) && PairProduct != null)
                {
                    var config = new CountrySetting(PairProduct.Parent.Country);
                    var price= Common.GetPrice(PairProduct.ItemPrice);
                    var discount = Common.GetPrice(PairProduct.ItemDiscount);
                    var quantity= Common.GetPrice(PairProduct.ItemQtyOrdered);

                    sellPriceInv = Math.Round((price - ((discount/config.Tax)*quantity)), 2).ToString();
                }

                if (sellPriceInv != null)
                    sellPriceInv = Common.CleanPrice(sellPriceInv);

                return sellPriceInv;
            }

            set
            {
                sellPriceInv = value;
            }
        }

        private string sellPriceInvEUR;
        [System.ComponentModel.DisplayName("Predajná cena EUR")]
        public string SellPriceEUR
        {
            get
            {
                if (!string.IsNullOrEmpty(SellPriceInv) && PairProduct != null)
                {
                    var config = new CountrySetting(PairProduct.Parent.Country);

                    sellPriceInvEUR = Math.Round(Common.GetPrice(SellPriceInv) / config.ExchangeRate, 2).ToString();
                }
                else
                    sellPriceInvEUR = null;

                return sellPriceInvEUR;
            }

            set
            {
                sellPriceInvEUR = value;
            }
        }

        private string fictivePrice;
        [System.ComponentModel.DisplayName("Fiktívna cena s DPH")]
        public string FictivePrice
        {
            get
            {
                if (PairProduct != null)
                {
                    fictivePrice = PairProduct.PredajnaCena.ToString(); //Math.Round(Common.GetPrice(PairProduct.ItemOrigPrice) - Common.GetPrice(PairProduct.ItemDiscount), 2).ToString();
                }

                return fictivePrice;
            }

            set
            {
                fictivePrice = value;
            }
        }

        private string sizeInv;
        [System.ComponentModel.DisplayName("Veľkosť")]
        public string SizeInv
        {
            get
            {
                if (sizeInv == null && pairProd != null)
                    sizeInv = PairProduct.ItemOptions;

                return sizeInv;
            }

            set
            {
                sizeInv = value;
            }
        }

        internal bool ChangeColor;
        /// <summary>
        /// Zoznam stringov na nahradenie budu sa plnit z main okna
        /// </summary>
        internal static BindingList<ChildItem> ChildItems { get; set; }

        private double priceEURnoTax;
        [System.ComponentModel.DisplayName("Nákupná cena bez DPH")]
        public double PriceEURnoTax
        {
            get
            {
                if (FromFile != null && FromFile != null)
                    priceEURnoTax = Price * FromFile.ExchRate;

                double tax = 1.2;

                if (ChildItems != null && description != null)
                {
                    var found = ChildItems.Where(ci => description.Contains(ci.ItemText));
                    if (found != null && found.Count() > 0)
                    {
                        tax = 1.0;
                        ChangeColor = true;
                    }
                }

                if (FromFile != null && FromFile.FileName.Contains("refund"))
                    tax = 1.0;

                priceEURnoTax /= tax;

                return Math.Round(priceEURnoTax, 2);
            }

            set
            {
                priceEURnoTax = value;
            }
        }

        private double priceEURnoTaxEUR;
        private bool computePriceEURnoTaxEUR = true;
        [System.ComponentModel.DisplayName("Nákupná cena bez DPH EUR")]
        public double PriceEURnoTaxEUR
        {
            get
            {
                if (!double.IsNaN(PriceEURnoTax) && FromFile != null && !double.IsNaN(FromFile.ExchRate) && !double.IsNaN(FromFile.Delivery) && FromFile.ProdCount > 0 && computePriceEURnoTaxEUR)
                {
                    //var config = new CountrySetting(PairProduct.Parent.Country);
                    priceEURnoTaxEUR = Math.Round((PriceEURnoTax * FromFile.ExchRate) + (FromFile.Delivery * FromFile.ExchRate / FromFile.ProdCount), 2);
                    computePriceEURnoTaxEUR = false;    // tato hodnota sa bude pocitat len raz
                }

                return priceEURnoTaxEUR;
            }

            set
            {
                priceEURnoTaxEUR = value;
            }
        }

        internal double TotalEUR { get; set; }

        internal double PriceWithDeliveryEUR { get; set; }

        internal double PriceWithDelivery { get; set; }

        internal int Ord_Qty { get; set; }

        internal int Disp_Qty { get; set; }

        internal double Price { get; set; }

        internal double Total { get; set; }

        internal string Currency { get; set; }

        private string sklad;
        public string Sklad
        {
            get
            {
                return sklad;
            }

            set
            {
                sklad = value;
            }
        }

        [System.ComponentModel.DisplayName("Dátum obj.")]
        public DateTime OrderDate { get; set; }

        [System.ComponentModel.DisplayName("Číslo faktúry")]
        public FileItem FromFile { get; set; }

        public override string ToString()
        {
            return ProductCode + " - " + Description;
        }

        #region ICloneable Members

        public object Clone()
        {
            StockItem newObj = new StockItem();

            newObj.ChangeColor = ChangeColor;
            newObj.Currency = Currency;
            newObj.Description = Description;
            newObj.Disp_Qty = Disp_Qty;
            //newObj.EquippedInv = EquippedInv;
            newObj.FictivePrice = FictivePrice;
            newObj.FromFile = FromFile;
            //newObj.Icon = Icon;
            newObj.IsFromDB = IsFromDB;
            newObj.ItemNameInv = ItemNameInv;
            newObj.Ord_Qty = Ord_Qty;
            newObj.OrderDate = OrderDate;
            if (PairProduct != null)
                newObj.PairProduct = PairProduct;
            newObj.PreviousState = PreviousState;
            newObj.Price = Price;
            newObj.PriceEURnoTax = PriceEURnoTax;
            newObj.PriceEURnoTaxEUR = PriceEURnoTaxEUR;
            newObj.PriceWithDelivery = PriceWithDelivery;
            newObj.PriceWithDeliveryEUR = PriceWithDeliveryEUR;
            newObj.ProductCode = ProductCode;
            newObj.SellPriceEUR = SellPriceEUR;
            newObj.SellPriceInv = SellPriceInv;
            newObj.SizeInv = SizeInv;
            newObj.Sklad = Sklad;
            newObj.State = State;
            newObj.Total = Total;
            newObj.TotalEUR = TotalEUR;

            return newObj;
        }

        #endregion
    }
}
