
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-registro-empresa',
  imports: [RouterLink, FormsModule],
  templateUrl: './registro-empresa.html',
  styleUrl: './registro-empresa.css'
})
export class RegistroEmpresa {

  // Datos del formulario.
  // Por ahora solo permanecen en memoria del navegador.
  nombreEmpresa = '';
  nit = '';
  correo = '';
  contrasena = '';
  terminos = false;

  // Control de la pantalla de confirmación.
  solicitudPreparada = false;
  errorGeneral = '';

  prepararSolicitud(formulario: NgForm): void {

    this.errorGeneral = '';

    // No continuar con un formulario inválido.
    if (formulario.invalid) {
      return;
    }

    // Evitar un nombre compuesto solamente por espacios.
    if (!this.nombreEmpresa.trim()) {
      this.errorGeneral =
        'Ingresa el nombre de la empresa.';
      return;
    }

    // Normalizar los datos de texto.
    this.nombreEmpresa = this.nombreEmpresa.trim();
    this.nit = this.nit.trim();
    this.correo = this.correo.trim().toLowerCase();

    // SOLO DEMOSTRACIÓN:
    // No se llama a ninguna API ni se registra la empresa.
    this.solicitudPreparada = true;
  }

  volverAEditar(): void {
    this.solicitudPreparada = false;
    this.errorGeneral = '';
  }
}
