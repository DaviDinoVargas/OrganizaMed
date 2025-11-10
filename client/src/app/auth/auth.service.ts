// src/app/auth/auth.service.ts
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

  // adapta tanto /autenticar quanto /registrar
  autenticar(userName: string, password: string): Observable<TokenResponse> {
    return this.http.post<any>(`${this.base}/autenticar`, { userName, password })
      .pipe(map(raw => this.handleAndSaveResponse(raw)));
  }

  registrar(userName: string, email: string, password: string): Observable<TokenResponse> {
    return this.http.post<any>(`${this.base}/registrar`, { userName, email, password })
      .pipe(map(raw => this.handleAndSaveResponse(raw)));
  }

  // centraliza parsing + salvar
  private handleAndSaveResponse(raw: any): TokenResponse {
    // 1) se o backend retornou um envelope { sucesso: true, dados: { ... } }
    if (raw && typeof raw === 'object' && raw.dados) {
      raw = raw.dados;
    }

    // 2) suportar possíveis formatos/casing
    const chave = raw?.chave ?? raw?.Chave ?? raw?.token ?? raw?.accessToken ?? '';
    const dataExpiracao = raw?.dataExpiracao ?? raw?.DataExpiracao ?? raw?.expires ?? raw?.expiration ?? new Date().toISOString();
    const usuario = raw?.usuario ?? raw?.Usuario ?? raw?.user ?? raw?.usuarioDto ?? { id: '', userName: '', email: '' };

    // 3) valida token
    if (!chave || chave.trim().length === 0) {
      // lança para o subscribe do componente tratar
      throw new Error('Resposta inválida do servidor: token de acesso ausente.');
    }

    // 4) salva no localStorage
    localStorage.setItem(this.storageKey, chave);
    localStorage.setItem(this.storageExpKey, dataExpiracao);
    localStorage.setItem(this.storageUserKey, JSON.stringify(usuario));

    return { chave, dataExpiracao, usuario } as TokenResponse;
  }

  // método utilitário para salvar token manualmente (útil em dev)
  setToken(token: string, dataExpiracaoIso: string, usuario: UsuarioDto | null = null) {
    if (!token) return false;
    const dataExp = dataExpiracaoIso ?? new Date(Date.now() + 5 * 60 * 1000).toISOString();
    localStorage.setItem(this.storageKey, token);
    localStorage.setItem(this.storageExpKey, dataExp);
    if (usuario) localStorage.setItem(this.storageUserKey, JSON.stringify(usuario));
    return true;
  }

  getToken(): string | null {
    return localStorage.getItem(this.storageKey);
  }

  getUsuario(): UsuarioDto | null {
    const v = localStorage.getItem(this.storageUserKey);
    if (!v) return null;
    try {
      return JSON.parse(v) as UsuarioDto;
    } catch {
      this.clearStorage();
      return null;
    }
  }

  isTokenExpired(): boolean {
    const exp = localStorage.getItem(this.storageExpKey);
    if (!exp) return true;
    return Date.now() >= new Date(exp).getTime();
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired();
  }

  sair(): Promise<void> {
    return this.http.post(`${this.base}/sair`, {}).toPromise()
      .catch(() => {})
      .then(() => this.clearStorage());
  }

  private clearStorage() {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.storageExpKey);
    localStorage.removeItem(this.storageUserKey);
  }
}
