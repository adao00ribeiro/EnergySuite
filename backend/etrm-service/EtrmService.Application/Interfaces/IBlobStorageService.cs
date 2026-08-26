using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EtrmService.Application.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
}
