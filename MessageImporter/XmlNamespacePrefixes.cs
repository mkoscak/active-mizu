using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MessageImporter
{
    class XmlNamespacePrefixes
    {
        /// <summary>
        /// Tento objekt pouzit pri volani Serialize ak chceme prefixey - hlavne po pregenerovani tried z XSD!!!
        /// Vzor:
        /// 
        /// var ns = XmlNamespacePrefixes.GetSerializerNamespaces();
        /// Serializer.Serialize(xmlWriter, this, ns);
        /// 
        /// </summary>
        /// <returns></returns>
        public static XmlSerializerNamespaces GetSerializerNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("dat", "http://www.stormware.cz/schema/version_2/data.xsd");
            ns.Add("typ", "http://www.stormware.cz/schema/version_2/type.xsd");
            ns.Add("stk", "http://www.stormware.cz/schema/version_2/stock.xsd");
            ns.Add("ftr", "http://www.stormware.cz/schema/version_2/filter.xsd");
            ns.Add("inv", "http://www.stormware.cz/schema/version_2/invoice.xsd");
            ns.Add("prn", "http://www.stormware.cz/schema/version_2/print.xsd");
            ns.Add("tns", "urn:schemas-microsoft-com:office:spreadsheet");

            return ns;
        }
    }
}
