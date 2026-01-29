# 🎯 ACTION PLAN: QIMy ERP Development Roadmap
## January 23, 2026

---

## EXECUTIVE SUMMARY

This is a detailed, step-by-step roadmap to take QIMy from 35% completion to MVP status (Sevdesk/Everbill feature parity). Organized by priority, timeline, and specific C# code tasks.

---

## PHASE 1: STABILIZATION & FOUNDATION (Today - Jan 27)
**Goal:** Fix AR module, complete CQRS migration, implement ER schema
**Effort:** 12-15 hours
**Status:** IN PROGRESS

---

## IMMEDIATE (Next 2-3 hours)

### STEP 1: Verify & Test Invoice Creation Fix

**File:** `src/QIMy.Infrastructure/Services/InvoiceService.cs`

**Current Status:**
- Hotfix applied on 23.01 to auto-generate `InvoiceNumber` and default `CurrencyId`
- **Action needed:** Test that the fix works end-to-end

**Task:**
```
1. Navigate to: http://localhost:5204/ar/invoices/create
2. Fill in REQUIRED fields:
   - ClientId: Select any client
   - InvoiceDate: Today
   - DueDate: Today + 30 days
3. Leave EMPTY (should auto-fill):
   - InvoiceNumber
   - CurrencyId
4. Click "Create"
5. Expected Result: ✅ Invoice created without DB errors
6. Verify in database: SELECT * FROM Invoices WHERE InvoiceNumber LIKE 'INV-%'
```

**Success Criteria:**
- ✅ Invoice created successfully
- ✅ InvoiceNumber auto-generated (format: `INV-{BusinessId}-{Ticks}`)
- ✅ CurrencyId defaults to EUR (IsDefault = true)
- ✅ No DbUpdateException thrown

**If it fails:**
- Check if currencies table is empty → Execute STEP 2 first
- Check InvoiceService.cs line 45-60 for hotfix logic
- Check Azure SQL connection is working

**Estimated Time:** 15 min

---

### STEP 2: Seed Reference Data

**Files:**
- `src/QIMy.Infrastructure/Data/SeedData.cs` (modify)
- Database migrations (check if currencies exist)

**Current Status:**
- `SeedData.cs` exists but might not be running
- Currencies table might be empty

**Task 2a: Check if data exists**

```powershell
# Open PowerShell and run:
dotnet build
sqlcmd -S "qimy-sql-server.database.windows.net" `
       -U "qimyadmin" `
       -P "h970334054CRgd1!" `
       -d "QImyDb" `
       -Q "SELECT COUNT(*) AS CurrencyCount FROM Currencies;"
```

**Expected Output:**
- If `CurrencyCount = 0` → Need to seed
- If `CurrencyCount > 0` → Skip to STEP 3

**Task 2b: If currencies missing, add seeding logic**

Open `src/QIMy.Infrastructure/Data/SeedData.cs` and add/modify:

```csharp
public static class SeedData
{
    public static void Seed(ApplicationDbContext context)
    {
        // CURRENCIES
        if (!context.Currencies.Any())
        {
            var currencies = new[]
            {
                new Currency
                {
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    ExchangeRate = 1.0m,
                    IsDefault = true
                },
                new Currency
                {
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    ExchangeRate = 1.10m,
                    IsDefault = false
                },
                new Currency
                {
                    Code = "CHF",
                    Name = "Swiss Franc",
                    Symbol = "CHF",
                    ExchangeRate = 0.95m,
                    IsDefault = false
                }
            };
            context.Currencies.AddRange(currencies);
            context.SaveChanges();
        }

        // TAX RATES
        if (!context.TaxRates.Any())
        {
            var taxRates = new[]
            {
                new TaxRate { Name = "VAT 19%", Rate = 19.0m, IsDefault = true, IsActive = true },
                new TaxRate { Name = "VAT 7%", Rate = 7.0m, IsDefault = false, IsActive = true },
                new TaxRate { Name = "VAT 0%", Rate = 0.0m, IsDefault = false, IsActive = true }
            };
            context.TaxRates.AddRange(taxRates);
            context.SaveChanges();
        }

        // PAYMENT METHODS
        if (!context.PaymentMethods.Any())
        {
            var methods = new[]
            {
                new PaymentMethod { Name = "Bank Transfer", Description = "SEPA/Wire transfer" },
                new PaymentMethod { Name = "Cash", Description = "Payment in cash" },
                new PaymentMethod { Name = "Check", Description = "Check payment" }
            };
            context.PaymentMethods.AddRange(methods);
            context.SaveChanges();
        }
    }
}
```

