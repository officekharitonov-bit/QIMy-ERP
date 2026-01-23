using MediatR;
using QIMy.Application.Businesses.DTOs;

namespace QIMy.Application.Businesses.Queries.GetBusinessById;

public record GetBusinessByIdQuery(int BusinessId) : IRequest<BusinessDto?>;
