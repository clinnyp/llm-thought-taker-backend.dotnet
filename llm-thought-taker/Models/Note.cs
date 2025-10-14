using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace llm_thought_taker.Models;

public class Note
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
   
    [Required]
    [Column("title")]
    public string Title { get; set; } = String.Empty;
    [Required]
    [Column("prompt")]
    public string Prompt { get; set; } = String.Empty;
    [Required]
    [Column("content")]
    public string Content { get; set; } = String.Empty;
    [Column("created_at")]
    public DateTime Created { get; set; } = DateTime.UtcNow;
    [Column("modified_at")]
    public DateTime Modified { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column("userId")]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}