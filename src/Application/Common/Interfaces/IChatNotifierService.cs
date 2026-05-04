using System.Threading.Tasks;
using TwitterClone.Application.Chat.DTOs;

namespace TwitterClone.Application.Common.Interfaces
{
    public interface IChatNotifierService
    {
        Task SendMessageAsync(int conversationId, ChatMessageDto message);
        Task TypingAsync(int conversationId, int userId);
        Task SeenAsync(int conversationId, int messageId, int userId);
    }
}
