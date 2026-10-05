using FluentValidation;
using GastosCompartidos.Api.DTOs.Grupos;

namespace GastosCompartidos.Api.Validators;

public class CrearGrupoRequestValidator : AbstractValidator<CrearGrupoRequest>
{
    public CrearGrupoRequestValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre del grupo es obligatorio.")
            .MaximumLength(100);
    }
}
