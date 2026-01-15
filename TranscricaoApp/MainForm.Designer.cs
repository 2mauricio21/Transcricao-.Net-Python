namespace TranscricaoApp;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Label lblYouTubeUrl;
    private TextBox txtYouTubeUrl;
    private Button btnSelectFile;
    private Label lblSelectedFile;
    private Button btnTranscribe;
    private Button btnCancel;
    private ProgressBar progressBar;
    private Label lblProgress;
    private TextBox txtTranscription;
    private Button btnSaveTxt;
    private Button btnSaveSrt;
    private Label lblOr;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.lblTitle = new Label();
        this.lblYouTubeUrl = new Label();
        this.txtYouTubeUrl = new TextBox();
        this.btnSelectFile = new Button();
        this.lblOr = new Label();
        this.lblSelectedFile = new Label();
        this.btnTranscribe = new Button();
        this.btnCancel = new Button();
        this.progressBar = new ProgressBar();
        this.lblProgress = new Label();
        this.txtTranscription = new TextBox();
        this.btnSaveTxt = new Button();
        this.btnSaveSrt = new Button();
        this.SuspendLayout();
        
        // lblTitle
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        this.lblTitle.Location = new Point(12, 9);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new Size(350, 30);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Transcrição de Vídeos com IA";
        
        // lblYouTubeUrl
        this.lblYouTubeUrl.AutoSize = true;
        this.lblYouTubeUrl.Location = new Point(12, 50);
        this.lblYouTubeUrl.Name = "lblYouTubeUrl";
        this.lblYouTubeUrl.Size = new Size(100, 15);
        this.lblYouTubeUrl.TabIndex = 1;
        this.lblYouTubeUrl.Text = "URL do YouTube:";
        
        // txtYouTubeUrl
        this.txtYouTubeUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.txtYouTubeUrl.Location = new Point(12, 68);
        this.txtYouTubeUrl.Name = "txtYouTubeUrl";
        this.txtYouTubeUrl.Size = new Size(776, 23);
        this.txtYouTubeUrl.TabIndex = 2;
        this.txtYouTubeUrl.PlaceholderText = "https://www.youtube.com/watch?v=...";
        
        // lblOr
        this.lblOr.AutoSize = true;
        this.lblOr.Location = new Point(12, 100);
        this.lblOr.Name = "lblOr";
        this.lblOr.Size = new Size(20, 15);
        this.lblOr.TabIndex = 3;
        this.lblOr.Text = "OU";
        
        // btnSelectFile
        this.btnSelectFile.Location = new Point(12, 118);
        this.btnSelectFile.Name = "btnSelectFile";
        this.btnSelectFile.Size = new Size(150, 30);
        this.btnSelectFile.TabIndex = 4;
        this.btnSelectFile.Text = "Selecionar Arquivo MP4";
        this.btnSelectFile.UseVisualStyleBackColor = true;
        this.btnSelectFile.Click += new EventHandler(this.btnSelectFile_Click);
        
        // lblSelectedFile
        this.lblSelectedFile.AutoSize = true;
        this.lblSelectedFile.Location = new Point(12, 155);
        this.lblSelectedFile.Name = "lblSelectedFile";
        this.lblSelectedFile.Size = new Size(0, 15);
        this.lblSelectedFile.TabIndex = 5;
        this.lblSelectedFile.Visible = false;
        
        // btnTranscribe
        this.btnTranscribe.BackColor = Color.FromArgb(0, 122, 204);
        this.btnTranscribe.ForeColor = Color.White;
        this.btnTranscribe.Location = new Point(12, 180);
        this.btnTranscribe.Name = "btnTranscribe";
        this.btnTranscribe.Size = new Size(150, 30);
        this.btnTranscribe.TabIndex = 6;
        this.btnTranscribe.Text = "Iniciar Transcrição";
        this.btnTranscribe.UseVisualStyleBackColor = false;
        this.btnTranscribe.Click += new EventHandler(this.btnTranscribe_Click);
        
        // btnCancel
        this.btnCancel.Enabled = false;
        this.btnCancel.Location = new Point(12, 180);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(150, 30);
        this.btnCancel.TabIndex = 7;
        this.btnCancel.Text = "Cancelar";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Visible = false;
        this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
        
        // progressBar
        this.progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.progressBar.Location = new Point(12, 220);
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new Size(776, 23);
        this.progressBar.TabIndex = 8;
        
        // lblProgress
        this.lblProgress.AutoSize = true;
        this.lblProgress.Location = new Point(12, 246);
        this.lblProgress.Name = "lblProgress";
        this.lblProgress.Size = new Size(0, 15);
        this.lblProgress.TabIndex = 9;
        
        // txtTranscription
        this.txtTranscription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.txtTranscription.Font = new Font("Consolas", 9F);
        this.txtTranscription.Location = new Point(12, 270);
        this.txtTranscription.Multiline = true;
        this.txtTranscription.Name = "txtTranscription";
        this.txtTranscription.ReadOnly = true;
        this.txtTranscription.ScrollBars = ScrollBars.Vertical;
        this.txtTranscription.Size = new Size(776, 350);
        this.txtTranscription.TabIndex = 10;
        
        // btnSaveTxt
        this.btnSaveTxt.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.btnSaveTxt.Enabled = false;
        this.btnSaveTxt.Location = new Point(638, 570);
        this.btnSaveTxt.Name = "btnSaveTxt";
        this.btnSaveTxt.Size = new Size(150, 30);
        this.btnSaveTxt.TabIndex = 11;
        this.btnSaveTxt.Text = "Salvar como TXT";
        this.btnSaveTxt.UseVisualStyleBackColor = true;
        this.btnSaveTxt.Click += new EventHandler(this.btnSaveTxt_Click);
        
        // btnSaveSrt
        this.btnSaveSrt.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.btnSaveSrt.Enabled = false;
        this.btnSaveSrt.Location = new Point(482, 570);
        this.btnSaveSrt.Name = "btnSaveSrt";
        this.btnSaveSrt.Size = new Size(150, 30);
        this.btnSaveSrt.TabIndex = 12;
        this.btnSaveSrt.Text = "Salvar como SRT";
        this.btnSaveSrt.UseVisualStyleBackColor = true;
        this.btnSaveSrt.Click += new EventHandler(this.btnSaveSrt_Click);
        
        // MainForm
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(800, 610);
        this.Controls.Add(this.btnSaveSrt);
        this.Controls.Add(this.btnSaveTxt);
        this.Controls.Add(this.txtTranscription);
        this.Controls.Add(this.lblProgress);
        this.Controls.Add(this.progressBar);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnTranscribe);
        this.Controls.Add(this.lblSelectedFile);
        this.Controls.Add(this.btnSelectFile);
        this.Controls.Add(this.lblOr);
        this.Controls.Add(this.txtYouTubeUrl);
        this.Controls.Add(this.lblYouTubeUrl);
        this.Controls.Add(this.lblTitle);
        this.MinimumSize = new Size(600, 400);
        this.Name = "MainForm";
        this.Text = "Transcrição de Vídeos com IA";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
