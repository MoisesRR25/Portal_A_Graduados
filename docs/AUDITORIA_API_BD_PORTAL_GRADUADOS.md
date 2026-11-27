# Auditoría de API y Base de Datos — Portal de Seguimiento a Graduados

**Fecha:** 27 de noviembre de 2026  
**Repositorio:** `github.com/MoisesRR25/Portal_A_Graduados`  
**Rama revisada:** `feature/endpoints-api`  
**Último commit revisado:** `6b90a45` — “Ampliar pruebas de postulaciones: oferta en revision y ajustes”  
**Alcance:** backend ASP.NET Core 10, Entity Framework Core, PostgreSQL 18, Docker Compose y scripts SQL.  
**Fuentes revisadas:** PDF de requerimientos “Portal de Seguimiento a Graduados y Bolsa de Trabajo — Especificación de requisitos de software, entrega final”; PDF “Guía de Entrega y Continuidad” del 25/11/2026; código en `backend/`, `database/` e `infra/`.

> Nota de verificación: el proyecto compila con `dotnet build` (0 errores, 0 advertencias). El 27/11/2026 se inició Docker Desktop, se aplicó la migración `20261127212221_SemillaRoles` y se verificaron los roles `Administrador de la Escuela`, `Graduado` y `Empresa` en la base local.

Orden recomendado
1. ✅ Semilla de roles + migración aplicada + cadena de conexión local (completado).
2. Unificar estados de oferta y corregir scripts B4.
3. Implementar autenticación JWT y autorización por roles.
4. Aplicar ownership y privacidad en todos los controladores.
5. Implementar endpoints faltantes: reportes, notificaciones, cargas, panel, verificación y matching.
6. Agregar índices/constraints y una migración nueva.
7. Swagger, pruebas y documentación.


## 1. Resumen ejecutivo

El proyecto avanzó considerablemente respecto al punto de entrega descrito en la guía. Ya existen las 14 entidades, el `DbContext`, la migración inicial, los DTOs y los controladores CRUD principales. La base del modelo relacional y la lógica de ofertas/postulaciones está implementada.

Sin embargo, el apartado de API y base de datos **todavía no está listo para entrega** porque faltan dos bloques críticos, aunque el de datos semilla ya quedó resuelto:

1. **Datos semilla y aplicación de migraciones:** resuelto con la migración idempotente `SemillaRoles`, la aplicación automática en `Program.cs` y la cadena de conexión local en user-secrets.
2. **Autenticación, autorización y privacidad:** no existe login ni control de acceso por rol; varios endpoints exponen datos personales o permiten operaciones sin validar propiedad.
3. **Requisitos funcionales incompletos:** faltan endpoints de reportes, notificaciones, cargas masivas, panel de estadísticas, verificación de identidad y matching.

A esto se suman correcciones de reglas de negocio, índices/restricciones en PostgreSQL, inconsistencias en los scripts B4 y falta de documentación/pruebas.

---

## 2. Estado por fase de la guía

| Fase de la guía | Estado | Observación |
|---|---|---|
| Fase A — Revisar y congelar el modelo relacional | Completada | Las 14 tablas coinciden con el diccionario de datos del SRS. |
| Fase B — Crear entidades C# y `DbContext` | Completada | Entidades, `DbSet`, PK/FK, longitudes y relaciones configuradas. |
| Fase C — Crear migraciones y las 14 tablas | Completada | Migración inicial, semilla idempotente de roles y aplicación automática al arrancar la API. |
| Fase D — Desarrollar endpoints REST | Parcial | CRUD principal implementado; faltan RF-09, RF-10, RF-12, RF-13, RF-16, RF-17, RF-18, RF-20 y RF-22. |
| Fase E — Seguridad e integración Angular | Pendiente | No hay autenticación, autorización, CORS ni consumo desde Angular. |
| Fase F — Pruebas, documentación y entrega | Pendiente | No hay proyecto de pruebas, Swagger UI ni README de la API. |

