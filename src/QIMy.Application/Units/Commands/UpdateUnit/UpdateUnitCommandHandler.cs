using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Units.DTOs;
using QIMy.Core.Entities;
using Unit = QIMy.Core.Entities.Unit;

namespace QIMy.Application.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, Result<UnitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUnitCommandHandler> _logger;

    public UpdateUnitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateUnitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UnitDto>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Unit: Id={Id}", request.Id);

        try
        {
            var Unit = await _unitOfWork.Units.GetByIdAsync(request.Id, cancellationToken);
            if (Unit == null)
                throw new NotFoundException("Unit", request.Id);

            var duplicate = await _unitOfWork.Units.FindAsync(
                t => t.Name == request.Name && t.Id != request.Id && !t.IsDeleted, cancellationToken);
            if (duplicate.Any())
                throw new DuplicateException("Unit", "Name", request.Name);

            Unit.Name = request.Name;
            Unit.ShortName = request.ShortName;
            Unit.IsDefault = request.IsDefault;

            await _unitOfWork.Units.UpdateAsync(Unit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Unit updated: Id={Id}", Unit.Id);
            return Result<UnitDto>.Success(_mapper.Map<UnitDto>(Unit));
        }
        catch (NotFoundException) { throw; }
        catch (DuplicateException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Unit");
            return Result<UnitDto>.Failure($"Ошибка обновления: {ex.Message}");
        }
    }
}
