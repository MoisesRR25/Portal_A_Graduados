
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  inject
} from '@angular/core';

import { RouterLink } from '@angular/router';
import { readSheet } from 'read-excel-file/browser';
import writeExcelFile from 'write-excel-file/browser';

interface RegistroGraduado {
  fila: number;
  nombre: string;
  correo: string;
  carnet: string;
  anio: string;
  estadoLaboral: string;
  errores: string[];
}

@Component({
  selector: 'app-carga-masiva',
  imports: [RouterLink],
  templateUrl: './carga-masiva.html',
  styleUrl: './carga-masiva.css'
})
export class CargaMasiva {

  // ========================================
  // DETECTOR DE CAMBIOS DE ANGULAR
  // ========================================

  private readonly cdr = inject(ChangeDetectorRef);

  // ========================================
  // NAVEGACIÓN Y PANELES
  // ========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ========================================
  // ESTADO DE ARCHIVO
  // ========================================

  readonly tamanoMaximo = 10 * 1024 * 1024;
  readonly anioActual = new Date().getFullYear();

  // Formato provisional: dos letras y cinco números.
  private readonly formatoCarnet = /^[A-Z]{2}\d{5}$/;

  procesandoArchivo = false;
  arrastrandoArchivo = false;
  archivoValidado = false;

  nombreArchivo = '';
  fechaAnalisis = '';

  errorArchivo = '';
  mensajeInformativo = '';

  // ========================================
  // RESULTADOS LOCALES
  // ========================================

  registros: RegistroGraduado[] = [];

  soloErrores = false;
  limiteVistaPrevia = 10;

  confirmacionAbierta = false;

  // NUEVO: controlar la confirmación de la carga.
  simulacionConfirmada = false;

  // ========================================
  // RESUMEN ORIGINAL DE DEMOSTRACIÓN
  // ========================================

  // Estos valores son ficticios.
  // No representan una carga realizada.

  readonly resumenEjemplo = {
    archivo: 'graduados_2025_semestre1.xlsx',
    fecha: '15 jun. 2025',
    total: 120,
    validos: 116,
    rechazados: 4
  };

  // ========================================
  // CONTADORES DEL ARCHIVO ACTUAL
  // ========================================

  get totalRegistrosActual(): number {
    return this.registros.length;
  }

  get totalValidosActual(): number {
    return this.registros.filter(
      registro => registro.errores.length === 0
    ).length;
  }

  get totalErroresActual(): number {
    return this.registros.filter(
      registro => registro.errores.length > 0
    ).length;
  }

  // NUEVO: pendientes de activación simulados.

  get totalPendientesActivacion(): number {
    return this.simulacionConfirmada
      ? this.totalValidosActual
      : 0;
  }

  // ========================================
  // CONTADORES VISIBLES EN RESUMEN
  // ========================================

  get totalResumen(): number {
    return this.archivoValidado
      ? this.totalRegistrosActual
      : this.resumenEjemplo.total;
  }

  get validosResumen(): number {
    return this.archivoValidado
      ? this.totalValidosActual
      : this.resumenEjemplo.validos;
  }

  get rechazadosResumen(): number {
    return this.archivoValidado
      ? this.totalErroresActual
      : this.resumenEjemplo.rechazados;
  }

  get nombreResumen(): string {
    return this.archivoValidado
      ? this.nombreArchivo
      : this.resumenEjemplo.archivo;
  }

  get fechaResumen(): string {
    return this.archivoValidado
      ? this.fechaAnalisis
      : this.resumenEjemplo.fecha;
  }

  get estadoResumen(): string {

    if (!this.archivoValidado) {
      return 'Ejemplo visual';
    }

    if (this.simulacionConfirmada) {
      return 'Simulación finalizada';
    }

    if (this.totalErroresActual > 0) {
      return 'Validado con observaciones';
    }

    return 'Validado sin observaciones';
  }

  // ========================================
  // NUEVO: ESTADO DE CADA REGISTRO
  // ========================================

