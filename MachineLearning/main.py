# main.py
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, Dict, Any
import re
import json
from datetime import datetime

app = FastAPI(title="Mini LLM - OrganizaMed", version="1.1")
# fazer ajuste e limpeza de código (obs: o código está limpando as entradas, realizar treinamento para
# a ML/IA compreender as entradas sem necessitar de limpeza adicional de código)

# CORS para dev local (Angular)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200", "http://127.0.0.1:4200"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class ComandoInput(BaseModel):
    mensagem: Optional[str] = None
    comando: Optional[str] = None

MONTHS = {
    "janeiro": 1, "fevereiro": 2, "março": 3, "marco": 3, "abril": 4,
    "maio": 5, "junho": 6, "julho": 7, "agosto": 8, "setembro": 9,
    "outubro": 10, "novembro": 11, "dezembro": 12
}

def parse_date_parts(day_s: str, month_s: str, year_s: Optional[str]) -> Optional[str]:
    try:
        day = int(day_s)
        # mês pode ser palavra ou numero
        month = None
        if month_s.isdigit():
            month = int(month_s)
        else:
            ms = month_s.lower()
            month = MONTHS.get(ms)
            if month is None:
                # tenta abreviação de 3 letras (jan, fev, mar, ...)
                ms3 = ms[:3]
                abbr_map = {
                    "jan": 1, "fev": 2, "mar": 3, "abr": 4, "mai": 5, "jun": 6,
                    "jul": 7, "ago": 8, "set": 9, "out": 10, "nov": 11, "dez": 12
                }
                month = abbr_map.get(ms3)
        if month is None:
            return None
        # anos com 2 ou 4 digitos
        if year_s:
            year = int(year_s)
            if year < 100:  # 2- digitos do ano -> assume 2000+
                year += 2000
        else:
            # sem ano = ano atual
            year = datetime.now().year
        return f"{year:04d}-{month:02d}-{day:02d}"
    except Exception:
        return None

def parse_time(t: str) -> Optional[str]:
    # aceita 9, 9h, 09:00, 9:30, 09:30
    t = t.strip().lower().replace('h', ':')
    if re.match(r'^\d{1,2}:\d{2}$', t):
        parts = t.split(':')
        h = int(parts[0])
        m = int(parts[1])
        return f"{h:02d}:{m:02d}"
    if re.match(r'^\d{1,2}$', t):
        h = int(t)
        return f"{h:02d}:00"
    return None

