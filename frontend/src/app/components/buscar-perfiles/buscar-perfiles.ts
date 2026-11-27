
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type EstadoAcademico =
  | 'graduado'
  | 'egresado'
  | 'estudiante';

type AreaProfesional =
  | 'desarrollo'
  | 'datos'
  | 'qa'
  | 'infraestructura';

type Experiencia =
  | 'sin-experiencia'
  | '1-2'
  | '3-mas';

type Disponibilidad =
  | 'inmediata'
  | 'tiempo-completo'
  | 'pasantia';

interface PerfilGraduado {
  id: number;
  nombre: string;
  iniciales: string;
  color: 'pink' | 'blue' | 'purple' | 'green';
  profesion: string;
  carrera: string;
  anio: number;
  habilidades: string[];
  compatibilidad: number;
  estadoAcademico: EstadoAcademico;
  area: AreaProfesional;
  experiencia: Experiencia;
  disponibilidad: Disponibilidad;
}

@Component({
  selector: 'app-buscar-perfiles',
  imports: [RouterLink, FormsModule],
  templateUrl: './buscar-perfiles.html',
  styleUrl: './buscar-perfiles.css'
})
export class BuscarPerfiles {

  // ==========================================
  // MENÚ Y PANELES
  // ==========================================

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // ==========================================
  // BÚSQUEDA
  // ==========================================

  textoBusqueda = '';
  busquedaAplicada = '';

  // ==========================================
  // FILTROS
  // ==========================================

  filtroAcademico = '';
  filtroArea = '';
  filtroExperiencia = '';
  filtroDisponibilidad = '';

  // ==========================================
  // PERFIL SELECCIONADO
  // ==========================================

  perfilSeleccionado: PerfilGraduado | null = null;

  // ==========================================
  // PERFILES DE DEMOSTRACIÓN
  // ==========================================

  perfiles: PerfilGraduado[] = [
    {
      id: 1,
      nombre: 'María Fernanda Mejía',
      iniciales: 'MM',
      color: 'pink',
      profesion: 'Desarrolladora Full Stack',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2022,
      habilidades: [
        'React',
        'Node.js',
        'PostgreSQL'
      ],
      compatibilidad: 92,
      estadoAcademico: 'graduado',
      area: 'desarrollo',
      experiencia: '3-mas',
      disponibilidad: 'tiempo-completo'
    },
    {
      id: 2,
      nombre: 'José Luis Ramírez',
      iniciales: 'JL',
      color: 'blue',
      profesion: 'Analista de Datos',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2023,
      habilidades: [
        'Python',
        'SQL',
        'Power BI'
      ],
      compatibilidad: 88,
      estadoAcademico: 'graduado',
      area: 'datos',
      experiencia: '1-2',
      disponibilidad: 'inmediata'
    },
    {
      id: 3,
      nombre: 'Andrea Vásquez',
      iniciales: 'AV',
      color: 'purple',
      profesion: 'QA Engineer Jr.',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2024,
      habilidades: [
        'Testing',
        'Cypress',
        'Jira'
      ],
      compatibilidad: 84,
      estadoAcademico: 'graduado',
      area: 'qa',
      experiencia: 'sin-experiencia',
      disponibilidad: 'pasantia'
    },
    {
      id: 4,
      nombre: 'Roberto Castillo',
      iniciales: 'RC',
      color: 'green',
      profesion: 'Soporte e Infraestructura',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2025,
      habilidades: [
        'Linux',
        'Redes',
        'Azure'
      ],
      compatibilidad: 79,
      estadoAcademico: 'graduado',
      area: 'infraestructura',
      experiencia: '1-2',
      disponibilidad: 'tiempo-completo'
    }
  ];

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
    this.cerrarPerfil();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // ==========================================
  // NORMALIZAR TEXTO
  // ==========================================

  private normalizarTexto(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  // ==========================================
  // BUSCAR PERFILES
  // ==========================================

  buscarPerfiles(): void {
    this.busquedaAplicada = this.textoBusqueda.trim();
  }

  // ==========================================
  // LIMPIAR FILTROS
  // ==========================================

  limpiarFiltros(): void {
    this.textoBusqueda = '';
    this.busquedaAplicada = '';

    this.filtroAcademico = '';
    this.filtroArea = '';
    this.filtroExperiencia = '';
    this.filtroDisponibilidad = '';
  }

  // ==========================================
  // OBTENER PERFILES FILTRADOS
  // ==========================================

  get perfilesFiltrados(): PerfilGraduado[] {

    const termino = this.normalizarTexto(
      this.busquedaAplicada
    );

    return this.perfiles.filter(perfil => {

      const textoPerfil = this.normalizarTexto([
        perfil.nombre,
        perfil.profesion,
        perfil.carrera,
        String(perfil.anio),
        ...perfil.habilidades
      ].join(' '));

      const coincideBusqueda =
        !termino || textoPerfil.includes(termino);

      const coincideAcademico =
        !this.filtroAcademico ||
        perfil.estadoAcademico === this.filtroAcademico;

      const coincideArea =
        !this.filtroArea ||
        perfil.area === this.filtroArea;

      const coincideExperiencia =
        !this.filtroExperiencia ||
        perfil.experiencia === this.filtroExperiencia;

      const coincideDisponibilidad =
        !this.filtroDisponibilidad ||
        perfil.disponibilidad ===
          this.filtroDisponibilidad;

      return (
        coincideBusqueda &&
        coincideAcademico &&
        coincideArea &&
        coincideExperiencia &&
        coincideDisponibilidad
      );
    });
  }

  // ==========================================
  // ABRIR DETALLE DE PERFIL
  // ==========================================

  verPerfil(perfil: PerfilGraduado): void {
    this.perfilSeleccionado = perfil;
    this.cerrarNotificaciones();
  }

  cerrarPerfil(): void {
    this.perfilSeleccionado = null;
  }

  // ==========================================
  // NOMBRES PARA LA VISTA DE PERFIL
  // ==========================================

  nombreEstado(
    estado: EstadoAcademico
  ): string {

    const estados: Record<EstadoAcademico, string> = {
      graduado: 'Graduado',
      egresado: 'Egresado',
      estudiante: 'Estudiante activo'
    };

    return estados[estado];
  }

  nombreArea(
    area: AreaProfesional
  ): string {

    const areas: Record<AreaProfesional, string> = {
      desarrollo: 'Desarrollo de software',
      datos: 'Datos y analítica',
      qa: 'Aseguramiento de calidad (QA)',
      infraestructura: 'Infraestructura y redes'
    };

    return areas[area];
  }

  nombreExperiencia(
    experiencia: Experiencia
  ): string {

    const experiencias: Record<Experiencia, string> = {
      'sin-experiencia': 'Sin experiencia',
      '1-2': '1 a 2 años',
      '3-mas': '3 años o más'
    };

    return experiencias[experiencia];
  }

  nombreDisponibilidad(
    disponibilidad: Disponibilidad
  ): string {

    const disponibilidades: Record<
      Disponibilidad,
      string
    > = {
      inmediata: 'Inmediata',
      'tiempo-completo': 'Tiempo completo',
      pasantia: 'Pasantía / Medio tiempo'
    };

    return disponibilidades[disponibilidad];
  }

  // ==========================================
  // CERRAR CON TECLA ESCAPE
  // ==========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarPerfil();
  }
}
