import json
import torch
from torch.utils.data import DataLoader, Dataset
import torch.nn as nn
from model.model import MiniLLM
from model.tokenizer import CharTokenizer

# Carregar dataset
with open("dataset/comandos.json") as f:
    data = json.load(f)

tokenizer = CharTokenizer()

class ComandoDataset(Dataset):
    def __init__(self, data, tokenizer, seq_len=128):
        self.data = data
        self.tokenizer = tokenizer
        self.seq_len = seq_len

    def __len__(self):
        return len(self.data)

    def __getitem__(self, idx):
        comando = self.data[idx]["comando"]
        resposta = json.dumps(self.data[idx]["json"])
        x = tokenizer.encode(comando)
        y = tokenizer.encode(resposta)
        # Pad
        x = x + [0]*(self.seq_len - len(x)) if len(x)<self.seq_len else x[:self.seq_len]
        y = y + [0]*(self.seq_len - len(y)) if len(y)<self.seq_len else y[:self.seq_len]
        return torch.tensor(x), torch.tensor(y)

dataset = ComandoDataset(data, tokenizer)
dataloader = DataLoader(dataset, batch_size=2, shuffle=True)

# Modelo
model = MiniLLM(tokenizer.vocab_size)
criterion = nn.CrossEntropyLoss()
optimizer = torch.optim.Adam(model.parameters(), lr=1e-3)

# Treinamento
epochs = 50
for epoch in range(epochs):
    total_loss = 0
    for x, y in dataloader:
        optimizer.zero_grad()
        out = model(x)
        out = out.view(-1, tokenizer.vocab_size)
        y = y.view(-1)
        loss = criterion(out, y)
        loss.backward()
        optimizer.step()
        total_loss += loss.item()
    print(f"Epoch {epoch+1}, Loss: {total_loss/len(dataloader)}")

# Salvar modelo
torch.save(model.state_dict(), "model/mini_llm.pth")
