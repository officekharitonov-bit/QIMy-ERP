# 🔧 ТЕХНИЧЕСКИЕ РЕКОМЕНДАЦИИ: Интеграция QIMy с клиентским паттерном (Google Cloud → BMD)

**Дата:** 24 января 2026  
**Уровень:** Enterprise Integration  
**Приоритет:** HIGH

---

## 📌 EXECUTIVE SUMMARY

Клиент 200478 использует **правильный и полнофункциональный паттерн**:
- Google Cloud как фронт-энд для ввода данных
- QIMy (или подобная система) для обработки и расчетов
- BMD NTCS как финальная бухгалтерская система

Текущая реализация Personen Index в QIMy уже **90% готова** к поддержке этого паттерна!

---

## 🎯 ТЕКУЩИЙ СТАТУС (vs Реализация QIMy)

| Компонент | Статус в QIMy | Статус у клиента | Gap |
|-----------|----------------|------------------|-----|
| **PersonenIndexEntry** | ✅ Создана | Personen Index.xlsx | ✅ Совпадает |
| **Invoice (AR)** | ✅ Создана + FK | AR-2025.xlsx | ✅ Совпадает |
| **ExpenseInvoice (ER)** | ✅ Создана + FK | ER-2025.xlsx | ✅ Совпадает |
| **JournalEntry** | ⚠️ Не реализована | BUCHUNGSSCHRITTE.xlsx | ❌ НУЖНА! |
| **BankStatement** | ❌ Не полная | 3_BANK/*.csv | ❌ НУЖНА! |
| **CashEntry** | ❌ Не полная | 4_KASSA/*.xlsx | ❌ НУЖНА! |
| **Export to BMD** | ❌ Не реализована | Manual export | ❌ НУЖНА! |

**Заключение:** Нужно добавить 3 критических компонента (JournalEntry, BankStatement, CashEntry) и экспорт в BMD.

---

## 🛠️ РЕАЛИЗАЦИЯ ЭТАПОМ (ROADMAP)

### ЭТАП 1: JournalEntry (BUCHUNGSSCHRITTE) ⭐ КРИТИЧЕСКИЙ

**Цель:** Автоматическое создание двойных бухгалтерских проводок

#### 1.1 Создать Entity: JournalEntry

```csharp
public class JournalEntry : BaseEntity
{
    /// <summary>
    /// Запись в журнале (BUCHUNGSSCHRITT в BMD)
    /// </summary>
    public int Id { get; set; }
    
    // Идентификация
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Ссылка на исходный документ
    public int? InvoiceId { get; set; }          // Если из AR
    public int? ExpenseInvoiceId { get; set; }   // Если из ER
    public int? BankStatementLineId { get; set; } // Если из BANK
    public int? CashEntryId { get; set; }         // Если из KASSA
    
    // Счета (двойная запись)
    public int DebitAccountId { get; set; }      // FK → Account
    public int CreditAccountId { get; set; }     // FK → Account
    
    // Сумма
    public decimal Amount { get; set; }
    
    // Справочные сведения для BMD
    public string ReferenceNumber { get; set; } = string.Empty; // Invoice/ER number
    public int? CountryCode { get; set; }  // Для VAT tracking
    public int? VATAccountId { get; set; }  // Если есть VAT
    public decimal? VATAmount { get; set; }
    
    // Многотенантность
    public int BusinessId { get; set; }
    
    // Навигация
    public Invoice? Invoice { get; set; }
    public ExpenseInvoice? ExpenseInvoice { get; set; }
    public Account? DebitAccount { get; set; }
    public Account? CreditAccount { get; set; }
    public Account? VATAccount { get; set; }
}
```

#### 1.2 Сервис: JournalEntryService

```csharp
public class JournalEntryService
{
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// Создать проводки из AR счета
    /// ПРИМЕР: Invoice 1000€ + 200€ VAT = 1200€
    /// </summary>
    public async Task CreateEntriesFromInvoice(Invoice invoice)
    {
        var entries = new List<JournalEntry>();
        
        // ENTRY 1: Дебет - Bank/Receivables (1100), Кредит - Revenue (4000)
        entries.Add(new JournalEntry
        {
            EntryDate = invoice.InvoiceDate,
            Description = $"Invoice {invoice.InvoiceNumber}",
            InvoiceId = invoice.Id,
            DebitAccountId = GetAccountId("1100"), // Bank/Receivables
            CreditAccountId = GetAccountId("4000"), // Revenue
            Amount = invoice.SubTotal,
            ReferenceNumber = invoice.InvoiceNumber,
            BusinessId = invoice.BusinessId ?? 1
        });
        
        // ENTRY 2: Дебет - Bank/Receivables (1100), Кредит - VAT Payable (2100)
        if (invoice.TaxAmount > 0)
        {
            entries.Add(new JournalEntry
            {
                EntryDate = invoice.InvoiceDate,
                Description = $"VAT on Invoice {invoice.InvoiceNumber}",
                InvoiceId = invoice.Id,
                DebitAccountId = GetAccountId("1100"), // Bank/Receivables
                CreditAccountId = GetAccountId("2100"), // VAT Payable
                Amount = invoice.TaxAmount,
                ReferenceNumber = invoice.InvoiceNumber,
                VATAmount = invoice.TaxAmount,
                BusinessId = invoice.BusinessId ?? 1
            });
        }
        
        await _context.JournalEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Создать проводки из ER счета (входящий)
    /// ПРИМЕР: ER 500€ + 95€ VAT = 595€
    /// </summary>
    public async Task CreateEntriesFromExpenseInvoice(ExpenseInvoice invoice)
    {
        var entries = new List<JournalEntry>();
        
        // ENTRY 1: Дебет - Purchases (5030), Кредит - AP (3000)
        entries.Add(new JournalEntry
        {
            EntryDate = invoice.InvoiceDate,
            Description = $"Expense Invoice {invoice.InvoiceNumber}",
            ExpenseInvoiceId = invoice.Id,
            DebitAccountId = GetAccountId("5030"), // Purchases
            CreditAccountId = GetAccountId("3000"), // Accounts Payable
            Amount = invoice.SubTotal,
            ReferenceNumber = invoice.InvoiceNumber,
            BusinessId = invoice.BusinessId ?? 1
        });
        
        // ENTRY 2: Дебет - VAT Receivable (2300), Кредит - AP (3000)
        if (invoice.TaxAmount > 0)
        {
            entries.Add(new JournalEntry
            {
                EntryDate = invoice.InvoiceDate,
                Description = $"VAT on Expense Invoice {invoice.InvoiceNumber}",
                ExpenseInvoiceId = invoice.Id,
                DebitAccountId = GetAccountId("2300"), // VAT Receivable
                CreditAccountId = GetAccountId("3000"), // Accounts Payable
                Amount = invoice.TaxAmount,
                ReferenceNumber = invoice.InvoiceNumber,
                VATAmount = invoice.TaxAmount,
                BusinessId = invoice.BusinessId ?? 1
            });
        }
        
        await _context.JournalEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Валидация: Дебет должен равняться Кредиту!
    /// </summary>
    public async Task<bool> ValidateJournalBalance()
    {
        var debitTotal = await _context.JournalEntries.SumAsync(j => j.Amount);
        var creditTotal = await _context.JournalEntries.SumAsync(j => j.Amount);
        
        return debitTotal == creditTotal; // ДОЛЖНО БЫТЬ ИСТИНОЙ!
    }
}
```

#### 1.3 Event Handler: Автоматическое создание при сохранении

```csharp
// В InvoiceService.cs
public async Task CreateInvoice(CreateInvoiceDto dto)
{
    var invoice = new Invoice { /* ... */ };
    await _context.Invoices.AddAsync(invoice);
    await _context.SaveChangesAsync();
    
    // 🔥 АВТОМАТИЧЕСКИ создать JournalEntry!
    await _journalEntryService.CreateEntriesFromInvoice(invoice);
}
```

---

### ЭТАП 2: BankStatement (Банковские выписки)

**Цель:** Импорт и обработка банковских выписок

#### 2.1 Entity: BankStatement

```csharp
public class BankStatement : BaseEntity
{
    public string StatementNumber { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    public int BankAccountId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    
    public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();
    public BankAccount? BankAccount { get; set; }
}

public class BankStatementLine : BaseEntity
{
    public int BankStatementId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty; // DEBIT/CREDIT
    
