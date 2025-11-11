import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { RegisterComponent } from './auth/register.component';
import { HomeComponent } from './components/home/home.component';
//import { PacientesComponent } from './components/pacientes/pacientes.component';
import { AtividadesComponent } from './components/atividades/atividades.component';
import { AuthGuard } from './auth/auth.guard';

import { MedicosListComponent } from './components/medicos/medicos-list.component';
import { MedicoFormComponent } from './components/medicos/medico-form.component';

import { PacientesListComponent } from './components/pacientes/pacientes-list.component';
import { PacienteFormComponent } from './components/pacientes/paciente-form.component';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'registrar', component: RegisterComponent },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },

  // usa MEDICOS_LIST como a página /medicos
  { path: 'medicos', component: MedicosListComponent, canActivate: [AuthGuard] },
  { path: 'medicos/new', component: MedicoFormComponent, canActivate: [AuthGuard] },
  { path: 'medicos/:id/edit', component: MedicoFormComponent, canActivate: [AuthGuard] },

  //{ path: 'pacientes', component: PacientesComponent, canActivate: [AuthGuard] },
  { path: 'atividades', component: AtividadesComponent, canActivate: [AuthGuard] },

  { path: 'pacientes/new', component: PacienteFormComponent, canActivate: [AuthGuard] },
  { path: 'pacientes/:id/edit', component: PacienteFormComponent, canActivate: [AuthGuard] },
  { path: 'pacientes', component: PacientesListComponent, canActivate: [AuthGuard] },

  { path: '**', redirectTo: 'home' }
];
