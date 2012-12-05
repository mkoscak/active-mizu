using System;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace MessageImporter
{
    public class DBProvider
    {
        private static SQLiteDataAdapter DB;
        internal static string DataSource = @".\activestyle.db";

        internal static string T_WAIT_PRODS = "WAITING_PRODS";
        internal static string T_READER = "READER";
        internal static string T_EXCH_RATE = "EXCH_RATE";

        static DBProvider()
        {
            // vytvorenie DB suboru
            if (!File.Exists(DataSource))
            {
                SQLiteConnection.CreateFile(DataSource);
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
            var sql_con = GetConnection();
            sql_con.Open();
            var sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }

        public static DataSet ExecuteQuery(string query)
        {
            DataSet DS = new DataSet();

            var sql_con = GetConnection();
            sql_con.Open();
            var sql_cmd = sql_con.CreateCommand();
            DB = new SQLiteDataAdapter(query, sql_con);
            DS.Reset();
            DB.Fill(DS);
            sql_con.Close();

            return DS;
        }

        public static DataSet ExecuteQuery(string tableName, string where, string order)
        {
            DataSet DS = new DataSet();

            var sql_con = GetConnection();
            sql_con.Open();
            var sql_cmd = sql_con.CreateCommand();
            string CommandText = "select * from " + tableName + " A " + where + " " + order;
            DB = new SQLiteDataAdapter(CommandText, sql_con);
            DS.Reset();
            DB.Fill(DS);
            sql_con.Close();

            return DS;
        }

        public static bool ExistsReaderItem(ReaderItem item)
        {
            string query = string.Format("select * from {0} where ORDER_NUMBER = {1} AND SKU = {2} AND VALID = 1", T_READER, item.OrderNr, item.SKU);
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
            string query = string.Format("update {0} set STORE_NR = {1} where ORDER_NUMBER = \"{2}\" AND SKU = \"{3}\" AND VALID = 1", T_READER, item.StoreNr, item.OrderNr, item.SKU);

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
    }
}
