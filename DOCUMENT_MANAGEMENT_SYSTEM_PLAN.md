# Document Management System (DMS) для QIMy
## Архитектура системы архива и умного импорта

Дата: 26.01.2026
Статус: Планирование

---

## 1. ЦЕЛЬ СИСТЕМЫ

Создать интегрированную систему управления документами, которая:

1. **Умный импорт папки с данными**
   - Загрузка папки (например, `C:\Projects\QIMy\tabellen\BKHA GmbH`)
   - Автоматический анализ структуры папки
   - Распознавание типов файлов (CSV, PDF, Excel, Images)
   - Извлечение данных из документов (OCR/парсинг PDF)
   - Автоматическое создание записей в БД

2. **Связывание документов с данными**
   - `FA und ZOLL/Bescheid UID Nummer.pdf` → Business.VatNumber
   - `STAMM/Datenblatt.pdf` → Business Address/Contact
   - `BANK/IBAN.pdf` → Business.BankAccount
   - Invoice PDF → Invoice entity
   - Contract PDF → Client/Supplier entity

3. **CRUD операции с документами**
   - Загрузка (Upload)
   - Просмотр (View/Preview)
   - Скачивание (Download)
   - Редактирование метаданных
   - Удаление
   - Версионирование

4. **UI интеграция**
   - Иконка 📄 рядом с полем (UID Number 📄)
   - Клик → просмотр документа
   - Drag & Drop загрузка
   - Список прикреплённых документов

---

## 2. АРХИТЕКТУРА БАЗЫ ДАННЫХ

### 2.1. Таблица Documents (основная)

```sql
CREATE TABLE Documents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName NVARCHAR(500) NOT NULL,
    OriginalFileName NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(200) NOT NULL,  -- application/pdf, image/jpeg
    FileSizeBytes BIGINT NOT NULL,
    StorageType NVARCHAR(50) NOT NULL,   -- 'Database' or 'FileSystem'
    
    -- For Database storage:
    FileData BLOB NULL,
    
    -- For FileSystem storage:
    StoragePath NVARCHAR(1000) NULL,
    
    UploadedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UploadedByUserId INTEGER NOT NULL,
    
    -- Document metadata
    DocumentType NVARCHAR(100) NULL,     -- 'UID Certificate', 'IBAN', 'Invoice', 'Contract'
    DocumentDate DATE NULL,              -- дата документа
    
    -- Extracted data (from OCR/parsing)
    ExtractedText TEXT NULL,
    ExtractedDataJson TEXT NULL,         -- JSON с распознанными полями
    
    -- Lifecycle
    IsArchived BIT NOT NULL DEFAULT 0,
    ArchivedDate DATETIME NULL,
    
    BusinessId INTEGER NOT NULL,
    FOREIGN KEY (BusinessId) REFERENCES Businesses(Id),
    FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id)
);
```

### 2.2. Таблица DocumentAttachments (связь документов с сущностями)

```sql
CREATE TABLE DocumentAttachments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DocumentId INTEGER NOT NULL,
    
    -- Generic relation (polymorphic)
    EntityType NVARCHAR(100) NOT NULL,   -- 'Business', 'Client', 'Supplier', 'Invoice', 'Product'
    EntityId INTEGER NOT NULL,
    
    -- Field-specific attachment
    FieldName NVARCHAR(100) NULL,        -- 'VatNumber', 'BankAccount', 'Address'
    
    AttachedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AttachedByUserId INTEGER NOT NULL,
    
    DisplayOrder INTEGER NOT NULL DEFAULT 0,
    Description NVARCHAR(500) NULL,
    
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (AttachedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_DocumentAttachments_Entity ON DocumentAttachments(EntityType, EntityId);
CREATE INDEX IX_DocumentAttachments_Field ON DocumentAttachments(EntityType, EntityId, FieldName);
```

### 2.3. Таблица DocumentVersions (версионирование)

```sql
CREATE TABLE DocumentVersions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DocumentId INTEGER NOT NULL,
    VersionNumber INTEGER NOT NULL,
    
    FileName NVARCHAR(500) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    FileData BLOB NULL,
    StoragePath NVARCHAR(1000) NULL,
    
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INTEGER NOT NULL,
    ChangeComment NVARCHAR(1000) NULL,
    
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);
```

---

## 3. DOMAIN ENTITIES

### 3.1. Document.cs

