# Session Log - 26 января 2026: Document Management System Planning

**Дата:** 26.01.2026
**Тип сессии:** Архитектурное планирование
**Статус:** Отложено для будущей реализации

---

## КОНТЕКСТ СЕССИИ

### Предыдущая работа
1. ✅ Исправлена мультитенантность (BusinessId везде)
2. ✅ Очищена БД от тестовых данных
3. ✅ Проанализирована папка BKHA GmbH для импорта
4. ✅ Созданы скрипты подготовки CSV:
   - `PrepareClientsImport.ps1` → 1 клиент готов
   - `PrepareSuppliersImport.ps1` → 9 поставщиков готово

### Новое требование
**Запрос пользователя:**
> "я хочу сделать так чтобы в QIMy, была такая функция - я загружаю массив данных (как только что - C:\Projects\QIMy\tabellen\BKHA GmbH), а QIMy - создаёт все таблицы для импорта и импортирует данные вместе с документами для просмотра"

**Пример использования:**
- Папка `FA und ZOLL` → документ `Bescheid UID Nummer.pdf`
- Система извлекает UID номер (ATU12345678)
- Сохраняет документ в систему
- В поле Business.VatNumber добавляет иконку 📄
- Клик на иконку → просмотр PDF документа

**Цель:** Полноценная система управления документами (DMS) с умным импортом

---

## СОЗДАННАЯ АРХИТЕКТУРА

### Документ
📄 **DOCUMENT_MANAGEMENT_SYSTEM_PLAN.md** (6500+ строк)

### Ключевые компоненты

#### 1. Database Schema
```sql
-- Основная таблица документов
Documents (Id, FileName, ContentType, FileSizeBytes, FileData/StoragePath,
           DocumentType, ExtractedText, ExtractedDataJson, BusinessId)

-- Связь документов с сущностями (полиморфная)
DocumentAttachments (DocumentId, EntityType, EntityId, FieldName)

-- Версионирование
DocumentVersions (DocumentId, VersionNumber, FileData, ChangeComment)
```

#### 2. Domain Entities
- `Document.cs` - основная сущность документа
- `DocumentAttachment.cs` - связь документ ↔ сущность
- `DocumentVersion.cs` - история изменений
- `DocumentStorageType` enum (Database/FileSystem)

#### 3. Services
```csharp
IDocumentService - Upload, Download, Attach, Delete, Archive
IDocumentParserService - Extract text/fields from PDF
ISmartImportService - Analyze folder, Import entire structure
```

#### 4. Smart Import Rules
**Folder Detection:**
- `FA und ZOLL` → Tax & Customs documents
- `STAMM` → Company master data
- `BANK` → Banking documents
- `BH` → Accounting data (CSV)
- `Rechnungen` → Invoices

**Document Type Patterns:**
- Filename contains "UID" → UIDCertificate → Extract VatNumber
- Filename contains "EORI" → EORICertificate → Extract CustomsNumber
- Filename contains "IBAN" → IBANCertificate → Extract BankAccount
- Filename contains "Firmenbuch" → CompanyRegistration → Extract FN

**Field Extraction (Regex):**
```regex
UID: ATU\d{8}
EORI: AT[A-Z0-9]+
FN: FN\s*\d+\w?
IBAN: AT\d{2}\s*\d{4}\s*\d{4}\s*\d{4}\s*\d{4}
```

#### 5. UI Components
```razor
<DocumentUploadComponent />          // Drag & Drop upload
<DocumentFieldAttachment />          // 📄 icon next to field
<DocumentViewerModal />              // PDF/Image viewer
/Admin/SmartImport/Index.razor       // Folder import wizard
```

#### 6. CQRS Commands
- `UploadDocumentCommand` - загрузка файла
- `AttachDocumentCommand` - привязка к сущности/полю
- `ImportFolderCommand` - импорт всей папки

---

## IMPLEMENTATION ROADMAP

### Phase 1: Core DMS (1-2 дня)
- [ ] Create Document entities (Document, DocumentAttachment, DocumentVersion)
- [ ] Database migration
- [ ] DocumentService implementation
- [ ] API endpoints (Upload, Download, Delete)
- [ ] Basic UI components

### Phase 2: Document Parsing (1 день)
- [ ] Integrate PDF parser (iTextSharp/PdfPig)
- [ ] DocumentParserService
- [ ] Regex patterns for field extraction
- [ ] Test on real PDFs from BKHA GmbH

### Phase 3: UI Integration (1 день)
- [ ] DocumentFieldAttachment component
- [ ] Add 📄 icons to Business/Client/Supplier forms
- [ ] DocumentViewerModal
- [ ] Document list on each entity page

### Phase 4: Smart Import (2-3 дня)
- [ ] FolderAnalyzer service
- [ ] SmartImportService implementation
- [ ] ImportFolderCommand + Handler
- [ ] Smart Import wizard UI
- [ ] Test with BKHA GmbH folder

### Phase 5: Advanced Features (1-2 дня)
- [ ] Document versioning
- [ ] Full-text search (ExtractedText)
- [ ] Archive old documents
- [ ] Document permissions

**Total estimate:** 7-9 дней работы

---

## ТЕХНОЛОГИИ

### Backend
- **PDF Parser:** iTextSharp 8.x или PdfPig
- **OCR (optional):** Tesseract.NET для сканов
- **Storage:** Database BLOB (SQLite) → FileSystem later

