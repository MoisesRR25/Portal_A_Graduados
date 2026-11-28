
import { Component, HostListener } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

// ==========================================
// INTERFAZ DE LOS DATOS DE LA OFERTA
// ==========================================

interface DatosOferta {
  titulo: string;
  area: string;
  contrato: string;
  ubicacion: string;
  modalidad: string;

  // NUEVO: rango salarial opcional
  salarioMinimo: number | null;
  salarioMaximo: number | null;

  descripcion: string;

  // CORREGIDO: es un texto, no un arreglo
  requisitos: string;

  habilidades: string[];
  fechaLimite: string;
  plazas: number | null;
  recienGraduados: boolean;
  sinExperiencia: boolean;
  pasantia: boolean;
}

@Component({
  selector: 'app-publicar-oferta',
  imports: [RouterLink, FormsModule],
  templateUrl: './publicar-oferta.html',
  styleUrl: './publicar-oferta.css'
})
export class PublicarOferta {

  // ==========================================
  // MENÚ LATERAL
  // ==========================================

  menuAbierto = false;

  // ==========================================
  // PANELES INFORMATIVOS
  // ==========================================

  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ==========================================
  // VISTA PREVIA
  // ==========================================

  vistaPreviaAbierta = false;

  // ==========================================
  // DATOS DEL FORMULARIO
  // ==========================================

  tituloOferta = '';
  areaOferta = '';
  tipoContrato = 'tiempo-completo';
  ubicacionOferta = '';
  modalidadOferta = 'hibrido';

  // NUEVO: rango salarial opcional
  salarioMinimo: number | null = null;
  salarioMaximo: number | null = null;

  descripcionOferta = '';
  requisitosOferta = '';

  // Habilidades requeridas
  habilidadNueva = '';
  habilidades: string[] = [];

  // Fecha y plazas
  fechaLimite = '';
  plazasDisponibles: number | null = 1;

  // Clasificación
  recienGraduados = false;
  sinExperiencia = false;
  pasantia = false;

  // ==========================================
  // BORRADOR TEMPORAL
  // ==========================================

  borradorLocal: DatosOferta | null = null;

  // ==========================================
  // MENSAJES
  // ==========================================

  errorGeneral = '';
  mensajeInformativo = '';

  // Fecha mínima para la oferta
  readonly fechaMinima = this.obtenerFechaLocal();

  // ==========================================
  // OBTENER FECHA LOCAL
  // ==========================================

  private obtenerFechaLocal(): string {
    const fecha = new Date();

    const anio = fecha.getFullYear();

    const mes = String(
      fecha.getMonth() + 1
    ).padStart(2, '0');

    const dia = String(
      fecha.getDate()
    ).padStart(2, '0');

    return `${anio}-${mes}-${dia}`;
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
    this.cerrarVistaPrevia();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ==========================================
  // AGREGAR HABILIDADES
  // ==========================================

  // CORRECCIÓN DEL ERROR TS2345
  agregarHabilidadConEnter(evento: Event): void {
    evento.preventDefault();
    this.agregarHabilidad();
  }

  agregarHabilidad(): void {

    const habilidad = this.habilidadNueva.trim();

    if (!habilidad) {
      return;
    }

    // Validar longitud
    if (habilidad.length > 40) {
      this.errorGeneral =
        'Cada habilidad puede tener como máximo 40 caracteres.';
      return;
    }

    // Evitar habilidades duplicadas
    const yaExiste = this.habilidades.some(
      item =>
        item.toLowerCase() === habilidad.toLowerCase()
    );

    if (yaExiste) {
      this.errorGeneral =
        'Esa habilidad ya está agregada.';
      return;
    }

    // Limitar número de habilidades
    if (this.habilidades.length >= 15) {
      this.errorGeneral =
        'Puedes agregar un máximo de 15 habilidades.';
      return;
    }

    this.habilidades = [
      ...this.habilidades,
      habilidad
    ];

    this.habilidadNueva = '';
    this.errorGeneral = '';
  }

  // ==========================================
  // QUITAR HABILIDADES
  // ==========================================

  quitarHabilidad(habilidad: string): void {
    this.habilidades = this.habilidades.filter(
      item => item !== habilidad
    );
  }

  // ==========================================
  // NUEVO: VALIDAR RANGO SALARIAL
  // ==========================================

  get errorRangoSalarial(): string {

    const minimo = this.salarioMinimo;
    const maximo = this.salarioMaximo;

    // El rango salarial es opcional.
    if (minimo === null && maximo === null) {
      return '';
    }

    // Si se especifica un salario,
    // deben completarse ambos campos.
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
      Math.abs(minimo - Math.round(minimo * 100) / 100) > 0.00000001 ||
      Math.abs(maximo - Math.round(maximo * 100) / 100) > 0.00000001
    ) {
      return 'Los salarios deben tener como máximo dos decimales.';
    }

    return '';
  }