```csharp
public class Document : BaseAuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    
    public DocumentStorageType StorageType { get; set; }
    public byte[]? FileData { get; set; }  // For DB storage
    public string? StoragePath { get; set; }  // For FS storage
    
    public DateTime UploadedDate { get; set; }
    public int UploadedByUserId { get; set; }
    public User? UploadedBy { get; set; }
    
    public string? DocumentType { get; set; }
    public DateTime? DocumentDate { get; set; }
    
    public string? ExtractedText { get; set; }
    public string? ExtractedDataJson { get; set; }
    
    public bool IsArchived { get; set; }
    public DateTime? ArchivedDate { get; set; }
    
    public int BusinessId { get; set; }
    public Business? Business { get; set; }
    
    public ICollection<DocumentAttachment> Attachments { get; set; } = new List<DocumentAttachment>();
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}

public enum DocumentStorageType
{
    Database = 1,
    FileSystem = 2
}
```

### 3.2. DocumentAttachment.cs

```csharp
public class DocumentAttachment : BaseEntity
{
    public int DocumentId { get; set; }
    public Document? Document { get; set; }
    
    public string EntityType { get; set; } = string.Empty;  // "Business", "Client", etc.
    public int EntityId { get; set; }
    public string? FieldName { get; set; }  // "VatNumber", "BankAccount", etc.
    
    public DateTime AttachedDate { get; set; }
    public int AttachedByUserId { get; set; }
    public User? AttachedBy { get; set; }
    
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}
```

---

## 4. SERVICES ARCHITECTURE

### 4.1. IDocumentService

```csharp
public interface IDocumentService
{
    // Upload
    Task<Document> UploadDocumentAsync(Stream fileStream, string fileName, 
        string contentType, int businessId, int userId);
    
    // Attach to entity
    Task AttachDocumentAsync(int documentId, string entityType, int entityId, 
        string? fieldName, int userId);
    
    // Retrieve
    Task<Document?> GetDocumentAsync(int documentId);
    Task<byte[]> GetDocumentContentAsync(int documentId);
    Task<List<Document>> GetEntityDocumentsAsync(string entityType, int entityId);
    Task<Document?> GetFieldDocumentAsync(string entityType, int entityId, string fieldName);
    
    // Delete
    Task DeleteDocumentAsync(int documentId, int userId);
    Task DetachDocumentAsync(int attachmentId, int userId);
    
    // Archive
    Task ArchiveDocumentAsync(int documentId, int userId);
    Task UnarchiveDocumentAsync(int documentId, int userId);
}
```

### 4.2. IDocumentParserService

```csharp
public interface IDocumentParserService
{
    // PDF text extraction
    Task<string> ExtractTextFromPdfAsync(Stream pdfStream);
    
    // Smart field extraction
    Task<Dictionary<string, string>> ExtractFieldsAsync(string text, string documentType);
    
    // Examples:
    // ExtractFieldsAsync(text, "UIDCertificate") → { "VatNumber": "ATU12345678" }
    // ExtractFieldsAsync(text, "IBANCertificate") → { "IBAN": "AT611904300234573201" }
    // ExtractFieldsAsync(text, "CompanyRegistration") → { "CompanyRegistrationNumber": "FN 123456a" }
}
```

### 4.3. ISmartImportService

```csharp
public interface ISmartImportService
{
    // Analyze folder structure
    Task<FolderAnalysisResult> AnalyzeFolderAsync(string folderPath);
    
    // Import entire folder
    Task<ImportResult> ImportFolderAsync(string folderPath, int businessId, int userId);
    
    // Process single file
    Task<FileProcessResult> ProcessFileAsync(string filePath, string category, 
        int businessId, int userId);
}

public class FolderAnalysisResult
{
    public List<FileInfo> CsvFiles { get; set; } = new();
    public List<FileInfo> PdfFiles { get; set; } = new();
    public List<FileInfo> ExcelFiles { get; set; } = new();
    public List<FileInfo> ImageFiles { get; set; } = new();
    
    public Dictionary<string, List<FileInfo>> CategorizedFiles { get; set; } = new();
    // "FA und ZOLL" → [Bescheid UID.pdf, EORI.pdf]
    // "STAMM" → [Datenblatt.pdf, ...]
}
```

---

## 5. SMART IMPORT RULES

### 5.1. Folder → Category Mapping

