using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Price_Arbitrage_Calculator.Controllers.ControllerHelpers;
using Price_Arbitrage_Calculator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Price_Arbitrage_Calculator.Models.ErrorViewModel;
using static Price_Arbitrage_Calculator.Models.Responses;

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

            JsonResponseObject bitCoin = new JsonResponseObject();
            JsonResponseObject xrp = new JsonResponseObject();

            double bitCoinArbitrageValue=0;
            double xrpArbitrageValue=0;

            StatusCodeResult errorObject = null ;

            AskResponse usdAskBitCoinObject = new AskResponse();
            AskResponse usdAskXRPObject = new AskResponse();
            BidResponse zarBidBitCoinObject = new BidResponse();
            BidResponse zarBidXRPObject = new BidResponse();
            ExchangeResponse usdZarExchangeObject = new ExchangeResponse();

            var task1 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
            var task2 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");
            var task3 = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
            var task4 = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");
            var task5 = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
            await Task.WhenAll(task1, task2, task3, task4, task5); // Make all the API calls, at the same time, wait for them to come back at the same time

            //Asign the results to the objects, using await again, to make sure that no task is waiting for another
            var usdAskBitCoin = await task1 ; 
            OkObjectResult usdAskBitCoinOkResult = usdAskBitCoin as OkObjectResult;

            var usdAskXRP = await task2;
            OkObjectResult usdAskXRPOkResult = usdAskXRP as OkObjectResult;

            var zarBidBitCoin = await task3;
            OkObjectResult zarBidBitCoinOkResult = zarBidBitCoin as OkObjectResult;

            var zarBidXRP = await task4;
            OkObjectResult zarBidXRPOkResult = zarBidXRP as OkObjectResult;

            var usdZarExchange = await task5 ;
            OkObjectResult usdZarExchangeOkResult = usdZarExchange as OkObjectResult;

            //Checking status of the call
            if (usdAskBitCoinOkResult != null)
            {
                usdAskBitCoinObject = (AskResponse)usdAskBitCoinOkResult.Value;
            }
            else
            {
                errorObject = usdAskBitCoin as StatusCodeResult;
                bitCoin.Error = errorObject.StatusCode.ToString();
            }
               
            if (usdAskXRPOkResult != null)
            {
                usdAskXRPObject = (AskResponse)usdAskXRPOkResult.Value;
            }
            else
            {
                errorObject = usdAskXRP as StatusCodeResult;
                xrp.Error = errorObject.StatusCode.ToString();
            }
                
            if (zarBidBitCoinOkResult != null)
            {
                zarBidBitCoinObject = (BidResponse)zarBidBitCoinOkResult.Value;
            }
            else
            {
                errorObject = zarBidBitCoin as StatusCodeResult;
                bitCoin.Error = errorObject.StatusCode.ToString();
            }
                
            if (zarBidXRPOkResult != null)
            {
                zarBidXRPObject = (BidResponse)zarBidXRPOkResult.Value;
            }
            else
            {
                errorObject = zarBidXRP as StatusCodeResult;
                xrp.Error = errorObject.StatusCode.ToString();
            }

            if (usdZarExchangeOkResult != null)
            {
                usdZarExchangeObject = (ExchangeResponse)usdZarExchangeOkResult.Value;
            }
            else
            {
                errorObject = usdZarExchange as StatusCodeResult;
                bitCoin.Error = errorObject.StatusCode.ToString(); ;
                xrp.Error = errorObject.StatusCode.ToString(); ;
            }
            
            if(String.IsNullOrEmpty(bitCoin.Error))
                bitCoinArbitrageValue = CalculatorHelper.ArbitrageCaculator(Convert.ToDouble(zarBidBitCoinObject.bidPrice), Convert.ToDouble(usdAskBitCoinObject.ask), usdZarExchangeObject.conversion_rate);
            if (String.IsNullOrEmpty(xrp.Error))
                xrpArbitrageValue = CalculatorHelper.ArbitrageCaculator(Convert.ToDouble(zarBidXRPObject.bidPrice), Convert.ToDouble(usdAskXRPObject.ask), usdZarExchangeObject.conversion_rate);

            Dictionary<string, double> bitCoinData = new Dictionary<string, double>();
            bitCoinData.Add("bitCoinArbitrageValue", bitCoinArbitrageValue);
            bitCoin.Data = bitCoinData;

            Dictionary<string, double> xRPData = new Dictionary<string, double>();
            xRPData.Add("xrpArbitrageValue", xrpArbitrageValue);
            xrp.Data = xRPData;

            List <JsonResponseObject> returnObject = new List<JsonResponseObject>();
            returnObject.Add(bitCoin);
            returnObject.Add(xrp);

            watch.Stop();

            System.Diagnostics.Debug.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            return Json(returnObject);
        }
      
        // USD ASK price
        /// <summary>
        /// Makes API Call to get ASK Price object
        /// </summary>
        /// <param name="ApiUrl"></param>
        /// <returns>Returns USD ASK Price object </returns>
        public async Task<IActionResult> GetApiAsk(string ApiUrl)
        {
            AskResponse responseObject = new AskResponse();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<AskResponse>(jsonDocument);
                }

                return Ok(responseObject);

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred on  the API Call", ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
            BidResponse responseObject = new BidResponse();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<BidResponse>(jsonDocument);
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred on  the API Call", ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
            ExchangeResponse responseObject = new ExchangeResponse();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(ApiUrl).ConfigureAwait(false);
                    var jsonDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    responseObject = JsonConvert.DeserializeObject<ExchangeResponse>(jsonDocument);
                }

                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred on  the API Call", ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

        /// <summary>
        /// Returns the Aritrage List object for Bitcoin
        /// </summary>
        /// <returns>A chart details object</returns>
        [Route("~/api/GetArbitrageValueBitCoin")]
        [HttpGet]
        public async Task<JsonResult> GetArbitrageValueBitCoin()
        {
            double usdAskBitCoinValue = 0;
            double zarBidBitCoinValue = 0;
            double usdZarExchangeValue = 0;

            try
            {
                _logger.LogInformation("Fetching all the values from the different API requests and perfoming an BitCoint arbitrage calculation");
                AskResponse usdAskBitCoinObject = new AskResponse();
                BidResponse zarBidBitCoinObject = new BidResponse();
                ExchangeResponse usdZarExchangeObject = new ExchangeResponse();

                var task1 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/btcusd/");
                var task2 = GetApiBid("https://api.valr.com/v1/public/BTCZAR/marketsummary");
                var task3 = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
                await Task.WhenAll(task1, task2, task3);

                var usdAskBitCoin = await task1;
                OkObjectResult usdAskBitCoinOkResult = usdAskBitCoin as OkObjectResult;

                var zarBidBitCoin = await task2;
                OkObjectResult zarBidBitCoinOkResult = zarBidBitCoin as OkObjectResult;

                var usdZarExchange = await task3;
                OkObjectResult usdZarExchangeOkResult = usdZarExchange as OkObjectResult;

                if (usdAskBitCoinOkResult != null)
                {
                    usdAskBitCoinObject = (AskResponse)usdAskBitCoinOkResult.Value;
                    usdAskBitCoinValue = Convert.ToDouble(usdAskBitCoinObject.ask);
                }
                else
                    throw new Exception();

                if (zarBidBitCoinOkResult != null)
                {
                    zarBidBitCoinObject = (BidResponse)zarBidBitCoinOkResult.Value;
                    zarBidBitCoinValue = Convert.ToDouble(zarBidBitCoinObject.bidPrice);
                }
                else
                    throw new Exception();

                if (usdZarExchangeOkResult != null)
                {
                    usdZarExchangeObject = (ExchangeResponse) usdZarExchangeOkResult.Value;
                    usdZarExchangeValue = Convert.ToDouble(usdZarExchangeObject.conversion_rate);
                }
                else
                    throw new Exception();

                double bitCoinArbitrageValue = CalculatorHelper.ArbitrageCaculator(zarBidBitCoinValue, usdAskBitCoinValue, usdZarExchangeValue);
                Dictionary<string, double> listObject = new Dictionary<string, double>();
                listObject.Add("zarBidBitCoin", zarBidBitCoinValue);
                listObject.Add("usdAskBitCoin", usdAskBitCoinValue);
                listObject.Add("usdZarExchange", usdZarExchangeValue);
                listObject.Add("arbitrageCalculationForBitcoin", bitCoinArbitrageValue);

                _logger.LogInformation($"Returning BitCoin Price Arbitage values and calculation result.");
                return Json(listObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                return Json(StatusCode(500, "Internal server error"));
            }
        }

        /// <summary>
        /// Returns the Aritrage List object for XRP
        /// </summary>
        /// <returns>A chart details object</returns>
        [Route("~/api/GetArbitrageValueXRP")]
        [HttpGet]
        public async Task<JsonResult> GetArbitrageValueXRP()
        {
            double usdAskXRPValue = 0;
            double zarBidXRPValue = 0;
            double usdZarExchangeValue = 0;
            try
            {
                _logger.LogInformation("Fetching all the values from the different API requests and perfoming an BitCoint arbitrage calculation");
                AskResponse usdAskXRPObject = new AskResponse();
                BidResponse zarBidXRPObject = new BidResponse();
                ExchangeResponse usdZarExchangeObject = new ExchangeResponse();

                var task1 = GetApiAsk("https://www.bitstamp.net/api/v2/ticker/xrpusd/");
                var task2 = GetApiBid("https://api.valr.com/v1/public/XRPZAR/marketsummary");
                var task3 = GetApiExchange("https://v6.exchangerate-api.com/v6/1d3b83b2178cb028dba53670/pair/USD/ZAR");
                await Task.WhenAll(task1, task2, task3);

                var usdAskXRP = await task1;
                OkObjectResult usdAskXRPOkResult = usdAskXRP as OkObjectResult;

                var zarBidXRP = await task2;
                OkObjectResult zarBidXRPOkResult = zarBidXRP as OkObjectResult;

                var usdZarExchange = await task3;
                OkObjectResult usdZarExchangeOkResult = usdZarExchange as OkObjectResult;

                if (usdAskXRPOkResult != null)
                {
                    usdAskXRPObject = (AskResponse)usdAskXRPOkResult.Value;
                    usdAskXRPValue = Convert.ToDouble(usdAskXRPObject.ask);
                }
                else
                    throw new Exception();

                if (zarBidXRPOkResult != null)
                {
                    zarBidXRPObject = (BidResponse)zarBidXRPOkResult.Value;
                    zarBidXRPValue = Convert.ToDouble(zarBidXRPObject.bidPrice);
                }
                else
                    throw new Exception();

                if (usdZarExchangeOkResult != null)
                {
                    usdZarExchangeObject = (ExchangeResponse)usdZarExchangeOkResult.Value;
                    usdZarExchangeValue = Convert.ToDouble(usdZarExchangeObject.conversion_rate);
                }
                else
                    throw new Exception();

                double xrpArbitrageValue = CalculatorHelper.ArbitrageCaculator(zarBidXRPValue, usdAskXRPValue, usdZarExchangeValue);

                Dictionary<string, double> listObject = new Dictionary<string, double>();
                listObject.Add("zarBidXRP", zarBidXRPValue);
                listObject.Add("usdAskXRP", usdAskXRPValue);
                listObject.Add("usdZarExchange", usdZarExchangeValue);
                listObject.Add("arbitrageCalculationForXRP", xrpArbitrageValue);

                _logger.LogInformation($"Returning XRP Price Arbitage values and calculation result.");
                return Json(listObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                return Json(StatusCode(500, "Internal server error"));
            }
        }
    }
}
