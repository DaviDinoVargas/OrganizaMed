import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AtividadeMedicaDto } from './atividade-medica.model';

@Injectable({ providedIn: 'root' })
export class AtividadesMedicasService {
  private base = 'https://localhost:7043/api/atividades-medicas';

  constructor(private http: HttpClient) {}

  private extractData<T>(raw: any): T {
    if (!raw) return raw;
    if (raw.dados) return raw.dados as T;
    if (raw.data) return raw.data as T;
    if (raw.registros) return raw.registros as T;
    return raw as T;
  }

  listar(tipo?: 'Consulta' | 'Cirurgia'): Observable<AtividadeMedicaDto[]> {
  let params = new HttpParams();
  if (tipo) params = params.set('tipoAtividade', tipo);

  return this.http.get<any>(this.base, { params }).pipe(
    map(raw => {
      // Extrai o array de registros
      return raw.registros ?? [];
    })
  );
}

  obter(id: string): Observable<AtividadeMedicaDto> {
    return this.http.get<any>(`${this.base}/${id}`)
      .pipe(map(raw => this.extractData<AtividadeMedicaDto>(raw)));
  }

  criar(payload: AtividadeMedicaDto): Observable<AtividadeMedicaDto> {
    return this.http.post<any>(this.base, payload)
      .pipe(map(raw => this.extractData<AtividadeMedicaDto>(raw)));
  }

  atualizar(id: string, payload: AtividadeMedicaDto): Observable<AtividadeMedicaDto> {
    return this.http.put<any>(`${this.base}/${id}`, payload)
      .pipe(map(raw => this.extractData<AtividadeMedicaDto>(raw)));
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<any>(`${this.base}/${id}`)
      .pipe(map(raw => this.extractData<void>(raw)));
  }

}
