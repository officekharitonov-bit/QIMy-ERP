# OLD QIM FORMAT ANALYSIS - Import/Export Compatibility

**Дата**: 29 января 2026
**Цель**: Обеспечить совместимость QIMy с форматами старого QIM для импорта/экспорта инвойсов

---

## 📂 Анализ существующих файлов

**Источник**: `C:\Projects\QIMy\tabellen\BKHA GmbH\BH\AR\invoices 2025-01\`

### Найденные файлы:
1. **FinalReport_2026-01-29_00-14-23.csv** - CSV экспорт для бухгалтерии (BMD NTCS format)
2. **INVOICE AR2026010001 Innogate Technology s. r. o. SK2120677625.pdf** - PDF инвойс

---

## 📋 CSV Format (BMD NTCS Buchungssatz)

### Структура CSV файла:

```csv
satzart;konto;gkonto;buchdatum;belegdatum;belegnr;betrag;steuer;text;buchtyp;buchsymbol;filiale;prozent;steuercode;buchcode;fwbetrag;fwsteuer;waehrung;periode;gegenbuchkz;verbuchkz;ausz-belegnr;ausz-betrag;extid;extid;verbuchstatus;uidnr;dokumente
```

### Поля (29 колонок):

| № | Поле | Значение | Описание |
|---|------|----------|----------|
| 1 | `satzart` | 0 | Тип записи (0 = бухгалтерская запись) |
| 2 | `konto` | 230008 | Номер счета клиента (Kto-Nr из PersonenIndex) |
| 3 | `gkonto` | 4113 | Счет учета (Sachkonto - Erlöskonto) |
| 4 | `buchdatum` | 14.01.2026 | Дата проводки |
| 5 | `belegdatum` | 14.01.2026 | Дата документа |
| 6 | `belegnr` | 2026001Inogate | Номер документа/счета |
| 7 | `betrag` | 16000,00 | Сумма (с запятой, немецкий формат) |
| 8 | `steuer` | 0,00 | Сумма налога |
| 9 | `text` | INVOICE AR2026010001... | Описание проводки |
| 10 | `buchtyp` | 1 | Тип проводки (1 = AR, 2 = ER) |
| 11 | `buchsymbol` | AR | Символ проводки |
| 12 | `filiale` | (пусто) | Филиал |
| 13 | `prozent` | 0 | Процент налога |
| 14 | `steuercode` | 77 | Код налога (Steuercode) |
| 15 | `buchcode` | 1 | Код проводки |
| 16 | `fwbetrag` | (пусто) | Сумма в валюте |
| 17 | `fwsteuer` | (пусто) | Налог в валюте |
| 18 | `waehrung` | EUR | Валюта |
| 19 | `periode` | 01 | Период (месяц) |
| 20 | `gegenbuchkz` | E | Признак встречной проводки |
| 21 | `verbuchkz` | A | Признак проведения |
| 22 | `ausz-belegnr` | (пусто) | Номер исходного документа |
| 23 | `ausz-betrag` | (пусто) | Сумма исходного документа |
| 24 | `extid` | (пусто) | Внешний ID 1 |
| 25 | `extid` | (пусто) | Внешний ID 2 |
| 26 | `verbuchstatus` | 0 | Статус проведения |
| 27 | `uidnr` | SK2120677625 | UID номер клиента |
| 28 | `dokumente` | (пусто) | Ссылка на документы |

### Пример записи:
```csv
0;230008;4113;14.01.2026;14.01.2026;2026001Inogate;16000,00;0,00;INVOICE AR2026010001 Innogate Technology s. r. o., SK2120677625;1;AR;;0;77;1;;;EUR;01;E;A;;;;;0;SK2120677625;
```

### Ключевые особенности:
- ✅ Разделитель: точка с запятой (`;`)
- ✅ Десятичный разделитель: запятая (`,`)
- ✅ Формат даты: `dd.MM.yyyy`
- ✅ Кодировка: Windows-1252 (вероятно)
- ✅ Заголовки: есть (первая строка)

---

## 📄 PDF Format

### Структура PDF:
**Имя файла**: `INVOICE AR2026010001 Innogate Technology s. r. o. SK2120677625.pdf`

**Naming Convention**:
```
INVOICE {InvoiceType}{InvoiceNumber} {ClientCompanyName} {ClientVatNumber}.pdf
```

**Пример**:
- `InvoiceType`: AR (Accounts Receivable - исходящий счет)
- `InvoiceNumber`: 2026010001
- `ClientCompanyName`: Innogate Technology s. r. o.
- `ClientVatNumber`: SK2120677625

### PDF Содержание (предполагаемое):
1. **Header**: Логотип, реквизиты BKHA GmbH
2. **Invoice Info**:
   - Invoice Number: AR2026010001
   - Invoice Date: 14.01.2026
   - Due Date: (Payment Terms дней)
3. **Client Info**:
   - Company Name: Innogate Technology s. r. o.
   - Address
   - VAT Number: SK2120677625
4. **Line Items**: Описание услуг/товаров
5. **Summary**:
   - Subtotal (Netto)
   - VAT 0% (innergemeinschaftliche Lieferung)
   - Total (Brutto): 16,000.00 EUR
6. **Footer**:
   - Bank details
   - Tax notes (UID-Nr, etc.)
   - Payment terms

---

## 🎯 Implementation Plan

### Phase 1: CSV Export для BMD NTCS ✅ **РЕАЛИЗОВАНО**

**Существующий код**: `FinalReportService.GenerateFinalReportCsvAsync()`

**Что было доработано**:
1. ✅ Формат полностью совместим с BMD (29 полей)
2. ✅ Немецкое форматирование чисел (запятая вместо точки: `16000,00`)
3. ✅ Немецкое форматирование дат (dd.MM.yyyy: `14.01.2026`)
4. ✅ Добавлено поле `uidnr` (VAT Number из Client.VatNumber)
5. ✅ Автоматический маппинг Steuercode по типу инвойса
6. ✅ UI кнопки для экспорта на странице счетов

### Phase 2: CSV Import из BMD NTCS

**Новый сервис**: `BmdInvoiceImportService`

**Задачи**:
1. Парсинг CSV с 29 колонками
2. Маппинг полей на `Invoice` entity
3. Создание клиента если не существует (по `konto`)
4. Определение типа инвойса (AR/ER по `buchtyp`)
5. Валидация данных
6. Обработка ошибок

**Пример кода**:
```csharp
public class BmdInvoiceImportService
{
    public async Task<BmdImportResult> ImportFromBmdCsvAsync(
        Stream csvStream,
        int businessId)
    {
        // 1. Parse CSV with German format
        // 2. Map to Invoice entities
        // 3. Create clients if needed
        // 4. Save invoices
    }
}
```

### Phase 3: PDF Export (Enhanced) ✅ **РЕАЛИЗОВАНО**

**Существующий код**: `PdfGenerator.cs` (QuestPDF)

**Что было доработано**:
1. ✅ PDF generation работает
2. ✅ Naming convention: `INVOICE {Type}{Number} {ClientName} {VatNumber}.pdf`
3. ⏳ Добавить поле UID-Nr клиента на PDF (TODO)
4. ⏳ Добавить tax notes для разных типов инвойсов (TODO)

**Новый метод**:
```csharp
public static byte[] GeneratePdfWithBmdNaming(
    Invoice invoice,
    string outputPath)
{
    var pdfBytes = GeneratePdf(invoice);
    var fileName = $"INVOICE {invoice.InvoiceNumber} {invoice.Client.CompanyName} {invoice.Client.VatNumber}.pdf";
    // Save to outputPath
    return pdfBytes;
}
```

### Phase 4: Batch Export для BMD

**Новый сервис**: `BmdBatchExportService`

**Функционал**:
1. Экспорт всех инвойсов за период в BMD CSV
2. Генерация PDF для каждого инвойса
3. Создание структуры папок:
   ```
   tabellen/
   ├── {Business Name}/
   │   ├── {Business Code}/
   │   │   ├── AR/
   │   │   │   ├── invoices 2025-01/
   │   │   │   │   ├── FinalReport_2026-01-29.csv
   │   │   │   │   ├── INVOICE AR2026010001 Client1 VAT.pdf
   │   │   │   │   ├── INVOICE AR2026010002 Client2 VAT.pdf
   ```

---

## 📌 Priority Tasks

### High Priority (Next Session)
1. ✅ Analyze old QIM format (DONE - 29 января 2026)
2. ✅ Add UID-Nr (VAT Number) to CSV export (DONE - 29 января 2026)
3. ✅ Fix CSV export number format (comma instead of dot) (DONE - 29 января 2026)
4. ✅ Add all 29 BMD fields to CSV export (DONE - 29 января 2026)
5. ✅ Implement PDF naming convention (DONE - 29 января 2026)
6. ✅ Add UI buttons for export (DONE - 29 января 2026)
7. ⏳ Add UID-Nr to PDF document itself
8. ⏳ Add tax notes to PDF for different invoice types

### Medium Priority
1. Create `BmdInvoiceImportService` for CSV import
2. Add batch export UI page
3. Test with real BKHA data

### Low Priority
1. Add unit tests for import/export
2. Add error handling and logging
3. Create documentation for users

---

## 🔍 Testing Checklist

### CSV Export Test:
- [ ] All 29 fields present
- [ ] Numbers with comma (16000,00)
- [ ] Dates in dd.MM.yyyy format
- [ ] Encoding: Windows-1252
- [ ] Delimiter: semicolon
- [ ] UID-Nr populated from client

### PDF Export Test:
- [ ] Filename matches BMD convention
- [ ] UID-Nr displayed on invoice
- [ ] Tax notes correct for invoice type
- [ ] All client/business info present

### Import Test:
- [ ] Can import FinalReport CSV from old QIM
- [ ] Clients auto-created from konto field
- [ ] Invoice type detected from buchtyp
- [ ] Steuercode mapped correctly
- [ ] Dates parsed correctly (German format)

---

## 💡 Code Locations

**Existing Services**:
- `FinalReportService.cs` - CSV export (needs enhancement)
- `PdfGenerator.cs` - PDF generation (needs naming fix)
- `ClientImportService.cs` - Client import (reference for BMD import)

**New Services Needed**:
- `BmdInvoiceImportService.cs` - Import invoices from BMD CSV
- `BmdBatchExportService.cs` - Batch export to BMD format
- `BmdNamingService.cs` - File naming conventions

**Database**:
- Add `VatNumber` to `Client` entity ✅ (already exists)
- Add `Steuercode`, `Konto`, `Proz` to `Invoice` entity ✅ (already exists)

---

## 📊 Example Data Mapping

### CSV → Invoice Entity

| CSV Field | Invoice Property | Notes |
|-----------|------------------|-------|
| konto | ClientCode | Look up client by code |
| gkonto | Konto | Revenue account |
| buchdatum | InvoiceDate | Parse German date |
| belegnr | InvoiceNumber | Strip prefix if needed |
| betrag | SubTotal | Parse German decimal |
| steuer | TaxAmount | Parse German decimal |
| steuercode | Steuercode | Tax code mapping |
| waehrung | Currency | EUR default |
| uidnr | Client.VatNumber | From PersonenIndex |

---

**Status**: 📋 ANALYSIS COMPLETE
**Next Step**: Implement CSV export enhancements and PDF naming convention
