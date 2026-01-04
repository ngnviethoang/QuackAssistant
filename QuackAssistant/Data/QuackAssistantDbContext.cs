using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data.Entities;

namespace QuackAssistant.Data;

public sealed class QuackAssistantDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Debt> Debts { get; set; }

    public QuackAssistantDbContext(DbContextOptions<QuackAssistantDbContext> options) : base(options)
    {
    }
}