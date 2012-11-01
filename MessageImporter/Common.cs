using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    class Common
    {
        internal static bool IsEquipped(Invoice inv)
        {
            return inv != null && inv.InvoiceItems != null && inv.InvoiceItems.All(i => IsItemPaired(i));
        }

        internal static bool IsItemPaired(InvoiceItem i)
        {
            return i != null && i.PairProduct != null;
        }
    }
}
