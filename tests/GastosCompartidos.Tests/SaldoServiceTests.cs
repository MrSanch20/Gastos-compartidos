namespace GastosCompartidos.Tests;

public class SaldoServiceTests
{
    [Fact]
    public async Task DosPersonas_UnGastoAPartesIguales_ElQuePagoQuedaConSaldoPositivo()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(2);
        await escenario.AnadirGastoPartesIgualesAsync(pagadorIndice: 0, importe: 10m, 0, 1);

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.Equal(5m, saldos.Single(s => s.UsuarioId == escenario.IdDe(0)).Saldo);
        Assert.Equal(-5m, saldos.Single(s => s.UsuarioId == escenario.IdDe(1)).Saldo);
    }

    [Fact]
    public async Task DosPersonas_SinNingunGasto_TodosEnPaz()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(2);

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.All(saldos, s => Assert.Equal(0m, s.Saldo));
        var pagos = await escenario.CrearServicioSaldos().ProponerLiquidacionAsync(escenario.Grupo.Id, escenario.IdDe(0), default);
        Assert.Empty(pagos);
    }

    [Fact]
    public async Task TresPersonas_GastoDeDiezEuros_RepartenLosCentimosYElSaldoSumaCero()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(3);
        await escenario.AnadirGastoPartesIgualesAsync(pagadorIndice: 0, importe: 10m, 0, 1, 2);

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        // Reparto: Persona1 (pagador) se queda su participación de 3,34; Persona2 y Persona3, 3,33 cada una.
        Assert.Equal(6.66m, saldos.Single(s => s.UsuarioId == escenario.IdDe(0)).Saldo); // pagó 10, le corresponden 3,34
        Assert.Equal(-3.33m, saldos.Single(s => s.UsuarioId == escenario.IdDe(1)).Saldo);
        Assert.Equal(-3.33m, saldos.Single(s => s.UsuarioId == escenario.IdDe(2)).Saldo);
        Assert.Equal(0m, saldos.Sum(s => s.Saldo));
    }

    [Fact]
    public async Task TresPersonas_ReparticionPersonalizada_SaldosCoincidenConLoIndicado()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(3);
        await escenario.AnadirGastoPersonalizadoAsync(pagadorIndice: 0, importe: 100m,
            (0, 50m), (1, 30m), (2, 20m));

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.Equal(50m, saldos.Single(s => s.UsuarioId == escenario.IdDe(0)).Saldo);
        Assert.Equal(-30m, saldos.Single(s => s.UsuarioId == escenario.IdDe(1)).Saldo);
        Assert.Equal(-20m, saldos.Single(s => s.UsuarioId == escenario.IdDe(2)).Saldo);
    }

    [Fact]
    public async Task CincoPersonas_VariosGastosDeDistintosPagadores_ElSaldoTotalSiempreSumaCero()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(5);
        await escenario.AnadirGastoPartesIgualesAsync(0, 100m, 0, 1, 2, 3, 4);
        await escenario.AnadirGastoPartesIgualesAsync(2, 37.50m, 0, 1, 2, 3, 4);
        await escenario.AnadirGastoPersonalizadoAsync(4, 60m, (0, 10m), (1, 10m), (2, 10m), (3, 10m), (4, 20m));

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.Equal(0m, saldos.Sum(s => s.Saldo));
    }

    [Fact]
    public async Task UnaLiquidacionQueSaldaCompletamente_DejaElSaldoACero()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(2);
        await escenario.AnadirGastoPartesIgualesAsync(0, 10m, 0, 1);

        await escenario.AnadirLiquidacionAsync(deIndice: 1, paraIndice: 0, importe: 5m);

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.Equal(0m, saldos.Single(s => s.UsuarioId == escenario.IdDe(0)).Saldo);
        Assert.Equal(0m, saldos.Single(s => s.UsuarioId == escenario.IdDe(1)).Saldo);
    }

    [Fact]
    public async Task UnaLiquidacionParcial_ReduceElSaldoSinSaldarloDelTodo()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(2);
        await escenario.AnadirGastoPartesIgualesAsync(0, 10m, 0, 1);

        await escenario.AnadirLiquidacionAsync(deIndice: 1, paraIndice: 0, importe: 2m);

        var saldos = await escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, escenario.IdDe(0), default);

        Assert.Equal(3m, saldos.Single(s => s.UsuarioId == escenario.IdDe(0)).Saldo);
        Assert.Equal(-3m, saldos.Single(s => s.UsuarioId == escenario.IdDe(1)).Saldo);
    }

    [Fact]
    public async Task SiNoSoyMiembroDelGrupo_NoPuedoVerLosSaldos()
    {
        using var escenario = await EscenarioGrupo.CrearAsync(2);
        var intruso = new Api.Models.Usuario { Nombre = "Intruso", Email = "intruso@test.com", PasswordHash = "x" };
        escenario.Contexto.Usuarios.Add(intruso);
        await escenario.Contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<Api.Excepciones.PermisoDenegadoException>(
            () => escenario.CrearServicioSaldos().CalcularSaldosAsync(escenario.Grupo.Id, intruso.Id, default));
    }
}