  obtenerEstadoRegistro(registro: RegistroGraduado): string {

    if (registro.errores.length > 0) {
      return 'Rechazado';
    }

    if (this.simulacionConfirmada) {
      return 'Pendiente de activación (simulado)';
    }

    return 'Apto para carga';
  }

  // ========================================
  // VISTA PREVIA
  // ========================================

  get registrosFiltrados(): RegistroGraduado[] {

    if (this.soloErrores) {
      return this.registros.filter(
        registro => registro.errores.length > 0
      );
    }

    return this.registros;
  }

  get registrosVistaPrevia(): RegistroGraduado[] {
    return this.registrosFiltrados.slice(
      0,
      this.limiteVistaPrevia
    );
  }

  get hayMasRegistros(): boolean {
    return (
      this.registrosFiltrados.length >
      this.limiteVistaPrevia
    );
  }

  alternarFiltroErrores(): void {
    this.soloErrores = !this.soloErrores;
    this.limiteVistaPrevia = 10;
  }

  mostrarMasRegistros(): void {
    this.limiteVistaPrevia += 50;
  }

  // ========================================
  // MENÚ MÓVIL
  // ========================================

  alternarMenu(): void {

    this.menuAbierto = !this.menuAbierto;

    if (this.menuAbierto) {
      this.cerrarNotificaciones();
    }
  }

  cerrarMenu(): void {
    this.menuAbierto = false;
  }

  // ========================================
  // NOTIFICACIONES
  // ========================================

