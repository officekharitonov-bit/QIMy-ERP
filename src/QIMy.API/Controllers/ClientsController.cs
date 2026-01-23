using Microsoft.AspNetCore.Mvc;
using MediatR;
using QIMy.Application.Clients.Queries.GetAllClients;
using QIMy.Application.Clients.Queries.GetClientById;
using QIMy.Application.Clients.Commands.CreateClient;
using QIMy.Application.Clients.Commands.UpdateClient;
using QIMy.Application.Clients.Commands.DeleteClient;
using QIMy.Core.Interfaces;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IClientService _clientService;

    public ClientsController(IMediator mediator, IClientService clientService)
    {
        _mediator = mediator;
        _clientService = clientService;
    }

    /// <summary>
    /// Get all clients
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllClientsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var query = new GetClientByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { error = "Client not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create new client
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update existing client
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { error = "ID in URL doesn't match ID in body" });
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete client
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var command = new DeleteClientCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv()
    {
        try
        {
            var csvBytes = await _clientService.ExportToCsvAsync();
            var fileName = $"Personen_index_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";

            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Ошибка экспорта: {ex.Message}" });
        }
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportFromCsv(IFormFile file, [FromForm] bool updateExisting = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Файл не выбран или пуст" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Неверный формат файла. Требуется CSV" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var (success, message, importedCount) = await _clientService.ImportFromCsvAsync(stream, updateExisting);

            if (success)
            {
                return Ok(new { success = true, message, importedCount });
            }
            else
            {
                return BadRequest(new { success = false, error = message });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Ошибка импорта: {ex.Message}" });
        }
    }
}
