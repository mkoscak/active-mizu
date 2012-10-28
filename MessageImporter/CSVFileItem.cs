using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MessageImporter
{
    /// <summary>
    /// Polozka CSVcka
    /// </summary>
    public class CSVFileItem
    {
        /// <summary>
        /// Konstruktor s textom na sparsovanie
        /// </summary>
        /// <param name="toParse">Zoznam poloziek triedy oddeleny ciarkami</param>
        public CSVFileItem(string toParse)
        {
            List<char> tmp = new List<char>(toParse.Length);

            var chars = toParse.ToCharArray();
            bool inside = false;
            foreach (var c in chars)
            {
                char a = c;

                if (a == '"')
                    inside = !inside;

                if (a == ',' && inside)
                    a = '_';

                tmp.Add(a);
            }

            string back = new string(tmp.ToArray());

            var splitted = back.Split(',');
            for (int i = 0; i < splitted.Length; i++)
            {
                // ok je vysledny text bez uvodzoviek a _ je nahradeny desatinnym separatorom
                var ok = splitted[i].Replace("_", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Trim('"');
                switch (i)
                {
                    case 0: OrderNumber = ok; break;
                    case 1: OrderDate = ok; break;
                    case 2: OrderStatus = ok; break;
                    case 3: OrderPurchasedFrom = ok; break;
                    case 4: OrderPaymentMethod = ok; break;
                    case 5: OrderShippingMethod = ok; break;
                    case 6: OrderSubtotal = ok; break;
                    case 7: OrderTax = ok; break;
                    case 8: OrderShipping = ok; break;
                    case 9: OrderDiscount = ok; break;
                    case 10: OrderGrandTotal = ok; break;
                    case 11: OrderPaid = ok; break;
                    case 12: OrderRefunded = ok; break;
                    case 13: OrderDue = ok; break;
                    case 14: TotQtyOrdered = ok; break;
                    case 15: CustomerName = ok; break;
                    case 16: CustomerEmail = ok; break;
                    case 17: ShippingName = ok; break;
                    case 18: ShippingCompany = ok; break;
                    case 19: ShippingStreet = ok; break;
                    case 20: ShippingZip = ok; break;
                    case 21: ShippingCity = ok; break;
                    case 22: ShippingState = ok; break;
                    case 23: ShippingStateName = ok; break;
                    case 24: ShippingCountry = ok; break;
                    case 25: ShippingCountryName = ok; break;
                    case 26: ShippingPhoneNumber = ok; break;
                    case 27: BillingName = ok; break;
                    case 28: BillingCompany = ok; break;
                    case 29: BillingStreet = ok; break;
                    case 30: BillingZip = ok; break;
                    case 31: BillingCity = ok; break;
                    case 32: BillingState = ok; break;
                    case 33: BillingStateName = ok; break;
                    case 34: BillingCountry = ok; break;
                    case 35: BillingCountryName = ok; break;
                    case 36: BillingPhoneNumber = ok; break;
                    case 37: OrderItemIncrement = ok; break;
                    case 38: ItemName = ok; break;
                    case 39: ItemStatus = ok; break;
                    case 40: ItemSKU = ok; break;
                    case 41: ItemOptions = ok; break;
                    case 42: ItemOrigPrice = ok; break;
                    case 43: ItemPrice = ok; break;
                    case 44: ItemQtyOrdered = ok; break;
                    case 45: ItemQtyInvoiced = ok; break;
                    case 46: ItemQtyShipped = ok; break;
                    case 47: ItemQtyCanceled = ok; break;
                    case 48: ItemQtyRefunded = ok; break;
                    case 49: ItemTax = ok; break;
                    case 50: ItemDiscount = ok; break;
                    case 51: ItemTotal = ok; break;

                }
            }
        }

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
                LongSKU = pairProd.ProductCode;
                ShoppingPrice = pairProd.PriceEUR;
                InvoiceDate = pairProd.OrderDate;
            }
        }

        public string LongSKU { get; set; }
        public double ShoppingPrice { get; set; }
        public DateTime InvoiceDate { get; set; }

        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string ItemName { get; set; }
        public string ItemSKU { get; set; }
        public string ItemOptions { get; set; }
        public string ItemOrigPrice { get; set; }
        public string ItemPrice { get; set; }
        public string TotQtyOrdered { get; set; }
        public string ItemTax { get; set; }
        public string ItemDiscount { get; set; }
        public string ItemTotal { get; set; }
        public string OrderStatus { get; set; }
        public string ItemStatus { get; set; }
        public string OrderPurchasedFrom { get; set; }
        public string OrderPaymentMethod { get; set; }
        public string OrderShippingMethod { get; set; }
        public string OrderSubtotal { get; set; }
        public string OrderTax { get; set; }
        public string OrderShipping { get; set; }
        public string OrderDiscount { get; set; }
        public string OrderGrandTotal { get; set; }
        public string OrderPaid { get; set; }
        public string OrderRefunded { get; set; }
        public string OrderDue { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingName { get; set; }
        public string ShippingCompany { get; set; }
        public string ShippingStreet { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingStateName { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingCountryName { get; set; }
        public string ShippingPhoneNumber { get; set; }
        public string BillingName { get; set; }
        public string BillingCompany { get; set; }
        public string BillingStreet { get; set; }
        public string BillingZip { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingStateName { get; set; }
        public string BillingCountry { get; set; }
        public string BillingCountryName { get; set; }
        public string BillingPhoneNumber { get; set; }
        public string OrderItemIncrement { get; set; }
        public string ItemQtyOrdered { get; set; }
        public string ItemQtyInvoiced { get; set; }
        public string ItemQtyShipped { get; set; }
        public string ItemQtyCanceled { get; set; }
        public string ItemQtyRefunded { get; set; }

        public override string ToString()
        {
            return ItemSKU + " - " + ItemName;
        }
    }
}
