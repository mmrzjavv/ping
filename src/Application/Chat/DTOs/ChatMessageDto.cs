using System;
using AutoMapper;
using TwitterClone.Application.Common.Mappings;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Chat.DTOs
{
    public class ChatMessageDto : IMapFrom<Message>
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentUrl { get; set; }
        public string AttachmentContentType { get; set; }
        public bool IsSeen { get; set; }
        public DateTime Created { get; set; }
        public DateTime? SeenAt { get; set; }
        public ChatUserDto CreatedBy { get; set; }
    }
}
