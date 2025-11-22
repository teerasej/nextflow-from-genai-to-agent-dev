---
lab:
    title: 'Use a custom function in an AI agent (C#)'
    description: 'Learn how to use functions to add custom capabilities to your agents using C# and .NET.'
---

# Use a custom function in an AI agent (C#)

In this exercise you'll explore creating an agent that can use custom functions as a tool to complete tasks. You'll build a simple technical support agent that can collect details of a technical problem and generate a support ticket.

> **Tip**: The code used in this exercise is based on the Azure AI Foundry SDK for .NET. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Azure AI Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Create an Azure AI Foundry project

Let's start by creating an Azure AI Foundry project.

1. In a web browser, open the [Azure AI Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Azure AI Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

    ![Screenshot of Azure AI Foundry portal.](./Media/ai-foundry-home.png)

1. In the home page, select **Create an agent**.
1. When prompted to create a project, enter a valid name for your project and expand **Advanced options**.
1. Confirm the following settings for your project:
    - **Azure AI Foundry resource**: *A valid name for your Azure AI Foundry resource*
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Select any **AI Foundry recommended***\*

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

1. Select **Create** and wait for your project to be created.
1. If prompted, deploy a **gpt-4o** model using either the *Global Standard* or *Standard* deployment option (depending on your quota availability).

    >**Note**: If quota is available, a GPT-4o base model may be deployed automatically when creating your Agent and project.

1. When your project is created, the Agents playground will be opened.

1. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

    ![Screenshot of a Azure AI Foundry project overview page.](./Media/ai-foundry-project.png)

1. Copy the **Azure AI Foundry project endpoint** values to a notepad, as you'll use them to connect to your project in a client application.

## Develop an agent that uses function tools

Now that you've created your project in AI Foundry, let's develop an app that implements an agent using custom function tools.

### Clone the repo containing the application code

1. In the terminal, navigate or enter the following command to change the working directory to the folder containing the code files and list them all.

    ```
   cd ai-agents/Labfiles/03-ai-agent-functions/C-sharp
   ls -a -l
    ```

    The provided files include application code and a file for configuration settings.

### Configure the application settings

1. Enter the following command to edit the configuration file that has been provided:

    ```
   code appsettings.json
    ```

    The file is opened in a code editor.

1. In the code file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Azure AI Foundry portal) and ensure that the MODEL_DEPLOYMENT_NAME variable is set to your model deployment name (which should be *gpt-4o*).
1. After you've replaced the placeholder, use the **CTRL+S** command to save your changes and then use the **CTRL+Q** command to close the code editor while keeping the cloud shell command line open.

### Review the custom function code

1. Enter the following command to review the code file that defines the custom functions:

    ```
   code UserFunctions.cs
    ```

1. Review the code, which includes:
   - A `SubmitTicket` method that creates support tickets
   - A `GetSubmitTicketToolDefinition` method that returns a `FunctionToolDefinition` describing the function to the agent
   - A `GetResolvedToolOutput` helper method that executes the function when called by the agent

1. The `FunctionToolDefinition` uses JSON schema to describe the function's parameters:

    ```csharp
    parameters: BinaryData.FromObjectAsJson(
        new
        {
            Type = "object",
            Properties = new
            {
                Email = new
                {
                    Type = "string",
                    Description = "The user's email address"
                },
                Description = new
                {
                    Type = "string",
                    Description = "A description of the technical problem"
                }
            },
            Required = new[] { "email", "description" }
        },
        new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
    ```

1. Close the code editor (*CTRL+Q*) when you're finished reviewing.

### Write code to implement an agent that can use your function

1. Enter the following command to begin editing the agent code.

    ```
   code Program.cs
    ```

    > **Tip**: As you add code to the code file, be sure to maintain the correct indentation.

1. Review the existing code, which retrieves the application configuration settings and sets up a loop in which the user can enter prompts for the agent. The rest of the file includes comments where you'll add the necessary code to implement your technical support agent.


2. Note that the code already connects to the Azure AI project using the current Azure credentials with these lines:

    ```csharp
    // Connect to the Agent client
    PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());
    ```

3. The code then creates an instance of `UserFunctions` and gets the function tool definition:

    ```csharp
    // Create an instance of UserFunctions
    UserFunctions userFunctions = new UserFunctions();
    FunctionToolDefinition submitTicketTool = userFunctions.GetSubmitTicketToolDefinition();
    ```

4. The agent is then created with the function tool:

    ```csharp
    // Define an agent that can use the custom functions
    PersistentAgent agent = client.Administration.CreateAgent(
        model: modelDeploymentName,
        name: "Technical Support Agent",
        instructions: "You are a helpful technical support agent. " +
                     "Collect the user's email address and a description of their technical problem. " +
                     "Once you have both pieces of information, use the submit_ticket function to create a support ticket. " +
                     "Be friendly and professional in your interactions.",
        tools: new List<ToolDefinition> { submitTicketTool }
    );
    ```

5. Review the main loop section that handles user interaction. Note how the code:
   - Sends user messages to the agent
   - Checks if the run requires action (function calling)
   - Executes the function using `GetResolvedToolOutput` when needed
   - Submits the function results back to the agent
   - Displays the agent's response

6. The key section for handling function calls looks like this:

    ```csharp
    // Check if the run requires action (function calling)
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
            run = client.Runs.SubmitToolOutputsToRun(run, toolOutputs);
        }
    }
    ```

7. Save the code file (*CTRL+S*) when you have finished. You can also close the code editor (*CTRL+Q*); though you may want to keep it open in case you need to make any edits to the code you added. In either case, keep the cloud shell command-line pane open.

### Sign into Azure and run the app

1. In the cloud shell command-line pane, enter the following command to sign into Azure.

    ```
    az login
    ```

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.
    
1. When prompted, follow the instructions to open the sign-in page in a new tab and enter the authentication code provided and your Azure credentials. Then complete the sign in process in the command line, selecting the subscription containing your Azure AI Foundry hub if prompted.

1. After you have signed in, enter the following command to run the application:

    ```
   dotnet run
    ```

    The application runs using the credentials for your authenticated Azure session to connect to your project and create and run the agent.

1. When prompted, enter a prompt such as:

    ```
    I need help with my computer
    ```

1. View the response. The agent may ask for your email address and a description of the issue. You can use any email address (for example, `alex@contoso.com`) and any issue description (for example `my computer won't start`)

    When it has enough information, the agent should choose to use your function as required.

1. You can continue the conversation if you like. The thread is *stateful*, so it retains the conversation history - meaning that the agent has the full context for each response. Enter `quit` when you're done.

1. Review the conversation messages that were retrieved from the thread, and the tickets that were generated.

1. The tool should have saved support tickets in the app folder. You can use the `ls` command to check, and then use the `cat` command to view the file contents, like this:

    ```
    ls ticket_*.txt
    cat ticket_XXXXXXXX.txt
    ```

## Clean up

Now that you've finished the exercise, you should delete the cloud resources you've created to avoid unnecessary resource usage.

1. Open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` and view the contents of the resource group where you deployed the hub resources used in this exercise.
1. On the toolbar, select **Delete resource group**.
1. Enter the resource group name and confirm that you want to delete it.
