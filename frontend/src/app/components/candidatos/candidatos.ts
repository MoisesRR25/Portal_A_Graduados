
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type EstadoPostulacion =
  | 'En revisión'
  | 'Preseleccionado'
  | 'Descartado';

type FiltroEstado = 'Todos' | EstadoPostulacion;

interface CandidatoDemo {
  id: number;
  nombre: string;
  iniciales: string;
  color: 'pink' | 'blue' | 'purple' | 'green';
  rol: string;
  carrera: string;
  anio: number;
  habilidades: string[];
  compatibilidad: number;
  estado: EstadoPostulacion;
}

@Component({
  selector: 'app-candidatos',
  imports: [RouterLink, FormsModule],
  templateUrl: './candidatos.html',
  styleUrl: './candidatos.css'
})
export class Candidatos {

  // NAVEGACIÓN
  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;

  // BÚSQUEDA Y FILTROS
  textoBusqueda = '';
  busquedaAplicada = '';
  filtroEstado: FiltroEstado = 'Todos';

  // VENTANAS DE DETALLE
  candidatoSeleccionado: CandidatoDemo | null = null;
  tipoDetalle: 'perfil' | 'contacto' | null = null;

  // MENSAJE DE CAMBIO DE ESTADO
  mensajeEstado = '';

  // DATOS DE DEMOSTRACIÓN
  candidatos: CandidatoDemo[] = [
    {
      id: 1,
      nombre: 'María Fernanda Mejía',
      iniciales: 'MM',
      color: 'pink',
      rol: 'Desarrolladora Full Stack',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2022,
      habilidades: ['React', 'Node.js', 'PostgreSQL'],
      compatibilidad: 92,
      estado: 'En revisión'
    },
    {
      id: 2,
      nombre: 'José Luis Ramírez',
      iniciales: 'JL',
      color: 'blue',
      rol: 'Analista de Datos',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2023,
      habilidades: ['Python', 'SQL', 'Power BI'],
      compatibilidad: 88,
      estado: 'En revisión'
    },
    {
      id: 3,
      nombre: 'Andrea Vásquez',
      iniciales: 'AV',
      color: 'purple',
      rol: 'QA Engineer Jr.',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2024,
      habilidades: ['Testing', 'Cypress', 'Jira'],
      compatibilidad: 84,
      estado: 'En revisión'
    },
    {
      id: 4,
      nombre: 'Roberto Castillo',
      iniciales: 'RC',
      color: 'green',
      rol: 'Soporte e Infraestructura',
      carrera: 'Ingeniería de Sistemas Informáticos',
      anio: 2025,
      habilidades: ['Linux', 'Redes', 'Azure'],
      compatibilidad: 79,
      estado: 'En revisión'
    }
  ];

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
  abrirSeguridad(): void {
    this.seguridadAbierta = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarDetalle();
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  // NORMALIZACIÓN PARA BÚSQUEDA
  // Permite encontrar José escribiendo Jose.
  private normalizarTexto(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  // BUSCAR CANDIDATOS
  buscarCandidatos(): void {
    this.busquedaAplicada = this.textoBusqueda.trim();
  }

  // LIMPIAR BÚSQUEDA Y FILTROS
  limpiarFiltros(): void {
    this.textoBusqueda = '';
    this.busquedaAplicada = '';
    this.filtroEstado = 'Todos';
  }

  // RESULTADOS DE BÚSQUEDA
  get candidatosFiltrados(): CandidatoDemo[] {
    const termino = this.normalizarTexto(
      this.busquedaAplicada
    );

    return this.candidatos.filter(candidato => {

      const coincideEstado =
        this.filtroEstado === 'Todos' ||
        candidato.estado === this.filtroEstado;

      const contenidoBuscable = this.normalizarTexto([
        candidato.nombre,
        candidato.rol,
        candidato.carrera,
        String(candidato.anio),
        ...candidato.habilidades
      ].join(' '));

      const coincideBusqueda =
        !termino || contenidoBuscable.includes(termino);

      return coincideEstado && coincideBusqueda;
    });
  }

  // VER PERFIL
  abrirPerfil(candidato: CandidatoDemo): void {
    this.candidatoSeleccionado = candidato;
    this.tipoDetalle = 'perfil';
    this.mensajeEstado = '';
    this.cerrarNotificaciones();
  }

  // CONSULTAR CONTACTO
  // No se implementa chat interno.
  abrirContacto(candidato: CandidatoDemo): void {
    this.candidatoSeleccionado = candidato;
    this.tipoDetalle = 'contacto';
    this.mensajeEstado = '';
    this.cerrarNotificaciones();
  }

  // CAMBIAR ESTADO LOCALMENTE
  actualizarEstado(
    candidato: CandidatoDemo,
    nuevoEstado: EstadoPostulacion
  ): void {
    candidato.estado = nuevoEstado;

    this.mensajeEstado =
      'El estado se actualizó únicamente en esta ' +
      'demostración. No se guardó en PostgreSQL ' +
      'ni se notificó al candidato.';
  }

  // CERRAR PERFIL O CONTACTO
  cerrarDetalle(): void {
    this.candidatoSeleccionado = null;
    this.tipoDetalle = null;
    this.mensajeEstado = '';
  }

  // CERRAR CON ESCAPE
  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarSeguridad();
    this.cerrarDetalle();
  }
}
