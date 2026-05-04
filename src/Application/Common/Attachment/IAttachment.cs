using System.IO;
using System.Threading.Tasks;
using TwitterClone.Application.Common.Attachment.DTOs;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Application.Common.Attachment
{
    public interface IAttachment
    {
        Task<AttachmentResult<object>> Upload(AttachmentDto file, AttachmentType attachmentType);
        Task<AttachmentResult<string>> UploadFromStreamAsync(Stream stream, string key, string contentType, AttachmentType attachmentType);
        Task<AttachmentResult<string>> Delete(string filename, AttachmentType attachmentType);
        Task<AttachmentResult<string>> GeneratePresignedUrl(string filename, AttachmentType attachmentType);
        Task<AttachmentResult<string>> GetPermanentUrl(string filename, AttachmentType attachmentType);
        Task<AttachmentResult<string>> ListFiles(AttachmentType? attachmentType = null);
        Task<AttachmentResult<(byte[] data, string contentType)?>> Download(string filename, AttachmentType attachmentType);
    }
}
