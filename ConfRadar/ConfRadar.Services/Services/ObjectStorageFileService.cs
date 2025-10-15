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
    }
    public class ObjectStorageFileService : IObjectStorageFileService
    {
        private readonly IMinioClient _minioClient;
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
    }
}
