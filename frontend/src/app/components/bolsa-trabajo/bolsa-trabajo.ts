
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type Modalidad = 'Presencial' | 'Híbrido' | 'Remoto';
type OrdenOfertas = 'recientes' | 'compatibilidad';

interface OfertaPublica {
  id: number;
  empresa: string;
  iniciales: string;
  color: string;
  puesto: string;
  area: string;
  ubicacion: string;
  modalidad: Modalidad;
  habilidades: string[];
  compatibilidad: number;
  diasPublicacion: number;
  tiempoPublicado: string;
}

@Component({
  selector: 'app-bolsa-trabajo',
  imports: [RouterLink, FormsModule],
  templateUrl: './bolsa-trabajo.html',
  styleUrl: './bolsa-trabajo.css'
})
export class BolsaTrabajo {

  // ========================================
  // DATOS DE DEMOSTRACIÓN
  // ========================================

  readonly ofertas: OfertaPublica[] = [
    {
      id: 1,
      empresa: 'Applaudo Studios',
      iniciales: 'AS',
      color: 'as',
      puesto: 'Desarrollador Frontend Jr.',
      area: 'Desarrollo de software',
      ubicacion: 'San Salvador',
      modalidad: 'Híbrido',
      habilidades: [
        'React',
        'TypeScript',
        'Sin experiencia'
      ],
      compatibilidad: 87,
      diasPublicacion: 2,
      tiempoPublicado: 'Hace 2 días'
    },
    {
      id: 2,
      empresa: 'Banco Agrícola',
      iniciales: 'BA',
      color: 'ba',
      puesto: 'Analista de Datos',
      area: 'Datos y analítica',
      ubicacion: 'Antiguo Cuscatlán',
      modalidad: 'Presencial',
      habilidades: [
        'SQL',
        'Power BI',
        'Python'
      ],
      compatibilidad: 87,
      diasPublicacion: 4,
      tiempoPublicado: 'Hace 4 días'
    }
  ];

  readonly areas = [
    'Desarrollo de software',
    'Datos y analítica',
    'Soporte técnico'
  ];

  readonly modalidades: Modalidad[] = [
    'Presencial',
    'Híbrido',
    'Remoto'
  ];

  // ========================================
  // BÚSQUEDA Y FILTROS
  // ========================================

  textoBusqueda = '';
  busquedaAplicada = '';

  areaSeleccionada = '';
  modalidadesSeleccionadas: Modalidad[] = [];

  ordenSeleccionado: OrdenOfertas = 'recientes';

  // ========================================
  // VENTANA DE DETALLE
  // ========================================

  ofertaSeleccionada: OfertaPublica | null = null;

  // ========================================
  // NORMALIZAR TEXTO PARA BÚSQUEDA
  // ========================================

  private normalizar(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  // ========================================
  // OBTENER OFERTAS FILTRADAS
  // ========================================

  get ofertasFiltradas(): OfertaPublica[] {

    const busqueda = this.normalizar(
      this.busquedaAplicada
    );

    const resultados = this.ofertas.filter(oferta => {

      const contenido = this.normalizar([
        oferta.puesto,
        oferta.empresa,
        oferta.area,
        oferta.ubicacion,
        oferta.modalidad,
        ...oferta.habilidades
      ].join(' '));

      const coincideBusqueda =
        !busqueda || contenido.includes(busqueda);

      const coincideArea =
        !this.areaSeleccionada ||
        oferta.area === this.areaSeleccionada;

      const coincideModalidad =
        this.modalidadesSeleccionadas.length === 0 ||
        this.modalidadesSeleccionadas.includes(
          oferta.modalidad
        );

      return (
        coincideBusqueda &&
        coincideArea &&
        coincideModalidad
      );

    });

    // Ordenar sin modificar la lista original.
    return resultados.sort((a, b) => {

      if (this.ordenSeleccionado === 'compatibilidad') {

        return (
          b.compatibilidad - a.compatibilidad ||
          a.diasPublicacion - b.diasPublicacion
        );

      }

      return a.diasPublicacion - b.diasPublicacion;

    });
  }

  // ========================================
  // CONTADORES
  // ========================================

  get totalOfertas(): number {
    return this.ofertas.length;
  }

  get totalEncontradas(): number {
    return this.ofertasFiltradas.length;
  }

  get hayFiltrosActivos(): boolean {
    return Boolean(
      this.busquedaAplicada ||
      this.areaSeleccionada ||
      this.modalidadesSeleccionadas.length
    );
  }

  // ========================================
  // BUSCAR OFERTAS
  // ========================================

  buscarOfertas(): void {
    this.busquedaAplicada = this.textoBusqueda.trim();
  }

  // ========================================
  // FILTRAR POR MODALIDAD
  // ========================================

  alternarModalidad(modalidad: Modalidad): void {

    if (this.modalidadesSeleccionadas.includes(modalidad)) {

      this.modalidadesSeleccionadas =
        this.modalidadesSeleccionadas.filter(
          seleccionada => seleccionada !== modalidad
        );

    } else {

      this.modalidadesSeleccionadas = [
        ...this.modalidadesSeleccionadas,
        modalidad
      ];

    }
  }

  // ========================================
  // LIMPIAR FILTROS
  // ========================================

  limpiarFiltros(): void {

    this.textoBusqueda = '';
    this.busquedaAplicada = '';

    this.areaSeleccionada = '';
    this.modalidadesSeleccionadas = [];

    this.ordenSeleccionado = 'recientes';
  }

  // ========================================
  // VER DETALLE DE OFERTA
  // ========================================

  verOferta(oferta: OfertaPublica): void {
    this.ofertaSeleccionada = oferta;
  }

  cerrarOferta(): void {
    this.ofertaSeleccionada = null;
  }

  // ========================================
  // CERRAR VENTANA CON ESCAPE
  // ========================================

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarOferta();
  }
}
