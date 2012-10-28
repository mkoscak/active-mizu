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
    public class StockItem
    {
        internal InvoiceItem PairProduct { get; set; }

        public Image Icon
        {
            get
            {
                return Equipped ? Icons.Eqipped : Icons.NonEquipped;
            }
        }

        public bool Equipped { get; set; }

        public string ProductCode { get; set; }

        public string Description { get; set; }

        public string ItemNameInv
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ItemName;
            }
        }

        public string SellPriceInv
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ItemPrice;
            }
        }

        public string SizeInv
        {
            get
            {
                return PairProduct == null ? string.Empty : PairProduct.ItemOptions;
            }
        }

        public double PriceEUR { get; set; }

        public double TotalEUR { get; set; }

        public double PriceWithDeliveryEUR { get; set; }

        public double PriceWithDelivery { get; set; }

        public int Ord_Qty { get; set; }

        public int Disp_Qty { get; set; }

        public double Price { get; set; }

        public double Total { get; set; }

        public string Currency { get; set; }

        public DateTime OrderDate { get; set; }

        public FileItem FromFile { get; set; }

        public override string ToString()
        {
            return ProductCode + " - " + Description;
        }
    }
}
