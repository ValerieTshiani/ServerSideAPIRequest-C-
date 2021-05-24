using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Price_Arbitrage_Calculator.Models
{
    public class Responses
    {
        public class AskResponse
        {
            public string high { get; set; }
            public string last { get; set; }
            public string timestamp { get; set; }
            public string bid { get; set; }
            public string vwap { get; set; }
            public string volume { get; set; }
            public string low { get; set; }
            public string ask { get; set; }
            public string open { get; set; }
        }
 
        public class BidResponse
        {
            public string currencyPair { get; set; }
            public string askPrice { get; set; }
            public string bidPrice { get; set; }
            public string lastTradedPrice { get; set; }
            public string previousClosePrice { get; set; }
            public string baseVolume { get; set; }
            public string highPrice { get; set; }
            public string lowPrice { get; set; }
            public DateTime created { get; set; }
            public string changeFromPrevious { get; set; }
        }

   
        public class ExchangeResponse
        {
            public string result { get; set; }
            public string documentation { get; set; }
            public string terms_of_use { get; set; }
            public int time_last_update_unix { get; set; }
            public string time_last_update_utc { get; set; }
            public int time_next_update_unix { get; set; }
            public string time_next_update_utc { get; set; }
            public string base_code { get; set; }
            public string target_code { get; set; }
            public double conversion_rate { get; set; }
        }
    }
}
