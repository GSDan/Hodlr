using System;
using System.Collections.Generic;

namespace Hodlr.Models
{
    public class FiatConvert
    {
        public string BaseFiat { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, double> Rates { get; set; }
        public Dictionary<string, double> UsdToCrypto { get; set; }

        public FiatConvert()
        {
            Rates = new Dictionary<string, double>();
            UsdToCrypto = new Dictionary<string, double>();

            foreach(string crypto in AppUtils.CryptoCurrencies)
            {
                UsdToCrypto[crypto] = 0;
            }
        }
    }
}
