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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request)
        {
            try
            {
                var requestContent = await request.ToRequestContent();
                
                _logger.LogInformation("Received request: {RequestBody}", requestContent.ToString());
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, "Error while processing request");

            }

            return new OkObjectResult("OK");
        }
    }
}
