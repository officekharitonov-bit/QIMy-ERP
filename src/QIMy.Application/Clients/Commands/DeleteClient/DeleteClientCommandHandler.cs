using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Clients.Commands.DeleteClient;

/// <summary>
/// Обработчик команды удаления клиента
/// </summary>
public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteClientCommandHandler> _logger;

    public DeleteClientCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteClientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting client: Id={ClientId}", request.ClientId);

        try
        {
            // 1. Проверяем существование клиента
            var exists = await _unitOfWork.Clients.ExistsAsync(request.ClientId, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Client with Id {ClientId} not found", request.ClientId);
                throw new NotFoundException("Client", request.ClientId);
            }

            // 2. Проверяем наличие связанных счетов
            var invoices = await _unitOfWork.Invoices
                .FindAsync(i => i.ClientId == request.ClientId && !i.IsDeleted, cancellationToken);

            if (invoices.Any())
            {
                _logger.LogWarning("Cannot delete client {ClientId} - has {Count} invoices",
                    request.ClientId, invoices.Count());
                return Result.Failure($"Невозможно удалить клиента: существуют связанные счета ({invoices.Count()})");
            }

            // 3. Удаляем (soft delete)
            await _unitOfWork.Clients.DeleteAsync(request.ClientId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Client deleted successfully: Id={ClientId}", request.ClientId);

            return Result.Success();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client: Id={ClientId}", request.ClientId);
            return Result.Failure($"Ошибка при удалении клиента: {ex.Message}");
        }
    }
}
