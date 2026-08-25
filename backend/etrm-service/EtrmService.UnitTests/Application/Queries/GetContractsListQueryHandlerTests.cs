using System;
using System.Collections.Generic;
using System.Linq;
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

public class GetContractsListQueryHandlerTests
{
    private readonly Mock<IContractRepository> _repositoryMock;
    private readonly GetContractsListQueryHandler _handler;

    public GetContractsListQueryHandlerTests()
    {
        _repositoryMock = new Mock<IContractRepository>();
        _handler = new GetContractsListQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_List_Of_ContractDtos()
    {
        // Arrange
        var contract1 = new Contract(
            "Counterparty A", ContractType.Purchase, EnergySubmarket.SE_CO, 10m, 150m, DateTime.Now, DateTime.Now.AddMonths(1));
        var contract2 = new Contract(
            "Counterparty B", ContractType.Sale, EnergySubmarket.NORDESTE, 5m, 180m, DateTime.Now, DateTime.Now.AddMonths(2));

        var contractsList = new List<Contract> { contract1, contract2 };

        _repositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractsList);

        var query = new GetContractsListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        
        var firstResult = result.First();
        firstResult.Id.ShouldBe(contract1.Id);
        firstResult.CounterpartyName.ShouldBe(contract1.CounterpartyName);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Contracts()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract>());

        var query = new GetContractsListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
