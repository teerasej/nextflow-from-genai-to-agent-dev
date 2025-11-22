---
lab:
    title: 'Develop a multi-agent solution with Azure AI Foundry'
    description: 'Learn to configure multiple agents to collaborate using Azure AI Foundry Agent Service'
---

# Develop a multi-agent solution

In this exercise, you'll create a project that orchestrates multiple AI agents using Azure AI Foundry Agent Service. You'll design an AI solution that assists with ticket triage. The connected agents will assess the ticket's priority, suggest a team assignment, and determine the level of effort required to complete the ticket. Let's get started!

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

## Create an AI Agent client app

Now you're ready to create a client app that defines the agents and instructions. Some code is provided for you in a GitHub repository.

### Prepare the environment

1. In the terminal, navigate or enter the following command to change the working directory to the folder containing the code files and list them all.

    ```
   cd ai-agents/Labfiles/03b-build-multi-agent-solution/C-sharp
   ls -a -l
    ```

    The provided files include application code and a file for configuration settings.

### Configure the application settings

1. Enter the following command to edit the configuration file that is provided:

    ```
   code appsettings.json
    ```

    The file is opened in a code editor.

1. In the code file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Azure AI Foundry portal), and ensure that the **MODEL_DEPLOYMENT_NAME** variable is set to your model deployment name (which should be *gpt-4o*).

1. After you've replaced the placeholder, use the **CTRL+S** command to save your changes and then use the **CTRL+Q** command to close the code editor while keeping the cloud shell command line open.

### Create AI agents

Now you're ready to create the agents for your multi-agent solution! Let's get started!

1. Enter the following command to edit the **Program.cs** file:

    ```
   code Program.cs
    ```

1. Review the code in the file, noting that it contains strings for each agent name and instructions.

1. Find the comment **Add references** and add the following code to import the classes you'll need:

    ```csharp
   // Add references
   using Azure.AI.Agents.Persistent;
    ```

1. Note that code to load the project endpoint and model name from your configuration settings has been provided.

1. Find the comment **Connect to the agents client**, and add the following code to create a PersistentAgentsClient connected to your project:

    ```csharp
   // Connect to the agents client
   PersistentAgentsClient agentsClient = new(projectEndpoint, new DefaultAzureCredential());
    ```

    The code connects to the Azure AI Foundry project using the current Azure credentials.

1. Find the comment **Create an agent to prioritize support tickets**, and enter the following code (being careful to retain the right level of indentation):

    ```csharp
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
    ```

1. Find the comment **Create an agent to assign tickets to the appropriate team**, and enter the following code:

    ```csharp
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
    ```

1. Find the comment **Create an agent to estimate effort for a support ticket**, and enter the following code:

    ```csharp
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
    ```

    So far, you've created three agents; each of which has a specific role in triaging a support ticket. Now let's create ConnectedAgentToolDefinition objects for each of these agents so they can be used by other agents.

1. Find the comment **Create connected agent tools for the support agents**, and enter the following code:

    ```csharp
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
    ```

    Now you're ready to create a primary agent that will coordinate the ticket triage process, using the connected agents as required.

1. Find the comment **Create an agent to triage support ticket processing by using connected agents**, and enter the following code:

    ```csharp
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
    ```

    Now that you have defined a primary agent, you can submit a prompt to it and have it use the other agents to triage a support issue.

1. Find the comment **Use the agents to triage a support issue**, and enter the following code:

    ```csharp
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
    ```

1. Find the comment **Clean up**, and enter the following code to delete the agents when they are no longer required:

    ```csharp
   // Clean up
   Console.WriteLine("\nCleaning up resources...");
   await agentsClient.Threads.DeleteThreadAsync(threadId: thread.Id);
   await agentsClient.Administration.DeleteAgentAsync(agentId: triageAgent.Id);
   await agentsClient.Administration.DeleteAgentAsync(agentId: priorityAgent.Id);
   await agentsClient.Administration.DeleteAgentAsync(agentId: teamAgent.Id);
   await agentsClient.Administration.DeleteAgentAsync(agentId: effortAgent.Id);
   Console.WriteLine("Resources cleaned up successfully.");
    ```

1. Use the **CTRL+S** command to save your changes to the code file. You can keep it open (in case you need to edit the code to fix any errors) or use the **CTRL+Q** command to close the code editor while keeping the cloud shell command line open.

### Sign into Azure and run the app

Now you're ready to run your code and watch your AI agents collaborate.

1. In the cloud shell command-line pane, enter the following command to sign into Azure.

    ```
    az login --use-device-code
    ```

    **<font color="red">You must sign into Azure - even though the cloud shell session is already authenticated.</font>**

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.

1. When prompted, follow the instructions to open the sign-in page in a new tab and enter the authentication code provided and your Azure credentials. Then complete the sign in process in the command line, selecting the subscription containing your Azure AI Foundry hub if prompted.

1. After you have signed in, enter the following command to run the application:

    ```
    dotnet run
    ```

    The application runs using the credentials for your authenticated Azure session to connect to your project and create and run the agents.

1. When prompted, enter a support ticket description such as:

    ```
   Users can't reset their password from the mobile app.
    ```

    > **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

1. View the response, which should include the triage assessment from the three specialized agents (priority, team, and effort). The conversation log shows the interaction between the main triage agent and the connected agents.

1. You can run the application multiple times with different ticket scenarios to see how the agents collaborate. For example:

    ```
   Investigate occasional 502 errors from the search endpoint.
    ```

    or

    ```
   The homepage banner image is slightly misaligned on tablets.
    ```

## Clean up

If you've finished exploring Azure AI Agent Service, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs.

1. Return to the browser tab containing the Azure portal (or re-open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` in a new browser tab) and view the contents of the resource group where you deployed the resources used in this exercise.

1. On the toolbar, select **Delete resource group**.

1. Enter the resource group name and confirm that you want to delete it.
