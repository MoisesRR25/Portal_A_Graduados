
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login-empresa',
  imports: [RouterLink, FormsModule],
  templateUrl: './login-empresa.html',
  styleUrl: './login-empresa.css'
})
export class LoginEmpresa {

  // Datos del formulario
  correo = '';
  contrasena = '';

  // Controles de la interfaz
  mostrarContrasena = false;
  mostrarRecuperacion = false;

  // Mensaje informativo del acceso
  mensajeAcceso = '';

  // Mostrar u ocultar contraseña
  alternarContrasena(): void {
    this.mostrarContrasena = !this.mostrarContrasena;
  }

  // Mostrar u ocultar aviso de recuperación
  alternarRecuperacion(): void {
    this.mostrarRecuperacion = !this.mostrarRecuperacion;
    this.mensajeAcceso = '';
  }

  limpiarMensajeAcceso(): void {
    this.mensajeAcceso = '';
  }

  // Validación inicial del formulario
  intentarIngresar(formulario: NgForm): void {

    this.mensajeAcceso = '';

    // Mostrar errores de los campos incompletos.
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

    // MODO DEMOSTRACIÓN:
    // No se consultan credenciales.
    // No se crean sesiones.
    // No se navega al panel empresarial.
    // No se envían datos a la API.
    this.mensajeAcceso =
      'El formulario está completo, pero el inicio ' +
      'de sesión de empresas todavía no está conectado ' +
      'a la API. Ninguna cuenta ha sido autenticada.';
  }
}
