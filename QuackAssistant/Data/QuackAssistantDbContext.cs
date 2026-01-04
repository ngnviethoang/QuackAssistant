using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data.Entities;

namespace QuackAssistant.Data;

public class QuackAssistantDbContext : DbContext
{
    public QuackAssistantDbContext(DbContextOptions<QuackAssistantDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Debt> Debts { get; set; }
}