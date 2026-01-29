using Microsoft.AspNetCore.Mvc;
using QIMy.Infrastructure.Services;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly PersonenIndexImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        PersonenIndexImportService importService,
        ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// Import data from Personen Index Excel file
    /// </summary>
    /// <param name="file">Excel file (.xlsx)</param>
    /// <returns>Import result with statistics</returns>
    [HttpPost("personen-index")]
    public async Task<IActionResult> ImportPersonenIndex(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only .xlsx files are supported" });
        }

        try
        {
            // Save uploaded file temporarily
            var tempPath = Path.Combine(Path.GetTempPath(), $"personen_index_{Guid.NewGuid()}.xlsx");

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Starting Personen Index import from {FileName}", file.FileName);

            // Import from Excel
            var result = await _importService.ImportFromExcelAsync(tempPath);

            // Clean up temp file
            System.IO.File.Delete(tempPath);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Personen Index import completed with errors: {ErrorCount}", result.Errors.Count);
                return Ok(new
                {
                    success = false,
                    countriesImported = result.CountriesImported,
                    countriesUpdated = result.CountriesUpdated,
                    euDataImported = result.EuDataImported,
                    euDataUpdated = result.EuDataUpdated,
                    errors = result.Errors,
                    summary = result.Summary
                });
            }

            _logger.LogInformation("Personen Index import completed successfully: {Summary}", result.Summary);

            return Ok(new
            {
                success = true,
                countriesImported = result.CountriesImported,
                countriesUpdated = result.CountriesUpdated,
                euDataImported = result.EuDataImported,
                euDataUpdated = result.EuDataUpdated,
                errors = result.Errors,
                summary = result.Summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Personen Index import");
            return StatusCode(500, new { error = $"Import failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Import from default Personen Index file location
    /// </summary>
    [HttpPost("personen-index/default")]
    public async Task<IActionResult> ImportPersonenIndexDefault()
    {
        var defaultPath = @"c:\Projects\QIMy\tabellen\Personen index.xlsx";

        if (!System.IO.File.Exists(defaultPath))
        {
            return NotFound(new { error = $"Default file not found: {defaultPath}" });
        }

        try
        {
            _logger.LogInformation("Starting Personen Index import from default location: {Path}", defaultPath);

            var result = await _importService.ImportFromExcelAsync(defaultPath);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Personen Index import completed with errors: {ErrorCount}", result.Errors.Count);
                return Ok(new
                {
                    success = false,
                    countriesImported = result.CountriesImported,
                    countriesUpdated = result.CountriesUpdated,
                    euDataImported = result.EuDataImported,
                    euDataUpdated = result.EuDataUpdated,
                    errors = result.Errors,
                    summary = result.Summary
                });
            }

            _logger.LogInformation("Personen Index import completed successfully: {Summary}", result.Summary);

            return Ok(new
            {
                success = true,
                countriesImported = result.CountriesImported,
                countriesUpdated = result.CountriesUpdated,
                euDataImported = result.EuDataImported,
                euDataUpdated = result.EuDataUpdated,
                errors = result.Errors,
                summary = result.Summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Personen Index import");
            return StatusCode(500, new { error = $"Import failed: {ex.Message}" });
        }
    }
}
