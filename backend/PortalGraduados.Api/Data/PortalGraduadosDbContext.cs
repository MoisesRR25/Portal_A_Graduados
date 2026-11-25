
using Microsoft.EntityFrameworkCore;
using PortalGraduados.Api.Models;

namespace PortalGraduados.Api.Data
{
    public class PortalGraduadosDbContext : DbContext
    {
        public PortalGraduadosDbContext(
            DbContextOptions<PortalGraduadosDbContext> options
        ) : base(options)
        {
        }

        // Entidades de la base de datos
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Graduado> Graduados => Set<Graduado>();
        public DbSet<ExperienciaLaboral> ExperienciasLaborales => Set<ExperienciaLaboral>();
        public DbSet<Proyecto> Proyectos => Set<Proyecto>();
        public DbSet<Habilidad> Habilidades => Set<Habilidad>();
        public DbSet<GraduadoHabilidad> GraduadoHabilidades => Set<GraduadoHabilidad>();
        public DbSet<Empresa> Empresas => Set<Empresa>();
        public DbSet<Oferta> Ofertas => Set<Oferta>();
        public DbSet<OfertaHabilidad> OfertaHabilidades => Set<OfertaHabilidad>();
        public DbSet<Postulacion> Postulaciones => Set<Postulacion>();
        public DbSet<ReporteOferta> ReportesOferta => Set<ReporteOferta>();
        public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
        public DbSet<CargaPerfiles> CargasPerfiles => Set<CargaPerfiles>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // TABLA 1: ROL
            // ========================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("rol");

                entity.HasKey(r => r.IdRol);

                entity.Property(r => r.IdRol)
                    .HasColumnName("id_rol");

