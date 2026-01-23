using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Businesses.Commands.DeleteBusiness;

public record DeleteBusinessCommand(int BusinessId) : IRequest<Result>;
