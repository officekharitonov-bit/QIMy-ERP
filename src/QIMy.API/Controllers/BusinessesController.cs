using Microsoft.AspNetCore.Mvc;
using MediatR;
using QIMy.Application.Businesses.Queries.GetAllBusinesses;
using QIMy.Application.Businesses.Queries.GetBusinessById;
using QIMy.Application.Businesses.Commands.CreateBusiness;
using QIMy.Application.Businesses.Commands.UpdateBusiness;
using QIMy.Application.Businesses.Commands.DeleteBusiness;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusinessesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllBusinessesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetBusinessByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteBusinessCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result);
    }
}
