// src/app/components/chat/chat.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatMessage } from './chat.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatButtonModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent {
  comando = '';
  mensagens: ChatMessage[] = [];
  loading = false;

  constructor(private chat: ChatService) {
    this.chat.messages$.subscribe(msgs => this.mensagens = msgs);
  }

  async enviarComando() {
    const texto = (this.comando || '').trim();
    if (!texto) return;
    this.loading = true;
    await this.chat.sendCommandAndCreateActivity(texto);
    this.comando = '';
    this.loading = false;

    setTimeout(() => {
      const el = document.querySelector('.chat-messages');
      if (el) el.scrollTop = el.scrollHeight;
    }, 50);
  }
}
