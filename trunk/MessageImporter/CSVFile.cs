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
        public FileItem File { get; set; }

        public CSVFileItem[] Items { get; set; }

        public CSVFile(FileItem file)
        {
            File = file;
        }
    }
}
