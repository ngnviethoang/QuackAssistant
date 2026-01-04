using System.ComponentModel.DataAnnotations;

namespace QuackAssistant.Data.Entities;

public class Category
{
    public Category(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public Guid Id { get; set; }

    [MaxLength(512)]
    public string Name { get; set; }

    [MaxLength(512)]
    public string Description { get; set; }
}