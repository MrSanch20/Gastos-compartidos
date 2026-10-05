using GastosCompartidos.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
