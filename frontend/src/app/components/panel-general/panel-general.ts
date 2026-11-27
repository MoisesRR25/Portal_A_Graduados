
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface IndicadorPanel {
  etiqueta: string;
  valor: string;
  icono: string;
  color: string;
}

interface RegistroAnual {
  anio: number;
  cantidad: number;
}

interface EstadoLaboral {
  nombre: string;
  porcentaje: number;
  color: string;
}

interface AreaEmpleabilidad {
  nombre: string;
  porcentaje: number;
}

interface PendienteAprobacion {
  titulo: string;
  descripcion: string;
  icono: string;
  color: string;
}

@Component({
  selector: 'app-panel-general',
  imports: [RouterLink, FormsModule],
  templateUrl: './panel-general.html',
  styleUrl: './panel-general.css'
})
export class PanelGeneral {

  // ========================================
  // MENÚ Y PANELES
  // ========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ========================================
  // SELECCIÓN DE PERÍODO
  // ========================================

  periodoSeleccionado = '6';

  // ========================================
  // MENSAJES DE EXPORTACIÓN
  // ========================================

  mensajeExportacion = '';

  // ========================================
  // INDICADORES DE DEMOSTRACIÓN
  // ========================================

  readonly indicadores: IndicadorPanel[] = [
    {
      etiqueta: 'Graduados registrados',
      valor: '1,248',
      icono: '👥',
      color: 'icon-pink'
    },
    {
      etiqueta: 'Tasa de empleabilidad',
      valor: '78%',
      icono: '💼',
      color: 'icon-green'
    },
    {
      etiqueta: 'Empresas aliadas',
      valor: '86',
      icono: '🏢',
      color: 'icon-blue'
    },
    {
      etiqueta: 'Ofertas activas',
      valor: '142',
      icono: '📄',
      color: 'icon-amber'
    }
  ];

  // ========================================
  // GRADUADOS POR AÑO
  // ========================================
  // Se conservan los valores originales
  // de 2020 a 2025.
  // 2018 y 2019 son ejemplos adicionales
  // para la opción "Todos los registros".

  readonly graduadosPorAnio: RegistroAnual[] = [
    { anio: 2018, cantidad: 39 },
    { anio: 2019, cantidad: 46 },
    { anio: 2020, cantidad: 52 },
    { anio: 2021, cantidad: 68 },
    { anio: 2022, cantidad: 59 },
    { anio: 2023, cantidad: 82 },
    { anio: 2024, cantidad: 74 },
    { anio: 2025, cantidad: 91 }
  ];

  get graduadosVisibles(): RegistroAnual[] {

    if (this.periodoSeleccionado === '3') {
      return this.graduadosPorAnio.slice(-3);
    }

    if (this.periodoSeleccionado === '6') {
      return this.graduadosPorAnio.slice(-6);
    }

    return this.graduadosPorAnio;
  }

  alturaBarra(cantidad: number): number {
    const altura = (cantidad / 95) * 100;

    return Math.max(
      0,
      Math.min(100, altura)
    );
  }

  // ========================================
  // ESTADO LABORAL
  // ========================================

  readonly totalGraduados = '1,248';

  readonly estadosLaborales: EstadoLaboral[] = [
    {
      nombre: 'Empleado en TI',
      porcentaje: 58,
      color: 'dot-red'
    },
    {
      nombre: 'Buscando empleo',
      porcentaje: 18,
      color: 'dot-blue'
    },
    {
      nombre: 'Fuera de TI',
      porcentaje: 12,
      color: 'dot-amber'
    },
    {
      nombre: 'Emprendedor',
      porcentaje: 7,
      color: 'dot-purple'
    },
    {
      nombre: 'Estudiando',
      porcentaje: 5,
      color: 'dot-green'
    }
  ];

  // ========================================
  // EMPLEABILIDAD POR ÁREA
  // ========================================

