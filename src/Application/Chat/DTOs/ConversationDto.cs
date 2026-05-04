using System;
using System.Collections.Generic;
using TwitterClone.Application.Common.Mappings;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Chat.DTOs
{
    public class ConversationDto : IMapFrom<Conversation>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime Created { get; set; }
        public ChatMessageDto LastMessage { get; set; }
        public ICollection<ChatUserDto> Members { get; set; }
    }
}
