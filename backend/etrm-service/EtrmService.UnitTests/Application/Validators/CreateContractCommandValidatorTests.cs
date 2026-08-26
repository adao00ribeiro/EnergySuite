using System;
using System.Linq;
using Shouldly;
using Xunit;
using EtrmService.Application.Commands;
using EtrmService.Application.Validators;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Application.Validators;

public class CreateContractCommandValidatorTests
{
    private readonly CreateContractCommandValidator _validator;

    public CreateContractCommandValidatorTests()
    {
        _validator = new CreateContractCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_CounterpartyName_Is_Empty()
    {
        var command = new CreateContractCommand { CounterpartyName = "" };
        var result = _validator.Validate(command);
        result.Errors.ShouldContain(x => x.PropertyName == "CounterpartyName");
    }

    [Fact]
    public void Should_Have_Error_When_VolumeMwMed_Is_Zero_Or_Less()
    {
        var command = new CreateContractCommand { VolumeMwMed = 0 };
        var result = _validator.Validate(command);
        result.Errors.ShouldContain(x => x.PropertyName == "VolumeMwMed");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero_Or_Less()
    {
        var command = new CreateContractCommand { Price = 0 };
        var result = _validator.Validate(command);
        result.Errors.ShouldContain(x => x.PropertyName == "Price");
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Before_StartDate()
    {
        var command = new CreateContractCommand 
        { 
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 5, 1)
        };
        var result = _validator.Validate(command);
        result.Errors.ShouldContain(x => x.PropertyName == "EndDate");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new CreateContractCommand 
        { 
            CounterpartyName = "Valid Counterparty",
            Type = ContractType.Sale,
            VolumeMwMed = 10.5m,
            Price = 120.0m,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 12, 31)
        };
        var result = _validator.Validate(command);
        if (!result.IsValid) throw new Exception(string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
        result.IsValid.ShouldBeTrue();
    }
}
