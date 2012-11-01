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
    public enum InvoiceState
    {
        NonComplete,
        Complete,
        Cancelled
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

        public Image Icon
        {
            get
            {
                if (Canceled)
                    return Icons.NonComplete;
                
                Image ret = Icons.Complete;
                
                switch (InvoiceStatus)
                {
                    case InvoiceState.NonComplete:
                        ret = Icons.NonComplete;
                        break;
                    case InvoiceState.Complete:
                        ret = Icons.Complete;
                        break;
                    case InvoiceState.Cancelled:
                        ret = Icons.Waiting;
                        break;

                    default:
                        ret = Icons.Complete;
                        break;
                }

                return ret;
            }
        }

        /// <summary>
        /// Vybavenost objednavky
        /// </summary>
        public bool Equipped
        {
            get
            {
                return InvoiceStatus == InvoiceState.Complete;
            }

            set
            {
                InvoiceStatus = value ? InvoiceState.Complete : InvoiceState.NonComplete;
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

        internal InvoiceState InvoiceStatus;
        internal List<InvoiceItem> InvoiceItems { get; set; }
    }
}
