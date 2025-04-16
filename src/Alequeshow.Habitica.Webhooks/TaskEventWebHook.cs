using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alequeshow.Habitica.Webhooks
{
    public class TaskEventWebHook
    {
        private readonly ILogger<TaskEventWebHook> _logger;

        public TaskEventWebHook(ILogger<TaskEventWebHook> logger)
        {
            _logger = logger;
        }

        [Function("TaskEventWebHook")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
