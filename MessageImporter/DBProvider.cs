using System;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MessageImporter
{
    public class DBProvider
    {
        //private static SQLiteDataAdapter DB;
        internal static string DataSource = @".\activestyle.db";

        internal static string T_WAIT_INVOICES = "WAITING_INV_ITEMS";
        internal static string T_WAIT_STOCK = "WAITING_PRODS";
        internal static string T_READER = "READER";
        internal static string T_EXCH_RATE = "EXCH_RATE";
        // nova tabulka cakajucich produktov
        internal static string T_WAITING_PRODUCTS = "WAITING_PRODUCTS";

        static DBProvider()
        {
            // vytvorenie DB suboru
            if (!File.Exists(DataSource))
            {
                SQLiteConnection.CreateFile(DataSource);
            }
        }

        private static DBProvider db;
        public static DBProvider Instance
        {
            get
            {
                if (db == null)
                    db = new DBProvider();

                return db;
            }
        }


        public static SQLiteConnection GetConnection()
        {
            try
            {
                var sql_con = new SQLiteConnection(@"Data Source=" + DataSource + "; Version=3;");

                return sql_con;
            }
            catch (Exception)
            {
                MessageBox.Show(null, "Error while creating DB connection!", "GetConnection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public static void ExecuteNonQuery(string txtQuery)
        {
            using (var sql_con = GetConnection())
            {
                sql_con.Open();
                var sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                sql_cmd.ExecuteNonQuery();
                sql_con.Close();
            }
        }

        public static DataSet ExecuteQuery(string query)
        {
            DataSet DS = new DataSet();

            using (var sql_con = GetConnection())
            {
                sql_con.Open();
                var sql_cmd = sql_con.CreateCommand();
                var DB = new SQLiteDataAdapter(query, sql_con);
                DS.Reset();
                DB.Fill(DS);
                sql_con.Close();
            }
            return DS;
        }

        public static DataSet ExecuteQuery(string tableName, string where, string order)
        {
            DataSet DS = new DataSet();

            using (var sql_con = GetConnection())
            {
                sql_con.Open();
                var sql_cmd = sql_con.CreateCommand();
                string CommandText = "select * from " + tableName + " A " + where + " " + order;
                var DB = new SQLiteDataAdapter(CommandText, sql_con);
                DS.Reset();
                DB.Fill(DS);
                sql_con.Close();
            }
            return DS;
        }

        public static bool ExistsReaderItem(ReaderItem item)
        {
            string query = string.Format("select * from {0} where ORDER_NUMBER = {1} AND SKU = \"{2}\" AND VALID = 1", T_READER, item.OrderNr, item.SKU);
            try
            {
                var res = ExecuteQuery(query);

                if (res != null && res.Tables != null && res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
                    return true;    // zaznam existuje
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static bool InsertReaderItem(ReaderItem item)
        {
            if (ExistsReaderItem(item))
                return UpdateReaderItem(item);

            string query = string.Format("insert into {0} values ( {1}, \"{2}\", \"{3}\", \"{4}\", {5} )", T_READER, "null", item.OrderNr, item.SKU, item.StoreNr, item.Valid);

            try
            {
                ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool UpdateReaderItem(ReaderItem item)
        {
            string query = string.Format("update {0} set STORE_NR = \"{1}\" where ORDER_NUMBER = \"{2}\" AND SKU = \"{3}\" AND VALID = 1", T_READER, item.StoreNr, item.OrderNr, item.SKU);

            try
            {
                ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool ExistsExRate(ExRateItem item)
        {
            string query = string.Format("select * from {0} where DATE = \"{1}\"", T_EXCH_RATE, item.Date);
            try
            {
                var res = ExecuteQuery(query);

                if (res != null && res.Tables != null && res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
                    return true;    // zaznam existuje
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private static string GetReal(double number)
        {
            var str = number.ToString();

            return str.Replace(',', '.');
        }

        public static bool UpdateExRate(ExRateItem item)
        {

            string query = string.Format("update {0} set RATE_CZK = {2}, RATE_PLN = {3}, RATE_HUF = {4} where DATE = \"{1}\"", T_EXCH_RATE, item.Date, GetReal(item.RateCZK), GetReal(item.RatePLN), GetReal(item.RateHUF));

            try
            {
                ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool InsertExRate(ExRateItem item)
        {
            if (ExistsExRate(item))
                return UpdateExRate(item);

            string query = string.Format("insert into {0} values ( {1}, \"{2}\", {3}, {4}, {5} )", T_EXCH_RATE, "null", item.Date, GetReal(item.RateCZK), GetReal(item.RatePLN), GetReal(item.RateHUF));

            try
            {
                ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static ExRateItem GetExRate(string date)
        {
            string query = string.Format("select ID,DATE,RATE_CZK,RATE_PLN,RATE_HUF from {0} where DATE = \"{1}\"", T_EXCH_RATE, date);
            try
            {
                var res = ExecuteQuery(query);

                if (res != null && res.Tables != null && res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
                {
                    ExRateItem ret = new ExRateItem();
                    ret.Id = int.Parse(res.Tables[0].Rows[0].ItemArray[0].ToString());
                    ret.Date = res.Tables[0].Rows[0][1] as string;
                    ret.RateCZK = (double)res.Tables[0].Rows[0][2];
                    ret.RatePLN = (double)res.Tables[0].Rows[0][3];
                    ret.RateHUF = (double)res.Tables[0].Rows[0][4];

                    return ret;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static ExRateItem GetExRateDayBefore(DateTime date)
        {
            DateTime dt = date.AddDays(-1);
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                dt = dt.AddDays(-1);
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                dt = dt.AddDays(-1);

            // dt je predchadzajuci pracovny den
            return GetExRate(dt.ToString("yyyy-MM-dd"));
        }

        internal static void InsertWaitingInvoice(InvoiceItem inv)
        {
            string strMaxCode = string.Empty;

            var maxCodeSel = "select * from WAITING_PRODS where Id = (select max(id) from WAITING_PRODS)";
            var maxCode = ExecuteQuery(maxCodeSel);
            if (maxCode != null && maxCode.Tables != null && maxCode.Tables.Count > 0)
                strMaxCode = maxCode.Tables[0].Rows[0]["ID"].ToString();
            if (string.IsNullOrEmpty(strMaxCode))
                return;

            var insert = string.Format("insert into WAITING_INV_ITEMS values ({0},{1},{2},\"{3}\",\"{4}\",{5},\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",{13},{14})",
                "null", int.Parse(strMaxCode), DBPrice(inv.BuyingPrice), inv.Datetime, inv.ItemName, DBPrice(inv.PredajnaCena), inv.ItemOptions, inv.ItemOrigPrice, inv.ItemPrice, inv.ItemTax, inv.ItemDiscount, inv.ItemTotal, inv.ItemStatus, inv.ItemQtyOrdered, 1);

            ExecuteNonQuery(insert);
        }

        static string DBPrice(double price)
        {
            return price.ToString().Replace(',','.');
        }

        internal static List<InvoiceItem> ReadWaitingInvoices(string orderNumber, ref List<StockItem> stocksToUpdate)
        {
            var ret = new List<InvoiceItem>();
    
          /*  var x="dd";
    if (orderNumber == "433266")
        x = "aa";*/

            var query = string.Format("select * from WAITING_INV_ITEMS inv join WAITING_PRODS stock on stock.ID = inv.WAITING_PRODS_ID where stock.ORDER_NUMBER = \"{0}\" and inv.valid = 1 and stock.valid = 1", orderNumber);
            var data = ExecuteQuery(query);
          
            if (data != null && data.Tables != null && data.Tables.Count > 0)
            {
                for (int i = 0; i < data.Tables[0].Rows.Count; i++)
			    {
                    var inv = new InvoiceItem();
                    var stock = new StockItem();
                    stock.Sklad = Properties.Settings.Default.Storage;
                    stock.FromFile = new FileItem();
                    stocksToUpdate.Add(stock);

                    inv.PairProduct = stock;
                    stock.PairProduct = inv;

                    inv.MSG_SKU = data.Tables[0].Rows[i]["DESCRIPTION"].ToString();

                  //  inv.itemStorage = stock.Sklad;

                    inv.invSKU = data.Tables[0].Rows[i]["INV_SKU"].ToString();
                    inv.PairCode = data.Tables[0].Rows[i]["SKU"].ToString();
                    inv.OrderNumber = data.Tables[0].Rows[i]["ORDER_NUMBER"].ToString();
                    stock.FromFile.OrderNumber = inv.OrderNumber;
                    //inv.BuyingPrice = Common.GetPrice(data.Tables[0].Rows[i]["BUYING_PRICE"].ToString());
                    inv.BuyingPrice = double.NaN;
                    inv.Datetime = DateTime.Parse(data.Tables[0].Rows[i]["DATE"].ToString());
                    inv.ItemName = data.Tables[0].Rows[i]["DESC_WEB"].ToString();
                    inv.PredajnaCena = Common.GetPrice(data.Tables[0].Rows[i]["SELL_PRICE"].ToString());
                    inv.ItemOptions = data.Tables[0].Rows[i]["SIZE"].ToString();
                    inv.ItemOrigPrice = data.Tables[0].Rows[i]["ITEM_ORIG_PRICE"].ToString();
                    inv.ItemPrice = data.Tables[0].Rows[i]["ITEM_PRICE"].ToString();
                    inv.ItemTax = data.Tables[0].Rows[i]["ITEM_TAX"].ToString();
                    inv.ItemDiscount = data.Tables[0].Rows[i]["ITEM_DISCOUNT"].ToString();
                    inv.ItemTotal = data.Tables[0].Rows[i]["ITEM_TOTAL"].ToString();
                    inv.ItemStatus = data.Tables[0].Rows[i]["ITEM_STATUS"].ToString();
                    inv.ItemQtyOrdered = data.Tables[0].Rows[i]["ORD_COUNT"].ToString();
                    stock.IsFromDB = true;

                    ret.Add(inv);
			    }
            }

            return ret;
        }

        internal static void UpdateWaitingValidity(string tblName, int toValue, int[] ids)
        {
            var cmd = string.Format("update {0} set valid = {1} where ID in ({2})", tblName, toValue, string.Join(",", ids.Select(id => id.ToString()).ToArray()));
            
            ExecuteNonQuery(cmd);
        }
    }
}
