using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Common.Exceptions;
using TwitterClone.Application.Common.Interfaces;

namespace TwitterClone.Application.Chat.Commands.MarkMessageSeen
{
    public class MarkMessageSeenCommandHandler : IRequestHandler<MarkMessageSeenCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;
        private readonly IChatNotifierService _chatNotifierService;

        public MarkMessageSeenCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime,
            IChatNotifierService chatNotifierService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
            _chatNotifierService = chatNotifierService;
        }

        public async Task<Unit> Handle(MarkMessageSeenCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.Messages
                .Include(m => m.Conversation)
                .ThenInclude(c => c.Members)
                .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

            if (message == null)
                throw new NotFoundException(nameof(Domain.Entities.Message), request.MessageId);

            var currentUser = await _context.DomainUsers
                .FirstOrDefaultAsync(u => u.ApplicationUserId == _currentUserService.UserId, cancellationToken);

            if (currentUser == null || !message.Conversation.Members.Any(m => m.Id == currentUser.Id))
                throw new ForbiddenAccessException();

            message.IsSeen = true;
            message.SeenAt = _dateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            await _chatNotifierService.SeenAsync(message.ConversationId, message.Id, currentUser.Id);

            return Unit.Value;
        }
    }
}
