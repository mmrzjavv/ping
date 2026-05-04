using System.Collections.Generic;
using MediatR;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Security;

namespace TwitterClone.Application.Chat.Queries.GetConversations
{
    [Authorize]
    public class GetConversationsQuery : IRequest<IEnumerable<ConversationDto>>
    {
    }
}
