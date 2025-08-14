using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alequeshow.Habitica.Webhooks
{
    public class TaskEventWebHook(
        ILogger<TaskEventWebHook> logger,
        ITaskService taskService)
    {
        [Function("TaskEventWebHook")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request)
        {
            try
            {
                var requestContent = await request.ToRequestContent();

                logger.LogInformation("TaskEventWebHook received request: {RequestBody}", requestContent.ToString());

                if(requestContent.Body != null)
                {
                    await taskService.HandleTaskActivityAsync(requestContent.Body);                    
                }
            }
            catch (Exception ex)
            {
                var payload = request.ReadFromJsonAsync<string>();
                logger.LogError(ex, "Error while processing request. {Payload} |", payload);
            }

            return new OkObjectResult("OK");
        }
    }
}
