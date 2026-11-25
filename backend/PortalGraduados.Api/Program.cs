
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Data;

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
