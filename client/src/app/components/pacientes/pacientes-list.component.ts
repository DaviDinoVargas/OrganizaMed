import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PacientesService, } from './pacientes.service';
import { PacienteDto } from './paciente.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pacientes-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSnackBarModule, FormsModule],
  templateUrl: './pacientes-list.component.html',
  styleUrls: ['../scss/global.scss']
})
export class PacientesListComponent {
  pacientes: PacienteDto[] = [];
  rawResponse: any = null;
  loading = false;

  constructor(
    private svc: PacientesService,
    private router: Router,
    private snack: MatSnackBar
  ) {
    this.carregar();
  }

  carregar() {
    console.log('Carregando pacientes...');
    this.loading = true;
    this.rawResponse = null;

    this.svc.listar().subscribe({
      next: (dados: any) => {
        console.log('Resposta listar pacientes():', dados);
        this.rawResponse = dados;

        if (Array.isArray(dados)) this.pacientes = dados;
        else if (dados?.registros && Array.isArray(dados.registros)) this.pacientes = dados.registros;
        else if (dados?.dados && Array.isArray(dados.dados)) this.pacientes = dados.dados;
        else {
          const arr = Object.values(dados || {}).find(v => Array.isArray(v));
          this.pacientes = Array.isArray(arr) ? arr as any : [];
        }

        console.log('pacientes array final:', this.pacientes);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.rawResponse = err;
        console.error('Erro ao listar pacientes', err);
        this.snack.open('Erro ao listar pacientes (veja console).', 'Fechar', { duration: 4000 });
      }
    });
  }

  novo() { this.router.navigate(['/pacientes/new']); }
  editar(id?: string) { if (id) this.router.navigate([`/pacientes/${id}/edit`]); }

  excluir(id?: string) {
    if (!id) return;
    if (!confirm('Deseja realmente excluir este paciente?')) return;

    this.svc.excluir(id).subscribe({
      next: () => {
        this.snack.open('Paciente excluído.', 'Fechar', { duration: 3000 });
        this.carregar();
      },
      error: (err) => {
        console.error('Erro ao excluir paciente', err);
        this.snack.open('Falha ao excluir paciente.', 'Fechar', { duration: 4000 });
      }
    });
  }
}
