using FluentValidation;
using GastosCompartidos.Api.DTOs.Saldos;

namespace GastosCompartidos.Api.Validators;

public class RegistrarLiquidacionRequestValidator : AbstractValidator<RegistrarLiquidacionRequest>
{
    public RegistrarLiquidacionRequestValidator()
    {
        RuleFor(r => r.DeId).GreaterThan(0);
        RuleFor(r => r.ParaId).GreaterThan(0);
        RuleFor(r => r.Importe).GreaterThan(0).WithMessage("El importe debe ser mayor que cero.");

        RuleFor(r => r)
            .Must(r => r.DeId != r.ParaId)
            .WithMessage("El pagador y el receptor no pueden ser la misma persona.");
    }
}
