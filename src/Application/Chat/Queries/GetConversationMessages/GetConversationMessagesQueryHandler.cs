using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Attachment;
using TwitterClone.Application.Common.Exceptions;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Chat.Queries.GetConversationMessages
{
    public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, IEnumerable<ChatMessageDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IAttachment _attachment;

        public GetConversationMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper, IAttachment attachment)
        {
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _attachment = attachment;
        }

        public async Task<IEnumerable<ChatMessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
                throw new NotFoundException(nameof(Domain.Entities.Conversation), request.ConversationId);

            if (!conversation.Members.Any(m => m.ApplicationUserId == _currentUserService.UserId))
                throw new ForbiddenAccessException();

            var messages = await _context.Messages
                .Include(m => m.CreatedBy)
                .Where(m => m.ConversationId == request.ConversationId)
                .OrderBy(m => m.Created)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<ChatMessageDto>>(messages);

            foreach (var message in result.Where(m => !string.IsNullOrWhiteSpace(m.AttachmentFileName)))
            {
                var attachmentType = message.Type == MessageType.Audio ? AttachmentType.Audio : AttachmentType.Image;
                message.AttachmentUrl = (await _attachment.GetPermanentUrl(message.AttachmentFileName, attachmentType)).Data;
            }

            return result;
        }
    }
}