def interpretar_comando(texto: str) -> Optional[Dict[str, Any]]:
    """
    Tenta extrair: tipoAtividade, pacienteNome, medicoNome, inicio (ISO), fim (ISO opcional)
    Retorna dict ou None se não conseguiu.
    """
    if not texto:
        return None

    texto_original = texto.strip()
    texto_lower = texto_original.lower()

    print("interpretar_comando - texto original:", repr(texto_original))

    def clean_person_name(s: str) -> str:
        if not s:
            return s
        s = s.strip()

        # padrões de prefixos a remover (case-insensitive)
        prefixes = [
            r'^consulta\s+para\s+o\s+paciente\s+',
            r'^consulta\s+para\s+o\s+',
            r'^consulta\s+para\s+',
            r'^para\s+o\s+paciente\s+',
            r'^para\s+o\s+',
            r'^para\s+',
            r'^paciente\s+',
            r'^\bcom\b\s+',
            r'^\bo\b\s+',
            r'^\ba\b\s+'
        ]

        for p in prefixes:
            s = re.sub(p, '', s, flags=re.IGNORECASE).strip()

        # remover títulos redundantes no início (ex: "dr.", "dra.", "sr.", "sra.")
        s = re.sub(r'^(dr\.?|dra\.?|sr\.?|sra\.?)\s+', '', s, flags=re.IGNORECASE)

        # colapsar espaços múltiplos e normalizar
        s = re.sub(r'\s+', ' ', s).strip()

        # Title case (mantém acentos)
        return s.title()

    # --- pattern_para_com (executa cedo, usando texto_lower para casar) ---
    pattern_para_com = re.compile(
        r"(?:marcar|marque|agendar|agende|marcar uma|marque uma|agendar uma).*?"
        r"(?:para\s+)?([A-Za-zÀ-ÿ0-9\.\s]{2,60}?)\s+com\s+([A-Za-zÀ-ÿ0-9\.\s]{2,60}?)\s+"
        r"(?:no dia|no|no\s+dia|em|para o dia|para)\s+(\d{1,2})\s*(?:de\s+)?([A-Za-zÀ-ÿ]+|\d{1,2})\s*(?:de\s+)?(\d{2,4})?.*?"
        r"(?:das|de|às|as|a)?\s*(\d{1,2}(?::\d{2})?)\s*(?:às|a|até|ate|-)\s*(\d{1,2}(?::\d{2})?)",
        re.IGNORECASE
    )
    mpc = pattern_para_com.search(texto_lower)
    if mpc:
        print("DEBUG: pattern_para_com casou. grupos:", mpc.groups())
        paciente_raw = mpc.group(1).strip()
        medico_raw = mpc.group(2).strip()
        dia_s = mpc.group(3)
        mes_s = mpc.group(4)
        ano_s = mpc.group(5)
        inicio_s = mpc.group(6)
        fim_s = mpc.group(7)

        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            print("DEBUG: parse_date_parts falhou para:", dia_s, mes_s, ano_s)
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            print("DEBUG: parse_time falhou para inicio:", inicio_s)
            return None

        resultado = {
            "tipoAtividade": "Consulta",
            "pacienteNome": clean_person_name(paciente_raw),
            "medicoNome": clean_person_name(medico_raw),
            "inicio": f"{date_prefix}T{inicio_time}"
        }
        if fim_time:
            resultado["fim"] = f"{date_prefix}T{fim_time}"
        return resultado

    # 1) Tentar padrões com mês por extenso: "12 de novembro de 2025 das 10:30 às 11:30"
    pattern1 = re.compile(
        r"(consulta|cirurgia|retorno|teleconsulta|exame|consulta online|agendar|marcar).*?"
        r"(paciente|entre o paciente)\s+([A-Za-zÀ-ÿ0-9\.\s]+?)\s+(?:e\s+o\s+medico|e\s+o\s+médico|e\s+o\s+dr\.?|e\s+o\s+dra\.?|e\s+o\s+dr|e\s+o\s+drs|e\s+o)\s*([A-Za-zÀ-ÿ0-9\.\s]+?)\s+"
        r"(?:para o dia|no dia|em|para|dia)\s+(\d{1,2})\s*(?:de\s+)?([A-Za-zÀ-ÿ]+|\d{1,2})\s*(?:de\s+)?(\d{2,4})?"
        r".*?(?:das|de|às|as|a)?\s*(\d{1,2}(?::\d{2})?)\s*(?:às|a|até|ate|-)\s*(\d{1,2}(?::\d{2})?)",
        re.IGNORECASE
    )
    m = pattern1.search(texto_lower)
    if m:
        print("DEBUG: pattern1 casou. grupos:", m.groups())
        tipo_raw = m.group(1)
        paciente_raw = m.group(3).strip()
        medico_raw = m.group(4).strip()
        dia_s = m.group(5)
        mes_s = m.group(6)
        ano_s = m.group(7)
        inicio_s = m.group(8)
        fim_s = m.group(9)

        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            print("DEBUG: parse_date_parts falhou no pattern1")
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            print("DEBUG: parse_time falhou no pattern1")
            return None

        resultado = {
            "tipoAtividade": "Consulta" if "consulta" in tipo_raw.lower() else tipo_raw.capitalize(),
            "pacienteNome": clean_person_name(paciente_raw),
            "medicoNome": clean_person_name(medico_raw),
            "inicio": f"{date_prefix}T{inicio_time}"
        }
        if fim_time:
            resultado["fim"] = f"{date_prefix}T{fim_time}"
        return resultado

    # 2) Padrões com formato dd/mm/yyyy ou dd/mm
    pattern2 = re.compile(
        r"(consulta|cirurgia|retorno|teleconsulta|exame).*?"
        r"(?:paciente|entre o paciente)?\s*([A-Za-zÀ-ÿ0-9\.\s]+?)\s+(?:com|e|e o médico|com o médico|com a médica)\s*([A-Za-zÀ-ÿ0-9\.\s]+?)\s+"
        r"(?:no dia|em|para|dia)?\s*(\d{1,2})[\/\-](\d{1,2})(?:[\/\-](\d{2,4}))?.*?(?:às|as|a|das)?\s*(\d{1,2}:\d{2})\s*(?:até|ate|às|a|-)\s*(\d{1,2}:\d{2})",
        re.IGNORECASE
    )
    m2 = pattern2.search(texto_lower)
    if m2:
        print("DEBUG: pattern2 casou. grupos:", m2.groups())
        tipo_raw = m2.group(1)
        paciente_raw = m2.group(2).strip()
        medico_raw = m2.group(3).strip()
        dia_s = m2.group(4)
        mes_s = m2.group(5)
        ano_s = m2.group(6)
        inicio_s = m2.group(7)
        fim_s = m2.group(8)

        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            return None
        resultado = {
            "tipoAtividade": "Consulta" if "consulta" in tipo_raw.lower() else tipo_raw.capitalize(),
            "pacienteNome": clean_person_name(paciente_raw),
            "medicoNome": clean_person_name(medico_raw),
            "inicio": f"{date_prefix}T{inicio_time}"
        }
        if fim_time:
            resultado["fim"] = f"{date_prefix}T{fim_time}"
        return resultado

    # 3) Padrão simples "marque uma consulta ... dia dd/mm/yyyy as hh:mm até hh:mm"
    pattern3 = re.compile(
        r"(consulta|cirurgia|retorno|teleconsulta|exame).*?(?:paciente)?\s*([A-Za-zÀ-ÿ0-9\.\s]+?)\s+(?:com|e|com o médico|com a médica)\s*([A-Za-zÀ-ÿ0-9\.\s]+?).*?(\d{1,2})[\/\-](\d{1,2})[\/\-]?(\d{2,4})?.*?(\d{1,2}:\d{2}).*?(?:até|a)\s*(\d{1,2}:\d{2})",
        re.IGNORECASE
    )
    m3 = pattern3.search(texto_lower)
    if m3:
        print("DEBUG: pattern3 casou. grupos:", m3.groups())
        tipo_raw = m3.group(1)
        paciente_raw = m3.group(2).strip()
        medico_raw = m3.group(3).strip()
        dia_s = m3.group(4)
        mes_s = m3.group(5)
        ano_s = m3.group(6)
        inicio_s = m3.group(7)
        fim_s = m3.group(8)
        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            return None
        resultado = {
            "tipoAtividade": "Consulta" if "consulta" in tipo_raw.lower() else tipo_raw.capitalize(),
            "pacienteNome": clean_person_name(paciente_raw),
            "medicoNome": clean_person_name(medico_raw),
            "inicio": f"{date_prefix}T{inicio_time}"
        }
        if fim_time:
            resultado["fim"] = f"{date_prefix}T{fim_time}"
        return resultado

    # 4) fallback heurístico (mantive seu fallback original)
    paciente_m = re.search(r"paciente\s+([A-Za-zÀ-ÿ0-9\.\s]+?)(?:\s+com|\s+e|,|\.|$)", texto_lower)
    medico_m = re.search(r"(?:m[eé]dico|dr\.|dra\.|médico|médica)\s+([A-Za-zÀ-ÿ0-9\.\s]+?)(?:\s+dia|\s+para|\s+no|\s+às|,|$)", texto_lower)
    time_m = re.search(r"(\d{1,2}:\d{2})", texto_lower)
    date_m = re.search(r"(\d{1,2})[\/\-](\d{1,2})(?:[\/\-](\d{2,4}))?", texto_lower)
    if paciente_m and medico_m and time_m and date_m:
        paciente_raw = paciente_m.group(1).strip()
        medico_raw = medico_m.group(1).strip()
        dia_s = date_m.group(1)
        mes_s = date_m.group(2)
        ano_s = date_m.group(3)
        inicio_s = time_m.group(1)
        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        resultado = {
            "tipoAtividade": "Consulta",
            "pacienteNome": clean_person_name(paciente_raw),
            "medicoNome": clean_person_name(medico_raw),
            "inicio": f"{date_prefix}T{parse_time(inicio_s)}"
        }
        return resultado

    print("DEBUG: nenhum pattern casou para:", repr(texto_original))
    return None


@app.post("/comando")
def processar_comando(body: ComandoInput):
    texto = (body.mensagem or body.comando or "").strip()
    print("=== /comando recebido ===")
    print("raw body:", body)
    print("texto extraido:", repr(texto))
    if not texto:
        print("DEBUG: texto vazio")
        return {"sucesso": False, "erro": "Comando vazio. Envie 'mensagem' ou 'comando' no body."}
    try:
        resultado = interpretar_comando(texto)
        if not resultado:
            print("DEBUG: interpretar_comando retornou None para:", repr(texto))
            # Retorna debug no response para o front mostrar
            return {"sucesso": False, "erro": "Não foi possível entender o comando.", "debug": {"texto_recebido": texto}}
        print("DEBUG: interpretar_comando sucesso ->", resultado)
        return {"sucesso": True, "dados": resultado}
    except Exception as e:
        import traceback; traceback.print_exc()
        return {"sucesso": False, "erro": str(e), "debug": {"texto_recebido": texto}}

@app.get("/")
def root():
    return {"status": "ok", "mensagem": "API Mini LLM rodando."}
