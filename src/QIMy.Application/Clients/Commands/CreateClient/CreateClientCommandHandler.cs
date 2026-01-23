using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Clients.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Clients.Commands.CreateClient;

/// <summary>
/// Обработчик команды создания клиента
/// </summary>
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public CreateClientCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateClientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating client: {CompanyName}", request.CompanyName);

        try
        {
            // 1. Проверка на дубликат по VatNumber
            if (!string.IsNullOrEmpty(request.VatNumber))
            {
                var existingClients = await _unitOfWork.Clients
                    .FindAsync(c => c.VatNumber == request.VatNumber && !c.IsDeleted, cancellationToken);

                if (existingClients.Any())
                {
                    _logger.LogWarning("Client with VAT {VatNumber} already exists", request.VatNumber);
                    throw new DuplicateException("Client", "VatNumber", request.VatNumber);
                }
            }

            // 2. Создание entity из command
            var client = new Client
            {
                CompanyName = request.CompanyName,
                ContactPerson = request.ContactPerson,
                Email = request.Email,
                Phone = request.Phone,
                VatNumber = request.VatNumber,
                Address = request.Address,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country ?? "Österreich",
                ClientTypeId = request.ClientTypeId,
                ClientAreaId = request.ClientAreaId,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Генерация ClientCode
            client.ClientCode = await GenerateNextClientCodeAsync(client.ClientAreaId, cancellationToken);

            // 4. Сохранение в БД
            await _unitOfWork.Clients.AddAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Client created successfully: Id={ClientId}, Code={ClientCode}",
                client.Id, client.ClientCode);

            // 5. Получение полной сущности с навигационными свойствами
            var createdClient = await _unitOfWork.Clients.GetByIdAsync(client.Id, cancellationToken);

            // 6. Маппинг в DTO
            var dto = _mapper.Map<ClientDto>(createdClient);

            return Result<ClientDto>.Success(dto);
        }
        catch (DuplicateException)
        {
            throw; // Пробрасываем выше для обработки в UI
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client: {CompanyName}", request.CompanyName);
            return Result<ClientDto>.Failure($"Ошибка при создании клиента: {ex.Message}");
        }
    }

    /// <summary>
    /// Генерация следующего кода клиента на основе области
    /// </summary>
    private async Task<int> GenerateNextClientCodeAsync(int? clientAreaId, CancellationToken cancellationToken)
    {
        // Определяем базовый код по области клиента
        int baseCode = clientAreaId switch
        {
            1 => 200000, // Inland (1)
            2 => 230000, // EU (2)
            3 => 260000, // Drittland (3)
            _ => 200000  // По умолчанию Inland
        };

        int maxRange = baseCode + 29999;

        // Получаем всех клиентов в диапазоне
        var clients = await _unitOfWork.Clients
            .FindAsync(c => c.ClientCode.HasValue && c.ClientCode.Value >= baseCode && c.ClientCode.Value <= maxRange, cancellationToken);

        // Находим максимальный код
        var clientsList = clients.ToList();
        var maxCode = clientsList.Any() ? clientsList.Max(c => c.ClientCode ?? baseCode - 1) : baseCode - 1;

        return maxCode + 1;
    }
}
