using FluentValidation;
using GastosCompartidos.Api.DTOs.Auth;

namespace GastosCompartidos.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
