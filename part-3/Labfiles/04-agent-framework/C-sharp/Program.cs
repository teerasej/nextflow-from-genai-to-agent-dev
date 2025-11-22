using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

// Clear the console
Console.Clear();

// Load configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var azureOpenAIEndpoint = configuration["AZURE_OPENAI_ENDPOINT"];
var modelDeployment = configuration["MODEL_DEPLOYMENT_NAME"];

if (string.IsNullOrEmpty(azureOpenAIEndpoint) || string.IsNullOrEmpty(modelDeployment))
{
    Console.WriteLine("Please set AZURE_OPENAI_ENDPOINT and MODEL_DEPLOYMENT_NAME in appsettings.json");
    return;
}

// Load the expenses data file
var expensesData = await File.ReadAllTextAsync("data.txt");

// Ask for a prompt
Console.WriteLine($"Here is the expenses data in your file:\n\n{expensesData}\n");
Console.Write("What would you like me to do with it?\n\n");
string? userPrompt = Console.ReadLine();

if (string.IsNullOrEmpty(userPrompt))
{
    Console.WriteLine("No prompt provided. Exiting.");
    return;
}

// Process the expenses data
await ProcessExpensesData(userPrompt, expensesData, azureOpenAIEndpoint, modelDeployment);

// Create a tool function for the email functionality
[Description("Send an email with a subject and body to a recipient.")]
    static void SendEmail(
        [Description("The email address to send the email to.")] string to,
        [Description("The subject of the email.")] string subject,
        [Description("The body content of the email.")] string body)
    {     
        Console.WriteLine($"\nTo: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"{body}\n");
    }

async Task ProcessExpensesData(string prompt, string expensesData, string endpoint, string deployment)
{
    // Create a chat agent
    try
    {
        // Create the Azure OpenAI client
        AzureOpenAIClient client = new(
            new Uri(endpoint),
            new AzureCliCredential()
        );

        // Get the chat client for the model deployment
        var chatClient = client.GetChatClient(deployment);

        // Create the agent with instructions and the SendEmail tool
        AIAgent agent = chatClient.CreateAIAgent(
            instructions: @"You are an AI assistant for expense claim submission.
                           When a user submits expenses data and requests an expense claim, use the SendEmail function to send an email to expenses@contoso.com with the subject 'Expense Claim' and a body that contains itemized expenses with a total.
                           Then confirm to the user that you've done so.",
            name: "expenses_agent",
            tools: [AIFunctionFactory.Create(SendEmail)]
        );

        // Invoke the agent with the user's prompt and expenses data
        string fullPrompt = $"{prompt}: {expensesData}";
        AgentRunResponse response = await agent.RunAsync(fullPrompt);

        // Display the agent's response
        Console.WriteLine($"\n# Agent:\n{response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

}
