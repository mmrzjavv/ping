using System.Collections.Generic;
using MediatR;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Security;

namespace TwitterClone.Application.Chat.Queries.GetConversationMessages
{
    [Authorize]
    public class GetConversationMessagesQuery : IRequest<IEnumerable<ChatMessageDto>>
    {
        public int ConversationId { get; set; }
    }
}
