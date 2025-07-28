namespace llm_thought_taker.DTOs;

public class NoteRequest
{
    public string? ExternalUserId { get; set; }
    public string? Title { get; set; }
    public string? Prompt { get; set; }
    public string? Content { get; set; }
}