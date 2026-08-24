using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using EtrmService.Application.Commands;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Application.Commands;

public class CreateContractCommandHandlerTests
{
    private readonly Mock<IContractRepository> _repositoryMock;
    private readonly CreateContractCommandHandler _handler;

    public CreateContractCommandHandlerTests()
    {
        _repositoryMock = new Mock<IContractRepository>();
        _handler = new CreateContractCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Add_Contract_To_Repository_And_Return_Guid()
    {
        // Arrange
        var command = new CreateContractCommand
        {
            CounterpartyName = "Solaris Energy",
            Type = ContractType.Sale,
            Submarket = EnergySubmarket.NORDESTE,
            VolumeMwMed = 5.0m,
            Price = 180.50m,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 31)
        };

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.ShouldNotBe(Guid.Empty);
        
        // Verifica se o método AddAsync foi chamado exatamente 1 vez com a entidade preenchida corretamente
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Contract>(c => 
            c.Id == resultId &&
            c.CounterpartyName == command.CounterpartyName &&
            c.VolumeMwMed == command.VolumeMwMed
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
