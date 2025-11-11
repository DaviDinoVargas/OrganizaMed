from fastapi import FastAPI
from pydantic import BaseModel
import requests
from infer import gerar_json_do_modelo

app = FastAPI()
C_SHARP_API_BASE = "https://localhost:7043/api/atividades-medicas"

class ComandoInput(BaseModel):
    mensagem: str

@app.post("/comando")
def processar_comando(cmd: ComandoInput):
    json_data = gerar_json_do_modelo(cmd.mensagem)

    if "erro" in json_data:
        return {"sucesso": False, "mensagem": json_data["erro"]}

    # Validar paciente/médico via API C#
    pacientes = requests.get(f"{C_SHARP_API_BASE.replace('/atividades-medicas','')}/pacientes").json().get('registros', [])
    paciente = next((p for p in pacientes if p['nome'].lower() == json_data['pacienteNome'].lower()), None)
    if not paciente:
        return {"sucesso": False, "mensagem": f"Paciente {json_data['pacienteNome']} não encontrado"}

    medicos = requests.get(f"{C_SHARP_API_BASE.replace('/atividades-medicas','')}/medicos").json().get('registros', [])
    medico = next((m for m in medicos if m['nome'].lower() == json_data['medicoNome'].lower()), None)
    if not medico:
        return {"sucesso": False, "mensagem": f"Médico {json_data['medicoNome']} não encontrado"}

    payload = {
        "tipoAtividade": json_data['tipoAtividade'],
        "inicio": json_data['inicio'],
        "pacienteId": paciente['id'],
        "medicos": [medico['id']]
    }

    response = requests.post(C_SHARP_API_BASE, json=payload)
    return {"sucesso": response.ok, "mensagem": "Atividade criada" if response.ok else response.text}
