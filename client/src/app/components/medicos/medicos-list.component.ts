import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MedicosService, MedicoDto } from './medicos.service';

@Component({
  selector: 'app-medicos-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSnackBarModule],
  templateUrl: './medicos-list.component.html'
})
export class MedicosListComponent {
  medicos: MedicoDto[] = [];
  rawResponse: any = null; // <- declarado para o <pre> debug
  loading = false;

  constructor(
    private svc: MedicosService,
    private router: Router,
    private snack: MatSnackBar
  ) {
    this.carregar();
  }

  carregar() {
    console.log('Carregando médicos...');
    this.loading = true;
    this.rawResponse = null;

    this.svc.listar().subscribe({
      next: (dados: any) => {
        console.log('Resposta listar():', dados);
        this.rawResponse = dados;

        // Tratar diferentes formatos de retorno
        if (Array.isArray(dados)) {
          this.medicos = dados;
        } else if (dados?.registros && Array.isArray(dados.registros)) {
          this.medicos = dados.registros;
        } else if (dados?.dados && Array.isArray(dados.dados)) {
          this.medicos = dados.dados;
        } else {
          // tenta achar o primeiro array dentro do objeto
          const arr = Object.values(dados || {}).find(v => Array.isArray(v));
          this.medicos = Array.isArray(arr) ? (arr as any) : [];
        }

        console.log('medicos array final:', this.medicos);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.rawResponse = err;
        console.error('Erro ao listar médicos', err);
        this.snack.open('Erro ao listar médicos (veja console).', 'Fechar', { duration: 4000 });
      }
    });
  }

  novo() { this.router.navigate(['/medicos/new']); }
  editar(id?: string) { if (id) this.router.navigate([`/medicos/${id}/edit`]); }

  excluir(id?: string) {
    if (!id) return;
    if (!confirm('Deseja realmente excluir este médico?')) return;

    this.svc.excluir(id).subscribe({
      next: () => {
        this.snack.open('Médico excluído.', 'Fechar', { duration: 3000 });
        this.carregar();
      },
      error: (err) => {
        console.error('Erro ao excluir médico', err);
        this.snack.open('Falha ao excluir médico.', 'Fechar', { duration: 4000 });
      }
    });
  }

  verTop10() {
    console.log('Buscando top 10...');
    this.loading = true;
    const hoje = new Date();
    const inicio = new Date(hoje.getFullYear(), hoje.getMonth(), 1).toISOString();
    const fim = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0).toISOString();

    this.svc.top10(inicio, fim).subscribe({
      next: (dados: any) => {
        console.log('Resposta top10():', dados);
        this.rawResponse = dados;
        this.medicos = Array.isArray(dados) ? dados : (dados?.registros ?? dados?.dados ?? []) ?? [];
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.rawResponse = err;
        console.error('Erro top-10', err);
        this.snack.open('Falha ao obter top-10.', 'Fechar', { duration: 4000 });
      }
    });
  }
}
