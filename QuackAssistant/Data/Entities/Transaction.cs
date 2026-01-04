using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuackAssistant.Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public int Amount { get; set; }

    [MaxLength(10)]
    public string Type { get; set; }

    public Guid CategoryId { get; set; }

    [MaxLength(1024)]
    public string Note { get; set; }

    public DateTimeOffset TransactionTime { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}