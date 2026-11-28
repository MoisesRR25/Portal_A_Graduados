
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

  // RANGO SALARIAL OPCIONAL
  salarioMinimo: number | null;
  salarioMaximo: number | null;
}

@Component({
  selector: 'app-mis-ofertas-empresa',
  imports: [RouterLink, FormsModule],
  templateUrl: './mis-ofertas-empresa.html',
  styleUrl: './mis-ofertas-empresa.css'
})
export class MisOfertasEmpresa {

  // ==========================================
  // MENÚ MÓVIL
  // ==========================================

  menuAbierto = false;

  // ==========================================
  // SECCIONES DEL PANEL
  // ==========================================

  vistaSeguridad = false;
  notificacionesAbiertas = false;
  estadoVerificacionAbierto = false;

  // ==========================================
  // PESTAÑAS
  // ==========================================

  filtroActual: FiltroOferta = 'Todas';

  // ==========================================
  // VENTANAS DE ACCIONES
  // ==========================================

  accionActual: AccionOferta = null;
  ofertaSeleccionada: OfertaEmpresa | null = null;

  // ==========================================
  // EDICIÓN LOCAL
  // ==========================================

  tituloEdicion = '';
  ubicacionEdicion = '';
  modalidadEdicion = '';

  // Edición del rango salarial
  salarioMinimoEdicion: number | null = null;
  salarioMaximoEdicion: number | null = null;

  // ==========================================
  // AVISOS
  // ==========================================

  mensajeAccion = '';
  errorEdicion = '';

  // ==========================================
  // DATOS DE DEMOSTRACIÓN
  // No proceden de la API ni de PostgreSQL.
  // ==========================================

  ofertas: OfertaEmpresa[] = [
    {
      id: 1,
      titulo: 'Desarrollador Frontend Jr.',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '16 jun. 2025',
      postulantes: 18,
      estado: 'Activa',
      salarioMinimo: 500,
      salarioMaximo: 800
    },
    {
      id: 2,
      titulo: 'Analista de soporte técnico',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '10 jun. 2025',
      postulantes: 12,
      estado: 'Pendiente',
      salarioMinimo: null,
      salarioMaximo: null
    },
    {
      id: 3,
      titulo: 'Pasantía de desarrollo web',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '2 jun. 2025',
      postulantes: 9,
      estado: 'Activa',
      salarioMinimo: 200,
      salarioMaximo: 350
    },
    {
      id: 4,
      titulo: 'Diseñador UX/UI',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      fecha: '20 may. 2025',
      postulantes: 9,
      estado: 'Cerrada',
      salarioMinimo: 600,
      salarioMaximo: 900
    }
  ];

  // ==========================================
  // CONTADORES LOCALES
  // ==========================================

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

  // ==========================================
  // FILTRADO DE OFERTAS
  // ==========================================

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

  // ==========================================
  // ESTADO DE LA CUENTA
  // ==========================================

  alternarEstadoVerificacion(): void {

    this.estadoVerificacionAbierto =
      !this.estadoVerificacionAbierto;
  }

  // ==========================================
  // MOSTRAR RANGO SALARIAL
  // ==========================================

  obtenerRangoSalarial(oferta: OfertaEmpresa): string {

    if (
      oferta.salarioMinimo === null ||
      oferta.salarioMaximo === null
    ) {
      return 'No especificado';
    }

    return '$' + oferta.salarioMinimo.toFixed(2) +
      ' - $' + oferta.salarioMaximo.toFixed(2);
  }

  // ==========================================
  // VALIDAR RANGO SALARIAL
  // ==========================================

  get errorRangoSalarial(): string {

    const minimo = this.salarioMinimoEdicion ?? null;
    const maximo = this.salarioMaximoEdicion ?? null;

    // Ambos vacíos: rango salarial opcional.
    if (minimo === null && maximo === null) {
      return '';
    }

    // Si se ingresa un salario, completar ambos.
    if (minimo === null || maximo === null) {

      return 'Completa ambos salarios o deja ambos campos vacíos.';
    }

    // Evitar números inválidos, negativos o cero.
    if (
      !Number.isFinite(minimo) ||
      !Number.isFinite(maximo) ||
      minimo < 0.01 ||
      maximo < 0.01
    ) {

      return 'Los salarios deben ser iguales o mayores que $0.01.';
    }

    // Validar el orden del rango.
    if (minimo > maximo) {

      return 'El salario mínimo no puede superar al salario máximo.';
    }

    // Permitir como máximo dos decimales.
    if (
      Math.abs(
        minimo - Math.round(minimo * 100) / 100
      ) > 0.00000001 ||

      Math.abs(
        maximo - Math.round(maximo * 100) / 100
      ) > 0.00000001
    ) {

      return 'Los salarios deben tener como máximo dos decimales.';
    }

    return '';
  }

  // ==========================================
  // REVISIÓN ADMINISTRATIVA POR SALARIO
  // ==========================================

