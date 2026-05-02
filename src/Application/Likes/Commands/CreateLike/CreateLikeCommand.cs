using MediatR;

namespace TwitterClone.Application.Likes.Commands.CreateLike
{
    public class CreateLikeCommand : IRequest
    {
        public int PostId { get; set; }
    }
}