                entity.Property(r => r.NombreRol)
                    .HasColumnName("nombre_rol")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(r => r.Descripcion)
                    .HasColumnName("descripcion")
                    .HasMaxLength(100);
            });

            // ========================================
            // TABLA 2: USUARIO
            // ========================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario");

                entity.HasKey(u => u.IdUsuario);

                entity.Property(u => u.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(u => u.IdRol)
                    .HasColumnName("id_rol");

                entity.Property(u => u.Correo)
                    .HasColumnName("correo")
                    .HasMaxLength(150)
                    .IsRequired();

                // Correo único
                entity.HasIndex(u => u.Correo)
                    .IsUnique();

                entity.Property(u => u.ContrasenaHash)
                    .HasColumnName("contrasena_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.EstadoCuenta)
                    .HasColumnName("estado_cuenta")
                    .IsRequired();

                // Relación Usuario -> Rol
                entity.HasOne(u => u.Rol)
                    .WithMany()
                    .HasForeignKey(u => u.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 3: GRADUADO
            // ========================================
            modelBuilder.Entity<Graduado>(entity =>
            {
                entity.ToTable("graduado");

                entity.HasKey(g => g.IdGraduado);

                entity.Property(g => g.IdGraduado)
                    .HasColumnName("id_graduado");

                entity.Property(g => g.IdUsuario)
                    .HasColumnName("id_usuario");

                // Datos personales
                entity.Property(g => g.Nombres)
                    .HasColumnName("nombres")
                    .HasMaxLength(100);

                entity.Property(g => g.Apellidos)
                    .HasColumnName("apellidos")
                    .HasMaxLength(100);

                entity.Property(g => g.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(20);

                // Información académica
                entity.Property(g => g.AnioGraduacion)
                    .HasColumnName("anio_graduacion");

                entity.Property(g => g.EstadoAcademico)
                    .HasColumnName("estado_academico")
                    .HasMaxLength(30);

                // Información laboral
                entity.Property(g => g.EstadoLaboral)
                    .HasColumnName("estado_laboral")
                    .HasMaxLength(30);

                entity.Property(g => g.AreaTrabajo)
                    .HasColumnName("area_trabajo")
                    .HasMaxLength(100);

                entity.Property(g => g.Ubicacion)
                    .HasColumnName("ubicacion")
                    .HasMaxLength(100);

                // Documentos
                entity.Property(g => g.CvAdjunto)
                    .HasColumnName("cv_adjunto")
                    .HasMaxLength(255);

                entity.Property(g => g.PortafolioEnlace)
                    .HasColumnName("portafolio_enlace")
                    .HasMaxLength(255);

                // Verificación
                entity.Property(g => g.Verificado)
                    .HasColumnName("verificado")
                    .HasDefaultValue(false);

                entity.Property(g => g.Consentimiento)
                    .HasColumnName("consentimiento")
                    .HasDefaultValue(false);

                entity.Property(g => g.FechaUltimaActualizacion)
                    .HasColumnName("fecha_ultima_actualizacion")
                    .HasColumnType("timestamp without time zone");

                // Relación uno a uno con Usuario
                entity.HasOne(g => g.Usuario)
                    .WithOne()
                    .HasForeignKey<Graduado>(g => g.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 4: EXPERIENCIA_LABORAL
            // ========================================
            modelBuilder.Entity<ExperienciaLaboral>(entity =>
            {
                entity.ToTable("experiencia_laboral");

                entity.HasKey(e => e.IdExperiencia);

                entity.Property(e => e.IdExperiencia)
                    .HasColumnName("id_experiencia");

                entity.Property(e => e.IdGraduado)
                    .HasColumnName("id_graduado");

                // Información laboral
                entity.Property(e => e.Empresa)
                    .HasColumnName("empresa")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Cargo)
                    .HasColumnName("cargo")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasColumnName("descripcion")
                    .HasColumnType("text");

                // Fechas
                entity.Property(e => e.FechaInicio)
                    .HasColumnName("fecha_inicio")
                    .HasColumnType("date");

                entity.Property(e => e.FechaFin)
                    .HasColumnName("fecha_fin")
                    .HasColumnType("date");

                // Relación con Graduado
                entity.HasOne(e => e.Graduado)
                    .WithMany()
                    .HasForeignKey(e => e.IdGraduado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 5: PROYECTO
            // ========================================
            modelBuilder.Entity<Proyecto>(entity =>
            {
                entity.ToTable("proyecto");

                entity.HasKey(p => p.IdProyecto);

                entity.Property(p => p.IdProyecto)
                    .HasColumnName("id_proyecto");

                entity.Property(p => p.IdGraduado)
                    .HasColumnName("id_graduado");

                // Información del proyecto
                entity.Property(p => p.NombreProyecto)
                    .HasColumnName("nombre_proyecto")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(p => p.Descripcion)
                    .HasColumnName("descripcion")
                    .HasColumnType("text");

                entity.Property(p => p.TecnologiasUtilizadas)
                    .HasColumnName("tecnologias_utilizadas")
                    .HasColumnType("text");

                entity.Property(p => p.UrlProyecto)
                    .HasColumnName("url_proyecto")
                    .HasMaxLength(255);

                // Relación con Graduado
                entity.HasOne(p => p.Graduado)
                    .WithMany()
                    .HasForeignKey(p => p.IdGraduado)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 6: HABILIDAD
            // ========================================
            modelBuilder.Entity<Habilidad>(entity =>
            {
                entity.ToTable("habilidad");

                entity.HasKey(h => h.IdHabilidad);

                entity.Property(h => h.IdHabilidad)
                    .HasColumnName("id_habilidad");

                entity.Property(h => h.NombreHabilidad)
                    .HasColumnName("nombre_habilidad")
                    .HasMaxLength(100)
                    .IsRequired();

                // Nombre único
                entity.HasIndex(h => h.NombreHabilidad)
                    .IsUnique();

                entity.Property(h => h.Categoria)
                    .HasColumnName("categoria")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // ========================================
            // TABLA 7: GRADUADO_HABILIDAD
            // ========================================
            modelBuilder.Entity<GraduadoHabilidad>(entity =>
            {
                entity.ToTable("graduado_habilidad");

                // Clave primaria compuesta
                entity.HasKey(gh => new
                {
                    gh.IdGraduado,
                    gh.IdHabilidad
                });

                entity.Property(gh => gh.IdGraduado)
                    .HasColumnName("id_graduado");

                entity.Property(gh => gh.IdHabilidad)
                    .HasColumnName("id_habilidad");

                // Relación con Graduado
                entity.HasOne(gh => gh.Graduado)
                    .WithMany()
                    .HasForeignKey(gh => gh.IdGraduado)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Habilidad
                entity.HasOne(gh => gh.Habilidad)
                    .WithMany()
                    .HasForeignKey(gh => gh.IdHabilidad)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 8: EMPRESA
            // ========================================
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("empresa");

                entity.HasKey(e => e.IdEmpresa);

                entity.Property(e => e.IdEmpresa)
                    .HasColumnName("id_empresa");

                entity.Property(e => e.IdUsuario)
                    .HasColumnName("id_usuario");

                // Información de la empresa
                entity.Property(e => e.NombreComercial)
                    .HasColumnName("nombre_comercial")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.RazonSocial)
                    .HasColumnName("razon_social")
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(e => e.TelefonoContacto)
                    .HasColumnName("telefono_contacto")
                    .HasMaxLength(20)
                    .IsRequired();

                // Estado de aprobación
                entity.Property(e => e.EstadoAprobacion)
                    .HasColumnName("estado_aprobacion")
                    .HasDefaultValue(false)
                    .IsRequired();

                // Relación uno a uno con Usuario
                entity.HasOne(e => e.Usuario)
                    .WithOne()
                    .HasForeignKey<Empresa>(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.IdUsuario)
                    .IsUnique();
            });

            // ========================================
            // TABLA 9: OFERTA
            // ========================================
            modelBuilder.Entity<Oferta>(entity =>
            {
                entity.ToTable("oferta");

                entity.HasKey(o => o.IdOferta);

                entity.Property(o => o.IdOferta)
                    .HasColumnName("id_oferta");

                entity.Property(o => o.IdEmpresa)
                    .HasColumnName("id_empresa");

                // Información de la oferta
                entity.Property(o => o.Cargo)
                    .HasColumnName("cargo")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(o => o.Area)
                    .HasColumnName("area")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.Ubicacion)
                    .HasColumnName("ubicacion")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.DescripcionCorta)
                    .HasColumnName("descripcion_corta")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.Descripcion)
                    .HasColumnName("descripcion")
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(o => o.Requisitos)
                    .HasColumnName("requisitos")
                    .HasColumnType("text")
                    .IsRequired();

                // Condiciones de la oferta
                entity.Property(o => o.RangoSalarial)
                    .HasColumnName("rango_salarial")
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(o => o.Modalidad)
                    .HasColumnName("modalidad")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(o => o.ClasificacionOportunidad)
                    .HasColumnName("clasificacion_oportunidad")
                    .HasMaxLength(30)
                    .IsRequired();

                // Fechas de publicación y cierre
                entity.Property(o => o.FechaPublicacion)
                    .HasColumnName("fecha_publicacion")
                    .HasColumnType("timestamp without time zone");

                entity.Property(o => o.FechaCierre)
                    .HasColumnName("fecha_cierre")
                    .HasColumnType("timestamp without time zone");

                // Estado de la oferta
                entity.Property(o => o.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(20)
                    .IsRequired();

                // Relación muchos a uno con Empresa
                entity.HasOne(o => o.Empresa)
                    .WithMany()
                    .HasForeignKey(o => o.IdEmpresa)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 10: OFERTA_HABILIDAD
            // ========================================
            modelBuilder.Entity<OfertaHabilidad>(entity =>
            {
                entity.ToTable("oferta_habilidad");

                // Clave primaria compuesta
                entity.HasKey(oh => new
                {
                    oh.IdOferta,
                    oh.IdHabilidad
                });

                entity.Property(oh => oh.IdOferta)
                    .HasColumnName("id_oferta");

                entity.Property(oh => oh.IdHabilidad)
                    .HasColumnName("id_habilidad");

                // Relación con Oferta
                entity.HasOne(oh => oh.Oferta)
                    .WithMany()
                    .HasForeignKey(oh => oh.IdOferta)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Habilidad
                entity.HasOne(oh => oh.Habilidad)
                    .WithMany()
                    .HasForeignKey(oh => oh.IdHabilidad)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 11: POSTULACION
            // ========================================
            modelBuilder.Entity<Postulacion>(entity =>
            {
                entity.ToTable("postulacion");

                // Clave primaria
                entity.HasKey(p => p.IdPostulacion);

                entity.Property(p => p.IdPostulacion)
                    .HasColumnName("id_postulacion");

                // Claves foráneas
                entity.Property(p => p.IdGraduado)
                    .HasColumnName("id_graduado");

                entity.Property(p => p.IdOferta)
                    .HasColumnName("id_oferta");

                // Fecha de postulación
                entity.Property(p => p.FechaPostulacion)
                    .HasColumnName("fecha_postulacion")
                    .HasColumnType("timestamp without time zone");

                // Currículum enviado
                entity.Property(p => p.CvEnviado)
                    .HasColumnName("cv_enviado")
                    .HasMaxLength(255)
                    .IsRequired();

                // Estado de la postulación
                entity.Property(p => p.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(20)
                    .IsRequired();

                // Una postulación por graduado y oferta
                entity.HasIndex(p => new
                {
                    p.IdGraduado,
                    p.IdOferta
                }).IsUnique();

                // Relación con Graduado
                entity.HasOne(p => p.Graduado)
                    .WithMany()
                    .HasForeignKey(p => p.IdGraduado)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Oferta
                entity.HasOne(p => p.Oferta)
                    .WithMany()
                    .HasForeignKey(p => p.IdOferta)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 12: REPORTE_OFERTA
            // ========================================
            modelBuilder.Entity<ReporteOferta>(entity =>
            {
                entity.ToTable("reporte_oferta");

                // Clave primaria
                entity.HasKey(r => r.IdReporte);

                entity.Property(r => r.IdReporte)
                    .HasColumnName("id_reporte");

                // Claves foráneas
                entity.Property(r => r.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(r => r.IdOferta)
                    .HasColumnName("id_oferta");

                // Motivo del reporte
                entity.Property(r => r.Motivo)
                    .HasColumnName("motivo")
                    .HasColumnType("text")
                    .IsRequired();

                // Fecha del reporte
                entity.Property(r => r.FechaReporte)
                    .HasColumnName("fecha_reporte")
                    .HasColumnType("timestamp without time zone");

                // Relación con Usuario
                entity.HasOne(r => r.Usuario)
                    .WithMany()
                    .HasForeignKey(r => r.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Oferta
                entity.HasOne(r => r.Oferta)
                    .WithMany()
                    .HasForeignKey(r => r.IdOferta)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 13: NOTIFICACION
            // ========================================
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("notificacion");

                // Clave primaria
                entity.HasKey(n => n.IdNotificacion);

                entity.Property(n => n.IdNotificacion)
                    .HasColumnName("id_notificacion");

                // Clave foránea hacia Usuario
                entity.Property(n => n.IdUsuario)
                    .HasColumnName("id_usuario");

                // Tipo de notificación
                entity.Property(n => n.Tipo)
                    .HasColumnName("tipo")
                    .HasMaxLength(30)
                    .IsRequired();

                // Mensaje de la notificación
                entity.Property(n => n.Mensaje)
                    .HasColumnName("mensaje")
                    .HasColumnType("text")
                    .IsRequired();

                // Fecha de la notificación
                entity.Property(n => n.Fecha)
                    .HasColumnName("fecha")
                    .HasColumnType("timestamp without time zone");

                // Estado de lectura
                entity.Property(n => n.Leida)
                    .HasColumnName("leida")
                    .HasDefaultValue(false)
                    .IsRequired();

                // Relación muchos a uno con Usuario
                entity.HasOne(n => n.Usuario)
                    .WithMany()
                    .HasForeignKey(n => n.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // TABLA 14: CARGA_PERFILES
            // ========================================
            modelBuilder.Entity<CargaPerfiles>(entity =>
            {
                entity.ToTable("carga_perfiles");

                // Clave primaria
                entity.HasKey(c => c.IdCarga);

                entity.Property(c => c.IdCarga)
                    .HasColumnName("id_carga");

                // Clave foránea hacia Usuario
                entity.Property(c => c.IdUsuario)
                    .HasColumnName("id_usuario");

                // Fecha de la carga masiva
                entity.Property(c => c.FechaCarga)
                    .HasColumnName("fecha_carga")
                    .HasColumnType("timestamp without time zone");

                // Cantidad total de registros procesados
                entity.Property(c => c.TotalRegistros)
                    .HasColumnName("total_registros")
                    .IsRequired();

                // Registros cargados correctamente
                entity.Property(c => c.TotalCargados)
                    .HasColumnName("total_cargados")
                    .IsRequired();

                // Registros rechazados
                entity.Property(c => c.TotalRechazados)
                    .HasColumnName("total_rechazados")
                    .IsRequired();

                // Relación muchos a uno con Usuario
                entity.HasOne(c => c.Usuario)
                    .WithMany()
                    .HasForeignKey(c => c.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
