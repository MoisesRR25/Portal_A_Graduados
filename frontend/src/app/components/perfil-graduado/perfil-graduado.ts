
import { isPlatformBrowser } from '@angular/common';
import { Component, HostListener, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

type TipoExperiencia = 'Experiencia' | 'Proyecto';

interface DatosPerfil {
  nombre: string;
  correo: string;
  telefono: string;
  ubicacion: string;
  biografia: string;
  estadoAcademico: string;
  anioGraduacion: string;
  estadoLaboral: string;
  portafolio: string;
  linkedin: string;
}

interface Experiencia {
  id: number;
  tipo: TipoExperiencia;
  titulo: string;
  entidad: string;
  periodo: string;
  descripcion: string;
}

@Component({
  selector: 'app-perfil-graduado',
  imports: [RouterLink, FormsModule],
  templateUrl: './perfil-graduado.html',
  styleUrl: './perfil-graduado.css'
})
export class PerfilGraduado implements OnInit {
  private readonly navegador = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly clave = 'portal-graduados-perfil-demo-v1';

  menuAbierto = false;
  notificacionesAbiertas = false;
  seguridadAbierta = false;
  cambiosPendientes = false;
  mensaje = '';
  errorFormulario = '';

  readonly ubicaciones = ['San Salvador', 'San Miguel', 'Santa Ana'];
  readonly anios = ['2026', '2025', '2024', '2023', '2022', '2021', '2020', '2019', '2018'];
  readonly estadosLaborales = [
    'Empleado en TI',
    'Fuera de TI',
    'Emprendedor',
    'Buscando empleo',
    'Estudiando'
  ];

  // Datos originales de demostración.
  perfil: DatosPerfil = {
    nombre: 'Carlos Eduardo Martínez López',
    correo: 'carlos.martinez@gmail.com',
    telefono: '+503 7777-1234',
    ubicacion: 'San Salvador',
    biografia: 'Desarrollador frontend con interés en crear productos digitales accesibles y de alto impacto.',
    estadoAcademico: 'Graduado',
    anioGraduacion: '2023',
    estadoLaboral: 'Empleado en TI',
    portafolio: 'carlosmartinez.dev',
    linkedin: 'linkedin.com/in/cmartinez'
  };

  experiencias: Experiencia[] = [
    {
      id: 1,
      tipo: 'Experiencia',
      titulo: 'Desarrollador Frontend Junior',
      entidad: 'Soluciones Digitales SV · Tiempo completo',
      periodo: 'Enero 2024 - Actualidad',
      descripcion: 'Desarrollo de interfaces web responsivas y mantenimiento de plataformas internas.'
    },
    {
      id: 2,
      tipo: 'Proyecto',
      titulo: 'Plataforma de gestión académica',
      entidad: 'Proyecto personal',
      periodo: '2023',
      descripcion: 'Aplicación web para seguimiento de actividades y calificaciones.'
    }
  ];

  habilidades = [
    'React', 'TypeScript', 'JavaScript',
    'HTML & CSS', 'Git', 'Figma'
  ];

  editandoHabilidades = false;
  nuevaHabilidad = '';
  errorHabilidad = '';

  modalExperienciaAbierto = false;
  experienciaFormulario: Experiencia = this.experienciaVacia();
  errorExperiencia = '';
  confirmarEliminacion = false;

  fotoVistaPrevia: string | null = null;
  cvArchivo: File | null = null;
  errorArchivo = '';

  // Recuperar datos guardados en esta pestaña.
  ngOnInit(): void {
    if (!this.navegador) return;

    try {
      const guardado = sessionStorage.getItem(this.clave);
      if (!guardado) return;

      const datos = JSON.parse(guardado) as {
        perfil?: DatosPerfil;
        experiencias?: Experiencia[];
        habilidades?: string[];
      };

      if (datos.perfil && typeof datos.perfil.nombre === 'string') {
        this.perfil = { ...this.perfil, ...datos.perfil };
      }

      if (Array.isArray(datos.experiencias)) {
        this.experiencias = datos.experiencias.filter(
          e => e && typeof e.titulo === 'string' &&
          typeof e.entidad === 'string'
        );
      }

      if (Array.isArray(datos.habilidades)) {
        this.habilidades = datos.habilidades.filter(
          h => typeof h === 'string'
        );
      }
    } catch {
      // Si hay datos locales dañados, mantener los originales.
    }
  }

  get nombreVisible(): string {
    const partes = this.perfil.nombre.trim().split(/\s+/).filter(Boolean);

    if (partes.length >= 3) {
      return `${partes[0]} ${partes[partes.length - 2]}`;
    }

    return partes.slice(0, 2).join(' ') || 'Graduado';
  }

  get iniciales(): string {
    const partes = this.perfil.nombre.trim().split(/\s+/).filter(Boolean);

    if (partes.length >= 3) {
      return (partes[0][0] + partes[partes.length - 2][0]).toUpperCase();
    }

    if (partes.length >= 2) {
      return (partes[0][0] + partes[1][0]).toUpperCase();
    }

    return (partes[0]?.slice(0, 2) || 'GR').toUpperCase();
  }

  get porcentajePerfil(): number {
    const campos = Object.values(this.perfil);

    const completados = campos.filter(
      valor => typeof valor === 'string' && valor.trim()
    ).length;

    const extras =
      Number(this.experiencias.length > 0) +
      Number(this.habilidades.length > 0) +
      Number(!!this.fotoVistaPrevia) +
      Number(!!this.cvArchivo);

    return Math.round(
      ((completados + extras) / (campos.length + 4)) * 100
    );
  }

  marcarPendiente(): void {
    this.cambiosPendientes = true;
    this.mensaje = '';
    this.errorFormulario = '';
  }

  // Validar y guardar únicamente en el navegador.
  guardarCambios(): void {
    const { nombre, correo, telefono } = this.perfil;

    if (!nombre.trim() || !correo.trim() || !telefono.trim()) {
      this.errorFormulario = 'Completa el nombre, el correo y el teléfono.';
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo.trim())) {
      this.errorFormulario = 'Escribe un correo electrónico válido.';
      return;
    }

    if (
      !/^[+\d\s()-]+$/.test(telefono) ||
      telefono.replace(/\D/g, '').length < 8
    ) {
      this.errorFormulario =
        'Escribe un número de teléfono válido (mínimo 8 dígitos).';
      return;
    }

    this.errorFormulario = '';
    if (!this.navegador) return;

    try {
      sessionStorage.setItem(
        this.clave,
        JSON.stringify({
          perfil: this.perfil,
          experiencias: this.experiencias,
          habilidades: this.habilidades
        })
      );

      this.cambiosPendientes = false;
      this.mensaje =
        'Datos guardados solo en esta pestaña (demostración). No se enviaron a la API.';
    } catch {
      this.errorFormulario =
        'El navegador no permitió guardar los datos locales.';
    }
  }

  // Agregar o editar experiencia.
  private experienciaVacia(): Experiencia {
    return {
      id: 0,
      tipo: 'Experiencia',
      titulo: '',
      entidad: '',
      periodo: '',
      descripcion: ''
    };
  }

  agregarExperiencia(): void {
    this.experienciaFormulario = this.experienciaVacia();
    this.errorExperiencia = '';
    this.confirmarEliminacion = false;
    this.modalExperienciaAbierto = true;
  }

  editarExperiencia(experiencia: Experiencia): void {
    this.experienciaFormulario = { ...experiencia };
    this.errorExperiencia = '';
    this.confirmarEliminacion = false;
    this.modalExperienciaAbierto = true;
  }

  guardarExperiencia(): void {
    const experiencia = {
      ...this.experienciaFormulario,
      titulo: this.experienciaFormulario.titulo.trim(),
      entidad: this.experienciaFormulario.entidad.trim(),
      periodo: this.experienciaFormulario.periodo.trim(),
      descripcion: this.experienciaFormulario.descripcion.trim()
    };

    if (!experiencia.titulo || !experiencia.entidad || !experiencia.periodo) {
      this.errorExperiencia =
        'Completa el título, la empresa o proyecto y el período.';
      return;
    }

    if (experiencia.id === 0) {
      const nuevoId = Math.max(
        0, ...this.experiencias.map(e => e.id)
      ) + 1;

      this.experiencias = [
        ...this.experiencias,
        { ...experiencia, id: nuevoId }
      ];
    } else {
      this.experiencias = this.experiencias.map(
        e => e.id === experiencia.id ? experiencia : e
      );
    }

    this.cerrarExperiencia();
    this.marcarPendiente();
  }

  eliminarExperiencia(): void {
    if (this.experienciaFormulario.id === 0) return;

    if (!this.confirmarEliminacion) {
      this.confirmarEliminacion = true;
      return;
    }

    this.experiencias = this.experiencias.filter(
      e => e.id !== this.experienciaFormulario.id
    );

    this.cerrarExperiencia();
    this.marcarPendiente();
  }

  cerrarExperiencia(): void {
    this.modalExperienciaAbierto = false;
    this.confirmarEliminacion = false;
  }

  // Habilidades.
  agregarHabilidad(): void {
    const habilidad = this.nuevaHabilidad.trim();

    if (!habilidad || habilidad.length > 30) {
      this.errorHabilidad =
        'Escribe una habilidad de 1 a 30 caracteres.';
      return;
    }

    if (this.habilidades.some(
      h => h.toLocaleLowerCase('es') === habilidad.toLocaleLowerCase('es')
    )) {
      this.errorHabilidad = 'Esa habilidad ya está agregada.';
      return;
    }

    this.habilidades = [...this.habilidades, habilidad];
    this.nuevaHabilidad = '';
    this.errorHabilidad = '';
    this.marcarPendiente();
  }

  quitarHabilidad(habilidad: string): void {
    this.habilidades = this.habilidades.filter(
      h => h !== habilidad
    );
    this.marcarPendiente();
  }

  // Seleccionar foto para vista previa (sin subirla).
  seleccionarFoto(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';

    if (!archivo) return;

    if (
      !['image/jpeg', 'image/png', 'image/webp'].includes(archivo.type) ||
      archivo.size > 2 * 1024 * 1024
    ) {
      this.errorArchivo =
        'La foto debe ser JPG, PNG o WebP y pesar como máximo 2 MB.';
      return;
    }

    const lector = new FileReader();

    lector.onload = () => {
      if (typeof lector.result === 'string') {
        this.fotoVistaPrevia = lector.result;
        this.errorArchivo = '';
        this.marcarPendiente();
      }
    };

    lector.readAsDataURL(archivo);
  }

  // Seleccionar CV (sin enviarlo).
  seleccionarCv(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';

    if (!archivo) return;

    if (
      !(archivo.type === 'application/pdf' ||
        archivo.name.toLowerCase().endsWith('.pdf')) ||
      archivo.size > 5 * 1024 * 1024
    ) {
      this.errorArchivo =
        'El CV debe ser PDF y pesar como máximo 5 MB.';
      return;
    }

    this.cvArchivo = archivo;
    this.errorArchivo = '';
    this.marcarPendiente();
  }

  // Menú y ventanas.
  alternarMenu(): void {
    this.menuAbierto = !this.menuAbierto;
    this.notificacionesAbiertas = false;
  }

  cerrarMenu(): void {
    this.menuAbierto = false;
  }

  alternarNotificaciones(): void {
    this.notificacionesAbiertas = !this.notificacionesAbiertas;
  }

  abrirSeguridad(): void {
    this.seguridadAbierta = true;
    this.cerrarMenu();
    this.notificacionesAbiertas = false;
  }

  cerrarSeguridad(): void {
    this.seguridadAbierta = false;
  }

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.notificacionesAbiertas = false;
    this.cerrarExperiencia();
    this.cerrarSeguridad();
  }
}
