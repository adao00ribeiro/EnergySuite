using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.CceeIntegration.Commands;
using EtrmService.Application.CceeIntegration.Queries;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace EtrmService.UnitTests.Application.CceeIntegration;

public class CceeIntegrationHandlersTests
{
    private readonly Mock<IEtrmDbContext> _contextMock;

    public CceeIntegrationHandlersTests()
    {
        _contextMock = new Mock<IEtrmDbContext>();
    }

    [Fact]
    public async Task ProcessCliqCceeCsv_ValidCsvStream_ShouldParseAndSaveComparisons()
    {
        // Arrange
        var csvHeaderAndBody = "Period;CounterpartyCode;OperationTicketId;CceeVolume\n2026-09-01;CP_100;a1b2c3d4-e5f6-4a7b-8c9d-000000000001;100.5\n2026-09-02;CP_102;b2c3d4e5-f6a7-4b8c-9d0e-000000000002;250.0";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvHeaderAndBody));

        var dbSetComparisons = new List<CceeComparison>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.CceeComparisons).Returns(dbSetComparisons.Object);

        var dbSetOperations = new List<Operation>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Operations).Returns(dbSetOperations.Object);

        var handler = new ProcessCliqCceeCsvCommandHandler(_contextMock.Object);
        var command = new ProcessCliqCceeCsvCommand { CsvStream = stream };

        // Act
        var count = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, count);
        _contextMock.Verify(c => c.CceeComparisons.Add(It.IsAny<CceeComparison>()), Times.Exactly(2));
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAdjustmentXml_PendingComparisons_ShouldGenerateXmlAndUpdateStatus()
    {
        // Arrange
        var comparisonId = Guid.NewGuid();
        var comparisons = new List<CceeComparison>
        {
            new CceeComparison
            {
                Id = comparisonId,
                Period = DateTime.UtcNow,
                CounterpartyCceeCode = "CP_CCEE_1",
                BackOpsVolume = 100m,
                CceeVolume = 90m,
                Difference = 10m,
                Status = CceeComparisonStatus.Pendente
            }
        };

        var dbSetMock = comparisons.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.CceeComparisons).Returns(dbSetMock.Object);

        var handler = new GenerateAdjustmentXmlCommandHandler(_contextMock.Object);
        var command = new GenerateAdjustmentXmlCommand { ComparisonIds = new List<Guid> { comparisonId } };

        // Act
        var xmlResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(xmlResult);
        Assert.Contains("cceal", xmlResult);
        Assert.Contains("CP_CCEE_1", xmlResult);
        Assert.Equal(CceeComparisonStatus.Ajustado, comparisons[0].Status);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCceeComparisons_ShouldReturnComparisonsList()
    {
        // Arrange
        var comparisons = new List<CceeComparison>
        {
            new CceeComparison
            {
                Id = Guid.NewGuid(),
                Period = DateTime.UtcNow,
                CounterpartyCceeCode = "CP_1",
                BackOpsVolume = 50m,
                CceeVolume = 50m,
                Difference = 0m,
                Status = CceeComparisonStatus.Ok
            }
        };

        var dbSetMock = comparisons.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.CceeComparisons).Returns(dbSetMock.Object);

        var handler = new GetCceeComparisonsQueryHandler(_contextMock.Object);

        // Act
        var results = await handler.Handle(new GetCceeComparisonsQuery(), CancellationToken.None);

        // Assert
        Assert.Single(results);
        Assert.Equal("CP_1", results.First().CounterpartyCceeCode);
    }
}
