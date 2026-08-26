using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Pluvia.Commands;

public class UploadCustomMapCommandHandler : IRequestHandler<UploadCustomMapCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<UploadCustomMapCommandHandler> _logger;

    public UploadCustomMapCommandHandler(
        IEtrmDbContext context, 
        IBlobStorageService blobStorage, 
        ILogger<UploadCustomMapCommandHandler> logger)
    {
        _context = context;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<Guid> Handle(UploadCustomMapCommand request, CancellationToken cancellationToken)
    {
        var objectName = $"custom_maps/{Guid.NewGuid()}_{request.FileName}";
        
        var uploadUrl = await _blobStorage.UploadFileAsync(
            bucketName: "datalake", 
            objectName: objectName, 
            fileStream: request.FileStream, 
            contentType: request.ContentType, 
            cancellationToken: cancellationToken);

        var customScenario = new CustomScenario(
            name: request.Name,
            referenceDate: request.ReferenceDate,
            horizonDays: request.HorizonDays,
            uploadUrl: uploadUrl,
            blendConfig: null
        );

        _context.CustomScenarios.Add(customScenario);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Custom scenario {customScenario.Id} created with map {uploadUrl}");

        return customScenario.Id;
    }
}
