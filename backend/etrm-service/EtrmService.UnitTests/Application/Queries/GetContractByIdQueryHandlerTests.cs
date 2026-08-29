using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MockQueryable.Moq;
using Shouldly;
using Xunit;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Queries;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Application.Queries;

public class GetContractByIdQueryHandlerTests
{
    private readonly Mock<IEtrmDbContext> _contextMock;
    private readonly GetContractByIdQueryHandler _handler;

    public GetContractByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IEtrmDbContext>();
        _handler = new GetContractByIdQueryHandler(_contextMock.Object);
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
            DateTime.Now.AddMonths(6),
            null,
            null,
            Guid.NewGuid(),
            PriceIndexType.IPCA,
            0.05m);

        var contracts = new List<Contract> { contract };
        var mockDbSet = contracts.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Contracts).Returns(mockDbSet.Object);

        var query = new GetContractByIdQuery(contract.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(contract.Id);
        result.CounterpartyName.ShouldBe(contract.CounterpartyName);
        result.Type.ShouldBe(contract.Type.ToString());
        result.Submarket.ShouldBe(contract.Submarket.ToString());
        result.Version.ShouldBe(contract.Version);
        result.PriceIndexType.ShouldBe(contract.PriceIndexType.ToString());
        result.FlexibilityMargin.ShouldBe(contract.FlexibilityMargin);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Contract_Does_Not_Exist()
    {
        // Arrange
        var contracts = new List<Contract>();
        var mockDbSet = contracts.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Contracts).Returns(mockDbSet.Object);

        var query = new GetContractByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
