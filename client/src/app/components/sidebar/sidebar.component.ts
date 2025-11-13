import { Component, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { CommonModule, NgIf } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  standalone: true,           // importante para usar imports
  imports: [MatIcon, CommonModule, NgIf]  // <--- aqui
})
export class SidebarComponent {
  isMobile = false;
  mobileOpen = false;
  collapsed = false;

  readonly width = 260;
  readonly collapsedWidth = 64;

  constructor(private router: Router) {
    this.checkMobile();
  }

  @HostListener('window:resize')
  onResize() {
    this.checkMobile();
  }

  private checkMobile() {
    this.isMobile = window.matchMedia('(max-width: 800px)').matches;
    if (!this.isMobile) this.mobileOpen = false;
  }

  openMobile() {
    this.mobileOpen = true;
  }

  closeMobile() {
    this.mobileOpen = false;
  }

  toggleCollapse() {
    if (!this.isMobile) this.collapsed = !this.collapsed;
  }

  onNavigateMobile() {
    if (this.isMobile) this.closeMobile();
  }

  navigate(path: string) {
    this.router.navigate([path]);
    this.onNavigateMobile();
  }

  isActive(path: string) {
    return this.router.url.startsWith(path);
  }
}
