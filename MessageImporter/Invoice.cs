using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MessageImporter
{
    /// <summary>
    /// stav objednavky
    /// </summary>
    public enum InvoiceStatus
    {
        NonEquipped,
        Equipped
    }

    /// <summary>
    /// Objednavka
    /// </summary>
    public class Invoice
    {
        public Invoice()
        {
            InvoiceItems = new List<InvoiceItem>();
        }

        public Image i
        {
            get
            {
                return Equipped && !Canceled ? Icons.Eqipped : Icons.NonEquipped;
            }
        }

        /// <summary>
        /// Vybavenost objednavky
        /// </summary>
        public bool Equipped
        {
            get
            {
                return InvoiceStatus == InvoiceStatus.Equipped;
            }

            set
            {
                InvoiceStatus = value ? InvoiceStatus.Equipped : InvoiceStatus.NonEquipped;
            }
        }

        // stornovana?
        public bool Canceled { get; set; }

        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string TotQtyOrdered { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string OrderSubtotal { get; set; }
        public string OrderTax { get; set; }
        public string OrderShipping { get; set; }
        public string OrderDiscount { get; set; }
        public string OrderGrandTotal { get; set; }
        public string OrderPaid { get; set; }
        public string OrderRefunded { get; set; }
        public string OrderDue { get; set; }

        public string OrderStatus { get; set; }
        public string OrderPurchasedFrom { get; set; }
        public string OrderPaymentMethod { get; set; }
        public string OrderShippingMethod { get; set; }
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

        internal InvoiceStatus InvoiceStatus;
        internal List<InvoiceItem> InvoiceItems { get; set; }
    }
}
