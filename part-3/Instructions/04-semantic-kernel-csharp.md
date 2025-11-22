---
lab:
    title: 'Develop an Azure AI agent with the Microsoft Agent Framework SDK'
    description: 'Learn how to use the Microsoft Agent Framework SDK to create and use an Azure AI chat agent.'
---

# Develop an Azure AI chat agent with the Microsoft Agent Framework SDK

In this exercise, you'll use Azure OpenAI and Microsoft Agent Framework to create an AI agent that processes expense claims.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Deploy a model in an Azure OpenAI resource

Let's start by deploying a model in an Azure OpenAI resource.

1. In a web browser, open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` and sign in using your Azure credentials.

1. In the search bar at the top of the page, search for **Azure OpenAI** and select **Azure OpenAI** from the results.

1. Select **Create** to create a new Azure OpenAI resource.

1. On the **Create Azure OpenAI** page, configure the following settings:
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Select any available region*
    - **Name**: *A unique name for your Azure OpenAI resource*
    - **Pricing tier**: Standard S0

1. Select **Next** through the remaining pages, accepting the default settings, and then select **Create**.

1. Wait for the deployment to complete, then go to the deployed Azure OpenAI resource.

1. In the navigation pane on the left, under **Resource Management**, select **Keys and Endpoint**.

1. Copy the **Endpoint** value to a notepad, as you'll use it to connect to your Azure OpenAI resource in a client application.

1. In the navigation pane on the left, under **Resource Management**, select **Model deployments**.

1. Select **Manage Deployments** to open Azure OpenAI Studio in a new tab.

1. In Azure OpenAI Studio, on the **Deployments** page, select **Create new deployment**.

1. Create a new deployment with the following settings:
    - **Model**: gpt-4o
    - **Deployment name**: gpt-4o
    - **Deployment type**: Standard
    - **Model version**: Default
    - **Tokens per minute rate limit**: 10K

1. Wait for the deployment to be created.

## Create an agent client app

Now you're ready to create a client app that defines an agent and a custom function. Some code has been provided for you.

### Prepare the development environment

1. Open Visual Studio Code.

1. Open a terminal and navigate to the folder where you want to create your project:

    ```bash
    cd part-3/Labfiles/04-agent-framework/C-sharp
    ```

1. Review the provided files:
    - **AgentFramework.csproj**: The project file that references the required NuGet packages
    - **appsettings.json**: Configuration file for Azure OpenAI settings
    - **Program.cs**: Application code with placeholders for your agent implementation
    - **data.txt**: Expenses data file

### Configure the application settings

1. Open the **appsettings.json** file in Visual Studio Code.

1. Replace the **your_azure_openai_endpoint** placeholder with the endpoint for your Azure OpenAI resource (copied from the **Keys and Endpoint** page in the Azure portal).

1. Ensure the **MODEL_DEPLOYMENT_NAME** is set to **gpt-4o** (the name of your model deployment).

1. Save your changes.

### Write code for an agent app

Now you're ready to implement the agent functionality!

1. Open the **Program.cs** file in Visual Studio Code.

1. Review the code in the file. It contains:
    - Import statements for required namespaces
    - Code to load configuration and expenses data
    - A prompt to ask the user what to do with the data
    - A **ProcessExpensesData** method where you'll add your agent implementation
    - A placeholder for the email function tool

1. Find the comment **Create a tool function for the email functionality**, and add the following code to define a function that your agent will use to send email:

    ```csharp
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
    ```

    > **Note**: The function *simulates* sending an email by printing it to the console. In a real application, you'd use an SMTP service or similar to actually send the email!

1. In the **ProcessExpensesData** method, find the comment **Create a chat agent**, and add the following code to create an **AIAgent** object with the tool and instructions:

    ```csharp
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
    ```

    The code creates an Azure OpenAI client using Azure CLI credentials, gets a chat client for the specified model deployment, and then creates an agent with custom instructions and the **SendEmail** function tool. The agent is then invoked with the user's prompt and expenses data.

1. Save your code changes.

### Sign into Azure and run the app

1. In the terminal, enter the following command to sign into Azure:

    ```bash
    az login
    ```

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.

1. When prompted, follow the instructions to open the sign-in page in a browser and enter the authentication code provided and your Azure credentials.

1. After you have signed in, return to the terminal and enter the following command to build the application:

    ```bash
    dotnet build
    ```

1. If there are any build errors, review your code and fix them. Then run **dotnet build** again.

1. When the build succeeds, enter the following command to run the application:

    ```bash
    dotnet run
    ```

    The application runs using the credentials for your authenticated Azure session to connect to your Azure OpenAI resource and create and run the agent.

1. When asked what to do with the expenses data, enter the following prompt:

    ```
    Submit an expense claim
    ```

1. When the application has finished, review the output. The agent should have composed an email for an expenses claim based on the data that was provided.

    > **Tip**: If the app fails because the rate limit is exceeded, wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

## Summary

In this exercise, you used the Microsoft Agent Framework SDK to create an agent with a custom function tool that processes expense claims.

## Clean up

If you've finished exploring Azure OpenAI, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs.

1. Return to the browser tab containing the Azure portal and view the contents of the resource group where you deployed the Azure OpenAI resource.

1. On the toolbar, select **Delete resource group**.

1. Enter the resource group name and confirm that you want to delete it.
