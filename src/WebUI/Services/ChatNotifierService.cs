using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.WebUI.Hubs;

namespace TwitterClone.WebUI.Services
{
    public class ChatNotifierService : IChatNotifierService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatNotifierService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendMessageAsync(int conversationId, ChatMessageDto message)
        {
            return _hubContext.Clients.Group(ChatHub.GetConversationGroup(conversationId)).SendAsync("ReceiveMessage", message);
        }

        public Task TypingAsync(int conversationId, int userId)
        {
            return _hubContext.Clients.Group(ChatHub.GetConversationGroup(conversationId)).SendAsync("Typing", new { conversationId, userId });
        }

        public Task SeenAsync(int conversationId, int messageId, int userId)
        {
            return _hubContext.Clients.Group(ChatHub.GetConversationGroup(conversationId)).SendAsync("Seen", new { conversationId, messageId, userId });
        }
    }
}
