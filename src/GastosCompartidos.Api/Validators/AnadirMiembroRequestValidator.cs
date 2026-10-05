using FluentValidation;
using GastosCompartidos.Api.DTOs.Grupos;

namespace GastosCompartidos.Api.Validators;

public class AnadirMiembroRequestValidator : AbstractValidator<AnadirMiembroRequest>
{
    public AnadirMiembroRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");
    }
}
