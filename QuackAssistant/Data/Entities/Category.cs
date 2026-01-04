using System.ComponentModel.DataAnnotations;

namespace QuackAssistant.Data.Entities;

public class Category
{
    public Guid Id { get; set; }

    [MaxLength(1024)]
    public string Name { get; set; }
}