using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using GastosCompartidos.Api.Data;
using GastosCompartidos.Api.Middleware;
using GastosCompartidos.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opciones =>
    {
        // Los enums (RolGrupo, TipoReparto...) se exponen como texto ("PartesIguales") en vez
        // de como números, para que la API sea legible sin tener que consultar el código.
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IJwtGenerador, JwtGenerador>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<ISaldoService, SaldoService>();

var proveedorBaseDeDatos = builder.Configuration["BaseDeDatos:Proveedor"] ?? "Sqlite";
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<GastosCompartidosDbContext>(opciones =>
{
    if (proveedorBaseDeDatos.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        opciones.UseNpgsql(cadenaConexion);
    }
    else
    {
        opciones.UseSqlite(cadenaConexion);
    }
});

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gastos Compartidos API",
        Version = "v1",
        Description = "API REST para gestionar gastos compartidos entre grupos de personas."
    });

    // Permite introducir el token JWT desde la interfaz de Swagger.
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT con el prefijo 'Bearer '. Ejemplo: \"Bearer {token}\""
    });

    opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", documento), new List<string>() }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Se configura a través de IConfiguration inyectado (no leyendo builder.Configuration directamente)
// para que las opciones se resuelvan de forma perezosa, con la configuración ya completa
// (incluida la que sobrescriben los tests de integración).
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((opciones, configuracion) =>
    {
        var claveJwt = configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException("Falta configurar 'Jwt:Clave' (usa user-secrets en desarrollo).");

        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracion["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = configuracion["Jwt:Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Aplica las migraciones pendientes al arrancar. Es opt-in (apagado por defecto) para no
// interferir con los tests de integración, que montan su propio esquema; se activa en
// Docker con la variable de entorno AplicarMigracionesAlArrancar=true.
if (app.Configuration.GetValue<bool>("AplicarMigracionesAlArrancar"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<GastosCompartidosDbContext>().Database.Migrate();
}

app.UseMiddleware<ManejadorExcepcionesMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Gastos Compartidos API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
