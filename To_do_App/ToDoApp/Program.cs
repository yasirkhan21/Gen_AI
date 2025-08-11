using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;
internal class Program
{
    private static async Task Main(string[] args)
    {
        // Set up configuration
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string apiKey = config["OpenAI:ApiKey"];
        var builder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                "gpt-4o-mini",
                apiKey // use the value from appsettings.json
            );

        builder.Plugins.AddFromType<TaskPlugin>("ToDo");

        var kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };
        var history = new ChatHistory();

        while (true)
        {
            Console.Write("\nWhat do you need help with? ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
            {
                Console.WriteLine("👋 Exiting...");
                break;
            }

            history.AddUserMessage(userInput);

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );

            Console.WriteLine("\nAssistant > " + result);

            history.AddAssistantMessage(result.Content);
        }
    }
}