**Verify in Program.cs:**

Open `src/QIMy.Web/Program.cs` and ensure seeding runs on startup:

```csharp
// After app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();  // Apply pending migrations
    SeedData.Seed(context);       // Seed reference data
}
```

**Estimated Time:** 20 min

---

### STEP 3: Rebuild & Run Application

```powershell
cd c:\Projects\QIMy
dotnet build
dotnet run --project src/QIMy.Web/QIMy.Web.csproj
```

**Expected Output:**
- Build succeeds (Exit code 0)
- App starts without errors
- Browser opens: http://localhost:5204

**Estimated Time:** 10 min

---

## SHORT-TERM (Jan 24-27) — CQRS Migration

### STEP 4: Migrate Remaining Modules to CQRS

**Status:** Clients ✅, TaxRates ✅, 8 modules remaining ❌

**Modules to migrate (in order):**
1. **Currencies** (25 min) — HIGH priority
2. **Accounts** (30 min) — HIGH priority
3. **Businesses** (25 min) — HIGH priority
4. **Products** (30 min) — MEDIUM priority
5. **Units** (20 min) — LOW priority
6. **Discounts** (25 min) — LOW priority
7. **PaymentMethods** (25 min) — MEDIUM priority
8. **BankAccounts** (25 min) — MEDIUM priority

**Total Effort:** 3.5-4 hours

**Pattern (already established in Clients module):**

For each module `XYZ`:

```
src/QIMy.Application/XYZ/
├── Commands/
│   ├── Create/
│   │   ├── CreateXyzCommand.cs
│   │   ├── CreateXyzCommandHandler.cs
│   │   └── CreateXyzCommandValidator.cs
│   ├── Update/
│   │   ├── UpdateXyzCommand.cs
│   │   ├── UpdateXyzCommandHandler.cs
│   │   └── UpdateXyzCommandValidator.cs
│   └── Delete/
│       ├── DeleteXyzCommand.cs
│       └── DeleteXyzCommandHandler.cs
├── Queries/
│   ├── GetAllXyz/
│   │   ├── GetAllXyzQuery.cs
│   │   └── GetAllXyzQueryHandler.cs
│   └── GetXyzById/
│       ├── GetXyzByIdQuery.cs
│       └── GetXyzByIdQueryHandler.cs
├── DTOs/
│   └── XyzDtos.cs (XyzDto, CreateXyzDto, UpdateXyzDto)
└── (Validators inside Commands)

src/QIMy.Application/MappingProfiles/
└── XyzProfile.cs

src/QIMy.Web/Components/Pages/
└── [ModulePath]/
    ├── Index.razor (update to use IMediator instead of DbContext)
    └── CreateEdit.razor (update to use IMediator instead of DbContext)
```

**Template: CreateXyzCommand.cs**

```csharp
using MediatR;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;

namespace QIMy.Application.XYZ.Commands.Create;

public record CreateXyzCommand(
    string Name,
    string? Description,
    int? BusinessId
) : IRequest<XyzDto>;

public class CreateXyzCommandHandler : IRequestHandler<CreateXyzCommand, XyzDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateXyzCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<XyzDto> Handle(CreateXyzCommand request, CancellationToken cancellationToken)
    {
        var entity = new Xyz
        {
            Name = request.Name,
            Description = request.Description,
            BusinessId = request.BusinessId
        };

        _unitOfWork.XyzRepository.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<XyzDto>(entity);
    }
}
```

**Checklist for each module:**
- [ ] Create DTOs (DTO, CreateDTO, UpdateDTO)
- [ ] Create Commands (Create, Update, Delete) with Handlers
- [ ] Create Queries (GetAll, GetById) with Handlers
- [ ] Create Validators (for Create & Update commands)
- [ ] Create AutoMapper profile
- [ ] Update UI (Index.razor, CreateEdit.razor) to use IMediator
- [ ] Register in DI container (Program.cs)

**Timeline:**
- Monday (24.01): Currencies (25 min) + Accounts (30 min) + Businesses (25 min) = 1h 20 min
- Tuesday (24.01): Products (30 min) + Units (20 min) + Discounts (25 min) = 1h 15 min
- Wednesday (25.01): PaymentMethods (25 min) + BankAccounts (25 min) = 50 min

