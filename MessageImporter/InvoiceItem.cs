using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Diagnostics;
using MessageImporter.Entities;//pre debug

namespace MessageImporter
{
    /// <summary>
    /// Polozka objednavky
    /// </summary>
    [Serializable]
    public class InvoiceItem
    {
        public InvoiceItem()
        {
           // PairProductStack=new List<StockItem>(); 
           //this.PairProductStack = new List<StockItem>();
        }

        public InvoiceItem(InvoiceItem copy, Invoice parent)
        {
            this.BuyingPrice = copy.BuyingPrice;
            this.Datetime = copy.Datetime;
            //this.FromDB = copy.FromDB;
            this.invSKU = copy.invSKU;
            this.ItemDiscount = copy.ItemDiscount;
            this.ItemName = copy.ItemName;
            this.ItemOptions = copy.ItemOptions;
            this.ItemOrigPrice = copy.ItemOrigPrice;
            this.ItemPrice = copy.ItemPrice;
            this.ItemQtyCanceled = copy.ItemQtyCanceled;
            this.ItemQtyInvoiced = copy.ItemQtyInvoiced;
            this.ItemQtyOrdered = copy.ItemQtyOrdered;
            this.ItemQtyRefunded = copy.ItemQtyRefunded;
            this.ItemQtyShipped = copy.ItemQtyShipped;
            this.ItemStatus = copy.ItemStatus;
            this.ItemTax = copy.ItemTax;
            this.ItemTotal = copy.ItemTotal;
            
            this.MSG_SKU = copy.MSG_SKU;
            this.OrderItemIncrement = copy.OrderItemIncrement;
            // dopravu parujeme
            if (copy.PairCode != null && copy.PairCode == Properties.Settings.Default.ShippingCode)
            {
                //this.PairCode = copy.PairCode;
                this.PairProduct = copy.PairProduct.Clone() as StockItem;
               
            }
            this.PairProductStack = copy.PairProductStack;
            this.itemStorage = copy.itemStorage;

            this.Parent = parent;
            this.PredajnaCena = copy.PredajnaCena;
        }

        public InvoiceItem(Invoice parent)
        {
            Parent = parent;
        }

        internal WaitingProductEntity CreatedFromWaiting;
        public InvoiceItem(WaitingProductEntity waitingEnt)
        {
            this.PairCode = waitingEnt.Sku;
            this.invSKU = waitingEnt.InvSku;
            this.MSG_SKU = waitingEnt.Description;
            this.BuyingPrice = waitingEnt.BuyingPrice;
            this.Datetime = Convert.ToDateTime(waitingEnt.Date);
            this.ItemName = waitingEnt.DescriptionWeb;
            this.PredajnaCena = waitingEnt.SellPrice;
            this.ItemOptions = waitingEnt.Size;
            this.ItemOrigPrice = waitingEnt.ItemOrigPrice;
            this.ItemPrice = waitingEnt.ItemPrice;
            this.ItemTax = waitingEnt.ItemTax;
            this.ItemDiscount = waitingEnt.ItemDiscount;
            this.Zlava_Pohoda = waitingEnt.DiscountPohoda;
            this.ItemTotal = waitingEnt.ItemTotal;
            this.ItemStatus = waitingEnt.ItemStatus;
            this.ItemQtyOrdered = waitingEnt.OrdCount;
            this.itemStorage = waitingEnt.Storage;

            //if (!string.IsNullOrEmpty(waitingEnt.Sku))
            {
                StockItem newitem = new StockItem();
                newitem.State = StockItemState.Paired;
                newitem.ProductCode = waitingEnt.Sku;
                newitem.Description = waitingEnt.Description;
                newitem.IsFromDB = true;
                
                this.PairProduct = newitem;
            }

            // zapametame si entitu, z ktorej bola polozka vytvorena
            CreatedFromWaiting = waitingEnt;
        }

        internal List<StockItem> PairProductStack { get; set; }

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
                
                // pripadne odparovanie povodneho
                if (pairProd != null)
                    pairProd.PairProduct = null;

                pairProd = value;
              //  this.itemStorage = pairProd.Sklad;
                if (pairProd != null)
                    pairProd.PairProduct = this;

                if (pairProd != null && pairProd.FromFile != null && pairProd.FromFile.Type == MSG_TYPE.MANDM_DIRECT)
                {
                    ItemOptions = pairProd.Size;    // velkost sa bude preberat z MM faktur

                    var postfix = ItemOptions.Trim().Replace(" ", "");
                    if (postfix.Length > 4)
                        postfix = postfix.Substring(0, 4);

                    pairProd.ProductCode = pairProd.ProductCode + "_" + postfix;
                }

