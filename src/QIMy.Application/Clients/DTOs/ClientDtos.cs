namespace QIMy.Application.Clients.DTOs;

/// <summary>
/// DTO для отображения клиента
/// </summary>
public record ClientDto
{
    public int Id { get; init; }
    public int ClientCode { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }

    public int? ClientTypeId { get; init; }
    public string? ClientTypeName { get; init; }

    public int? ClientAreaId { get; init; }
    public string? ClientAreaName { get; init; }

    public string? TaxNumber { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO для создания клиента
/// </summary>
public record CreateClientDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
}

/// <summary>
/// DTO для обновления клиента
/// </summary>
public record UpdateClientDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
}
