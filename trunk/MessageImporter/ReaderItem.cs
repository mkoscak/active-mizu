﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    public class ReaderItem
    {
        public int Id { get; set; }
        public string OrderNr { get; set; }
        public string SKU { get; set; }
        public string StoreNr { get; set; }
        public int Valid { get; set; }
    }
}