**Estimated Time:** 3.5-4 hours total

---

## MEDIUM-TERM (Jan 24-27) — ER Module Foundation

### STEP 5: Expand ER Data Model

**File:** `src/QIMy.Core/Entities/ExpenseInvoice.cs`

**Current State:**
- Entity exists but fields are minimal
- Missing critical fields for approval workflow & document management

**Task:** Expand `ExpenseInvoice.cs`:

```csharp
namespace QIMy.Core.Entities;

/// <summary>
/// Expense Invoice entity (Eingangsrechnung - ER)
/// Represents an incoming invoice from a supplier
/// </summary>
public class ExpenseInvoice : BaseEntity
{
    // Invoice identification
    public string ExpenseNumber { get; set; } = string.Empty;      // ER-2026-00001
    public string? VendorInvoiceNumber { get; set; }               // External invoice reference

    // Dates
    public DateTime InvoiceDate { get; set; }                       // When vendor issued invoice
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;   // When we received it
    public DateTime DueDate { get; set; }

    // Business context
    public int SupplierId { get; set; }
    public int CurrencyId { get; set; }
    public int? CostCenterId { get; set; }                         // For allocation
    public string? Department { get; set; }                        // Which department ordered

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;

    // Approval workflow
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }                        // UserId

    // Document & OCR
    public string? DocumentUrl { get; set; }                       // Azure Blob Storage path
    public string? OcrExtractedData { get; set; }                  // Raw JSON from OCR
    public bool IsOcrParsed { get; set; } = false;                // Whether auto-extracted

    // Matching (3-way match: PO → Receipt → Invoice)
    public int? PoId { get; set; }                                 // Purchase Order reference
    public DateTime? GoodsReceiptDate { get; set; }                // When goods arrived
    public bool IsMatched { get; set; } = false;                   // All 3 matched

    public string? Notes { get; set; }
    public bool IsReverseCharge { get; set; } = false;             // VAT treatment for intra-EU

    // Navigation properties
    public Supplier Supplier { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
    public ICollection<ExpenseInvoiceItem> Items { get; set; } = new List<ExpenseInvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum ExpenseStatus
{
    Draft = 0,              // Being entered/OCR parsed
    Submitted = 1,          // Sent for approval
    ApprovingManager = 2,   // Waiting for manager approval
    ApprovingDirector = 3,  // Waiting for director approval
    Approved = 4,           // All approvals done
    PartiallyPaid = 5,      // Some payment received
    Paid = 6,               // Full payment received
    Rejected = 7,           // Approval rejected
    Cancelled = 8           // Cancelled by user
}
```

**Also update `Supplier.cs`:**

```csharp
namespace QIMy.Core.Entities;

public class Supplier : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Tax & Bank
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? BankAccount { get; set; }
    public string? BankCode { get; set; }
    public string? Iban { get; set; }

    // Classification (like Client)
    public int? SupplierTypeId { get; set; }     // B2B, B2C, Government, etc.
    public int? SupplierAreaId { get; set; }     // Inland, EU, Third Country

    // Codes & status
    public string? SupplierCode { get; set; }    // Auto-generated (300000-399999)
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedDate { get; set; }

    // Accounting
    public int? DefaultAccountId { get; set; }   // For GL mapping
    public int? DefaultTaxRateId { get; set; }   // For tax calculation

    // Navigation
    public ICollection<ExpenseInvoice> ExpenseInvoices { get; set; } = new List<ExpenseInvoice>();
}
```

**Estimated Time:** 30 min

---

### STEP 6: Create Database Migration for ER Expansion

**File:** Create new migration

```powershell
cd src/QIMy.Infrastructure
dotnet ef migrations add "ExpandERModule" --startup-project ../QIMy.Web/QIMy.Web.csproj
```

This will automatically detect changes to `ExpenseInvoice` and `Supplier` entities.

**Review generated migration in:**
`src/QIMy.Infrastructure/Migrations/[timestamp]_ExpandERModule.cs`

**Apply migration:**

```powershell
dotnet ef database update --startup-project ../QIMy.Web/QIMy.Web.csproj
```

**Estimated Time:** 10 min

---

### STEP 7: Create ER CQRS Skeleton

**File structure:**