    public int? RelatedInvoiceId { get; set; }  // Если payment для AR
    public int? RelatedExpenseInvoiceId { get; set; } // Если payment для ER
    
    public BankStatement? BankStatement { get; set; }
    public Invoice? RelatedInvoice { get; set; }
    public ExpenseInvoice? RelatedExpenseInvoice { get; set; }
}
```

#### 2.2 Импорт CSV

```csharp
public class BankStatementImportService
{
    /// <summary>
    /// Импортировать выписку из CSV
    /// Формат: Date,Description,Amount,Balance
    /// </summary>
    public async Task ImportFromCsv(string filePath, int bankAccountId)
    {
        var statement = new BankStatement
        {
            StatementNumber = Path.GetFileNameWithoutExtension(filePath),
            StatementDate = DateTime.UtcNow,
            BankAccountId = bankAccountId
        };
        
        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');
                var statementLine = new BankStatementLine
                {
                    TransactionDate = DateTime.Parse(parts[0]),
                    Description = parts[1],
                    Amount = decimal.Parse(parts[2]),
                    TransactionType = decimal.Parse(parts[2]) > 0 ? "CREDIT" : "DEBIT"
                };
                
                // Попытка найти связанный документ
                statementLine.RelatedInvoiceId = 
                    await FindRelatedInvoice(parts[1], Math.Abs(decimal.Parse(parts[2])));
                
                statement.Lines.Add(statementLine);
            }
        }
        
