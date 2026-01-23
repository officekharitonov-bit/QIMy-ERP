using MediatR;
using QIMy.Application.Businesses.DTOs;

namespace QIMy.Application.Businesses.Queries.GetAllBusinesses;

public record GetAllBusinessesQuery : IRequest<IEnumerable<BusinessDto>>;
