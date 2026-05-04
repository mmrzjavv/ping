using System.Collections.Generic;
using TwitterClone.Domain.Common;

namespace TwitterClone.Domain.Entities
{
    public class Conversation : AuditableEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsPrivate { get; set; } = true;
        public ICollection<User> Members { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
