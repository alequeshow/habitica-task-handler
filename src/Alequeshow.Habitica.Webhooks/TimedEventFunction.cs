using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Alequeshow.Habitica.Webhooks
{
    public class TimedEventFunction(
        ILogger<TimedEventFunction> logger,
        ITaskService taskService)
    {
        private readonly ILogger<TimedEventFunction> logger = logger;

        [Function("TimedEventFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
        {
            logger.LogInformation("Timed event function executed at: {Now}. Next execution: {next}", DateTime.UtcNow, timer.ScheduleStatus?.Next);
            
            try
            {
                await taskService.HandleCronAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing timed event function.");
            }            
        }
    }
}