namespace MarketDataServer
{
    public class Tick
    {
        public long Price { get; set; }
        public long LastPrice { get; set; }
        public string Symbol { get; set; }
        public long Volume { get; set; }
    }
}