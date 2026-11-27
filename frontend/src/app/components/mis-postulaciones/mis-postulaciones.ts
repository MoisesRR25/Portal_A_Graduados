
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type EstadoPostulacion = 'En revisión' | 'Entrevista' | 'Finalizado';
type FiltroPostulaciones = 'todas' | 'activas' | 'finalizadas';

interface Postulacion {
  id: number;
  puesto: string;
  empresa: string;
  iniciales: string;
  color: string;
  fecha: string;
  modalidad: string;
  estado: EstadoPostulacion;
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
  // 2. BÚSQUEDA Y FILTROS
  // ==========================================

  busqueda = '';
  filtroSeleccionado: FiltroPostulaciones = 'todas';

  // ==========================================
  // 3. POSTULACIONES DE DEMOSTRACIÓN
  // ==========================================

  readonly postulaciones: Postulacion[] = [
    {
      id: 1,
      puesto: 'Desarrollador Frontend Jr.',
      empresa: 'Applaudo Studios',
      iniciales: 'AS',
      color: 'as',
      fecha: '12 Nov, 2026',
      modalidad: 'Híbrido',
      estado: 'En revisión'
    },
    {
      id: 2,
      puesto: 'Analista de Datos',
      empresa: 'Banco Agrícola',
      iniciales: 'BA',
      color: 'ba',
      fecha: '10 Nov, 2026',
      modalidad: 'Presencial',
      estado: 'Entrevista'
    },
    {
      id: 3,
      puesto: 'QA Tester Trainee',
      empresa: 'Elaniin',
      iniciales: 'EL',
      color: 'el',
      fecha: '01 Nov, 2026',
      modalidad: 'Remoto',
      estado: 'En revisión'
    },
    {
      id: 4,
      puesto: 'Backend Developer',
      empresa: 'Hugo App',
      iniciales: 'HA',
      color: 'ha',
      fecha: '20 Oct, 2026',
      modalidad: 'Híbrido',
      estado: 'Finalizado'
    },
    {
      id: 5,
      puesto: 'Pasantía de Desarrollo',
      empresa: 'Sertracen',
      iniciales: 'SE',
      color: 'se',
      fecha: '15 Oct, 2026',
      modalidad: 'Presencial',
      estado: 'Finalizado'
    }
  ];

  // ==========================================
  // 4. ESTADÍSTICAS AUTOMÁTICAS
  // ==========================================

  get totalPostulaciones(): number {
    return this.postulaciones.length;
  }

  get totalRevision(): number {
    return this.postulaciones.filter(
      p => p.estado === 'En revisión'
    ).length;
  }

  get totalEntrevistas(): number {
    return this.postulaciones.filter(
      p => p.estado === 'Entrevista'
    ).length;
  }

  get totalFinalizadas(): number {
    return this.postulaciones.filter(
      p => p.estado === 'Finalizado'
    ).length;
  }

  get totalActivas(): number {
    return this.totalPostulaciones - this.totalFinalizadas;
  }

  // ==========================================
  // 5. BÚSQUEDA Y FILTRADO
  // ==========================================

  private normalizar(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  get postulacionesFiltradas(): Postulacion[] {

    const texto = this.normalizar(this.busqueda);

    return this.postulaciones.filter(postulacion => {

      const contenido = this.normalizar(
        `${postulacion.empresa} ${postulacion.puesto}`
      );

      const coincideBusqueda =
        !texto || contenido.includes(texto);

      let coincideEstado = true;

      if (this.filtroSeleccionado === 'activas') {
        coincideEstado = postulacion.estado !== 'Finalizado';
      }

      if (this.filtroSeleccionado === 'finalizadas') {
        coincideEstado = postulacion.estado === 'Finalizado';
      }

      return coincideBusqueda && coincideEstado;
    });
  }

  seleccionarFiltro(filtro: FiltroPostulaciones): void {
    this.filtroSeleccionado = filtro;
  }

  limpiarBusqueda(): void {
    this.busqueda = '';
  }

  // ==========================================
  // 6. DETALLE DE POSTULACIÓN
  // ==========================================

  verDetalle(postulacion: Postulacion): void {
    this.postulacionSeleccionada = postulacion;
    this.notificacionesAbiertas = false;
  }

  cerrarDetalle(): void {
    this.postulacionSeleccionada = null;
  }

  // ==========================================
  // 7. MENÚ MÓVIL
  // ==========================================

  alternarMenu(): void {
    this.menuAbierto = !this.menuAbierto;
    this.notificacionesAbiertas = false;
  }

  cerrarMenu(): void {
    this.menuAbierto = false;
  }

  // ==========================================
  // 8. NOTIFICACIONES
  // ==========================================

  alternarNotificaciones(): void {
    this.notificacionesAbiertas = !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {
    this.notificacionesAbiertas = false;
  }

  // ==========================================
  // 9. ACCESO Y SEGURIDAD
  // ==========================================

  abrirSeguridad(): void {
    this.seguridadAbierta = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ==========================================
  // 10. CERRAR CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarDetalle();
    this.cerrarSeguridad();
  }
}
