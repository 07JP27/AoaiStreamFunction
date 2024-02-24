using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace AoaiStreamFunction
{
    public class Chatstream(OpenAIClient client, IOptions<OpenAIOptions> options, ILogger<Chatstream> _logger)
    {
        [Function("chat-stream")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ChatHistory>(requestBody);
            var chatOptions = new ChatCompletionsOptions(
                options.Value.DeployName, 
                this.ConvertToChatRequestMessage(data)
            );

            var response = req.HttpContext.Response;
            response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
            response.Headers.Append(HeaderNames.CacheControl, CacheControlHeaderValue.NoCacheString);
            string dataformat = "data: {0}\r\n\r\n";
            await foreach (StreamingChatCompletionsUpdate chatUpdate in await client.GetChatCompletionsStreamingAsync(chatOptions))
            {
                _logger.LogInformation(chatUpdate.ContentUpdate);
                await response.WriteAsync(string.Format(dataformat, chatUpdate.ContentUpdate));
                await response.Body.FlushAsync();
                await Task.Delay(10);
            }

            await response.WriteAsync(string.Format(dataformat, "[DONE]"));
            await response.Body.FlushAsync();
            return new EmptyResult();
        }

        private IEnumerable<ChatRequestMessage> ConvertToChatRequestMessage(ChatHistory data)
        {
            var messages = new List<ChatRequestMessage>();
            foreach (var message in data.Messages)
            {
                if (message.Role == "user")
                {
                    messages.Add(new ChatRequestUserMessage(message.Content));
                }
                else if (message.Role == "assistant")
                {
                    messages.Add(new ChatRequestAssistantMessage(message.Content));
                }
                else
                {
                    messages.Add(new ChatRequestSystemMessage(message.Content));
                }
            }
            return messages;
        }
    }
}
