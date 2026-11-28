
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

// Estados unificados con el componente candidatos.
type EstadoPostulacion =
  | 'En revisión'
  | 'Preseleccionado'
  | 'Descartado';

type FiltroPostulaciones =
  | 'todas'
  | 'activas'
  | 'finalizadas';

interface Postulacion {
  id: number;
  graduadoId: number;
  ofertaId: number;
  puesto: string;
  empresa: string;
  iniciales: string;
  color: string;
  fecha: string;
  modalidad: string;
  estado: EstadoPostulacion;

  // Datos para la trazabilidad de la postulación.
  nombreCV: string;
  fechaActualizacion: string;
}

@Component({
  selector: 'app-mis-postulaciones',
  imports: [RouterLink, FormsModule],
  templateUrl: './mis-postulaciones.html',
  styleUrl: './mis-postulaciones.css'
})
export class MisPostulaciones {

  // ==========================================
  // 1. NAVEGACIÓN Y VENTANAS
  // ==========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  postulacionSeleccionada: Postulacion | null = null;

  // ==========================================
  // 2. GRADUADO DE DEMOSTRACIÓN
  // ==========================================
  // La identidad real deberá obtenerse
  // mediante la autenticación de la API.

  readonly nombreGraduado = 'Carlos Martínez';

  private readonly graduadoIdActual = 1;

  // ==========================================
  // 3. BÚSQUEDA Y FILTROS
  // ==========================================

  busqueda = '';

  filtroSeleccionado: FiltroPostulaciones = 'todas';

  // ==========================================
  // 4. POSTULACIONES DE DEMOSTRACIÓN
  // ==========================================
  // Estos registros son ficticios.
  // No proceden de PostgreSQL ni de la API.

  private readonly postulacionesDemo: Postulacion[] = [
    {
      id: 1,
      graduadoId: 1,
      ofertaId: 1,
      puesto: 'Desarrollador Frontend Jr.',
      empresa: 'Applaudo Studios',
      iniciales: 'AS',
      color: 'as',
      fecha: '2026-11-12',
      modalidad: 'Híbrido',
      estado: 'En revisión',
      nombreCV: 'CV_Carlos_Martinez.pdf',
      fechaActualizacion: '2026-11-12'
    },
    {
      id: 2,
      graduadoId: 1,
      ofertaId: 2,
      puesto: 'Analista de Datos',
      empresa: 'Banco Agrícola',
      iniciales: 'BA',
      color: 'ba',
      fecha: '2026-11-10',
      modalidad: 'Presencial',
      estado: 'Preseleccionado',
      nombreCV: 'CV_Carlos_Martinez.pdf',
      fechaActualizacion: '2026-11-15'
    },
    {
      id: 3,
      graduadoId: 1,
      ofertaId: 3,
      puesto: 'QA Tester Trainee',
      empresa: 'Elaniin',
      iniciales: 'EL',
      color: 'el',
      fecha: '2026-11-01',
      modalidad: 'Remoto',
      estado: 'En revisión',
      nombreCV: 'CV_Carlos_Martinez.pdf',
      fechaActualizacion: '2026-11-01'
    },
    {
      id: 4,
      graduadoId: 1,
      ofertaId: 5,
      puesto: 'Backend Developer',
      empresa: 'Hugo App',
      iniciales: 'HA',
      color: 'ha',
      fecha: '2026-10-20',
      modalidad: 'Híbrido',
      estado: 'Descartado',
      nombreCV: 'CV_Carlos_Martinez.pdf',
      fechaActualizacion: '2026-10-28'
    },
    {
      id: 5,
      graduadoId: 1,
      ofertaId: 6,
      puesto: 'Pasantía de Desarrollo',
      empresa: 'Sertracen',
      iniciales: 'SE',
      color: 'se',
      fecha: '2026-10-15',
      modalidad: 'Presencial',
      estado: 'Descartado',
      nombreCV: 'CV_Carlos_Martinez.pdf',
      fechaActualizacion: '2026-10-25'
    },

    // Registro ficticio de otro graduado.
    // No debe aparecer en el historial actual.
    {
      id: 6,
      graduadoId: 2,
      ofertaId: 4,
      puesto: 'Soporte Técnico N1',
      empresa: 'Telefónica Tech',
      iniciales: 'TT',
      color: 'tt',
      fecha: '2026-11-18',
      modalidad: 'Presencial',
      estado: 'En revisión',
      nombreCV: 'CV_Otro_Graduado.pdf',
      fechaActualizacion: '2026-11-18'
    }
  ];

  // ==========================================
  // 5. POSTULACIONES DEL GRADUADO ACTUAL
  // ==========================================
  // Filtrado local de demostración.
  // La API deberá verificar los permisos reales.

  readonly postulaciones: Postulacion[] =
    this.postulacionesDemo.filter(
      postulacion =>
        postulacion.graduadoId === this.graduadoIdActual
    );

  // ==========================================
  // 6. ESTADÍSTICAS AUTOMÁTICAS
  // ==========================================

  get totalPostulaciones(): number {

    return this.postulaciones.length;
  }

