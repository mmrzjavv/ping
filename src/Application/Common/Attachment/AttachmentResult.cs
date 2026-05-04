using System.Net;

namespace TwitterClone.Application.Common.Attachment
{
    public class AttachmentResult<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public HttpStatusCode Status { get; set; }
        public T Data { get; set; }

        public AttachmentResult<T> Succeed(string message, T data = default)
        {
            Succeeded = true;
            Message = message;
            Status = HttpStatusCode.OK;
            Data = data;
            return this;
        }

        public AttachmentResult<T> Failed(string message, HttpStatusCode status, string error = null)
        {
            Succeeded = false;
            Message = message;
            Status = status;
            Error = error;
            return this;
        }
    }
}
