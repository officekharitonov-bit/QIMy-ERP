using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Clients.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Clients.Commands.UpdateClient;

/// <summary>
/// Обработчик команды обновления клиента
/// </summary>
public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateClientCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public UpdateClientCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateClientCommandHandler> logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating client: Id={ClientId}, CompanyName={CompanyName}",
            request.Id, request.CompanyName);

        try
        {
            // 1. Получаем существующего клиента
            var client = await _unitOfWork.Clients.GetByIdAsync(request.Id, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client with Id {ClientId} not found", request.Id);
                throw new NotFoundException("Client", request.Id);
            }

            // 1.5. Проверка безопасности: BusinessId должен совпадать
            if (request.BusinessId.HasValue && client.BusinessId != request.BusinessId.Value)
            {
                _logger.LogWarning("Unauthorized access attempt: Client {ClientId} belongs to BusinessId {ActualBusinessId}, but request is for BusinessId {RequestBusinessId}",
                    request.Id, client.BusinessId, request.BusinessId.Value);
                throw new UnauthorizedBusinessAccessException("Client", request.Id, request.BusinessId.Value, client.BusinessId);
            }

            // 2. Проверка на дубликат по названию/коду
            var duplicate = await _duplicateDetectionService.CheckClientDuplicateAsync(
                request.CompanyName,
                clientCode: null,
                excludeId: request.Id,
                cancellationToken);

            if (duplicate != null)
            {
                if (!request.IgnoreDuplicateWarning)
                {
                    return Result<ClientDto>.Failure(
                        $"Дубликат клиента: {duplicate.Message}. Подтвердите IgnoreDuplicateWarning=true и DoubleConfirmed=true, чтобы сохранить изменения как дубликат.");
                }

                if (request.IgnoreDuplicateWarning && !request.DoubleConfirmed)
                {
                    return Result<ClientDto>.Failure(
                        $"Требуется второе подтверждение для дубликата: {duplicate.Message}. Установите DoubleConfirmed=true.");
                }

                _logger.LogWarning("Client duplicate accepted after double confirmation on update: Id={ClientId}", request.Id);
            }

            // 3. Проверка на дубликат VatNumber (если изменился) — строгий запрет
            if (!string.IsNullOrEmpty(request.VatNumber) &&
                request.VatNumber != client.VatNumber)
            {
                var existingClients = await _unitOfWork.Clients
                    .FindAsync(c => c.VatNumber == request.VatNumber &&
                                   c.Id != request.Id &&
                                   !c.IsDeleted, cancellationToken);

                if (existingClients.Any())
                {
                    _logger.LogWarning("Client with VAT {VatNumber} already exists", request.VatNumber);
                    throw new DuplicateException("Client", "VatNumber", request.VatNumber);
                }
            }

            // 4. Обновляем свойства
            client.CompanyName = request.CompanyName;
            client.ContactPerson = request.ContactPerson;
            client.Email = request.Email;
            client.Phone = request.Phone;
            client.VatNumber = request.VatNumber;
            client.Address = request.Address;
            client.City = request.City;
            client.PostalCode = request.PostalCode;
            client.Country = request.Country ?? "Österreich";
            client.ClientTypeId = request.ClientTypeId;
            client.ClientAreaId = request.ClientAreaId;

            if (request.BusinessId.HasValue && request.BusinessId.Value != client.BusinessId)
            {
                return Result<ClientDto>.Failure("Changing BusinessId is not allowed.");
            }

            // 5. Сохраняем
            await _unitOfWork.Clients.UpdateAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Client updated successfully: Id={ClientId}", client.Id);

            // 6. Получаем обновленного клиента с навигационными свойствами
            var updatedClient = await _unitOfWork.Clients.GetByIdAsync(client.Id, cancellationToken);

            // 7. Маппинг в DTO
            var dto = _mapper.Map<ClientDto>(updatedClient);

            return Result<ClientDto>.Success(dto);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DuplicateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client: Id={ClientId}", request.Id);
            return Result<ClientDto>.Failure($"Ошибка при обновлении клиента: {ex.Message}");
        }
    }
}
