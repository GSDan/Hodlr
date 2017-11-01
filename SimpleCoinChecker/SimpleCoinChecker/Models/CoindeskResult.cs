using System.Collections.Generic;

namespace Hodlr.Models
{
    public class CoindeskResult
    {
        public string Disclaimer { get; set; }
        public string ChartName { get; set; }
        public Dictionary<string, CoindeskResultInfo> Bpi { get; set; }
    }

    public class CoindeskResultInfo
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Rate { get; set; }
        public string Description { get; set; }
        public double Rate_float { get; set; }
    }
}
