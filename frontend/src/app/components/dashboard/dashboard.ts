
import { Component, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {

  // Menú lateral para teléfonos
  menuAbierto = false;

  // Panel de notificaciones
  notificacionesAbiertas = false;

  // Vista local de acceso y seguridad
  vistaSeguridad = false;

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
    this.notificacionesAbiertas = !this.notificacionesAbiertas;
  }

  cerrarNotificaciones(): void {
    this.notificacionesAbiertas = false;
  }

  // NAVEGACIÓN INTERNA DEL DASHBOARD
  mostrarSeguridad(): void {
    this.vistaSeguridad = true;
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }

  mostrarResumen(): void {
    this.vistaSeguridad = false;
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }

  // CERRAR MENÚS CON ESCAPE
  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {
    this.cerrarMenu();
    this.cerrarNotificaciones();
  }
}
