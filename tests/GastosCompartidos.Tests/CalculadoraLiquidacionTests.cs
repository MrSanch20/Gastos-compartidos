using GastosCompartidos.Api.DTOs.Saldos;
using GastosCompartidos.Api.Services;

namespace GastosCompartidos.Tests;

public class CalculadoraLiquidacionTests
{
    [Fact]
    public void TodosEnPaz_NoProponeNingunPago()
    {
        var saldos = new List<SaldoDto>
        {
            new(1, "Ana", 0m),
            new(2, "Bea", 0m),
            new(3, "Caz", 0m)
        };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.Empty(pagos);
    }

    [Fact]
    public void DosPersonas_UnSoloPagoBastaParaSaldar()
    {
        var saldos = new List<SaldoDto> { new(1, "Ana", 5m), new(2, "Bea", -5m) };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.Single(pagos);
        Assert.Equal(2, pagos[0].DeId);
        Assert.Equal(1, pagos[0].ParaId);
        Assert.Equal(5m, pagos[0].Importe);
    }

    [Fact]
    public void TresPersonas_UnDeudorYDosAcreedores_NecesitaDosPagos()
    {
        // Ana pagó de más (le deben 6,67), Bea y Caz deben 3,33 y 3,34 respectivamente.
        var saldos = new List<SaldoDto> { new(1, "Ana", 6.67m), new(2, "Bea", -3.33m), new(3, "Caz", -3.34m) };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.Equal(2, pagos.Count);
        AsumirSaldanTodasLasDeudas(saldos, pagos);
    }

    [Fact]
    public void CuatroPersonas_DosDeudoresYDosAcreedoresEquilibrados_NecesitaDosPagosNoTres()
    {
        // Caso clásico: no hace falta que todos se cobren entre todos, el greedy encuentra el mínimo.
        var saldos = new List<SaldoDto>
        {
            new(1, "DeudorA", -5m),
            new(2, "DeudorB", -5m),
            new(3, "AcreedorC", 5m),
            new(4, "AcreedorD", 5m)
        };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.Equal(2, pagos.Count);
        AsumirSaldanTodasLasDeudas(saldos, pagos);
    }

    [Fact]
    public void CincoPersonas_SaldosVariados_ElNumeroDePagosNuncaSuperaCuatro()
    {
        // Con 5 personas, como máximo se necesitan (nº de personas - 1) pagos para saldar cualquier combinación.
        var saldos = new List<SaldoDto>
        {
            new(1, "P1", 12.50m),
            new(2, "P2", 7.50m),
            new(3, "P3", -4.00m),
            new(4, "P4", -6.00m),
            new(5, "P5", -10.00m)
        };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.True(pagos.Count <= saldos.Count - 1);
        AsumirSaldanTodasLasDeudas(saldos, pagos);
    }

    [Fact]
    public void UnSoloAcreedorConVariosDeudoresPequenos_CadaDeudorPagaExactamenteLoQueDebe()
    {
        var saldos = new List<SaldoDto>
        {
            new(1, "Acreedor", 9m),
            new(2, "Deudor1", -3m),
            new(3, "Deudor2", -3m),
            new(4, "Deudor3", -3m)
        };

        var pagos = CalculadoraLiquidacion.ProponerPagos(saldos);

        Assert.Equal(3, pagos.Count);
        Assert.All(pagos, p => Assert.Equal(3m, p.Importe));
        AsumirSaldanTodasLasDeudas(saldos, pagos);
    }

    /// <summary>
    /// Aplica los pagos propuestos sobre una copia de los saldos originales y comprueba
    /// que, tras aplicarlos, todo el mundo queda exactamente a cero.
    /// </summary>
    private static void AsumirSaldanTodasLasDeudas(List<SaldoDto> saldosOriginales, List<PagoPropuestoDto> pagos)
    {
        var saldosFinales = saldosOriginales.ToDictionary(s => s.UsuarioId, s => s.Saldo);

        foreach (var pago in pagos)
        {
            saldosFinales[pago.DeId] += pago.Importe;
            saldosFinales[pago.ParaId] -= pago.Importe;
        }

        Assert.All(saldosFinales.Values, saldo => Assert.Equal(0m, saldo));
    }
}
