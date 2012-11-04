using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    /// <summary>
    /// Podporovane krajiny
    /// </summary>
    public enum Country
    {
        Unknown,
        Slovakia,
        Hungary,
        Poland,
        CzechRepublic
    }

    public class CountrySetting
    {
        public CountrySetting(Country country)
        {
            var prop = Properties.Settings.Default;

            Tax = 1.0;
            ShipText = null;
            ShipPrice = 0.0;
            ExchangeRate = 1.0;

            switch (country)
            {
                case Country.Slovakia:
                    Tax = Common.GetPrice(prop.TaxSkk);
                    ShipText = prop.ShipTextSkk;
                    ShipPrice = Common.GetPrice(prop.ShipPriceSkk);
                    break;

                case Country.Hungary:
                    Tax = Common.GetPrice(prop.TaxHuf);
                    ShipText = prop.ShipTextHuf;
                    ShipPrice = Common.GetPrice(prop.ShipPriceHuf);
                    ExchangeRate = Common.GetPrice(prop.ExRateHuf);
                    break;

                case Country.Poland:
                    Tax = Common.GetPrice(prop.TaxPln);
                    ShipText = prop.ShipTextPln;
                    ShipPrice = Common.GetPrice(prop.ShipPricePln);
                    ExchangeRate = Common.GetPrice(prop.ExRatePln);
                    break;

                case Country.CzechRepublic:
                    Tax = Common.GetPrice(prop.TaxCzk);
                    ShipText = prop.ShipTextCzk;
                    ShipPrice = Common.GetPrice(prop.ShipPriceCzk);
                    ExchangeRate = Common.GetPrice(prop.ExRateCzk);
                    break;

                case Country.Unknown:
                    ShipText = "<Unrecognized country>";
                    break;

                default:
                    break;
            }
        }

        public double Tax { get; set; }
        public string ShipText { get; set; }
        public double ShipPrice { get; set; }
        public double ExchangeRate { get; set; }
    }
}
