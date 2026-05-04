using FluentValidation;

namespace TwitterClone.Application.Conversations.Commands.CreateConversation
{
    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator()
        {
            RuleFor(c => c.Members).NotEmpty();
            RuleFor(c => c.Members).Must(members => members != null && System.Linq.Enumerable.Count(System.Linq.Enumerable.Distinct(members)) == 1)
                .WithMessage("Private chat must contain exactly one recipient.");
        }
    }
}
