using FluentValidation;
using GastosCompartidos.Api.DTOs.Gastos;
using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Api.Validators;

public class ActualizarGastoRequestValidator : AbstractValidator<ActualizarGastoRequest>
{
    public ActualizarGastoRequestValidator()
    {
        RuleFor(r => r.Concepto)
            .NotEmpty().WithMessage("El concepto es obligatorio.")
            .MaximumLength(200);

        RuleFor(r => r.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor que cero.");

        RuleFor(r => r.Participaciones)
            .NotEmpty().WithMessage("El gasto debe tener al menos un participante.");

        RuleForEach(r => r.Participaciones)
            .ChildRules(p => p.RuleFor(x => x.UsuarioId).GreaterThan(0));

        RuleFor(r => r)
            .Must(r => r.Participaciones.Select(p => p.UsuarioId).Distinct().Count() == r.Participaciones.Count)
            .WithMessage("No se puede repetir el mismo participante en un gasto.");

        When(r => r.TipoReparto == TipoReparto.Personalizado, () =>
        {
            RuleForEach(r => r.Participaciones)
                .ChildRules(p => p.RuleFor(x => x.Importe)
                    .NotNull().WithMessage("En un reparto personalizado hay que indicar el importe de cada participante.")
                    .GreaterThan(0).WithMessage("El importe de cada participante debe ser mayor que cero."));
        });
    }
}