```
src/QIMy.Application/ExpenseInvoices/
├── Commands/
│   ├── Create/
│   │   ├── CreateExpenseInvoiceCommand.cs
│   │   ├── CreateExpenseInvoiceCommandHandler.cs
│   │   └── CreateExpenseInvoiceCommandValidator.cs
│   ├── Update/...
│   ├── Submit/
│   │   ├── SubmitExpenseInvoiceCommand.cs
│   │   └── SubmitExpenseInvoiceCommandHandler.cs
│   ├── Approve/
│   │   ├── ApproveExpenseInvoiceCommand.cs
│   │   └── ApproveExpenseInvoiceCommandHandler.cs
│   └── Delete/...
├── Queries/
│   ├── GetAllExpenseInvoices/
│   │   ├── GetAllExpenseInvoicesQuery.cs
│   │   └── GetAllExpenseInvoicesQueryHandler.cs
│   ├── GetExpenseInvoiceById/...
│   └── GetExpenseInvoicesByStatus/
│       ├── GetExpenseInvoicesByStatusQuery.cs
│       └── GetExpenseInvoicesByStatusQueryHandler.cs
└── DTOs/
    └── ExpenseInvoiceDtos.cs
```

**Template: CreateExpenseInvoiceCommand.cs**

```csharp
using MediatR;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;

namespace QIMy.Application.ExpenseInvoices.Commands.Create;

public record CreateExpenseInvoiceCommand(
    int SupplierId,
    string? VendorInvoiceNumber,
    DateTime InvoiceDate,
    DateTime ReceiptDate,
    DateTime DueDate,
    int CurrencyId,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Department,
    int? CostCenterId,
    string? Notes
) : IRequest<ExpenseInvoiceDto>;

public class CreateExpenseInvoiceCommandHandler : IRequestHandler<CreateExpenseInvoiceCommand, ExpenseInvoiceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateExpenseInvoiceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExpenseInvoiceDto> Handle(CreateExpenseInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Auto-generate ExpenseNumber
        var lastExpense = await _unitOfWork.ExpenseInvoiceRepository
            .GetAllAsync(e => true);
        var expenseNumber = $"ER-{DateTime.UtcNow.Year}-{lastExpense.Count() + 1:00001}";

        var expense = new ExpenseInvoice
        {
            ExpenseNumber = expenseNumber,
            VendorInvoiceNumber = request.VendorInvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            ReceiptDate = request.ReceiptDate,
            DueDate = request.DueDate,
            SupplierId = request.SupplierId,
            CurrencyId = request.CurrencyId,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.TotalAmount,
            Department = request.Department,
            CostCenterId = request.CostCenterId,
            Notes = request.Notes,
            Status = ExpenseStatus.Draft
        };

        _unitOfWork.ExpenseInvoiceRepository.Add(expense);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ExpenseInvoiceDto>(expense);
    }
}
```

**DTOs:**

```csharp
namespace QIMy.Application.ExpenseInvoices.DTOs;

public class ExpenseInvoiceDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; }
    public string? VendorInvoiceNumber { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public ExpenseStatus Status { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

public class CreateExpenseInvoiceDto
{
    [Required]
    public int SupplierId { get; set; }

    public string? VendorInvoiceNumber { get; set; }

    [Required]
    public DateTime InvoiceDate { get; set; }

    [Required]
    public DateTime ReceiptDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public string? Department { get; set; }
    public int? CostCenterId { get; set; }
    public string? Notes { get; set; }
}
```

**Estimated Time:** 45 min

---

## END OF WEEK (Jan 27) — Testing & Consolidation

### STEP 8: End-to-End Testing

**Test Scenarios:**

```
1. AR Cycle (Outgoing Invoice):
   ✅ Create Client
   ✅ Create Invoice (auto-numbered)
   ✅ Create Invoice Items
   ✅ Mark as Sent
   ✅ Record Payment
   ✅ Verify Paid status

2. ER Cycle (Incoming Invoice):
   ✅ Create Supplier
   ✅ Create Expense Invoice (auto-numbered)
   ✅ Submit for Approval
   ✅ Approve Expense
   ✅ Record Payment
   ✅ Verify Paid status

3. Reference Data:
   ✅ Currency defaults work
   ✅ Tax rates apply correctly
   ✅ Payment methods available
   ✅ Bank accounts selectable
```

**Estimated Time:** 1 hour

---

### STEP 9: Database Backup & Clean-up

