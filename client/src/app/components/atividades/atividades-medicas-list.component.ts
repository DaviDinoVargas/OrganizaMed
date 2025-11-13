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
  ],
  styleUrls: ['../scss/global.scss']
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
      this.atividades = data; // já vem limpo
      this.loading = false;
    },
    error: err => {
      console.error('Erro ao listar atividades:', err);
      this.loading = false;
    }
  });
}

 editar(id: string | undefined) {
  if (!id) return;
  this.router.navigate(['/atividades-medicas/edit', id]);
}

excluir(id: string | undefined) {
  if (!id || !confirm('Confirma a exclusão desta atividade?')) return;
  this.svc.excluir(id).subscribe({
    next: () => this.listar(),
    error: err => console.error(err)
  });
}

 getMedicosNomes(medicos: { nome: string }[]): string {
  return medicos.map(m => m.nome).join(', ');
}
}
