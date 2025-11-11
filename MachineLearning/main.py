# main.py
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, Dict, Any
import re
import json
from datetime import datetime

app = FastAPI(title="Mini LLM - OrganizaMed", version="1.1")

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
            month = MONTHS.get(month_s.lower())
            if month is None:
                # quando utilizado apenas as 3 primeiras letras
                month = MONTHS.get(month_s[:3].lower())
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

    original = texto
    texto = texto.lower()

    # 1) Tentar padrões com mês por extenso: "12 de novembro de 2025 das 10:30 às 11:30"
    pattern1 = re.compile(
        r"(consulta|cirurgia|retorno|teleconsulta|exame|consulta online|agendar|marcar).*?"
        r"(paciente|entre o paciente)\s+([A-Za-zÀ-ÿ0-9\.\s]+?)\s+(?:e\s+o\s+medico|e\s+o\s+médico|e\s+o\s+dr\.?|e\s+o\s+dra\.?|e\s+o\s+dr|e\s+o\s+drs|e\s+o)\s*([A-Za-zÀ-ÿ0-9\.\s]+?)\s+"
        r"(?:para o dia|no dia|em|para|dia)\s+(\d{1,2})\s*(?:de\s+)?([A-Za-zÀ-ÿ]+|\d{1,2})\s*(?:de\s+)?(\d{2,4})?"
        r".*?(?:das|de|às|as|a)?\s*(\d{1,2}(?::\d{2})?)\s*(?:às|a|até|ate|-)\s*(\d{1,2}(?::\d{2})?)",
        re.IGNORECASE
    )

    m = pattern1.search(texto)
    if m:
        # groups: tipo, 'paciente' literal, pacienteNome, medicoNome, dia, mes, ano, inicio, fim
        tipo_raw = m.group(1)
        paciente = m.group(3).strip()
        medico = m.group(4).strip()
        dia_s = m.group(5)
        mes_s = m.group(6)
        ano_s = m.group(7)
        inicio_s = m.group(8)
        fim_s = m.group(9)

        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            return None

        resultado = {
            "tipoAtividade": "Consulta" if "consulta" in tipo_raw.lower() else tipo_raw.capitalize(),
            "pacienteNome": paciente.title(),
            "medicoNome": medico.title(),
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
    m2 = pattern2.search(texto)
    if m2:
        tipo_raw = m2.group(1)
        paciente = m2.group(2).strip()
        medico = m2.group(3).strip()
        dia_s = m2.group(4)
        mes_s = m2.group(5)
        ano_s = m2.group(6)
        inicio_s = m2.group(7)
        fim_s = m2.group(8)

        # se ano ausente, assume ano atual
        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        inicio_time = parse_time(inicio_s)
        fim_time = parse_time(fim_s)
        if not inicio_time:
            return None
        resultado = {
            "tipoAtividade": "Consulta" if "consulta" in tipo_raw.lower() else tipo_raw.capitalize(),
            "pacienteNome": paciente.title(),
            "medicoNome": medico.title(),
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
    m3 = pattern3.search(texto)
    if m3:
        tipo_raw = m3.group(1)
        paciente = m3.group(2).strip()
        medico = m3.group(3).strip()
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
            "pacienteNome": paciente.title(),
            "medicoNome": medico.title(),
            "inicio": f"{date_prefix}T{inicio_time}"
        }
        if fim_time:
            resultado["fim"] = f"{date_prefix}T{fim_time}"
        return resultado

    # 4) fallback: tentar extrair nomes e qualquer hora presente (menos confiável)
    # extrai pacientes e médicos por heurística simples: "paciente X" e "médico Y" / "dr. Y" / "dra. Y"
    paciente_m = re.search(r"paciente\s+([A-Za-zÀ-ÿ0-9\.\s]+?)(?:\s+com|\s+e|,|\.|$)", texto)
    medico_m = re.search(r"(?:m[eé]dico|dr\.|dra\.|médico|médica)\s+([A-Za-zÀ-ÿ0-9\.\s]+?)(?:\s+dia|\s+para|\s+no|\s+às|,|$)", texto)
    time_m = re.search(r"(\d{1,2}:\d{2})", texto)
    date_m = re.search(r"(\d{1,2})[\/\-](\d{1,2})(?:[\/\-](\d{2,4}))?", texto)
    if paciente_m and medico_m and time_m and date_m:
        paciente = paciente_m.group(1).strip()
        medico = medico_m.group(1).strip()
        dia_s = date_m.group(1)
        mes_s = date_m.group(2)
        ano_s = date_m.group(3)
        inicio_s = time_m.group(1)
        date_prefix = parse_date_parts(dia_s, mes_s, ano_s)
        if not date_prefix:
            return None
        resultado = {
            "tipoAtividade": "Consulta",
            "pacienteNome": paciente.title(),
            "medicoNome": medico.title(),
            "inicio": f"{date_prefix}T{parse_time(inicio_s)}"
        }
        return resultado

    # se nada bateu, devolve None
    return None

@app.post("/comando")
def processar_comando(body: ComandoInput):
    texto = (body.mensagem or body.comando or "").strip()
    if not texto:
        return {"sucesso": False, "erro": "Comando vazio. Envie 'mensagem' ou 'comando' no body."}
    try:
        resultado = interpretar_comando(texto)
        if not resultado:
            # devolve debug para ajudar no front a exibir melhor erro
            return {"sucesso": False, "erro": "Não foi possível entender o comando.", "debug": {"texto_recebido": texto}}
        return {"sucesso": True, "dados": resultado}
    except Exception as e:
        import traceback; traceback.print_exc()
        return {"sucesso": False, "erro": str(e), "debug": {"texto_recebido": texto}}

@app.get("/")
def root():
    return {"status": "ok", "mensagem": "API Mini LLM rodando."}
