// src/app/components/sidebar/sidebar.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

// IMPORTE o componente standalone, NÃO o declare
import { SidebarComponent } from './sidebar.component';

@NgModule({
  // não usar `declarations` para componente standalone
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    SidebarComponent // <-- import standalone component here
  ],
  exports: [
    SidebarComponent // <-- re-export para outros módulos poderem usar
  ]
})
export class SidebarModule { }
