using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        // var kernel = Kernel
        //     .CreateBuilder()
        //     .AddOpenAIChatCompletion(
        //         "gpt-4o-mini",
        //         "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"
        //     )
        //     .Build();
        var builder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                "gpt-4o-mini",
                "API_KEY_HERE"
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
// const string prompt =
//     @"
// You are an AI-powered to-do assistant. Based on the user's input, generate a simple to-do list.
// Return the tasks as a numbered list.
// ---
// User request: {{$input}}
// ";

// var todoFunction = kernel.CreateFunctionFromPrompt(prompt);

// Console.Write("What do you need help with? ");
// var userInput = Console.ReadLine();

// var result = await todoFunction.InvokeAsync(kernel, new KernelArguments { ["input"] = userInput });

// Console.WriteLine("\nHere is your to-do list:\n" + result);
