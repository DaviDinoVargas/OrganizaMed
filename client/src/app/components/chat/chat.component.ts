import { Component, HostListener, OnDestroy, OnInit, ElementRef } from '@angular/core';
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

  // CONFIG: largura do chat (px) e offset do header (px)
  private chatWidthPx = 360;
  private headerOffsetPx = 72;

  private sub?: Subscription;

  constructor(private chat: ChatService, private elRef: ElementRef) {}

  ngOnInit() {
    this.sub = this.chat.messages$.subscribe(msgs => {
      this.mensagens = msgs;
      setTimeout(() => this.scrollToBottom(), 40);
    });

    this.checkMobile();
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
    this.removeBodyPadding(); // cleanup
  }

  @HostListener('window:resize')
  onResize() {
    this.checkMobile();
  }

  private checkMobile() {
    const wasMobile = this.isMobile;
    this.isMobile = window.matchMedia('(max-width: 800px)').matches;

    if (this.isMobile) {
      // mobile: início fechado (aparece o FAB)
      this.isOpen = false;
      this.removeBodyPadding();
    } else {
      // desktop: sempre aberto
      this.isOpen = true;
      // aplica padding para "reservar" espaço para o chat na página
      this.applyBodyPadding();
      // rolar para baixo após abrir
      setTimeout(() => this.scrollToBottom(), 50);
    }

    // se houve mudança de breakpoint e agora não é mobile, garantir padding
    if (!wasMobile && !this.isMobile) {
      this.applyBodyPadding();
    }
  }

  // No desktop não usa toggle — toggle só para mobile (abrir/fechar overlay)
  toggle() {
    if (this.isMobile) {
      this.isOpen = !this.isOpen;
      // mobile overlay não precisa tocar no body
    }
  }

  abrir() {
    this.isOpen = true;
    if (!this.isMobile) this.applyBodyPadding();
    setTimeout(() => this.scrollToBottom(), 100);
  }

  fechar() {
    if (this.isMobile) {
      this.isOpen = false;
    } else {
      // se quiser permitir fechar no desktop, descomente:
      // this.isOpen = false;
      // this.removeBodyPadding();
    }
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
    // usa query dentro do componente para evitar conflitar com outros .chat-messages
    const host: HTMLElement = this.elRef.nativeElement;
    const el = host.querySelector('.chat-messages') as HTMLElement | null;
    if (el) el.scrollTop = el.scrollHeight;
  }

  // -------------------- Manipulação do body para empurrar conteúdo --------------------
  private applyBodyPadding() {
    try {
      // coloca padding-right no body igual à largura do chat
      // e padding-top no caso de header offset (opcional)
      document.body.style.paddingRight = `${this.chatWidthPx}px`;
      // se você quiser compensar header em layouts com header fixo, pode ajustar paddingTop:
      // document.body.style.paddingTop = `${this.headerOffsetPx}px`;
      document.body.classList.add('has-chat-sidebar');
    } catch (e) {
      // falha silenciosa (e.g. server-side rendering ou ambiente restrito)
      console.warn('Não foi possível aplicar padding no body para sidebar do chat', e);
    }
  }

  private removeBodyPadding() {
    try {
      // limpa alterações feitas no body
      if (document.body.style.paddingRight) document.body.style.paddingRight = '';
      // if (document.body.style.paddingTop) document.body.style.paddingTop = '';
      document.body.classList.remove('has-chat-sidebar');
    } catch (e) {
      console.warn('Não foi possível remover padding do body', e);
    }
  }
}