---

## 3. Lo que ya está implementado

### 3.1 Base de datos

- Migración `20261125185004_CrearModeloInicial` con las 14 tablas:
  `rol`, `usuario`, `graduado`, `experiencia_laboral`, `proyecto`, `habilidad`, `graduado_habilidad`, `empresa`, `oferta`, `oferta_habilidad`, `postulacion`, `reporte_oferta`, `notificacion`, `carga_perfiles`.
- Claves primarias, claves foráneas, longitudes y relaciones uno a uno y muchos a muchos.
- Índices únicos en `usuario.correo`, `graduado.id_usuario`, `empresa.id_usuario`, `habilidad.nombre_habilidad` y `postulacion(id_graduado, id_oferta)`.
- Columnas de fecha como `timestamp without time zone` y fechas de experiencia/proyecto como `date`.

### 3.2 API

- Controladores funcionales:
  - `RolesController`
  - `HabilidadesController`
  - `GraduadosController`
  - `EmpresasController`
  - `ExperienciasController`
  - `ProyectosController`
  - `OfertasController`
  - `PostulacionesController`
- Endpoints de salud: `GET /health` y `GET /health/database`.
- DTOs con validaciones (`DataAnnotations`), paginación y encabezado `X-Total-Count`.
- Transacciones para registro de graduado/empresa y manejo de violaciones de unicidad.
- Reglas implementadas:
  - Oferta sin rango salarial o primera oferta pasa a `En revision`.
  - Aprobación/rechazo de ofertas con notificación a la empresa.
  - Cierre manual de ofertas.
  - Postulación bloqueada si el perfil no está verificado, la oferta no está publicada o ya venció, o si existe postulación duplicada.
  - Transiciones de estado de postulación.

---

## 4. Bloqueantes para arrancar desde cero

| # | Problema | Evidencia | Acción requerida |
|---|---|---|---|
| 1 | No hay datos semilla de roles. | La migración no contiene `InsertData`; los controladores buscan `Graduado` y `Empresa`. | Agregar `HasData` o script SQL para `Graduado`, `Empresa` y `Administrador de la Escuela`. |
| 2 | La migración no se aplica automáticamente ni está documentado el comando. | `Program.cs` no ejecuta `Database.Migrate()`. | Agregar migración al arranque o documentar `dotnet ef database update`. |
| 3 | No hay cadena de conexión para ejecución local. | `appsettings.json` y `appsettings.Development.json` no tienen `ConnectionStrings`. | Configurar `127.0.0.1:5438` en user-secrets o `appsettings.Development.json`. |
| 4 | El servicio `api` no depende de `postgres`. | `infra/docker-compose.yml`. | Agregar `depends_on` con `condition: service_healthy`. |
| 5 | `UseHttpsRedirection()` está activo, pero Docker solo expone HTTP 8080. | `Program.cs:30`. | Configurar HTTPS real o quitar la redirección en Development. |
| 6 | No hay `.env.example`. | `infra/` solo contiene `.env` local. | Agregar plantilla sin credenciales. |

---

## 5. Seguridad y autorización

`Program.cs` solo registra `UseAuthorization()`; no existe `AddAuthentication`, login, JWT ni `UseAuthentication`. Por tanto, **RF-11, RF-15, RNF-02 y RNF-03 no están cubiertos**.

### 5.1 Falta implementar

- `AuthController` con `POST /api/auth/login` y `GET /api/auth/me`.
- Autenticación JWT (o cookie) y servicio de generación de tokens.
- Verificación de contraseña con `IPasswordHasher<Usuario>.VerifyHashedPassword`.
- Validación de `Usuario.EstadoCuenta` al iniciar sesión.
- `[Authorize]` y autorización por roles en todos los controladores.
- Validación de propiedad de recursos:
  - Empresa solo modifica/cierra sus propias ofertas (RF-06, RF-07, RN-04).
  - Empresa solo ve sus propias candidaturas (RF-20).
  - Graduado solo ve/modifica su perfil y sus postulaciones (RF-19).
  - Administrador único para roles, habilidades, aprobación de empresas, verificación, reportes, panel y cargas.
