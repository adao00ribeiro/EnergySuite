using FluentValidation;
using EtrmService.Application.Commands;

namespace EtrmService.Application.Validators;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.CounterpartyName)
            .NotEmpty().WithMessage("CounterpartyName is required.")
            .MaximumLength(150).WithMessage("CounterpartyName must not exceed 150 characters.");

        RuleFor(x => x.VolumeMwMed)
            .GreaterThan(0).WithMessage("VolumeMwMed must be greater than zero.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}
