using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AppCliente.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IConfiguration Configuration { get; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;

        }
        public void OnGet()
            {
              ViewData["response"] = "none";
            }
        public async Task OnPostAsync()
        {
            //----------Call API Externa (Publica - sin Authorize)--------------------
            //var httpClient = new HttpClient();
            //var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

            //if (defaultRequestHeaders.Accept == null ||
            //    !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            //{
            //  httpClient.DefaultRequestHeaders.Accept.Add(new
            //    MediaTypeWithQualityHeaderValue("application/json"));
            //}
            //var requestUri = Configuration["BaseAddress"].ToString() + "/weatherforecast";
            //HttpResponseMessage response = await httpClient.GetAsync(requestUri);
            //if (response.IsSuccessStatusCode)
            //{
            //  //string json = await response.Content.ReadAsStringAsync();
            //  var data = await response.Content.ReadAsStringAsync();
            //  ViewData["response"] = data;
            //}
            //else
            //{
            //  ViewData["response"] = System.Net.HttpStatusCode.BadRequest;
            //}

            ////----------Call API Externa (Authorize)--------------------

            AuthConfig config = AuthConfig.ReadFromJsonFile("appsettings.json");

            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            string[] ResourceIds = new string[] { config.ResourceID };

            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();
            }
            catch (MsalClientException ex)
            {
                throw new MsalClientException(ex.ErrorCode, ex.Message);
            }

            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null ||
                    !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new
                      MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("bearer", result.AccessToken);
                var requestUri = Configuration["BaseAddress"].ToString() + "/weatherforecast";
                HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    //string json = await response.Content.ReadAsStringAsync();
                    var data = await response.Content.ReadAsStringAsync();
                    ViewData["response"] = data;
                }
                else
                {
                    ViewData["response"] = System.Net.HttpStatusCode.BadRequest;
                }
            }
            else
            {
                ViewData["response"] = System.Net.HttpStatusCode.Unauthorized;
            }

        }
  }
}
