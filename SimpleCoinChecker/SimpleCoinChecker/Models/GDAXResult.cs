using System;

namespace Hodlr.Models
{
    public class GDAXResult
    {
        public int Trade_id { get; set; }
        public double Price { get; set; }
        public double Size { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Volume { get; set; }
        public DateTime Time { get; set; }
    }
}