        await _context.BankStatements.AddAsync(statement);
        await _context.SaveChangesAsync();
        
        // Создать JournalEntry для платежей
        foreach (var line in statement.Lines)
        {
            if (line.RelatedInvoiceId.HasValue || line.RelatedExpenseInvoiceId.HasValue)
            {
                await _journalEntryService.CreateEntriesFromBankPayment(line);
            }
        }
    }
}
```

---

### ЭТАП 3: CashEntry (Кассовые операции)

**Цель:** Отслеживание наличных денег

#### 3.1 Entity: CashEntry

```csharp
public class CashEntry : BaseEntity
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string EntryType { get; set; } = string.Empty; // INCOME/EXPENSE/WITHDRAWAL
    
    public int? RelatedInvoiceId { get; set; }  // Если приход от продажи
    public int? RelatedExpenseInvoiceId { get; set; } // Если расход
    
    public Invoice? RelatedInvoice { get; set; }
    public ExpenseInvoice? RelatedExpenseInvoice { get; set; }
}

public class CashBook : BaseEntity
{
    public DateTime EntryDate { get; set; }
    public decimal BeginningBalance { get; set; }
    public decimal Income { get; set; }    // Приход
    public decimal Expense { get; set; }   // Расход
    public decimal EndingBalance { get; set; }
    
    public ICollection<CashEntry> Entries { get; set; } = new List<CashEntry>();
}
```

#### 3.2 Валидация кассовой книги

```csharp
public class CashBookService
{
    /// <summary>
    /// Валидация: Beginning + Income - Expense = Ending
    /// </summary>
    public async Task<bool> ValidateCashBalance(CashBook cashBook)
    {
        var calculated = cashBook.BeginningBalance 
                       + cashBook.Income 
                       - cashBook.Expense;
        
        return calculated == cashBook.EndingBalance;
    }
    
