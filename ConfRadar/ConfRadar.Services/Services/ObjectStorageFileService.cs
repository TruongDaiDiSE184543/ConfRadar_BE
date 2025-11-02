using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
namespace ConfRadar.Services.Services
{
    public interface IObjectStorageFileService
    {
        Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType);
        Task<Stream> GetFileAsync(string bucketName, string objectName);
        Task DeleteFileAsync(string bucketName, string objectName);
        //Task<string> GeneratePresignedUrlAsync(string bucketName, string objectName, int expirySeconds);
        Task EnsureBucketExistsAsync(string bucketName);
        public bool IsValidImageFile(IFormFile file);
        public bool IsValidDocumentFile(IFormFile file);
        public bool IsValidVideoFile(IFormFile file);
    }
    public class ObjectStorageFileService : IObjectStorageFileService
    {
        private readonly IMinioClient _minioClient;

        private static readonly Dictionary<string, string> AllowedImageTypes = new()
    {
        { "image/jpeg", ".jpeg" },
        { "image/jpg", ".jpg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
        { "image/webp", ".webp" }
    };

        // Dictionary of allowed document content types for research papers.
        private static readonly Dictionary<string, string> AllowedDocumentTypes = new()
    {
        { "application/pdf", ".pdf" },
        { "application/msword", ".doc" },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" }
    };

        // Dictionary of allowed video content types.
        private static readonly Dictionary<string, string> AllowedVideoTypes = new()
    {
        { "video/mp4", ".mp4" },
        { "video/mpeg", ".mpeg" },
        { "video/quicktime", ".mov" },
        { "video/x-msvideo", ".avi" }
    };
        public ObjectStorageFileService(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }


        public async Task DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
            }
            catch (MinioException ex)
            {
                throw;
            }
        }

        public async Task EnsureBucketExistsAsync(string bucketName)
        {
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool exists = await _minioClient.BucketExistsAsync(beArgs);
            if (!exists)
            {
                var newBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(newBucketArgs);
            }
        }

        public async Task<Stream> GetFileAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream =>
                    {
                        stream.CopyTo(memoryStream);

                    });
                await _minioClient.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (MinioException ex)
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType)
        {
            try
            {
                await EnsureBucketExistsAsync(bucketName);
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType);
                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                return $"{bucketName}/{objectName}";
            }
            catch (MinioException e)
            {
                throw;
            }
        }


        /// <summary>
        /// Validates if the uploaded file is an allowed image type based on its content type.
        /// </summary>
        /// <param name="file">The IFormFile to validate.</param>
        /// <returns>True if the content type is a valid image type, otherwise false.</returns>
        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            return AllowedImageTypes.ContainsKey(file.ContentType.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the uploaded file is an allowed document type (for research papers).
        /// </summary>
        /// <param name="file">The IFormFile to validate.</param>
        /// <returns>True if the content type is a valid document type, otherwise false.</returns>
        public bool IsValidDocumentFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            return AllowedDocumentTypes.ContainsKey(file.ContentType.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the uploaded file is an allowed video type.
        /// </summary>
        /// <param name="file">The IFormFile to validate.</param>
        /// <returns>True if the content type is a valid video type, otherwise false.</returns>
        public bool IsValidVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            return AllowedVideoTypes.ContainsKey(file.ContentType.ToLowerInvariant());
        }
    }
}
