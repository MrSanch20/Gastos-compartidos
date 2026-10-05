using FluentValidation;
using GastosCompartidos.Api.DTOs.Auth;

namespace GastosCompartidos.Api.Validators;

public class RegistroRequestValidator : AbstractValidator<RegistroRequest>
{
    public RegistroRequestValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100);

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(256);

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}
