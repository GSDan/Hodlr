using Newtonsoft.Json;
using Hodlr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Hodlr.Interfaces;
using Xamarin.Forms;

namespace Hodlr
{
    public static class AppUtils
    {
        public static DateTime LastConvertRefresh;
        public static FiatConvert FiatConvert;
        public static string FiatPref = "USD";
        public static int SourcePrefIndex = 0;
        public static double LastShownHint = 0;

        // Commented out providers only give BTC prices
        //public const string CoindeskName = "Coindesk";
        //public const string BlockchainName = "Blockchain.info";
        public const string CryptoCompareName = "CryptoCompare";
        public const string GDAXName = "GDAX";
        public const string CoinbaseName = "Coinbase";
        public static string[] PriceSources = { CryptoCompareName, GDAXName, CoinbaseName };
        public static string[] CryptoCurrencies = { "BTC", "ETH", "LTC" };

        public static async Task<double> GetCurrentUsdtoCrypto(string priceSource, string crypto)
        {
            switch(priceSource)
            {
                //case BlockchainName:
                //    var btcResp = await Comms.Get<Dictionary<string, BlockchainResult>>(Comms.BlockChainApi, Comms.BlockChainPriceRoute);
                //    if (btcResp.Success) return btcResp.Data["USD"].Delayed;
                //    break;
                //case CoindeskName:
                //    var deskResp = await Comms.Get<CoindeskResult>(Comms.CoindeskApi, Comms.CoinDeskPriceRoute);
                //    if (deskResp.Success) return deskResp.Data.Bpi["USD"].Rate_float;
                //    break;
                case CoinbaseName:
                    var cbResp = await Comms.Get<CoinbaseResult>(Comms.CoinbaseApi, string.Format(Comms.CoinbasePriceRoute, crypto));
                    if (cbResp.Success) return cbResp.Data.Data.Amount;
                    break;
                case GDAXName:
                    var gdaxResp = await Comms.Get<GDAXResult>(Comms.GdaxApi, string.Format(Comms.GdaxPriceRoute, crypto));
                    if (gdaxResp.Success) return gdaxResp.Data.Price;
                    break;
                case CryptoCompareName:
                    var compResp = await Comms.Get<CryptoCompareResult>(Comms.CryptoCompareApi, string.Format(Comms.CryptoCompareRoute, crypto));
                    if (compResp.Success) return compResp.Data.USD;
                    break;
            }             
            return -1;
        }

        public static async Task<FiatConvert> RefreshCryptos(FiatConvert con, string source)
        {
            bool success = true;

            foreach (string crypto in CryptoCurrencies)
            {
                double returnedUsdToCrypto = await GetCurrentUsdtoCrypto(source, crypto);
                if (returnedUsdToCrypto == -1)
                {
                    success = false;
                    break;
                }

                con.UsdToCrypto[crypto] = returnedUsdToCrypto;
            }

            return (success) ? con : null;
        }

        public static async Task<bool> RefreshAllVals(string source = CryptoCompareName)
        {
            FiatConvert convert = FiatConvert;

            if (convert == null || (DateTime.Now - LastConvertRefresh) > TimeSpan.FromDays(1))
            {
                var fiatResp = await Comms.Get<FiatConvert>(Comms.ConverterApi, "latest?base=USD");
                if (fiatResp.Success && fiatResp.Data != null)
                {
                    LastConvertRefresh = DateTime.Now;
                    convert = fiatResp.Data;
                }
            }

            if (convert == null) return false;

            FiatConvert refreshed = await RefreshCryptos(convert, source);

            if(refreshed != null)
            {
                FiatConvert = refreshed;
                SaveCache();
            }

            return refreshed != null;
        }

        public static void SetupCache()
        {
            AppCache cache = App.DB.GetCache();
            if (cache != null)
            {
                if (!string.IsNullOrWhiteSpace(cache.FiatPref)) FiatPref = cache.FiatPref;
                LastConvertRefresh = cache.LastConvertRefresh;
                FiatConvert = JsonConvert.DeserializeObject<FiatConvert>(cache.ConvertDataJson);
                SourcePrefIndex = cache.SourcePref;
                LastShownHint = cache.LastShownHint;
            }
        }

        public static void SaveCache()
        {
            App.DB.AddCache(new AppCache
            {
                LastConvertRefresh = LastConvertRefresh,
                FiatPref = FiatPref,
                ConvertDataJson = JsonConvert.SerializeObject(FiatConvert),
                SourcePref = SourcePrefIndex,
                LastShownHint = LastShownHint
            });
        }

        public static List<string> GetCurrencies()
        {
            if (FiatConvert?.Rates == null) return null;

            List<string> currs = FiatConvert.Rates.Keys.ToList();
            currs.Add("USD");
            currs.Sort();
            return currs;
        }

        public static double GetFiatValOfCrypto(string currency, string crypto, double cryptoAmount, FiatConvert convert = null)
        {
            if (convert == null) convert = FiatConvert;
            double inUsd = cryptoAmount * convert.UsdToCrypto[crypto];
            return  ConvertFiat("USD", currency, inUsd, convert);            
        }

        public static double GetCryptoValOfFiat(string currency, string crypto, double fiatAmount, FiatConvert convert = null)
        {
            if (convert == null) convert = FiatConvert;
            double inUsd = ConvertFiat(currency, "USD", fiatAmount, convert);
            return inUsd / convert.UsdToCrypto[crypto];
        }

        public static double ConvertFiat(string lhs, string rhs, double amount, FiatConvert convert = null)
        {
            // Default to cached FiatConvert
            if (convert == null) convert = FiatConvert;

            if (convert == null ||
                (lhs != "USD" && !convert.Rates.ContainsKey(lhs)) || 
                (rhs != "USD" && !convert.Rates.ContainsKey(rhs)))
            {
                return -1;
            }

            double usdVal = (lhs == "USD") ? amount : amount / convert.Rates[lhs];
            double convertedVal = (rhs == "USD") ? usdVal : usdVal * convert.Rates[rhs];

            return convertedVal;
        }  

        public static string GetMoneyString(double amount, string currencyCode)
        {
            RegionInfo region = DependencyService.Get<ICurrencySymbolManager>().GetRegion(currencyCode);
            CultureInfo culture = DependencyService.Get<ICurrencySymbolManager>().GetCulture(region.Name);

            var formattedAmount = String.Format(culture, "{0:C}", amount);
            return (formattedAmount);
        }

    }
}
