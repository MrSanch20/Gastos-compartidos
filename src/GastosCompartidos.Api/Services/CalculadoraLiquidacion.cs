using GastosCompartidos.Api.DTOs.Saldos;

namespace GastosCompartidos.Api.Services;

/// <summary>
/// Dados los saldos de un grupo, propone la forma de saldar las deudas con el menor número
/// de pagos posible: en cada paso cruza al mayor deudor con el mayor acreedor. Esta estrategia
/// "greedy" es la que usan en la práctica las apps de gastos compartidos y da el mínimo exacto
/// de transacciones en los casos que maneja esta app (grupos pequeños).
/// </summary>
public static class CalculadoraLiquidacion
{
    public static List<PagoPropuestoDto> ProponerPagos(IReadOnlyList<SaldoDto> saldos)
    {
        var acreedores = saldos
            .Where(s => s.Saldo > 0)
            .OrderByDescending(s => s.Saldo)
            .Select(s => (s.UsuarioId, s.Nombre, s.Saldo))
            .ToList();

        var deudores = saldos
            .Where(s => s.Saldo < 0)
            .OrderBy(s => s.Saldo)
            .Select(s => (s.UsuarioId, s.Nombre, s.Saldo))
            .ToList();

        var pagos = new List<PagoPropuestoDto>();
        var i = 0;
        var j = 0;

        while (i < deudores.Count && j < acreedores.Count)
        {
            var deudor = deudores[i];
            var acreedor = acreedores[j];
            var monto = Math.Min(-deudor.Saldo, acreedor.Saldo);

            pagos.Add(new PagoPropuestoDto(deudor.UsuarioId, deudor.Nombre, acreedor.UsuarioId, acreedor.Nombre, monto));

            var nuevoSaldoDeudor = deudor.Saldo + monto;
            var nuevoSaldoAcreedor = acreedor.Saldo - monto;
            deudores[i] = (deudor.UsuarioId, deudor.Nombre, nuevoSaldoDeudor);
            acreedores[j] = (acreedor.UsuarioId, acreedor.Nombre, nuevoSaldoAcreedor);

            if (nuevoSaldoDeudor == 0)
            {
                i++;
            }

            if (nuevoSaldoAcreedor == 0)
            {
                j++;
            }
        }

        return pagos;
    }
}
