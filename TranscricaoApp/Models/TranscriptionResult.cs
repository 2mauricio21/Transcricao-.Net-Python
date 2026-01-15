using System.Collections.Generic;

namespace TranscricaoApp.Models;

public class TranscriptionResult
{
    public string Text { get; set; } = string.Empty;
    public List<TranscriptionSegment> Segments { get; set; } = new();
    public string Language { get; set; } = string.Empty;
}

public class TranscriptionSegment
{
    public int Id { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public string Text { get; set; } = string.Empty;
}
