using System.ComponentModel.DataAnnotations;
using QuackAssistant.Shared.Enumerations;

namespace QuackAssistant.Data.Entities;

public class Debt
{
    public Guid Id { get; set; }

    [MaxLength(1024)]
    public string PersonName { get; set; }

    [MaxLength(1024)]
    public string Description { get; set; }

    public int Amount { get; set; }

    public DebtDirectionType Direction { get; set; }

    public DateTimeOffset CreationTime { get; set; }
}