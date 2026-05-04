using FluentValidation;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Chat.Commands.SendMessage
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(v => v.ConversationId).GreaterThan(0);
            RuleFor(v => v.Type).IsInEnum();
            RuleFor(v => v)
                .Must(command => !string.IsNullOrWhiteSpace(command.Content) || !string.IsNullOrWhiteSpace(command.AttachmentFileName))
                .WithMessage("Message content or attachment is required.");
            RuleFor(v => v.Content)
                .MaximumLength(4000)
                .When(v => !string.IsNullOrWhiteSpace(v.Content));
            RuleFor(v => v.AttachmentFileName)
                .NotEmpty()
                .When(v => v.Type == MessageType.Image || v.Type == MessageType.Audio);
        }
    }
}