| Папка | Категория | Документы | Связь с |
|-------|-----------|-----------|---------|
| `FA und ZOLL` | Tax & Customs | UID, EORI, Tax certificates | Business |
| `STAMM` | Company Master Data | Registration, Articles | Business |
| `BANK` | Banking | IBAN, Account statements | Business |
| `BH` | Accounting | Sachkonten, Personenkonten, Journal | Accounts, Clients, Suppliers |
| `Rechnungen` | Invoices | PDF invoices | Invoice entity |
| `Verträge` | Contracts | Client/Supplier contracts | Client/Supplier |

### 5.2. Document Type Detection (by filename patterns)

```csharp
var documentTypePatterns = new Dictionary<string, string[]>
{
    ["UIDCertificate"] = new[] { "UID", "Bescheid Abgabensteuer", "Umsatzsteuer" },
    ["EORICertificate"] = new[] { "EORI", "Zoll", "Customs" },
    ["CompanyRegistration"] = new[] { "Firmenbuch", "FN", "Datenblatt Unternehmen" },
    ["IBANCertificate"] = new[] { "IBAN", "Kontoeröffnung", "Bank" },
    ["Invoice"] = new[] { "Rechnung", "Invoice", "RE-" },
    ["Contract"] = new[] { "Vertrag", "Contract", "Vereinbarung" },
    ["TaxReturn"] = new[] { "Steuererklärung", "UVA", "Tax Return" }
};
```

### 5.3. Field Extraction Rules

**UID Certificate:**
```regex
UID-Nummer:\s*(ATU\d{8})
Steuernummer:\s*(ATU\d{8})
```
→ Business.VatNumber

**EORI Certificate:**
```regex
EORI:\s*(AT[A-Z0-9]+)
```
→ Business.CustomsNumber

**Company Registration:**
```regex
Firmenbuchnummer:\s*(FN\s*\d+\w?)
Rechtsform:\s*(GmbH|AG|OG|KG)
Sitz:\s*(.+)
```
→ Business.CompanyRegistrationNumber, LegalForm, Address

**IBAN:**
```regex
IBAN:\s*(AT\d{2}\s*\d{4}\s*\d{4}\s*\d{4}\s*\d{4})
```
→ Business.BankAccount

---

## 6. UI COMPONENTS

### 6.1. DocumentUploadComponent.razor

```razor
<div class="document-upload-zone" @ondrop="HandleDrop" @ondragover="PreventDefault">
    <InputFile OnChange="HandleFileSelected" accept=".pdf,.jpg,.png,.csv,.xlsx" />
    <p>Drag & drop files or click to browse</p>
</div>

<div class="uploaded-documents">
    @foreach (var doc in Documents)
    {
        <div class="document-card">
            <span class="document-icon">📄</span>
            <span class="document-name">@doc.FileName</span>
            <button @onclick="() => ViewDocument(doc.Id)">View</button>
            <button @onclick="() => DownloadDocument(doc.Id)">Download</button>
            <button @onclick="() => DeleteDocument(doc.Id)">Delete</button>
        </div>
    }
</div>
```

### 6.2. DocumentFieldAttachment.razor (for inline attachment)

```razor
@* Usage: <DocumentFieldAttachment EntityType="Business" EntityId="@businessId" FieldName="VatNumber" /> *@

<div class="field-with-document">
    <InputText @bind-Value="Value" />
    
    @if (HasDocument)
    {
        <button class="btn-icon" @onclick="ViewDocument" title="View attached document">
            📄
        </button>
    }
    else
    {
        <button class="btn-icon" @onclick="AttachDocument" title="Attach document">
            📎
        </button>
    }
</div>
```

### 6.3. DocumentViewerModal.razor

```razor
<Modal IsOpen="@IsOpen" OnClose="Close">
    <ModalHeader>@Document.FileName</ModalHeader>
    <ModalBody>
        @if (Document.ContentType.StartsWith("image/"))
        {
            <img src="@ImageDataUrl" alt="@Document.FileName" />
        }
        else if (Document.ContentType == "application/pdf")
        {
            <iframe src="@PdfDataUrl" width="100%" height="600px"></iframe>
        }
        else
        {
            <p>Preview not available. <a href="@DownloadUrl">Download</a></p>
        }
    </ModalBody>
    <ModalFooter>
        <button @onclick="Download">Download</button>
        <button @onclick="Close">Close</button>
    </ModalFooter>
</Modal>
```

---

## 7. SMART FOLDER IMPORT PAGE

### 7.1. /Admin/SmartImport/Index.razor

