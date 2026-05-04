using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Application.Chat.Commands.MarkMessageSeen;
using TwitterClone.Application.Chat.Commands.SendMessage;
using TwitterClone.Application.Chat.DTOs;
using TwitterClone.Application.Chat.Queries.GetConversationMessages;
using TwitterClone.Application.Chat.Queries.GetConversations;

namespace TwitterClone.WebUI.Controllers
{
    [Authorize]
    public class ChatController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<ConversationDto>> GetConversations()
        {
            return await Mediator.Send(new GetConversationsQuery());
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IEnumerable<ChatMessageDto>> GetMessages(int conversationId)
        {
            return await Mediator.Send(new GetConversationMessagesQuery { ConversationId = conversationId });
        }

        [HttpPost("messages")]
        public async Task<ActionResult<SendMessageResponseDto>> SendMessage(SendMessageCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPost("messages/{messageId}/seen")]
        public async Task<IActionResult> MarkSeen(int messageId)
        {
            await Mediator.Send(new MarkMessageSeenCommand { MessageId = messageId });
            return Ok();
        }
    }
}
