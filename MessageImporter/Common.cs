using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Windows.Forms;
using System.IO;

namespace MessageImporter
{
    /// <summary>
    /// Staticke spolocne metody
    /// </summary>
    static class Common
    {
        static int IdCounter = 0;
        static int IdCounter2 = 0;

        public static int NextId
        {
            get
            {
                return ++IdCounter;
            }
        }

        public static void ResetCounter()
        {
            IdCounter = 0;
        }

        public static int NextId2
        {
            get
            {
                return ++IdCounter2;
            }
        }

        public static void ResetCounter2()
        {
            IdCounter2 = 0;
        }

        /// <summary>
        /// Extension metoda na konverziu double cisla na string pre DB operacie
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToDBString(this double num)
        {
            if (double.IsNaN(num))
                return "null";

            return num.ToString().Replace(',', '.');
        }

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

            var tmp = CleanPrice(strPrice);

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

        public static string CleanPrice(string strPrice)
        {
            if (string.IsNullOrEmpty(strPrice))
                return string.Empty;

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

            strPrice = new string(strPrice.ToCharArray().Where(c => Char.IsDigit(c) || c == ',' || c == '.' || c == '-').ToArray()).Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            return strPrice;
        }

        public static string Proper(string s)
        {
            if (s == null)
                s = "";
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        public static string ToNumeric(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

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
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var i = value.IndexOf(':');
            if (i == -1)
                return value;

            return value.Substring(0, i - 2).Trim();
        }

        
        public static string ExtractFileName(string pathAndName)
        {
            if (!pathAndName.Contains('\\'))
                return pathAndName;

            return pathAndName.Substring(pathAndName.LastIndexOf('\\') + 1);
        }

        public static string ModifyOrderNumber(string orderNumber)
        {
            return orderNumber.Replace("-", "");
        }

        /// <summary>
        /// Odstrani vsetko za pomlckou, ak existuje..
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        public static string ModifyOrderNumber2(string orderNumber)
        {
            var pos = orderNumber.IndexOf('-');
            if (pos == -1)
                return orderNumber;

            return orderNumber.Remove(pos);
        }

        public static string ModifyPhoneNr(string phone, Country from)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            var ret = new string(phone.ToCharArray().Where(c => c != ' ' && c != '/').ToArray());

            // pre cesko
            if (from == Country.CzechRepublic)
            {
                if (ret.StartsWith("420"))
                    ret = "+" + ret;
                else if (!ret.StartsWith("+420"))
                    ret = "+420" + ret;
            }

            return ret;
        }

        internal static string TruncPrice(string value)
        {
            var dblVal = GetPrice(value);
            
            return Math.Round(dblVal).ToString();
        }

        public static string NullableLong(long? l)
        {
            return l.HasValue ? l.Value.ToString() : "null";
        }

        internal static void ExportExcel(string name, CustomDataGridView grid)
        {
            string fName = GetFileName(name);
            if (fName == null)
                return;

            try
            {
                DoExport(fName, grid);

                MessageBox.Show(null, "Export finished!", "Excel export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, string.Format("Error during export: {0}", ex.Message), "Excel export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DoExport(string fName, CustomDataGridView grid)
        {
            ExcelPackage ep = new ExcelPackage();
            var ws = ep.Workbook.Worksheets.Add("Exported data");

            // hlavicka
            var col = 1;
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                // len zobrazene stlpce
                if (!grid.Columns[i].Visible)
                    continue;

                ws.Cells[1, col].Value = grid.Columns[i].HeaderText;
                ws.Cells[1, col].Style.Font.Bold = true;
                ws.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, col].Style.Fill.BackgroundColor.SetColor(Color.GreenYellow);
                ws.Cells[1, col].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.DarkGray);
                ws.Column(col).Width = 17;

                var row = 2;
                for (int j = 0; j < grid.Rows.Count; j++)
                {
                    var obj = grid[i, j].Value;
                    // farbicky pre parne riadky
                    if (obj != null)
                    {
                        /*var dp = GetPrice(obj.ToString());
                        if (!double.IsNaN(dp))
                            ws.Cells[row, col].Value = dp;
                        else*/
                            ws.Cells[row, col].Value = obj.ToString();
                    }

                    if (row % 2 == 1)
                    {
                        ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LemonChiffon);
                    }
                    ws.Cells[row, col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[row, col].Style.Border.Left.Color.SetColor(Color.DarkGray);
                    ws.Cells[row, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[row, col].Style.Border.Right.Color.SetColor(Color.DarkGray);

                    row++;
                }

                col++;
            }

            // ulozenie
            ep.SaveAs(new FileInfo(fName));
        }

        private static string GetFileName(string name)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = name.Replace(' ', '_') + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
            sfd.Filter = "Excel 2007 files|*.xlsx";
            sfd.CheckFileExists = false;
            sfd.InitialDirectory = ".";
            sfd.OverwritePrompt = true;
            sfd.SupportMultiDottedExtensions = true;
            sfd.Title = "Type file name or select existing";
            if (sfd.ShowDialog() == DialogResult.Cancel)
                return null;

            return sfd.FileName;
        }
    }
}
