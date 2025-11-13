// src/app/components/chat/chat.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface ChatMessage {
  id?: string;
  // permite os literais 'user' | 'bot' mas também aceita string vinda de runtime
  usuario: 'user' | 'bot' | string;
  texto: string;
  ts?: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  // ajuste as URLs conforme sua infra
  private pythonUrl = 'http://127.0.0.1:8000/comando';
  private dotnetBase = 'https://localhost:7043/api';

  constructor(private http: HttpClient) {}

  getMessagesSnapshot(): ChatMessage[] {
    return this.messagesSubject.getValue();
  }

  pushLocalMessage(msg: ChatMessage) {
    const normalized: ChatMessage = {
      ...msg,
      ts: msg.ts ?? new Date().toISOString()
    };
    const arr = [...this.getMessagesSnapshot(), normalized];
    this.messagesSubject.next(arr);
  }

  clear() {
    this.messagesSubject.next([]);
  }

  // Envia comando para o interpretador Python e, opcionalmente, cria atividade no backend.
  async sendCommandAndCreateActivity(command: string): Promise<ChatMessage> {
    // publica a mensagem do usuário
    this.pushLocalMessage({ usuario: 'user', texto: command } as ChatMessage);

    try {
      const pyRes: any = await firstValueFrom(this.http.post<any>(this.pythonUrl, { mensagem: command }));

      if (!pyRes?.sucesso || !pyRes?.dados) {
        const errMsg = pyRes?.erro ?? 'Interpretador não retornou dados';
        const botMsg: ChatMessage = { usuario: 'bot', texto: `Erro: ${errMsg}`, ts: new Date().toISOString() };
        this.pushLocalMessage(botMsg);
        return botMsg;
      }

      const dados = pyRes.dados;
      const botMsgInterpret: ChatMessage = { usuario: 'bot', texto: JSON.stringify(dados), ts: new Date().toISOString() };
      this.pushLocalMessage(botMsgInterpret);

      // tentativa de criar atividade (não quebra a UI se falhar)
      try {
        // 1) buscar pacientes
        const pacResp: any = await firstValueFrom(this.http.get<any>(`${this.dotnetBase}/pacientes`));
        const pacientesList = (pacResp?.dados?.registros ?? pacResp?.registros ?? pacResp) as any[] || [];
        const paciente = pacientesList.find((p: any) => (p.nome ?? '').trim().toLowerCase() === (dados.pacienteNome ?? '').trim().toLowerCase());
        if (!paciente) {
          const m: ChatMessage = { usuario: 'bot', texto: `Paciente "${dados.pacienteNome}" não encontrado.`, ts: new Date().toISOString() };
          this.pushLocalMessage(m);
          return m;
        }

        // 2) buscar médicos
        const medResp: any = await firstValueFrom(this.http.get<any>(`${this.dotnetBase}/medicos`));
        const medList = (medResp?.dados?.registros ?? medResp?.registros ?? medResp) as any[] || [];
        const medico = medList.find((m: any) => (m.nome ?? '').trim().toLowerCase() === (dados.medicoNome ?? '').trim().toLowerCase());
        if (!medico) {
          const m: ChatMessage = { usuario: 'bot', texto: `Médico "${dados.medicoNome}" não encontrado.`, ts: new Date().toISOString() };
          this.pushLocalMessage(m);
          return m;
        }

        // 3) montar payload e criar atividade
        const payload: any = {
          inicio: dados.inicio,
          termino: dados.fim ?? undefined,
          tipoAtividade: dados.tipoAtividade ?? 'Consulta',
          pacienteId: paciente.id,
          medicos: [medico.id]
        };

        await firstValueFrom(this.http.post<any>(`${this.dotnetBase}/atividades-medicas`, payload));

        const okMsg: ChatMessage = { usuario: 'bot', texto: 'Atividade médica criada com sucesso.', ts: new Date().toISOString() };
        this.pushLocalMessage(okMsg);
        return okMsg;
      } catch (errCreate: any) {
        const errMsg: ChatMessage = { usuario: 'bot', texto: `Erro ao criar atividade: ${JSON.stringify(errCreate)}`, ts: new Date().toISOString() };
        this.pushLocalMessage(errMsg);
        return errMsg;
      }
    } catch (err: any) {
      const errMsg: ChatMessage = { usuario: 'bot', texto: `Erro de rede: ${err?.message ?? JSON.stringify(err)}`, ts: new Date().toISOString() };
      this.pushLocalMessage(errMsg);
      return errMsg;
    }
  }
}
