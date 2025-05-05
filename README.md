# Habitica Task Handlers

This project contains Azure Functions that interact with Habitica's task management system. It includes an HTTP-triggered function (webhook) for handling task events and a timer-triggered function for scheduled operations.

In order to use this repo, you'll need:

- Some dotnet development knowledge
- Azure account (even a free one is enough) and knowledge to deploy and host your azure function instance
- Knowledge of how [Habitica API](https://habitica.com/apidoc/) works.

## Project Structure

- **src/Alequeshow.Habitica.Webhooks**
  - **TaskEventWebHook.cs**: Contains the `TaskEventWebHook` class, which defines an Azure Function that processes HTTP requests related to task events.
  - **TimedEventFunction.cs**: Defines a new Azure Function that operates as a timed event, executing code at specified intervals.
  - **Service/Interfaces/ITaskService.cs**: Defines the `ITaskService` interface for task-related operations.

- **host.json**: Configuration settings for the Azure Functions host, including timeout settings and logging configurations.

- **local.settings.json**: Local development settings, including connection strings and application settings. Not versioned (handle your own for security reasons 😉)

## Setup Instructions

1. Clone the repository to your local machine.
2. Navigate to the project directory.
3. Install the necessary dependencies.
   - [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools): Required to run and debug Azure Functions locally.
   - Install [azurate](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage) to run timed-trigger events
4. Configure your local.settings.json with the required settings for local development. Required environment variables to be added:
   - `HABITICA_URL`: The base URL for the habitica environment. Use `https://habitica.com` for production or any other for testing purposes
   - `HABITICA_USER_ID`: User ID Key you can acquire your from `https://habitica.com/user/settings/siteData` Site Data/User section
   - `HABITICA_USER_TOKEN`: User token value you can get from `https://habitica.com/user/settings/siteData` Site Data/API section
   - `HABITICA_SNOOZE_TAG_ID`: The id of the tag you defined to the function identifies which daily-tasks to consider to create as to-do when due and not completed
      - See [Habitica API reference](https://habitica.com/apidoc/#api-Tag-GetTags)   
   - We strongly suggest to manage sensitive data into [user-secres](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0&tabs=linux#use-the-cli)
   - `TIMED_FUNCTION_CRON`: Cron function to define the periodicity to trigger the [`TimedEventFunction`](src/Alequeshow.Habitica.Webhooks/TimedEventFunction.cs) event.
      - My task is configured to run every 2AM: `0 0 2 * * *`
   - `DUE_TASK_COMPARE_YESTERDAY` [Optional]: Since I'm in Brazil and there is a timezone difference to UTC (3h), The time I set the function to run is to it runs in my scope at 11PM.
      - This way its needed to consider tasks from the past days instead of today, this flags indicates that.
5. Run the Azure Functions locally to test the functionality.
   - Run azurie and start the function:
```
azurite
cd src/Alequeshow.Habitica.Webhooks
funct start
```
   

## Snooze Task Handler

The main motivation for this project is to give Habitica the ability to create a to-do task for dailies that weren't completed and have a need to follow-up. 

Example:
I have a weekly/montly task that there's no harm if I do it one, two or even few days later. As long as I do it eventually.
Let's say I want to do my personal financial review monthly every 28th day of the month. However in that particular day I was travelling or had a busy day and couldn't even look at it. It's something I can do in the following days if I have the proper tracking.
Because Habitica will hide this task when I review pending tasks in the day after, it'd be nice to have it replicated as a to-do for the next day.

To achieve it, the `TimedEventFunction` can be configured to run in a specific time of the day to ensure eligible due-and-unfinished dailies will be replicated as a to-do task. 
My function is configured to run everyday at 11pm. This time I know I'll hardly do any pending tasks because I'll be probably asleep.

To turn tasks eligible you need to set a proper tag for it, grabs its id and store in the `HABITICA_SNOOZE_TAG_ID` environment variable. My tag I named `SnoozeTask` and set for whatever task I want to be tracked

### Next steps
In future I'll extend the snooze task handler to deal with Habits that had little to no score given its periodiocity (from weekly on just to avoid a chaotic to-do list)
