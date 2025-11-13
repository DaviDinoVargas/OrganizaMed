import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MedicosService } from '../../components/medicos/medicos.service';
import { AtividadesMedicasService } from '../../components/atividades/atividades-medicas.service';

interface Top10Item {
  medicoId?: string;
  medico: string;
  crm?: string;
  totalDeHorasTrabalhadas?: number;
}

interface DescansoWindow {
  inicio: string; // ISO
  fim: string;    // ISO
}

interface DescansosPorMedico {
  medicoId?: string;
  medicoNome: string;
  crm?: string;
  descansos: DescansoWindow[];
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatListModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  comando = '';
  mensagens: any[] = [];

  top10: Top10Item[] = [];
  loadingTop10 = false;

  descansosPorMedico: DescansosPorMedico[] = [];
  loadingDescansos = false;

  private dotnetBase = 'https://localhost:7043/api';

  constructor(
    private medSvc: MedicosService,
    private atvSvc: AtividadesMedicasService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.refreshTop10();
    this.refreshDescansos();
  }

  // --- HELPERS --------------------------------------------------------------
  private extractArrayFromEnvelope(raw: any): any[] {
    if (!raw) return [];
    if (Array.isArray(raw)) return raw;
    if (raw.registros && Array.isArray(raw.registros)) return raw.registros;
    if (raw.dados && Array.isArray(raw.dados)) return raw.dados;
    if (raw.dados?.registros && Array.isArray(raw.dados.registros)) return raw.dados.registros;
    // fallback: maybe paged envelope with .dados.registros
    return [];
  }

  /**
   * tenta extrair CRM / nome / id de formatos variados:
   * - { medicoId, medico, totalDeHorasTrabalhadas }
   * - { id, nome, crm, horas }
   * - { medico: { id, nome, crm }, totalDeHorasTrabalhadas }
   */
  private normalizeTop10Item(x: any): Top10Item {
    const medicoObj = x.medico && typeof x.medico === 'object' ? x.medico : null;
    const medicoNome =
      x.medico && typeof x.medico === 'string' ? x.medico :
      medicoObj?.nome ?? x.nome ?? x.medicoNome ?? x.medico ?? '—';

    const medicoId = x.medicoId ?? x.id ?? medicoObj?.id;
    const crm = medicoObj?.crm ?? x.crm ?? x.crmMedico ?? '—';

    // heurística para horas — servidor pode devolver diferentes campos:
    let horas = 0;
    if (typeof x.totalDeHorasTrabalhadas === 'number') horas = x.totalDeHorasTrabalhadas;
    else if (typeof x.horas === 'number') horas = x.horas;
    else if (typeof x.horasTrabalhadas === 'number') horas = x.horasTrabalhadas;
    else if (typeof x.totalEmMinutos === 'number') horas = x.totalEmMinutos / 60;
    else if (typeof x.totalEmSegundos === 'number') horas = x.totalEmSegundos / 3600;

    // se for um número alto (>24) e houver suspeita de minutos, tentamos detectar:
    if (horas > 24 && Number.isInteger(horas)) {
      // se > 24 e for inteiro, é provável que o servidor tenha retornado minutos sem nome padrão
      // Dividimos por 60 como fallback
      horas = horas / 60;
    }

    return {
      medicoId,
      medico: medicoNome,
      crm,
      totalDeHorasTrabalhadas: Math.round((horas + Number.EPSILON) * 100) / 100
    };
  }

  // --- TOP10 ----------------------------------------------------------------
  refreshTop10() {
    this.loadingTop10 = true;
    const inicio = new Date();
    const termino = new Date();
    termino.setDate(inicio.getDate() + 30);

    const inicioIso = inicio.toISOString();
    const terminoIso = termino.toISOString();

    this.medSvc.top10(inicioIso, terminoIso).subscribe({
      next: (r: any) => {
        try {
          const arr = this.extractArrayFromEnvelope(r);
          this.top10 = (arr ?? []).map((x: any) => this.normalizeTop10Item(x));
        } catch (e) {
          console.error('Erro ao normalizar top10', e);
          this.top10 = [];
        } finally {
          this.loadingTop10 = false;
        }
      },
      error: (err) => {
        console.error('Erro top10', err);
        this.top10 = [];
        this.loadingTop10 = false;
      }
    });
  }

