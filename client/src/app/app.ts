// src/app/app.ts  (ou onde estiver seu AppComponent)
import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ChatComponent } from './components/chat/chat.component'; // confirme esse caminho
import { AuthService } from './auth/auth.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ChatComponent,
  ],
  template: `
    <router-outlet></router-outlet>
    <app-chat *ngIf="showChat"></app-chat>
  `,
})
export class AppComponent {
  showChat = false;
  private hideRoutes = ['/login', '/registrar'];

  constructor(private router: Router, private auth: AuthService) {
    // inicial
    this.updateShowChat(this.router.url);

    // atualiza quando navega
    this.router.events.pipe(
      filter(evt => evt instanceof NavigationEnd)
    ).subscribe((evt: any) => {
      this.updateShowChat(evt.urlAfterRedirects ?? evt.url);
    });
  }

  private updateShowChat(url: string) {
    const path = url.split('?')[0].split('#')[0];
    const isAuthRoute = this.hideRoutes.includes(path);
    // mostra só se não for rota de auth e usuário estiver logado
    this.showChat = !isAuthRoute && this.auth.isLoggedIn();
  }
}
