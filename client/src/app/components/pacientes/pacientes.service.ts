import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { PacienteDto } from './paciente.model';

@Injectable({ providedIn: 'root' })
export class PacientesService {
  private base = 'https://localhost:7043/api/pacientes';

  constructor(private http: HttpClient) {}

  private extractData<T>(raw: any): T {
    if (raw === null || raw === undefined) return raw;
    if (typeof raw === 'object' && raw.dados !== undefined) return raw.dados as T;
    if (typeof raw === 'object' && raw.data !== undefined) return raw.data as T;
    if (typeof raw === 'object' && raw.registros !== undefined) return raw.registros as T;
    return raw as T;
  }

  listar(): Observable<PacienteDto[]> {
    return this.http.get<any>(this.base).pipe(map(raw => this.extractData<PacienteDto[]>(raw)));
  }

  obter(id: string): Observable<PacienteDto> {
    return this.http.get<any>(`${this.base}/${id}`).pipe(map(raw => this.extractData<PacienteDto>(raw)));
  }

  criar(payload: PacienteDto): Observable<PacienteDto> {
    return this.http.post<any>(this.base, payload).pipe(map(raw => this.extractData<PacienteDto>(raw)));
  }

  atualizar(id: string, payload: PacienteDto): Observable<PacienteDto> {
    return this.http.put<any>(`${this.base}/${id}`, payload).pipe(map(raw => this.extractData<PacienteDto>(raw)));
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<any>(`${this.base}/${id}`).pipe(map(raw => this.extractData<void>(raw)));
  }

  // exemplo de filtro / paginação (opcional)
  buscarPorNome(nome: string): Observable<PacienteDto[]> {
    const params = new HttpParams().set('nome', nome);
    return this.http.get<any>(this.base, { params }).pipe(map(raw => this.extractData<PacienteDto[]>(raw)));
  }
}