                // ak nema produkt z MSG kod produktu
                if (pairProd != null && pairProd.Description != null && pairProd.Description == pairProd.ProductCode && itemOptions != null)
                {
                    if (pairProd.FromFile != null && pairProd.FromFile.Type == MSG_TYPE.FIVE_POUNDS)
                    {
                        var xi = invSKU.IndexOf('x');
                        if (xi == -1)
                            xi = invSKU.Length;
                        else
                            ++xi;

                        var code = "EFP_" + invSKU.Substring(0, xi) + "_";

                        var si = pairProd.SizeInv.ToLower().IndexOf("size");
                        if (si == -1)
                            si = 0;
                        else
                            si += 5;//za textom "size"

                        var postfix = pairProd.SizeInv.Substring(si, 4);

                        pairProd.ProductCode = code + postfix;
                    }
                    else
                    {
                        var prefix = invSKU;
                        if (!prefix.ToUpper().StartsWith("AG"))
                            prefix = "AG" + prefix;
                        prefix = prefix.Insert(2, "_");

                        var postfix = ItemOptions.Trim().Replace(" ", "");
                        if (postfix.Length > 4)
                            postfix = postfix.Substring(0, 4);

                        pairProd.ProductCode = prefix + "_" + postfix;
                    }
                }
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

            set
            {
                if (PairProduct == null)
                {
                    PairProduct = new StockItem();
                    if (string.IsNullOrEmpty(ItemQtyOrdered))
                        ItemQtyOrdered = (-1).ToString();
                }

                PairProduct.ProductCode = value;
                PairProduct.PairProduct = this;
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
                if (value != null)
                    itemOptions = value.Replace("Veľkosť:", "").Replace("Veľkost:", "").Replace("Velkost:", "").Replace("Méret", "").Replace("Meret", "").Trim();
            }
        }

        public string ItemOrigPrice { get; set; }
        public string ItemPrice { get; set; }
        public string ItemTax { get; set; }
        public string ItemDiscount { get; set; }
        public string Zlava_Pohoda { get; set; }
        public string ItemTotal { get; set; }
        public string ItemStatus { get; set; }
        
        internal string OrderItemIncrement { get; set; }

        [System.ComponentModel.DisplayName("Počet ks")]
        public string ItemQtyOrdered { get; set; }

        [System.ComponentModel.DisplayName("Sklad")]
        public string itemStorage// { get; set; }
        {
        /*    get
            {
                return pairProd.Sklad;
               var x= pairProd.Sklad;
                Debug.WriteLine(PairProduct == null ? string.Empty : PairProduct.Sklad);
                return PairProduct == null ? string.Empty : PairProduct.Description;
            }

            set
            {
                if (PairProduct != null)
                    PairProduct.Description = value;
                Debug.WriteLine(PairProduct.Description);
            }*/
           /**/

            get
            {
                return PairProduct == null ? string.Empty : PairProduct.Sklad;
            }

            set
            {
                if (PairProduct != null)
                    PairProduct.Sklad = value;
            }
        }

        internal string ItemQtyInvoiced { get; set; }
        internal string ItemQtyShipped { get; set; }
        internal string ItemQtyCanceled { get; set; }
        internal string ItemQtyRefunded { get; set; }

        internal string OrderNumber { get; set; }

        /// <summary>
        /// Vytvori novu entitu waiting produkt s null ID
        /// </summary>
        /// <returns></returns>
        public WaitingProductEntity GetWaitingEntity()
        {
            if (CreatedFromWaiting != null)
                return CreatedFromWaiting;

            var ret = new WaitingProductEntity();
            ret.Valid = true;
            ret.Sku = this.PairCode;
            ret.InvSku = this.invSKU;
            ret.Description = this.MSG_SKU;
            ret.BuyingPrice = this.BuyingPrice;
            ret.Date = this.Datetime.ToString();
            ret.DescriptionWeb = this.ItemName;
            ret.SellPrice = this.PredajnaCena;
            ret.Size = this.ItemOptions;
            ret.ItemOrigPrice = this.ItemOrigPrice;
            ret.ItemPrice = this.ItemPrice;
            ret.ItemTax = this.ItemTax;
            ret.ItemDiscount = this.ItemDiscount;
            ret.DiscountPohoda = this.Zlava_Pohoda;
            ret.ItemTotal = this.ItemTotal;
            ret.ItemStatus = this.ItemStatus;
            ret.OrdCount = this.ItemQtyOrdered;
            ret.Storage = this.itemStorage;

            // cislo objednavky
            ret.InvoiceNr = this.Parent.OrderNumber;

            return ret;
        }
    }
}