using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using EtrmService.Application.Queries;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Application.Queries;

public class GetContractByIdQueryHandlerTests
{
    private readonly Mock<IContractRepository> _repositoryMock;
    private readonly GetContractByIdQueryHandler _handler;

    public GetContractByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IContractRepository>();
        _handler = new GetContractByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_ContractDto_When_Contract_Exists()
    {
        // Arrange
        var contract = new Contract(
            "Test Counterparty", 
            ContractType.Purchase, 
            EnergySubmarket.SE_CO, 
            10m, 
            200m, 
            DateTime.Now, 
            DateTime.Now.AddMonths(6));

        _repositoryMock.Setup(repo => repo.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var query = new GetContractByIdQuery(contract.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(contract.Id);
        result.CounterpartyName.ShouldBe(contract.CounterpartyName);
        result.Type.ShouldBe(contract.Type.ToString());
        result.Submarket.ShouldBe(contract.Submarket.ToString());
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Contract_Does_Not_Exist()
    {
        // Arrange
        var query = new GetContractByIdQuery(Guid.NewGuid());

        _repositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
