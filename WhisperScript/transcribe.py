#!/usr/bin/env python3
"""
Script para transcrever áudio/vídeo usando Whisper
Retorna JSON com texto e timestamps
"""

import sys
import json
import whisper
import os

def transcribe_file(file_path, model_name="base", language="pt"):
    """
    Transcreve um arquivo de áudio ou vídeo usando Whisper
    
    Args:
        file_path: Caminho para o arquivo de áudio/vídeo
        model_name: Nome do modelo Whisper (tiny, base, small, medium, large)
        language: Código do idioma (pt para português)
    
    Returns:
        Dicionário com texto completo e segmentos com timestamps
    """
    try:
        # Carrega o modelo
        print(f"Carregando modelo Whisper: {model_name}...", file=sys.stderr)
        model = whisper.load_model(model_name)
        
        # Transcreve o arquivo
        print(f"Transcrevendo arquivo: {file_path}...", file=sys.stderr)
        result = model.transcribe(
            file_path,
            language=language,
            verbose=False,
            task="transcribe"
        )
        
        # Prepara o resultado
        output = {
            "text": result["text"].strip(),
            "language": result["language"],
            "segments": []
        }
        
        # Processa os segmentos
        for i, segment in enumerate(result.get("segments", [])):
            output["segments"].append({
                "id": i + 1,
                "start": round(segment["start"], 2),
                "end": round(segment["end"], 2),
                "text": segment["text"].strip()
            })
        
        # Retorna JSON
        print(json.dumps(output, ensure_ascii=False, indent=2))
        return output
        
    except Exception as e:
        error_output = {
            "error": str(e),
            "text": "",
            "language": "",
            "segments": []
        }
        print(json.dumps(error_output, ensure_ascii=False), file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Uso: python transcribe.py <arquivo> [modelo] [idioma]", file=sys.stderr)
        print("Exemplo: python transcribe.py video.mp4 base pt", file=sys.stderr)
        sys.exit(1)
    
    file_path = sys.argv[1]
    model_name = sys.argv[2] if len(sys.argv) > 2 else "base"
    language = sys.argv[3] if len(sys.argv) > 3 else "pt"
    
    if not os.path.exists(file_path):
        error_output = {
            "error": f"Arquivo não encontrado: {file_path}",
            "text": "",
            "language": "",
            "segments": []
        }
        print(json.dumps(error_output, ensure_ascii=False), file=sys.stderr)
        sys.exit(1)
    
    transcribe_file(file_path, model_name, language)
