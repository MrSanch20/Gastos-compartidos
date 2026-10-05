using GastosCompartidos.Api.Services;

namespace GastosCompartidos.Tests;

public class RepartidorGastosTests
{
    [Fact]
    public void RepartirPartesIguales_DiezEurosEntreTres_AsignaLosCentimosSobrantesAlosPrimeros()
    {
        var resultado = RepartidorGastos.RepartirPartesIguales(10m, [1, 2, 3]);

        Assert.Equal(3.34m, resultado[0].Importe);
        Assert.Equal(3.33m, resultado[1].Importe);
        Assert.Equal(3.33m, resultado[2].Importe);
        Assert.Equal(10m, resultado.Sum(r => r.Importe));
    }

    [Fact]
    public void RepartirPartesIguales_DivisionExacta_RepartoIdenticoParaTodos()
    {
        var resultado = RepartidorGastos.RepartirPartesIguales(30m, [1, 2, 3]);

        Assert.All(resultado, r => Assert.Equal(10m, r.Importe));
    }

    [Fact]
    public void RepartirPartesIguales_UnSoloParticipante_SeLlevaTodoElImporte()
    {
        var resultado = RepartidorGastos.RepartirPartesIguales(99.99m, [1]);

        Assert.Equal(99.99m, resultado[0].Importe);
    }

    [Fact]
    public void RepartirPartesIguales_CentimoSobranteEntreCinco_SumaExactamenteElTotal()
    {
        var resultado = RepartidorGastos.RepartirPartesIguales(100m, [1, 2, 3, 4, 5]);

        Assert.Equal(100m, resultado.Sum(r => r.Importe));
        Assert.All(resultado, r => Assert.Equal(20m, r.Importe));
    }

    [Fact]
    public void RepartirPartesIguales_ConRestoNoDivisible_LosPrimerosParticipantesRecibenUnCentimoMas()
    {
        var resultado = RepartidorGastos.RepartirPartesIguales(10.01m, [1, 2, 3]);

        Assert.Equal(3.34m, resultado[0].Importe);
        Assert.Equal(3.34m, resultado[1].Importe);
        Assert.Equal(3.33m, resultado[2].Importe);
        Assert.Equal(10.01m, resultado.Sum(r => r.Importe));
    }
}
