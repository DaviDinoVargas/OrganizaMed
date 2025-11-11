export interface MedicoDto {
  id: string;
  nome: string;
  crm: string;
}

export interface PacienteDto {
  id: string;
  nome: string;
  email: string;
  telefone: string;
}

export interface AtividadeMedicaDto {
  id?: string;
  inicio: string;
  termino?: string;
  tipoAtividade: 'Consulta' | 'Cirurgia';
  pacienteId: string;
  paciente?: PacienteDto; // Adicione esta linha
  medicos: MedicoDto[];
}
