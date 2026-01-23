using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Core.DTOs;

namespace QIMy.Application.Clients.Commands.ImportClients;

public class ImportClientsCommandHandler : IRequestHandler<ImportClientsCommand, ImportResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportClientsCommandHandler> _logger;

    public ImportClientsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ImportClientsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ImportResult> Handle(ImportClientsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== НАЧАЛО ИМПОРТА КЛИЕНТОВ ===");
        _logger.LogInformation("Размер потока: {FileSize} байт, Пропускать ошибки: {SkipErrors}",
            request.FileStream.Length, request.SkipErrors);

        var startTime = DateTime.UtcNow;
        var result = new ImportResult { ImportedAt = startTime };

        try
        {
            _logger.LogDebug("Этап 1: Подготовка потока данных");
            // Ensure we have a readable, seekable stream
            Stream inputStream;
            if (request.FileStream.CanSeek)
            {
                _logger.LogDebug("Поток поддерживает Seek, используем напрямую");
                request.FileStream.Position = 0;
                inputStream = request.FileStream;
            }
            else
            {
                _logger.LogDebug("Поток не поддерживает Seek, копируем в MemoryStream");
                // Copy to memory to allow parsing from the beginning
                inputStream = new MemoryStream();
                await request.FileStream.CopyToAsync(inputStream, cancellationToken);
                inputStream.Position = 0;
                _logger.LogDebug("Скопировано {Size} байт в память", inputStream.Length);
            }

            _logger.LogDebug("Этап 2: Парсинг CSV файла");
            // Parse CSV
            var clients = await ParseCsvAsync(inputStream, cancellationToken);
            result.TotalRows = clients.Count;
            _logger.LogInformation("Распарсено строк: {TotalRows}", clients.Count);

            _logger.LogDebug("Этап 3: Валидация и сохранение клиентов");
            var existingCodes = new HashSet<int>();
            var processedCount = 0;

            foreach (var dto in clients)
            {
                processedCount++;
                _logger.LogDebug("Обработка строки {RowNumber}/{TotalRows}: Код={ClientCode}, Компания={CompanyName}",
                    dto.RowNumber, clients.Count, dto.ClientCode, dto.CompanyName);

                // Validate
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(dto.CompanyName))
                {
                    errors.Add("CompanyName is required");
                    _logger.LogWarning("Строка {RowNumber}: Отсутствует название компании", dto.RowNumber);
                }

                if (!int.TryParse(dto.ClientCode, out var clientCode))
                {
                    errors.Add("ClientCode must be a valid integer");
                    _logger.LogWarning("Строка {RowNumber}: Неверный формат кода клиента '{ClientCode}'", dto.RowNumber, dto.ClientCode);
                }

                if (errors.Any())
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = dto.RowNumber,
                        ClientCode = dto.ClientCode,
                        CompanyName = dto.CompanyName,
                        ErrorMessage = "Validation failed",
                        Details = errors.ToArray()
                    });
                    _logger.LogError("❌ Ошибка валидации строки {RowNumber}: {Errors}", dto.RowNumber, string.Join(", ", errors));

                    if (!request.SkipErrors)
                    {
                        _logger.LogError("Импорт остановлен из-за ошибки (SkipErrors=false)");
                        throw new InvalidOperationException($"Import stopped at row {dto.RowNumber}");
                    }
                    continue;
                }

                // Check for duplicates in current import
                if (existingCodes.Contains(clientCode))
                {
                    result.SkippedCount++;
                    _logger.LogWarning("⚠️ Дубликат в импорте - код {ClientCode} уже встречался", clientCode);
                    continue;
                }

                // Check database
                _logger.LogDebug("Проверка существования клиента {ClientCode} в БД", clientCode);
                var existing = await _unitOfWork.Clients.FindAsync(
                    c => c.ClientCode == clientCode && !c.IsDeleted, cancellationToken);

                if (existing.Any())
                {
                    result.SkippedCount++;
                    _logger.LogWarning("⚠️ Клиент {ClientCode} уже существует в БД, пропускаем", clientCode);
                    continue;
                }

                try
                {
                    _logger.LogDebug("Создание клиента {ClientCode} - {CompanyName}", clientCode, dto.CompanyName);
                    // Create client
                    var client = new Core.Entities.Client
                    {
                        ClientCode = clientCode,
                        CompanyName = dto.CompanyName ?? string.Empty,
                        ContactPerson = dto.ContactPerson,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        VatNumber = dto.VatNumber,
                        Address = dto.Address,
                        PostalCode = dto.PostalCode,
                        City = dto.City,
                        Country = dto.Country ?? "Österreich",
                        TaxNumber = dto.TaxNumber,
                        ClientAreaId = null,  // Would need to look up by code
                        ClientTypeId = null   // Would need to look up by code
                    };

                    await _unitOfWork.Clients.AddAsync(client, cancellationToken);
                    existingCodes.Add(clientCode);
                    result.SuccessCount++;

                    _logger.LogInformation("✅ Импортирован клиент #{SuccessCount}: {ClientCode} - {CompanyName}",
                        result.SuccessCount, clientCode, client.CompanyName);
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = dto.RowNumber,
                        ClientCode = dto.ClientCode,
                        CompanyName = dto.CompanyName,
                        ErrorMessage = ex.Message
                    });
                    _logger.LogError(ex, "❌ Ошибка при создании клиента {ClientCode} (строка {RowNumber}): {Message}",
                        dto.ClientCode, dto.RowNumber, ex.Message);

                    if (!request.SkipErrors)
                    {
                        _logger.LogError("Импорт остановлен из-за ошибки (SkipErrors=false)");
                        throw;
                    }
                }
            }

            _logger.LogInformation("Этап 4: Сохранение изменений в БД");
            // Save all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("✅ Изменения успешно сохранены в БД");

            _logger.LogInformation("=== ИМПОРТ ЗАВЕРШЁН ===\n" +
                "  ✅ Успешно: {SuccessCount}\n" +
                "  ❌ Ошибки: {ErrorCount}\n" +
                "  ⚠️ Пропущено: {SkippedCount}\n" +
                "  📊 Всего строк: {TotalRows}",
                result.SuccessCount, result.ErrorCount, result.SkippedCount, result.TotalRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ КРИТИЧЕСКАЯ ОШИБКА ИМПОРТА: {Message}\nStackTrace: {StackTrace}",
                ex.Message, ex.StackTrace);
            result.ErrorCount++;
            result.Errors.Add(new ImportError
            {
                ErrorMessage = $"Import failed: {ex.Message}"
            });
        }

        // Set duration
        result.Duration = DateTime.UtcNow - startTime;
        return result;
    }

    private async Task<List<ClientImportDto>> ParseCsvAsync(Stream stream, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Начало парсинга CSV (кодировка: windows-1252)");
        var clients = new List<ClientImportDto>();

        using var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1252"));

        // Skip header
        var header = await reader.ReadLineAsync();
        _logger.LogDebug("Заголовок CSV: {Header}", header);

        int rowNumber = 2;
        int parsedRows = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                _logger.LogDebug("Строка {RowNumber}: пустая, пропускаем", rowNumber);
                continue;
            }

            var parts = line.Split(';');
            if (parts.Length < 3)
            {
                _logger.LogWarning("Строка {RowNumber}: недостаточно колонок ({Count}), пропускаем", rowNumber, parts.Length);
                continue;
            }

            var dto = new ClientImportDto
            {
                RowNumber = rowNumber,
                CountryCode = parts[0].Trim(),
                ClientCode = parts[1].Trim(),
                CompanyName = parts[2].Trim(),
                Country = parts.Length > 3 ? parts[3].Trim() : "Österreich",
                Address = parts.Length > 4 ? parts[4].Trim() : null,
                PostalCode = parts.Length > 5 ? parts[5].Trim() : null,
                City = parts.Length > 6 ? parts[6].Trim() : null,
                VatNumber = parts.Length > 7 ? parts[7].Trim() : null,
                Email = parts.Length > 8 ? parts[8].Trim() : null,
                Phone = parts.Length > 9 ? parts[9].Trim() : null,
                ContactPerson = parts.Length > 10 ? parts[10].Trim() : null,
                TaxNumber = parts.Length > 11 ? parts[11].Trim() : null
            };

            clients.Add(dto);
            parsedRows++;
            rowNumber++;
        }

        _logger.LogInformation("Парсинг завершён: распознано {ParsedRows} строк данных (строки {StartRow}-{EndRow})",
            parsedRows, 2, rowNumber - 1);
        return clients;
    }
}
