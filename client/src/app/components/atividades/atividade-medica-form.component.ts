import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AtividadesMedicasService } from './atividades-medicas.service';
import { AtividadeMedicaDto } from './atividade-medica.model';

@Component({
  selector: 'app-atividade-medica-form',
  templateUrl: './atividade-medica-form.component.html',
  standalone: true,
  imports: [
    CommonModule,         // ngIf, ngFor, pipes como date
    ReactiveFormsModule,  // formGroup, formControlName
    RouterModule          // routerLink e router.navigate
  ]
})
export class AtividadeMedicaFormComponent implements OnInit {
  form!: FormGroup;
  id?: string | null = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private svc: AtividadesMedicasService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      inicio: ['', Validators.required],
      termino: [''],
      tipoAtividade: ['Consulta', Validators.required],
      pacienteId: ['', Validators.required],
      medicos: [[], Validators.required]
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.id = params.get('id');
      if (this.id) this.carregar(this.id);
    });
  }

  carregar(id: string) {
    this.loading = true;
    this.svc.obter(id).subscribe({
      next: (a) => this.form.patchValue(a),
      error: (err) => console.error(err),
      complete: () => this.loading = false
    });
  }

  salvar() {
    if (this.form.invalid) return;

    const payload: AtividadeMedicaDto = this.form.value;

    this.loading = true;
    const obs = this.id
      ? this.svc.atualizar(this.id, payload)
      : this.svc.criar(payload);

    obs.subscribe({
      next: () => this.router.navigate(['/atividades-medicas']),
      error: (err) => { console.error(err); this.loading = false; }
    });
  }

  cancelar() {
    this.router.navigate(['/atividades-medicas']);
  }
}
