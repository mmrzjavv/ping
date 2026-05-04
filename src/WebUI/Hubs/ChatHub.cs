using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TwitterClone.Application.Chat.Commands.MarkMessageSeen;
using TwitterClone.Application.Chat.Commands.SendMessage;
using TwitterClone.Domain.Enums;

namespace TwitterClone.WebUI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ISender _mediator;

        public ChatHub(ISender mediator)
        {
            _mediator = mediator;
        }

        public Task JoinConversation(int conversationId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));
        }

        public Task LeaveConversation(int conversationId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));
        }

        public Task Typing(int conversationId)
        {
            var userId = Context.User?.Claims?.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value;
            return Clients.OthersInGroup(GetConversationGroup(conversationId)).SendAsync("Typing", new { conversationId, userId });
        }

        public async Task SendMessageAsync(int conversationId, string content)
        {
            await _mediator.Send(new SendMessageCommand
            {
                ConversationId = conversationId,
                Content = content,
                Type = MessageType.Text
            });
        }

        public async Task SendAudioAsync(int conversationId, string attachmentFileName, string contentType)
        {
            await _mediator.Send(new SendMessageCommand
            {
                ConversationId = conversationId,
                AttachmentFileName = attachmentFileName,
                AttachmentContentType = contentType,
                Type = MessageType.Audio
            });
        }

        public async Task SendImageAsync(int conversationId, string attachmentFileName, string contentType, string content = null)
        {
            await _mediator.Send(new SendMessageCommand
            {
                ConversationId = conversationId,
                AttachmentFileName = attachmentFileName,
                AttachmentContentType = contentType,
                Content = content,
                Type = MessageType.Image
            });
        }

        public async Task Seen(int messageId)
        {
            await _mediator.Send(new MarkMessageSeenCommand { MessageId = messageId });
        }

        public static string GetConversationGroup(int conversationId) => $"conversation-{conversationId}";
    }
}
