
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
}

@Component({
  selector: 'app-bolsa-interna',
  imports: [RouterLink, FormsModule],
  templateUrl: './bolsa-interna.html',
  styleUrl: './bolsa-interna.css'
})
export class BolsaInterna {
  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;
  ofertaSeleccionada: OfertaTrabajo | null = null;

  textoBusqueda = '';
  busquedaAplicada = '';
  areaSeleccionada = '';
  ubicacionSeleccionada = '';
  modalidadesSeleccionadas: Modalidad[] = [];
  clasificacionesSeleccionadas: Clasificacion[] = [];
  habilidadesSeleccionadas: string[] = [];
  ordenSeleccionado: OrdenOfertas = 'recientes';

  favoritos: number[] = [];
  postulacionesSimuladas: number[] = [];
  mensajeAccion = '';

  readonly areas = [
    'Desarrollo de Software', 'Bases de Datos',
    'QA y Testing', 'Soporte Técnico'
  ];

  readonly ubicaciones = [
    'San Salvador', 'Antiguo Cuscatlán', 'La Libertad',
    'Soyapango', 'Santa Ana', 'San Miguel'
  ];

  readonly modalidades: Modalidad[] = ['Presencial', 'Híbrido', 'Remoto'];
  readonly clasificaciones: Clasificacion[] = [
    'Recién graduados', 'Sin experiencia', 'Pasantía'
  ];
  readonly habilidades = ['React', 'Java', 'SQL', 'Python', 'Soporte'];

  // Son las seis vacantes del diseño original; datos de ejemplo, no API.
  readonly ofertas: OfertaTrabajo[] = [
    {
      id: 1, empresa: 'Applaudo Studios', iniciales: 'AS', color: 'as',
      puesto: 'Desarrollador Frontend Jr.', area: 'Desarrollo de Software',
      ubicacion: 'San Salvador', modalidad: 'Híbrido',
      clasificaciones: ['Recién graduados', 'Sin experiencia'],
      etiquetas: ['React', 'TypeScript', 'Sin experiencia'],
      antiguedadDias: 2, publicada: 'Hace 2 días', compatibilidad: 87
    },
    {
      id: 2, empresa: 'Banco Agrícola', iniciales: 'BA', color: 'ba',
      puesto: 'Analista de Datos', area: 'Bases de Datos',
      ubicacion: 'Antiguo Cuscatlán', modalidad: 'Presencial',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['SQL', 'Power BI', 'Python'],
      antiguedadDias: 4, publicada: 'Hace 4 días', compatibilidad: 87
    },
    {
      id: 3, empresa: 'Elaniin', iniciales: 'EL', color: 'el',
      puesto: 'QA Tester Trainee', area: 'QA y Testing',
      ubicacion: 'San Salvador', modalidad: 'Remoto',
      clasificaciones: ['Pasantía'],
      etiquetas: ['Testing', 'Pasantía'],
      antiguedadDias: 7, publicada: 'Hace 1 semana', compatibilidad: 87
    },
    {
      id: 4, empresa: 'Telefónica Tech', iniciales: 'TT', color: 'tt',
      puesto: 'Soporte Técnico N1', area: 'Soporte Técnico',
      ubicacion: 'San Salvador', modalidad: 'Presencial',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['Redes', 'Servicio al cliente'],
      antiguedadDias: 3, publicada: 'Hace 3 días'
    },
    {
      id: 5, empresa: 'Hugo App', iniciales: 'HA', color: 'ha',
      puesto: 'Backend Developer', area: 'Desarrollo de Software',
      ubicacion: 'La Libertad', modalidad: 'Híbrido',
      clasificaciones: ['Recién graduados'],
      etiquetas: ['Node.js', 'PostgreSQL'],
      antiguedadDias: 5, publicada: 'Hace 5 días'
    },
    {
      id: 6, empresa: 'Sertracen', iniciales: 'SE', color: 'se',
      puesto: 'Pasantía de Desarrollo', area: 'Desarrollo de Software',
      ubicacion: 'Soyapango', modalidad: 'Presencial',
      clasificaciones: ['Pasantía', 'Sin experiencia'],
      etiquetas: ['Pasantía', 'Sin experiencia'],
      antiguedadDias: 7, publicada: 'Hace 1 semana'
    }
  ];

  private normalizar(texto: string): string {
    return texto.normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .toLocaleLowerCase('es').trim();
  }

