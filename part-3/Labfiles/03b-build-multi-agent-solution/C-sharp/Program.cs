using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;

// Clear the console
Console.Clear();

// Load configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var projectEndpoint = configuration["PROJECT_ENDPOINT"];
var modelDeployment = configuration["MODEL_DEPLOYMENT_NAME"];

if (string.IsNullOrEmpty(projectEndpoint) || string.IsNullOrEmpty(modelDeployment))
{
    Console.WriteLine("Please set PROJECT_ENDPOINT and MODEL_DEPLOYMENT_NAME in appsettings.json");
    return;
}

// Connect to the agents client
PersistentAgentsClient agentsClient = new(projectEndpoint, new DefaultAzureCredential());

// Create an agent to prioritize support tickets
string priorityAgentName = "priority_agent";
string priorityAgentInstructions = @"
Assess how urgent a ticket is based on its description.

Respond with one of the following levels:
- High: User-facing or blocking issues
- Medium: Time-sensitive but not breaking anything
- Low: Cosmetic or non-urgent tasks

Only output the urgency level and a very brief explanation.
";

PersistentAgent priorityAgent = await agentsClient.Administration.CreateAgentAsync(
    model: modelDeployment!,
    name: priorityAgentName,
    instructions: priorityAgentInstructions
);
Console.WriteLine($"Created priority agent: {priorityAgent.Name}");

// Create an agent to assign tickets to the appropriate team
string teamAgentName = "team_agent";
string teamAgentInstructions = @"
Decide which team should own each ticket.

Choose from the following teams:
- Frontend
- Backend
- Infrastructure
- Marketing

Base your answer on the content of the ticket. Respond with the team name and a very brief explanation.
";

PersistentAgent teamAgent = await agentsClient.Administration.CreateAgentAsync(
    model: modelDeployment!,
    name: teamAgentName,
    instructions: teamAgentInstructions
);
Console.WriteLine($"Created team agent: {teamAgent.Name}");

// Create an agent to estimate effort for a support ticket
string effortAgentName = "effort_agent";
string effortAgentInstructions = @"
Estimate how much work each ticket will require.

Use the following scale:
- Small: Can be completed in a day
- Medium: 2-3 days of work
- Large: Multi-day or cross-team effort

Base your estimate on the complexity implied by the ticket. Respond with the effort level and a brief justification.
";

PersistentAgent effortAgent = await agentsClient.Administration.CreateAgentAsync(
    model: modelDeployment!,
    name: effortAgentName,
    instructions: effortAgentInstructions
);
Console.WriteLine($"Created effort agent: {effortAgent.Name}");

// Create connected agent tools for the support agents
ConnectedAgentToolDefinition priorityAgentTool = new(
    new ConnectedAgentDetails(
        priorityAgent.Id,
        priorityAgent.Name,
        "Assess the priority of a ticket"
    )
);

ConnectedAgentToolDefinition teamAgentTool = new(
    new ConnectedAgentDetails(
        teamAgent.Id,
        teamAgent.Name,
        "Determines which team should take the ticket"
    )
);

ConnectedAgentToolDefinition effortAgentTool = new(
    new ConnectedAgentDetails(
        effortAgent.Id,
        effortAgent.Name,
        "Determines the effort required to complete the ticket"
    )
);
Console.WriteLine("Created connected agent tools");

// Create an agent to triage support ticket processing by using connected agents
string triageAgentName = "triage-agent";
string triageAgentInstructions = @"
Triage the given ticket. Use the connected tools to determine the ticket's priority, 
which team it should be assigned to, and how much effort it may take.
";

PersistentAgent triageAgent = await agentsClient.Administration.CreateAgentAsync(
    model: modelDeployment!,
    name: triageAgentName,
    instructions: triageAgentInstructions,
    tools: new List<ToolDefinition>
    {
        priorityAgentTool,
        teamAgentTool,
        effortAgentTool
    }
);
Console.WriteLine($"Created triage agent: {triageAgent.Name}");

// Use the agents to triage a support issue
Console.WriteLine("Creating agent thread.");
PersistentAgentThread thread = await agentsClient.Threads.CreateThreadAsync();

// Create the ticket prompt
Console.Write("\nWhat's the support problem you need to resolve?: ");
string? prompt = Console.ReadLine();

if (string.IsNullOrEmpty(prompt))
{
    Console.WriteLine("No prompt provided. Exiting.");
    return;
}

// Send a prompt to the agent
PersistentThreadMessage message = await agentsClient.Messages.CreateMessageAsync(
    threadId: thread.Id,
    role: MessageRole.User,
    content: prompt
);

// Run the thread using the primary agent
ThreadRun run = await agentsClient.Runs.CreateRunAsync(
    thread.Id,
    triageAgent.Id
);

// Wait for the run to complete
do
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    run = await agentsClient.Runs.GetRunAsync(thread.Id, run.Id);
}
while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress);

// Check the run status for failures
if (run.Status == RunStatus.Failed)
{
    Console.WriteLine($"Run failed: {run.LastError?.Message}");
}
else
{
    // Show the latest response from the agent
    List<PersistentThreadMessage> messages = await agentsClient.Messages.GetMessagesAsync(
        threadId: thread.Id,
        order: ListSortOrder.Descending
    ).ToListAsync();

    var lastMessage = messages.FirstOrDefault(m => m.Role == MessageRole.Agent);

    if (lastMessage != null)
    {
        var content = lastMessage.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
        if (content != null)
        {
            Console.WriteLine($"\nTriage Result:\n{content.Text}");
        }
    }

    // Get the conversation history
    Console.WriteLine("\n\nConversation Log:\n");
    var allMessages = await agentsClient.Messages.GetMessagesAsync(
        threadId: thread.Id,
        order: ListSortOrder.Ascending
    ).ToListAsync();

    foreach (PersistentThreadMessage threadMessage in allMessages)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
        foreach (MessageContent contentItem in threadMessage.ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.Write(textItem.Text);
            }
        }
        Console.WriteLine();
    }
}

// Clean up
Console.WriteLine("\nCleaning up resources...");
await agentsClient.Threads.DeleteThreadAsync(threadId: thread.Id);
await agentsClient.Administration.DeleteAgentAsync(agentId: triageAgent.Id);
await agentsClient.Administration.DeleteAgentAsync(agentId: priorityAgent.Id);
await agentsClient.Administration.DeleteAgentAsync(agentId: teamAgent.Id);
await agentsClient.Administration.DeleteAgentAsync(agentId: effortAgent.Id);
Console.WriteLine("Resources cleaned up successfully.");

