using SQLite;
using System;

namespace Hodlr.Models
{
    public class Transaction
    {
        public static int CurrentDataVersion = 2;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CryptoCurrency { get; set; }
        public string FiatCurrency { get; set; }
        public double FiatValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public double CryptoAmount { get; set; }
        public bool AcquireCrypto { get; set; }

        public int DataVersion { get; set; }

        // Keep old values around for one version so users don't lose their data
        [Obsolete("BtcAmount is deprecated, please use CryptoAmount instead.")]
        public double BtcAmount { get; set; }
        [Obsolete("AcquireBtc is deprecated, please use AcquireCrypto instead.")]
        public bool AcquireBtc { get; set; }
    }
}