  alternarNotificaciones(): void {
    this.notificacionesAbiertas =
      !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {
    this.notificacionesAbiertas = false;
  }

  // ========================================
  // ACCESO Y SEGURIDAD
  // ========================================

  abrirSeguridad(): void {

    this.seguridadAbierta = true;

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarConfirmacion();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ========================================
  // DESCARGAR PLANTILLA EXCEL
  // ========================================

  async descargarPlantilla(): Promise<void> {

    this.errorArchivo = '';
    this.mensajeInformativo = '';

    try {

      const encabezados = [
        'Nombre completo',
        'Correo',
        'Carnet',
        'Año de graduación',
        'Estado laboral'
      ];

      const columnas = [
        { width: 34 },
        { width: 32 },
        { width: 18 },
        { width: 22 },
        { width: 28 }
      ];

      const datos = [
        encabezados
      ];

      await writeExcelFile(
        datos,
        {
          sheet: 'Graduados',
          columns: columnas,
          stickyRowsCount: 1
        }
      ).toFile('plantilla_graduados.xlsx');

      this.mensajeInformativo =
        'Se generó la plantilla Excel. ' +
        'Completa los datos debajo de los encabezados ' +
        'y conserva el formato .xlsx.';

    } catch {

      this.errorArchivo =
        'No se pudo generar la plantilla Excel. ' +
        'Comprueba que las bibliotecas estén instaladas.';

    } finally {

      this.cdr.markForCheck();
    }
  }

  // ========================================
  // SELECCIONAR ARCHIVO
  // ========================================

  async alSeleccionarArchivo(
    evento: Event
  ): Promise<void> {

    const entrada = evento.target as HTMLInputElement;
    const archivo = entrada.files?.item(0);

    // Permite seleccionar nuevamente el mismo archivo.
    entrada.value = '';

    if (!archivo) {
      return;
    }

    await this.procesarArchivo(archivo);
  }

  // ========================================
  // ARRASTRAR ARCHIVO
  // ========================================

  permitirSoltar(evento: DragEvent): void {

    evento.preventDefault();

    if (this.procesandoArchivo) {
      return;
    }

    this.arrastrandoArchivo = true;
  }

  salirArrastre(evento: DragEvent): void {

    evento.preventDefault();

    this.arrastrandoArchivo = false;
  }

  async alSoltarArchivo(
    evento: DragEvent
  ): Promise<void> {

    evento.preventDefault();

    this.arrastrandoArchivo = false;

    if (this.procesandoArchivo) {
      return;
    }

    const archivos = evento.dataTransfer?.files;

    if (!archivos || archivos.length === 0) {
      return;
    }

    if (archivos.length > 1) {

      this.errorArchivo =
        'Selecciona únicamente un archivo Excel a la vez.';

      return;
    }

    await this.procesarArchivo(archivos[0]);
  }

  // ========================================
  // NORMALIZAR ENCABEZADOS
  // ========================================

  private normalizarEncabezado(
    valor: unknown
  ): string {

    return String(valor ?? '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .replace(/\s+/g, ' ')
      .trim();
  }

  // ========================================
  // CONVERTIR CELDA A TEXTO
  // ========================================

  private valorCelda(
    valor: unknown
  ): string {

    if (valor === null || valor === undefined) {
      return '';
    }

    return String(valor).trim();
  }

  // ========================================
  // VALIDAR ARCHIVO EXCEL
  // ========================================

  async procesarArchivo(
    archivo: File
  ): Promise<void> {

    if (this.procesandoArchivo) {
      return;
    }

    // Limpiar resultados de un análisis anterior.

    this.errorArchivo = '';
    this.mensajeInformativo = '';
    this.nombreArchivo = '';
    this.fechaAnalisis = '';
    this.archivoValidado = false;

    this.registros = [];
    this.soloErrores = false;
    this.limiteVistaPrevia = 10;

    this.simulacionConfirmada = false;

    this.cerrarConfirmacion();

    // ========================================
    // VALIDAR EXTENSIÓN
    // ========================================

    if (!archivo.name.toLowerCase().endsWith('.xlsx')) {

      this.errorArchivo =
        'Formato no permitido. ' +
        'Selecciona un archivo Excel con extensión .xlsx. ' +
        'Los archivos .xls antiguos no son compatibles.';

      return;
    }

    // ========================================
    // VALIDAR ARCHIVO VACÍO
    // ========================================

    if (archivo.size === 0) {

      this.errorArchivo =
        'El archivo está vacío. Selecciona otro archivo.';

      return;
    }

    // ========================================
    // VALIDAR TAMAÑO MÁXIMO
    // ========================================

    if (archivo.size > this.tamanoMaximo) {

      this.errorArchivo =
        'El archivo supera el tamaño máximo de 10 MB.';

      return;
    }

    // ========================================
    // INICIAR PROCESAMIENTO
    // ========================================

    this.procesandoArchivo = true;

    // Conservar corrección de actualización de Angular.
    this.cdr.markForCheck();

    let temporizadorLectura:
      ReturnType<typeof setTimeout> | undefined;

    try {

      console.info(
        '[Carga masiva] Iniciando lectura:',
        archivo.size,
        'bytes'
      );

      // ========================================
      // LEER ARCHIVO EXCEL
      // ========================================
      // Límite de espera de 15 segundos.

      const filas = await Promise.race([

        readSheet(archivo),

        new Promise<never>((_, reject) => {

          temporizadorLectura = setTimeout(() => {

            reject(
              new Error(
                'Tiempo máximo de lectura superado (15 segundos).'
              )
            );

          }, 15000);
        })
      ]);

      console.info(
        '[Carga masiva] Filas leídas:',
        filas.length
      );

      // ========================================
      // VALIDAR CANTIDAD DE FILAS
      // ========================================

      if (filas.length < 2) {

        this.errorArchivo =
          'El archivo debe contener los encabezados ' +
          'y al menos una fila con datos.';

        return;
      }

      // ========================================
      // VALIDAR ENCABEZADOS
      // ========================================

      const encabezados = filas[0].map(
        valor => this.normalizarEncabezado(valor)
      );

      const columnasRequeridas = [
        'nombre completo',
        'correo',
        'carnet',
        'ano de graduacion',
        'estado laboral'
      ];

      const posiciones = columnasRequeridas.map(
        nombre => encabezados.indexOf(nombre)
      );

      if (posiciones.some(posicion => posicion === -1)) {

        this.errorArchivo =
          'El archivo no contiene las cinco columnas ' +
          'requeridas: Nombre completo, Correo, Carnet, ' +
          'Año de graduación y Estado laboral. ' +
          'Descarga la plantilla y vuelve a intentarlo.';

        return;
      }

      // ========================================
      // VALIDAR ENCABEZADOS DUPLICADOS
      // ========================================

      const encabezadosRepetidos =
        columnasRequeridas.some(
          nombre =>
            encabezados.filter(
              encabezado => encabezado === nombre
            ).length > 1
        );

      if (encabezadosRepetidos) {

        this.errorArchivo =
          'El archivo contiene encabezados duplicados. ' +
          'Cada columna requerida debe aparecer una sola vez.';

        return;
      }

      // ========================================
      // CONTROL DE CORREOS Y CARNETS DUPLICADOS
      // ========================================
      // Solo se valida dentro del archivo.
      // No se consulta PostgreSQL.

      const correosEncontrados = new Set<string>();
      const carnetsEncontrados = new Set<string>();

      const resultados: RegistroGraduado[] = [];

      // ========================================
      // RECORRER LOS REGISTROS
      // ========================================

      for (let indice = 1; indice < filas.length; indice++) {

        const fila = filas[indice];

        if (!fila) {
          continue;
        }

        const nombre = this.valorCelda(
          fila[posiciones[0]]
        );

        const correo = this.valorCelda(
          fila[posiciones[1]]
        );

        const carnet = this.valorCelda(
          fila[posiciones[2]]
        ).toUpperCase();

        const anio = this.valorCelda(
          fila[posiciones[3]]
        );

        const estadoLaboral = this.valorCelda(
          fila[posiciones[4]]
        );

        // Omitir filas completamente vacías.

        if (
          !nombre &&
          !correo &&
          !carnet &&
          !anio &&
          !estadoLaboral
        ) {
          continue;
        }

        const errores: string[] = [];

        // ========================================
        // VALIDAR NOMBRE
        // ========================================

        if (nombre.length < 3) {

          errores.push(
            'Nombre completo obligatorio (mínimo 3 caracteres)'
          );
        }

        // ========================================
        // VALIDAR CORREO
        // ========================================

        const correoNormalizado =
          correo.toLowerCase();

        const formatoCorreo =
          /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!formatoCorreo.test(correo)) {

          errores.push(
            'Correo electrónico inválido'
          );
        }

        // Comprobar correos duplicados.

        if (correoNormalizado) {

          if (
            correosEncontrados.has(correoNormalizado)
          ) {

            errores.push(
              'Correo duplicado dentro del archivo'
            );

          } else {

            correosEncontrados.add(correoNormalizado);
          }
        }

        // ========================================
        // VALIDAR CARNET
        // ========================================

        if (!carnet) {

          errores.push(
            'Carnet obligatorio'
          );

        } else if (!this.formatoCarnet.test(carnet)) {

          errores.push(
            'Carnet inválido: debe contener dos letras ' +
            'y cinco números. Ejemplo: RR23015'
          );
        }

        // Comprobar carnets duplicados.

        if (carnet) {

          if (carnetsEncontrados.has(carnet)) {

            errores.push(
              'Carnet duplicado dentro del archivo'
            );

          } else {

            carnetsEncontrados.add(carnet);
          }
        }

        // ========================================
        // VALIDAR AÑO DE GRADUACIÓN
        // ========================================

        const formatoAnio = /^\d{4}$/;
        const numeroAnio = Number(anio);

        if (
          !formatoAnio.test(anio) ||
          !Number.isInteger(numeroAnio) ||
          numeroAnio < 1990 ||
          numeroAnio > this.anioActual
        ) {

          errores.push(
            'Año de graduación inválido: debe estar entre 1990 y ' +
            this.anioActual
          );
        }

        // ========================================
        // VALIDAR ESTADO LABORAL
        // ========================================

        if (!estadoLaboral) {

          errores.push(
            'Estado laboral obligatorio'
          );

        } else if (estadoLaboral.length > 80) {

          errores.push(
            'Estado laboral demasiado largo'
          );
        }

        // ========================================
        // GUARDAR RESULTADOS EN MEMORIA
        // ========================================

        resultados.push({
          fila: indice + 1,
          nombre,
          correo,
          carnet,
          anio,
          estadoLaboral,
          errores
        });
      }

      // ========================================
      // COMPROBAR QUE EXISTAN REGISTROS
      // ========================================

      if (resultados.length === 0) {

        this.errorArchivo =
          'El archivo no contiene registros de graduados.';

        return;
      }

      // ========================================
      // MOSTRAR RESULTADOS
      // ========================================

      this.registros = resultados;

      this.nombreArchivo = archivo.name;

      this.fechaAnalisis = new Date()
        .toLocaleDateString('es-SV', {
          day: '2-digit',
          month: 'short',
          year: 'numeric'
        });

      this.archivoValidado = true;

      const validos = this.totalValidosActual;
      const rechazados = this.totalErroresActual;

      this.mensajeInformativo =
        'Archivo leído y validado localmente. ' +
        `Resultado: ${validos} ` +
        `${validos === 1 ? 'registro válido' : 'registros válidos'} y ` +
        `${rechazados} ` +
        `${rechazados === 1 ? 'registro rechazado' : 'registros rechazados'}. ` +
        'No se guardó ningún dato en PostgreSQL.';

    } catch (error) {

      console.error(
        '[Carga masiva] Error al leer o validar Excel:',
        error
      );

      this.errorArchivo =
        'No fue posible leer el archivo Excel. ' +
        'Comprueba que sea un .xlsx válido, ' +
        'que no esté protegido con contraseña ' +
        'y que no esté dañado.' +
        (
          error instanceof Error
            ? ' Detalle: ' + error.message
            : ''
        );

    } finally {

      // ========================================
      // FINALIZAR PROCESAMIENTO
      // ========================================

      if (temporizadorLectura !== undefined) {
        clearTimeout(temporizadorLectura);
      }

      this.procesandoArchivo = false;

      // CORRECCIÓN CONSERVADA:
      // Notificar a Angular que debe actualizar la interfaz.

      this.cdr.markForCheck();
    }
  }

  // ========================================
  // DESCARGAR REPORTE DE ERRORES
  // ========================================

  descargarReporteErrores(): void {

    this.errorArchivo = '';

    if (!this.archivoValidado) {

      this.errorArchivo =
        'Selecciona y valida primero un archivo Excel. ' +
        'El resumen inicial es solo un ejemplo.';

      return;
    }

    const rechazados = this.registros.filter(
      registro => registro.errores.length > 0
    );

    if (rechazados.length === 0) {

      this.mensajeInformativo =
        'No se encontraron errores en el archivo seleccionado.';

      return;
    }

    const filas: string[][] = [

      [
        'Fila',
        'Nombre completo',
        'Correo',
        'Carnet',
        'Año de graduación',
        'Estado laboral',
        'Errores'
      ],

      ...rechazados.map(registro => [
        String(registro.fila),
        registro.nombre,
        registro.correo,
        registro.carnet,
        registro.anio,
        registro.estadoLaboral,
        registro.errores.join(' | ')
      ])
    ];

    this.generarCSV(
      filas,
      'reporte_errores_graduados.csv'
    );

    this.mensajeInformativo =
      'Se generó el reporte de errores en formato CSV, ' +
      'compatible con Excel.';
  }

  // ========================================
  // NUEVO: REPORTE COMPLETO DE RESULTADOS
  // ========================================

  descargarReporteResultados(): void {

    this.errorArchivo = '';

    if (!this.archivoValidado) {

      this.errorArchivo =
        'Selecciona y valida primero un archivo Excel.';

      return;
    }

    const filas: string[][] = [

      [
        'Fila',
        'Nombre completo',
        'Correo',
        'Carnet',
        'Año de graduación',
        'Estado laboral',
        'Resultado de validación',
        'Estado de activación simulado',
        'Errores'
      ],

      ...this.registros.map(registro => [

        String(registro.fila),
        registro.nombre,
        registro.correo,
        registro.carnet,
        registro.anio,
        registro.estadoLaboral,

        registro.errores.length === 0
          ? 'Aceptado en validación local'
          : 'Rechazado',

        this.obtenerEstadoRegistro(registro),

        registro.errores.length === 0
          ? 'Sin errores'
          : registro.errores.join(' | ')
      ])
    ];

    this.generarCSV(
      filas,
      'reporte_resultados_graduados.csv'
    );

    this.mensajeInformativo =
      'Se generó el reporte completo de registros ' +
      'aceptados y rechazados. ' +
      'El reporte corresponde únicamente a la validación local.';
  }

  // ========================================
  // GENERAR ARCHIVOS CSV
  // ========================================

  private generarCSV(
    filas: string[][],
    nombre: string
  ): void {

    const contenido =
      '\uFEFFsep=;\r\n' +
      filas.map(
        fila => fila.map(
          celda => this.escaparCSV(celda)
        ).join(';')
      ).join('\r\n');

    const archivo = new Blob(
      [contenido],
      {
        type: 'text/csv;charset=utf-8;'
      }
    );

    this.descargarBlob(
      archivo,
      nombre
    );
  }

  // ========================================
  // PROTEGER CELDAS EN EXPORTACIÓN CSV
  // ========================================

  private escaparCSV(valor: string): string {

    // Evitar que los valores se interpreten
    // como fórmulas al abrir el CSV en Excel.

    const contenido =
      /^[\s]*[=+\-@]/.test(valor)
        ? "'" + valor
        : valor;

    return '"' +
      contenido.replace(/"/g, '""') +
      '"';
  }

  // ========================================
  // DESCARGA DE ARCHIVOS
  // ========================================

  private descargarBlob(
    archivo: Blob,
    nombre: string
  ): void {

    const url = URL.createObjectURL(archivo);

    const enlace = document.createElement('a');

    enlace.href = url;
    enlace.download = nombre;

    document.body.appendChild(enlace);

    enlace.click();
    enlace.remove();

    // Liberar la URL después de la descarga.

    setTimeout(() => {
      URL.revokeObjectURL(url);
    }, 1000);
  }

  // ========================================
  // CONFIRMACIÓN DE SIMULACIÓN
  // ========================================

  abrirConfirmacion(): void {

    if (
      !this.archivoValidado ||
      this.totalValidosActual === 0 ||
      this.simulacionConfirmada ||
      this.procesandoArchivo
    ) {
      return;
    }

    this.confirmacionAbierta = true;
  }

  cerrarConfirmacion(): void {
    this.confirmacionAbierta = false;
  }

  confirmarSimulacion(): void {

    if (
      !this.archivoValidado ||
      this.totalValidosActual === 0 ||
      this.simulacionConfirmada ||
      !this.confirmacionAbierta
    ) {

      this.cerrarConfirmacion();
      return;
    }

    // NUEVO: marcar la carga como simulada.
    // No se crean cuentas ni se importan registros reales.

    this.simulacionConfirmada = true;

    this.errorArchivo = '';

    const validos = this.totalValidosActual;
    const rechazados = this.totalErroresActual;

    this.mensajeInformativo =
      'Simulación finalizada. ' +
      `${validos} ` +
      `${validos === 1 ? 'registro quedaría' : 'registros quedarían'} ` +
      'en estado pendiente de activación. ' +
      `${rechazados} ` +
      `${rechazados === 1 ? 'registro sería rechazado' : 'registros serían rechazados'}. ` +
      'No se importó ningún graduado ni se guardaron datos en PostgreSQL.';

    this.cerrarConfirmacion();
  }

  // ========================================
  // LIMPIAR RESULTADOS
  // ========================================

  limpiarAnalisis(): void {

    if (this.procesandoArchivo) {
      return;
    }

    this.registros = [];

    this.archivoValidado = false;
    this.procesandoArchivo = false;
    this.arrastrandoArchivo = false;

    this.nombreArchivo = '';
    this.fechaAnalisis = '';

    this.errorArchivo = '';
    this.mensajeInformativo = '';

    this.soloErrores = false;
    this.limiteVistaPrevia = 10;

    this.simulacionConfirmada = false;

    this.cerrarConfirmacion();
  }

  cerrarMensajes(): void {

    this.errorArchivo = '';
    this.mensajeInformativo = '';
  }

  // ========================================
  // TECLA ESCAPE
  // ========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarConfirmacion();
  }
}
