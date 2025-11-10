import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService, private snack: MatSnackBar, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.getToken();
    let cloned = req;
    if (token) {
      cloned = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(cloned).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401) {
          // Sessão expirada ou token inválido
          this.snack.open('Sessão expirada ou não autorizada. Faça login novamente.', 'Fechar', { duration: 5000 });
          this.auth.sair().finally(() => this.router.navigate(['/login']));
        } else if (err.status === 403) {
          this.snack.open('Acesso negado.', 'Fechar', { duration: 4000 });
        } else {
          // mensagem genérica (pode ser melhorada lendo err.error)
          const msg = err.error && (typeof err.error === 'string' ? err.error : (err.error?.message ?? JSON.stringify(err.error))) || 'Erro na requisição';
          this.snack.open(msg, 'Fechar', { duration: 4000 });
        }
        return throwError(() => err);
      })
    );
  }
}
