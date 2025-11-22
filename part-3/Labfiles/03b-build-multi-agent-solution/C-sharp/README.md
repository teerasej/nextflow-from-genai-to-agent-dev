# Multi-Agent Ticket Triage Solution - C# Implementation

This project demonstrates a multi-agent solution using Azure AI Foundry Agent Service. The solution orchestrates multiple specialized agents to collaboratively triage support tickets.

## Overview

The application creates four AI agents:
1. **Priority Agent** - Assesses ticket urgency (High/Medium/Low)
2. **Team Agent** - Determines which team should handle the ticket (Frontend/Backend/Infrastructure/Marketing)
3. **Effort Agent** - Estimates work required (Small/Medium/Large)
4. **Triage Agent** - Orchestrates the other three agents using Connected Agent tools

## Prerequisites

- .NET 8.0 SDK
- Azure subscription
- Azure AI Foundry project with a deployed GPT-4o model
- Azure CLI (for authentication)

## Configuration

1. Update `appsettings.json` with your Azure AI Foundry project details:
   ```json
   {
     "PROJECT_ENDPOINT": "your_project_endpoint",
     "MODEL_DEPLOYMENT_NAME": "gpt-4o"
   }
   ```

2. Sign in to Azure CLI:
   ```bash
   az login
   ```

## Running the Application

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. When prompted, enter a support ticket description. For example:
   - "Users can't reset their password from the mobile app."
   - "Investigate occasional 502 errors from the search endpoint."
   - "The homepage banner image is slightly misaligned on tablets."

## How It Works

1. The application creates three specialized agents (priority, team, effort)
2. Each specialized agent is wrapped in a `ConnectedAgentToolDefinition`
3. A main triage agent is created with the connected agents as tools
4. When you submit a ticket, the triage agent:
   - Analyzes the ticket description
   - Delegates tasks to the appropriate specialized agents
   - Compiles the results into a comprehensive triage assessment

## Key Components

### PersistentAgentsClient
Connects to the Azure AI Foundry project using DefaultAzureCredential.

### ConnectedAgentToolDefinition
Wraps specialized agents so they can be used as tools by the main agent.

### Agent Instructions
Each agent has specific instructions that define its role and output format.

## Output

The application displays:
- Progress messages as agents are created
- The final triage result with priority, team assignment, and effort estimate
- A conversation log showing all interactions between agents

## Cleanup

The application automatically deletes all created agents and threads when finished.

## Troubleshooting

- **Rate limit exceeded**: Wait a few seconds and try again
- **Quota issues**: Ensure your subscription has available quota for GPT-4o
- **Authentication errors**: Verify you're signed in with `az login`

## Learn More

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-foundry/)
- [Connected Agents Guide](https://learn.microsoft.com/azure/ai-foundry/agents/how-to/connected-agents)
- [Azure.AI.Agents.Persistent SDK](https://learn.microsoft.com/dotnet/api/overview/azure/ai.agents.persistent-readme)
