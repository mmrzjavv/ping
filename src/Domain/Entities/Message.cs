using TwitterClone.Domain.Common;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Domain.Entities
{
    public class Message : AuthorAuditableEntity
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public string AttachmentFileName { get; set; }
        public string AttachmentContentType { get; set; }
        public bool IsSeen { get; set; }
        public System.DateTime? SeenAt { get; set; }
    }
}
