// src/app/components/medicos/medicos-list.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MedicosService } from './medicos.service';
import { MedicoDto } from './medico.model';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-medicos-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSnackBarModule],
  templateUrl: './medicos-list.component.html'
})
export class MedicosListComponent {
  medicos: MedicoDto[] = [];
  loading = false;

  constructor(
    private svc: MedicosService,
    private router: Router,
    private snack: MatSnackBar
  ) {
    this.carregar();
  }

  carregar() {
    this.loading = true;
    this.svc.listar().subscribe({
      next: (dados) => {
        this.medicos = dados ?? [];
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Erro ao listar médicos', err);
        this.snack.open('Erro ao listar médicos', 'Fechar', { duration: 4000 });
      }
    });
  }

  novo() {
    // navega para a rota de criação
    this.router.navigate(['/medicos/new']);
  }

  editar(id?: string) {
    if (!id) return;
    this.router.navigate([`/medicos/${id}/edit`]);
  }

  excluir(id?: string) {
    if (!id) return;
    if (!confirm('Deseja realmente excluir este médico?')) return;

    this.svc.excluir(id).subscribe({
      next: () => {
        this.snack.open('Médico excluído.', 'Fechar', { duration: 3000 });
        this.carregar();
      },
      error: (err) => {
        console.error('Erro ao excluir', err);
        this.snack.open('Falha ao excluir médico.', 'Fechar', { duration: 4000 });
      }
    });
  }
}
