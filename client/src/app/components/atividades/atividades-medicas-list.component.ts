import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AtividadesMedicasService } from './atividades-medicas.service';
import { AtividadeMedicaDto, MedicoDto } from './atividade-medica.model';

@Component({
  selector: 'app-atividades-medicas-list',
  templateUrl: './atividades-medicas-list.component.html',
  standalone: true,
  imports: [
    CommonModule, // *ngFor, pipes como date
    FormsModule,  // ngModel
    RouterModule  // routerLink
  ]
})
export class AtividadesMedicasListComponent implements OnInit {
  atividades: AtividadeMedicaDto[] = [];
  loading = false;
  filtroTipo?: 'Consulta' | 'Cirurgia';

  constructor(private svc: AtividadesMedicasService, private router: Router) {}

  ngOnInit() {
    this.listar();
  }

  listar() {
  this.loading = true;
  this.svc.listar(this.filtroTipo).subscribe({
    next: data => {
      this.atividades = (data as any).dados?.registros ?? [];
      this.loading = false;
    },
    error: err => {
      console.error(err);
      this.loading = false;
    }
  });
}

  editar(id: string) {
    this.router.navigate(['/atividades-medicas/edit', id]);
  }

  excluir(id: string) {
    if (!confirm('Confirma a exclusão desta atividade?')) return;
    this.svc.excluir(id).subscribe({
      next: () => this.listar(),
      error: err => console.error(err)
    });
  }

  // Corrige o erro do map no template
  getMedicosNomes(medicos?: MedicoDto[]): string {
    if (!medicos || medicos.length === 0) return '';
    return medicos.map(m => m.nome).join(', ');
  }
}
