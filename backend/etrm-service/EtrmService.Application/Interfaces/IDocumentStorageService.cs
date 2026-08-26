using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EtrmService.Application.Interfaces;

public interface IDocumentStorageService
{
    Task<string> UploadAsync(string bucketName, string objectKey, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string bucketName, string objectKey, int expirySeconds = 3600);
}