### Frontend
- **File Upload:** Blazor InputFile
- **Drag & Drop:** HTML5 API
- **PDF Viewer:** `<iframe>` с data URL или PDF.js
- **Icons:** Unicode 📄📎📋

### Configuration
- **File Size Limit:** 50 MB per file
- **Allowed Types:** PDF, JPG, PNG, CSV, XLSX
- **Storage Path:** `wwwroot/uploads/{businessId}/{year}/{month}/`

---

## ПРИМЕР ИСПОЛЬЗОВАНИЯ

### Scenario: Import BKHA GmbH folder

```csharp
// 1. User selects folder: C:\Projects\QIMy\tabellen\BKHA GmbH
var command = new ImportFolderCommand(
    FolderPath: @"C:\Projects\QIMy\tabellen\BKHA GmbH",
    BusinessId: 2
);

// 2. System analyzes structure:
// FA und ZOLL/
//   ├── Bescheid UID Nummer.pdf → Detect: UIDCertificate
//   └── EORI-Antrag.pdf → Detect: EORICertificate
// BH/
//   ├── PK 2025.csv → Detect: Personenkonten
//   └── Sachkonten 2025.csv → Detect: ChartOfAccounts

// 3. System processes:
// - Upload "Bescheid UID Nummer.pdf"
// - Extract text with PDF parser
// - Find "ATU12345678" with regex
// - Update Business.VatNumber = "ATU12345678"
// - Create DocumentAttachment(EntityType: "Business", EntityId: 2, FieldName: "VatNumber")
// - Import PK 2025.csv → 1 Client + 9 Suppliers
// - Import Sachkonten 2025.csv → 92 Accounts

// 4. Result:
var result = await _mediator.Send(command);
// result.ClientsImported = 1
// result.SuppliersImported = 9
// result.AccountsImported = 92
// result.DocumentsUploaded = 6
// result.AttachmentsCreated = 6
```

### UI After Import

**Business Edit Page:**
```
┌─────────────────────────────────────┐
│ Company Name: BKHA GmbH             │
│ UID Number:   ATU12345678  [📄]     │ ← Click → View PDF
│ EORI:         AT123456789  [📄]     │ ← Click → View PDF
│ IBAN:         AT61...      [📄]     │ ← Click → View PDF
└─────────────────────────────────────┘
```

---

## SECURITY CONSIDERATIONS

```csharp
// Document access control
public enum DocumentPermission
{
    ViewDocument,
    UploadDocument,
    DeleteDocument,
    ArchiveDocument,
    ViewAllBusinessDocuments  // Admin only
}

// Handler validation
if (document.BusinessId != currentBusinessId)
    throw new UnauthorizedBusinessAccessException("Document", documentId);
```

---

## OPEN QUESTIONS

1. **Storage strategy:** Database BLOB vs FileSystem?
   - **Recommendation:** FileSystem для файлов >1MB
   - Database только для метаданных

2. **OCR needed now?**
   - **Recommendation:** Phase 2, optional для сканов без текста

3. **Document versioning mandatory in Phase 1?**
   - **Recommendation:** Phase 5, не критично для MVP

4. **Max file size: 50 MB enough?**
   - **Recommendation:** 50 MB достаточно для PDF/images

5. **Start implementation now or finish manual BKHA import first?**
   - **Decision:** Отложено (пользователь: "оставим пока")

---

## РЕШЕНИЕ ПОЛЬЗОВАТЕЛЯ

**Статус:** ⏸️ ОТЛОЖЕНО

**Цитата:**
> "оставим пока, но запомни план - создай логи"

**Next steps:**
1. ✅ План сохранён в DOCUMENT_MANAGEMENT_SYSTEM_PLAN.md
2. ✅ Session log создан
3. ⏳ Вернуться к ручному импорту BKHA GmbH:
   - Import Clients_BKHA_Import.csv (1 client)
   - Import Suppliers_BKHA_Import.csv (9 suppliers)
   - Extract company details from PDFs manually
   - Import Sachkonten (92 accounts)

**Future implementation:** Когда система стабильна, вернуться к DMS (7-9 дней работы)

---

## СВЯЗАННЫЕ ДОКУМЕНТЫ

- 📄 [DOCUMENT_MANAGEMENT_SYSTEM_PLAN.md](DOCUMENT_MANAGEMENT_SYSTEM_PLAN.md) - Full architecture
- 📄 [BKHA_IMPORT_PLAN.md](BKHA_IMPORT_PLAN.md) - Current manual import plan
- 📄 [AI_CONTEXT.md](AI_CONTEXT.md) - System context
- 📄 [INDEX_AI_MEMORY_SYSTEM.md](INDEX_AI_MEMORY_SYSTEM.md) - Memory index

---

## ВЫВОДЫ

1. **Архитектура готова:** Полный план из 13 разделов с кодом
2. **Scope понятен:** 5 фаз, 7-9 дней разработки
3. **Технологии выбраны:** iTextSharp + Blazor InputFile + FileSystem storage
4. **Приоритет:** Отложено, сначала доделать базовый функционал

**Рекомендация:** Вернуться к DMS после:
- Завершения CQRS миграции всех модулей
- Стабилизации мультитенантности
- Импорта первых реальных данных (BKHA GmbH)
- Тестирования базового функционала с пользователями
