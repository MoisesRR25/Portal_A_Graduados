
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type EstadoPostulacion =
  | 'En revisión'
  | 'Preseleccionado'
  | 'Descartado';

type FiltroEstado = 'Todos' | EstadoPostulacion;

interface OfertaDemo {
  id: number;
  empresaId: number;
  titulo: string;
}

interface CandidatoDemo {
  id: number;
  ofertaId: number;
  nombre: string;
  iniciales: string;
  color: 'pink' | 'blue' | 'purple' | 'green';
  rol: string;
  carrera: string;
  anio: number;
  habilidades: string[];
  compatibilidad: number;
  estado: EstadoPostulacion;

  // Datos de la postulación.
  fechaPostulacion: string;
  correo: string;
  nombreCV: string;
}

interface NotificacionDemo {
  id: number;
  candidatoId: number;
  mensaje: string;
  fecha: string;
  leida: boolean;
}

@Component({
  selector: 'app-candidatos',
  imports: [RouterLink, FormsModule],
  templateUrl: './candidatos.html',
  styleUrl: './candidatos.css'
})
export class Candidatos {

  // ==========================================
  // NAVEGACIÓN
  // ==========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ==========================================
  // EMPRESA DE DEMOSTRACIÓN
  // ==========================================

  // Este identificador es ficticio.
  // La API deberá obtener la empresa autenticada.
  private readonly empresaIdActual = 1;

  // ==========================================
  // BÚSQUEDA Y FILTROS
  // ==========================================

  textoBusqueda = '';
  busquedaAplicada = '';

  filtroEstado: FiltroEstado = 'Todos';
  filtroOferta: number | null = null;

  readonly estadosPostulacion: EstadoPostulacion[] = [
    'En revisión',
    'Preseleccionado',
    'Descartado'
  ];

  // ==========================================
  // VENTANAS DE DETALLE
  // ==========================================

  candidatoSeleccionado: CandidatoDemo | null = null;

  tipoDetalle: 'perfil' | 'contacto' | null = null;

  mensajeEstado = '';

  // ==========================================
  // OFERTAS DE DEMOSTRACIÓN
  // ==========================================

  private readonly ofertasDemostracion: OfertaDemo[] = [
    {
      id: 1,
      empresaId: 1,
      titulo: 'Desarrollador Frontend Jr.'
    },
    {
      id: 2,
      empresaId: 1,
      titulo: 'Analista de soporte técnico'
    },
    {
      id: 3,
      empresaId: 1,
      titulo: 'Pasantía de desarrollo web'
    },
    {
      id: 4,
      empresaId: 1,
      titulo: 'Diseñador UX/UI'
    },
    {
      id: 5,
      empresaId: 2,
      titulo: 'Oferta de otra empresa'
    }
  ];

  // Mostrar exclusivamente las ofertas propias.
  readonly ofertasEmpresa = this.ofertasDemostracion.filter(
    oferta => oferta.empresaId === this.empresaIdActual
  );

  // ==========================================
  // CANDIDATURAS DE DEMOSTRACIÓN
  // ==========================================

