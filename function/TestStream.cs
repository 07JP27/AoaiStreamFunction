using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace AoaiStreamFunction
{
    public class TestStream(ILogger<TestStream> _logger)
    {
        [Function("test-stream")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var response = req.HttpContext.Response;
            response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
            response.Headers.Append(HeaderNames.CacheControl, CacheControlHeaderValue.NoCacheString);

            string returnMsg = "Hello World";
            string dataformat = "data: {0}\r\n\r\n";
            for (int i = 0; i < returnMsg.Length; i++)
            {
                _logger.LogInformation(string.Format(dataformat, returnMsg[i]));
                await response.WriteAsync(string.Format(dataformat, returnMsg[i]));
                await response.Body.FlushAsync();
                await Task.Yield();
            }

            await response.WriteAsync("event: finish\r\n");
            await response.WriteAsync(string.Format(dataformat, string.Empty));
            await response.Body.FlushAsync();
            return new EmptyResult();
        }
    }
}
