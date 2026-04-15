using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MiMundoHuellitas.Controllers
{
    public class PayPalController : Controller
    {
        private readonly string clientId = ConfigurationManager.AppSettings["PayPalClientId"];
        private readonly string secret = ConfigurationManager.AppSettings["PayPalSecret"];
        private readonly string baseUrl = ConfigurationManager.AppSettings["PayPalBaseUrl"];

        private async Task<string> GetAccessToken()
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var content = new StringContent(
                    "grant_type=client_credentials",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"
                );

                var response = await client.PostAsync($"{baseUrl}/v1/oauth2/token", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error obteniendo token de PayPal: " + json);
                }

                var obj = JObject.Parse(json);
                return obj["access_token"]?.ToString();
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder()
        {
            try
            {
                decimal total = 15.00m;

                var accessToken = await GetAccessToken();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    var body = new
                    {
                        intent = "CAPTURE",
                        purchase_units = new[]
                        {
                            new
                            {
                                amount = new
                                {
                                    currency_code = "USD",
                                    value = total.ToString("F2")
                                }
                            }
                        }
                    };

                    var jsonBody = JsonConvert.SerializeObject(body);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{baseUrl}/v2/checkout/orders", content);
                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = json });
                    }

                    var obj = JObject.Parse(json);

                    return Json(new
                    {
                        success = true,
                        id = obj["id"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CaptureOrder(string orderID)
        {
            try
            {
                var accessToken = await GetAccessToken();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    var response = await client.PostAsync(
                        $"{baseUrl}/v2/checkout/orders/{orderID}/capture",
                        new StringContent("", Encoding.UTF8, "application/json")
                    );

                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = json });
                    }

                    var obj = JObject.Parse(json);

                    return Json(new
                    {
                        success = true,
                        paypalOrderId = obj["id"]?.ToString(),
                        status = obj["status"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}