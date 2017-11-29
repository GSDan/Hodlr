using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hodlr.Models
{
    public class HodlStatus
    {
        public Dictionary<string, double> TotalCryptos { get; set; }
        public double FloatingFiat { get; set; }
        public double TotalFiatInvestment { get; set; }
        public double CryptoFiatVal { get; set; }
        public double Profit
        {
            get { return FloatingFiat + CryptoFiatVal - TotalFiatInvestment; }
        }
        public double PercentChange
        {
            get { return (FloatingFiat != 0) ? Profit / TotalFiatInvestment * 100 : 0; }
        }

        public HodlStatus()
        {
            TotalCryptos = new Dictionary<string, double>();
            foreach (string crypto in AppUtils.CryptoCurrencies)
            {
                TotalCryptos[crypto] = 0;
            }
        }

        public static HodlStatus GetCurrent(List<Transaction> transactions, FiatConvert convert = null)
        {
            HodlStatus status = new HodlStatus();

            foreach (var tr in transactions)
            {
                double thisFiat = AppUtils.ConvertFiat(tr.FiatCurrency, AppUtils.FiatPref, tr.FiatValue, convert);

                if (tr.AcquireCrypto)
                {
                    status.TotalCryptos[tr.CryptoCurrency] += tr.CryptoAmount;
                    status.TotalFiatInvestment += thisFiat;
                }
                else
                {
                    status.TotalCryptos[tr.CryptoCurrency] -= tr.CryptoAmount;
                    status.FloatingFiat += thisFiat;
                }
            }

            foreach(string crypto in AppUtils.CryptoCurrencies)
            {
                status.CryptoFiatVal += AppUtils.GetFiatValOfCrypto(
                    AppUtils.FiatPref,
                    crypto, 
                    status.TotalCryptos[crypto],
                    convert);
            }

            return status;
        }
    }
}
