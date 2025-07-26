namespace llm_thought_taker.Data;
using Microsoft.EntityFrameworkCore;
using llm_thought_taker.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
}
