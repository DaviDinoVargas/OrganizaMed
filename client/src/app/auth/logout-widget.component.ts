// src/app/components/auth/logout-widget.component.ts
import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthService } from '../auth/auth.service'; // ajuste caminho se necessário
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-logout-widget',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './logout-widget.component.html',
  styleUrls: ['./css/logout-widget.component.scss']
})
export class LogoutWidgetComponent implements OnDestroy {
  public show = false;
  public usuarioNome: string | null = null;

  private sub?: Subscription;

  // rotas onde o widget ficará oculto
  private hidePrefixes = ['/login', '/registrar'];

  constructor(
    private router: Router,
    private auth: AuthService,
    private snack: MatSnackBar
  ) {
    // atualiza visibilidade no início
    this.updateVisibility(this.router.url);

    // atualiza em navegações
    this.sub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e: any) => this.updateVisibility(e.urlAfterRedirects ?? e.url));
  }

  private updateVisibility(url: string) {
    const path = (url.split('?')[0] || '').toLowerCase();
    const isHidden = this.hidePrefixes.some(p => path.startsWith(p));
    // também escondemos se não estiver logado
    const logged = this.auth.isLoggedIn();
    this.show = !isHidden && logged;

    const usuario = this.auth.getUsuario();
    this.usuarioNome = usuario?.userName ?? null;
  }

  async sair() {
    try {
      await this.auth.sair();
      this.snack.open('Deslogado com sucesso', 'OK', { duration: 2500 });
      // limpa estado local e redireciona para login
      this.router.navigate(['/login']);
    } catch (err) {
      console.error('Erro no logout', err);
      this.snack.open('Erro ao deslogar', 'Fechar', { duration: 4000 });
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
