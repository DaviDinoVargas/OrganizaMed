import { Component, ViewChild } from '@angular/core';
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
import { SidebarComponent } from "./components/sidebar/sidebar.component";

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
    SidebarComponent
  ],
  template: `
    <app-logout-widget></app-logout-widget>

    <div class="app-container">
      <!-- Sidebar só aparece em rotas permitidas -->
      <app-sidebar #sidebar *ngIf="showSidebar"></app-sidebar>

      <main class="app-main" [class.collapsed]="sidebar?.collapsed">
        <router-outlet></router-outlet>
      </main>
    </div>

    <app-chat *ngIf="showChat"></app-chat>
  `,
  styles: [`
    .app-container {
      display: flex;
      flex-direction: row;
      height: 100vh;
      width: 100%;
      overflow: hidden; /* remove scroll geral */
    }

    .app-main {
      flex: 1;
      padding: 16px;
      box-sizing: border-box;
      height: 100%;
      overflow: hidden; /* remove scroll do main */
      transition: margin 180ms ease;
    }

    /* Ajusta a margem quando a sidebar está colapsada */
    .app-main.collapsed {
      margin-left: 64px;
    }

    @media (max-width: 800px) {
      .app-main {
        margin-left: 0; /* mobile: sidebar flutua */
      }
    }
  `]
})
export class AppComponent {
  @ViewChild('sidebar') sidebar?: SidebarComponent;

  showChat = false;
  private hideRoutes = ['/login', '/registrar'];

  constructor(private router: Router, private auth: AuthService) {
    this.updateShowChat(this.router.url);

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

  get showSidebar(): boolean {
    const path = this.router.url.split('?')[0].split('#')[0];
    return !this.hideRoutes.includes(path); // sidebar oculta em login/registrar
  }
}
