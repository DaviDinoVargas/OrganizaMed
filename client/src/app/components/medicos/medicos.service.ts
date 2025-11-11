// src/app/components/medicos/medicos.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface MedicoDto {
  id?: string;
  nome: string;
  crm: string;
}

@Injectable({ providedIn: 'root' })
export class MedicosService {
  private base = 'https://localhost:7043/api/medicos';

  constructor(private http: HttpClient) {}

  private extractData<T>(raw: any): T {
    if (raw === null || raw === undefined) return raw;
    if (typeof raw === 'object' && raw.dados !== undefined) return raw.dados as T;
    if (typeof raw === 'object' && raw.data !== undefined) return raw.data as T;
    if (typeof raw === 'object' && raw.registros !== undefined) return raw.registros as T;
    return raw as T;
  }

  listar(): Observable<MedicoDto[] | any> {
    return this.http.get<any>(this.base).pipe(
      map(raw => this.extractData<MedicoDto[]>(raw))
    );
  }

  obter(id: string) {
    return this.http.get<any>(`${this.base}/${id}`).pipe(
      map(raw => this.extractData<MedicoDto>(raw))
    );
  }

  criar(payload: MedicoDto) {
    return this.http.post<any>(this.base, payload).pipe(
      map(raw => this.extractData<MedicoDto>(raw))
    );
  }

  atualizar(id: string, payload: MedicoDto) {
    return this.http.put<any>(`${this.base}/${id}`, payload).pipe(
      map(raw => this.extractData<MedicoDto>(raw))
    );
  }

  excluir(id: string) {
    return this.http.delete<any>(`${this.base}/${id}`).pipe(
      map(raw => this.extractData<void>(raw))
    );
  }

  top10(dataInicioIso: string, dataFimIso: string) {
    const params = new HttpParams().set('dataInicio', dataInicioIso).set('dataFim', dataFimIso);
    return this.http.get<any>(`${this.base}/top-10`, { params }).pipe(
      map(raw => this.extractData<MedicoDto[]>(raw))
    );
  }
}