  readonly areasEmpleabilidad: AreaEmpleabilidad[] = [
    {
      nombre: 'Desarrollo de software',
      porcentaje: 86
    },
    {
      nombre: 'Datos y analítica',
      porcentaje: 68
    },
    {
      nombre: 'Infraestructura y redes',
      porcentaje: 54
    },
    {
      nombre: 'Soporte técnico',
      porcentaje: 42
    },
    {
      nombre: 'Gestión de proyectos',
      porcentaje: 31
    }
  ];

  // ========================================
  // APROBACIONES PENDIENTES
  // ========================================

  readonly empresasPendientes = 3;
  readonly ofertasPendientes = 2;

  get totalPendientes(): number {
    return (
      this.empresasPendientes +
      this.ofertasPendientes
    );
  }

  readonly pendientes: PendienteAprobacion[] = [
    {
      titulo: '3 empresas',
      descripcion: 'Esperando verificación',
      icono: '🏢',
      color: 'icon-amber'
    },
    {
      titulo: '2 ofertas laborales',
      descripcion: 'Pendientes de revisión',
      icono: '💼',
      color: 'icon-blue'
    }
  ];

  // ========================================
  // MENÚ LATERAL
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
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ========================================
  // EXPORTAR A CSV COMPATIBLE CON EXCEL
  // ========================================

  exportarExcel(): void {

    const filas: string[][] = [
      ['REPORTE DEL PANEL GENERAL - DEMOSTRACIÓN'],
      [],
      ['INDICADORES', 'VALOR'],

      ...this.indicadores.map(indicador => [
        indicador.etiqueta,
        indicador.valor
      ]),

      [],
      ['GRADUADOS POR AÑO', 'CANTIDAD'],

      ...this.graduadosVisibles.map(registro => [
        String(registro.anio),
        String(registro.cantidad)
      ]),

      [],
      ['ESTADO LABORAL', 'PORCENTAJE'],

      ...this.estadosLaborales.map(estado => [
        estado.nombre,
        `${estado.porcentaje}%`
      ]),

      [],
      ['EMPLEABILIDAD POR ÁREA', 'PORCENTAJE'],

      ...this.areasEmpleabilidad.map(area => [
        area.nombre,
        `${area.porcentaje}%`
      ]),

      [],
      ['APROBACIONES PENDIENTES', 'CANTIDAD'],
      ['Empresas', String(this.empresasPendientes)],
      ['Ofertas laborales', String(this.ofertasPendientes)],

      [],
      ['NOTA', 'Datos de demostración. No provienen de PostgreSQL.']
    ];

    const contenidoCSV =
      '\uFEFFsep=;\r\n' +
      filas.map(fila =>
        fila.map(valor =>
          `"${valor.replace(/"/g, '""')}"`
        ).join(';')
      ).join('\r\n');

    try {
      const archivo = new Blob(
        [contenidoCSV],
        {
          type: 'text/csv;charset=utf-8;'
        }
      );

      const url = URL.createObjectURL(archivo);

      const enlace = document.createElement('a');

      enlace.href = url;
      enlace.download = 'panel_general_demostracion.csv';

      document.body.appendChild(enlace);

      enlace.click();
      enlace.remove();

      URL.revokeObjectURL(url);

      this.mensajeExportacion =
        'Se preparó un archivo CSV compatible con Excel. ' +
        'Los valores son de demostración y no provienen de la API.';

    } catch {
      this.mensajeExportacion =
        'No fue posible descargar el reporte. ' +
        'Revisa los permisos de descarga del navegador.';
    }
  }

  // ========================================
  // EXPORTAR MEDIANTE IMPRESIÓN / PDF
  // ========================================

  exportarPDF(): void {

    this.cerrarNotificaciones();
    this.cerrarMenu();

    this.mensajeExportacion =
      'Para obtener el PDF, selecciona ' +
      '"Guardar como PDF" en la ventana de impresión.';

    // Usa la función de impresión del navegador.
    // No necesita instalar librerías.
    window.print();
  }

  cerrarMensajeExportacion(): void {
    this.mensajeExportacion = '';
  }

  // ========================================
  // TECLA ESCAPE
  // ========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
  }
}
