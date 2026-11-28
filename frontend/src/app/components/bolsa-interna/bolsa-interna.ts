
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type Modalidad = 'Presencial' | 'Híbrido' | 'Remoto';
type Clasificacion = 'Recién graduados' | 'Sin experiencia' | 'Pasantía';
type OrdenOfertas = 'recientes' | 'compatibilidad';

interface OfertaTrabajo {
  id: number;
  empresa: string;
  iniciales: string;
  color: string;
  puesto: string;
  area: string;
  ubicacion: string;
  modalidad: Modalidad;
  clasificaciones: Clasificacion[];
  etiquetas: string[];
  antiguedadDias: number;
  publicada: string;
  compatibilidad?: number;

  // Rango salarial opcional.
  salarioMinimo: number | null;
  salarioMaximo: number | null;
}

@Component({
  selector: 'app-bolsa-interna',
  imports: [RouterLink, FormsModule],
  templateUrl: './bolsa-interna.html',
  styleUrl: './bolsa-interna.css'
})
export class BolsaInterna {

  // ==========================================
  // ESTADO DE LA INTERFAZ
  // ==========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  ofertaSeleccionada: OfertaTrabajo | null = null;

  // ==========================================
  // FILTROS DE BÚSQUEDA
  // ==========================================

  textoBusqueda = '';
  busquedaAplicada = '';
  areaSeleccionada = '';
  ubicacionSeleccionada = '';
  modalidadesSeleccionadas: Modalidad[] = [];
  clasificacionesSeleccionadas: Clasificacion[] = [];
  habilidadesSeleccionadas: string[] = [];
  ordenSeleccionado: OrdenOfertas = 'recientes';

  // Filtros salariales.
  salarioMinimoFiltro: number | null = null;
  salarioMaximoFiltro: number | null = null;

  // ==========================================
  // ACCIONES LOCALES
  // ==========================================

  favoritos: number[] = [];
  postulacionesSimuladas: number[] = [];
  mensajeAccion = '';

  // ==========================================
  // NUEVO: ARCHIVO CV - RF-08
  // ==========================================

  archivoCV: File | null = null;
  errorCV = '';

  // Tamaño máximo permitido: 5 MiB.
  readonly maximoBytesCV = 5 * 1024 * 1024;

  // Solo conserva nombres de archivos en memoria.
  // No almacena los documentos enviados.
  cvPostulacionesSimuladas: Record<number, string> = {};

  // ==========================================
  // OPCIONES DE FILTRADO
  // ==========================================

  readonly areas = [
    'Desarrollo de Software',
    'Bases de Datos',
    'QA y Testing',
    'Soporte Técnico'
  ];

  readonly ubicaciones = [
    'San Salvador',
    'Antiguo Cuscatlán',
    'La Libertad',
    'Soyapango',
    'Santa Ana',
    'San Miguel'
  ];

  readonly modalidades: Modalidad[] = [
    'Presencial',
    'Híbrido',
    'Remoto'
  ];

  readonly clasificaciones: Clasificacion[] = [
    'Recién graduados',
    'Sin experiencia',
    'Pasantía'
  ];

  readonly habilidades = [
    'React',
    'Java',
    'SQL',
    'Python',
    'Soporte'
  ];

  // ==========================================
  // DATOS DE DEMOSTRACIÓN
  // Son las seis vacantes del diseño original.
  // Los salarios también son ficticios.
  // No proceden de la API.
  // ==========================================

