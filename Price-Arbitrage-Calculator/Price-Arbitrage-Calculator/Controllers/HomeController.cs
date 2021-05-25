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
        public JsonResult GetValues()
        {
            Responses.AskResponse usdAskBitCoinObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
            Responses.AskResponse usdAskXRPObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");

            Responses.BidResponse zarBidBitCoinObject = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
            Responses.BidResponse zarBidXRPObject = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");

            Responses.ExchangeResponse usdZarExchange = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");

            double bitCoinArbitrageValue = ArbitrageCaculator(Convert.ToDouble(zarBidBitCoinObject.bidPrice), Convert.ToDouble(usdAskBitCoinObject.ask), usdZarExchange.conversion_rate);
            double xrpArbitrageValue = ArbitrageCaculator(Convert.ToDouble(zarBidXRPObject.bidPrice), Convert.ToDouble(usdAskXRPObject.ask), usdZarExchange.conversion_rate);

            Dictionary<string, double> returnObject = new Dictionary<string, double>();
            returnObject.Add("bitCoinArbitrageValue", Math.Round(bitCoinArbitrageValue, 2));
            returnObject.Add("xrpArbitrageValue", Math.Round(xrpArbitrageValue, 2));


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

        // GET
        // USD ASK price
        /// <summary>
        /// Makes API Call to get ASK Price object
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD ASK Price object </returns>
        public static Responses.AskResponse GetApiAsk(string ApiUrl)
        {

            Responses.AskResponse responseObject = new Responses.AskResponse();
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string apiResponse = reader.ReadToEnd();
                        responseObject = JsonConvert.DeserializeObject<Responses.AskResponse>(apiResponse);
                    }
                }
                return (responseObject);

            }
            catch (Exception ex)
            {

                #if DEBUG
                                throw;
                #endif
                throw new Exception("An error occurred on  the API Call", ex);
            }

        }

        // GET
        // USD BID price
        /// <summary>
        /// Makes API Call to get BID Price object
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD BID Price object </returns>
        public static Responses.BidResponse GetApiBid(string ApiUrl)
        {
            Responses.BidResponse responseObject = new Responses.BidResponse();
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string apiResponse = reader.ReadToEnd();
                        responseObject = JsonConvert.DeserializeObject<Responses.BidResponse>(apiResponse);
                    }
                }
                return (responseObject);

            }
            catch (Exception ex)
            {
                #if DEBUG
                                throw;
                #endif
                throw new Exception("An error occurred on the API Call", ex);
            }

        }

        // GET
        // USD ZAR Exchange Rate
        /// <summary>
        /// Makes API Call to get USD ZAR Exchange Rate
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD ZAR Exchange Rate object </returns>
        public static Responses.ExchangeResponse GetApiExchange(string ApiUrl)
        {

            Responses.ExchangeResponse responseObject = new Responses.ExchangeResponse();
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string apiResponse = reader.ReadToEnd();
                        responseObject = JsonConvert.DeserializeObject<Responses.ExchangeResponse>(apiResponse);
                    }
                }
                return (responseObject);

            }
            catch (Exception ex)
            {

                #if DEBUG
                                throw;
                #endif
                throw new Exception("An error occurred on the API Call", ex);
            }

        }

        /// <summary>
        /// Returns the FCR chart object and meta data needed to build the chart.
        /// </summary>
        /// <returns>A chart details object</returns>
        [Route("~/api/GetArbitrageValueBitCoin")]
        [HttpGet]
        public JsonResult GetArbitrageValueBitCoin()
        {
            try
            {
                _logger.LogInformation("Fetching all the values from the different API requests and perfoming an BitCoint arbitrage calculation");
                Responses.AskResponse usdAskBitCoinObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
                double usdAskBitCoin = Convert.ToDouble(usdAskBitCoinObject.ask);

                Responses.BidResponse zarBidBitCoinObject = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
                double zarBidBitCoin = Convert.ToDouble(zarBidBitCoinObject.bidPrice);

                Responses.ExchangeResponse usdZarExchangeObject = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
                double usdZarExchange = usdZarExchangeObject.conversion_rate;

                double bitCoinArbitrageValue = ArbitrageCaculator(zarBidBitCoin, usdAskBitCoin, usdZarExchange);

                Dictionary<string, double> listObject = new Dictionary<string, double>();
                listObject.Add("zarBidBitCoin", zarBidBitCoin);
                listObject.Add("usdAskBitCoin", usdAskBitCoin);
                listObject.Add("usdZarExchange", usdZarExchange);
                listObject.Add("arbitrageCalculationForBitcoin", Math.Round(bitCoinArbitrageValue, 2));

                _logger.LogInformation($"Returning BitCoin Price Arbitage values and calculation result.");
                return Json(listObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return Json(StatusCode(500, "Internal server error"));
            }
        }

        /// <summary>
        /// API Call to get Calculations
        /// </summary>
        /// <returns>A chart details object</returns>
        [Route("~/api/GetArbitrageValueXRP")]
        [HttpGet]
        public JsonResult GetArbitrageValueXRP()
        {
            try
            {
                _logger.LogInformation("Fetching all the values from the different API requests and perfoming an XRP arbitrage calculation");
                Responses.AskResponse usdAskXRPObject = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");
                double usdAskXRP = Convert.ToDouble(usdAskXRPObject.ask);

                Responses.BidResponse zarBidXRPObject = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");
                double zarBidXRP = Convert.ToDouble(zarBidXRPObject.bidPrice);

                Responses.ExchangeResponse usdZarExchangeObject = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
                double usdZarExchange = usdZarExchangeObject.conversion_rate;

                double xrpArbitrageValue = ArbitrageCaculator(zarBidXRP, usdAskXRP, usdZarExchange);

                Dictionary<string, double> listObject = new Dictionary<string, double>();
                listObject.Add("zarBidXRP", zarBidXRP);
                listObject.Add("usdAskXRP", usdAskXRP);
                listObject.Add("usdZarExchange", usdZarExchange);
                listObject.Add("arbitrageCalculationForXRP", Math.Round(xrpArbitrageValue, 2));

                _logger.LogInformation($"Returning XRP Price Arbitage values and calculation result.");
                return Json(listObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return Json(StatusCode(500, "Internal server error"));
            }
        }

    }
}
