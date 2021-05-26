using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Price_Arbitrage_Calculator.Controllers.ControllerHelpers
{
    public class CalculatorHelper
    {
        /// <summary>
        /// Takes in values and places them in arbitrage formula
        /// </summary>
        /// <returns> returns arbitrage value</returns>
        public static double ArbitrageCaculator(double zarBid, double usdAsk, double usdZarExchange)
        {
            return Math.Round((zarBid / (usdAsk * usdZarExchange)), 2);
        }
    }
}
