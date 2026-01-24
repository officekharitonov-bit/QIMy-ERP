# Suppliers Module - API Documentation

## Обзор

Модуль Suppliers реализует полный CQRS функционал для управления поставщиками с проверкой дубликатов.

## Архитектура

### Application Layer

```
QIMy.Application/Suppliers/
├── Commands/
│   ├── CreateSupplier/
│   │   ├── CreateSupplierCommand.cs
│   │   ├── CreateSupplierCommandHandler.cs
│   │   └── CreateSupplierCommandValidator.cs
│   ├── UpdateSupplier/
│   │   ├── UpdateSupplierCommand.cs
│   │   ├── UpdateSupplierCommandHandler.cs
│   │   └── UpdateSupplierCommandValidator.cs
│   ├── DeleteSupplier/
│   │   ├── DeleteSupplierCommand.cs
│   │   └── DeleteSupplierCommandHandler.cs
│   └── ImportSuppliers/
│       ├── ImportSuppliersCommand.cs
│       └── ImportSuppliersCommandHandler.cs
├── Queries/
│   ├── GetSuppliers/
│   │   ├── GetSuppliersQuery.cs
│   │   └── GetSuppliersQueryHandler.cs
│   └── GetSupplierById/
│       ├── GetSupplierByIdQuery.cs
│       └── GetSupplierByIdQueryHandler.cs
└── DTOs/
    └── SupplierDto.cs
```

### API Endpoints

#### GET /api/suppliers
Получить список всех поставщиков с фильтрацией

**Query Parameters:**
- `businessId` (int, optional) - Фильтр по бизнесу
- `searchTerm` (string, optional) - Поиск по названию, контакту, email, VAT

**Response:**
```json
[
  {
    "id": 1,
    "businessId": 1,
    "companyName": "ООО Поставщик",
    "contactPerson": "Иван Иванов",
    "email": "info@supplier.com",
    "phone": "+43 123 456789",
    "address": "Улица 123",
    "city": "Вена",
    "postalCode": "1010",
    "country": "Австрия",
    "taxNumber": "123456789",
    "vatNumber": "ATU12345678",
    "bankAccount": "AT123456789012345678",
    "createdAt": "2026-01-24T10:00:00Z",
    "updatedAt": null
  }
]
```

#### GET /api/suppliers/{id}
Получить поставщика по ID

**Response:**
```json
{
  "id": 1,
  "businessId": 1,
  "companyName": "ООО Поставщик",
  ...
}
```

#### POST /api/suppliers
Создать нового поставщика

**Request Body:**
```json
{
  "businessId": 1,
  "companyName": "ООО Новый Поставщик",
  "contactPerson": "Петр Петров",
  "email": "contact@newsupplier.com",
  "phone": "+43 987 654321",
  "address": "Улица 456",
  "city": "Зальцбург",
  "postalCode": "5020",
  "country": "Австрия",
  "taxNumber": "987654321",
  "vatNumber": "ATU98765432",
  "bankAccount": "AT987654321098765432",
  "ignoreDuplicateWarning": false,
  "doubleConfirmed": false
}
```

**Проверка дубликатов (двухэтапное подтверждение):**

1. **Первая попытка** (без флагов):
```json
{
  "isSuccess": false,
  "error": "A supplier with similar details already exists. If you want to proceed, set IgnoreDuplicateWarning=true and DoubleConfirmed=true to confirm. Duplicate details: Similar company name found: 'ООО Поставщик' (Id: 1)"
}
```

2. **Вторая попытка** (с `ignoreDuplicateWarning: true`):
```json
{
  "businessId": 1,
  "companyName": "ООО Поставщик",
  ...,
  "ignoreDuplicateWarning": true,
  "doubleConfirmed": false
}
```

Ответ:
```json
{
  "isSuccess": false,
  "error": "Please confirm that you want to create a duplicate supplier by setting DoubleConfirmed=true. Duplicate details: ..."
}
```

3. **Третья попытка** (с `doubleConfirmed: true`):
```json
{
  "businessId": 1,
  "companyName": "ООО Поставщик",
  ...,
  "ignoreDuplicateWarning": true,
  "doubleConfirmed": true
}
```

Ответ: 201 Created

#### PUT /api/suppliers/{id}
Обновить поставщика (с той же логикой проверки дубликатов)

**Request Body:** (то же что и POST, плюс `id`)

#### DELETE /api/suppliers/{id}
Удалить поставщика

**Response:** 
- 204 No Content - успешно
- 400 Bad Request - если есть связанные расходные накладные

#### GET /api/suppliers/export
Экспортировать поставщиков в CSV

**Query Parameters:**
- `businessId` (int, optional) - Фильтр по бизнесу

**Response:** CSV файл `Suppliers_2026-01-24_12-30-00.csv`

#### POST /api/suppliers/import
Импортировать поставщиков из CSV

