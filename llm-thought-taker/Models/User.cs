using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace llm_thought_taker.Models;

using System.ComponentModel.DataAnnotations;


public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("first_name")]
    public string FirstName { get; set; } = "";
    
    [Required]
    [Column("last_name")]
    public string LastName  { get; set; } = "";
    
    [Column("email_address")]
    public string EmailAddress { get; set; } = "";
    
    [Required]
    [Column("external_id")]
    public string ExternalId { get; set; } = "";
    
    [JsonIgnore]
    public ICollection<Note> Notes { get; set; }

}