  readonly ofertas: OfertaTrabajo[] = [
    {
      id: 1,
      empresa: 'Applaudo Studios',
      iniciales: 'AS',
      color: 'as',
      puesto: 'Desarrollador Frontend Jr.',
      area: 'Desarrollo de Software',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      clasificaciones: [
        'Recién graduados',
        'Sin experiencia'
      ],
      etiquetas: [
        'React',
        'TypeScript',
        'Sin experiencia'
      ],
      antiguedadDias: 2,
      publicada: 'Hace 2 días',
      compatibilidad: 87,
      salarioMinimo: 500,
      salarioMaximo: 800
    },
    {
      id: 2,
      empresa: 'Banco Agrícola',
      iniciales: 'BA',
      color: 'ba',
      puesto: 'Analista de Datos',
      area: 'Bases de Datos',
      ubicacion: 'Antiguo Cuscatlán',
      modalidad: 'Presencial',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['SQL', 'Power BI', 'Python'],
      antiguedadDias: 4,
      publicada: 'Hace 4 días',
      compatibilidad: 87,
      salarioMinimo: 700,
      salarioMaximo: 1000
    },
    {
      id: 3,
      empresa: 'Elaniin',
      iniciales: 'EL',
      color: 'el',
      puesto: 'QA Tester Trainee',
      area: 'QA y Testing',
      ubicacion: 'San Salvador',
      modalidad: 'Remoto',
      clasificaciones: ['Pasantía'],
      etiquetas: ['Testing', 'Pasantía'],
      antiguedadDias: 7,
      publicada: 'Hace 1 semana',
      compatibilidad: 87,
      salarioMinimo: 250,
      salarioMaximo: 400
    },
    {
      id: 4,
      empresa: 'Telefónica Tech',
      iniciales: 'TT',
      color: 'tt',
      puesto: 'Soporte Técnico N1',
      area: 'Soporte Técnico',
      ubicacion: 'San Salvador',
      modalidad: 'Presencial',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['Redes', 'Servicio al cliente'],
      antiguedadDias: 3,
      publicada: 'Hace 3 días',
      salarioMinimo: 450,
      salarioMaximo: 650
    },
    {
      id: 5,
      empresa: 'Hugo App',
      iniciales: 'HA',
      color: 'ha',
      puesto: 'Backend Developer',
      area: 'Desarrollo de Software',
      ubicacion: 'La Libertad',
      modalidad: 'Híbrido',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['Node.js', 'PostgreSQL'],
      antiguedadDias: 5,
      publicada: 'Hace 5 días',
      salarioMinimo: 900,
      salarioMaximo: 1300
    },
    {
      id: 6,
      empresa: 'Sertracen',
      iniciales: 'SE',
      color: 'se',
      puesto: 'Pasantía de Desarrollo',
      area: 'Desarrollo de Software',
      ubicacion: 'Soyapango',
      modalidad: 'Presencial',
      clasificaciones: [
        'Pasantía',
        'Sin experiencia'
      ],
      etiquetas: ['Pasantía', 'Sin experiencia'],
      antiguedadDias: 7,
      publicada: 'Hace 1 semana',

      // Oferta sin salario, supuesta como aprobada
      // para esta demostración.
      salarioMinimo: null,
      salarioMaximo: null
    }
  ];

  // ==========================================
  // NORMALIZAR TEXTO PARA LAS BÚSQUEDAS
  // ==========================================

  private normalizar(texto: string): string {
    return texto.normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLocaleLowerCase('es')
      .trim();
  }

  // ==========================================
  // MOSTRAR RANGO SALARIAL
  // ==========================================

