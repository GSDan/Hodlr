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
        public static async Task<double> GetCurrentUsdtoBtc(string priceSource)
        {
            switch(priceSource)
            {
                case App.BlockchainName:
                    var btcResp = await Comms.Get<Dictionary<string, BlockchainResult>>(Comms.BlockChainApi, Comms.BlockChainPriceRoute);
                    if (btcResp.Success) return btcResp.Data["USD"].Delayed;
                    break;
                case App.CoindeskName:
                    var deskResp = await Comms.Get<CoindeskResult>(Comms.CoindeskApi, Comms.CoinDeskPriceRoute);
                    if (deskResp.Success) return deskResp.Data.Bpi["USD"].Rate_float;
                    break;
                case App.CoinbaseName:
                    var cbResp = await Comms.Get<CoinbaseResult>(Comms.CoinbaseApi, Comms.CoinbasePriceRoute);
                    if (cbResp.Success) return cbResp.Data.Data.Amount;
                    break;
            }             
            return -1;
        }

        public static async Task<bool> RefreshVals(string source = App.BlockchainName)
        {
            double returnedUsdtoBtc = await GetCurrentUsdtoBtc(source);
            FiatConvert convert = App.FiatConvert;

            if (returnedUsdtoBtc != -1)
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
                    App.UsdToBtc = returnedUsdtoBtc;
                    App.FiatConvert = convert;

                    SaveCache();
                }
            }
            return (returnedUsdtoBtc != -1) && (convert != null);
        }

        public static void SaveCache()
        {
            App.db.AddCache(new AppCache
            {
                LastConvertRefresh = App.lastConvertRefresh,
                FiatPref = App.FiatPref,
                ConvertDataJson = JsonConvert.SerializeObject(App.FiatConvert),
                UsdToBtc = App.UsdToBtc
            });
        }

        public static List<string> GetCurrencies()
        {
            if (App.FiatConvert?.Rates == null) return null;

            List<string> currs = App.FiatConvert.Rates.Keys.ToList();
            currs.Add("USD");
            currs.Sort();
            return currs;
        }

        public static double GetFiatValOfBtc(string currency, double btcAmount)
        {
            double inUsd = btcAmount * App.UsdToBtc;
            return  ConvertFiat("USD", currency, inUsd);            
        }

        public static double GetBtcValOfFiat(string currency, double fiatAmount)
        {
            double inUsd = ConvertFiat(currency, "USD", fiatAmount);
            return inUsd / App.UsdToBtc;
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
