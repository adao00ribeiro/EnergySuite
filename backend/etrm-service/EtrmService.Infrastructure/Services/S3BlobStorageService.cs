using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using EtrmService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EtrmService.Infrastructure.Services;

public class S3BlobStorageService : IBlobStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3BlobStorageService> _logger;

    public S3BlobStorageService(IAmazonS3 s3Client, ILogger<S3BlobStorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var transferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = objectName,
                BucketName = bucketName,
                ContentType = contentType
            };

            await transferUtility.UploadAsync(uploadRequest, cancellationToken);
            
            _logger.LogInformation($"Successfully uploaded {objectName} to {bucketName}");

            // Assuming a format like s3://bucket/object for our data lake processing
            return $"s3://{bucketName}/{objectName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading {objectName} to S3");
            throw;
        }
    }
}
