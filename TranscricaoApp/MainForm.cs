using TranscricaoApp.Models;
using TranscricaoApp.Services;
using System.Text;

namespace TranscricaoApp;

public partial class MainForm : Form
{
    private readonly VideoService _videoService;
    private readonly TranscriptionService _transcriptionService;
    private CancellationTokenSource? _cancellationTokenSource;
    private TranscriptionResult? _currentResult;
    private string? _currentVideoPath;

    public MainForm()
    {
        InitializeComponent();
        _videoService = new VideoService();
        _transcriptionService = new TranscriptionService();
        
        UpdateUIState(false);
    }

    private async void btnSelectFile_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "Arquivos de vídeo|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.wav;*.mp3|Todos os arquivos|*.*",
            Title = "Selecionar arquivo de vídeo ou áudio"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtYouTubeUrl.Text = string.Empty;
            _currentVideoPath = openFileDialog.FileName;
            lblSelectedFile.Text = $"Arquivo selecionado: {Path.GetFileName(_currentVideoPath)}";
            lblSelectedFile.Visible = true;
        }
    }

    private async void btnTranscribe_Click(object sender, EventArgs e)
    {
        try
        {
            UpdateUIState(true);
            _cancellationTokenSource = new CancellationTokenSource();
            progressBar.Value = 0;
            txtTranscription.Text = "Iniciando transcrição...\r\n";
            _currentResult = null;

            string videoPath;

            // Verifica se é URL do YouTube ou arquivo local
            if (!string.IsNullOrWhiteSpace(txtYouTubeUrl.Text))
            {
                var url = txtYouTubeUrl.Text.Trim();
                if (!_videoService.IsYouTubeUrl(url))
                {
                    throw new ArgumentException("URL do YouTube inválida. Por favor, insira uma URL válida do YouTube.");
                }

                txtTranscription.Text = "Baixando vídeo do YouTube...\r\n";
                Application.DoEvents();

                string tempDirectory = Path.Combine(Path.GetTempPath(), "TranscricaoApp_YouTube");
                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                }

                var progressDownload = new Progress<int>(p => 
                {
                    progressBar.Value = Math.Min(20, p / 5);
                    Application.DoEvents();
                });

                videoPath = await _videoService.DownloadYouTubeVideoAsync(url, tempDirectory, progressDownload);
                _currentVideoPath = videoPath;
            }
            else if (!string.IsNullOrEmpty(_currentVideoPath) && File.Exists(_currentVideoPath))
            {
                videoPath = _currentVideoPath;
            }
            else
            {
                throw new InvalidOperationException("Por favor, selecione um arquivo de vídeo ou insira uma URL do YouTube.");
            }

            // Verifica se o arquivo existe
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {videoPath}");
            }

            txtTranscription.Text += $"Arquivo: {Path.GetFileName(videoPath)}\r\n";
            txtTranscription.Text += "Transcrevendo com Whisper (isso pode levar alguns minutos)...\r\n";
            txtTranscription.Text += "Por favor, aguarde...\r\n";
            Application.DoEvents();

            var progressTranscribe = new Progress<int>(p => 
            {
                progressBar.Value = 20 + Math.Min(75, (p * 75) / 100);
                Application.DoEvents();
            });

            _currentResult = await _transcriptionService.TranscribeAsync(
                videoPath,
                "base",
                "pt",
                progressTranscribe,
                _cancellationTokenSource.Token);

            // Exibe resultado
            txtTranscription.Text = _currentResult.Text;
            progressBar.Value = 100;

            MessageBox.Show("Transcrição concluída com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            txtTranscription.Text += "\r\nTranscrição cancelada pelo usuário.";
            MessageBox.Show("Transcrição cancelada.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            txtTranscription.Text += $"\r\n\r\nERRO: {ex.Message}";
            MessageBox.Show($"Erro durante a transcrição:\r\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UpdateUIState(false);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        btnCancel.Enabled = false;
    }

    private void btnSaveTxt_Click(object sender, EventArgs e)
    {
        if (_currentResult == null || string.IsNullOrEmpty(_currentResult.Text))
        {
            MessageBox.Show("Nenhuma transcrição disponível para salvar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Arquivo de texto|*.txt",
            Title = "Salvar transcrição como TXT",
            FileName = "transcricao.txt"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(saveFileDialog.FileName, _currentResult.Text, Encoding.UTF8);
            MessageBox.Show("Arquivo salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnSaveSrt_Click(object sender, EventArgs e)
    {
        if (_currentResult == null || _currentResult.Segments.Count == 0)
        {
            MessageBox.Show("Nenhuma transcrição com timestamps disponível para salvar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Arquivo de legendas|*.srt",
            Title = "Salvar transcrição como SRT",
            FileName = "transcricao.srt"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            var srtContent = GenerateSrtContent(_currentResult);
            File.WriteAllText(saveFileDialog.FileName, srtContent, Encoding.UTF8);
            MessageBox.Show("Arquivo SRT salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private string GenerateSrtContent(TranscriptionResult result)
    {
        var sb = new StringBuilder();
        
        foreach (var segment in result.Segments)
        {
            sb.AppendLine(segment.Id.ToString());
            sb.AppendLine($"{FormatTime(segment.Start)} --> {FormatTime(segment.End)}");
            sb.AppendLine(segment.Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string FormatTime(double seconds)
    {
        var hours = (int)(seconds / 3600);
        var minutes = (int)((seconds % 3600) / 60);
        var secs = (int)(seconds % 60);
        var milliseconds = (int)((seconds % 1) * 1000);

        return $"{hours:D2}:{minutes:D2}:{secs:D2},{milliseconds:D3}";
    }

    private void UpdateUIState(bool isProcessing)
    {
        btnSelectFile.Enabled = !isProcessing;
        txtYouTubeUrl.Enabled = !isProcessing;
        btnTranscribe.Enabled = !isProcessing;
        btnCancel.Enabled = isProcessing;
        btnSaveTxt.Enabled = !isProcessing && _currentResult != null;
        btnSaveSrt.Enabled = !isProcessing && _currentResult != null && _currentResult.Segments.Count > 0;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnFormClosing(e);
    }
}
