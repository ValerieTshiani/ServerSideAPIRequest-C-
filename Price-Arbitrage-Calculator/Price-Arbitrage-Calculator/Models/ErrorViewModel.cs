using System;
using System.Collections.Generic;

namespace Price_Arbitrage_Calculator.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public class JsonResponseObject
        {
            public string Error { get; set; }
            public Dictionary<string, double> Data {get; set;}

        }
    }
}