**Request:**
- Form Data:
  - `file` (IFormFile) - CSV файл
  - `businessId` (int) - ID бизнеса для всех импортируемых поставщиков

**CSV Format:** (разделитель: `;`)
```
CompanyName;ContactPerson;Email;Phone;Address;City;PostalCode;Country;TaxNumber;VatNumber;BankAccount
ООО Поставщик;Иван Иванов;info@supplier.com;+43 123 456789;Улица 123;Вена;1010;Австрия;123456789;ATU12345678;AT123456789012345678
```

**Поддерживаемые названия колонок:**
- CompanyName / Company / Name / Firma
- ContactPerson / Contact / Kontakt
- Email / E-Mail
- Phone / Telefon / Tel
- Address / Adresse
- City / Stadt / Ort
- PostalCode / PLZ / Zip
- Country / Land
- TaxNumber / Steuernummer / TaxNo
- VatNumber / UID / VAT / USt-IdNr
- BankAccount / IBAN / Bank

**Response:**
```json
{
  "totalRows": 10,
  "successCount": 8,
  "failureCount": 1,
  "duplicateCount": 1,
  "errors": [
    {
      "rowNumber": 5,
      "companyName": "ООО Дубликат",
      "errorMessage": "Duplicate: Similar company name found..."
    }
  ]
}
```

#### POST /api/suppliers/bulk-delete
Массовое удаление поставщиков

**Request Body:**
```json
[1, 2, 3, 4, 5]
```

**Response:**
```json
{
  "success": true,
  "successCount": 4,
  "failCount": 1,
  "total": 5,
  "errors": [
    {
      "id": 3,
      "success": false,
      "error": "Cannot delete supplier 'ООО Поставщик' because it has related expense invoices..."
    }
  ]
}
```

## Валидация

### CreateSupplierCommand / UpdateSupplierCommand

- `BusinessId` > 0 (обязательно)
- `CompanyName` не пустое, макс. 200 символов (обязательно)
- `Email` валидный формат email (если указан)
- `Phone` макс. 50 символов (если указан)
- `Address` макс. 500 символов (если указан)
- `City` макс. 100 символов (если указан)
- `PostalCode` макс. 20 символов (если указан)
- `Country` макс. 100 символов (если указан)
- `TaxNumber` макс. 50 символов (если указан)
- `VatNumber` макс. 50 символов (если указан)
- `BankAccount` макс. 100 символов (если указан)
- `DoubleConfirmed = true` если `IgnoreDuplicateWarning = true` (обязательно)

## Логика проверки дубликатов

### CheckSupplierDuplicateAsync (IDuplicateDetectionService)

Проверяет:
1. **Название компании** (CompanyName) - case-insensitive
2. **VAT номер** (VatNumber) - точное совпадение

**Severity:**
- **Warning** - совпадение по названию → требует двойного подтверждения
- **Warning** - совпадение по VAT → требует двойного подтверждения

## Интеграция с PersonenIndex

В будущем планируется автоматическая синхронизация:
- При создании Supplier → создается PersonenIndexEntry
- При изменении Supplier → обновляется PersonenIndexEntry
- PersonenIndexEntry.Type = "Supplier"

## Примеры использования (C# клиент)

### Создание поставщика с обработкой дубликатов

```csharp
public async Task<SupplierDto?> CreateSupplierWithDuplicateHandlingAsync(CreateSupplierCommand command)
{
    // First attempt
    var result = await mediator.Send(command);
    
    if (result.IsSuccess)
        return result.Value;
    
    // Check if it's a duplicate warning
    if (result.Error.Contains("similar details already exists"))
    {
        // Show warning to user
        var userConfirmed = await ShowDuplicateWarningAsync(result.Error);
        
        if (!userConfirmed)
            return null;
        
        // Second attempt with IgnoreDuplicateWarning
        command = command with { IgnoreDuplicateWarning = true };
        result = await mediator.Send(command);
        
        if (result.IsSuccess)
            return result.Value;
        
        // Third attempt with DoubleConfirmed
        if (result.Error.Contains("confirm that you want"))
        {
            var doubleConfirmed = await ShowFinalConfirmationAsync(result.Error);
            
            if (!doubleConfirmed)
                return null;
            
            command = command with { DoubleConfirmed = true };
            result = await mediator.Send(command);
            
            if (result.IsSuccess)
                return result.Value;
        }
    }
    
    // Show error
    await ShowErrorAsync(result.Error);
    return null;
}
```

## История изменений

- **2026-01-24** - Создан модуль Suppliers с полным CQRS функционалом
  - Commands: Create, Update, Delete, Import
  - Queries: GetAll, GetById
  - API Controller с CSV import/export
  - Проверка дубликатов через DuplicateDetectionService
  - Двухэтапное подтверждение для дубликатов
  - Bulk delete операция
