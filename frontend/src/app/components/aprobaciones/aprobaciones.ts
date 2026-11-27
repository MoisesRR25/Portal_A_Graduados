
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type TipoSolicitud = 'Empresa' | 'Oferta';

type EstadoSolicitud =
  | 'Pendiente'
  | 'Aprobada'
  | 'Rechazada';

type AccionSolicitud = 'Aprobar' | 'Rechazar';

interface SolicitudAprobacion {
  id: number;
  tipo: TipoSolicitud;
  titulo: string;
  dato: string;
  fecha: string;
  estado: EstadoSolicitud;
  motivoRechazo: string;
}

@Component({
  selector: 'app-aprobaciones',
  imports: [RouterLink, FormsModule],
  templateUrl: './aprobaciones.html',
  styleUrl: './aprobaciones.css'
})
export class Aprobaciones {

  // ==========================================
  // MENÚ, NOTIFICACIONES Y SEGURIDAD
  // ==========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ==========================================
  // PESTAÑAS
  // ==========================================

  pestanaActual: EstadoSolicitud = 'Pendiente';

  // ==========================================
  // MODALES
  // ==========================================

  solicitudDetalle: SolicitudAprobacion | null = null;
  solicitudAccion: SolicitudAprobacion | null = null;
  accionActual: AccionSolicitud | null = null;

  // ==========================================
  // RECHAZO Y MENSAJES
  // ==========================================

  motivoRechazo = '';
  errorMotivo = '';
  mensajeResultado = '';

  // ==========================================
  // SOLICITUDES DE DEMOSTRACIÓN
  // ==========================================
  // Son las cuatro solicitudes originales.
  // No se consultan desde PostgreSQL.

  solicitudes: SolicitudAprobacion[] = [
    {
      id: 1,
      tipo: 'Empresa',
      titulo: 'Data Solutions SV',
      dato: 'NIT 0614-120824-101-8',
      fecha: '17 jun. 2025',
      estado: 'Pendiente',
      motivoRechazo: ''
    },
    {
      id: 2,
      tipo: 'Empresa',
      titulo: 'Grupo Tecnológico Cuscatlán',
      dato: 'NIT 0614-090421-103-2',
      fecha: '16 jun. 2025',
      estado: 'Pendiente',
      motivoRechazo: ''
    },
    {
      id: 3,
      tipo: 'Oferta',
      titulo: 'Desarrollador Java Junior',
      dato: 'TechLab El Salvador',
      fecha: '18 jun. 2025',
      estado: 'Pendiente',
      motivoRechazo: ''
    },
    {
      id: 4,
      tipo: 'Oferta',
      titulo: 'Analista de ciberseguridad',
      dato: 'Banco Azul',
      fecha: '17 jun. 2025',
      estado: 'Pendiente',
      motivoRechazo: ''
    }
  ];

  // ==========================================
  // CONTADORES DINÁMICOS
  // ==========================================

  get totalPendientes(): number {
    return this.solicitudes.filter(
      solicitud => solicitud.estado === 'Pendiente'
    ).length;
  }

  get totalAprobadas(): number {
    return this.solicitudes.filter(
      solicitud => solicitud.estado === 'Aprobada'
    ).length;
  }

  get totalRechazadas(): number {
    return this.solicitudes.filter(
      solicitud => solicitud.estado === 'Rechazada'
    ).length;
  }

  get solicitudesVisibles(): SolicitudAprobacion[] {
    return this.solicitudes.filter(
      solicitud => solicitud.estado === this.pestanaActual
    );
  }

  // ==========================================
  // MENÚ MÓVIL
  // ==========================================

  alternarMenu(): void {
    this.menuAbierto = !this.menuAbierto;

    if (this.menuAbierto) {
      this.cerrarNotificaciones();
    }
  }

  cerrarMenu(): void {
    this.menuAbierto = false;
  }

  // ==========================================
  // NOTIFICACIONES
  // ==========================================

  alternarNotificaciones(): void {
    this.notificacionesAbiertas =
      !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {
    this.notificacionesAbiertas = false;
  }

  // ==========================================
  // ACCESO Y SEGURIDAD
  // ==========================================

  abrirSeguridad(): void {
    this.seguridadAbierta = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarRevision();
    this.cancelarAccion();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ==========================================
  // CAMBIAR PESTAÑA
  // ==========================================

  seleccionarPestana(
    pestana: EstadoSolicitud
  ): void {
    this.pestanaActual = pestana;
    this.mensajeResultado = '';
  }

  // ==========================================
  // REVISAR SOLICITUD
  // ==========================================

  revisarSolicitud(
    solicitud: SolicitudAprobacion
  ): void {
    this.solicitudDetalle = solicitud;
    this.cerrarNotificaciones();
    this.mensajeResultado = '';
  }

  cerrarRevision(): void {
    this.solicitudDetalle = null;
  }

  // ==========================================
  // ABRIR CONFIRMACIÓN
  // ==========================================

  solicitarAccion(
    solicitud: SolicitudAprobacion,
    accion: AccionSolicitud
  ): void {

    if (solicitud.estado !== 'Pendiente') {
      return;
    }

    this.solicitudAccion = solicitud;
    this.accionActual = accion;

    this.motivoRechazo = '';
    this.errorMotivo = '';

    this.cerrarRevision();
    this.cerrarNotificaciones();
    this.mensajeResultado = '';
  }

  // ==========================================
  // CANCELAR APROBACIÓN O RECHAZO
  // ==========================================

  cancelarAccion(): void {
    this.solicitudAccion = null;
    this.accionActual = null;
    this.motivoRechazo = '';
    this.errorMotivo = '';
  }

  // ==========================================
  // CONFIRMAR ACCIÓN LOCAL
  // ==========================================

  confirmarAccion(): void {

    if (!this.solicitudAccion || !this.accionActual) {
      return;
    }

    const solicitud = this.solicitudAccion;
    const accion = this.accionActual;

    if (solicitud.estado !== 'Pendiente') {
      this.cancelarAccion();
      return;
    }

    // Validar el motivo del rechazo.
    if (accion === 'Rechazar') {

      const motivo = this.motivoRechazo.trim();

      if (motivo.length < 10) {
        this.errorMotivo =
          'Escribe un motivo de al menos 10 caracteres.';
        return;
      }

      if (motivo.length > 500) {
        this.errorMotivo =
          'El motivo no puede superar 500 caracteres.';
        return;
      }

      solicitud.estado = 'Rechazada';
      solicitud.motivoRechazo = motivo;

      this.mensajeResultado =
        'La solicitud "' + solicitud.titulo +
        '" se marcó como rechazada en esta demostración. ' +
        'No se guardó el cambio ni se envió una notificación.';
    }

    if (accion === 'Aprobar') {

      solicitud.estado = 'Aprobada';
      solicitud.motivoRechazo = '';

      this.mensajeResultado =
        'La solicitud "' + solicitud.titulo +
        '" se marcó como aprobada en esta demostración. ' +
        'No se guardó en la base de datos ni se publicó nada.';
    }

    this.cancelarAccion();
  }

  // ==========================================
  // CERRAR MENSAJE
  // ==========================================

  cerrarMensajeResultado(): void {
    this.mensajeResultado = '';
  }

  // ==========================================
  // CERRAR TODO CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarRevision();
    this.cancelarAccion();
  }
}
