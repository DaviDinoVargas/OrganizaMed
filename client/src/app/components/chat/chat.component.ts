// src/app/components/chat/chat.component.ts
import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatMessage } from './chat.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatButtonModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, OnDestroy {
  mensagens: ChatMessage[] = [];
  comando = '';
  loading = false;

  isMobile = false;
  isOpen = true; // NO DESKTOP: sempre aberto

  private sub?: Subscription;

  constructor(private chat: ChatService) {}

  ngOnInit() {
    this.sub = this.chat.messages$.subscribe(msgs => {
      this.mensagens = msgs;
      setTimeout(() => this.scrollToBottom(), 40);
    });

    this.checkMobile();
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  @HostListener('window:resize')
  onResize() {
    this.checkMobile();
  }

  private checkMobile() {
    const wasMobile = this.isMobile;
    this.isMobile = window.matchMedia('(max-width: 600px)').matches;

    if (this.isMobile) {
      // mobile: início fechado (aparece o FAB)
      this.isOpen = false;
    } else {
      // desktop: sempre aberto
      this.isOpen = true;
    }

    // se houve mudança, rolar para baixo quando abrir
    if (!wasMobile && !this.isMobile) {
      setTimeout(() => this.scrollToBottom(), 50);
    }
  }

  // No desktop não usa toggle — toggle só para mobile (abrir/fechar overlay)
  toggle() {
    if (this.isMobile) this.isOpen = !this.isOpen;
  }

  abrir() {
    this.isOpen = true;
    setTimeout(() => this.scrollToBottom(), 100);
  }

  fechar() {
    if (this.isMobile) this.isOpen = false;
  }

  async enviarComando() {
    const texto = (this.comando || '').trim();
    if (!texto) return;
    this.loading = true;
    await this.chat.sendCommandAndCreateActivity(texto);
    this.comando = '';
    this.loading = false;
    setTimeout(() => this.scrollToBottom(), 50);
  }

  private scrollToBottom() {
    const el = document.querySelector('.chat-messages');
    if (el) el.scrollTop = el.scrollHeight;
  }
}
