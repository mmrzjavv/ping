namespace TwitterClone.Application.Chat.DTOs
{
    public class SendMessageResponseDto
    {
        public int ConversationId { get; set; }
        public ChatMessageDto Message { get; set; }
    }
}