  obtenerRangoSalarial(oferta: OfertaTrabajo): string {

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
  // VALIDAR FILTROS SALARIALES
  // ==========================================

  get errorFiltroSalarial(): string {

    const minimo = this.salarioMinimoFiltro ?? null;
    const maximo = this.salarioMaximoFiltro ?? null;

    const valores = [minimo, maximo];

    for (const valor of valores) {

      if (valor === null) {
        continue;
      }

      if (!Number.isFinite(valor) || valor < 0) {

        return 'Ingresa salarios válidos, mayores o iguales a cero.';
      }

      if (
        Math.abs(
          valor - Math.round(valor * 100) / 100
        ) > 0.00000001
      ) {

        return 'Los salarios deben tener como máximo dos decimales.';
      }
    }

    if (
      minimo !== null &&
      maximo !== null &&
      minimo > maximo
    ) {

      return 'El salario mínimo no puede superar al máximo.';
    }

    return '';
  }

  // ==========================================
  // COMPROBAR FILTRO SALARIAL
  // ==========================================

  private coincideSalario(oferta: OfertaTrabajo): boolean {

    const minimo = this.salarioMinimoFiltro ?? null;
    const maximo = this.salarioMaximoFiltro ?? null;

    // Sin filtro salarial, mostrar todas las ofertas.
    if (minimo === null && maximo === null) {
      return true;
    }

    // Excluir ofertas con salario no especificado
    // cuando se utiliza el filtro salarial.
    if (
      oferta.salarioMinimo === null ||
      oferta.salarioMaximo === null
    ) {
      return false;
    }

    // Comprobar coincidencia entre intervalos.
    const cumpleMinimo =
      minimo === null ||
      oferta.salarioMaximo >= minimo;

    const cumpleMaximo =
      maximo === null ||
      oferta.salarioMinimo <= maximo;

    return cumpleMinimo && cumpleMaximo;
  }

  // ==========================================
  // FILTRAR Y ORDENAR OFERTAS
  // ==========================================

  get ofertasFiltradas(): OfertaTrabajo[] {

    if (this.errorFiltroSalarial) {
      return [];
    }

    const busqueda = this.normalizar(
      this.busquedaAplicada
    );

    return this.ofertas.filter(oferta => {

      const contenido = this.normalizar([
        oferta.puesto,
        oferta.empresa,
        oferta.area,
        oferta.ubicacion,
        oferta.modalidad,
        ...oferta.etiquetas
      ].join(' '));

      const coincideTexto =
        !busqueda || contenido.includes(busqueda);

      const coincideArea =
        !this.areaSeleccionada ||
        oferta.area === this.areaSeleccionada;

      const coincideUbicacion =
        !this.ubicacionSeleccionada ||
        oferta.ubicacion === this.ubicacionSeleccionada;

      const coincideModalidad =
        this.modalidadesSeleccionadas.length === 0 ||
        this.modalidadesSeleccionadas.includes(
          oferta.modalidad
        );

      const coincideClasificacion =
        this.clasificacionesSeleccionadas.length === 0 ||
        this.clasificacionesSeleccionadas.some(
          tipo => oferta.clasificaciones.includes(tipo)
        );

      const coincideHabilidad =
        this.habilidadesSeleccionadas.length === 0 ||
        this.habilidadesSeleccionadas.some(
          habilidad => contenido.includes(
            this.normalizar(habilidad)
          )
        );

      const coincideRangoSalarial =
        this.coincideSalario(oferta);

      return (
        coincideTexto &&
        coincideArea &&
        coincideUbicacion &&
        coincideModalidad &&
        coincideClasificacion &&
        coincideHabilidad &&
        coincideRangoSalarial
      );

    }).sort((a, b) =>

      this.ordenSeleccionado === 'compatibilidad'

        ? (b.compatibilidad ?? -1) -
          (a.compatibilidad ?? -1) ||
          a.antiguedadDias - b.antiguedadDias

        : a.antiguedadDias - b.antiguedadDias
    );
  }

  // ==========================================
  // COMPROBAR FILTROS ACTIVOS
  // ==========================================

  get hayFiltrosActivos(): boolean {

    return Boolean(
      this.busquedaAplicada ||
      this.areaSeleccionada ||
      this.ubicacionSeleccionada ||
      this.modalidadesSeleccionadas.length ||
      this.clasificacionesSeleccionadas.length ||
      this.habilidadesSeleccionadas.length ||
      this.salarioMinimoFiltro !== null ||
      this.salarioMaximoFiltro !== null
    );
  }

  // ==========================================
  // BUSCAR OFERTAS
  // ==========================================

  buscar(): void {

    this.busquedaAplicada =
      this.textoBusqueda.trim();
  }

  // ==========================================
  // LIMPIAR TODOS LOS FILTROS
  // ==========================================

  limpiarFiltros(): void {

    this.textoBusqueda = '';
    this.busquedaAplicada = '';
    this.areaSeleccionada = '';
    this.ubicacionSeleccionada = '';
    this.modalidadesSeleccionadas = [];
    this.clasificacionesSeleccionadas = [];
    this.habilidadesSeleccionadas = [];
    this.ordenSeleccionado = 'recientes';

    this.salarioMinimoFiltro = null;
    this.salarioMaximoFiltro = null;
  }

  // ==========================================
  // FILTROS DE MODALIDAD
  // ==========================================

  alternarModalidad(modalidad: Modalidad): void {

    this.modalidadesSeleccionadas =
      this.modalidadesSeleccionadas.includes(modalidad)

        ? this.modalidadesSeleccionadas.filter(
            valor => valor !== modalidad
          )

        : [...this.modalidadesSeleccionadas, modalidad];
  }

  // ==========================================
  // FILTROS DE CLASIFICACIÓN
  // ==========================================

  alternarClasificacion(
    clasificacion: Clasificacion
  ): void {

    this.clasificacionesSeleccionadas =
      this.clasificacionesSeleccionadas.includes(
        clasificacion
      )

        ? this.clasificacionesSeleccionadas.filter(
            valor => valor !== clasificacion
          )

        : [
            ...this.clasificacionesSeleccionadas,
            clasificacion
          ];
  }

  // ==========================================
  // FILTROS DE HABILIDADES
  // ==========================================

  alternarHabilidad(habilidad: string): void {

    this.habilidadesSeleccionadas =
      this.habilidadesSeleccionadas.includes(habilidad)

        ? this.habilidadesSeleccionadas.filter(
            valor => valor !== habilidad
          )

        : [...this.habilidadesSeleccionadas, habilidad];
  }

  // ==========================================
  // CONTADORES
  // ==========================================

  contarModalidad(modalidad: Modalidad): number {

    return this.ofertas.filter(
      oferta => oferta.modalidad === modalidad
    ).length;
  }

  contarClasificacion(
    clasificacion: Clasificacion
  ): number {

    return this.ofertas.filter(
      oferta => oferta.clasificaciones.includes(
        clasificacion
      )
    ).length;
  }

  // ==========================================
  // FAVORITOS
  // ==========================================

  esFavorita(id: number): boolean {

    return this.favoritos.includes(id);
  }

  alternarFavorita(oferta: OfertaTrabajo): void {

    if (this.esFavorita(oferta.id)) {

      this.favoritos = this.favoritos.filter(
        id => id !== oferta.id
      );

      this.mensajeAccion =
        `Quitaste «${oferta.puesto}» de favoritos de esta demostración.`;

    } else {

      this.favoritos = [
        ...this.favoritos,
        oferta.id
      ];

      this.mensajeAccion =
        `Guardaste «${oferta.puesto}» como favorito en esta pantalla.`;
    }
  }

  // ==========================================
  // ABRIR Y CERRAR DETALLE DE OFERTA
  // ==========================================

  abrirOferta(oferta: OfertaTrabajo): void {

    // Limpiar cualquier CV de una oferta anterior.
    this.quitarCV();

    this.ofertaSeleccionada = oferta;
    this.notificacionesAbiertas = false;
  }

  cerrarOferta(): void {

    this.ofertaSeleccionada = null;

    // No conservar archivos de una postulación cancelada.
    this.quitarCV();
  }

  // ==========================================
  // NUEVO: VALIDAR ARCHIVO CV
  // ==========================================

  private validarCV(archivo: File): string {

    // Comprobar extensión PDF.
    const extensionValida =
      archivo.name.toLowerCase().endsWith('.pdf');

    // Comprobar tipo MIME cuando está disponible.
    const tipoValido =
      archivo.type === 'application/pdf' ||
      archivo.type === '' ||
      archivo.type === 'application/octet-stream';

    if (!extensionValida || !tipoValido) {

      return 'Solo se permiten archivos en formato PDF.';
    }

    // Impedir documentos vacíos.
    if (archivo.size === 0) {

      return 'El archivo seleccionado está vacío.';
    }

    // Validar tamaño máximo.
    if (archivo.size > this.maximoBytesCV) {

      return 'El CV no puede superar los 5 MB.';
    }

    return '';
  }

  // ==========================================
  // NUEVO: SELECCIONAR ARCHIVO CV
  // ==========================================

  seleccionarCV(evento: Event): void {

    this.errorCV = '';
    this.archivoCV = null;

    const input =
      evento.target as HTMLInputElement;

    const archivo =
      input.files?.[0] ?? null;

    if (!archivo) {
      return;
    }

    const error = this.validarCV(archivo);

    if (error) {

      this.errorCV = error;

      // Permitir volver a seleccionar el archivo.
      input.value = '';

      return;
    }

    // El documento permanece únicamente en memoria.
    this.archivoCV = archivo;
  }

  // ==========================================
  // NUEVO: QUITAR ARCHIVO CV
  // ==========================================

  quitarCV(): void {

    this.archivoCV = null;
    this.errorCV = '';
  }

  // ==========================================
  // NUEVO: MOSTRAR TAMAÑO DEL ARCHIVO
  // ==========================================

  obtenerTamanoCV(bytes: number): string {

    if (bytes < 1024) {
      return `${bytes} B`;
    }

    if (bytes < 1024 * 1024) {

      return `${(bytes / 1024).toFixed(2)} KB`;
    }

    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  // ==========================================
  // POSTULACIONES SIMULADAS
  // ==========================================

  estaPostulada(id: number): boolean {

    return this.postulacionesSimuladas.includes(id);
  }

  // Comprobar si puede realizar la postulación.
  get puedePostular(): boolean {

    return (
      this.ofertaSeleccionada !== null &&
      !this.estaPostulada(this.ofertaSeleccionada.id) &&
      this.archivoCV !== null &&
      !this.errorCV
    );
  }

  // ==========================================
  // NUEVO: OBTENER NOMBRE DEL CV POSTULADO
  // ==========================================

  obtenerNombreCVPostulado(id: number): string {

    return this.cvPostulacionesSimuladas[id] ?? '';
  }

  // ==========================================
  // POSTULAR CON CV ADJUNTO - RF-08
  // ==========================================

  simularPostulacion(): void {

    // Comprobar que exista una oferta seleccionada.
    if (!this.ofertaSeleccionada) {
      return;
    }

    // Evitar postulaciones duplicadas.
    if (this.estaPostulada(this.ofertaSeleccionada.id)) {

      return;
    }

    // El CV es obligatorio.
    if (!this.archivoCV) {

      this.errorCV =
        'Debes adjuntar tu CV antes de postularte.';

      return;
    }

    // Validar nuevamente antes de confirmar.
    const error = this.validarCV(this.archivoCV);

    if (error) {

      this.errorCV = error;
      return;
    }

    const oferta = this.ofertaSeleccionada;
    const nombreCV = this.archivoCV.name;

    // Registrar la postulación en memoria.
    this.postulacionesSimuladas = [
      ...this.postulacionesSimuladas,
      oferta.id
    ];

    // Guardar únicamente el nombre del CV.
    this.cvPostulacionesSimuladas = {
      ...this.cvPostulacionesSimuladas,
      [oferta.id]: nombreCV
    };

    // Liberar el archivo seleccionado.
    this.quitarCV();

    // Confirmación de la simulación.
    this.mensajeAccion =
      `Postulación simulada a «${oferta.puesto}» ` +
      `con el CV «${nombreCV}». ` +
      'El documento no se envió al servidor ni ' +
      'se guardó en PostgreSQL. ' +
      'No aparecerá todavía en Mis postulaciones.';
  }

  limpiarMensaje(): void {

    this.mensajeAccion = '';
  }

  // ==========================================
  // MENÚ MÓVIL
  // ==========================================

  alternarMenu(): void {

    this.menuAbierto = !this.menuAbierto;
    this.notificacionesAbiertas = false;
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
  }

  cerrarSeguridad(): void {

    this.seguridadAbierta = false;
  }

  // ==========================================
  // CERRAR ELEMENTOS CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarOferta();
    this.cerrarSeguridad();
  }
}
