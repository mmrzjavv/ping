using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using TwitterClone.Application.Common.Attachment;
using TwitterClone.Application.Common.Attachment.DTOs;
using TwitterClone.Domain.Enums;

namespace TwitterClone.Infrastructure.Services
{
    public class AttachmentManager : IAttachment
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _endpointUrl;

        public AttachmentManager(IConfiguration configuration)
        {
            _bucketName = configuration["LiaraStorage:BucketName"]
                ?? throw new InvalidOperationException("LiaraStorage:BucketName is not configured in appsettings.json");

            var accessKey = configuration["LiaraStorage:AccessKey"]
                ?? throw new InvalidOperationException("LiaraStorage:AccessKey is not configured in appsettings.json");

            var secretKey = configuration["LiaraStorage:SecretKey"]
                ?? throw new InvalidOperationException("LiaraStorage:SecretKey is not configured in appsettings.json");

            _endpointUrl = configuration["LiaraStorage:EndpointUrl"]
                ?? throw new InvalidOperationException("LiaraStorage:EndpointUrl is not configured in appsettings.json");

            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            var config = new AmazonS3Config
            {
                ServiceURL = _endpointUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(credentials, config);
        }

        public async Task<AttachmentResult<object>> Upload(AttachmentDto file, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<object>();

            try
            {
                if (file?.FileData == null || file.FileData.Length == 0)
                    return result.Failed("File is null or empty", HttpStatusCode.BadRequest);

                var fileName = GenerateFileName(file.FileData.FileName, attachmentType);

                using var stream = file.FileData.OpenReadStream();
                var response = await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = stream,
                    ContentType = file.FileData.ContentType
                });

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return result.Succeed("File uploaded successfully", new
                    {
                        FileName = fileName,
                        Url = BuildPermanentUrl(fileName),
                        ContentType = file.FileData.ContentType,
                        Size = file.FileData.Length
                    });
                }

                return result.Failed("Failed to upload file", response.HttpStatusCode);
            }
            catch (AmazonS3Exception ex)
            {
                return result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return result.Failed("Error uploading file", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AttachmentResult<string>> UploadFromStreamAsync(Stream stream, string key, string contentType, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<string>();

            try
            {
                if (stream == null || !stream.CanRead)
                    return result.Failed("Stream is null or not readable", HttpStatusCode.BadRequest);

                var response = await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType
                });

                return response.HttpStatusCode == HttpStatusCode.OK
                    ? result.Succeed("File uploaded successfully", key)
                    : result.Failed("Failed to upload file", response.HttpStatusCode);
            }
            catch (AmazonS3Exception ex)
            {
                return result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return result.Failed("Error uploading file", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AttachmentResult<string>> Delete(string filename, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<string>();

            try
            {
                var response = await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filename
                });

                return response.HttpStatusCode == HttpStatusCode.NoContent || response.HttpStatusCode == HttpStatusCode.OK
                    ? result.Succeed("File deleted successfully")
                    : result.Failed("Failed to delete file", response.HttpStatusCode);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return result.Failed("File not found", HttpStatusCode.NotFound, ex.Message);
            }
            catch (AmazonS3Exception ex)
            {
                return result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return result.Failed("Error deleting file", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public Task<AttachmentResult<string>> GeneratePresignedUrl(string filename, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<string>();

            try
            {
                var url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = filename,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                return Task.FromResult(result.Succeed("Presigned URL generated successfully", url));
            }
            catch (AmazonS3Exception ex)
            {
                return Task.FromResult(result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message));
            }
            catch (Exception ex)
            {
                return Task.FromResult(result.Failed("Error generating presigned URL", HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        public Task<AttachmentResult<string>> GetPermanentUrl(string filename, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<string>();

            try
            {
                return Task.FromResult(result.Succeed("Permanent URL retrieved successfully", BuildPermanentUrl(filename)));
            }
            catch (Exception ex)
            {
                return Task.FromResult(result.Failed("Error getting permanent URL", HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        public async Task<AttachmentResult<string>> ListFiles(AttachmentType? attachmentType = null)
        {
            var result = new AttachmentResult<string>();

            try
            {
                var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName
                });

                var files = new List<string>();

                foreach (var entry in response.S3Objects)
                {
                    if (!attachmentType.HasValue || RecognizeAttachmentType(entry.Key) == attachmentType.Value)
                        files.Add(entry.Key);
                }

                return result.Succeed("Files listed successfully", JsonSerializer.Serialize(files));
            }
            catch (AmazonS3Exception ex)
            {
                return result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return result.Failed("Error listing files", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AttachmentResult<(byte[] data, string contentType)?>> Download(string filename, AttachmentType attachmentType)
        {
            var result = new AttachmentResult<(byte[] data, string contentType)?>();

            try
            {
                using var response = await _s3Client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filename
                });
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);

                return result.Succeed("File downloaded successfully", (memoryStream.ToArray(), response.Headers.ContentType ?? "application/octet-stream"));
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return result.Failed("File not found", HttpStatusCode.NotFound, ex.Message);
            }
            catch (AmazonS3Exception ex)
            {
                return result.Failed("S3 error occurred", HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return result.Failed("Error downloading file", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string BuildPermanentUrl(string fileName)
        {
            var endpointUri = new Uri(_endpointUrl);
            return $"https://{_bucketName}.{endpointUri.Host}/{fileName}";
        }

        private static string GenerateFileName(string originalFileName, AttachmentType attachmentType)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            return $"{attachmentType.ToString().ToLowerInvariant()}/{fileNameWithoutExtension}_{timestamp}_{uniqueId}{extension}";
        }

        private static AttachmentType RecognizeAttachmentType(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            if (filename.StartsWith("reports/", StringComparison.OrdinalIgnoreCase))
                return AttachmentType.Report;

            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => AttachmentType.Image,
                ".mov" or ".mp4" or ".avi" or ".mkv" => AttachmentType.Video,
                ".pdf" => AttachmentType.Pdf,
                ".xlsx" or ".xls" or ".csv" => AttachmentType.Report,
                ".mp3" or ".wav" or ".ogg" or ".webm" => AttachmentType.Audio,
                _ => AttachmentType.Image
            };
        }
    }
}
