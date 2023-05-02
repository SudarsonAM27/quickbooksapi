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
        public async Task<ActionResult> ApiCallService()
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

                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://sandbox-quickbooks.api.intuit.com/v3/company/4620816365300853590/customer?minorversion=23");
                    //request.Headers.Add("User-Agent", "{{UserAgent}}");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", "Bearer " + principal.FindFirst("access_token").Value);
                      //  "//eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..HFNC4OUkmgoqI0y9hPb9yQ.sgOpoNa5El9Hk3szsqTm_Q7DhbSJJuQ69dOZCIvYlVTlgNU8QD2uskHMHFGp8bcDhNyMz648b-3_tbNkcQ-QgddvGKtf2VXnebJcztyL2GuqtbAB-trt39CsU-A-LnsagFoLVEwHx4KLhqj3I7r0-YxVRZU4mi0V-gKa3kFLdggaTuMt3QWG3rkoW98D6bq_SAtYnnDYDUoR5hDAUJVLTcGdh6RE1BmOp66vuzFgruaibnLXiFamxnCPPQK8h7Fsji3KrZVTjt7TdnyqEUk2aPHOKUwsKvyjqZ0IpEOVPcD_ZXyuatg4HUVr8GAyubWqeUcCl_i6AswEzE_SbjHmLx0jRhxdyRr7wOdPXKFNGpXwLl2BlLsSDk4NRuWzOZgSw9jOvyweJNn1qMm9Jc9XexhFyB4Uio07CrznP3jngWBgNIF2Uwm9rzQ9wr2hqOS0GDWBaDSF8FCMGOr3J4rcLcVQ3FF67wsDpglMQxlZa5eMFfy217QSJmYPrnqxNg9UiJhlkcwwOHgxjcX7YbNq3nsO-jneVWtOna81ddeBPa8lLSDe2yVKfWLWJKZj6l9HNwE5GeIMGzawHYe9SbrnwMc-aiEUyhCYD2DPur5N88Txx8U8xmRqEicWSz03DgB247NzTtdZI2k66DGzgJVfISsCMGS5IRR_oOMOLZ-gJZ9VH9ivMzJD0IFeyed4b0pnus-hf_2Cn_kGuP_28ukPiym8Gdl0PRQNnlrS2c7jIq_84PrYAnk2_WQGwTurpxDy.fCkkYclb-mK3xWJjvITEeg");
                    var content = new StringContent("{\n    \"BillAddr\": {\n        \"Line1\": \"123 Main Street\",\n        \"City\": \"Mountain View\",\n        \"Country\": \"IND\",\n        \"CountrySubDivisionCode\": \"CA\",\n        \"PostalCode\": \"94042\"\n    },\n    \"Notes\": \"Here are other details.\",\n    \"DisplayName\": \"New Company\",\n    \"PrimaryPhone\": {\n        \"FreeFormNumber\": \"(555) 555-5555\"\n    },\n    \"PrimaryEmailAddr\": {\n        \"Address\": \"jdrew@myemail.com\"\n    }\n}\n", null, "application/json");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine(await response.Content.ReadAsStringAsync());


                    string output = "Data pushed" ;
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

        public void CreateInvoice(ServiceContext serviceContext)
        {
            /// Step 1: Initialize OAuth2RequestValidator and ServiceContext
            
            /// Step 2: Initialize an Invoice object
            Invoice invoice = new Invoice();
            invoice.Deposit = new Decimal(0.00);
            invoice.DepositSpecified = true;

            /// Step 3: Invoice is always created for a customer, so retrieve reference to a customer and set it in Invoice
            QueryService<Customer> querySvc = new QueryService<Customer>(serviceContext);
            Customer customer = querySvc.ExecuteIdsQuery
                    ("SELECT * FROM Customer WHERE CompanyName like 'Amy%'").
                    FirstOrDefault();
            invoice.CustomerRef = new ReferenceType()
            {
                Value = customer.Id
            };

            /// Step 4: Invoice is always created for an item so the retrieve reference to an item and create a Line item to the invoice
            QueryService<Item> querySvcItem =new QueryService<Item>(serviceContext);
            Item item = querySvcItem.ExecuteIdsQuery("SELECT * FROM Item WHERE Name = 'Lighting'").FirstOrDefault();
            List<Line> lineList = new List<Line>();
            Line line = new Line();
            line.Description = "Description";
            line.Amount = new Decimal(100.00);
            line.AmountSpecified = true;
            lineList.Add(line);
            invoice.Line = lineList.ToArray();

            SalesItemLineDetail salesItemLineDeatil = new SalesItemLineDetail();
            salesItemLineDeatil.Qty = new Decimal(1.0);
            salesItemLineDeatil.ItemRef = new ReferenceType
            {
                Value = item.Id
            };
            line.AnyIntuitObject = salesItemLineDeatil;

            line.DetailType = LineDetailTypeEnum.SalesItemLineDetail;
            line.DetailTypeSpecified = true;

            /// Step 5: Set other properties such as total amount, due date, email status, and transaction date
            invoice.DueDate = DateTime.UtcNow.Date;
            invoice.DueDateSpecified = true;

            invoice.TotalAmt = new Decimal(10.00);
            invoice.TotalAmtSpecified = true;

            invoice.EmailStatus = EmailStatusEnum.NotSet;
            invoice.EmailStatusSpecified = true;

            invoice.Balance = new Decimal(10.00);
            invoice.BalanceSpecified = true;

            invoice.TxnDate = DateTime.UtcNow.Date;
            invoice.TxnDateSpecified = true;
            invoice.TxnTaxDetail = new TxnTaxDetail()
            {
                TotalTax = Convert.ToDecimal(10),
                TotalTaxSpecified = true
            };

            ///Step 6: Initialize the service object and create Invoice
            DataService service = new DataService(serviceContext);
            Invoice addedInvoice = service.Add<Invoice>(invoice);
        }
    }
}