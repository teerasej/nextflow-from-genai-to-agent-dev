using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Azure.AI.Agents.Persistent;

namespace AgentFunctions
{
    /// <summary>
    /// User-defined functions that can be called by the AI agent
    /// </summary>
    public class UserFunctions
    {
        // Create a function to submit a support ticket
        public string SubmitTicket(string email, string description)
        {
            // Generate a unique ticket number
            string ticketNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            
            // Create ticket data
            var ticket = new
            {
                TicketNumber = ticketNumber,
                Email = email,
                Description = description,
                SubmittedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };
            
            // Save the ticket as a text file
            string fileName = $"ticket_{ticketNumber}.txt";
            string ticketContent = $"Support Ticket: {ticketNumber}\n" +
                                   $"Email: {email}\n" +
                                   $"Description: {description}\n" +
                                   $"Submitted: {ticket.SubmittedAt}\n";
            
            File.WriteAllText(fileName, ticketContent);
            Console.WriteLine($"Created support ticket file: {fileName}");
            
            return $"Support ticket {ticketNumber} has been created successfully.";
        }

        // Define a set of callable functions
        public FunctionToolDefinition GetSubmitTicketToolDefinition()
        {
            return new FunctionToolDefinition(
                name: "submit_ticket",
                description: "Submits a technical support ticket with user's email and problem description.",
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
            );
        }

        // Helper method to resolve tool calls
        public ToolOutput GetResolvedToolOutput(RequiredToolCall toolCall, FunctionToolDefinition submitTicketTool)
        {
            if (toolCall is RequiredFunctionToolCall functionToolCall)
            {
                if (functionToolCall.Name == submitTicketTool.Name)
                {
                    using JsonDocument argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);
                    string email = argumentsJson.RootElement.GetProperty("email").GetString();
                    string description = argumentsJson.RootElement.GetProperty("description").GetString();
                    
                    string result = SubmitTicket(email, description);
                    return new ToolOutput(toolCall, result);
                }
            }
            return null;
        }
    }
}
