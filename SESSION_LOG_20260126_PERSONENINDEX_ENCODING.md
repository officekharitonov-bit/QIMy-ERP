# SESSION LOG: Personenindex Import + Auto-Encoding Detection

**Date:** 2026-01-26
**Duration:** ~2 hours
**Status:** ✅ COMPLETED

## 🎯 TASKS COMPLETED

### 1. ✅ Personenindex Rename (PersonenKonten → PersonenIndex)
- **File:** `PersonenKonten.razor` → `PersonenIndex.razor`
- **Route:** `/Admin/SmartImport/PersonenIndex`
- **Navigation:** Updated NavMenu.razor
- **Breadcrumbs:** Updated all references

### 2. ✅ Menu Restructure (AR/ER Simplification)
**Removed:**
- Клиенты from AR section
- Поставщики from ER section
- АГ - ПРЕДЛОЖЕНИЯ section

**New Structure:**
```
AR - СЧЕТА (ИСХОДЯЩИЕ)
  ├── Счета исходящие
  └── Проформы (links to /ag/quotes)

ER - СЧЕТА (ВХОДЯЩИЕ)
  ├── Счета входящие
  └── Проформы

АДМИН - СПРАВОЧНИКИ
  └── Personenindex Import (unified clients + suppliers)
```

### 3. ✅ Account Number Ranges Fixed
**Corrected from documentation:**
- **2xxxxx (200000-299999)** = Клиенты (ALL codes starting with 2)
- **3xxxxx (300000-399999)** = Поставщики (ALL codes starting with 3)
- **1-9999** = Sachkonten

**Updated files:**
- `ImportSuppliersCommandHandler.cs` - filter `3xxxxx` only
- `PersonenIndex.razor` - UI text updated
- `Suppliers/Import.razor` - description updated

### 4. ✅ Automatic Encoding Detection
**Problem:** User saw "кубики" (encoding issues) in CSV imports

**Solution:** Added `DetectEncoding()` method to ALL import handlers:
- ✅ `ImportClientsCommandHandler.cs` - auto-detect + fallback to Windows-1252
- ✅ `ImportSuppliersCommandHandler.cs` - REVERTED (had merge conflict)
- ✅ `Admin/Accounts/Import.razor` - auto-detect BOM

**Detection Logic:**
```csharp
1. Check UTF-8 BOM (EF BB BF)
2. Check UTF-16 LE BOM (FF FE)
3. Check UTF-16 BE BOM (FE FF)
4. Fallback: Windows-1252 (BMD default)
```

### 5. ✅ Missing Import Pages Created (Previous Session)
- `/ER/Suppliers/Import` - standalone supplier import
- `/Admin/Accounts/Import` - Sachkonten import
- Added navigation buttons to list pages

## 📁 FILES MODIFIED

### Core Logic
1. `ImportClientsCommandHandler.cs` ✅
   - Added `DetectEncoding()` method
   - Auto-detect in `ParseCsvAsync()`

2. `ImportSuppliersCommandHandler.cs` ⚠️ REVERTED
   - Attempted encoding detection
   - Merge conflict during edit
   - User did `git checkout` to restore

3. `Admin/Accounts/Import.razor` ✅
   - Added `DetectEncoding()` method
   - Applied before StreamReader

### UI Components
4. `PersonenIndex.razor` ✅ (renamed from PersonenKonten)
   - Route: `/Admin/SmartImport/PersonenIndex`
   - Title: "Personenindex Import"
   - Description: 2xxxxx/3xxxxx ranges

5. `NavMenu.razor` ✅
   - Removed "Клиенты" link (AR)
   - Removed "Поставщики" link (ER)
   - Removed "АГ - ПРЕДЛОЖЕНИЯ" section
   - Updated "Personenkonten" → "Personenindex"
   - Проформы now links to `/ag/quotes`

6. `ER/Suppliers/Import.razor` ✅
   - Updated codes: 3xxxxx (300000-399999)

## 🔧 TECHNICAL DETAILS

### Encoding Detection Algorithm
```csharp
private Encoding DetectEncoding(Stream stream)
{
    var bom = new byte[4];
    var bytesRead = stream.Read(bom, 0, 4);
    stream.Position = 0;

    // UTF-8 BOM
    if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        return Encoding.UTF8;

    // UTF-16 LE BOM
    if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
        return Encoding.Unicode;

    // UTF-16 BE BOM
    if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
        return Encoding.BigEndianUnicode;

    // Default for BMD files
    return Encoding.GetEncoding("windows-1252");
}
```

