using System.Text.Json;
using Alequeshow.Habitica.Webhooks.Domain;
using Microsoft.AspNetCore.Http;

namespace Alequeshow.Habitica.Webhooks
{
    public class RequestContent
    {
        public Dictionary<string, string> Headers { get; set; } = [];

        public Dictionary<string, string> Parameters { get; set; } = [];

        public TaskActivityEvent? Body { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public static class RequestExtensions
    {
        public static async Task<RequestContent> ToRequestContent(this HttpRequest request)
        {
            var requestContent = new RequestContent
            {
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Parameters = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString())
            };

            var bodyString = await new StreamReader(request.Body).ReadToEndAsync();
            requestContent.Body = JsonSerializer.Deserialize<TaskActivityEvent>(bodyString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return requestContent;
        }
    }
}