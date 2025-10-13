using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QD.ERP.Shared.Service;

namespace QD.ERP.Shared.Service
{
    public class OpenAIChatService
    {
        private readonly ChatClient _chatClient;

        // ✅ Add this to fix '_sessions' not found error
        private readonly Dictionary<string, List<ChatMessage>> _sessions = new();

        public OpenAIChatService(string endpointUrl, string deploymentName, string apiKey)
        {
            var endpoint = new Uri(endpointUrl);
            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
            _chatClient = azureClient.GetChatClient(deploymentName);
        }

        // ✅ For one-shot (optional if you don't use it)
        public string SendMessage(string sessionId, string userInput)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                _sessions[sessionId] = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful assistant.")
                };
            }

            var messages = _sessions[sessionId];
            messages.Add(new UserChatMessage(userInput));
            var response = _chatClient.CompleteChat(messages);
            var reply = response.Value.Content[0].Text;
            messages.Add(new AssistantChatMessage(reply));
            return reply;
        }

        // ✅ Streaming version used by your controller
        public async Task StreamMessageAsync(string sessionId, string userInput, Func<string, Task> streamCallback)
{
    if (!_sessions.ContainsKey(sessionId))
    {
        _sessions[sessionId] = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant. Please respond using Markdown formatting — headings, bold, links, lists, and proper spacing.")
        };
    }

    var messages = _sessions[sessionId];
    messages.Add(new UserChatMessage(userInput));

    var response = _chatClient.CompleteChatStreaming(messages);

    string fullReply = "";
    string buffer = "";

     foreach (var update in response)
    {
        foreach (var part in update.ContentUpdate)
        {
            fullReply += part.Text;
            buffer += part.Text;

            // ✅ Only stream when a full word is ready (based on space or line break)
            while (buffer.Contains(" ") || buffer.Contains("\n"))
            {
                int spaceIndex = buffer.IndexOfAny(new[] { ' ', '\n' });

                if (spaceIndex == -1)
                    break;

                var word = buffer.Substring(0, spaceIndex + 1); // keep space or newline
                buffer = buffer.Substring(spaceIndex + 1);

                await streamCallback(word); // ✅ send with spacing preserved
            }
        }
    }

    // Flush remaining buffer
    if (!string.IsNullOrWhiteSpace(buffer))
    {
        await streamCallback(buffer);
    }

    messages.Add(new AssistantChatMessage(fullReply));
}
    }
}