  // ==========================================
  // NUEVO: MOSTRAR RANGO SALARIAL
  // ==========================================

  get rangoSalarial(): string {

    if (this.errorRangoSalarial) {
      return 'Rango salarial inválido';
    }

    if (
      this.salarioMinimo === null ||
      this.salarioMaximo === null
    ) {
      return 'No especificado';
    }

    return '$' + this.salarioMinimo.toFixed(2) +
      ' - $' + this.salarioMaximo.toFixed(2);
  }

  // ==========================================
  // NUEVO: REVISIÓN ADMINISTRATIVA POR SALARIO
  // ==========================================

  get requiereRevisionPorSalario(): boolean {
    return (
      this.salarioMinimo === null &&
      this.salarioMaximo === null
    );
  }

  // ==========================================
  // OBTENER DATOS DEL FORMULARIO
  // ==========================================

  private obtenerDatos(): DatosOferta {
    return {
      titulo: this.tituloOferta.trim(),
      area: this.areaOferta,
      contrato: this.tipoContrato,
      ubicacion: this.ubicacionOferta.trim(),
      modalidad: this.modalidadOferta,

      // NUEVO: guardar salarios en el borrador
      salarioMinimo: this.salarioMinimo,
      salarioMaximo: this.salarioMaximo,

      descripcion: this.descripcionOferta.trim(),

      // CORREGIDO: ambos son de tipo string
      requisitos: this.requisitosOferta.trim(),

      habilidades: [...this.habilidades],
      fechaLimite: this.fechaLimite,
      plazas: this.plazasDisponibles,
      recienGraduados: this.recienGraduados,
      sinExperiencia: this.sinExperiencia,
      pasantia: this.pasantia
    };
  }

  // ==========================================
  // GUARDAR BORRADOR
  // ==========================================

  guardarBorrador(): void {

    this.borradorLocal = this.obtenerDatos();

    this.errorGeneral = '';

    this.mensajeInformativo =
      'Borrador guardado temporalmente en esta pantalla. ' +
      'No se ha enviado a la API ni guardado en PostgreSQL. ' +
      'Se perderá al recargar o abandonar la página.';
  }

  // ==========================================
  // RESTAURAR BORRADOR
  // ==========================================

  restaurarBorrador(): void {

    if (!this.borradorLocal) {
      return;
    }

    const datos = this.borradorLocal;

    this.tituloOferta = datos.titulo;
    this.areaOferta = datos.area;
    this.tipoContrato = datos.contrato;

    this.ubicacionOferta = datos.ubicacion;
    this.modalidadOferta = datos.modalidad;

    // NUEVO: restaurar salarios
    this.salarioMinimo = datos.salarioMinimo;
    this.salarioMaximo = datos.salarioMaximo;

    this.descripcionOferta = datos.descripcion;

    // CORREGIDO: string asignado a string
    this.requisitosOferta = datos.requisitos;

    this.habilidades = [...datos.habilidades];
    this.habilidadNueva = '';

    this.fechaLimite = datos.fechaLimite;
    this.plazasDisponibles = datos.plazas;

    this.recienGraduados = datos.recienGraduados;
    this.sinExperiencia = datos.sinExperiencia;
    this.pasantia = datos.pasantia;

    this.errorGeneral = '';

    this.mensajeInformativo =
      'Borrador restaurado correctamente. ' +
      'Los datos siguen siendo temporales.';
  }

  // ==========================================
  // VALIDAR Y REVISAR LA OFERTA
  // ==========================================

