using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Application.Common.Attachment;
using TwitterClone.Application.Common.Attachment.DTOs;
using TwitterClone.Domain.Enums;

namespace TwitterClone.WebUI.Controllers
{
    [Authorize]
    [ApiController]
    public class AttachmentController : ApiControllerBase
    {
        private readonly IAttachment _attachmentService;

        public AttachmentController(IAttachment attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost("api/attachment/upload")]
        public async Task<IActionResult> Upload([FromForm] AttachmentDto file, [FromQuery] AttachmentType attachmentType)
        {
            var result = await _attachmentService.Upload(file, attachmentType);
            return StatusCode((int)result.Status, result);
        }

        [HttpDelete("api/attachment/delete")]
        public async Task<IActionResult> Delete([FromQuery] string filename, [FromQuery] AttachmentType attachmentType)
        {
            var result = await _attachmentService.Delete(filename, attachmentType);
            return StatusCode((int)result.Status, result);
        }

        [HttpGet("api/attachment/presigned-url")]
        public async Task<IActionResult> GeneratePresignedUrl([FromQuery] string filename, [FromQuery] AttachmentType attachmentType)
        {
            var result = await _attachmentService.GeneratePresignedUrl(filename, attachmentType);
            return StatusCode((int)result.Status, result);
        }

        [HttpGet("api/attachment/permanent-url")]
        public async Task<IActionResult> GetPermanentUrl([FromQuery] string filename, [FromQuery] AttachmentType attachmentType)
        {
            var result = await _attachmentService.GetPermanentUrl(filename, attachmentType);
            return StatusCode((int)result.Status, result);
        }

        [HttpGet("api/attachment/list")]
        public async Task<IActionResult> ListFiles([FromQuery] AttachmentType? attachmentType = null)
        {
            var result = await _attachmentService.ListFiles(attachmentType);
            return StatusCode((int)result.Status, result);
        }
    }
}
