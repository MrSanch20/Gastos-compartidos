using System.Text;
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

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IJwtGenerador, JwtGenerador>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

    opciones.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", null, null), new List<string>() }
    });
});

var claveJwt = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException("Falta configurar 'Jwt:Clave' (usa user-secrets en desarrollo).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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
