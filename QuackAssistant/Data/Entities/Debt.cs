using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QuackAssistant.Shared.Enumerations;

namespace QuackAssistant.Data.Entities;

[Index(nameof(CreationTime), IsUnique = true)]
public class Debt
{
    public Debt(string personName, string description, int amount, DebtDirectionType direction, DateTimeOffset creationTime)
    {
        PersonName = personName;
        Description = description;
        Amount = amount;
        Direction = direction;
        CreationTime = creationTime;
    }

    public Guid Id { get; set; }

    [MaxLength(512)]
    public string PersonName { get; set; }

    [MaxLength(512)]
    public string Description { get; set; }

    public int Amount { get; set; }

    public DebtDirectionType Direction { get; set; }

    public DateTimeOffset CreationTime { get; set; }
}