  private readonly candidaturasDemo: CandidatoDemo[] = [
    {
      id: 1,
      ofertaId: 1,
      nombre: 'María Fernanda Mejía',
      iniciales: 'MM',
      color: 'pink',
      rol: 'Desarrolladora Full Stack',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2022,
      habilidades: ['React', 'Node.js', 'PostgreSQL'],
      compatibilidad: 92,
      estado: 'En revisión',
      fechaPostulacion: '17 jun. 2025',
      correo: 'maria.mejia@example.com',
      nombreCV: 'CV_Maria_Mejia.pdf'
    },
    {
      id: 2,
      ofertaId: 1,
      nombre: 'José Luis Ramírez',
      iniciales: 'JL',
      color: 'blue',
      rol: 'Analista de Datos',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2023,
      habilidades: ['Python', 'SQL', 'Power BI'],
      compatibilidad: 88,
      estado: 'En revisión',
      fechaPostulacion: '18 jun. 2025',
      correo: 'jose.ramirez@example.com',
      nombreCV: 'CV_Jose_Ramirez.pdf'
    },
    {
      id: 3,
      ofertaId: 3,
      nombre: 'Andrea Vásquez',
      iniciales: 'AV',
      color: 'purple',
      rol: 'QA Engineer Jr.',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2024,
      habilidades: ['Testing', 'Cypress', 'Jira'],
      compatibilidad: 84,
      estado: 'En revisión',
      fechaPostulacion: '4 jun. 2025',
      correo: 'andrea.vasquez@example.com',
      nombreCV: 'CV_Andrea_Vasquez.pdf'
    },
    {
      id: 4,
      ofertaId: 4,
      nombre: 'Roberto Castillo',
      iniciales: 'RC',
      color: 'green',
      rol: 'Soporte e Infraestructura',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2025,
      habilidades: ['Linux', 'Redes', 'Azure'],
      compatibilidad: 79,
      estado: 'En revisión',
      fechaPostulacion: '22 may. 2025',
      correo: 'roberto.castillo@example.com',
      nombreCV: 'CV_Roberto_Castillo.pdf'
    },

    // Candidatura ficticia de otra empresa.
    // No debe aparecer en la interfaz.
    {
      id: 5,
      ofertaId: 5,
      nombre: 'Candidato de otra empresa',
      iniciales: 'CE',
      color: 'blue',
      rol: 'Desarrollador de prueba',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2025,
      habilidades: ['C#', 'Angular'],
      compatibilidad: 80,
      estado: 'En revisión',
      fechaPostulacion: '20 jun. 2025',
      correo: 'prueba@example.com',
      nombreCV: 'CV_Prueba.pdf'
    }
  ];

  // ==========================================
  // CANDIDATOS AUTORIZADOS LOCALMENTE
  // ==========================================
  // Esto solamente simula el filtrado por empresa.
  // La autorización real corresponde a la API.

  readonly candidatos: CandidatoDemo[] =
    this.candidaturasDemo.filter(candidato =>
      this.ofertasEmpresa.some(
        oferta => oferta.id === candidato.ofertaId
      )
    );

  // ==========================================
  // NOTIFICACIONES SIMULADAS
  // ==========================================

  notificaciones: NotificacionDemo[] =
    this.candidatos.map(candidato => ({
      id: candidato.id,
      candidatoId: candidato.id,
      mensaje:
        candidato.nombre +
        ' se postuló a «' +
        this.obtenerTituloOferta(candidato.ofertaId) +
        '».',
      fecha: candidato.fechaPostulacion,
      leida: false
    }));

  get totalNotificacionesPendientes(): number {

    return this.notificaciones.filter(
      notificacion => !notificacion.leida
    ).length;
  }

  // ==========================================
  // OBTENER TÍTULO DE LA OFERTA
  // ==========================================

  obtenerTituloOferta(ofertaId: number): string {

    return this.ofertasEmpresa.find(
      oferta => oferta.id === ofertaId
    )?.titulo ?? 'Oferta no disponible';
  }

  // ==========================================
  // CONTADORES
  // ==========================================

  get totalEnRevision(): number {

    return this.candidatos.filter(
      candidato => candidato.estado === 'En revisión'
    ).length;
  }

  get totalPreseleccionados(): number {

    return this.candidatos.filter(
      candidato => candidato.estado === 'Preseleccionado'
    ).length;
  }

  get totalDescartados(): number {

    return this.candidatos.filter(
      candidato => candidato.estado === 'Descartado'
    ).length;
  }

