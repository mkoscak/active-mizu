using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MessageImporter.Entities
{
    /// <summary>
    /// DB entita cakajuceho produktu - invoice item
    /// </summary>
    public class WaitingProductEntity : BaseEntity<WaitingProductEntity>
    {
        internal bool Modified;
        [DisplayName("Modified?")]
        public string ModifiedFlag
        {
            get
            {
                if (Modified)
                    return "*";

                return string.Empty;
            }
        }

        public string InvoiceNr { get; set; }
        public string Sku { get; set; }
        public string InvSku { get; set; }
        public string Description { get; set; }
        public double BuyingPrice { get; set; }
        public string Date { get; set; }
        public string DescriptionWeb { get; set; }
        public double SellPrice { get; set; }
        public string Size { get; set; }
        public string ItemOrigPrice { get; set; }
        public string ItemPrice { get; set; }
        public string ItemTax { get; set; }
        public string ItemDiscount { get; set; }
        public string DiscountPohoda { get; set; }
        public string ItemTotal { get; set; }
        public string ItemStatus { get; set; }
        public string OrdCount { get; set; }
        public string Storage { get; set; }

        static string INVOICE_NR = "INVOICE_NR";
        static string SKU = "SKU";
        static string INV_SKU = "INV_SKU";
        static string DESCRIPTION = "DESCRIPTION";
        static string BUYING_PRICE = "BUYING_PRICE";
        static string ORDER_DATE = "ORDER_DATE";
        static string DESCRIPTION_WEB = "DESCRIPTION_WEB";
        static string SELL_PRICE = "SELL_PRICE";
        static string SIZE = "SIZE";
        static string ITEM_ORIG_PRICE = "ITEM_ORIG_PRICE";
        static string ITEM_PRICE = "ITEM_PRICE";
        static string ITEM_TAX = "ITEM_TAX";
        static string ITEM_DISCOUNT = "ITEM_DISCOUNT";
        static string DISCOUNT_POHODA = "DISCOUNT_POHODA";
        static string ITEM_TOTAL = "ITEM_TOTAL";
        static string ITEM_STATUS = "ITEM_STATUS";
        static string ORD_COUNT = "ORD_COUNT";
        static string STORAGE = "STORAGE";

        public WaitingProductEntity()
        {
            Clear();
        }

        public override void Clear()
        {
            base.Clear();

            InvoiceNr = string.Empty;
            Sku = string.Empty;
            InvSku = string.Empty;
            Description = string.Empty;
            BuyingPrice = double.NaN;
            Date = string.Empty;
            DescriptionWeb = string.Empty;
            SellPrice = double.NaN;
            Size = string.Empty;
            ItemOrigPrice = string.Empty;
            ItemPrice = string.Empty;
            ItemTax = string.Empty;
            ItemDiscount = string.Empty;
            DiscountPohoda = string.Empty;
            ItemTotal = string.Empty;
            ItemStatus = string.Empty;
            OrdCount = string.Empty;
            Storage = string.Empty;
        }

        public static List<WaitingProductEntity> LoadAll()
        {
            return BaseEntity<WaitingProductEntity>.LoadAll(DBProvider.T_WAITING_PRODUCTS);
        }

        /// <summary>
        /// Nacitanie produktov podla user defined filtra
        /// </summary>
        /// <param name="where">podmienka alebo null</param>
        /// <param name="order">order by alebo null (id desc)</param>
        /// <returns></returns>
        public static List<WaitingProductEntity> Load(string where, string order)
        {
            return BaseEntity<WaitingProductEntity>.Load(DBProvider.T_WAITING_PRODUCTS, where, order);
        }

        public void Save()
        {
            Save(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}", 
                ID, COMMENT, VALID, INVOICE_NR, SKU, INV_SKU, DESCRIPTION, BUYING_PRICE, ORDER_DATE, DESCRIPTION_WEB, SELL_PRICE, SIZE, ITEM_ORIG_PRICE, ITEM_PRICE, ITEM_TAX, ITEM_DISCOUNT, DISCOUNT_POHODA, ITEM_TOTAL, ITEM_STATUS, ORD_COUNT, STORAGE),

                string.Format("{0},\"{1}\",{2},\"{3}\",\"{4}\",\"{5}\",\"{6}\",{7},\"{8}\",\"{9}\",{10},\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\"",
                Common.NullableLong(Id), Comment, Valid ? 1 : 0, InvoiceNr, Sku, InvSku, Description, BuyingPrice.ToDBString(), Date, DescriptionWeb, SellPrice.ToDBString(), Size, ItemOrigPrice, ItemPrice, ItemTax, ItemDiscount, DiscountPohoda, ItemTotal, ItemStatus, OrdCount, Storage
                ));
        }

        internal override void ParseFromRow(System.Data.DataRow row)
        {
            base.ParseFromRow(row);

            InvoiceNr = row[INVOICE_NR].ToString();
            Sku = row[SKU].ToString();
            InvSku = row[INV_SKU].ToString();
            Description = row[DESCRIPTION].ToString();
            BuyingPrice = Common.GetPrice(row[BUYING_PRICE].ToString());
            Date = row[ORDER_DATE].ToString();
            DescriptionWeb = row[DESCRIPTION_WEB].ToString();
            SellPrice = Common.GetPrice(row[SELL_PRICE].ToString());
            Size = row[SIZE].ToString();
            ItemOrigPrice = row[ITEM_ORIG_PRICE].ToString();
            ItemPrice = row[ITEM_PRICE].ToString();
            ItemTax = row[ITEM_TAX].ToString();
            ItemDiscount = row[ITEM_DISCOUNT].ToString();
            DiscountPohoda = row[DISCOUNT_POHODA].ToString();
            ItemTotal = row[ITEM_TOTAL].ToString();
            ItemStatus = row[ITEM_STATUS].ToString();
            OrdCount = row[ORD_COUNT].ToString();
            Storage = row[STORAGE].ToString();
        }

        public override string ToString()
        {
            return Sku;
        }

        public override string GetTableName()
        {
            return DBProvider.T_WAITING_PRODUCTS;
        }
    }
}
