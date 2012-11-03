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
                // zrusena - cervenou
                if (Cancelled)
                    return Icons.NonComplete;

                // ak obsahuje nesprarovane - oranzova
                if (!Equipped)
                    return Icons.Warning;

                return Icons.Complete;
            }
        }

        /// <summary>
        /// Vybavenost objednavky
        /// </summary>
        [System.ComponentModel.DisplayName("Vybavená")]
        public bool Equipped
        {
            get;
            set;
        }

        // stornovana?
        [System.ComponentModel.DisplayName("Zrušená")]
        public bool Cancelled 
        { 
            get
            {
                return InvoiceStatus == InvoiceState.Cancelled;
            }

            set
            {
                if (value)
                    InvoiceStatus = InvoiceState.Cancelled;
                else
                    InvoiceStatus = Equipped ? InvoiceState.Complete : InvoiceState.NonComplete;
            }
        }

        [System.ComponentModel.DisplayName("Čís.obj.")]
        public string OrderNumber { get; set; }
        [System.ComponentModel.DisplayName("Dátum obj.")]
        public string OrderDate { get; set; }
        [System.ComponentModel.DisplayName("Položiek")]
        public string TotQtyOrdered { get; set; }
        [System.ComponentModel.DisplayName("Meno")]
        public string CustomerName { get; set; }
        [System.ComponentModel.DisplayName("E-mail")]
        public string CustomerEmail { get; set; }

        internal string OrderSubtotal { get; set; }
        internal string OrderTax { get; set; }

        [System.ComponentModel.DisplayName("Cena za dopravu")]
        public string OrderShipping { get; set; }

        internal string OrderDiscount { get; set; }

        [System.ComponentModel.DisplayName("Cena za obj. s DPH")]
        public string OrderGrandTotal { get; set; }

        internal string OrderPaid { get; set; }
        internal string OrderRefunded { get; set; }
        internal string OrderDue { get; set; }
        
        [System.ComponentModel.DisplayName("Status")]
        public string OrderStatus { get; set; }
        
        internal string OrderPurchasedFrom { get; set; }

        [System.ComponentModel.DisplayName("Platobná metóda")]
        public string OrderPaymentMethod { get; set; }
        [System.ComponentModel.DisplayName("Spôsob dopravy")]
        public string OrderShippingMethod { get; set; }
        [System.ComponentModel.DisplayName("ShippingName")]
        public string ShippingName { get; set; }

        internal string ShippingCompany { get; set; }

        [System.ComponentModel.DisplayName("ShippingStreet")]
        public string ShippingStreet { get; set; }
        [System.ComponentModel.DisplayName("ShippingZip")]
        public string ShippingZip { get; set; }
        [System.ComponentModel.DisplayName("ShippingCity")]
        public string ShippingCity { get; set; }

        internal string ShippingState { get; set; }
        internal string ShippingStateName { get; set; }

        [System.ComponentModel.DisplayName("ShippingCountry")]
        public string ShippingCountry { get; set; }

        internal string ShippingCountryName { get; set; }

        [System.ComponentModel.DisplayName("ShippingPhoneNumber")]
        public string ShippingPhoneNumber { get; set; }
        [System.ComponentModel.DisplayName("BillingName")]
        public string BillingName { get; set; }
        
        internal string BillingCompany { get; set; }

        [System.ComponentModel.DisplayName("BillingStreet")]
        public string BillingStreet { get; set; }
        [System.ComponentModel.DisplayName("BillingZip")]
        public string BillingZip { get; set; }
        [System.ComponentModel.DisplayName("BillingCity")]
        public string BillingCity { get; set; }

        internal string BillingState { get; set; }
        internal string BillingStateName { get; set; }
        
        [System.ComponentModel.DisplayName("BillingCountry")]
        public string BillingCountry { get; set; }
        
        internal string BillingCountryName { get; set; }
        
        [System.ComponentModel.DisplayName("BillingPhoneNumber")]
        public string BillingPhoneNumber { get; set; }

        private InvoiceState invoiceStatus;
        internal InvoiceState InvoiceStatus
        {
            get
            {
                return invoiceStatus;
            }

            set
            {
                invoiceStatus = value;
                if (invoiceStatus == InvoiceState.Cancelled)
                {
                    foreach (var item in InvoiceItems)
                    {
                        if (item.PairProduct != null)
                            item.PairProduct.State = StockItemState.PermanentStorage;
                    }
                }
                else
                {
                    foreach (var item in InvoiceItems)
                    {
                        if (item.PairProduct != null && item.PairProduct.State == StockItemState.PermanentStorage && item.PairProduct.PreviousState.HasValue)
                            item.PairProduct.State = item.PairProduct.PreviousState.Value;
                    }
                }
            }
        }

        internal List<InvoiceItem> InvoiceItems { get; set; }
    }
}
