using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace AoaiStreamFunction
{
    public class Chatstream(OpenAIClient client, IOptions<OpenAIOptions> options)
    {
        [FunctionName("chat-stream")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ChatHistory>(requestBody);
            var chatOptions = new ChatCompletionsOptions(
                options.Value.DeployName, 
                ConvertToChatRequestMessage(data)
            );

            var response = req.HttpContext.Response;
            response.Headers.Add(HeaderNames.ContentType, "text/event-stream");
            response.Headers.Add(HeaderNames.CacheControl, CacheControlHeaderValue.NoCacheString);
            string dataformat = "data: {0}\r\n\r\n";
            await foreach (StreamingChatCompletionsUpdate chatUpdate in client.GetChatCompletionsStreaming(chatOptions))
            {
                log.LogInformation(chatUpdate.ContentUpdate);
                await response.WriteAsync(string.Format(dataformat, chatUpdate.ContentUpdate));
                await response.Body.FlushAsync();
            }

            await response.WriteAsync("event: finish\r\n");
            await response.WriteAsync(string.Format(dataformat, string.Empty));
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
