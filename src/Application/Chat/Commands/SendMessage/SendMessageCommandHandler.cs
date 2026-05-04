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
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Chat.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IAttachment _attachment;
        private readonly IChatNotifierService _chatNotifierService;

        public SendMessageCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IAttachment attachment,
            IChatNotifierService chatNotifierService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _attachment = attachment;
            _chatNotifierService = chatNotifierService;
        }

        public async Task<SendMessageResponseDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
                throw new NotFoundException(nameof(Conversation), request.ConversationId);

            var sender = await _context.DomainUsers
                .FirstOrDefaultAsync(u => u.ApplicationUserId == _currentUserService.UserId, cancellationToken);

            if (sender == null || !conversation.Members.Any(m => m.Id == sender.Id))
                throw new ForbiddenAccessException();

            var message = new Message
            {
                ConversationId = conversation.Id,
                Content = request.Content?.Trim(),
                Type = request.Type,
                AttachmentFileName = request.AttachmentFileName,
                AttachmentContentType = request.AttachmentContentType,
                IsSeen = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            var savedMessage = await _context.Messages
                .Include(m => m.CreatedBy)
                .FirstAsync(m => m.Id == message.Id, cancellationToken);

            var dto = _mapper.Map<ChatMessageDto>(savedMessage);

            if (!string.IsNullOrWhiteSpace(savedMessage.AttachmentFileName))
            {
                var attachmentType = savedMessage.Type == Domain.Enums.MessageType.Audio
                    ? Domain.Enums.AttachmentType.Audio
                    : Domain.Enums.AttachmentType.Image;
                var urlResult = await _attachment.GetPermanentUrl(savedMessage.AttachmentFileName, attachmentType);
                dto.AttachmentUrl = urlResult.Data;
            }

            await _chatNotifierService.SendMessageAsync(conversation.Id, dto);

            return new SendMessageResponseDto
            {
                ConversationId = conversation.Id,
                Message = dto
            };
        }
    }
}