  // --- DESCANSOS ------------------------------------------------------------
  refreshDescansos() {
    this.loadingDescansos = true;
    const now = new Date();
    const end = new Date();
    end.setDate(now.getDate() + 30);

    // pegar todas as atividades (backend não tem filtro por data, então buscamos e filtramos cliente-side)
    this.atvSvc.listar().subscribe({
      next: (raw: any) => {
        try {
          const registros = this.extractArrayFromEnvelope(raw);
          const atividades = (registros ?? []).map((a: any) => ({
            id: a.id,
            inicio: a.inicio ? new Date(a.inicio) : null,
            termino: a.termino ? new Date(a.termino) : (a.inicio ? new Date(a.inicio) : null),
            tipo: (a.tipoAtividade ?? a.tipo ?? 'Consulta'),
            paciente: a.paciente,
            medicos: a.medicos ?? []
          }));

          // map médicoId -> lista de janelas de descanso
          const map = new Map<string, DescansoWindow[]>();

          atividades.forEach((a: { termino: Date | null; inicio: Date | null; tipo: string; medicos: any[]; }) => {
            if (!a.inicio && !a.termino) return;

            const termino = a.termino ?? a.inicio!;
            const recoveryMs = a.tipo === 'Cirurgia' ? (4 * 60 * 60 * 1000) : (10 * 60 * 1000); // 4h ou 10min

            // cálculo de janela de descanso real
            const inicioDesc = termino;
            const fimDesc = new Date(termino.getTime() + recoveryMs);

            // Relevância:
            // - atividades que terminam entre now..end
            // - ou atividades que terminaram antes de now, mas cuja janela de descanso (fimDesc) ainda é > now (ou seja, descanso em curso)
            // - ou atividades que iniciam entre now..end (eventos futuros)
            const terminoBetween = termino >= now && termino <= end;
            const inicioBetween = (a.inicio ?? termino) >= now && (a.inicio ?? termino) <= end;
            const descansoAindaVigente = fimDesc.getTime() > now.getTime() && termino < now; // terminou no passado, mas resta descanso

            if (!(terminoBetween || inicioBetween || descansoAindaVigente)) {
              return; // não relevante nos próximos 30 dias nem descanso em curso
            }

            (a.medicos ?? []).forEach((m: any) => {
              const id = m?.id ?? m;
              if (!id) return;
              const arr = map.get(id) ?? [];
              arr.push({ inicio: inicioDesc.toISOString(), fim: fimDesc.toISOString() });
              map.set(id, arr);
            });
          });

          // obter lista de médicos para enriquecer (nome, crm)
          this.medSvc.listar().subscribe({
            next: (medRaw: any) => {
              try {
                const medArr = this.extractArrayFromEnvelope(medRaw);
                const medLookup = new Map<string, any>();
                (medArr ?? []).forEach((m: any) => medLookup.set(m.id, m));

                const result: DescansosPorMedico[] = [];
                for (const [medId, janelas] of map.entries()) {
                  const m = medLookup.get(medId) ?? { id: medId, nome: '—', crm: '—' };
                  // sort by inicio
                  janelas.sort((a:any,b:any) => new Date(a.inicio).getTime() - new Date(b.inicio).getTime());
                  result.push({
                    medicoId: medId,
                    medicoNome: m.nome ?? m.medico ?? m.medicoNome ?? '—',
                    crm: m.crm ?? '—',
                    descansos: janelas
                  });
                }

                // order by name
                result.sort((a,b) => (a.medicoNome || '').localeCompare(b.medicoNome || ''));
                this.descansosPorMedico = result;
              } catch (e) {
                console.error('Erro ao montar descansosPorMedico', e);
                this.descansosPorMedico = [];
              } finally {
                this.loadingDescansos = false;
              }
            },
            error: (errMed) => {
              console.error('Erro ao buscar médicos para mapear descansos', errMed);
              this.descansosPorMedico = [];
              this.loadingDescansos = false;
            }
          });
        } catch (e) {
          console.error('Erro ao processar atividades', e);
          this.descansosPorMedico = [];
          this.loadingDescansos = false;
        }
      },
      error: (err) => {
        console.error('Erro ao buscar atividades', err);
        this.descansosPorMedico = [];
        this.loadingDescansos = false;
      }
    });
  }
}