```razor
@page "/admin/smart-import"

<h1>Smart Folder Import</h1>

<div class="import-wizard">
    @if (CurrentStep == 1)
    {
        <h3>Step 1: Select Folder</h3>
        <InputText @bind-Value="FolderPath" placeholder="C:\Data\Company Name" />
        <button @onclick="AnalyzeFolder">Analyze Folder</button>
    }
    else if (CurrentStep == 2)
    {
        <h3>Step 2: Review Detected Files</h3>
        
        <h4>CSVs (Data Import)</h4>
        @foreach (var csv in AnalysisResult.CsvFiles)
        {
            <div>
                <input type="checkbox" checked />
                <span>@csv.Name</span>
                <span>→ Import as: </span>
                <select>
                    <option>Clients</option>
                    <option>Suppliers</option>
                    <option>Accounts</option>
                </select>
            </div>
        }
        
        <h4>PDFs (Documents to Attach)</h4>
        @foreach (var pdf in AnalysisResult.PdfFiles)
        {
            <div>
                <input type="checkbox" checked />
                <span>@pdf.Name</span>
                <span>→ Detected: @GetDocumentType(pdf.Name)</span>
                <span>→ Attach to: @GetAttachmentTarget(pdf.Name)</span>
            </div>
        }
        
        <button @onclick="StartImport">Start Import</button>
    }
    else if (CurrentStep == 3)
    {
        <h3>Step 3: Import in Progress...</h3>
        <ProgressBar Value="@ImportProgress" />
        <ul>
            @foreach (var log in ImportLogs)
            {
                <li>@log</li>
            }
        </ul>
    }
    else if (CurrentStep == 4)
    {
        <h3>Step 4: Import Complete</h3>
        <div class="import-summary">
            <p>✅ Imported @ImportResult.ClientsCount clients</p>
            <p>✅ Imported @ImportResult.SuppliersCount suppliers</p>
            <p>✅ Imported @ImportResult.AccountsCount accounts</p>
            <p>✅ Uploaded @ImportResult.DocumentsCount documents</p>
            <p>✅ Created @ImportResult.AttachmentsCount attachments</p>
        </div>
        <button @onclick="Finish">Finish</button>
    }
</div>
```

---

## 8. CQRS COMMANDS

### 8.1. Upload Document

```csharp
public record UploadDocumentCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    int BusinessId,
    string? DocumentType = null
) : IRequest<int>;  // Returns DocumentId

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, int>
{
    public async Task<int> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        // 1. Validate file size/type
        // 2. Extract text if PDF
        // 3. Parse fields based on DocumentType
        // 4. Save to DB or FileSystem
        // 5. Create Document entity
        // 6. Return DocumentId
    }
}
```

### 8.2. Attach Document to Entity

```csharp
public record AttachDocumentCommand(
    int DocumentId,
    string EntityType,
    int EntityId,
    string? FieldName = null,
    string? Description = null
) : IRequest<int>;  // Returns AttachmentId
```

### 8.3. Import Folder

