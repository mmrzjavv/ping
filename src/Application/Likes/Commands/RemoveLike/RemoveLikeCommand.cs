using MediatR;

namespace TwitterClone.Application.Likes.Commands.RemoveLike
{
    public class RemoveLikeCommand : IRequest
    {
        public int PostId { get; set; }
    }
}