  revisarOferta(formulario: NgForm): void {

    this.errorGeneral = '';
    this.mensajeInformativo = '';

    // Agregar última habilidad escrita
    if (this.habilidadNueva.trim()) {

      this.agregarHabilidad();

      if (this.errorGeneral) {
        return;
      }
    }

    // Marcar los campos para mostrar errores
    formulario.control.markAllAsTouched();

    // NUEVO: validar rango salarial
    if (this.errorRangoSalarial) {
      this.errorGeneral = this.errorRangoSalarial;
      return;
    }

    if (formulario.invalid) {
      this.errorGeneral =
        'Revisa los campos obligatorios del formulario.';
      return;
    }

    // Validar datos generales
    if (
      this.tituloOferta.trim().length < 5 ||
      this.ubicacionOferta.trim().length < 2 ||
      this.descripcionOferta.trim().length < 20 ||
      this.requisitosOferta.trim().length < 10
    ) {
      this.errorGeneral =
        'Completa el título, la ubicación, la descripción ' +
        'y los requisitos con información válida.';
      return;
    }

    // Verificar habilidades
    if (this.habilidades.length === 0) {
      this.errorGeneral =
        'Agrega al menos una habilidad requerida.';
      return;
    }

    // Validar fecha límite
    if (
      !this.fechaLimite ||
      this.fechaLimite < this.fechaMinima
    ) {
      this.errorGeneral =
        'Selecciona una fecha límite válida que no haya pasado.';
      return;
    }

    // Validar plazas
    if (
      this.plazasDisponibles === null ||
      !Number.isInteger(this.plazasDisponibles) ||
      this.plazasDisponibles < 1 ||
      this.plazasDisponibles > 999
    ) {
      this.errorGeneral =
        'Las plazas disponibles deben estar entre 1 y 999.';
      return;
    }

    // Mostrar vista previa
    this.vistaPreviaAbierta = true;

    this.cerrarNotificaciones();
  }

  // ==========================================
  // CERRAR VISTA PREVIA
  // ==========================================

  cerrarVistaPrevia(): void {
    this.vistaPreviaAbierta = false;
  }

  // ==========================================
  // CONFIRMAR DATOS DE DEMOSTRACIÓN
  // ==========================================

  confirmarDatosDePrueba(): void {

    this.vistaPreviaAbierta = false;

    // NUEVO: informar si requiere revisión
    // por no especificar rango salarial.

    if (this.requiereRevisionPorSalario) {

      this.mensajeInformativo =
        'Los datos de la oferta están listos para revisión. ' +
        'La oferta requeriría revisión administrativa ' +
        'porque no incluye un rango salarial. ' +
        'Esta es una demostración: la oferta NO se ha enviado ' +
        'a la Escuela ni guardado en PostgreSQL.';

    } else {

      this.mensajeInformativo =
        'Los datos de la oferta están listos para revisión. ' +
        'El rango salarial fue validado correctamente. ' +
        'La API determinará si requiere revisión por otros motivos. ' +
        'Esta es una demostración: la oferta NO se ha enviado ' +
        'a la Escuela ni guardado en PostgreSQL.';

    }
  }

  // ==========================================
  // NOMBRE DEL ÁREA
  // ==========================================

  get nombreArea(): string {

    const areas: Record<string, string> = {
      desarrollo: 'Desarrollo de software',
      datos: 'Datos y analítica',
      infraestructura: 'Infraestructura y redes',
      soporte: 'Soporte técnico'
    };

    return areas[this.areaOferta] ?? 'Sin seleccionar';
  }

  // ==========================================
  // NOMBRE DEL CONTRATO
  // ==========================================

  get nombreContrato(): string {

    const contratos: Record<string, string> = {
      'tiempo-completo': 'Tiempo completo',
      'medio-tiempo': 'Medio tiempo',
      pasantia: 'Pasantía',
      proyecto: 'Por proyecto'
    };

    return contratos[this.tipoContrato] ??
      'Sin seleccionar';
  }

  // ==========================================
  // NOMBRE DE LA MODALIDAD
  // ==========================================

  get nombreModalidad(): string {

    const modalidades: Record<string, string> = {
      hibrido: 'Híbrido',
      presencial: 'Presencial',
      remoto: 'Remoto'
    };

    return modalidades[this.modalidadOferta] ??
      'Sin seleccionar';
  }

  // ==========================================
  // CLASIFICACIONES SELECCIONADAS
  // ==========================================

  get clasificaciones(): string[] {

    const seleccionadas: string[] = [];

    if (this.recienGraduados) {
      seleccionadas.push('Recién graduados');
    }

    if (this.sinExperiencia) {
      seleccionadas.push('Sin experiencia');
    }

    if (this.pasantia) {
      seleccionadas.push('Pasantía');
    }

    return seleccionadas;
  }

  // ==========================================
  // CERRAR ELEMENTOS CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarVistaPrevia();
  }
}