  get requiereRevisionPorSalario(): boolean {

    const minimo = this.salarioMinimoEdicion ?? null;
    const maximo = this.salarioMaximoEdicion ?? null;

    return (
      this.ofertaSeleccionada?.estado === 'Activa' &&
      minimo === null &&
      maximo === null
    );
  }

  // ==========================================
  // VER OFERTA
  // ==========================================

  verOferta(oferta: OfertaEmpresa): void {

    this.ofertaSeleccionada = oferta;
    this.accionActual = 'ver';
    this.errorEdicion = '';
  }

  // ==========================================
  // EDITAR OFERTA
  // ==========================================

  editarOferta(oferta: OfertaEmpresa): void {

    this.ofertaSeleccionada = oferta;
    this.accionActual = 'editar';
    this.errorEdicion = '';

    this.tituloEdicion = oferta.titulo;
    this.ubicacionEdicion = oferta.ubicacion;
    this.modalidadEdicion = oferta.modalidad;

    // Recuperar salarios actuales.
    this.salarioMinimoEdicion =
      oferta.salarioMinimo ?? null;

    this.salarioMaximoEdicion =
      oferta.salarioMaximo ?? null;
  }

  // ==========================================
  // GUARDAR EDICIÓN
  // ==========================================

  guardarEdicion(): void {

    if (
      this.accionActual !== 'editar' ||
      !this.ofertaSeleccionada
    ) {
      return;
    }

    this.errorEdicion = '';

    const titulo = this.tituloEdicion.trim();
    const ubicacion = this.ubicacionEdicion.trim();

    // Validar título y ubicación.
    if (
      titulo.length < 3 ||
      ubicacion.length < 2
    ) {

      this.errorEdicion =
        'Completa correctamente el título y la ubicación.';

      return;
    }

    // Validar modalidad.
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

    // ========================================
    // CORRECCIÓN: NORMALIZAR SALARIOS VACÍOS
    // ========================================

    const minimo = this.salarioMinimoEdicion ?? null;
    const maximo = this.salarioMaximoEdicion ?? null;

    this.salarioMinimoEdicion = minimo;
    this.salarioMaximoEdicion = maximo;

    // Validar rango salarial.
    if (this.errorRangoSalarial) {

      this.errorEdicion = this.errorRangoSalarial;

      return;
    }

    // Obtener el identificador y estado actual.
    const id = this.ofertaSeleccionada.id;

    const estadoAnterior =
      this.ofertaSeleccionada.estado;

    // Comprobar si ambos salarios están vacíos.
    const sinSalario =
      minimo === null && maximo === null;

    // ========================================
    // CORRECCIÓN: DETERMINAR NUEVO ESTADO
    // ========================================

    let nuevoEstado: EstadoOferta = estadoAnterior;

    if (
      estadoAnterior === 'Activa' &&
      sinSalario
    ) {

      nuevoEstado = 'Pendiente';
    }

    // Mostrar valores para comprobar el cambio.
    console.table({
      id,
      salarioMinimo: minimo,
      salarioMaximo: maximo,
      sinSalario,
      estadoAnterior,
      nuevoEstado
    });

    // ========================================
    // ACTUALIZAR EL ARREGLO LOCAL
    // ========================================

    this.ofertas = this.ofertas.map(oferta =>

      oferta.id === id

        ? {
            ...oferta,
            titulo,
            ubicacion,
            modalidad: this.modalidadEdicion,
            salarioMinimo: minimo,
            salarioMaximo: maximo,
            estado: nuevoEstado
          }

        : oferta
    );

    // Comprobar que la oferta se actualizó.
    console.log(
      'Oferta actualizada:',
      this.ofertas.find(oferta => oferta.id === id)
    );

    this.cerrarVentana();

    // ========================================
    // MOSTRAR RESULTADO DE LA EDICIÓN
    // ========================================

    if (
      estadoAnterior === 'Activa' &&
      nuevoEstado === 'Pendiente'
    ) {

      this.mensajeAccion =
        'La oferta pasó a pendiente de revisión ' +
        'porque no incluye un rango salarial. ' +
        'Los cambios son temporales. ' +
        'No se han guardado datos en PostgreSQL.';

    } else {

      this.mensajeAccion =
        'Cambios aplicados temporalmente en la demostración. ' +
        'No se han guardado en PostgreSQL.';
    }
  }

  // ==========================================
  // CERRAR OFERTA
  // ==========================================

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

        ? {
            ...oferta,
            estado: 'Cerrada'
          }

        : oferta
    );

    this.cerrarVentana();

    this.mensajeAccion =
      'La oferta se marcó como cerrada solo en esta ' +
      'demostración. No se actualizó la base de datos.';
  }

  // ==========================================
  // CERRAR VENTANA
  // ==========================================

  cerrarVentana(): void {

    this.accionActual = null;
    this.ofertaSeleccionada = null;
    this.errorEdicion = '';

    // Limpiar salarios de edición.
    this.salarioMinimoEdicion = null;
    this.salarioMaximoEdicion = null;
  }

  // ==========================================
  // CERRAR ELEMENTOS CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarVentana();
  }
}
