namespace QuackAssistant.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandAttribute : Attribute
{
    public CommandAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string? Description { get; init; }

    public string? Example { get; init; }

    public string? Alias { get; init; }
}