  get ofertasFiltradas(): OfertaTrabajo[] {
    const busqueda = this.normalizar(this.busquedaAplicada);
    return this.ofertas.filter(oferta => {
      const contenido = this.normalizar([
        oferta.puesto, oferta.empresa, oferta.area, oferta.ubicacion,
        oferta.modalidad, ...oferta.etiquetas
      ].join(' '));
      const coincideTexto = !busqueda || contenido.includes(busqueda);
      const coincideArea = !this.areaSeleccionada || oferta.area === this.areaSeleccionada;
      const coincideUbicacion = !this.ubicacionSeleccionada || oferta.ubicacion === this.ubicacionSeleccionada;
      const coincideModalidad = this.modalidadesSeleccionadas.length === 0 ||
        this.modalidadesSeleccionadas.includes(oferta.modalidad);
      const coincideClasificacion = this.clasificacionesSeleccionadas.length === 0 ||
        this.clasificacionesSeleccionadas.some(tipo => oferta.clasificaciones.includes(tipo));
      const coincideHabilidad = this.habilidadesSeleccionadas.length === 0 ||
        this.habilidadesSeleccionadas.some(habilidad => contenido.includes(this.normalizar(habilidad)));
      return coincideTexto && coincideArea && coincideUbicacion &&
        coincideModalidad && coincideClasificacion && coincideHabilidad;
    }).sort((a, b) => this.ordenSeleccionado === 'compatibilidad'
      ? (b.compatibilidad ?? -1) - (a.compatibilidad ?? -1) || a.antiguedadDias - b.antiguedadDias
      : a.antiguedadDias - b.antiguedadDias);
  }

  get hayFiltrosActivos(): boolean {
    return Boolean(this.busquedaAplicada || this.areaSeleccionada || this.ubicacionSeleccionada ||
      this.modalidadesSeleccionadas.length || this.clasificacionesSeleccionadas.length ||
      this.habilidadesSeleccionadas.length);
  }

  buscar(): void {
    this.busquedaAplicada = this.textoBusqueda.trim();
  }

  limpiarFiltros(): void {
    this.textoBusqueda = '';
    this.busquedaAplicada = '';
    this.areaSeleccionada = '';
    this.ubicacionSeleccionada = '';
    this.modalidadesSeleccionadas = [];
    this.clasificacionesSeleccionadas = [];
    this.habilidadesSeleccionadas = [];
    this.ordenSeleccionado = 'recientes';
  }

  alternarModalidad(modalidad: Modalidad): void {
    this.modalidadesSeleccionadas = this.modalidadesSeleccionadas.includes(modalidad)
      ? this.modalidadesSeleccionadas.filter(valor => valor !== modalidad)
      : [...this.modalidadesSeleccionadas, modalidad];
  }

  alternarClasificacion(clasificacion: Clasificacion): void {
    this.clasificacionesSeleccionadas = this.clasificacionesSeleccionadas.includes(clasificacion)
      ? this.clasificacionesSeleccionadas.filter(valor => valor !== clasificacion)
      : [...this.clasificacionesSeleccionadas, clasificacion];
  }

  alternarHabilidad(habilidad: string): void {
    this.habilidadesSeleccionadas = this.habilidadesSeleccionadas.includes(habilidad)
      ? this.habilidadesSeleccionadas.filter(valor => valor !== habilidad)
      : [...this.habilidadesSeleccionadas, habilidad];
  }

  contarModalidad(modalidad: Modalidad): number {
    return this.ofertas.filter(oferta => oferta.modalidad === modalidad).length;
  }

  contarClasificacion(clasificacion: Clasificacion): number {
    return this.ofertas.filter(oferta => oferta.clasificaciones.includes(clasificacion)).length;
  }

  esFavorita(id: number): boolean {
    return this.favoritos.includes(id);
  }

  alternarFavorita(oferta: OfertaTrabajo): void {
    if (this.esFavorita(oferta.id)) {
      this.favoritos = this.favoritos.filter(id => id !== oferta.id);
      this.mensajeAccion = `Quitaste «${oferta.puesto}» de favoritos de esta demostración.`;
    } else {
      this.favoritos = [...this.favoritos, oferta.id];
      this.mensajeAccion = `Guardaste «${oferta.puesto}» como favorito en esta pantalla.`;
    }
  }

  abrirOferta(oferta: OfertaTrabajo): void {
    this.ofertaSeleccionada = oferta;
    this.notificacionesAbiertas = false;
  }

  cerrarOferta(): void {
    this.ofertaSeleccionada = null;
  }

  estaPostulada(id: number): boolean {
    return this.postulacionesSimuladas.includes(id);
  }

  simularPostulacion(): void {
    if (!this.ofertaSeleccionada || this.estaPostulada(this.ofertaSeleccionada.id)) return;
    const oferta = this.ofertaSeleccionada;
    this.postulacionesSimuladas = [...this.postulacionesSimuladas, oferta.id];
    this.mensajeAccion = `Postulación simulada a «${oferta.puesto}». No se envió ni aparecerá en Mis postulaciones.`;
  }

  limpiarMensaje(): void { this.mensajeAccion = ''; }

  alternarMenu(): void {
    this.menuAbierto = !this.menuAbierto;
    this.notificacionesAbiertas = false;
  }

  cerrarMenu(): void { this.menuAbierto = false; }

  alternarNotificaciones(): void {
    this.notificacionesAbiertas = !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void { this.notificacionesAbiertas = false; }

  abrirSeguridad(): void {
    this.seguridadAbierta = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }

  cerrarSeguridad(): void { this.seguridadAbierta = false; }

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
    this.cerrarOferta();
    this.cerrarSeguridad();
  }
}
