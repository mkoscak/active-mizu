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

        internal StockItem PairProduct { get; set; }

        public Image Icon
        {
            get
            {
                return PairProduct == null ? Icons.NonComplete : Icons.Complete;
            }
        }

        public string PairCode
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ProductCode;
            }
        }

        public string MSG_SKU
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.Description;
            }
        }

        public double BuyingPrice
        {
            get
            {
                return PairProduct == null ? double.NaN : PairProduct.PriceEUR;
            }
        }

        public DateTime Datetime
        {
            get
            {
                return PairProduct == null ? DateTime.Now : PairProduct.OrderDate;
            }
        }

        public string ItemName { get; set; }
        public string ItemSKU { get; set; }
        public string ItemOptions { get; set; }
        public string ItemOrigPrice { get; set; }
        public string ItemPrice { get; set; }
        public string ItemTax { get; set; }
        public string ItemDiscount { get; set; }
        public string ItemTotal { get; set; }
        public string ItemStatus { get; set; }
        public string OrderItemIncrement { get; set; }
        public string ItemQtyOrdered { get; set; }
        public string ItemQtyInvoiced { get; set; }
        public string ItemQtyShipped { get; set; }
        public string ItemQtyCanceled { get; set; }
        public string ItemQtyRefunded { get; set; }
    }
}