- CORS para el origen del frontend Angular.
- Manejo de errores con `ProblemDetails` y sin exponer detalles internos.

### 5.2 Correcciones de privacidad

- `GET /api/graduados` devuelve teléfono, correo, CV y portafolio a cualquier usuario (`GraduadosController.cs:27-45,49`). Las empresas solo deben ver nombre, área, habilidades y ubicación (RF-10, RNF-03).
- `GET /api/ofertas` permite consultar ofertas `En revision` o `Rechazada` si se envía `estado`, y por defecto incluye `Cerrada` (`OfertasController.cs:69-70`). Debe restringirse por rol y estado.
- `PUT /api/graduados/{id}/verificacion` y `PUT /api/empresas/{id}/aprobacion` no tienen autorización.
- El registro de graduado establece `EstadoCuenta = true` antes de verificar (`GraduadosController.cs:139`), lo que contradice RN-05. Debe quedar inactivo hasta verificar contacto y consentimiento.
- El registro de empresa también establece `EstadoCuenta = true` (`EmpresasController.cs:95`); debe validarse junto con `EstadoAprobacion`.

---

## 6. Requisitos funcionales: estado y faltantes

| Requisito | Estado | Qué falta |
|---|---|---|
| RF-01 Registrar perfil profesional | Implementado | Requiere semilla del rol `Graduado`. |
| RF-02 Actualizar perfil, proyectos, habilidades y portafolio | Implementado | Sin observaciones críticas. |
| RF-03 Validar formularios | Parcial | Agregar valores permitidos para `ClasificacionOportunidad` y estados. |
| RF-04 Fecha de última actualización | Implementado | — |
| RF-05 Publicar ofertas | Implementado | Validar clasificación y estado de cuenta del usuario. |
| RF-06 Modificar ofertas propias | Parcial | Falta validar propiedad de la empresa. |
| RF-07 Cerrar ofertas propias | Parcial | Falta validar propiedad de la empresa. |
| RF-08 Consultar detalle y postular con CV | Parcial | Falta subida/descarga real de CV. |
| RF-09 Filtrar vacantes | Parcial | Faltan filtros por habilidad, rango salarial, clasificación y búsqueda de texto. |
| RF-10 Consultar perfiles según rol | Parcial | Falta proyección limitada por rol y búsqueda por nombre/habilidades. |
| RF-11 Autorización por roles | No implementado | Login, JWT, roles y `[Authorize]`. |
| RF-12 Panel de estadísticas | No implementado | Controlador con totales por estado académico, laboral, área y oferta. |
| RF-13 Carga masiva Excel | No implementado | Endpoint/servicio, validación de filas, conteos y perfiles pendientes. |
| RF-14 Activar cuentas de empresas | Parcial | Endpoint existe, pero sin autorización de administrador. |
| RF-15 Iniciar sesión | No implementado | `AuthController`, verificación de contraseña y token. |
| RF-16 Verificar identidad y consentimiento | Parcial | Envío/confirmación de código, declaración de veracidad y activación condicionada. |
| RF-17 Notificar vacantes afines | No implementado | Matching por área + habilidad, alerta interna y correo. |
| RF-18 Reportar ofertas sospechosas | No implementado | Modelo existe; falta controlador y paso a revisión. |
| RF-19 Trazabilidad de postulaciones | Implementado | Ya valida cerrada/vencida y duplicados. |
| RF-20 Revisar candidaturas y notificar | Parcial | Falta notificación a la empresa y descarga de CV; falta restringir a ofertas propias. |
| RF-21 Aprobar/rechazar ofertas | Parcial | Funciona, pero sin autorización de administrador; falta activar revisión por reporte. |
| RF-22 Recordatorios y encuestas | No implementado | Prioridad “Podría”; opcional para la entrega. |

