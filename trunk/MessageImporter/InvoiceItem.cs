using System;
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

        [System.ComponentModel.DisplayName("SKU")]
        public string PairCode
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ProductCode;
            }
        }

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

        internal string ItemSKU { get; set; }

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
