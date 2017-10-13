using Newtonsoft.Json;
using Hodlr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hodlr
{
    public static class AppUtils
    {
        public static async Task<bool> RefreshVals()
        {
            var btcResp = await Comms.Get<Dictionary<string, FiatValue>>(Comms.BlockChainApi, "ticker");
            FiatConvert convert = App.FiatConvert;

            if (btcResp.Success && btcResp.Data != null)
            {
                if (convert == null || 
                    App.lastConvertRefresh == null || 
                    (DateTime.Now - App.lastConvertRefresh) > TimeSpan.FromDays(1))
                {
                    var fiatResp = await Comms.Get<FiatConvert>(Comms.ConverterApi, "latest?base=USD");
                    if (fiatResp.Success && fiatResp.Data != null)
                    {
                        App.lastConvertRefresh = DateTime.Now;
                        convert = fiatResp.Data;
                    }
                }
                if (convert != null)
                {
                    // Only use the currencies available from both APIs
                    var commonFiat = btcResp.Data.Keys.Intersect(convert.Rates.Keys).ToList();
                    commonFiat.Add("USD");

                    App.FiatValues = btcResp.Data
                         .Where(kvp => commonFiat.Contains(kvp.Key))
                         .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    App.FiatConvert = convert;
                    App.FiatConvert.Rates = App.FiatConvert.Rates
                         .Where(kvp => commonFiat.Contains(kvp.Key))
                         .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    SaveCache();
                }
            }
            return btcResp.Success && convert != null;
        }

        public static void SaveCache()
        {
            App.db.AddCache(new AppCache
            {
                LastConvertRefresh = App.lastConvertRefresh,
                FiatPref = App.FiatPref,
                ConvertDataJson = JsonConvert.SerializeObject(App.FiatConvert),
                ValueDataJson = JsonConvert.SerializeObject(App.FiatValues)
            });
        }

        public static double GetFiatValOfBtc(string currency, double btcAmount)
        {
            FiatValue fiat = App.FiatValues[currency];
            return  btcAmount * fiat.Delayed;            
        }

        public static double GetBtcValOfFiat(string currency, double fiatAmount)
        {
            FiatValue fiat = App.FiatValues[currency];
            return fiatAmount / fiat.Delayed;
        }

        public static double ConvertFiat(string lhs, string rhs, double amount)
        {
            if ((lhs != "USD" && !App.FiatConvert.Rates.ContainsKey(lhs)) || 
                (rhs != "USD" && !App.FiatConvert.Rates.ContainsKey(rhs)))
            {
                return -1;
            }

            double usdVal = (lhs == "USD") ? amount : amount / App.FiatConvert.Rates[lhs];
            double convertedVal = (rhs == "USD") ? usdVal : usdVal * App.FiatConvert.Rates[rhs];

            return convertedVal;
        }
    }
}