### 6.1 Endpoints faltantes

- `POST /api/auth/login`, `GET /api/auth/me`.
- `GET /api/panel/estadisticas` o similar (RF-12).
- `POST /api/cargas`, `GET /api/cargas` (RF-13).
- `POST /api/reportes`, `GET /api/reportes` (RF-18).
- `GET /api/notificaciones`, `PUT /api/notificaciones/{id}/leida` (RF-17, RF-20, RF-21).
- Verificación de contacto: envío y confirmación de código (RF-16).
- Subida/descarga de CV (RF-08, RF-20).
- Búsqueda de perfiles y filtros avanzados de ofertas (RF-09, RF-10).
- Recordatorios/encuestas (RF-22, opcional).

---

## 7. Requisitos no funcionales

| Requisito | Estado | Qué falta |
|---|---|---|
| RNF-01 Rendimiento de búsqueda | Parcial | Índices adecuados y prueba con 35 usuarios/5,000 registros. Corregir scripts B4. |
| RNF-02 Seguridad HTTPS y permisos | No implementado | Autenticación, autorización y HTTPS real. |
| RNF-03 Privacidad y consentimiento | Parcial | Proyección por rol, consentimiento y restricción de datos personales. |
| RNF-04 Usabilidad del perfil | Fuera del alcance API | Depende del frontend. |
| RNF-05 Compatibilidad web/móvil | Fuera del alcance API | Depende del frontend. |
| RNF-06 Disponibilidad | Parcial | `restart`, monitoreo y ventana de prueba. |
| RNF-07 Integridad de datos | Parcial | Faltan `CHECK` y algunas restricciones únicas. |
| RNF-08 Mantenibilidad y documentación | Pendiente | README de API, Swagger UI, documentación de migraciones y pruebas. |

---

## 8. Reglas de negocio a corregir

| Regla | Problema | Acción |
|---|---|---|
| RN-01 | Solo se valida `Empresa.EstadoAprobacion`; no `Usuario.EstadoCuenta`. | Validar ambos estados para publicar. |
| RN-02 | Una oferta vencida sigue en estado `Publicada`. | Cerrarla automáticamente o excluirla de listados/detalle como oferta abierta. |
| RN-03 | Una oferta `En revision` puede verse desde el detalle o con filtro por estado. | Ocultarla a graduados y anónimos. |
| RN-04 | No se valida que la empresa sea dueña de la oferta. | Validar `id_empresa` contra el usuario autenticado. |
| RN-05 | La cuenta del graduado se activa antes de verificar. | Mantener `EstadoCuenta = false` hasta verificar contacto y consentimiento. |
| RN-06 | La carga masiva aún no existe. | Implementar perfiles importados como pendientes y ocultos. |
| RF-21 | Una oferta reportada no pasa a revisión. | Al registrar un reporte, cambiar la oferta a `En revision` si aplica. |
| RF-20 | No se notifica a la empresa al recibir una postulación. | Crear `Notificacion` y, si aplica, enviar correo. |

---

## 9. Base de datos: correcciones y mejoras

### 9.1 Lo que está correcto

- Las 14 tablas del SRS existen en la migración.
- Las relaciones y claves foráneas principales están configuradas con `Restrict`.
- Los índices únicos de correo, usuario de graduado/empresa, habilidad y postulación están presentes.

### 9.2 Correcciones

1. **Semilla de roles:** resuelto en la migración `20261127212221_SemillaRoles` con inserción idempotente por nombre.
2. **Índice único en `rol.nombre_rol`:** hoy la unicidad solo se valida en código.
3. **Restricciones `CHECK`:**
   - Estados de oferta: `En revision`, `Aprobada`, `Rechazada`, `Publicada`, `Cerrada`.
   - Estados de postulación: `Pendiente`, `En revision`, `Aceptada`, `Rechazada`.
   - `modalidad`: `Presencial`, `Remoto`, `Hibrido`.
   - `clasificacion_oportunidad` según el SRS.
   - Rango de `anio_graduacion`.
