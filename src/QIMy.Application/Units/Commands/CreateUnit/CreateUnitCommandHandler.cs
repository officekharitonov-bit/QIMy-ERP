using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Units.DTOs;
using QIMy.Core.Entities;
using Unit = QIMy.Core.Entities.Unit;

namespace QIMy.Application.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUnitCommandHandler> _logger;

    public CreateUnitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateUnitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Unit: {Name} ({Name}%)", request.Name, request.Name);

        try
        {
            var existing = await _unitOfWork.Units.FindAsync(t => t.Name == request.Name && !t.IsDeleted, cancellationToken);
            if (existing.Any())
                throw new DuplicateException("Unit", "Name", request.Name);

            var unit = new Unit
            {
                Name = request.Name,
                ShortName = request.ShortName,
                IsDefault = request.IsDefault
            };

            await _unitOfWork.Units.AddAsync(unit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Unit created: Id={Id}", unit.Id);
            return Result<UnitDto>.Success(_mapper.Map<UnitDto>(unit));
        }
        catch (DuplicateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Unit");
            return Result<UnitDto>.Failure($"Ошибка создания: {ex.Message}");
        }
    }
}