```powershell
# Backup current database
sqlcmd -S "qimy-sql-server.database.windows.net" `
       -U "qimyadmin" `
       -P "h970334054CRgd1!" `
       -d "QImyDb" `
       -Q "BACKUP DATABASE [QImyDb] TO DISK = N'C:\Backups\QImyDb_20260127.bak';"

# Test restore (if needed)
```

**Estimated Time:** 10 min

---

## PHASE 2: FEATURE PARITY (Jan 28 - Feb 10)

**After Phase 1 completes, focus on:**

### Priority 1 (1 week)
- [ ] **Email Import** (4h) — Parse PDF invoices, extract supplier/amount/date
- [ ] **Client Code Auto-generation** (1h) — Stored procedure + UI
- [ ] **Supplier Code Auto-generation** (1h) — Stored procedure + UI

### Priority 2 (1 week)
- [ ] **PDF Invoice Generation** (2h) — Using QuestPDF
- [ ] **VAT Compliance Reports** (2h) — By supplier country
- [ ] **Expense Approval Workflow** (2h) — Multi-level approval

### Priority 3 (1 week)
- [ ] **CSV Import/Export** (2h) — Bulk operations
- [ ] **Registrierkasse Integration** (2h) — POS terminal support
- [ ] **Bank Statement Import** (2h) — Reconciliation workflow

---

## PHASE 3: SaaS READINESS (Feb 11 - Feb 28)

- [ ] **Analytics Dashboard** (3h)
- [ ] **Localization** (2h) — DE/EN
- [ ] **API Documentation** (1h) — OpenAPI completeness
- [ ] **Performance Optimization** (2h) — Caching, async ops
- [ ] **Security Audit** (2h) — Permissions, data access
- [ ] **User Documentation** (3h) — Help, tutorials

---

## RISK MITIGATION

### Risk 1: Invoice creation still fails
**Mitigation:** Verify hotfix is correct before proceeding to STEP 4

### Risk 2: CQRS migration introduces regressions
**Mitigation:** Test each module thoroughly before marking complete

### Risk 3: Email import is complex
**Mitigation:** Use Azure Form Recognizer (not custom OCR)

### Risk 4: Database growth unmanaged
**Mitigation:** Set up automatic backups, monitor DTU usage

---

## SUCCESS METRICS (by Jan 27)

- ✅ AR Invoice creation works without errors
- ✅ Reference data (currencies, tax rates) seeded
- ✅ CQRS migration 100% complete (all 10 modules)
- ✅ ER data model expanded
- ✅ ER CQRS skeleton implemented
- ✅ Database migrations applied successfully
- ✅ E2E tests pass for AR & ER basic flows
- ✅ Code committed to GitHub main branch

---

## COMMIT CHECKLIST

Before pushing to GitHub, ensure:
```
✅ All builds pass: dotnet build
✅ No compiler warnings
✅ New migrations applied
✅ Reference data seeded
✅ Tests pass (if any)
✅ Session log updated
✅ Changes documented in code comments
```

---

**Generated:** 2026-01-23
**Updated:** As phases complete
**Owner:** GitHub Copilot (QIMy Development)

---

## APPENDIX: Quick Command Reference

```powershell
# Build & Run
dotnet build
dotnet run --project src/QIMy.Web/QIMy.Web.csproj

# Database Operations
dotnet ef migrations add "Name"
dotnet ef database update

# Seed data (if needed separately)
dotnet run --project src/QIMy.Infrastructure/QIMy.Infrastructure.csproj

# Testing
dotnet test

# Publish to Azure
dotnet publish -c Release
```

---

## APPENDIX: File Checklist

| File | Status | Priority |
|------|--------|----------|
| `src/QIMy.Infrastructure/Services/InvoiceService.cs` | ✅ Hotfix applied | 🔴 TEST NOW |
| `src/QIMy.Infrastructure/Data/SeedData.cs` | ❌ Needs currencies | 🔴 TODAY |
| `src/QIMy.Core/Entities/ExpenseInvoice.cs` | ❌ Needs expansion | 🟠 JAN 24 |
| `src/QIMy.Core/Entities/Supplier.cs` | ❌ Needs expansion | 🟠 JAN 24 |
| `src/QIMy.Application/ExpenseInvoices/*` | ❌ Needs creation | 🟠 JAN 24 |
| `src/QIMy.Web/Components/Pages/ER/*` | ❌ Needs creation | 🟡 JAN 25 |

---
