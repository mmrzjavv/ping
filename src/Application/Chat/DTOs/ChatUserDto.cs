using TwitterClone.Application.Common.Mappings;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Chat.DTOs
{
    public class ChatUserDto : IMapFrom<User>
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string PictureId { get; set; }
    }
}
