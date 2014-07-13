using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Globalization;

namespace MessageImporter
{
    public enum MSG_TYPE
    {
        UNKNOWN,
        SPORTS_DIRECT,
        MANDM_DIRECT,
        MANDM_DIRECT_OLD,
        CSV,
        CSVX,
        GETTHELABEL,
        FIVE_POUNDS
    }

    /// <summary>
    /// Pomocna struktura do datagridu na vyber suborov na spracovanie
    /// </summary>
    [Serializable]
    public class FileItem
    {
        internal const string SportsDirect = "sportsdirect";    // na identifikaciu faktur zo sportsdirect
        internal const string MandMDirectOld = "mandmdirect";    // na identifikaciu faktur z mandmdirect
        internal const string MandMDirect = "mandm direct";    // na identifikaciu faktur z mandmdirect
        internal const string Refund = "refund";    // na identifikaciu faktur z mandmdirect
        internal const string FivePounds = "5pounds";    // na identifikaciu faktur z 5Pounds

        public Image i
        {
            get
            {
                return Process ? Icons.Complete : Icons.NonComplete;
            }
        }

        public bool Process { get; set; }

        internal string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
                
                if (string.IsNullOrEmpty(fileName))
                    return;

                if (fileName.ToLower().EndsWith(".msg"))
                {
                    if (fileName.ToLower().Contains(SportsDirect.ToLower()))
                        Type = MSG_TYPE.SPORTS_DIRECT;
                    else if (fileName.ToLower().Contains(MandMDirect.ToLower()) || fileName.ToLower().Contains(MandMDirectOld.ToLower()))
                        Type = MSG_TYPE.MANDM_DIRECT;
                    else if (fileName.ToLower().Contains(FivePounds.ToLower()))
                        Type = MSG_TYPE.FIVE_POUNDS;
                    else
                        Type = MSG_TYPE.GETTHELABEL;
                }
                else if (fileName.ToLower().EndsWith(".csv"))
                    Type = MSG_TYPE.CSV;
                else if (fileName.ToLower().EndsWith(".csvx"))
                    Type = MSG_TYPE.CSVX;
                else
                    Type = MSG_TYPE.UNKNOWN;

                if (Type == MSG_TYPE.UNKNOWN)
                    Process = false;

                // nastavenie zakladnej dane pre dany subor
                Tax = 1.0 + Properties.Settings.Default.DPH_percent / 100;
                if (Type == MSG_TYPE.SPORTS_DIRECT && fileName.ToLower().Contains(Refund))
                    Tax = 1.0;
            }
        }

        public double Tax { get; set; }

        public double ExchRate { get; set; }

        public string Currency { get; set; }

        internal MSG_TYPE type;
        public MSG_TYPE Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
                ExchRate = 1.0;

                if (type == MSG_TYPE.MANDM_DIRECT)
                    ExchRate = Common.GetPrice("1.28");
                else if (type == MSG_TYPE.SPORTS_DIRECT)
                    ExchRate = Common.GetPrice("1.28");
            }
        }
        
        public DateTime OrderDate { get; set; }

        public double Delivery { get; set; }

        [System.ComponentModel.DisplayName("Číslo faktúry")]
        public string OrderNumber { get; set; }

        public bool PopisWEB { get; set; }

        internal int ProdCount { get; set; }

        internal string FullFileName { get; set; }

       // internal string OrderNumber { get; set; }
        
        

        public FileItem()
        {
            //ExchRate = double.Parse("1.28".Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            OrderDate = DateTime.Now;
        }

        public FileItem(bool process, string fileName)
            : this()
        {
            Process = process;
            FileName = Common.ExtractFileName(fileName);
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
            if (OrderNumber == null)
                return "CHYBA_FAKTURA!!!";
            return OrderNumber;
        }
    }
}
