using MediatR;
using QIMy.Application.Clients.DTOs;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Clients.Commands.UpdateClient;

/// <summary>
/// Команда для обновления клиента
/// </summary>
public record UpdateClientCommand : IRequest<Result<ClientDto>>
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public int? ClientTypeId { get; set; }
    public int? ClientAreaId { get; set; }

    /// <summary>
    /// Первое игнорирование предупреждения о дубликате (пользователь подтвердил 1 раз)
    /// </summary>
    public bool IgnoreDuplicateWarning { get; set; }

    /// <summary>
    /// Второе подтверждение для создания дубликата (пользователь подтвердил 2 раза)
    /// </summary>
    public bool DoubleConfirmed { get; set; }
}
