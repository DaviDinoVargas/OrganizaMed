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
import { filter } from 'rxjs';

import { ChatComponent } from './components/chat/chat.component';
import { LogoutWidgetComponent } from './auth/logout-widget.component';
import { AuthService } from './auth/auth.service';

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
    LogoutWidgetComponent,
],
  template: `
    <app-logout-widget></app-logout-widget>
    <router-outlet></router-outlet>
    <app-chat *ngIf="showChat"></app-chat>
  `,
})
export class AppComponent {
  showChat = false;
  private hideRoutes = ['/login', '/registrar'];

  constructor(private router: Router, private auth: AuthService) {
    // inicializa visibilidade
    this.updateShowChat(this.router.url);

    // atualiza em navegação
    this.router.events.pipe(
      filter(evt => evt instanceof NavigationEnd)
    ).subscribe((evt: any) => {
      this.updateShowChat(evt.urlAfterRedirects ?? evt.url);
    });
  }

  private updateShowChat(url: string) {
    const path = url.split('?')[0].split('#')[0];
    const isAuthRoute = this.hideRoutes.includes(path);
    this.showChat = !isAuthRoute && this.auth.isLoggedIn();
  }
}
