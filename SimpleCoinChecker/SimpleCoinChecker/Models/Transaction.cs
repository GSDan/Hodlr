using SQLite;
using System;

namespace Hodlr.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double BtcAmount { get; set; }
        public string FiatCurrency { get; set; }
        public double FiatValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool AcquireBtc { get; set; }
    }
}