4. **Índices de rendimiento:**
   - `oferta(estado, fecha_cierre, area)` o índice parcial con el estado correcto.
   - `graduado(area_trabajo, estado_laboral, anio_graduacion)`.
5. **Política de borrado:** definir cascada o bloqueo explícito para experiencias, proyectos, habilidades, postulaciones y notificaciones.
6. **Aplicación de migración:** resuelto con `Database.Migrate()` en `Program.cs` y `depends_on` en Docker Compose.
7. **Nueva migración:** crear una migración después de agregar índices y restricciones (la semilla ya está aplicada).

### 9.3 Inconsistencias en scripts B4

Los scripts de prueba de rendimiento usan estados en minúscula, mientras la API usa mayúscula inicial:

- `database/sql/b4_01_datos_prueba.sql:22` inserta `'publicada'`.
- `database/sql/b4_02_consulta_explain.sql:5` filtra `'publicada'`.
- `database/sql/b4_03_indice.sql:4` crea el índice parcial con `'publicada'`.
- `database/sql/b4_05_migrar_estados.sql:2` actualiza `'Activa'` a `'Publicada'`.

Consecuencia: los 50,000 registros de prueba no son visibles desde la API y el índice parcial no se usa en las consultas reales. Además, `b4_01` usa `id_empresa = 1`; si esa empresa no existe, la inserción falla por clave foránea.

**Acción:** unificar todos los estados a los valores que usa la API (`Publicada`, `Cerrada`, `En revision`, etc.) y documentar que estos scripts son solo para la prueba B4, no para crear el esquema.

---

## 10. Calidad, pruebas y entrega

- No existe proyecto de pruebas automatizadas.
- No hay Swagger/Scalar UI; solo OpenAPI sin interfaz.
- `README.md` sigue siendo el README genérico de Angular.
- No hay documentación de endpoints, variables de entorno ni migraciones.
- No hay manejo global de excepciones ni `ProblemDetails`.
- Queda un controlador de ejemplo `WeatherForecastController` que conviene eliminar.
- No hay `.env.example`.
- No hay volumen para archivos si se implementa subida de CV.

---

## 11. Checklist priorizado

### Prioridad 1 — Bloqueantes

- [x] Agregar semilla de roles (`Graduado`, `Empresa`, `Administrador de la Escuela`).
- [x] Aplicar la migración en base nueva y documentar el comando.
- [x] Configurar cadena de conexión local.
- [x] Agregar `depends_on` de `api` hacia `postgres`.
- [ ] Corregir o configurar HTTPS.
- [ ] Unificar estados de oferta y corregir scripts B4.

### Prioridad 2 — Seguridad

- [ ] Implementar login con JWT y `AuthController`.
- [ ] Aplicar `[Authorize]` y roles.
- [ ] Validar propiedad de ofertas, postulaciones y perfiles.
- [ ] Restringir datos personales por rol.
- [ ] Configurar CORS para Angular.
- [ ] Corregir activación de cuentas según RN-05.

### Prioridad 3 — Requisitos faltantes

- [ ] Reportes de ofertas (RF-18).
- [ ] Notificaciones y marcado como leídas (RF-17, RF-20, RF-21).
- [ ] Carga masiva Excel (RF-13).
- [ ] Panel de estadísticas (RF-12).
- [ ] Verificación de contacto (RF-16).
- [ ] Matching y correo (RF-17).
- [ ] Filtros y búsqueda avanzada (RF-09, RF-10).
- [ ] Subida/descarga de CV (RF-08, RF-20).

### Prioridad 4 — Base de datos

- [ ] Índice único en `rol.nombre_rol`.
- [ ] Restricciones `CHECK`.
- [ ] Índices de rendimiento.
- [ ] Política de borrado.
- [ ] Migración nueva con índices y restricciones (la semilla ya está aplicada).

