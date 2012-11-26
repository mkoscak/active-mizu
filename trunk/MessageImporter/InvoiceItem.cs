﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MessageImporter
{
    /// <summary>
    /// Polozka objednavky
    /// </summary>
    public class InvoiceItem
    {
        public InvoiceItem()
        {
        }

        public InvoiceItem(Invoice parent)
        {
            Parent = parent;
        }

        internal Invoice Parent { get; set; }

        private StockItem pairProd;
        internal StockItem PairProduct
        {
            get
            {
                return pairProd;
            }

            set
            {
                pairProd = value;
                if (pairProd != null)
                    pairProd.PairProduct = this;
            }
        }

        public Image Icon
        {
            get
            {
                return PairProduct == null ? Icons.NonComplete : Icons.Complete;
            }
        }

        [System.ComponentModel.DisplayName("Z DB?")]
        public bool FromDB
        {
            get
            {
                if (PairProduct != null)
                    return PairProduct.IsFromDB;

                return false;
            }
        }

        [System.ComponentModel.DisplayName("SKU")]
        public string PairCode
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ProductCode;
            }
        }

        public string invSKU { get; set; }

        [System.ComponentModel.DisplayName("Popis")]
        public string MSG_SKU
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.Description;
            }

            set
            {
                if (PairProduct != null)
                    PairProduct.Description = value;
            }
        }

        public double BuyingPrice
        {
            get
            {
                return PairProduct == null ? double.NaN : PairProduct.PriceEURnoTax;
            }

            set
            {
                if (PairProduct != null)
                    PairProduct.PriceEURnoTax = value;
            }
        }

        [System.ComponentModel.DisplayName("Dátum")]
        public DateTime Datetime
        {
            get
            {
                return PairProduct == null ? DateTime.Now : PairProduct.OrderDate;
            }

            set
            {
                if (PairProduct != null)
                    PairProduct.OrderDate = value;
            }
        }

        [System.ComponentModel.DisplayName("Popis web")]
        public string ItemName { get; set; }

        public double predajnaCena;
        [System.ComponentModel.DisplayName("Predajná cena")]
        public double PredajnaCena
        {
            get
            {
                predajnaCena = Common.GetPrice(ItemOrigPrice) - (Common.GetPrice(ItemDiscount) / Common.GetPrice(ItemQtyOrdered));
                predajnaCena = Math.Round(predajnaCena, 2);

                return predajnaCena;
            }

            set
            {
                predajnaCena = value;
            }
        }

        public string itemOptions;
        [System.ComponentModel.DisplayName("Veľkosť")]
        public string ItemOptions
        {
            get
            {
                return itemOptions;
            }

            set
            {
                itemOptions = value.Replace("Veľkosť:", "").Replace("Veľkost:", "").Replace("Velkost:", "").Replace("Méret", "").Replace("Meret", "").Trim();
            }
        }

        public string ItemOrigPrice { get; set; }
        public string ItemPrice { get; set; }
        public string ItemTax { get; set; }
        public string ItemDiscount { get; set; }
        public string ItemTotal { get; set; }
        public string ItemStatus { get; set; }
        
        internal string OrderItemIncrement { get; set; }

        [System.ComponentModel.DisplayName("Počet ks")]
        public string ItemQtyOrdered { get; set; }

        internal string ItemQtyInvoiced { get; set; }
        internal string ItemQtyShipped { get; set; }
        internal string ItemQtyCanceled { get; set; }
        internal string ItemQtyRefunded { get; set; }
    }
}
