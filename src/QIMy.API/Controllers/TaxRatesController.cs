using Microsoft.AspNetCore.Mvc;
using MediatR;
using QIMy.Application.TaxRates.Queries.GetAllTaxRates;
using QIMy.Application.TaxRates.Queries.GetTaxRateById;
using QIMy.Application.TaxRates.Commands.CreateTaxRate;
using QIMy.Application.TaxRates.Commands.UpdateTaxRate;
using QIMy.Application.TaxRates.Commands.DeleteTaxRate;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllTaxRatesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetTaxRateByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxRateCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTaxRateCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result);
    }
}
