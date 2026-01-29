using MediatR;
using Microsoft.AspNetCore.Mvc;
using QIMy.Application.Suppliers.Commands.CreateSupplier;
using QIMy.Application.Suppliers.Commands.DeleteSupplier;
using QIMy.Application.Suppliers.Commands.ImportSuppliers;
using QIMy.Application.Suppliers.Commands.UpdateSupplier;
using QIMy.Application.Suppliers.Queries.GetSupplierById;
using QIMy.Application.Suppliers.Queries.GetSuppliers;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(IMediator mediator, ILogger<SuppliersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all suppliers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? businessId, [FromQuery] string? searchTerm)
    {
        try
        {
            var query = new GetSuppliersQuery
            {
                BusinessId = businessId,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all suppliers");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get supplier by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var query = new GetSupplierByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create new supplier
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command)
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
            _logger.LogError(ex, "Error creating supplier");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update existing supplier
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierCommand command)
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
            _logger.LogError(ex, "Error updating supplier {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete supplier
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var command = new DeleteSupplierCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Export suppliers to CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv([FromQuery] int? businessId)
    {
        try
        {
            var query = new GetSuppliersQuery { BusinessId = businessId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null)
            {
                return BadRequest(new { error = "Failed to retrieve suppliers" });
            }

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };
            using var csv = new CsvWriter(writer, config);

            // Write header
            csv.WriteField("CompanyName");
            csv.WriteField("ContactPerson");
            csv.WriteField("Email");
            csv.WriteField("Phone");
            csv.WriteField("Address");
            csv.WriteField("City");
            csv.WriteField("PostalCode");
            csv.WriteField("Country");
            csv.WriteField("TaxNumber");
            csv.WriteField("VatNumber");
            csv.WriteField("BankAccount");
            await csv.NextRecordAsync();

            // Write data
            foreach (var supplier in result.Value)
            {
                csv.WriteField(supplier.CompanyName);
                csv.WriteField(supplier.ContactPerson);
                csv.WriteField(supplier.Email);
                csv.WriteField(supplier.Phone);
                csv.WriteField(supplier.Address);
                csv.WriteField(supplier.City);
                csv.WriteField(supplier.PostalCode);
                csv.WriteField(supplier.Country);
                csv.WriteField(supplier.TaxNumber);
                csv.WriteField(supplier.VatNumber);
                csv.WriteField(supplier.BankAccount);
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
            var csvBytes = memoryStream.ToArray();
            var fileName = $"Suppliers_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";

            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting suppliers to CSV");
            return BadRequest(new { error = $"Export error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Import suppliers from CSV
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportFromCsv(IFormFile file, int businessId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File not selected or empty" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Invalid file format. CSV required" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var command = new ImportSuppliersCommand
            {
                BusinessId = businessId,
                FileStream = stream,
                FileName = file.FileName
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing suppliers from CSV");
            return BadRequest(new { error = $"Import error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Bulk delete suppliers
    /// </summary>
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest(new { error = "No supplier IDs provided" });
        }

        try
        {
            var results = new List<object>();
            int successCount = 0;
            int failCount = 0;

            foreach (var id in ids)
            {
                var command = new DeleteSupplierCommand(id);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    results.Add(new { id, success = false, error = result.Error });
                }
            }

            return Ok(new
            {
                success = true,
                successCount,
                failCount,
                total = ids.Count,
                errors = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk delete");
            return BadRequest(new { error = $"Bulk delete error: {ex.Message}" });
        }
    }
}
