using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data.Entities;

namespace QuackAssistant.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(QuackAssistantDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new(name: "Fixed", description: "Rent, electricity, water, Internet"),
                new(name: "Food & Drink", description: "Restaurants, cafes, snacks"),
                new(name: "Transportation", description: "Gas, bus, taxi"),
                new(name: "Entertainment", description: "Movies, books, shopping"),
                new(name: "Savings", description: "Savings deposit, investments"),
                new(name: "Emergency", description: "Unexpected expenses"),
                new(name: "Other", description: "Expenses that do not fit into the above categories")
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}