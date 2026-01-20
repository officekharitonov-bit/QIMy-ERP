using Microsoft.AspNetCore.Mvc;
using QIMy.Core.Interfaces;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
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
