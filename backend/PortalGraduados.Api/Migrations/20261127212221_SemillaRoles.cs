using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalGraduados.Api.Migrations
{
    /// <inheritdoc />
    public partial class SemillaRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renombra el rol generico de administrador si aun no existe el nombre oficial
            migrationBuilder.Sql(@"
                UPDATE rol
                SET nombre_rol = 'Administrador de la Escuela',
                    descripcion = 'Gestiona cuentas, ofertas, reportes y estadisticas'
                WHERE nombre_rol = 'Administrador'
                  AND NOT EXISTS (
                      SELECT 1 FROM rol r WHERE r.nombre_rol = 'Administrador de la Escuela'
                  );
            ");

            // Inserta solo los roles que falten, sin depender de IDs fijos
            migrationBuilder.Sql(@"
                INSERT INTO rol (nombre_rol, descripcion)
                SELECT v.nombre_rol, v.descripcion
                FROM (VALUES
                    ('Graduado', 'Egresado que mantiene su perfil y postula a ofertas'),
                    ('Empresa', 'Empresa aliada que publica ofertas y revisa candidaturas'),
                    ('Administrador de la Escuela', 'Gestiona cuentas, ofertas, reportes y estadisticas')
                ) AS v(nombre_rol, descripcion)
                WHERE NOT EXISTS (
                    SELECT 1 FROM rol r WHERE r.nombre_rol = v.nombre_rol
                );
            ");

            // Sincroniza la secuencia de identidad con el maximo id existente
            migrationBuilder.Sql(@"
                SELECT setval(
                    pg_get_serial_sequence('rol', 'id_rol'),
                    COALESCE((SELECT MAX(id_rol) FROM rol), 1)
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
