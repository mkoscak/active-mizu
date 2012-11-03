using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace MessageImporter
{
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

            return double.Parse(strPrice.Replace('€', ' ').Trim().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
        }
    }
}
