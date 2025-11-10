import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TokenResponse, TokenResponseRaw, UsuarioDto } from './auth.models';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private base = 'https://localhost:7043/api/auth';
  private storageKey = 'organiza_med_token';
  private storageUserKey = 'organiza_med_user';
  private storageExpKey = 'organiza_med_exp';

  constructor(private http: HttpClient) {}

  registrar(userName: string, email: string, password: string): Observable<TokenResponse> {
    return this.http.post<TokenResponseRaw>(`${this.base}/registrar`, { userName, email, password })
      .pipe(map(raw => this.normalizeAndSave(raw)));
  }

  autenticar(userName: string, password: string): Observable<TokenResponse> {
    return this.http.post<TokenResponseRaw>(`${this.base}/autenticar`, { userName, password })
      .pipe(map(raw => this.normalizeAndSave(raw)));
  }

  private normalizeAndSave(raw: TokenResponseRaw): TokenResponse {
    const chave = raw.chave ?? raw.Chave ?? '';
    const dataExpiracao = raw.dataExpiracao ?? raw.DataExpiracao ?? new Date().toISOString();
    const usuario = raw.usuario ?? raw.Usuario ?? { id: '', userName: '', email: '' };

    // salva no localStorage
    localStorage.setItem(this.storageKey, chave);
    localStorage.setItem(this.storageExpKey, dataExpiracao);
    localStorage.setItem(this.storageUserKey, JSON.stringify(usuario));

    return { chave, dataExpiracao, usuario };
  }

  sair(): Promise<void> {
    // chama o endpoint /sair para invalidar server-side (opcional)
    // e limpa client-side
    return this.http.post(`${this.base}/sair`, {}).toPromise()
      .catch(() => {}) // ignorar erro de logout
      .then(() => {
        this.clearStorage();
      });
  }

  getToken(): string | null {
    return localStorage.getItem(this.storageKey);
  }

getUsuario(): UsuarioDto | null {
  const v = localStorage.getItem(this.storageUserKey);
  if (!v) return null;

  try {
    const parsed = JSON.parse(v) as UsuarioDto;
    if (!parsed || typeof parsed.id !== 'string') return null;
    return parsed;
  } catch {
    this.clearStorage();
    return null;
  }
}

  isTokenExpired(): boolean {
    const exp = localStorage.getItem(this.storageExpKey);
    if (!exp) return true;
    // parseando como UTC
    return Date.now() >= new Date(exp).getTime();
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired();
  }

  private clearStorage() {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.storageExpKey);
    localStorage.removeItem(this.storageUserKey);
  }
}
