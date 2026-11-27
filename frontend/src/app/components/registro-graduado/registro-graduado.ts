
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-registro-graduado',
  imports: [RouterLink, FormsModule],
  templateUrl: './registro-graduado.html',
  styleUrl: './registro-graduado.css'
})
export class RegistroGraduado {

  // Datos del formulario de demostración.
  nombreCompleto = '';
  carnet = '';
  correo = '';
  contrasena = '';
  terminos = false;

  // Estado local del formulario.
  datosRevisados = false;
  errorGeneral = '';

  prepararRegistro(formulario: NgForm): void {

    this.errorGeneral = '';

    // Mostrar las validaciones incluso si
    // el usuario intenta continuar sin llenar los campos.
    formulario.control.markAllAsTouched();

    if (formulario.invalid) {
      return;
    }

    // Normalizar los valores ingresados.
    const nombre = this.nombreCompleto
      .trim()
      .replace(/\s+/g, ' ');

    const carnetNormalizado = this.carnet
      .trim()
      .toUpperCase();

    const correoNormalizado = this.correo
      .trim()
      .toLowerCase();

    // Evitar nombres formados por espacios.
    if (nombre.length < 3) {
      this.errorGeneral =
        'Ingresa un nombre completo válido.';
      return;
    }

    // Validación provisional del formato de carnet
    // mostrado en el formulario: 2 letras y 5 dígitos.
    if (!/^[A-Z]{2}[0-9]{5}$/.test(carnetNormalizado)) {
      this.errorGeneral =
        'El carnet debe contener 2 letras y 5 números.';
      return;
    }

    this.nombreCompleto = nombre;
    this.carnet = carnetNormalizado;
    this.correo = correoNormalizado;

    // DEMOSTRACIÓN:
    // No se crea ninguna cuenta.
    // No se envían datos a la API.
    // No se guarda la contraseña.
    this.datosRevisados = true;
  }

  volverAEditar(): void {
    this.datosRevisados = false;
    this.errorGeneral = '';
  }
}
