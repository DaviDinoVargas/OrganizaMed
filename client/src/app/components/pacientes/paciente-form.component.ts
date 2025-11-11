// src/app/components/pacientes/paciente-form.component.ts
import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router, ParamMap } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PacientesService } from './pacientes.service';
import { PacienteDto } from './paciente.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-paciente-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatSnackBarModule],
  templateUrl: './paciente-form.component.html'
})
export class PacienteFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  id?: string | null = null;
  loading = false;
  editMode = false;
  serverErrors: string[] = [];

  private sub: Subscription | null = null;

  constructor(
    private fb: FormBuilder,
    private svc: PacientesService,
    private route: ActivatedRoute,
    private router: Router,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      nome: ['', Validators.required],
      cpf: ['', [Validators.required, Validators.pattern(/^\d{3}\.?\d{3}\.?\d{3}\-?\d{2}$/)]],
      telefone: ['', [Validators.pattern(/^\(\d{2}\)\s\d{4,5}-\d{4}$/)]],
      email: ['', [Validators.email]]
    });
  }

  ngOnInit() {
    this.sub = this.route.paramMap.subscribe((params: ParamMap) => {
      const idParam = params.get('id');
      this.id = idParam;
      this.editMode = !!this.id;

      console.log('[PacienteForm] param id:', this.id, ' editMode:', this.editMode);

      if (this.editMode && this.id) {
        this.carregar(this.id);
      } else {
        this.form.reset();
      }
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  carregar(id: string) {
    this.loading = true;
    this.svc.obter(id).subscribe({
      next: (p) => {
        console.log('[PacienteForm] carregar ->', p);
        this.form.patchValue({
          nome: p.nome,
          cpf: p.cpf,
          telefone: p.telefone,
          email: p.email
        });
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Erro ao obter paciente', err);
        this.snack.open('Erro ao carregar paciente', 'Fechar', { duration: 4000 });
        this.router.navigate(['/pacientes']);
      }
    });
  }

  formatTelefoneOnBlur() {
    const raw = this.form.get('telefone')?.value ?? '';
    const onlyDigits = raw.toString().replace(/\D/g, '');
    if (!onlyDigits) {
      this.form.get('telefone')?.setValue('');
      return;
    }
    if (onlyDigits.length === 11) {
      const part1 = onlyDigits.slice(0,2);
      const part2 = onlyDigits.slice(2,7);
      const part3 = onlyDigits.slice(7);
      this.form.get('telefone')?.setValue(`(${part1}) ${part2}-${part3}`);
    } else if (onlyDigits.length === 10) {
      const part1 = onlyDigits.slice(0,2);
      const part2 = onlyDigits.slice(2,6);
      const part3 = onlyDigits.slice(6);
      this.form.get('telefone')?.setValue(`(${part1}) ${part2}-${part3}`);
    } else {
      this.form.get('telefone')?.setValue(raw);
    }
  }

  salvar() {
    this.serverErrors = [];

    if (this.form.invalid) {
      this.snack.open('Verifique os campos. Há erros no formulário.', 'Fechar', { duration: 3500 });
      this.form.markAllAsTouched();
      return;
    }

    const rawValues = this.form.getRawValue();
    const payload: PacienteDto = { ...rawValues };

    const rawCpf = (payload.cpf ?? '').toString();
    const digits = rawCpf.replace(/\D/g, '');
    const cpfNormalizado = digits.length === 11
      ? digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
      : rawCpf;
    payload.cpf = cpfNormalizado;

    this.loading = true;

    this.svc.listar().subscribe({
      next: (todos: any) => {
        const arr = Array.isArray(todos) ? todos : (todos?.registros ?? todos?.dados ?? []);
        const lista = Array.isArray(arr) ? arr : [];

        console.log('[PacienteForm] salvar() -> this.id:', this.id, 'cpfDigits:', digits, 'lista count:', lista.length);

        // Checagem de duplicidade
        const existe = lista.find((p: any) => {
          const pCpfDigits = ((p.cpf ?? '') + '').replace(/\D/g, '');
          return pCpfDigits === digits;
        });

        if (existe && (!this.editMode || (this.editMode && String(existe.id) !== String(this.id)))) {
          this.loading = false;
          this.serverErrors = [`Um paciente com o CPF '${cpfNormalizado}' já foi cadastrado`];
          this.snack.open(this.serverErrors[0], 'Fechar', { duration: 6000 });
          return;
        }

        const successHandler = () => {
          this.loading = false;
          this.snack.open(this.editMode ? 'Paciente atualizado.' : 'Paciente criado.', 'Fechar', { duration: 3000 });
          this.router.navigate(['/pacientes']);
        };

        const errorHandler = (err: any) => {
          this.loading = false;
          console.error('Erro criar/atualizar paciente', err);
          const body = err?.error;
          if (body) {
            if (Array.isArray(body.erros)) this.serverErrors = body.erros;
            else if (Array.isArray(body)) this.serverErrors = body;
            else if (body.erros) this.serverErrors = [JSON.stringify(body.erros)];
            else this.serverErrors = [JSON.stringify(body)];
          } else {
            this.serverErrors = [err?.message ?? 'Erro desconhecido'];
          }
          this.snack.open(this.serverErrors[0] ?? 'Falha ao salvar paciente.', 'Fechar', { duration: 6000 });
        };

        if (this.editMode && this.id) {
          console.log('[PacienteForm] enviando PUT para id=', this.id, 'payload=', payload);
          this.svc.atualizar(this.id, payload).subscribe({ next: successHandler, error: errorHandler });
        } else {
          console.log('[PacienteForm] enviando POST payload=', payload);
          this.svc.criar(payload).subscribe({ next: successHandler, error: errorHandler });
        }
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Erro ao buscar pacientes para checagem de duplicidade', err);
        this.snack.open('Falha ao validar CPF. Tente novamente.', 'Fechar', { duration: 4000 });
      }
    });
  }

  cancelar() {
    this.router.navigate(['/pacientes']);
  }
}
