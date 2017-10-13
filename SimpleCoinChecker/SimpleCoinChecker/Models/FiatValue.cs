using Newtonsoft.Json;

namespace Hodlr.Models
{
    public class FiatValue
    {
        [JsonProperty("15m")]
        public double Delayed { get; set; }
        public double Last { get; set; }
        public double Buy { get; set; }
        public double Sell { get; set; }
        public string Symbol { get; set; }
    }
}
