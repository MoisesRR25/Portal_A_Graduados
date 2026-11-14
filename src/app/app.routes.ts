import { Routes } from '@angular/router';
import { Inicio } from './components/inicio/inicio';
import { BolsaTrabajo } from './components/bolsa-trabajo/bolsa-trabajo';
import { Login } from './components/login/login';
import { LoginEmpresa } from './components/login-empresa/login-empresa';
import { RegistroGraduado } from './components/registro-graduado/registro-graduado';
import { RegistroEmpresa } from './components/registro-empresa/registro-empresa';
import { Dashboard } from './components/dashboard/dashboard';
import { PerfilGraduado } from './components/perfil-graduado/perfil-graduado';
import { BolsaInterna } from './components/bolsa-interna/bolsa-interna';
import { MisPostulaciones } from './components/mis-postulaciones/mis-postulaciones';
import { MisOfertasEmpresa } from './components/mis-ofertas-empresa/mis-ofertas-empresa';

export const routes: Routes = [
  { path: '', component: Inicio },
  { path: 'bolsa-trabajo', component: BolsaTrabajo },
  { path: 'login', component: Login },
  { path: 'login-empresa', component: LoginEmpresa },
  { path: 'registro-graduado', component: RegistroGraduado },
  { path: 'registro-empresa', component: RegistroEmpresa },
  { path: 'dashboard', component: Dashboard },
  { path: 'perfil-graduado', component: PerfilGraduado },
  { path: 'bolsa-interna', component: BolsaInterna },
  { path: 'mis-postulaciones', component: MisPostulaciones },
  { path: 'mis-ofertas-empresa', component: MisOfertasEmpresa },
  { path: '**', redirectTo: '' }
];