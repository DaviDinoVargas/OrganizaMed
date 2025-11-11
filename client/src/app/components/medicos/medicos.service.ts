import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MedicoDto } from './medico.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class MedicosService {
  private base = 'https://localhost:7043/api/medicos';

  constructor(private http: HttpClient) {}

  listar(): Observable<MedicoDto[]> {
    return this.http.get<MedicoDto[]>(this.base);
  }

  obter(id: string): Observable<MedicoDto> {
    return this.http.get<MedicoDto>(`${this.base}/${id}`);
  }

  criar(payload: MedicoDto): Observable<MedicoDto> {
    return this.http.post<MedicoDto>(this.base, payload);
  }

  atualizar(id: string, payload: MedicoDto): Observable<MedicoDto> {
    return this.http.put<MedicoDto>(`${this.base}/${id}`, payload);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  // opcional: top-10 com período
  top10(dataInicioIso: string, dataFimIso: string): Observable<MedicoDto[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicioIso)
      .set('dataFim', dataFimIso);
    return this.http.get<MedicoDto[]>(`${this.base}/top-10`, { params });
  }
}
