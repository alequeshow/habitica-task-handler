using Alequeshow.Habitica.Webhooks;
using Alequeshow.Habitica.Webhooks.Service;
using Alequeshow.Habitica.Webhooks.Service.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Configuration
    .AddUserSecrets<Program>();

var configuration = builder.Configuration;

builder.Services.Configure<TaskServiceOptions>(options =>
{
    options.CompareDueTaskToYesterday = bool.Parse(configuration["DUE_TASK_COMPARE_YESTERDAY"] ?? "false");
    options.SnoozeableTagId = configuration["HABITICA_SNOOZE_TAG_ID"];
});

builder.Services.AddSingleton<ITaskService, TaskService>();
builder.Services.AddSingleton<IHabiticaApiService, HabiticaApiService>();

builder.Services.AddRefitClient<IHabiticaApiClient>()
    .ConfigureHttpClient(httpClient => 
    {
        httpClient.BaseAddress = new Uri(configuration["HABITICA_URL"]!);
        httpClient.DefaultRequestHeaders.Add("x-client", $"{configuration["HABITICA_USER_ID"]}-task-snoozer");
        httpClient.DefaultRequestHeaders.Add("x-api-user", configuration["HABITICA_USER_ID"]);
        httpClient.DefaultRequestHeaders.Add("x-api-key", configuration["HABITICA_USER_TOKEN"]);
    });

builder.Build().Run();
