
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  // Datos del formulario
  correo = '';
  contrasena = '';

  // Controles de la interfaz
  mostrarContrasena = false;
  mostrarRecuperacion = false;

  // Mensaje informativo de autenticación
  mensajeAcceso = '';

  alternarContrasena(): void {
    this.mostrarContrasena = !this.mostrarContrasena;
  }

  alternarRecuperacion(): void {
    this.mostrarRecuperacion = !this.mostrarRecuperacion;
    this.mensajeAcceso = '';
  }

  limpiarMensajeAcceso(): void {
    this.mensajeAcceso = '';
  }

  intentarIngresar(formulario: NgForm): void {

    this.mensajeAcceso = '';

    // Mostrar los errores de todos los campos.
    formulario.control.markAllAsTouched();

    if (formulario.invalid) {
      return;
    }

    // Normalizar el correo.
    const correoNormalizado = this.correo
      .trim()
      .toLowerCase();

    if (!correoNormalizado) {
      return;
    }

    this.correo = correoNormalizado;

    // DEMOSTRACIÓN:
    // Aquí NO se comprueban credenciales.
    // NO se crea una sesión.
    // NO se navega al Dashboard.
    // NO se llama a la API ni a PostgreSQL.
    this.mensajeAcceso =
      'El formulario está completo, pero el inicio ' +
      'de sesión todavía no está conectado a la API. ' +
      'Ninguna cuenta ha sido autenticada.';
  }
}
