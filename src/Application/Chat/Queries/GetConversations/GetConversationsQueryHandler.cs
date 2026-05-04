using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Common.Attachment;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Chat.Queries.GetConversations
{
    public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, IEnumerable<ConversationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IAttachment _attachment;

        public GetConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper, IAttachment attachment)
        {
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _attachment = attachment;
        }

        public async Task<IEnumerable<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
        {
            var applicationUserId = _currentUserService.UserId;

            var conversations = await _context.Conversations
                .Include(c => c.Members)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.CreatedBy)
                .Where(c => c.Members.Any(m => m.ApplicationUserId == applicationUserId))
                .OrderByDescending(c => c.Messages.OrderByDescending(m => m.Created).Select(m => (System.DateTime?)m.Created).FirstOrDefault() ?? c.Created)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<ConversationDto>>(conversations);

            foreach (var conversation in result)
            {
                var entity = conversations.First(c => c.Id == conversation.Id);
                var lastMessage = entity.Messages.OrderByDescending(m => m.Created).FirstOrDefault();
                if (lastMessage == null)
                    continue;

                conversation.LastMessage = _mapper.Map<ChatMessageDto>(lastMessage);

                if (!string.IsNullOrWhiteSpace(lastMessage.AttachmentFileName))
                {
                    var attachmentType = lastMessage.Type == MessageType.Audio ? AttachmentType.Audio : AttachmentType.Image;
                    conversation.LastMessage.AttachmentUrl = (await _attachment.GetPermanentUrl(lastMessage.AttachmentFileName, attachmentType)).Data;
                }
            }

            return result;
        }
    }
}
