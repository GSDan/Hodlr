using SQLite;
using System;

namespace Hodlr.Models
{
    public class AppCache
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ConvertDataJson { get; set; }
        public DateTime LastConvertRefresh { get; set; }
        public string FiatPref { get; set; }
        public string CryptoPref { get; set; }
        public int SourcePref { get; set; }
        public int LastShownHint { get; set; }
    }
}
