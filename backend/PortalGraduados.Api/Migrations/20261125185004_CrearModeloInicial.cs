using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PortalGraduados.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearModeloInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "habilidad",
                columns: table => new
                {
                    id_habilidad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_habilidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habilidad", x => x.id_habilidad);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contrasena_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    estado_cuenta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_rol_id_rol",
                        column: x => x.id_rol,
                        principalTable: "rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carga_perfiles",
                columns: table => new
                {
                    id_carga = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    fecha_carga = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_registros = table.Column<int>(type: "integer", nullable: false),
                    total_cargados = table.Column<int>(type: "integer", nullable: false),
                    total_rechazados = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carga_perfiles", x => x.id_carga);
                    table.ForeignKey(
                        name: "FK_carga_perfiles_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    nombre_comercial = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    razon_social = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    telefono_contacto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado_aprobacion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.id_empresa);
                    table.ForeignKey(
                        name: "FK_empresa_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "graduado",
                columns: table => new
                {
                    id_graduado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    anio_graduacion = table.Column<int>(type: "integer", nullable: false),
                    estado_academico = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado_laboral = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    area_trabajo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cv_adjunto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    portafolio_enlace = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    verificado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    consentimiento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_ultima_actualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graduado", x => x.id_graduado);
                    table.ForeignKey(
                        name: "FK_graduado_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    id_notificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    leida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK_notificacion_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "oferta",
                columns: table => new
                {
                    id_oferta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    cargo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion_corta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    requisitos = table.Column<string>(type: "text", nullable: false),
                    rango_salarial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    modalidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    clasificacion_oportunidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oferta", x => x.id_oferta);
                    table.ForeignKey(
                        name: "FK_oferta_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresa",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "experiencia_laboral",
                columns: table => new
                {
                    id_experiencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_graduado = table.Column<int>(type: "integer", nullable: false),
                    empresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experiencia_laboral", x => x.id_experiencia);
                    table.ForeignKey(
                        name: "FK_experiencia_laboral_graduado_id_graduado",
                        column: x => x.id_graduado,
                        principalTable: "graduado",
                        principalColumn: "id_graduado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "graduado_habilidad",
                columns: table => new
                {
                    id_graduado = table.Column<int>(type: "integer", nullable: false),
                    id_habilidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graduado_habilidad", x => new { x.id_graduado, x.id_habilidad });
                    table.ForeignKey(
                        name: "FK_graduado_habilidad_graduado_id_graduado",
                        column: x => x.id_graduado,
                        principalTable: "graduado",
                        principalColumn: "id_graduado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_graduado_habilidad_habilidad_id_habilidad",
                        column: x => x.id_habilidad,
                        principalTable: "habilidad",
                        principalColumn: "id_habilidad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proyecto",
                columns: table => new
                {
                    id_proyecto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_graduado = table.Column<int>(type: "integer", nullable: false),
                    nombre_proyecto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    tecnologias_utilizadas = table.Column<string>(type: "text", nullable: true),
                    url_proyecto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyecto", x => x.id_proyecto);
                    table.ForeignKey(
                        name: "FK_proyecto_graduado_id_graduado",
                        column: x => x.id_graduado,
                        principalTable: "graduado",
                        principalColumn: "id_graduado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "oferta_habilidad",
                columns: table => new
                {
                    id_oferta = table.Column<int>(type: "integer", nullable: false),
                    id_habilidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oferta_habilidad", x => new { x.id_oferta, x.id_habilidad });
                    table.ForeignKey(
                        name: "FK_oferta_habilidad_habilidad_id_habilidad",
                        column: x => x.id_habilidad,
                        principalTable: "habilidad",
                        principalColumn: "id_habilidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_oferta_habilidad_oferta_id_oferta",
                        column: x => x.id_oferta,
                        principalTable: "oferta",
                        principalColumn: "id_oferta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "postulacion",
                columns: table => new
                {
                    id_postulacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_graduado = table.Column<int>(type: "integer", nullable: false),
                    id_oferta = table.Column<int>(type: "integer", nullable: false),
                    fecha_postulacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    cv_enviado = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postulacion", x => x.id_postulacion);
                    table.ForeignKey(
                        name: "FK_postulacion_graduado_id_graduado",
                        column: x => x.id_graduado,
                        principalTable: "graduado",
                        principalColumn: "id_graduado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_postulacion_oferta_id_oferta",
                        column: x => x.id_oferta,
                        principalTable: "oferta",
                        principalColumn: "id_oferta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reporte_oferta",
                columns: table => new
                {
                    id_reporte = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_oferta = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    fecha_reporte = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reporte_oferta", x => x.id_reporte);
                    table.ForeignKey(
                        name: "FK_reporte_oferta_oferta_id_oferta",
                        column: x => x.id_oferta,
                        principalTable: "oferta",
                        principalColumn: "id_oferta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reporte_oferta_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_carga_perfiles_id_usuario",
                table: "carga_perfiles",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_empresa_id_usuario",
                table: "empresa",
                column: "id_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_experiencia_laboral_id_graduado",
                table: "experiencia_laboral",
                column: "id_graduado");

            migrationBuilder.CreateIndex(
                name: "IX_graduado_id_usuario",
                table: "graduado",
                column: "id_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_graduado_habilidad_id_habilidad",
                table: "graduado_habilidad",
                column: "id_habilidad");

            migrationBuilder.CreateIndex(
                name: "IX_habilidad_nombre_habilidad",
                table: "habilidad",
                column: "nombre_habilidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_id_usuario",
                table: "notificacion",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_oferta_id_empresa",
                table: "oferta",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_oferta_habilidad_id_habilidad",
                table: "oferta_habilidad",
                column: "id_habilidad");

            migrationBuilder.CreateIndex(
                name: "IX_postulacion_id_graduado_id_oferta",
                table: "postulacion",
                columns: new[] { "id_graduado", "id_oferta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_postulacion_id_oferta",
                table: "postulacion",
                column: "id_oferta");

            migrationBuilder.CreateIndex(
                name: "IX_proyecto_id_graduado",
                table: "proyecto",
                column: "id_graduado");

            migrationBuilder.CreateIndex(
                name: "IX_reporte_oferta_id_oferta",
                table: "reporte_oferta",
                column: "id_oferta");

            migrationBuilder.CreateIndex(
                name: "IX_reporte_oferta_id_usuario",
                table: "reporte_oferta",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_correo",
                table: "usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_rol",
                table: "usuario",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carga_perfiles");

            migrationBuilder.DropTable(
                name: "experiencia_laboral");

            migrationBuilder.DropTable(
                name: "graduado_habilidad");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "oferta_habilidad");

            migrationBuilder.DropTable(
                name: "postulacion");

            migrationBuilder.DropTable(
                name: "proyecto");

            migrationBuilder.DropTable(
                name: "reporte_oferta");

            migrationBuilder.DropTable(
                name: "habilidad");

            migrationBuilder.DropTable(
                name: "graduado");

            migrationBuilder.DropTable(
                name: "oferta");

            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "rol");
        }
    }
}
