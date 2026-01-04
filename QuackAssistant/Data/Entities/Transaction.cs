using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuackAssistant.Data.Entities;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(TransactionTime), IsUnique = true)]
public class Transaction
{
    public Transaction(Guid id, int amount, Guid categoryId, string note, DateTimeOffset transactionTime, string code)
    {
        Id = id;
        Amount = amount;
        CategoryId = categoryId;
        Note = note;
        TransactionTime = transactionTime;
        Code = code;
    }

    public Guid Id { get; set; }

    public int Amount { get; set; }

    public Guid CategoryId { get; set; }

    [MaxLength(512)]
    public string Note { get; set; }

    [MaxLength(32)]
    public string Code { get; set; }

    public DateTimeOffset TransactionTime { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}