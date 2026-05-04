using MediatR;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Security;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Chat.Commands.SendMessage
{
    [Authorize]
    public class SendMessageCommand : IRequest<SendMessageResponseDto>
    {
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentContentType { get; set; }
        public MessageType Type { get; set; }
    }
}
