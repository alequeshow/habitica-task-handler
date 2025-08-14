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
        public async Task Run([TimerTrigger("%TIMED_FUNCTION_CRON%")] TimerInfo timer)
        {
            logger.LogInformation("TimedEventFunction started at {ExecutionTime} with next execution at {NextExecution}", 
                DateTime.UtcNow, timer.ScheduleStatus?.Next);
            
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