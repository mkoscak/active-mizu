using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    /// <summary>
    /// Objednavka
    /// </summary>
    public class StockEntity
    {
        public StockItem[] Items { get; set; }

        public string OrderReference { get; set; }

        public string OurReference { get; set; }
    }
}
