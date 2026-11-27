
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type EstadoOferta =
  | 'Activa'
  | 'Pendiente'
  | 'Borrador'
  | 'Cerrada';

type FiltroOferta =
  | 'Todas'
  | 'Activas'
  | 'Borradores'
  | 'Pendientes'
  | 'Cerradas';

type AccionOferta = 'ver' | 'editar' | 'cerrar' | null;

interface OfertaEmpresa {
  id: number;
  titulo: string;
  ubicacion: string;
  modalidad: string;
  fecha: string;
  postulantes: number;
  estado: EstadoOferta;
}

@Component({
  selector: 'app-mis-ofertas-empresa',
  imports: [RouterLink, FormsModule],
  templateUrl: './mis-ofertas-empresa.html',
  styleUrl: './mis-ofertas-empresa.css'
})
export class MisOfertasEmpresa {

  // MENÚ MÓVIL
  menuAbierto = false;

  // SECCIONES DEL PANEL
  vistaSeguridad = false;
  notificacionesAbiertas = false;
  estadoVerificacionAbierto = false;

  // PESTAÑAS
  filtroActual: FiltroOferta = 'Todas';

  // VENTANAS DE ACCIONES
  accionActual: AccionOferta = null;
  ofertaSeleccionada: OfertaEmpresa | null = null;

  // EDICIÓN LOCAL
  tituloEdicion = '';
  ubicacionEdicion = '';
  modalidadEdicion = '';

  // AVISOS
  mensajeAccion = '';
  errorEdicion = '';

  // DATOS DE DEMOSTRACIÓN
  // No proceden de la API ni de PostgreSQL.
  ofertas: OfertaEmpresa[] = [
    {
      id: 1,
      titulo: 'Desarrollador Frontend Jr.',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '16 jun. 2025',
      postulantes: 18,
      estado: 'Activa'
    },
    {
      id: 2,
      titulo: 'Analista de soporte técnico',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '10 jun. 2025',
      postulantes: 12,
      estado: 'Pendiente'
    },
    {
      id: 3,
      titulo: 'Pasantía de desarrollo web',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '2 jun. 2025',
      postulantes: 9,
      estado: 'Activa'
    },
    {
      id: 4,
      titulo: 'Diseñador UX/UI',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '20 may. 2025',
      postulantes: 9,
      estado: 'Cerrada'
    }
  ];

  // CONTADORES LOCALES
  get totalOfertas(): number {
    return this.ofertas.length;
  }

  get ofertasActivas(): number {
    return this.ofertas.filter(
      oferta => oferta.estado === 'Activa'
    ).length;
  }

  get ofertasBorrador(): number {
    return this.ofertas.filter(
      oferta => oferta.estado === 'Borrador'
    ).length;
  }

  get ofertasPendientes(): number {
    return this.ofertas.filter(
      oferta => oferta.estado === 'Pendiente'
    ).length;
  }

  get ofertasCerradas(): number {
    return this.ofertas.filter(
      oferta => oferta.estado === 'Cerrada'
    ).length;
  }

  // FILTRADO DE OFERTAS
  get ofertasVisibles(): OfertaEmpresa[] {
    switch (this.filtroActual) {
      case 'Activas':
        return this.ofertas.filter(
          oferta => oferta.estado === 'Activa'
        );

      case 'Borradores':
        return this.ofertas.filter(
          oferta => oferta.estado === 'Borrador'
        );

      case 'Pendientes':
        return this.ofertas.filter(
          oferta => oferta.estado === 'Pendiente'
        );

      case 'Cerradas':
        return this.ofertas.filter(
          oferta => oferta.estado === 'Cerrada'
        );

      default:
        return this.ofertas;
    }
  }

  cambiarFiltro(filtro: FiltroOferta): void {
    this.filtroActual = filtro;
    this.mensajeAccion = '';
  }

  // MENÚ MÓVIL
  alternarMenu(): void {
    this.menuAbierto = !this.menuAbierto;

    if (this.menuAbierto) {
      this.cerrarNotificaciones();
    }
  }

  cerrarMenu(): void {
    this.menuAbierto = false;
  }

  // NOTIFICACIONES
  alternarNotificaciones(): void {
    this.notificacionesAbiertas =
      !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {
    this.notificacionesAbiertas = false;
  }

  // ACCESO Y SEGURIDAD
  mostrarSeguridad(): void {
    this.vistaSeguridad = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarVentana();
  }

  mostrarOfertas(): void {
    this.vistaSeguridad = false;
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }

  // ESTADO DE LA CUENTA
  alternarEstadoVerificacion(): void {
    this.estadoVerificacionAbierto =
      !this.estadoVerificacionAbierto;
  }

  // VER OFERTA
  verOferta(oferta: OfertaEmpresa): void {
    this.ofertaSeleccionada = oferta;
    this.accionActual = 'ver';
    this.errorEdicion = '';
  }

  // EDITAR OFERTA
  editarOferta(oferta: OfertaEmpresa): void {
    this.ofertaSeleccionada = oferta;
    this.accionActual = 'editar';
    this.errorEdicion = '';

    this.tituloEdicion = oferta.titulo;
    this.ubicacionEdicion = oferta.ubicacion;
    this.modalidadEdicion = oferta.modalidad;
  }

  guardarEdicion(): void {
    if (
      this.accionActual !== 'editar' ||
      !this.ofertaSeleccionada
    ) {
      return;
    }

    const titulo = this.tituloEdicion.trim();
    const ubicacion = this.ubicacionEdicion.trim();

    if (titulo.length < 3 || ubicacion.length < 2) {
      this.errorEdicion =
        'Completa correctamente el título y la ubicación.';
      return;
    }

    const modalidadesValidas = [
      'Presencial',
      'Híbrido',
      'Remoto'
    ];

    if (
      !modalidadesValidas.includes(
        this.modalidadEdicion
      )
    ) {
      this.errorEdicion =
        'Selecciona una modalidad válida.';
      return;
    }

    const id = this.ofertaSeleccionada.id;

    // Solo modifica el arreglo local de Angular.
    this.ofertas = this.ofertas.map(oferta =>
      oferta.id === id
        ? {
            ...oferta,
            titulo,
            ubicacion,
            modalidad: this.modalidadEdicion
          }
        : oferta
    );

    this.cerrarVentana();

    this.mensajeAccion =
      'Cambios aplicados temporalmente en la demostración. ' +
      'No se han guardado en PostgreSQL.';
  }

  // CERRAR OFERTA
  solicitarCierre(oferta: OfertaEmpresa): void {
    if (oferta.estado === 'Cerrada') {
      return;
    }

    this.ofertaSeleccionada = oferta;
    this.accionActual = 'cerrar';
  }

  confirmarCierre(): void {
    if (
      this.accionActual !== 'cerrar' ||
      !this.ofertaSeleccionada
    ) {
      return;
    }

    const id = this.ofertaSeleccionada.id;

    // Cambio temporal, solo en esta pantalla.
    this.ofertas = this.ofertas.map(oferta =>
      oferta.id === id
        ? { ...oferta, estado: 'Cerrada' }
        : oferta
    );

    this.cerrarVentana();

    this.mensajeAccion =
      'La oferta se marcó como cerrada solo en esta ' +
      'demostración. No se actualizó la base de datos.';
  }

  cerrarVentana(): void {
    this.accionActual = null;
    this.ofertaSeleccionada = null;
    this.errorEdicion = '';
  }

  // CERRAR ELEMENTOS CON ESCAPE
  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarVentana();
  }
}
