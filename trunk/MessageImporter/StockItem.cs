using System;
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

        public bool EquippedInv
        {
            get
            {
                if (pairProd == null)
                    return false;

                return pairProd.Parent.Equipped;
            }
        }

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