    /// <summary>
    /// Ежедневное закрытие кассы
    /// </summary>
    public async Task CloseDailyBook(DateTime date)
    {
        var entries = await _context.CashEntries
            .Where(c => c.EntryDate == date)
            .ToListAsync();
        
        var book = new CashBook
        {
            EntryDate = date,
            BeginningBalance = GetPreviousDayBalance(date),
            Income = entries.Where(e => e.EntryType == "INCOME").Sum(e => e.Amount),
            Expense = entries.Where(e => e.EntryType == "EXPENSE").Sum(e => e.Amount),
            Entries = entries
        };
        
        book.EndingBalance = book.BeginningBalance + book.Income - book.Expense;
        
        if (!await ValidateCashBalance(book))
            throw new Exception("Cash book does not balance!");
        
        await _context.CashBooks.AddAsync(book);
        await _context.SaveChangesAsync();
    }
}
```

---

### ЭТАП 4: Export to BMD NTCS

**Цель:** Выгрузка всех данных в BMD NTCS

#### 4.1 Export Service

```csharp
public class BmdNtcsExportService
{
    /// <summary>
    /// Экспортировать все данные в BMD NTCS формат
    /// </summary>
    public async Task ExportToFile(int businessId, string outputPath)
    {
        using (var workbook = new XLWorkbook())
        {
            // 1. BUCHUNGSSCHRITTE (JournalEntry)
            await ExportJournalEntries(workbook, businessId);
            
            // 2. AR (Invoice)
            await ExportInvoices(workbook, businessId);
            
            // 3. ER (ExpenseInvoice)
            await ExportExpenseInvoices(workbook, businessId);
            
            // 4. BANK (BankStatement)
            await ExportBankStatements(workbook, businessId);
            
            // 5. KASSA (CashBook)
            await ExportCashBooks(workbook, businessId);
            
            // 6. Personen Index
            await ExportPersonenIndex(workbook, businessId);
            
            workbook.SaveAs($"{outputPath}/BMD_Export_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }
    }
    
    private async Task ExportJournalEntries(XLWorkbook workbook, int businessId)
    {
        var sheet = workbook.Worksheets.Add("BUCHUNGSSCHRITTE");
        
        var entries = await _context.JournalEntries
            .Where(j => j.BusinessId == businessId)
            .Include(j => j.DebitAccount)
            .Include(j => j.CreditAccount)
            .ToListAsync();
        
        // Заголовки
        sheet.Cell(1, 1).Value = "Date";
        sheet.Cell(1, 2).Value = "Description";
        sheet.Cell(1, 3).Value = "Debit Account";
        sheet.Cell(1, 4).Value = "Debit Amount";
        sheet.Cell(1, 5).Value = "Credit Account";
        sheet.Cell(1, 6).Value = "Credit Amount";
        sheet.Cell(1, 7).Value = "Reference";
        
        // Данные
        int row = 2;
        foreach (var entry in entries)
        {
            sheet.Cell(row, 1).Value = entry.EntryDate;
            sheet.Cell(row, 2).Value = entry.Description;
            sheet.Cell(row, 3).Value = entry.DebitAccount?.AccountNumber;
            sheet.Cell(row, 4).Value = entry.Amount;
            sheet.Cell(row, 5).Value = entry.CreditAccount?.AccountNumber;
            sheet.Cell(row, 6).Value = entry.Amount;
            sheet.Cell(row, 7).Value = entry.ReferenceNumber;
            row++;
        }
    }
    
    // ... аналогично для AR, ER, BANK, KASSA ...
}
```

#### 4.2 Quarterly Archive

```csharp
public class QuarterlyArchiveService
{
    /// <summary>
    /// Архивировать данные по кварталам
    /// Создает папки: 2QU_2025/, 3QU_2025/, 4QU_2025/
    /// </summary>
    public async Task ArchiveByQuarter(int year)
    {
        for (int quarter = 1; quarter <= 4; quarter++)
        {
            var startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            
            var folder = $"{year}_{GetQuarterName(quarter)}";
            Directory.CreateDirectory(folder);
            
            // Экспортировать только данные за этот квартал
            await ExportPeriodData(folder, startDate, endDate);
        }
    }
}
```

---

## 📊 ПРИОРИТЕТЫ ПО ВЛИЯНИЮ

| Компонент | Влияние | Сложность | Время | Статус |
|-----------|---------|-----------|-------|--------|
| **JournalEntry** | 🔴 КРИТИЧЕСКИЙ | Средняя | 3-4 дня | ⏳ TODO |
| **BankStatement** | 🟡 ВЫСОКИЙ | Средняя | 2-3 дня | ⏳ TODO |
| **CashEntry** | 🟡 ВЫСОКИЙ | Низкая | 1-2 дня | ⏳ TODO |
| **Export to BMD** | 🟡 ВЫСОКИЙ | Средняя | 2 дня | ⏳ TODO |

**Рекомендуемый порядок реализации:**
1. JournalEntry (критический для работы)
2. BankStatement (для сверки платежей)
3. CashEntry (для полноты)
4. Export to BMD (для выгрузки)

---

## 🔒 КОНТРОЛЬНЫЕ ТОЧКИ (QA)

```
После реализации каждого компонента:

✅ Unit tests for business logic
✅ Integration tests for DB operations
✅ Validation tests for data integrity
✅ Performance tests for bulk operations
✅ UAT with client 200478

Конечные критерии:
✓ All JournalEntry: Debit = Credit
✓ All AR/ER → JournalEntry (no orphans)
✓ All BANK → matched to AR/ER or KASSA
✓ All KASSA → balanced daily
✓ Export → openable in BMD NTCS
```

---

## 💰 БИЗНЕС-ВЫГОДА

| Компонент | Выгода для клиента |
|-----------|-------------------|
| **JournalEntry** | Автоматический расчет + Полная аудит |
| **BankStatement** | Автосверка платежей + Контроль баланса |
| **CashEntry** | Управление наличностью + Отчеты |
| **Export** | Прямой выгруз в BMD + Экономия времени |

**Предполагаемая экономия для клиента 200478:**
- Ручной расчет JournalEntry: ~2 часа/день → 0 часов (автоматически)
- Сверка BANK: ~1 час/день → 5 минут (автоматически)
- Экспорт в BMD: ~1 час → 2 минуты (автоматически)

**Итого: экономия ~4 часов в день = ~100 часов в месяц! 🎯**

---

**Документ завершен**  
**Дата:** 24 января 2026  
**Статус:** ✅ ГОТОВЫЕ ТЕХНИЧЕСКИЕ РЕКОМЕНДАЦИИ
