using Microsoft.AspNetCore.Http;

namespace TwitterClone.Application.Common.Attachment.DTOs
{
    public class AttachmentDto
    {
        public IFormFile FileData { get; set; }
    }
}
