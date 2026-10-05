using GastosCompartidos.Api.Models;

namespace GastosCompartidos.Api.Services;

public interface IJwtGenerador
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
