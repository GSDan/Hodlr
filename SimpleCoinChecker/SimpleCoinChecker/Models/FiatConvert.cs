using System;
using System.Collections.Generic;

namespace Hodlr.Models
{
    public class FiatConvert
    {
        public string BaseFiat { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, double> Rates { get; set; }
        public double UsdToBtc { get; set; }
    }
}
