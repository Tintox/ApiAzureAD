using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace APIExterna.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {        
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

      [HttpGet]
      public async Task<IEnumerable<WeatherForecast>> GetAsync()
      {
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

          HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress + "/weatherforecast");
          if (response.IsSuccessStatusCode)
          {
            //string json = await response.Content.ReadAsStringAsync();
            var data = await response.Content.ReadAsStringAsync();
            var adulto = JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(data);
            return (IEnumerable<WeatherForecast>)adulto;
          }
          else
          {
            var content = await response.Content.ReadAsStringAsync();

            if (response.Content != null)
              response.Content.Dispose();

            throw new HttpResponseException(response.StatusCode, content);
          }
        }
        else
        {
          throw new HttpResponseException(HttpStatusCode.Unauthorized, result.AccessToken);
        }
      }
    
    }
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpResponseException(HttpStatusCode statusCode, string content) : base(content)
        {
            StatusCode = statusCode;
        }
    }
}