### Prioridad 5 — Calidad y entrega

- [ ] Swagger/Scalar UI.
- [ ] README de API y documentación de migraciones.
- [ ] Pruebas de integración.
- [ ] Manejo global de errores.
- [ ] Eliminar `WeatherForecast`.
- [ ] `.env.example`.

---

## 12. Anexo: endpoints existentes

### Roles
- `GET /api/roles`
- `GET /api/roles/{id}`
- `POST /api/roles`
- `PUT /api/roles/{id}`
- `DELETE /api/roles/{id}`

### Habilidades
- `GET /api/habilidades`
- `GET /api/habilidades/{id}`
- `POST /api/habilidades`
- `PUT /api/habilidades/{id}`
- `DELETE /api/habilidades/{id}`

### Graduados
- `GET /api/graduados`
- `GET /api/graduados/{id}`
- `POST /api/graduados`
- `PUT /api/graduados/{id}`
- `PUT /api/graduados/{id}/verificacion`
- `GET /api/graduados/{id}/habilidades`
- `PUT /api/graduados/{id}/habilidades/{idHabilidad}`
- `DELETE /api/graduados/{id}/habilidades/{idHabilidad}`

### Empresas
- `GET /api/empresas`
- `GET /api/empresas/{id}`
- `POST /api/empresas`
- `PUT /api/empresas/{id}`
- `PUT /api/empresas/{id}/aprobacion`

### Experiencias y proyectos
- `GET/POST /api/graduados/{id}/experiencias`
- `GET/PUT/DELETE /api/graduados/{id}/experiencias/{idExperiencia}`
- `GET/POST /api/graduados/{id}/proyectos`
- `GET/PUT/DELETE /api/graduados/{id}/proyectos/{idProyecto}`

### Ofertas
- `GET /api/ofertas`
- `GET /api/ofertas/{id}`
- `POST /api/ofertas`
- `PUT /api/ofertas/{id}`
- `PUT /api/ofertas/{id}/revision`
- `PUT /api/ofertas/{id}/cierre`
- `DELETE /api/ofertas/{id}`
- `GET /api/ofertas/{id}/habilidades`
- `PUT /api/ofertas/{id}/habilidades/{idHabilidad}`
- `DELETE /api/ofertas/{id}/habilidades/{idHabilidad}`

### Postulaciones
- `GET /api/postulaciones`
- `GET /api/postulaciones/{id}`
- `POST /api/postulaciones`
- `PUT /api/postulaciones/{id}/estado`
- `DELETE /api/postulaciones/{id}`

### Salud
- `GET /health`
- `GET /health/database`

---

## 13. Comandos de base de datos

```powershell
# Levantar PostgreSQL local
docker compose --env-file infra/.env -f infra/docker-compose.yml up -d postgres

# Configurar la cadena de conexión local en user-secrets
dotnet user-secrets set "ConnectionStrings:PortalGraduados" "<cadena>" --project backend/PortalGraduados.Api

# Crear y aplicar migraciones
dotnet ef migrations add <Nombre> --project backend/PortalGraduados.Api
dotnet ef database update --project backend/PortalGraduados.Api

# Verificar el estado de las migraciones
dotnet ef migrations list --project backend/PortalGraduados.Api
```

La API también aplica las migraciones pendientes automáticamente al arrancar mediante `Database.Migrate()` en `Program.cs`.

---

## 14. Conclusión

La estructura de datos y los CRUD principales están bien encaminados y compilan. La semilla de roles y la aplicación de migraciones ya quedaron resueltas. Para que el apartado de API y base de datos quede listo, el trabajo debe centrarse ahora en la autenticación/autorización. Después deben completarse los endpoints de reportes, notificaciones, cargas, panel, verificación y matching, y finalmente corregir reglas de negocio, índices, restricciones y documentación.