```csharp
public record ImportFolderCommand(
    string FolderPath,
    int BusinessId,
    Dictionary<string, string> FileTypeMapping  // FileName → ImportType
) : IRequest<ImportFolderResult>;

public class ImportFolderResult
{
    public int ClientsImported { get; set; }
    public int SuppliersImported { get; set; }
    public int AccountsImported { get; set; }
    public int DocumentsUploaded { get; set; }
    public int AttachmentsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

---

## 9. IMPLEMENTATION ROADMAP

### Phase 1: Core DMS (1-2 дня)
- [ ] Создать Document, DocumentAttachment, DocumentVersion entities
- [ ] Миграция БД
- [ ] DocumentService (Upload, Download, Attach, Delete)
- [ ] DocumentController API endpoints
- [ ] DocumentUploadComponent.razor

### Phase 2: Document Parsing (1 день)
- [ ] Интеграция PDF parser (iTextSharp / PdfPig)
- [ ] DocumentParserService
- [ ] Регулярные выражения для извлечения полей
- [ ] Тесты парсинга на реальных PDF

### Phase 3: UI Integration (1 день)
- [ ] DocumentFieldAttachment component
- [ ] Добавить 📄 иконки в Business/Edit, Client/Edit формы
- [ ] DocumentViewerModal
- [ ] Список документов на каждой странице

### Phase 4: Smart Import (2-3 дня)
- [ ] FolderAnalyzer service
- [ ] SmartImportService
- [ ] ImportFolderCommand + Handler
- [ ] /Admin/SmartImport UI wizard
- [ ] Тестирование на папке BKHA GmbH

### Phase 5: Advanced Features (1-2 дня)
- [ ] Версионирование документов
- [ ] Full-text search по ExtractedText
- [ ] Архивирование старых документов
- [ ] Права доступа к документам

---

## 10. ТЕХНОЛОГИИ

### Backend
- **PDF Parser**: iTextSharp 8.x / PdfPig
- **Image Processing**: System.Drawing / ImageSharp
- **OCR (optional)**: Tesseract.NET
- **Storage**: Database BLOB (SQLite → Azure SQL later)

### Frontend
- **File Upload**: InputFile (Blazor built-in)
- **Drag & Drop**: HTML5 DragDrop API
- **PDF Viewer**: `<iframe>` with data URL or PDF.js
- **Icons**: Unicode 📄📎📋 или Font Awesome

### Infrastructure
- **File Size Limit**: 50 MB per file (configurable)
- **Allowed Types**: PDF, JPG, PNG, CSV, XLSX
- **Storage Path**: `wwwroot/uploads/{businessId}/{year}/{month}/` (если FileSystem)

---

## 11. ПРИМЕР ИСПОЛЬЗОВАНИЯ

### Сценарий 1: Создание нового Business с документами

```csharp
// 1. User uploads folder "C:\Data\BKHA GmbH"
var importCommand = new ImportFolderCommand(
    FolderPath: @"C:\Data\BKHA GmbH",
    BusinessId: 2,
    FileTypeMapping: new Dictionary<string, string>
    {
        ["PK 2025 - BKHA GmbH.csv"] = "Personenkonten",
        ["Sachkonten 2025.csv"] = "ChartOfAccounts",
        ["Bescheid UID Nummer.pdf"] = "UIDCertificate",
        ["EORI-Antrag.pdf"] = "EORICertificate"
    }
);

var result = await _mediator.Send(importCommand);

// 2. System processes:
// - Parse "PK 2025.csv" → Import 1 Client + 9 Suppliers
// - Parse "Sachkonten 2025.csv" → Import 92 Accounts
// - Upload "Bescheid UID Nummer.pdf" → Extract "ATU12345678"
//   → Update Business.VatNumber = "ATU12345678"
//   → Attach document to Business, FieldName = "VatNumber"
// - Upload "EORI-Antrag.pdf" → Extract "AT123456789"
//   → Update Business.CustomsNumber = "AT123456789"
//   → Attach document to Business, FieldName = "CustomsNumber"

// 3. Result:
// - Business.VatNumber = "ATU12345678" [📄 icon clickable]
// - Business.CustomsNumber = "AT123456789" [📄 icon clickable]
// - 1 Client imported
// - 9 Suppliers imported
// - 92 Accounts imported
// - 2 Documents uploaded
// - 2 Attachments created
```

### Сценарий 2: User видит документ в UI

**Before:**
```razor
<div class="form-group">
    <label>UID Number</label>
    <InputText @bind-Value="Business.VatNumber" />
</div>
```

**After:**
```razor
<div class="form-group">
    <label>UID Number</label>
    <DocumentFieldAttachment 
        EntityType="Business" 
        EntityId="@Business.Id" 
        FieldName="VatNumber"
        @bind-Value="Business.VatNumber" />
    @* Renders: [ATU12345678] [📄 View Document] *@
</div>
```

Клик на 📄 → открывается modal с PDF "Bescheid UID Nummer.pdf"

---

## 12. SECURITY & PERMISSIONS

```csharp
public enum DocumentPermission
{
    ViewDocument = 1,
    UploadDocument = 2,
    DeleteDocument = 3,
    ArchiveDocument = 4,
    ViewAllBusinessDocuments = 5  // Admin only
}

// Проверка в handler:
if (document.BusinessId != currentBusinessId)
    throw new UnauthorizedBusinessAccessException("Document", documentId);
```

---

## 13. СЛЕДУЮЩИЕ ШАГИ

1. **Сейчас**: Утвердить архитектуру с пользователем
2. **Далее**: Начать с Phase 1 (Core DMS)
3. **Затем**: Протестировать на папке BKHA GmbH
4. **Результат**: Полноценная система управления документами

---

**Вопросы для обсуждения:**

1. Хранение файлов: Database BLOB или FileSystem? (рекомендую FileSystem для больших файлов)
2. OCR нужен сейчас или позже? (для сканов без текстового слоя)
3. Версионирование документов обязательно в Phase 1?
4. Максимальный размер файла: 50 MB достаточно?
5. Начать с Phase 1 сейчас или сначала доделать текущий импорт BKHA вручную?
