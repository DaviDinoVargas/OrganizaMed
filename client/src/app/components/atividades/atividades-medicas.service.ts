import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AtividadeMedicaDto } from './atividade-medica.model';

@Injectable({ providedIn: 'root' })
export class AtividadesMedicasService {
  private base = 'https://localhost:7043/api/atividades-medicas';

  constructor(private http: HttpClient) {}

  listar(tipo?: 'Consulta' | 'Cirurgia'): Observable<AtividadeMedicaDto[]> {
  let params = new HttpParams();
  if (tipo) params = params.set('tipoAtividade', tipo);

  return this.http.get<any>(this.base, { params }).pipe(
    map(raw => {
      console.log('Resposta raw do API:', raw);

      const registros = raw?.dados?.registros ?? [];

      return registros.map((a: any) => ({
        id: a.id,
        inicio: a.inicio,
        termino: a.termino,
        tipoAtividade: a.tipoAtividade,
        pacienteId: a.paciente?.id ?? '',
        paciente: a.paciente ?? { nome: 'N/A', id: '', email: '', telefone: '' },
        medicos: a.medicos ?? []
      })) as AtividadeMedicaDto[];
    })
  );
}


  obter(id: string): Observable<AtividadeMedicaDto> {
    return this.http.get<any>(`${this.base}/${id}`).pipe(
      map(raw => {
        console.log('Resposta detalhe:', raw);
        return this.extractData<AtividadeMedicaDto>(raw);
      })
    );
  }

  criar(payload: AtividadeMedicaDto): Observable<any> {
    console.log('Criando atividade:', payload);
    return this.http.post<any>(this.base, payload).pipe(
      map(raw => {
        console.log('Resposta criação:', raw);
        return this.extractData<any>(raw);
      })
    );
  }

  atualizar(id: string, payload: AtividadeMedicaDto): Observable<any> {
    console.log('Atualizando atividade:', payload);
    return this.http.put<any>(`${this.base}/${id}`, payload).pipe(
      map(raw => {
        console.log('Resposta atualização:', raw);
        return this.extractData<any>(raw);
      })
    );
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<any>(`${this.base}/${id}`).pipe(
      map(raw => this.extractData<void>(raw))
    );
  }

  private extractData<T>(raw: any): T {
    console.log('Extraindo dados de:', raw);
    if (!raw) return raw;
    if (raw.dados) return raw.dados as T;
    if (raw.data) return raw.data as T;
    if (raw.registros) return raw.registros as T;
    return raw as T;
  }
}
