using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Net.Http.Headers;

namespace AoaiStreamFunction
{
    public static class TestStream
    {
        [FunctionName("test-stream")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var response = req.HttpContext.Response;
            response.Headers.Add(HeaderNames.ContentType, "text/event-stream");
            response.Headers.Add(HeaderNames.CacheControl, CacheControlHeaderValue.NoCacheString);

            string returnMsg = "Hello World";
            string dataformat = "data: {0}\r\n\r\n";
            for (int i = 0; i < returnMsg.Length; i++)
            {
                log.LogInformation(string.Format(dataformat, returnMsg[i]));
                await response.WriteAsync(string.Format(dataformat, returnMsg[i]));
                await response.Body.FlushAsync();
                await Task.Delay(1000);
            }

            await response.WriteAsync("event: finish\r\n");
            await response.WriteAsync(string.Format(dataformat, string.Empty));
            await response.Body.FlushAsync();
            return new EmptyResult();
        }
    }
}
