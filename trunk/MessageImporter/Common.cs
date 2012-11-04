using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MessageImporter
{
    /// <summary>
    /// Podporovane krajiny
    /// </summary>
    public enum Country
    {
        Unknown,
        Slovakia,
        Hungary,
        Poland,
        CzechRepublic
    }

    class Common
    {
        public static bool IsEquipped(Invoice inv)
        {
            return inv != null && inv.InvoiceItems != null && inv.InvoiceItems.All(i => IsItemPaired(i));
        }

        public static bool IsItemPaired(InvoiceItem i)
        {
            return i != null && i.PairProduct != null;
        }

        public static double GetPrice(string strPrice)
        {
            if (strPrice == null)
                return double.NaN;

            var tmp = CleanPrice(ref strPrice);

            double ret = double.NaN;
            try
            {
                ret = double.Parse(tmp);
            }
            catch (Exception)
            {
            }

            return ret;
        }

        public static string CleanPrice(ref string strPrice)
        {
            // cena obsahuje aj bodku aj ciarku, napr 1,000.25.. prvy znak vyhodime
            if (strPrice.Contains(',') && strPrice.Contains('.'))
            {
                int pointIndex = strPrice.LastIndexOf('.');
                int commaIndex = strPrice.LastIndexOf(',');

                if (pointIndex > commaIndex)
                    strPrice = strPrice.Replace(",", "");   // odstranime vsetky ciarky
                else
                    strPrice = strPrice.Replace(".", "");   // odstranime vsetky bodky
            }

            strPrice = new string(strPrice.ToCharArray().Where(c => Char.IsDigit(c) || c == ',' || c == '.').ToArray()).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            return strPrice;
        }

        public static string Proper(string s)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        public static string ToNumeric(string str)
        {
            return new string(str.ToCharArray().Where(c => Char.IsDigit(c)).ToArray());
        }

        public static string SlovakPhone(string value)
        {
            var ret = new string(value.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()).Trim();  // odstranenie medzier a vsetkych bielych znakov

            if (ret.StartsWith("09"))
                ret = "+421" + ret.Substring(1);

            if (ret.StartsWith("00421"))
                ret = "+" + ret.Substring(2);

            return ret;
        }

        public static string GetDate(string value)
        {
            var i = value.IndexOf(':');
            if (i == -1)
                return value;

            return value.Substring(0, i - 2).Trim();
        }
    }
}
