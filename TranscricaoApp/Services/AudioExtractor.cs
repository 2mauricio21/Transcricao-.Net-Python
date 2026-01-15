using System.Diagnostics;
using System.IO;

namespace TranscricaoApp.Services;

public class AudioExtractor
{
    private readonly string _ffmpegPath;

    public AudioExtractor()
    {
        // Tenta encontrar FFmpeg no PATH ou em locais comuns
        _ffmpegPath = FindFFmpeg();
    }

    public async Task<string> ExtractAudioAsync(string videoPath, string outputDirectory, IProgress<int>? progress = null)
    {
        if (string.IsNullOrEmpty(_ffmpegPath))
        {
            throw new FileNotFoundException("FFmpeg não encontrado. Por favor, instale o FFmpeg e adicione-o ao PATH do sistema.");
        }

        // Usa um nome simples para evitar problemas com caracteres especiais ou caminhos longos
        var outputPath = Path.Combine(outputDirectory, "audio.wav");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var arguments = $"-i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"";

        var processInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        
        process.Start();
        
        // Lê a saída de erro para estimar progresso
        var errorOutput = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Erro ao extrair áudio: {errorOutput}");
        }

        // Aguarda o arquivo ser completamente escrito
        await WaitForFileReady(outputPath);

        progress?.Report(100);
        return outputPath;
    }

    private async Task WaitForFileReady(string filePath, int maxWaitSeconds = 30)
    {
        var startTime = DateTime.Now;
        long lastSize = 0;
        int stableCount = 0;
        const int requiredStableChecks = 3; // Arquivo deve estar estável por 3 verificações

        while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var currentSize = fileInfo.Length;

                    // Se o tamanho não mudou, incrementa contador de estabilidade
                    if (currentSize == lastSize && currentSize > 0)
                    {
                        stableCount++;
                        if (stableCount >= requiredStableChecks)
                        {
                            // Arquivo está estável, verifica se não está sendo usado
                            try
                            {
                                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    // Se conseguiu abrir, o arquivo está pronto
                                    return;
                                }
                            }
                            catch
                            {
                                // Arquivo ainda está sendo usado, continua aguardando
                            }
                        }
                    }
                    else
                    {
                        stableCount = 0; // Reset contador se tamanho mudou
                        lastSize = currentSize;
                    }
                }
                catch
                {
                    // Erro ao acessar arquivo, continua aguardando
                }
            }

            await Task.Delay(500); // Verifica a cada 500ms
        }

        // Se chegou aqui, verifica se o arquivo existe mesmo após o timeout
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Arquivo de áudio não foi criado após {maxWaitSeconds} segundos: {filePath}");
        }

        // Verifica tamanho mínimo
        var finalInfo = new FileInfo(filePath);
        if (finalInfo.Length == 0)
        {
            throw new Exception($"Arquivo de áudio está vazio: {filePath}");
        }
    }

    private string FindFFmpeg()
    {
        // Tenta encontrar no PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var paths = pathEnv.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            var ffmpegPath = Path.Combine(path, "ffmpeg.exe");
            if (File.Exists(ffmpegPath))
            {
                return ffmpegPath;
            }
        }

        // Tenta locais comuns no Windows
        var commonPaths = new[]
        {
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe"
        };

        foreach (var commonPath in commonPaths)
        {
            if (File.Exists(commonPath))
            {
                return commonPath;
            }
        }

        return string.Empty;
    }
}