  get totalRevision(): number {

    return this.postulaciones.filter(
      p => p.estado === 'En revisión'
    ).length;
  }

  get totalPreseleccionadas(): number {

    return this.postulaciones.filter(
      p => p.estado === 'Preseleccionado'
    ).length;
  }

  get totalDescartadas(): number {

    return this.postulaciones.filter(
      p => p.estado === 'Descartado'
    ).length;
  }

  get totalFinalizadas(): number {

    return this.totalDescartadas;
  }

  get totalActivas(): number {

    return (
      this.totalRevision +
      this.totalPreseleccionadas
    );
  }

  // ==========================================
  // 7. NORMALIZAR TEXTO
  // ==========================================

  private normalizar(texto: string): string {

    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  // ==========================================
  // 8. BÚSQUEDA Y FILTRADO
  // ==========================================

  get postulacionesFiltradas(): Postulacion[] {

    const texto = this.normalizar(this.busqueda);

    return this.postulaciones.filter(postulacion => {

      const contenido = this.normalizar([
        postulacion.empresa,
        postulacion.puesto,
        postulacion.modalidad,
        postulacion.estado
      ].join(' '));

      const coincideBusqueda =
        !texto || contenido.includes(texto);

      let coincideEstado = true;

      // Las postulaciones activas son aquellas
      // en revisión o preseleccionadas.

      if (this.filtroSeleccionado === 'activas') {

        coincideEstado =
          postulacion.estado === 'En revisión' ||
          postulacion.estado === 'Preseleccionado';
      }

      // Las postulaciones descartadas se conservan
      // en el historial de procesos finalizados.

      if (this.filtroSeleccionado === 'finalizadas') {

        coincideEstado =
          postulacion.estado === 'Descartado';
      }

      return coincideBusqueda && coincideEstado;

    }).sort((a, b) => {

      // Ordenar por fecha de postulación,
      // desde la más reciente hasta la más antigua.
      // Las fechas se almacenan en formato ISO.

      return b.fecha.localeCompare(a.fecha);
    });
  }

  // ==========================================
  // 9. SELECCIONAR FILTRO
  // ==========================================

  seleccionarFiltro(
    filtro: FiltroPostulaciones
  ): void {

    this.filtroSeleccionado = filtro;
  }

  limpiarBusqueda(): void {

    this.busqueda = '';
  }

  limpiarFiltros(): void {

    this.busqueda = '';

    this.filtroSeleccionado = 'todas';
  }

  // ==========================================
  // 10. FORMATEAR FECHAS
  // ==========================================

  formatearFecha(fecha: string): string {

    const fechaConvertida = new Date(
      fecha + 'T00:00:00'
    );

    if (Number.isNaN(fechaConvertida.getTime())) {
      return 'Fecha no disponible';
    }

    return fechaConvertida.toLocaleDateString(
      'es-SV',
      {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
      }
    );
  }

  // ==========================================
  // 11. DESCRIPCIÓN DEL ESTADO
  // ==========================================

  obtenerDescripcionEstado(
    estado: EstadoPostulacion
  ): string {

    switch (estado) {

      case 'En revisión':

        return (
          'Tu postulación está disponible para que ' +
          'la empresa revise tu perfil y currículum.'
        );

      case 'Preseleccionado':

        return (
          'Tu candidatura fue preseleccionada por ' +
          'la empresa. Esto no significa que ya ' +
          'exista una contratación o entrevista programada.'
        );

      case 'Descartado':

        return (
          'Tu candidatura no continuará en este ' +
          'proceso de selección. El registro se ' +
          'conserva en tu historial de postulaciones.'
        );

      default:

        return 'Estado no disponible.';
    }
  }

  // ==========================================
  // 12. DETALLE DE POSTULACIÓN
  // ==========================================

  verDetalle(postulacion: Postulacion): void {

    // Comprobar que la postulación pertenezca
    // al graduado actual de la demostración.

    const postulacionAutorizada =
      this.postulaciones.find(
        elemento => elemento.id === postulacion.id
      );

    if (!postulacionAutorizada) {
      return;
    }

    this.postulacionSeleccionada =
      postulacionAutorizada;

    this.cerrarNotificaciones();
    this.cerrarSeguridad();
  }

  cerrarDetalle(): void {

    this.postulacionSeleccionada = null;
  }

  // ==========================================
  // 13. MENÚ MÓVIL
  // ==========================================

  alternarMenu(): void {

    this.menuAbierto = !this.menuAbierto;

    this.notificacionesAbiertas = false;
  }

  cerrarMenu(): void {

    this.menuAbierto = false;
  }

  // ==========================================
  // 14. NOTIFICACIONES
  // ==========================================

  alternarNotificaciones(): void {

    this.notificacionesAbiertas =
      !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {

    this.notificacionesAbiertas = false;
  }

  // ==========================================
  // 15. ACCESO Y SEGURIDAD
  // ==========================================

  abrirSeguridad(): void {

    this.seguridadAbierta = true;

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarDetalle();
  }

  cerrarSeguridad(): void {

    this.seguridadAbierta = false;
  }

  // ==========================================
  // 16. CERRAR CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarDetalle();
    this.cerrarSeguridad();
  }
}
