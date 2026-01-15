using YoutubeExplode;
using YoutubeExplode.Videos;

namespace TranscricaoApp.Services;

public class VideoService
{
    private readonly YoutubeClient _youtubeClient;

    public VideoService()
    {
        _youtubeClient = new YoutubeClient();
    }

    public bool IsYouTubeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return url.Contains("youtube.com/watch") || 
               url.Contains("youtu.be/") || 
               url.Contains("youtube.com/embed/");
    }

    public async Task<string> DownloadYouTubeVideoAsync(string url, string outputDirectory, IProgress<int>? progress = null)
    {
        if (!IsYouTubeUrl(url))
        {
            throw new ArgumentException("URL inválida do YouTube", nameof(url));
        }

        try
        {
            var video = await _youtubeClient.Videos.GetAsync(url);
            var outputPath = Path.Combine(outputDirectory, SanitizeFileName(video.Title) + ".mp4");

            // Busca streams disponíveis
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            
            // Prioriza streams combinados (muxed) que já têm vídeo e áudio
            var combinedStream = streamManifest.GetMuxedStreams()
                .OrderByDescending(s => s.VideoQuality)
                .FirstOrDefault();

            if (combinedStream != null)
            {
                // Usa stream combinado (mais simples)
                await _youtubeClient.Videos.Streams.DownloadAsync(combinedStream, outputPath);
                progress?.Report(100);
                return outputPath;
            }

            // Se não houver stream combinado, tenta baixar vídeo e áudio separadamente
            var videoStreamInfo = streamManifest.GetVideoOnlyStreams()
                .OrderByDescending(s => s.VideoQuality)
                .FirstOrDefault();

            var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                .OrderByDescending(s => s.Bitrate)
                .FirstOrDefault();

            if (videoStreamInfo == null || audioStreamInfo == null)
            {
                throw new Exception("Nenhum stream de vídeo/áudio disponível");
            }

            // Baixa vídeo e áudio separadamente
            var videoPath = Path.Combine(outputDirectory, "temp_video.mp4");
            var audioPath = Path.Combine(outputDirectory, "temp_audio.mp4");

            await _youtubeClient.Videos.Streams.DownloadAsync(videoStreamInfo, videoPath);
            progress?.Report(50);
            await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, audioPath);
            progress?.Report(75);

            // Por enquanto, usa apenas o vídeo (Whisper pode processar diretamente)
            // Em uma versão futura, poderíamos combinar vídeo e áudio com FFmpeg
            outputPath = videoPath;
            
            // Limpa arquivo de áudio temporário se não for usado
            if (File.Exists(audioPath))
            {
                File.Delete(audioPath);
            }

            progress?.Report(100);
            return outputPath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao baixar vídeo do YouTube: {ex.Message}", ex);
        }
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
            .TrimEnd('.');
    }
}
