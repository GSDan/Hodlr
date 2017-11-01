namespace Hodlr.Models
{
    public class CoinbaseResult
    {
        public CoinbaseData Data { get; set; }
    }

    public class CoinbaseData
    {
        public string Base { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}
