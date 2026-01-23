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

    public UpdateClientCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateClientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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

            // 2. Проверка на дубликат VatNumber (если изменился)
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

            // 3. Обновляем свойства
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

            // 4. Сохраняем
            await _unitOfWork.Clients.UpdateAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Client updated successfully: Id={ClientId}", client.Id);

            // 5. Получаем обновленного клиента с навигационными свойствами
            var updatedClient = await _unitOfWork.Clients.GetByIdAsync(client.Id, cancellationToken);

            // 6. Маппинг в DTO
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
