using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    /// <summary>
    /// Reprezentuje CSV subor s objednavkou
    /// </summary>
    public class CSVFile
    {
        public string FileName { get; set; }

        public CSVFileItem[] Items { get; set; }

        public CSVFile(string fileName)
        {
            FileName = fileName;
        }
    }
}
