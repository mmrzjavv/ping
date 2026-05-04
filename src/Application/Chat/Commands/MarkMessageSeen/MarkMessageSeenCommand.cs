using MediatR;
using TwitterClone.Application.Common.Security;

namespace TwitterClone.Application.Chat.Commands.MarkMessageSeen
{
    [Authorize]
    public class MarkMessageSeenCommand : IRequest
    {
        public int MessageId { get; set; }
    }
}