### BMD CSV Format
```
Line 0: NTCS_CONSTID_INFO%22%MCA_... (metadata with %)
Line 1: Externe KontoNr;Kto-Nr;Nachname;... (header with ;)
Line 2+: ;200001;Anatolii Skrypniak;... (data with ;)
```

### Account Code Filtering
**Clients (ImportClientsCommand):**
- No filtering needed - imports ALL 2xxxxx codes
- Examples: 200001, 230009, 260000

**Suppliers (ImportSuppliersCommand):**
```csharp
if (string.IsNullOrWhiteSpace(accountNumber) ||
    !accountNumber.StartsWith("3") ||
    accountNumber.Length != 6)
{
    continue; // Skip non-suppliers
}
```

## 📊 BKHA GmbH Test Data
**File:** `C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv`

**Contents:**
- 200001 - Anatolii Skrypniak (client)
- 230001-230009 - Various clients (9 records, ALL starting with 2)
- 260000-260001 - Additional clients
- **Total:** 12 client records, 0 supplier records

**Note:** File contains NO suppliers (no 3xxxxx codes)

## ⚠️ ISSUES ENCOUNTERED

### 1. ImportSuppliersCommandHandler Merge Conflict
**Problem:** Duplicate `DetectEncoding()` method added outside class scope
**Cause:** Incorrect string replacement placement
**Resolution:** User executed `git checkout` to restore file
**Status:** ⚠️ Encoding detection NOT applied to suppliers import

### 2. Account Range Confusion
**Problem:** Initial confusion about 230xxx codes
**Cause:** Misreading of documentation (230xxx are clients, not suppliers)
**Resolution:** Studied `PersonenIndex_Structure_RU.md` - confirmed 2xxxxx = clients, 3xxxxx = suppliers

### 3. Server Crashes
**Problem:** Server kept shutting down after 5-10 seconds
**Cause:** Terminal tool interaction issues
**Resolution:** Used `Start-Process` with `-WindowStyle Minimized` for background execution

## ✅ BUILD STATUS

**Final Build:** SUCCESS (0 errors)
```
QIMy.Core -> OK
QIMy.Application -> OK
QIMy.Infrastructure -> OK
QIMy.Web -> OK
QIMy.API -> OK
```

**Server:** Running on http://localhost:5204

## 📝 PENDING TASKS

### High Priority
1. ⚠️ **Re-apply encoding detection to ImportSuppliersCommandHandler**
   - File was reverted due to merge conflict
   - Need clean implementation

2. **Test imports with real BKHA data**
   - Load PK 2025 CSV file
   - Verify 12 clients imported correctly
   - Check encoding (no кубики)

### Medium Priority
3. **Add encoding detection to remaining import pages**
   - Products import
   - Invoice line items import
   - Any other CSV uploads

4. **Document encoding detection pattern**
   - Add to coding guidelines
   - Create reusable helper class

### Low Priority
5. **Create Sachkonten test data**
   - Current BKHA file only has clients
   - Need 3xxxxx codes for supplier testing

## 🎓 LESSONS LEARNED

1. **Always check file structure before editing**
   - Read surrounding code carefully
   - Understand class boundaries

2. **Git checkout is recovery tool**
   - When merge conflicts occur
   - Faster than manual fixes

3. **Documentation is critical**
   - PersonenIndex_Structure_RU.md saved hours
   - Clear account range definitions essential

4. **User experience matters**
   - "кубики" (encoding issues) are unacceptable
   - Auto-detection > manual selection

## 📚 RELATED DOCUMENTATION

- [PersonenIndex_Structure_RU.md](PersonenIndex_Structure_RU.md) - Account range definitions
- [PERSONEN_INDEX_QUICK_REFERENCE.md](PERSONEN_INDEX_QUICK_REFERENCE.md) - Quick start guide
- [AI_CONTEXT.md](AI_CONTEXT.md) - Updated with session info

## 🔗 NAVIGATION

**Import Pages:**
- `/Admin/SmartImport/PersonenIndex` - Unified (clients + suppliers)
- `/ER/Suppliers/Import` - Suppliers only
- `/Admin/Accounts/Import` - Sachkonten

**Menu Structure:**
```
AR → Счета исходящие, Проформы
ER → Счета входящие, Проформы
АДМИН → Personenindex Import
```

---

**Session End:** 2026-01-26 03:40 UTC
**Next Session:** Complete encoding detection for suppliers + test real imports
