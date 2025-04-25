# Habitica Webhooks

This project contains Azure Functions that interact with Habitica's task management system. It includes an HTTP-triggered function for handling task events and a timer-triggered function for scheduled operations.

## Project Structure

- **src/Alequeshow.Habitica.Webhooks**
  - **TaskEventWebHook.cs**: Contains the `TaskEventWebHook` class, which defines an Azure Function that processes HTTP requests related to task events.
  - **TimedEventFunction.cs**: Defines a new Azure Function that operates as a timed event, executing code at specified intervals.
  - **Service/Interfaces/ITaskService.cs**: Defines the `ITaskService` interface for task-related operations.

- **host.json**: Configuration settings for the Azure Functions host, including timeout settings and logging configurations.

- **local.settings.json**: Local development settings, including connection strings and application settings.

## Setup Instructions

1. Clone the repository to your local machine.
2. Navigate to the project directory.
3. Install the necessary dependencies.
4. Configure your local.settings.json with the required settings for local development.
5. Run the Azure Functions locally to test the functionality.

## Usage

- The `TaskEventWebHook` function can be triggered via HTTP requests to handle task-related events.
- The `TimedEventFunction` will execute at specified intervals, allowing for scheduled tasks or maintenance operations.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.