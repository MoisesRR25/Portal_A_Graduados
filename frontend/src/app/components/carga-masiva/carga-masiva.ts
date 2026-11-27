
import { Component, HostListener } from '@angular/core';
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

    if (this.totalErroresActual > 0) {
      return 'Validado con observaciones';
    }

    return 'Validado sin observaciones';
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

    this.cerrarConfirmacion();

    // Validar extensión.
    if (!archivo.name.toLowerCase().endsWith('.xlsx')) {

      this.errorArchivo =
        'Formato no permitido. ' +
        'Selecciona un archivo Excel con extensión .xlsx. ' +
        'Los archivos .xls antiguos no son compatibles.';

      return;
    }

    // Validar archivo vacío.
    if (archivo.size === 0) {

      this.errorArchivo =
        'El archivo está vacío. Selecciona otro archivo.';

      return;
    }

    // Validar tamaño máximo.
    if (archivo.size > this.tamanoMaximo) {

      this.errorArchivo =
        'El archivo supera el tamaño máximo de 10 MB.';

      return;
    }

    this.procesandoArchivo = true;

    try {

      // Lee la primera hoja del archivo Excel.
      const filas = await readSheet(archivo);

      if (filas.length < 2) {

        this.errorArchivo =
          'El archivo debe contener los encabezados ' +
          'y al menos una fila con datos.';

        return;
      }

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

      // Comprobar todas las columnas.
      if (posiciones.some(posicion => posicion === -1)) {

        this.errorArchivo =
          'El archivo no contiene las cinco columnas ' +
          'requeridas: Nombre completo, Correo, Carnet, ' +
          'Año de graduación y Estado laboral. ' +
          'Descarga la plantilla y vuelve a intentarlo.';

        return;
      }

      // Comprobar encabezados repetidos.
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

      // Registrar correos y carnets dentro del archivo.
      // NO se consulta la base de datos.
      const correosEncontrados = new Set<string>();
      const carnetsEncontrados = new Set<string>();

      const resultados: RegistroGraduado[] = [];

      // La fila 1 contiene los encabezados.
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
        );

        const anio = this.valorCelda(
          fila[posiciones[3]]
        );

        const estadoLaboral = this.valorCelda(
          fila[posiciones[4]]
        );

        // Omitir las filas completamente vacías.
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

        // ====================================
        // VALIDAR NOMBRE
        // ====================================

        if (nombre.length < 3) {

          errores.push(
            'Nombre completo obligatorio (mínimo 3 caracteres)'
          );

        }

        // ====================================
        // VALIDAR CORREO
        // ====================================

        const correoNormalizado =
          correo.toLowerCase();

        const formatoCorreo =
          /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!formatoCorreo.test(correo)) {

          errores.push(
            'Correo electrónico inválido'
          );

        }

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

        // ====================================
        // VALIDAR CARNET
        // ====================================

        const carnetNormalizado =
          carnet.toUpperCase();

        if (!carnet) {

          errores.push(
            'Carnet obligatorio'
          );

        } else if (carnet.length > 40) {

          errores.push(
            'Carnet demasiado largo'
          );

        }

        if (carnetNormalizado) {

          if (
            carnetsEncontrados.has(carnetNormalizado)
          ) {

            errores.push(
              'Carnet duplicado dentro del archivo'
            );

          } else {

            carnetsEncontrados.add(carnetNormalizado);

          }
        }

        // ====================================
        // VALIDAR AÑO DE GRADUACIÓN
        // ====================================

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

        // ====================================
        // VALIDAR ESTADO LABORAL
        // ====================================

        if (!estadoLaboral) {

          errores.push(
            'Estado laboral obligatorio'
          );

        } else if (estadoLaboral.length > 80) {

          errores.push(
            'Estado laboral demasiado largo'
          );

        }

        // ====================================
        // GUARDAR RESULTADO EN MEMORIA
        // ====================================

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

      if (resultados.length === 0) {

        this.errorArchivo =
          'El archivo no contiene registros de graduados.';

        return;
      }

      this.registros = resultados;

      this.nombreArchivo = archivo.name;

      this.fechaAnalisis = new Date()
        .toLocaleDateString('es-SV', {
          day: '2-digit',
          month: 'short',
          year: 'numeric'
        });

      this.archivoValidado = true;

      this.mensajeInformativo =
        'Archivo leído y validado localmente. ' +
        'Se encontraron ' +
        this.totalValidosActual +
        ' registros sin errores y ' +
        this.totalErroresActual +
        ' registros con observaciones. ' +
        'No se guardó ningún dato en PostgreSQL.';

    } catch {

      this.errorArchivo =
        'No fue posible leer el archivo Excel. ' +
        'Comprueba que sea un .xlsx válido, ' +
        'que no esté protegido con contraseña ' +
        'y que no esté dañado.';

    } finally {

      this.procesandoArchivo = false;

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
      'reporte_errores_graduados.csv'
    );

    this.mensajeInformativo =
      'Se generó el reporte de errores en formato CSV, ' +
      'compatible con Excel.';
  }

  // ========================================
  // PROTEGER CELDAS EN EXPORTACIÓN CSV
  // ========================================

  private escaparCSV(valor: string): string {

    // Evitar que valores de texto se interpreten
    // como fórmulas al abrir el CSV con Excel.
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

    // Liberar la URL después de iniciar la descarga.
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
      this.totalValidosActual === 0
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
      this.totalValidosActual === 0
    ) {
      this.cerrarConfirmacion();
      return;
    }

    this.mensajeInformativo =
      'Simulación finalizada: ' +
      this.totalValidosActual +
      ' registros estarían listos para ser enviados ' +
      'a la API. No se importó ningún graduado, ' +
      'no se guardaron datos y no se envió ninguna solicitud.';

    this.cerrarConfirmacion();
  }

  // ========================================
  // LIMPIAR RESULTADOS
  // ========================================

  limpiarAnalisis(): void {

    this.registros = [];

    this.archivoValidado = false;
    this.procesandoArchivo = false;

    this.nombreArchivo = '';
    this.fechaAnalisis = '';

    this.errorArchivo = '';
    this.mensajeInformativo = '';

    this.soloErrores = false;
    this.limiteVistaPrevia = 10;

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
