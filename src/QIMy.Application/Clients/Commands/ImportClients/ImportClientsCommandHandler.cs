using System.Globalization;
using System.Linq;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Core.DTOs;
using QIMy.AI.Services;

namespace QIMy.Application.Clients.Commands.ImportClients;

public class ImportClientsCommandHandler : IRequestHandler<ImportClientsCommand, ImportResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportClientsCommandHandler> _logger;
    private readonly IAiEncodingDetectionService _aiEncoding;

    public ImportClientsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ImportClientsCommandHandler> logger,
        IAiEncodingDetectionService aiEncoding)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _aiEncoding = aiEncoding;
    }

    public async Task<ImportResult> Handle(ImportClientsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== НАЧАЛО ИМПОРТА КЛИЕНТОВ ===");
        _logger.LogInformation("Размер потока: {FileSize} байт, Пропускать ошибки: {SkipErrors}",
            request.FileStream.Length, request.SkipErrors);

        var startTime = DateTime.UtcNow;
        if (!request.BusinessId.HasValue || request.BusinessId.Value <= 0)
        {
            throw new InvalidOperationException("BusinessId is required for client import.");
        }

        var businessId = request.BusinessId.Value;
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

                // 🚫 FILTER: Skip supplier codes (300000-399999)
                if (clientCode >= 300000 && clientCode <= 399999)
                {
                    _logger.LogDebug("⏩ Строка {RowNumber}: Код {ClientCode} - это поставщик, пропускаем",
                        dto.RowNumber, clientCode);
                    result.SkippedCount++;
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
                    // Опционально: подбираем валюту по коду
                    int? currencyId = null;
                    if (!string.IsNullOrWhiteSpace(dto.Currency))
                    {
                        var currency = (await _unitOfWork.Currencies.FindAsync(
                            c => c.Code == dto.Currency, cancellationToken)).FirstOrDefault();
                        currencyId = currency?.Id;
                    }

                    // Платежные условия: пробуем распарсить в дни, иначе дефолт 30
                    var paymentTermsDays = 30;
                    if (!string.IsNullOrWhiteSpace(dto.PaymentTerms) &&
                        int.TryParse(dto.PaymentTerms, out var parsedTerms))
                    {
                        paymentTermsDays = parsedTerms;
                    }

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
                        ClientAreaId = null,  // TODO: map by country/area code if потребуется
                        ClientTypeId = null,  // TODO: map by type code if потребуется
                        BusinessId = businessId,
                        CurrencyId = currencyId,
                        PaymentTermsDays = paymentTermsDays,
                        Notes = dto.Description,
                        CustomField01 = dto.ExternalAccountNumber,
                        CustomField02 = dto.DiscountPercent,
                        CustomField03 = dto.DiscountDays,
                        CustomField04 = dto.AccountNumber,
                        CustomField05 = dto.SupplierSuggestedAccount,
                        CustomField06 = dto.Branch
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
        // 🤖 AI-enhanced encoding detection
        var encoding = await DetectEncodingAsync(stream);
        stream.Position = 0; // Reset after detection
        _logger.LogInformation("✅ Кодировка определена: {EncodingName}", encoding.EncodingName);

        var clients = new List<ClientImportDto>();

        using var reader = new StreamReader(stream, encoding);

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

            string GetPart(int idx) => idx < parts.Length ? parts[idx].Trim() : string.Empty;

            var dto = new ClientImportDto
            {
                RowNumber = rowNumber,
                CountryCode = GetPart(0), // Freifeld 01 по BMD может быть ISO, оставляем как есть
                ExternalAccountNumber = GetPart(0),
                ClientCode = GetPart(1),
                CompanyName = GetPart(2),
                Country = string.IsNullOrWhiteSpace(GetPart(3)) ? "Österreich" : GetPart(3),
                Address = GetPart(4),
                PostalCode = GetPart(5),
                City = GetPart(6),
                Currency = GetPart(7),
                PaymentTerms = GetPart(8),
                DiscountPercent = GetPart(9),
                DiscountDays = GetPart(10),
                VatNumber = GetPart(11),
                FreeField11 = GetPart(12),
                SupplierSuggestedAccount = GetPart(13),
                FreeField04 = GetPart(14),
                FreeField05 = GetPart(15),
                AccountNumber = GetPart(16),
                FreeField02 = GetPart(17),
                FreeField03 = GetPart(18),
                Branch = GetPart(19),
                CountryNumber = GetPart(20),
                Description = GetPart(21)
            };

            clients.Add(dto);
            parsedRows++;
            rowNumber++;
        }

        _logger.LogInformation("Парсинг завершён: распознано {ParsedRows} строк данных (строки {StartRow}-{EndRow})",
            parsedRows, 2, rowNumber - 1);
        return clients;
    }

    private async Task<Encoding> DetectEncodingAsync(Stream stream)
    {
        _logger.LogInformation("🤖 AI Encoding Detection начат...");

        try
        {
            var detectionResult = await _aiEncoding.DetectEncodingAsync(stream);

            _logger.LogInformation(
                "🤖 AI определил кодировку: {Encoding} (Confidence: {Confidence:P}, Method: {Method})",
                detectionResult.Encoding.EncodingName,
                detectionResult.Confidence,
                detectionResult.DetectionMethod);

            if (detectionResult.Alternatives.Any())
            {
                _logger.LogDebug("Альтернативные варианты: {Alternatives}",
                    string.Join(", ", detectionResult.Alternatives
                        .Select(a => $"{a.Encoding.EncodingName} ({a.Confidence:P})")));
            }

            // Log warning if confidence is low
            if (detectionResult.Confidence < 0.7m)
            {
                _logger.LogWarning(
                    "⚠️ Низкий confidence score ({Confidence:P}). Рекомендуется проверить результат.",
                    detectionResult.Confidence);
            }

            return detectionResult.Encoding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка AI encoding detection, использую fallback");

            // Fallback to old method
            return DetectEncodingFallback(stream);
        }
    }

    private Encoding DetectEncodingFallback(Stream stream)
    {
        // Read first 4 bytes to check for BOM
        var bom = new byte[4];
        var bytesRead = stream.Read(bom, 0, 4);
        stream.Position = 0;

        // Check for UTF-8 BOM (EF BB BF)
        if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        // Check for UTF-16 LE BOM (FF FE)
        if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        // Check for UTF-16 BE BOM (FE FF)
        if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        // Default to Windows-1252 (common for BMD/Austrian accounting software)
        return Encoding.GetEncoding("windows-1252");
    }
}
