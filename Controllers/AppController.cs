using Intuit.Ipp.OAuth2PlatformClient;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Net;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Intuit.Ipp.DataService;
using System.Net.Http;
using System.IdentityModel.Claims;
using Newtonsoft.Json.Linq;

namespace MvcCodeFlowClientManual.Controllers
{
    public class AppController : Controller
    {
        public static string clientid = ConfigurationManager.AppSettings["clientid"];
        public static string clientsecret = ConfigurationManager.AppSettings["clientsecret"];
        public static string redirectUrl = ConfigurationManager.AppSettings["redirectUrl"];
        public static string environment = ConfigurationManager.AppSettings["appEnvironment"];

        public static OAuth2Client auth2Client = new OAuth2Client(clientid, clientsecret, redirectUrl, environment);

        /// <summary>
        /// Use the Index page of App controller to get all endpoints from discovery url
        /// </summary>
        public ActionResult Index()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Session.Clear();
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut("Cookies");
            return View();
        }

        /// <summary>
        /// Start Auth flow
        /// </summary>
        public ActionResult InitiateAuth(string submitButton)
        {
            switch (submitButton)
            {
                case "Connect to QuickBooks":
                    List<OidcScopes> scopes = new List<OidcScopes>();
                    scopes.Add(OidcScopes.Accounting);
                    string authorizeUrl = auth2Client.GetAuthorizationURL(scopes);
                    return Redirect(authorizeUrl);
                default:
                    return (View());
            }
        }

        /// <summary>
        /// QBO API Request
        /// </summary>
        public async Task<ActionResult> ApiCallService(string inputData1, string inputData2, string inputData3, string inputData4, string inputData5, string inputData6, string inputData7, string inputData8, string inputData9)
        {
            if (Session["realmId"] != null)
            {
                string realmId = Session["realmId"].ToString();
                try
                {
                    var principal = User as ClaimsPrincipal;
                    OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(principal.FindFirst("access_token").Value);

                    // Create a ServiceContext with Auth tokens and realmId
                    ServiceContext serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, oauthValidator);
                    serviceContext.IppConfiguration.MinorVersion.Qbo = "23";

                    // Create a QuickBooks QueryService using ServiceContext
                    QueryService<CompanyInfo> c = new QueryService<CompanyInfo>(serviceContext);
                    CompanyInfo companyInfo =c.ExecuteIdsQuery("SELECT * FROM CompanyInfo").FirstOrDefault();

                   /* var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://sandbox-quickbooks.api.intuit.com/v3/company/4620816365300853590/customer?minorversion=23");
                    //request.Headers.Add("User-Agent", "{{UserAgent}}");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", "Bearer " + principal.FindFirst("access_token").Value);
                    //  "//eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..HFNC4OUkmgoqI0y9hPb9yQ.sgOpoNa5El9Hk3szsqTm_Q7DhbSJJuQ69dOZCIvYlVTlgNU8QD2uskHMHFGp8bcDhNyMz648b-3_tbNkcQ-QgddvGKtf2VXnebJcztyL2GuqtbAB-trt39CsU-A-LnsagFoLVEwHx4KLhqj3I7r0-YxVRZU4mi0V-gKa3kFLdggaTuMt3QWG3rkoW98D6bq_SAtYnnDYDUoR5hDAUJVLTcGdh6RE1BmOp66vuzFgruaibnLXiFamxnCPPQK8h7Fsji3KrZVTjt7TdnyqEUk2aPHOKUwsKvyjqZ0IpEOVPcD_ZXyuatg4HUVr8GAyubWqeUcCl_i6AswEzE_SbjHmLx0jRhxdyRr7wOdPXKFNGpXwLl2BlLsSDk4NRuWzOZgSw9jOvyweJNn1qMm9Jc9XexhFyB4Uio07CrznP3jngWBgNIF2Uwm9rzQ9wr2hqOS0GDWBaDSF8FCMGOr3J4rcLcVQ3FF67wsDpglMQxlZa5eMFfy217QSJmYPrnqxNg9UiJhlkcwwOHgxjcX7YbNq3nsO-jneVWtOna81ddeBPa8lLSDe2yVKfWLWJKZj6l9HNwE5GeIMGzawHYe9SbrnwMc-aiEUyhCYD2DPur5N88Txx8U8xmRqEicWSz03DgB247NzTtdZI2k66DGzgJVfISsCMGS5IRR_oOMOLZ-gJZ9VH9ivMzJD0IFeyed4b0pnus-hf_2Cn_kGuP_28ukPiym8Gdl0PRQNnlrS2c7jIq_84PrYAnk2_WQGwTurpxDy.fCkkYclb-mK3xWJjvITEeg");
                    var content = new StringContent("{\n    \"BillAddr\": {\n        \"Line1\": \"" + inputData1 + "\",\n        \"City\": \"" + inputData2 + "\",\n        \"Country\": \"" + inputData3 + "\",\n        \"CountrySubDivisionCode\": \"" + inputData4 + "\",\n        \"PostalCode\": \"" + inputData5 + "\"\n    },\n    \"Notes\": \"" + inputData6 + "\",\n    \"DisplayName\": \"" + inputData7 + "\",\n    \"PrimaryPhone\": {\n        \"FreeFormNumber\": \"" + inputData8 + "\"\n    },\n    \"PrimaryEmailAddr\": {\n        \"Address\": \"" + inputData9 + "\"\n    }\n}\n", null, "application/json");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine(await response.Content.ReadAsStringAsync());*/


                    string output = "Customer Has been Added" ;
                    return View("ApiCallService", (object)("QBO API call Successful!! Response: " + output));
                }
                catch (Exception ex)
                {
                    return View("ApiCallService", (object)("QBO API call Failed!" + " Error message: " + ex.Message));
                }
            }
            else
                return View("ApiCallService", (object)"QBO API call Failed!");
        }
        public async Task<ActionResult> GetNames()
        {
            if (Session["realmId"] != null)
            {
                string realmId = Session["realmId"].ToString();
                try
                {
                    var principal = User as ClaimsPrincipal;
                    OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(principal.FindFirst("access_token").Value);

                    // Create a ServiceContext with Auth tokens and realmId
                    ServiceContext serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, oauthValidator);
                    serviceContext.IppConfiguration.MinorVersion.Qbo = "23";

                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://sandbox-quickbooks.api.intuit.com/v3/company/4620816365300853590/query?query=select * from Customer&minorversion=23");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", "Bearer "+principal.FindFirst("access_token").Value.ToString());
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse=await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(jsonResponse);
                    dynamic responseObj = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                    //var ans = responseObj.Customer[0].DisplayName
                    JArray jsonArray = (JArray)json["QueryResponse"]["Customer"]; // replace "myArray" with the actual name of your array
                    int length = jsonArray.Count;
                    var Customer = json["QueryResponse"]["Customer"];
                    string[] name = new string[length];
                    //string name = "";
                    int i = 0;
                    foreach(var customer in Customer)
                    {
                        string x = customer["DisplayName"].ToString();
                        name[i] = x;
                        i++;
                    }
                    //ViewBag.Name = name;
                    return View("GetNames",name);
                }
                catch (Exception e)
                {
                    return View("GetNames",(object)e.Message);
                }
            }

            return View("GetNames");
        }
        /// <summary>
        /// Use the Index page of App controller to get all endpoints from discovery url
        /// </summary>
        public ActionResult Error()
        {
            return View("Error");
        }

        /// <summary>
        /// Action that takes redirection from Callback URL
        /// </summary>
        public ActionResult Tokens()
        {
            return View("Tokens");
        }

        
    }
}