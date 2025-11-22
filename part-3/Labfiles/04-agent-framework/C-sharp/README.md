# Agent Framework Exercise

This exercise demonstrates how to use the Microsoft Agent Framework SDK to create an AI agent that processes expense claims.

## Prerequisites

- .NET 10.0 SDK or later
- Azure OpenAI service endpoint and deployment configured
- Azure CLI installed and authenticated

## Configuration

1. Update `appsettings.json` with your Azure OpenAI endpoint and model deployment name
2. Sign into Azure using `az login`

## Running the Application

```bash
dotnet build
dotnet run
```

## What the Agent Does

The agent processes expenses data from `data.txt` and can perform actions such as:
- Submitting expense claims
- Sending emails (simulated) with itemized expenses

The agent uses a custom function tool to simulate sending emails to expenses@contoso.com.
