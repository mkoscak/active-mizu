using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    public class ExRateItem
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public double RateCZK { get; set; }
        public double RatePLN { get; set; }
        public double RateHUF { get; set; }
    }
}
