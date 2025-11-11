import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MedicosService } from './medicos.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-medico-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatSnackBarModule],
  templateUrl: './medico-form.component.html'
})
export class MedicoFormComponent {
  form!: FormGroup;
  id?: string;
  loading = false;
  editMode = false;

  constructor(
    private fb: FormBuilder,
    private svc: MedicosService,
    private route: ActivatedRoute,
    private router: Router,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      nome: ['', Validators.required],
      crm: ['', [Validators.required, Validators.pattern(/^\d{5}-[A-Z]{2}$/)]]
    });

    // pega param id (se existir)
    this.id = this.route.snapshot.paramMap.get('id') ?? undefined;
    this.editMode = !!this.id;

    if (this.editMode && this.id) {
      this.carregar(this.id);
    }
  }

  carregar(id: string) {
    this.loading = true;
    this.svc.obter(id).subscribe({
      next: (m) => {
        this.form.patchValue({ nome: m.nome, crm: m.crm });
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Erro ao obter médico', err);
        this.snack.open('Erro ao carregar médico', 'Fechar', { duration: 4000 });
        this.router.navigate(['/medicos']);
      }
    });
  }

  salvar() {
    if (this.form.invalid) {
      this.snack.open('Preencha corretamente os campos.', 'Fechar', { duration: 3000 });
      return;
    }
    const payload = this.form.value;
    this.loading = true;

    if (this.editMode && this.id) {
      this.svc.atualizar(this.id, payload).subscribe({
        next: () => {
          this.loading = false;
          this.snack.open('Médico atualizado.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/medicos']);
        },
        error: (err) => {
          this.loading = false;
          console.error('Erro atualizar', err);
          this.snack.open('Falha ao atualizar médico.', 'Fechar', { duration: 4000 });
        }
      });
    } else {
      this.svc.criar(payload).subscribe({
        next: () => {
          this.loading = false;
          this.snack.open('Médico criado.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/medicos']);
        },
        error: (err) => {
          this.loading = false;
          console.error('Erro criar', err);
          this.snack.open('Falha ao criar médico.', 'Fechar', { duration: 4000 });
        }
      });
    }
  }

  cancelar() {
    this.router.navigate(['/medicos']);
  }
}
