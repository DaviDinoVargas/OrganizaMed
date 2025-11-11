import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

interface Mensagem {
  usuario: 'user' | 'bot';
  texto: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, HttpClientModule, MatCardModule, MatButtonModule],
  templateUrl: './home.component.html',
})
export class HomeComponent {
  comando: string = '';
  mensagens: Mensagem[] = [];

  // ajuste as URLs conforme sua configuração local
  private pythonUrl = 'http://127.0.0.1:8000/comando';
  private dotnetBase = 'https://localhost:7043/api'; // base .NET (ajuste porta se necessário)

  constructor(private http: HttpClient) {}

  enviarComando(): void {
    const texto = this.comando?.trim();
    if (!texto) return;

    // adiciona ao histórico
    this.mensagens.push({ usuario: 'user', texto });

    // 1) chama o interpretador Python
    this.http.post<any>(this.pythonUrl, { mensagem: texto }).subscribe({
      next: (pyRes) => {
        // pyRes esperado: { sucesso: true, dados: { tipoAtividade, pacienteNome, medicoNome, inicio, fim? } }
        if (!pyRes?.sucesso || !pyRes?.dados) {
          const err = pyRes?.erro ?? 'Interpretador não retornou dados';
          this.mensagens.push({ usuario: 'bot', texto: `Erro: ${err}` });
          this.comando = '';
          return;
        }

        const dados = pyRes.dados;
        this.mensagens.push({ usuario: 'bot', texto: JSON.stringify(dados) });

        // 2) buscar paciente (por nome) no backend .NET
        // Tenta buscar via GET /api/pacientes (ajuste endpoint caso possua filtro)
        this.http.get<any>(`${this.dotnetBase}/pacientes`).subscribe({
          next: (pacResp) => {
            const pacientesList = (pacResp?.dados?.registros ?? pacResp?.registros ?? pacResp) as any[] || [];
            const paciente = pacientesList.find(p => (p.nome ?? '').trim().toLowerCase() === (dados.pacienteNome ?? '').trim().toLowerCase());
            if (!paciente) {
              this.mensagens.push({ usuario: 'bot', texto: `Paciente "${dados.pacienteNome}" não encontrado.` });
              this.comando = '';
              return;
            }

            // 3) buscar medico
            this.http.get<any>(`${this.dotnetBase}/medicos`).subscribe({
              next: (medResp) => {
                const medList = (medResp?.dados?.registros ?? medResp?.registros ?? medResp) as any[] || [];
                const medico = medList.find(m => (m.nome ?? '').trim().toLowerCase() === (dados.medicoNome ?? '').trim().toLowerCase());
                if (!medico) {
                  this.mensagens.push({ usuario: 'bot', texto: `Médico "${dados.medicoNome}" não encontrado.` });
                  this.comando = '';
                  return;
                }

                // 4) montar payload compatível com seu DTO
                const payload: any = {
                  inicio: dados.inicio,
                  termino: dados.fim ?? undefined,
                  tipoAtividade: dados.tipoAtividade ?? 'Consulta',
                  pacienteId: paciente.id,
                  medicos: [medico.id]
                };

                // 5) criar atividade médica
                this.http.post<any>(`${this.dotnetBase}/atividades-medicas`, payload).subscribe({
                  next: (createRes) => {
                    this.mensagens.push({ usuario: 'bot', texto: 'Atividade médica criada com sucesso.' });
                    // opcional: navegar ou atualizar lista
                  },
                  error: (err: any) => {
                    console.error('Erro criando atividade:', err);
                    this.mensagens.push({ usuario: 'bot', texto: `Erro ao criar atividade: ${err?.message ?? JSON.stringify(err)}` });
                  }
                });
              },
              error: (err: any) => {
                console.error('Erro ao buscar médicos:', err);
                this.mensagens.push({ usuario: 'bot', texto: 'Erro ao buscar médicos.' });
              }
            });

          },
          error: (err: any) => {
            console.error('Erro ao buscar pacientes:', err);
            this.mensagens.push({ usuario: 'bot', texto: 'Erro ao buscar pacientes.' });
          }
        });
      },
      error: (err: any) => {
        console.error('Erro ao chamar interpretador Python:', err);
        // mostra erro claro na UI
        if (err?.status === 0) {
          this.mensagens.push({ usuario: 'bot', texto: 'Erro: não foi possível conectar ao interpretador (verifique se o Python/uvicorn está rodando).' });
        } else if (err?.status === 422) {
          this.mensagens.push({ usuario: 'bot', texto: `Requisição inválida: ${JSON.stringify(err.error)}` });
        } else {
          this.mensagens.push({ usuario: 'bot', texto: `Erro desconhecido: ${err?.message ?? JSON.stringify(err)}` });
        }
      }
    });

    this.comando = '';
  }
}
