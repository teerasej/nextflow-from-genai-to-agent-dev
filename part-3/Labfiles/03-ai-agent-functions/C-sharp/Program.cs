using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AgentFunctions
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Clear the console
            Console.Clear();

            // Load configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var projectEndpoint = configuration["PROJECT_ENDPOINT"];
            var modelDeploymentName = configuration["MODEL_DEPLOYMENT_NAME"];

            Console.WriteLine("Technical Support Agent");
            Console.WriteLine("========================\n");


            // 1. Connect to the Agent client
            PersistentAgentsClient agentClient = new(projectEndpoint, new DefaultAzureCredential());

            // 2. Create an instance of UserFunctions
            UserFunctions userFunctions = new UserFunctions();
            FunctionToolDefinition submitTicketTool = userFunctions.GetSubmitTicketToolDefinition();

            // 3. Define an agent that can use the custom functions
            var agent = await agentClient.Administration.CreateAgentAsync(
                model: modelDeploymentName,
                name: "Technical Support Agent",
                instructions: "You are a helpful technical support agent. " +
                             "Collect the user's email address and a description of their technical problem. " +
                             "Once you have both pieces of information, use the submit_ticket function to create a support ticket. " +
                             "Be friendly and professional in your interactions.",
                tools: new List<ToolDefinition> { submitTicketTool }
            );
            Console.WriteLine($"Created agent: {agent.Value.Name}\n");

            // 4. Create a thread for the conversation
            var thread = await agentClient.Threads.CreateThreadAsync();
            Console.WriteLine($"Created conversation thread\n");

            try
            {
                // Loop until the user types 'quit'
                while (true)
                {
                    // Get input text
                    Console.Write("Enter a prompt (or type 'quit' to exit): ");
                    string userPrompt = Console.ReadLine();

                    if (userPrompt.ToLower() == "quit")
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(userPrompt))
                    {
                        Console.WriteLine("Please enter a prompt.\n");
                        continue;
                    }

                    // Send a prompt to the agent
                    await agentClient.Messages.CreateMessageAsync(
                       threadId: thread.Value.Id,
                        role: MessageRole.User,
                        content: userPrompt
                    );

                    ThreadRun run = await agentClient.Runs.CreateRunAsync(
                        thread.Value.Id,
                        agent.Value.Id
                    );

                    // Wait for the run to complete
                    do
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        run = await agentClient.Runs.GetRunAsync(thread.Value.Id, run.Id);

                        // Check if the run requires action (function calling), 
                        // This method handles function/tool calls the model requests: 
                        // it checks the required tool name, parses the JSON arguments, invokes the corresponding local function, 
                        // and returns a ToolOutput that you submit back to the run. 
                        // Without this glue code the run will stay in RequiresAction and won't complete.
                        if (run.Status == RunStatus.RequiresAction
                            && run.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
                        {
                            List<ToolOutput> toolOutputs = new List<ToolOutput>();
                            foreach (RequiredToolCall toolCall in submitToolOutputsAction.ToolCalls)
                            {
                                ToolOutput output = userFunctions.GetResolvedToolOutput(toolCall, submitTicketTool);
                                if (output != null)
                                {
                                    toolOutputs.Add(output);
                                }
                            }

                            if (toolOutputs.Count > 0)
                            {
                                run = agentClient.Runs.SubmitToolOutputsToRun(run, toolOutputs);
                            }
                        }
                    }
                    while (run.Status == RunStatus.Queued
                        || run.Status == RunStatus.InProgress);

                    // Check the run status for failures
                    if (run.Status == RunStatus.Failed)
                    {
                        Console.WriteLine($"Run failed: {run.LastError?.Message}\n");
                        continue;
                    }

                    // Show the latest response from the agent
                    List<PersistentThreadMessage> messages = await agentClient.Messages.GetMessagesAsync(
                        threadId: thread.Value.Id,
                        order: ListSortOrder.Descending
                    ).ToListAsync();

                    foreach (PersistentThreadMessage message in messages)
                    {
                        if (message.Role == MessageRole.Agent)
                        {
                            foreach (MessageContent contentItem in message.ContentItems)
                            {
                                if (contentItem is MessageTextContent textContent)
                                {
                                    Console.WriteLine($"\nAgent: {textContent.Text}\n");
                                }
                            }
                            break;
                        }
                    }
                }

                // Get the conversation history
                Console.WriteLine("\n=== Conversation History ===\n");
                List<PersistentThreadMessage> allMessages = await agentClient.Messages.GetMessagesAsync(
                    threadId: thread.Value.Id,
                    order: ListSortOrder.Ascending
                ).ToListAsync();

                foreach (PersistentThreadMessage message in allMessages)
                {
                    Console.Write($"[{message.Role}]: ");
                    foreach (MessageContent contentItem in message.ContentItems)
                    {
                        if (contentItem is MessageTextContent textContent)
                        {
                            Console.WriteLine(textContent.Text);
                        }
                    }
                }
            }
            finally
            {
                // Clean up
                Console.WriteLine("\nCleaning up resources...");
                await agentClient.Threads.DeleteThreadAsync(thread.Value.Id);
                await agentClient.Administration.DeleteAgentAsync(agent.Value.Id);
                Console.WriteLine("Done.");
            }
        }
    }
}
