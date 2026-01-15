# Transcrição de Vídeos com IA

Aplicação Windows Forms em .NET para transcrever vídeos do YouTube ou arquivos MP4 locais usando Whisper (IA gratuita da OpenAI).

## Funcionalidades

- 📥 Download automático de vídeos do YouTube
- 🎬 Transcrição de arquivos MP4 locais
- 🤖 Transcrição com Whisper (IA gratuita e open-source)
- 📊 Barra de progresso durante o processamento
- 💾 Exportação em formato TXT (texto puro)
- 📝 Exportação em formato SRT (legendas com timestamps)
- 🇧🇷 Suporte para português (e outros idiomas)

## Requisitos do Sistema

### Software Necessário

1. **.NET 8.0 SDK ou superior**
   - Download: https://dotnet.microsoft.com/download

2. **Python 3.8 a 3.11 (recomendado: Python 3.11)**
   - ⚠️ **IMPORTANTE**: Python 3.12+ pode ter problemas de compatibilidade
   - ⚠️ **Python 3.13.1 tem um bug conhecido com pip** - use Python 3.11 se possível
   - Download Python 3.11: https://www.python.org/downloads/release/python-31111/
   - Durante a instalação, marque a opção "Add Python to PATH"

3. **Whisper (biblioteca Python)**
   - Após instalar o Python, execute no terminal:
   ```bash
   pip install openai-whisper
   ```
   - Isso também instalará o FFmpeg automaticamente (via dependências)

4. **FFmpeg (Opcional, mas recomendado)**
   - O Whisper geralmente instala o FFmpeg automaticamente
   - Se necessário, download manual: https://ffmpeg.org/download.html
   - Adicione o FFmpeg ao PATH do sistema

## Instalação

### 1. Clone ou baixe o projeto

```bash
git clone <url-do-repositorio>
cd transcição
```

### 2. Instale as dependências Python

```bash
pip install openai-whisper
```

### 3. Compile o projeto .NET

```bash
cd TranscricaoApp
dotnet restore
dotnet build
```

### 4. Execute a aplicação

```bash
dotnet run
```

Ou compile em modo Release e execute o executável:

```bash
dotnet publish -c Release
```

O executável estará em `bin/Release/net8.0-windows/publish/`

## Como Usar

### Transcrever vídeo do YouTube

1. Abra a aplicação
2. Cole a URL do vídeo do YouTube no campo "URL do YouTube"
3. Clique em "Iniciar Transcrição"
4. Aguarde o download e a transcrição (pode levar alguns minutos)
5. A transcrição aparecerá na área de texto
6. Clique em "Salvar como TXT" ou "Salvar como SRT" para exportar

### Transcrever arquivo MP4 local

1. Abra a aplicação
2. Clique em "Selecionar Arquivo MP4"
3. Escolha o arquivo de vídeo
4. Clique em "Iniciar Transcrição"
5. Aguarde a transcrição
6. Exporte o resultado conforme necessário

## Modelos Whisper

A aplicação usa o modelo **"base"** por padrão, que oferece um bom equilíbrio entre velocidade e precisão.

Outros modelos disponíveis (edite o código em `MainForm.cs` se desejar alterar):
- `tiny` - Mais rápido, menos preciso
- `base` - Equilíbrio (padrão)
- `small` - Mais preciso, mais lento
- `medium` - Muito preciso, lento
- `large` - Máxima precisão, muito lento

## Estrutura do Projeto

```
Transcrição/
├── TranscricaoApp/
│   ├── MainForm.cs              # Interface principal
│   ├── MainForm.Designer.cs     # Design da interface
│   ├── Program.cs               # Ponto de entrada
│   ├── Services/
│   │   ├── VideoService.cs      # Download do YouTube
│   │   ├── AudioExtractor.cs    # Extração de áudio
│   │   └── TranscriptionService.cs # Integração com Whisper
│   └── Models/
│       └── TranscriptionResult.cs # Modelo de dados
├── WhisperScript/
│   └── transcribe.py            # Script Python para Whisper
└── README.md
```

## Solução de Problemas

### Erro: "ModuleNotFoundError: No module named 'html.entities'" (Python 3.13+)
**Este é um bug conhecido do Python 3.13.1 com pip.**

**Solução recomendada:**
- Instale Python 3.11 (compatível e estável): https://www.python.org/downloads/release/python-31111/
- Python 3.11 é totalmente compatível com Whisper e não tem esse bug

**Alternativas:**
- Aguarde uma atualização do Python 3.13 que corrija o bug
- Ou use um gerenciador alternativo como `uv` ou `conda`

### Erro: "Python não encontrado"
- Verifique se o Python está instalado: `python --version`
- Certifique-se de que o Python foi adicionado ao PATH durante a instalação
- Reinicie o terminal/aplicação após instalar o Python

### Erro: "Whisper não encontrado"
- Execute: `pip install openai-whisper`
- Se usar Python 3.11 ou anterior: `pip install --upgrade pip` (para atualizar pip)
- Se o pip estiver quebrado, reinstale o Python ou use Python 3.11

### Erro: "FFmpeg não encontrado"
- O Whisper geralmente instala o FFmpeg automaticamente
- Se necessário, instale manualmente: https://ffmpeg.org/download.html
- Adicione o FFmpeg ao PATH do sistema

### Erro ao baixar vídeo do YouTube
- Verifique sua conexão com a internet
- Certifique-se de que a URL do YouTube está correta
- Alguns vídeos podem estar restritos ou privados

### Transcrição muito lenta
- Use um modelo menor (tiny ou base)
- Vídeos longos levam mais tempo para processar
- O primeiro uso pode ser mais lento (download do modelo)

### Transcrição com baixa qualidade
- Use um modelo maior (small, medium ou large)
- Verifique a qualidade do áudio do vídeo original
- Vídeos com muito ruído de fundo podem ter resultados piores

## Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **Windows Forms** - Interface gráfica
- **YoutubeExplode** - Download de vídeos do YouTube
- **Whisper (OpenAI)** - Modelo de transcrição de IA
- **Python** - Execução do Whisper
- **FFmpeg** - Processamento de áudio/vídeo
- **Newtonsoft.Json** - Processamento de JSON

## Licença

Este projeto é fornecido "como está", sem garantias. O Whisper é open-source e gratuito para uso.

## Contribuições

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## Notas

- A primeira execução pode demorar mais devido ao download do modelo Whisper
- Vídeos muito longos podem levar bastante tempo para transcrever
- A precisão da transcrição depende da qualidade do áudio e do modelo escolhido
- O Whisper funciona offline após o download inicial do modelo
