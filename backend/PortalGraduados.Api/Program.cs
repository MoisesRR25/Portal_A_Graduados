
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;
using Microsoft.AspNetCore.Identity;
using PortalGraduados.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Conexión con PostgreSQL mediante Entity Framework Core
builder.Services.AddDbContext<PortalGraduadosDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PortalGraduados")
    )
);

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var app = builder.Build();

// Configuración del entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
