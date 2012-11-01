using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Globalization;

namespace MessageImporter
{
    /// <summary>
    /// Pomocna struktura do datagridu na vyber suborov na spracovanie
    /// </summary>
    public class FileItem
    {
        public Image i
        {
            get
            {
                return Process ? Icons.Complete : Icons.NonComplete;
            }
        }

        public bool Process { get; set; }

        public string FileName { get; set; }

        public double ExchRate { get; set; }

        public DateTime OrderDate { get; set; }

        public double Delivery { get; set; }

        internal string FullFileName { get; set; }

        public FileItem()
        {
            ExchRate = double.Parse("1.28".Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            OrderDate = DateTime.Now;
        }

        public FileItem(bool process, string fileName)
            : this()
        {
            Process = process;
            FileName = Functions.ExtractFileName(fileName);
            FullFileName = fileName;
        }

        public FileItem(bool process, string fileName, string fullFileName)
            : this()
        {
            Process = process;
            FileName = fileName;
            FullFileName = fullFileName;
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
