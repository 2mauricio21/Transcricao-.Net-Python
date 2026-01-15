using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using TranscricaoApp.Models;

namespace TranscricaoApp.Services;

public class TranscriptionService
{
    private readonly string _pythonPath;
    private readonly string _workDir;

    public TranscriptionService()
    {
        _pythonPath = FindPython();
        
        // Usa um diretório de trabalho fixo com caminho simples (sem caracteres especiais)
        _workDir = @"C:\TranscricaoApp";
        if (!Directory.Exists(_workDir))
        {
            Directory.CreateDirectory(_workDir);
        }
    }

    public async Task<TranscriptionResult> TranscribeAsync(
        string videoOrAudioPath, 
        string modelName = "base",
        string language = "pt",
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_pythonPath))
        {
            throw new FileNotFoundException("Python não encontrado. Por favor, instale o Python 3.8 ou superior.");
        }

        if (!File.Exists(videoOrAudioPath))
        {
            throw new FileNotFoundException($"Arquivo não encontrado: {videoOrAudioPath}");
        }

        // Limpa arquivos antigos do diretório de trabalho
        CleanWorkDirectory();

        // Copia o arquivo para o diretório de trabalho com nome simples
        // Isso resolve problemas de encoding com caracteres especiais no caminho
        var extension = Path.GetExtension(videoOrAudioPath).ToLower();
        var tempFileName = $"video{extension}";
        var tempFilePath = Path.Combine(_workDir, tempFileName);

        try
        {
            progress?.Report(5);
            
            // Copia o arquivo
            File.Copy(videoOrAudioPath, tempFilePath, true);
            
            // Aguarda a cópia ser concluída
            await Task.Delay(500);
            
            // Verifica se a cópia foi bem-sucedida
            if (!File.Exists(tempFilePath))
            {
                throw new Exception("Falha ao copiar arquivo para diretório de trabalho.");
            }

            var originalSize = new FileInfo(videoOrAudioPath).Length;
            var copiedSize = new FileInfo(tempFilePath).Length;
            
            if (copiedSize != originalSize)
            {
                throw new Exception($"Arquivo copiado está incompleto. Original: {originalSize} bytes, Copiado: {copiedSize} bytes");
            }

            progress?.Report(10);

            // Encontra o FFmpeg e adiciona ao PATH do processo
            var ffmpegPath = FindFFmpeg();
            var processPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            
            if (!string.IsNullOrEmpty(ffmpegPath))
            {
                var ffmpegDir = Path.GetDirectoryName(ffmpegPath);
                if (!string.IsNullOrEmpty(ffmpegDir) && !processPath.Contains(ffmpegDir))
                {
                    processPath = $"{ffmpegDir};{processPath}";
                }
            }

            // Executa whisper
            var outputDir = _workDir;
            var arguments = $"-m whisper \"{tempFilePath}\" --model {modelName} --language {language} --output_dir \"{outputDir}\" --output_format json";

            var processInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = _workDir
            };

            // Herda todas as variáveis de ambiente do processo atual
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                processInfo.Environment[entry.Key.ToString()!] = entry.Value?.ToString() ?? string.Empty;
            }

            // Garante que o PATH inclui o diretório do FFmpeg
            if (!string.IsNullOrEmpty(ffmpegPath))
            {
                var ffmpegDir = Path.GetDirectoryName(ffmpegPath);
                if (!string.IsNullOrEmpty(ffmpegDir))
                {
                    var currentPath = processInfo.Environment["PATH"] ?? string.Empty;
                    if (!currentPath.Contains(ffmpegDir, StringComparison.OrdinalIgnoreCase))
                    {
                        processInfo.Environment["PATH"] = $"{ffmpegDir};{currentPath}";
                    }
                }
            }

            using var process = new Process { StartInfo = processInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Simula progresso baseado no tempo
            var progressTask = Task.Run(async () =>
            {
                var startTime = DateTime.Now;
                var fileInfo = new FileInfo(tempFilePath);
                var estimatedDuration = EstimateProcessingTime(fileInfo.Length);
                
                while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    var estimatedProgress = 10 + Math.Min(85, (int)((elapsed / estimatedDuration) * 85));
                    progress?.Report(estimatedProgress);
                }
            }, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            
            try { await progressTask; } catch (OperationCanceledException) { }

            if (cancellationToken.IsCancellationRequested)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                throw new OperationCanceledException("Transcrição cancelada pelo usuário.");
            }

            var stdOut = outputBuilder.ToString();
            var stdErr = errorBuilder.ToString();

            // Verifica se houve erro fatal
            if (process.ExitCode != 0 && !stdErr.Contains("FP16 is not supported"))
            {
                throw new Exception($"Erro na transcrição (código {process.ExitCode}):\n{stdErr}");
            }

            // Aguarda o arquivo JSON ser gravado
            await Task.Delay(1000);

            // Procura o arquivo JSON gerado pelo Whisper
            var jsonFileName = Path.GetFileNameWithoutExtension(tempFileName) + ".json";
            var jsonFilePath = Path.Combine(outputDir, jsonFileName);

            // Aguarda o arquivo existir (até 10 segundos)
            for (int i = 0; i < 20 && !File.Exists(jsonFilePath); i++)
            {
                await Task.Delay(500);
            }

            if (!File.Exists(jsonFilePath))
            {
                // Tenta encontrar qualquer arquivo JSON recente
                var jsonFiles = Directory.GetFiles(outputDir, "*.json")
                    .Where(f => new FileInfo(f).LastWriteTime > DateTime.Now.AddMinutes(-5))
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .ToArray();
                    
                if (jsonFiles.Length > 0)
                {
                    jsonFilePath = jsonFiles[0];
                }
                else
                {
                    throw new Exception($"Arquivo JSON não encontrado.\nSaída: {stdOut}\nErro: {stdErr}");
                }
            }

            // Lê e processa o JSON
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8);
            progress?.Report(100);

            try
            {
                var whisperResult = JsonConvert.DeserializeObject<WhisperJsonResult>(jsonContent);
                
                if (whisperResult == null)
                {
                    throw new Exception("Erro ao processar resultado do Whisper.");
                }

                var result = new TranscriptionResult
                {
                    Text = whisperResult.Text?.Trim() ?? string.Empty,
                    Language = whisperResult.Language ?? language,
                    Segments = new List<TranscriptionSegment>()
                };

                if (whisperResult.Segments != null)
                {
                    for (int i = 0; i < whisperResult.Segments.Count; i++)
                    {
                        var seg = whisperResult.Segments[i];
                        result.Segments.Add(new TranscriptionSegment
                        {
                            Id = i + 1,
                            Start = seg.Start,
                            End = seg.End,
                            Text = seg.Text?.Trim() ?? string.Empty
                        });
                    }
                }

                return result;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Erro ao processar JSON: {ex.Message}");
            }
        }
        finally
        {
            // Limpa arquivos temporários após processamento
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                CleanWorkDirectory();
            });
        }
    }

    private void CleanWorkDirectory()
    {
        try
        {
            if (Directory.Exists(_workDir))
            {
                foreach (var file in Directory.GetFiles(_workDir))
                {
                    try
                    {
                        // Não deleta arquivos que ainda estão sendo usados
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.LastWriteTime < DateTime.Now.AddMinutes(-5))
                        {
                            File.Delete(file);
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    private string FindPython()
    {
        // Tenta encontrar Python no PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var paths = pathEnv.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            var pythonPaths = new[]
            {
                Path.Combine(path, "python.exe"),
                Path.Combine(path, "python3.exe"),
                Path.Combine(path, "py.exe")
            };

            foreach (var pythonPath in pythonPaths)
            {
                if (File.Exists(pythonPath))
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = pythonPath,
                            Arguments = "--version",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        };

                        using var process = Process.Start(processInfo);
                        if (process != null)
                        {
                            var version = process.StandardOutput.ReadToEnd();
                            if (version.Contains("Python 3"))
                            {
                                return pythonPath;
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        // Tenta locais comuns no Windows
        var commonPaths = new[]
        {
            @"C:\Python39\python.exe",
            @"C:\Python310\python.exe",
            @"C:\Python311\python.exe",
            @"C:\Python312\python.exe",
            @"C:\Python313\python.exe",
            @"C:\Python314\python.exe",
            @"C:\Program Files\Python39\python.exe",
            @"C:\Program Files\Python310\python.exe",
            @"C:\Program Files\Python311\python.exe",
            @"C:\Program Files\Python312\python.exe",
            @"C:\Program Files\Python313\python.exe",
            @"C:\Program Files\Python314\python.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python39", "python.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python310", "python.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python311", "python.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python312", "python.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python313", "python.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python314", "python.exe")
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

    private double EstimateProcessingTime(long fileSizeBytes)
    {
        var sizeMB = fileSizeBytes / (1024.0 * 1024.0);
        return Math.Max(60, sizeMB * 8); // ~8 segundos por MB
    }

    private string FindFFmpeg()
    {
        // Tenta encontrar no PATH do sistema
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
            @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ffmpeg", "bin", "ffmpeg.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe")
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

    private class WhisperJsonResult
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("segments")]
        public List<WhisperSegment>? Segments { get; set; }
    }

    private class WhisperSegment
    {
        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("end")]
        public double End { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}

