import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AtividadesMedicasService } from './atividades-medicas.service';
import { PacientesService } from '../pacientes/pacientes.service';
import { MedicosService } from '../medicos/medicos.service';
import { AtividadeMedicaDto } from './atividade-medica.model';
import { PacienteDto } from '../pacientes/paciente.model';
import { MedicoDto } from '../medicos/medico.model';

@Component({
  selector: 'app-atividade-medica-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './atividade-medica-form.component.html',
  styleUrls: ['../scss/global.scss']
})
export class AtividadeMedicaFormComponent implements OnInit {
  form!: FormGroup;
  id?: string | null = null;
  loading = false;

  pacientes: PacienteDto[] = [];
  medicosList: MedicoDto[] = [];

  constructor(
    private fb: FormBuilder,
    private svc: AtividadesMedicasService,
    private pacienteService: PacientesService,
    private medicoService: MedicosService,
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
    this.carregarPacientes();
    this.carregarMedicos();

    this.route.paramMap.subscribe(params => {
      this.id = params.get('id');
      if (this.id) this.carregarAtividade(this.id);
    });
  }

  carregarPacientes() {
  this.pacienteService.getAll().subscribe({
    next: (p: any) => {  // usar 'any' ou tipar como { registros?: PacienteDto[] }
      console.log('Pacientes carregados:', p);
      this.pacientes = p?.registros ?? [];  // pega só o array se existir, senão vazio
    },
    error: (err) => {
      console.error('Erro ao carregar pacientes:', err);
      this.pacienteService.listar().subscribe({
        next: (pacientes: any) => {
          console.log('Pacientes carregados via listar:', pacientes);
          this.pacientes = pacientes?.registros ?? [];
        },
        error: (err2) => console.error('Erro também no listar:', err2)
      });
    }
  });
}

carregarMedicos() {
  this.medicoService.getAll().subscribe({
    next: (m: any) => {
      console.log('Médicos carregados:', m);
      this.medicosList = m?.registros ?? [];
    },
    error: (err) => {
      console.error('Erro ao carregar médicos:', err);
      this.medicoService.listar().subscribe({
        next: (medicos: any) => {
          console.log('Médicos carregados via listar:', medicos);
          this.medicosList = medicos?.registros ?? [];
        },
        error: (err2) => console.error('Erro também no listar:', err2)
      });
    }
  });
}

  carregarAtividade(id: string) {
    this.loading = true;
    this.svc.obter(id).subscribe({
      next: (a) => {
        console.log('Atividade carregada:', a);

        // Formatar datas para o input datetime-local
        const inicio = a.inicio ? this.formatDateForInput(a.inicio) : '';
        const termino = a.termino ? this.formatDateForInput(a.termino) : '';

        this.form.patchValue({
          inicio: inicio,
          termino: termino,
          tipoAtividade: a.tipoAtividade,
          pacienteId: a.pacienteId,
          medicos: a.medicos?.map(m => m.id) || []
        });
      },
      error: err => {
        console.error('Erro ao carregar atividade:', err);
        this.loading = false;
      },
      complete: () => this.loading = false
    });
  }

  salvar() {
    if (this.form.invalid) {
      console.log('Formulário inválido', this.form.errors);
      this.marcarCamposComoSujos();
      return;
    }

    const formValue = this.form.value;

    // Converter datas para formato ISO
    const payload: AtividadeMedicaDto = {
      ...formValue,
      inicio: this.convertToISO(formValue.inicio),
      termino: formValue.termino ? this.convertToISO(formValue.termino) : undefined,
      medicos: Array.isArray(formValue.medicos) ? formValue.medicos : [formValue.medicos]
    };

    console.log('Payload enviado:', payload);

    this.loading = true;
    const obs = this.id
      ? this.svc.atualizar(this.id, payload)
      : this.svc.criar(payload);

    obs.subscribe({
      next: () => {
        console.log('Atividade salva com sucesso');
        this.router.navigate(['/atividades-medicas']);
      },
      error: err => {
        console.error('Erro ao salvar atividade:', err);
        this.loading = false;
      }
    });
  }

  // Método auxiliar para converter data para ISO
  private convertToISO(dateTimeString: string): string {
    if (!dateTimeString) return '';
    // Adiciona os segundos se não existirem
    if (dateTimeString.length === 16) {
      dateTimeString += ':00';
    }
    return new Date(dateTimeString).toISOString();
  }

  // Método auxiliar para formatar data para o input
  private formatDateForInput(isoString: string): string {
    if (!isoString) return '';
    const date = new Date(isoString);
    return date.toISOString().slice(0, 16);
  }

  // Marcar todos os campos como touched para mostrar erros
  private marcarCamposComoSujos() {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      control?.markAsTouched();
    });
  }

  cancelar() {
    this.router.navigate(['/atividades-medicas']);
  }
}
