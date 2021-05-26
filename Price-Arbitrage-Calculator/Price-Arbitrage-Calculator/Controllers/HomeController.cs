using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Price_Arbitrage_Calculator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Price_Arbitrage_Calculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Gets the Values from API Requests, then calculate price arbitrage
        /// </summary>
        /// <returns> returns json object with Arbitrage Values</returns>
        [HttpPost()]
        public async Task<JsonResult> GetValues()
        {

            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();


            var t1 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
            var m1 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");
            var t2 = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
            var m2 = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");
            var t3 = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
            await Task.WhenAll(t1, t2, t3);

           
            var usdAskBitCoin = await t1 as OkObjectResult;
            var zarBidBitCoin = await t2 as OkObjectResult;
            var usdZarExchange = await t3 as OkObjectResult;

            var usdAskXRP = await m1 as OkObjectResult;
            var zarBidXRP = await m2 as OkObjectResult;


            var usdAskBitCoinObject = (Responses.AskResponse)usdAskBitCoin.Value;
            var zarBidBitCoinObject = (Responses.BidResponse)zarBidBitCoin.Value;
            var usdZarExchangeObject = (Responses.ExchangeResponse)usdZarExchange.Value;

            var usdAskXRPObject = (Responses.AskResponse)usdAskXRP.Value;
            var zarBidXRPObject = (Responses.BidResponse)zarBidXRP.Value;


            double bitCoinArbitrageValue = ArbitrageCaculator(Convert.ToDouble(zarBidBitCoinObject.bidPrice), Convert.ToDouble(usdAskBitCoinObject.ask), usdZarExchangeObject.conversion_rate);
            double xrpArbitrageValue = ArbitrageCaculator(Convert.ToDouble(zarBidXRPObject.bidPrice), Convert.ToDouble(usdAskXRPObject.ask), usdZarExchangeObject.conversion_rate);

            Dictionary<string, double> returnObject = new Dictionary<string, double>();
            returnObject.Add("bitCoinArbitrageValue", Math.Round(bitCoinArbitrageValue, 2));
            returnObject.Add("xrpArbitrageValue", Math.Round(xrpArbitrageValue, 2));

            watch.Stop();

            System.Diagnostics.Debug.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            return Json(returnObject);
        }
        /// <summary>
        /// Takes in values and places them in arbitrage formula
        /// </summary>
        /// <returns> returns arbitrage value</returns>
        public static double ArbitrageCaculator(double zarBid, double usdAsk, double usdZarExchange)
        {
            return (zarBid / (usdAsk * usdZarExchange));
        }

        // USD ASK price
        /// <summary>
        /// Makes API Call to get ASK Price object
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD ASK Price object </returns>
        public async Task<IActionResult> GetApiAsk(string ApiUrl)
        {

            Responses.AskResponse responseObject = new Responses.AskResponse();
          
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<Responses.AskResponse>(jsonDocument);
                }

                return Ok(responseObject);

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred on  the API Call", ex);

                return BadRequest();
            }

        }

        // USD BID price
        /// <summary>
        /// Makes API Call to get BID Price object
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD BID Price object </returns>
        public async Task<IActionResult> GetApiBid(string ApiUrl)
        {
            Responses.BidResponse responseObject = new Responses.BidResponse();
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<Responses.BidResponse>(jsonDocument);
                }
                return Ok(responseObject);

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred on  the API Call", ex);

                return BadRequest();
            }

        }

        // USD ZAR Exchange Rate
        /// <summary>
        /// Makes API Call to get USD ZAR Exchange Rate
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD ZAR Exchange Rate object </returns>
        public async Task<IActionResult> GetApiExchange(string ApiUrl)
        {

            Responses.ExchangeResponse responseObject = new Responses.ExchangeResponse();
           
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<Responses.ExchangeResponse>(jsonDocument);
                }
                return Ok(responseObject);

            }
            catch (Exception ex)
            {

                _logger.LogError("An error occurred on  the API Call", ex);

                return BadRequest();
            }

        }

        ///// <summary>
        ///// Returns the FCR chart object and meta data needed to build the chart.
        ///// </summary>
        ///// <returns>A chart details object</returns>
        //[Route("~/api/GetArbitrageValueBitCoin")]
        //[HttpGet]
        //public JsonResult GetArbitrageValueBitCoin()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Fetching all the values from the different API requests and perfoming an BitCoint arbitrage calculation");
        //        Responses.AskResponse usdAskBitCoinObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
        //        double usdAskBitCoin = Convert.ToDouble(usdAskBitCoinObject.ask);

        //        Responses.BidResponse zarBidBitCoinObject = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
        //        double zarBidBitCoin = Convert.ToDouble(zarBidBitCoinObject.bidPrice);

        //        Responses.ExchangeResponse usdZarExchangeObject = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
        //        double usdZarExchange = usdZarExchangeObject.conversion_rate;

        //        double bitCoinArbitrageValue = ArbitrageCaculator(zarBidBitCoin, usdAskBitCoin, usdZarExchange);

        //        Dictionary<string, double> listObject = new Dictionary<string, double>();
        //        listObject.Add("zarBidBitCoin", zarBidBitCoin);
        //        listObject.Add("usdAskBitCoin", usdAskBitCoin);
        //        listObject.Add("usdZarExchange", usdZarExchange);
        //        listObject.Add("arbitrageCalculationForBitcoin", Math.Round(bitCoinArbitrageValue, 2));

        //        _logger.LogInformation($"Returning BitCoin Price Arbitage values and calculation result.");
        //        return Json(listObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong: {ex}");
        //        return Json(StatusCode(500, "Internal server error"));
        //    }
        //}

        ///// <summary>
        ///// API Call to get Calculations
        ///// </summary>
        ///// <returns>A chart details object</returns>
        //[Route("~/api/GetArbitrageValueXRP")]
        //[HttpGet]
        //public JsonResult GetArbitrageValueXRP()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Fetching all the values from the different API requests and perfoming an XRP arbitrage calculation");
        //        Responses.AskResponse usdAskXRPObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");
        //        double usdAskXRP = Convert.ToDouble(usdAskXRPObject.ask);

        //        Responses.BidResponse zarBidXRPObject = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");
        //        double zarBidXRP = Convert.ToDouble(zarBidXRPObject.bidPrice);

        //        Responses.ExchangeResponse usdZarExchangeObject = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
        //        double usdZarExchange = usdZarExchangeObject.conversion_rate;

        //        double xrpArbitrageValue = ArbitrageCaculator(zarBidXRP, usdAskXRP, usdZarExchange);

        //        Dictionary<string, double> listObject = new Dictionary<string, double>();
        //        listObject.Add("zarBidXRP", zarBidXRP);
        //        listObject.Add("usdAskXRP", usdAskXRP);
        //        listObject.Add("usdZarExchange", usdZarExchange);
        //        listObject.Add("arbitrageCalculationForXRP", Math.Round(xrpArbitrageValue, 2));

        //        _logger.LogInformation($"Returning XRP Price Arbitage values and calculation result.");
        //        return Json(listObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Something went wrong: {ex}");
        //        return Json(StatusCode(500, "Internal server error"));
        //    }
        //}

    }
}