  contarCandidatosOferta(ofertaId: number): number {

    return this.candidatos.filter(
      candidato => candidato.ofertaId === ofertaId
    ).length;
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

  abrirNotificacion(
    notificacion: NotificacionDemo
  ): void {

    const candidato = this.candidatos.find(
      elemento => elemento.id === notificacion.candidatoId
    );

    if (!candidato) {
      return;
    }

    notificacion.leida = true;

    this.abrirPerfil(candidato);
  }

  marcarTodasComoLeidas(): void {

    this.notificaciones = this.notificaciones.map(
      notificacion => ({
        ...notificacion,
        leida: true
      })
    );
  }

  // ==========================================
  // ACCESO Y SEGURIDAD
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
  // NORMALIZACIÓN PARA BÚSQUEDA
  // ==========================================

  private normalizarTexto(texto: string): string {

    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  // ==========================================
  // BUSCAR CANDIDATOS
  // ==========================================

  buscarCandidatos(): void {

    this.busquedaAplicada =
      this.textoBusqueda.trim();
  }

  // ==========================================
  // LIMPIAR BÚSQUEDA Y FILTROS
  // ==========================================

  limpiarFiltros(): void {

    this.textoBusqueda = '';
    this.busquedaAplicada = '';

    this.filtroEstado = 'Todos';
    this.filtroOferta = null;
  }

  // ==========================================
  // RESULTADOS DE BÚSQUEDA
  // ==========================================

  get candidatosFiltrados(): CandidatoDemo[] {

    const termino = this.normalizarTexto(
      this.busquedaAplicada
    );

    return this.candidatos.filter(candidato => {

      // Filtrar según estado de postulación.
      const coincideEstado =
        this.filtroEstado === 'Todos' ||
        candidato.estado === this.filtroEstado;

      // NUEVO: filtrar por oferta laboral.
      const coincideOferta =
        this.filtroOferta === null ||
        candidato.ofertaId === this.filtroOferta;

      const contenidoBuscable = this.normalizarTexto([
        candidato.nombre,
        candidato.rol,
        candidato.carrera,
        String(candidato.anio),
        this.obtenerTituloOferta(candidato.ofertaId),
        ...candidato.habilidades
      ].join(' '));

      const coincideBusqueda =
        !termino ||
        contenidoBuscable.includes(termino);

      return (
        coincideEstado &&
        coincideOferta &&
        coincideBusqueda
      );
    });
  }

  // ==========================================
  // COMPROBAR CANDIDATURA DE LA EMPRESA
  // ==========================================

  private obtenerCandidatoAutorizado(
    candidatoId: number
  ): CandidatoDemo | undefined {

    return this.candidatos.find(
      candidato => candidato.id === candidatoId
    );
  }

  // ==========================================
  // VER PERFIL
  // ==========================================

  abrirPerfil(candidato: CandidatoDemo): void {

    const autorizado =
      this.obtenerCandidatoAutorizado(candidato.id);

    if (!autorizado) {
      return;
    }

    this.candidatoSeleccionado = autorizado;
    this.tipoDetalle = 'perfil';
    this.mensajeEstado = '';

    this.cerrarNotificaciones();
  }

  // ==========================================
  // CONSULTAR CONTACTO
  // No se implementa chat interno.
  // ==========================================

  abrirContacto(candidato: CandidatoDemo): void {

    const autorizado =
      this.obtenerCandidatoAutorizado(candidato.id);

    if (!autorizado) {
      return;
    }

    this.candidatoSeleccionado = autorizado;
    this.tipoDetalle = 'contacto';
    this.mensajeEstado = '';

    this.cerrarNotificaciones();
  }

  // ==========================================
  // CAMBIAR ESTADO LOCALMENTE
  // ==========================================

  actualizarEstado(
    candidato: CandidatoDemo,
    nuevoEstado: EstadoPostulacion
  ): void {

    const autorizado =
      this.obtenerCandidatoAutorizado(candidato.id);

    if (!autorizado) {
      return;
    }

    if (!this.estadosPostulacion.includes(nuevoEstado)) {
      return;
    }

    if (autorizado.estado === nuevoEstado) {
      return;
    }

    autorizado.estado = nuevoEstado;

    this.mensajeEstado =
      `La postulación de ${autorizado.nombre} ` +
      `cambió a «${nuevoEstado}». ` +
      'El cambio solo se aplicó en esta demostración. ' +
      'No se guardó en PostgreSQL ni se notificó al candidato.';
  }

  // ==========================================
  // NUEVO: DESCARGAR CV DE DEMOSTRACIÓN
  // ==========================================
  // Se genera un PDF ficticio.
  // No se descarga ningún CV real de un graduado.

  descargarCV(candidato: CandidatoDemo): void {

    const autorizado =
      this.obtenerCandidatoAutorizado(candidato.id);

    if (!autorizado) {
      return;
    }

    try {

      const lineas = [
        'CURRICULUM VITAE - DEMOSTRACION',
        'Portal de Seguimiento a Graduados - UES',
        '',
        'Documento ficticio para pruebas del sistema.',
        '',
        'Nombre: ' + autorizado.nombre,
        'Perfil: ' + autorizado.rol,
        'Carrera: ' + autorizado.carrera,
        'Anio de graduacion: ' + autorizado.anio,
        'Correo ficticio: ' + autorizado.correo,
        '',
        'Habilidades:',
        ...autorizado.habilidades.map(
          habilidad => '- ' + habilidad
        ),
        '',
        'Oferta: ' +
          this.obtenerTituloOferta(autorizado.ofertaId),
        'Fecha de postulacion: ' +
          autorizado.fechaPostulacion,
        '',
        'Este documento no es un curriculum real.'
      ];

      const archivo =
        this.generarPDFDemostracion(lineas);

      const url = URL.createObjectURL(archivo);

      const enlace = document.createElement('a');

      enlace.href = url;
      enlace.download = autorizado.nombreCV;

      document.body.appendChild(enlace);

      enlace.click();
      enlace.remove();

      setTimeout(() => {
        URL.revokeObjectURL(url);
      }, 1000);

      this.mensajeEstado =
        'Se generó el CV PDF de demostración de ' +
        autorizado.nombre +
        '. El documento contiene información ficticia.';

    } catch {

      this.mensajeEstado =
        'No se pudo generar el CV de demostración.';
    }
  }

  // ==========================================
  // GENERAR PDF DE DEMOSTRACIÓN
  // Sin instalar bibliotecas adicionales.
  // ==========================================

  private generarPDFDemostracion(
    lineas: string[]
  ): Blob {

    // Convertir a caracteres ASCII y proteger
    // caracteres especiales del formato PDF.

    const escaparTexto = (texto: string): string => {

      return texto
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[^\x20-\x7E]/g, '?')
        .replace(/\\/g, '\\\\')
        .replace(/\(/g, '\\(')
        .replace(/\)/g, '\\)');
    };

    const contenido = [

      'BT',
      '/F1 12 Tf',
      '54 740 Td',
      '18 TL',

      ...lineas.map(
        (linea, indice) =>
          (indice === 0 ? '' : 'T* ') +
          '(' + escaparTexto(linea) + ') Tj'
      ),

      'ET'

    ].join('\n') + '\n';

    const objetos = [

      '<< /Type /Catalog /Pages 2 0 R >>',

      '<< /Type /Pages /Kids [3 0 R] /Count 1 >>',

      '<< /Type /Page /Parent 2 0 R ' +
      '/MediaBox [0 0 612 792] ' +
      '/Resources << /Font << /F1 4 0 R >> >> ' +
      '/Contents 5 0 R >>',

      '<< /Type /Font /Subtype /Type1 ' +
      '/BaseFont /Helvetica >>',

      '<< /Length ' + contenido.length + ' >>\n' +
      'stream\n' +
      contenido +
      'endstream'
    ];

    let pdf = '%PDF-1.4\n';

    const posiciones: number[] = [0];

    objetos.forEach((objeto, indice) => {

      posiciones.push(pdf.length);

      pdf +=
        (indice + 1) + ' 0 obj\n' +
        objeto +
        '\nendobj\n';
    });

    const inicioXref = pdf.length;

    pdf += 'xref\n';
    pdf += '0 ' + posiciones.length + '\n';
    pdf += '0000000000 65535 f \n';

    posiciones.slice(1).forEach(posicion => {

      pdf +=
        String(posicion).padStart(10, '0') +
        ' 00000 n \n';
    });

    pdf +=
      'trailer\n' +
      '<< /Size ' + posiciones.length +
      ' /Root 1 0 R >>\n' +
      'startxref\n' +
      inicioXref +
      '\n%%EOF\n';

    return new Blob(
      [pdf],
      { type: 'application/pdf' }
    );
  }

  // ==========================================
  // CERRAR PERFIL O CONTACTO
  // ==========================================

  cerrarDetalle(): void {

    this.candidatoSeleccionado = null;
    this.tipoDetalle = null;
    this.mensajeEstado = '';
  }

  // ==========================================
  // CERRAR CON ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarDetalle();
  }
}
