
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortalGraduados.Api.Data;
using PortalGraduados.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICIOS GENERALES
// ==========================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ==========================================
// 2. CONEXIÓN CON POSTGRESQL
// ==========================================

builder.Services.AddDbContext<PortalGraduadosDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "PortalGraduados"
        )
    )
);

builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>
>();

// ==========================================
// 3. CONFIGURACIÓN JWT
// ==========================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Debe configurar una clave JWT de al menos 32 bytes."
    );
}

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"] ?? "PortalGraduados.Api";

var jwtAudience =
    builder.Configuration["Jwt:Audience"] ?? "PortalGraduados.Angular";

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme
)
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ValidAlgorithms = new[]
            {
                SecurityAlgorithms.HmacSha256
            },

            NameClaimType = "email",
            RoleClaimType = "role",

            ClockSkew = TimeSpan.FromSeconds(30)
        };

    // Comprobar que el usuario y su rol
    // siguen siendo válidos en PostgreSQL.

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;

            if (!int.TryParse(
                principal?.FindFirst("sub")?.Value,
                out int idUsuario))
            {
                context.Fail("Identificador inválido.");
                return;
            }

            var rolToken =
                principal?.FindFirst("role")?.Value;

            var db = context.HttpContext.RequestServices
                .GetRequiredService<PortalGraduadosDbContext>();

            var usuario = await db.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario
                );

            if (usuario is null ||
                !usuario.EstadoCuenta ||
                usuario.Rol.NombreRol != rolToken)
            {
                context.Fail("Cuenta o rol no válido.");
                return;
            }

            if (rolToken == "Graduado")
            {
                var habilitado = await db.Graduados
                    .AnyAsync(g =>
                        g.IdUsuario == idUsuario &&
                        g.Verificado &&
                        g.Consentimiento
                    );

                if (!habilitado)
                {
                    context.Fail("Perfil no habilitado.");
                }
            }

            if (rolToken == "Empresa")
            {
                var habilitada = await db.Empresas
                    .AnyAsync(e =>
                        e.IdUsuario == idUsuario &&
                        e.EstadoAprobacion
                    );

                if (!habilitada)
                {
                    context.Fail("Empresa no habilitada.");
                }
            }
        }
    };
});

// ==========================================
// 4. AUTORIZACIÓN POR ROLES
// ==========================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administracion", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Administrador de la Escuela")
    );

    options.AddPolicy("Graduados", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Graduado")
    );

    options.AddPolicy("Empresas", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Empresa")
    );

    // Política provisional segura:
    // Los controladores existentes permanecen
    // restringidos al administrador hasta que
    // implementemos sus permisos individuales.

    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole("Administrador de la Escuela")
            .Build();
});

// ==========================================
// 5. CORS PARA ANGULAR
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Total-Count");
    });
});

// ==========================================
// 6. LIMITAR INTENTOS DE INICIO DE SESIÓN
// ==========================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString()
                ?? "desconocido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }
        )
    );
});

var app = builder.Build();

// ==========================================
// 7. MIGRACIONES Y ADMINISTRADOR INICIAL
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<PortalGraduadosDbContext>();

    await db.Database.MigrateAsync();

    // Crear administrador inicial únicamente
    // durante el desarrollo local.

    if (app.Environment.IsDevelopment())
    {
        var correoAdmin = builder.Configuration[
            "AdminBootstrap:Email"
        ]?.Trim().ToLowerInvariant();

        var claveAdmin = builder.Configuration[
            "AdminBootstrap:Password"
        ];

        if (!string.IsNullOrWhiteSpace(correoAdmin) &&
            !string.IsNullOrWhiteSpace(claveAdmin))
        {
            var existeAdministrador = await db.Usuarios
                .AnyAsync(u =>
                    u.Rol.NombreRol ==
                    "Administrador de la Escuela"
                );

            if (!existeAdministrador)
            {
                if (!new EmailAddressAttribute()
                        .IsValid(correoAdmin) ||
                    correoAdmin.Length > 150 ||
                    claveAdmin.Length < 12 ||
                    claveAdmin.Length > 128)
                {
                    throw new InvalidOperationException(
                        "Correo o contraseña inicial inválidos."
                    );
                }

                var rol = await db.Roles
                    .SingleAsync(r =>
                        r.NombreRol ==
                        "Administrador de la Escuela"
                    );

                if (await db.Usuarios.AnyAsync(
                    u => u.Correo == correoAdmin))
                {
                    throw new InvalidOperationException(
                        "El correo inicial ya está registrado."
                    );
                }

                var hasher = scope.ServiceProvider
                    .GetRequiredService<
                        IPasswordHasher<Usuario>
                    >();

                var administrador = new Usuario
                {
                    Correo = correoAdmin,
                    IdRol = rol.IdRol,
                    EstadoCuenta = true
                };

                administrador.ContrasenaHash =
                    hasher.HashPassword(
                        administrador,
                        claveAdmin
                    );

                db.Usuarios.Add(administrador);

                await db.SaveChangesAsync();

                app.Logger.LogInformation(
                    "Administrador inicial creado."
                );
            }
        }
    }
}

// ==========================================
// 8. ENTORNO DE EJECUCIÓN
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
       .RequireAuthorization("Administracion");
}
else
{
    app.UseHttpsRedirection();
}

// ==========================================
// 9. MIDDLEWARE
// ==========================================

app.UseRouting();

app.UseCors("Angular");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
