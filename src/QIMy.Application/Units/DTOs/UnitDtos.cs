namespace QIMy.Application.Units.DTOs;

public record UnitDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateUnitDto
{
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; } = false;
}

public record UpdateUnitDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}
