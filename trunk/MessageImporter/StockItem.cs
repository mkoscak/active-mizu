﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MessageImporter
{
    public enum StockItemState
    {
        Paired,
        NonPaired,
        Waiting,
        PermanentStorage
    }

    /// <summary>
    /// Polozka objednavky
    /// </summary>
    public class StockItem
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

                return Icons.Complete;
            }
        }

        internal StockItemState? PreviousState { get; set; }
        private StockItemState state;
        [System.ComponentModel.DisplayName("Stav")]
        public StockItemState State
        {
            get
            {
                // refresh stavu
                if (pairProd == null)
                    state = StockItemState.NonPaired;

                else if (pairProd != null && pairProd.Parent.Cancelled)
                    state = StockItemState.PermanentStorage;

                else if (pairProd != null)
                    state = StockItemState.Paired;
                
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

        [System.ComponentModel.DisplayName("Popis")]
        public string Description { get; set; }

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
                if (string.IsNullOrEmpty(sellPriceInv) && pairProd != null)
                    sellPriceInv = PairProduct.ItemPrice;

                if (sellPriceInv != null)
                    Common.CleanPrice(ref sellPriceInv);

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
                if (!string.IsNullOrEmpty(SellPriceInv))
                {
                    sellPriceInvEUR = (double.Parse(SellPriceInv) / 1.2).ToString();    // TODO tax_var
                }
                else
                    SellPriceInv = null;

                return sellPriceInvEUR;
            }

            set
            {
                sellPriceInvEUR = value;
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

        [System.ComponentModel.DisplayName("Nákupná cena bez DPH")]
        public double PriceEURnoTax { get; set; }

        [System.ComponentModel.DisplayName("Nákupná cena bez DPH EUR")]
        public double PriceEURnoTaxEUR { get; set; }

        internal double TotalEUR { get; set; }

        internal double PriceWithDeliveryEUR { get; set; }

        internal double PriceWithDelivery { get; set; }

        internal int Ord_Qty { get; set; }

        internal int Disp_Qty { get; set; }

        internal double Price { get; set; }

        internal double Total { get; set; }

        internal string Currency { get; set; }

        [System.ComponentModel.DisplayName("Dátum obj.")]
        public DateTime OrderDate { get; set; }

        [System.ComponentModel.DisplayName("Číslo faktúry")]
        public FileItem FromFile { get; set; }

        public override string ToString()
        {
            return ProductCode + " - " + Description;
        }
    }
}
