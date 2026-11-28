
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type AreaProfesional =
  | 'desarrollo'
  | 'datos'
  | 'qa'
  | 'infraestructura';

type EstadoActivacion =
  | 'activo'
  | 'pendiente'
  | 'inactivo';

interface PerfilGraduado {
  id: number;
  nombre: string;
  iniciales: string;
  color: 'pink' | 'blue' | 'purple' | 'green';
  area: AreaProfesional;
  ubicacion: string;
  habilidades: string[];
}

// Modelo utilizado únicamente para simular
// los estados de activación.
interface PerfilDemostracion extends PerfilGraduado {
  estadoActivacion: EstadoActivacion;
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
  // FILTROS PERMITIDOS - RF-10
  // ==========================================

  filtroArea = '';
  filtroUbicacion = '';
  filtroHabilidad = '';

  // ==========================================
  // PERFIL SELECCIONADO
  // ==========================================

  perfilSeleccionado: PerfilGraduado | null = null;

  // ==========================================
  // PERFILES DE DEMOSTRACIÓN
  // No proceden de la API ni de PostgreSQL.
  // ==========================================

  private readonly perfilesDemostracion: PerfilDemostracion[] = [
    {
      id: 1,
      nombre: 'María Fernanda Mejía',
      iniciales: 'MM',
      color: 'pink',
      area: 'desarrollo',
      ubicacion: 'San Salvador',
      habilidades: [
        'React',
        'Node.js',
        'PostgreSQL'
      ],
      estadoActivacion: 'activo'
    },
    {
      id: 2,
      nombre: 'José Luis Ramírez',
      iniciales: 'JL',
      color: 'blue',
      area: 'datos',
      ubicacion: 'San Miguel',
      habilidades: [
        'Python',
        'SQL',
        'Power BI'
      ],
      estadoActivacion: 'activo'
    },
    {
      id: 3,
      nombre: 'Andrea Vásquez',
      iniciales: 'AV',
      color: 'purple',
      area: 'qa',
      ubicacion: 'Santa Ana',
      habilidades: [
        'Testing',
        'Cypress',
        'Jira'
      ],
      estadoActivacion: 'activo'
    },
    {
      id: 4,
      nombre: 'Roberto Castillo',
      iniciales: 'RC',
      color: 'green',
      area: 'infraestructura',
      ubicacion: 'San Miguel',
      habilidades: [
        'Linux',
        'Redes',
        'Azure'
      ],
      estadoActivacion: 'activo'
    },

    // NUEVO: perfil ficticio pendiente de activación.
    // No debe aparecer en las búsquedas de empresas.
    {
      id: 5,
      nombre: 'Graduado de prueba',
      iniciales: 'GP',
      color: 'blue',
      area: 'desarrollo',
      ubicacion: 'La Unión',
      habilidades: [
        'C#',
        'Angular',
        'MySQL'
      ],
      estadoActivacion: 'pendiente'
    }
  ];

  // ==========================================
  // PERFILES VISIBLES PARA LA EMPRESA
  // RF-10, RF-13, RF-16 Y RN-06
  // ==========================================

  readonly perfiles: PerfilGraduado[] =
    this.perfilesDemostracion
      .filter(perfil =>
        perfil.estadoActivacion === 'activo'
      )
      .map(perfil => ({
        id: perfil.id,
        nombre: perfil.nombre,
        iniciales: perfil.iniciales,
        color: perfil.color,
        area: perfil.area,
        ubicacion: perfil.ubicacion,
        habilidades: [...perfil.habilidades]
      }));

  // ==========================================
  // UBICACIONES DISPONIBLES
  // ==========================================

  readonly ubicaciones: string[] = [
    'Ahuachapán',
    'Santa Ana',
    'Sonsonate',
    'Chalatenango',
    'La Libertad',
    'San Salvador',
    'Cuscatlán',
    'La Paz',
    'Cabañas',
    'San Vicente',
    'Usulután',
    'San Miguel',
    'Morazán',
    'La Unión'
  ];

  // ==========================================
  // HABILIDADES DE PERFILES ACTIVOS
  // ==========================================

  get habilidadesDisponibles(): string[] {

    const habilidades = this.perfiles.flatMap(
      perfil => perfil.habilidades
    );

    return [...new Set(habilidades)].sort(
      (a, b) => a.localeCompare(b, 'es')
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

    this.busquedaAplicada =
      this.textoBusqueda.trim();
  }

  // ==========================================
  // LIMPIAR FILTROS
  // ==========================================

  limpiarFiltros(): void {

    this.textoBusqueda = '';
    this.busquedaAplicada = '';

    this.filtroArea = '';
    this.filtroUbicacion = '';
    this.filtroHabilidad = '';
  }

  // ==========================================
  // OBTENER PERFILES FILTRADOS
  // ==========================================

  get perfilesFiltrados(): PerfilGraduado[] {

    const termino = this.normalizarTexto(
      this.busquedaAplicada
    );

    return this.perfiles.filter(perfil => {

      // Buscar únicamente en los datos permitidos.
      const textoPerfil = this.normalizarTexto([
        perfil.nombre,
        this.nombreArea(perfil.area),
        perfil.ubicacion,
        ...perfil.habilidades
      ].join(' '));

      // Búsqueda general.
      const coincideBusqueda =
        !termino || textoPerfil.includes(termino);

      // Filtro por área profesional.
      const coincideArea =
        !this.filtroArea ||
        perfil.area === this.filtroArea;

      // NUEVO: filtro por ubicación.
      const coincideUbicacion =
        !this.filtroUbicacion ||
        this.normalizarTexto(perfil.ubicacion) ===
        this.normalizarTexto(this.filtroUbicacion);

      // NUEVO: filtro por habilidad.
      const coincideHabilidad =
        !this.filtroHabilidad ||
        perfil.habilidades.some(habilidad =>
          this.normalizarTexto(habilidad) ===
          this.normalizarTexto(this.filtroHabilidad)
        );

      // Combinar todos los filtros.
      return (
        coincideBusqueda &&
        coincideArea &&
        coincideUbicacion &&
        coincideHabilidad
      );
    });
  }

  // ==========================================
  // ABRIR DETALLE DE PERFIL
  // ==========================================

  verPerfil(perfil: PerfilGraduado): void {

    // Consultar exclusivamente los perfiles
    // activos disponibles en esta demostración.
    const perfilVisible = this.perfiles.find(
      elemento => elemento.id === perfil.id
    );

    if (!perfilVisible) {
      return;
    }

    this.perfilSeleccionado = perfilVisible;

    this.cerrarNotificaciones();
  }

  cerrarPerfil(): void {

    this.perfilSeleccionado = null;
  }

  // ==========================================
  // NOMBRES DE ÁREAS PROFESIONALES
  // ==========================================

  nombreArea(area: AreaProfesional): string {

    const areas: Record<AreaProfesional, string> = {
      desarrollo: 'Desarrollo de software',
      datos: 'Datos y analítica',
      qa: 'Aseguramiento de calidad (QA)',
      infraestructura: 'Infraestructura y redes'
    };

    return areas[area];
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
