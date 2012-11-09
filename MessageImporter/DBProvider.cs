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
        private static string DataSource = @".\activestyle.db";

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
    }
}
