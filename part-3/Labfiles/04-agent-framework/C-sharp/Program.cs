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


async Task ProcessExpensesData(string prompt, string expensesData, string endpoint, string deployment)
{
    // Create a chat agent
    

}
