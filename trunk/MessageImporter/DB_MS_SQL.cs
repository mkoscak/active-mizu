using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;//pre debug

namespace MessageImporter
{
    public class ms_SQL
    {
        SqlConnection connectionMSSQL = new SqlConnection("Data Source=192.168.1.77;Initial Catalog=ebot_prod_ssd;Persist Security Info=True;User ID=ordinum2;Password=1234Abcd");

        Random random = new Random(Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds));//seed

        public ms_SQL()
        {

        }
        public void connect()
        {
            if (connectionMSSQL != null && connectionMSSQL.State == ConnectionState.Closed)
            {
                connectionMSSQL.Open();
            }

        }
        private List<string> getCategories(string sku)
        {
            connect();
            List<string> category = new List<string>();

            /*using (SqlCommand command = new SqlCommand("SELECT idcat FROM [dbo].[CatPro] WHERE idpro like '"+sku+"';", connectionMSSQL))
            using (SqlDataReader reader = command.ExecuteReader())
            {
            while (reader.Read())
            {
                category.Add(reader.GetString(0));
                //Console.WriteLine("{0} {1} {2}",reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
            }
            }*/
            SqlCommand command = connectionMSSQL.CreateCommand();
            command.CommandText = "SELECT idcat FROM botCatPro WHERE idpro like '%" + sku + "%';";
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string cat = reader.GetString(0).Trim();
                if (!category.Contains(cat) && char.IsDigit(cat.ElementAt(0)))//pridame len ak ezte nie je a otestujeme aj ci je cislo lebo v db su aj http linky
                    category.Add(cat);
                //Console.WriteLine("{0} {1} {2}",reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
            }
            reader.Close();
            if (category.Count == 0)
                category.Add("NOT_FOUND");
            return category;

        }
        public string getNomenclature(string sku)
        {
            string nomenclature = "";
            List<string> cat = getCategories(sku);//ziskanie kategorii 

            // var nomenclatures = new Dictionary<string, int>();
            var nomenclatures = new Dictionary<int, string>();
            int position = 0;

            connect();//preventivne spojenie z databazou
            SqlCommand command = connectionMSSQL.CreateCommand();

            command.CommandText = "SELECT nomenclature,occurrence FROM intrastat WHERE idcat like '" + string.Join("' OR idcat like '", cat.ToArray()) + "';";
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {

                if (reader.GetInt32(1) == 100)//ak je 100% vyskytu vratime
                {
                    nomenclature = reader.GetString(0);
                    reader.Close();
                    return nomenclature;
                }
                else//naplnime nomenklatury hornou a dolnou hodnotu vyskytu
                {
                    /*position++;//zvysime aby, do sa nerovnalo od
                    nomenclatures.Add(reader.GetString(0) + "-", position);//od
                    position += reader.GetInt32(1);//posunieme sa o percento vyskytu
                    nomenclatures.Add(reader.GetString(0) + "+", position);//do*/
                    position++;//zvysime aby, do sa nerovnalo od
                    nomenclatures.Add(position, reader.GetString(0));//od
                    position += reader.GetInt32(1);//posunieme sa o percento vyskytu
                    nomenclatures.Add(position, reader.GetString(0));//do
                }
            }
            reader.Close();
            //spracovanie nomenclatures, ak sme tu 100% nebolo

            int randomNumber = random.Next(1, 101);
            // nomenclatures.Intersect
            // Debug.WriteLine(randomNumber);
            List<int> occurrence = new List<int>(nomenclatures.Keys);
            for (int i = 0; nomenclature == "" && i < occurrence.Count(); i = i + 2)
            {
                //   Debug.WriteLine(occurrence.ElementAt(i));
                if (occurrence.ElementAt(i) <= randomNumber && occurrence.ElementAt(i + 1) >= randomNumber)
                    nomenclature = nomenclatures[occurrence.ElementAt(i)];
            }
            if (nomenclature == "")
                nomenclature = "NOT_FOUND:" + string.Join(",", cat.ToArray());//.ElementAt(0)
            return nomenclature;
        }
    }
}
