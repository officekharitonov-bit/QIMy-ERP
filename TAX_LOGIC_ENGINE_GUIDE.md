# Tax Logic Engine - Complete Guide

## 🎯 Назначение

**AustrianTaxLogicEngine** - это система автоматического определения налогового случая согласно австрийскому законодательству (UStG) и стандартам ERP BMD NTCS.

## 📊 Поддерживаемые налоговые случаи

### 1. INLAND (Внутренняя поставка)
**Когда:** Покупатель находится в Австрии
```
StC: 1 (Umsatzsteuer)
Proz: 20% (стандартная ставка) или 10% (сниженная)
Konto: 4000
Текст: "Umsatzsteuer 20%"
```

### 2. Kleinunternehmer (Малое предприятие)
**Когда:** Продавец - малое предприятие по §6 Abs.1 Z 27
```
StC: 16
Proz: 0%
Konto: 4062
Текст: "Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG"
UID: НЕ ТРЕБУЕТСЯ
```

### 3. Innergemeinschaftliche Lieferung (IGL)
**Когда:** Поставка товаров в другую страну ЕС, покупатель с UID
```
StC: 11 (Umsätze Art. 6 Abs. 1)
Proz: 0%
Konto: 4000
Текст: "Steuerfreie innergemeinschaftliche Lieferung gem. Art. 6 Abs. 1 UStG"
Обязательно: UID продавца (ATU) + UID покупателя
```

### 4. Reverse Charge (Перенос налоговой обязанности)
**Когда:** Услуги B2B в страну ЕС
```
StC: 19 (Umsätze § 19)
Proz: 0%
Konto: 4000
Текст: "Steuerschuldner ist der Rechnungsempfänger (Reverse Charge gem. § 19 UStG)"
Обязательно: UID продавца + UID покупателя
```

### 5. Export (Экспорт за пределы ЕС)
**Когда:** Покупатель в третьей стране (не ЕС)
```
StC: 10 (Ausfuhrlieferung § 6 Abs. 1 Z 1)
Proz: 0%
Konto: 4000
Текст: "Steuerfreie Ausfuhrlieferung gem. § 6 Abs. 1 Z 1 UStG"
Обязательно: Zollnummer (таможенный номер)
```

### 6. Dreiecksgeschäft (Трёхсторонняя сделка)
**Когда:** Три стороны в трёх разных странах ЕС
```
StC: 11
Proz: 0%
Konto: 4000
Текст: "Innergemeinschaftliches Dreiecksgeschäft gem. Art. 25 UStG. Die Steuerschuld geht auf den Empfänger über."
Обязательно: UID всех трёх сторон
```

---

## 🔧 Использование

### Пример 1: Стандартная поставка в Австрии
```csharp
var engine = new AustrianTaxLogicEngine();

var input = new TaxCaseInput
{
    BuyerCountry = "AT",
    BuyerCountryInEU = true,
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};

var result = engine.DetermineTaxCase(input);

// Result:
// TaxCase: Inland
// Steuercode: 1
// Proz: 20%
// Konto: 4000
// InvoiceText: "Umsatzsteuer 20%"
```

### Пример 2: Малое предприятие
```csharp
var input = new TaxCaseInput
{
    SellerIsSmallBusiness = true,
    BuyerCountry = "AT"
};

var result = engine.DetermineTaxCase(input);

// Result:
// TaxCase: Kleinunternehmer
// Steuercode: 16
// Proz: 0%
// Konto: 4062
// InvoiceText: "Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG"
// RequiredFields: ["Firmenname", "Adresse"]
// RequiresUidValidation: false
```

### Пример 3: IGL (Поставка в Германию)
```csharp
var input = new TaxCaseInput
{
    BuyerCountry = "DE",
    BuyerCountryInEU = true,
    BuyerUid = "DE123456789",
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};

var result = engine.DetermineTaxCase(input);

// Result:
// TaxCase: InnergemeinschaftlicheLieferung
// Steuercode: 11
// Proz: 0%
// Konto: 4000
// InvoiceText: "Steuerfreie innergemeinschaftliche Lieferung gem. Art. 6 Abs. 1 UStG"
// RequiredFields: ["UID Verkäufer (ATU)", "UID Käufer", "Lieferadresse EU"]
// RequiresUidValidation: true
```

### Пример 4: Reverse Charge (Услуги в ЕС)
```csharp
var input = new TaxCaseInput
{
    BuyerCountry = "FR",
    BuyerCountryInEU = true,
    BuyerUid = "FR12345678901",
    IsGoodsSupply = false, // Services
    SellerIsSmallBusiness = false
};

var result = engine.DetermineTaxCase(input);

// Result:
// TaxCase: ReverseCharge
// Steuercode: 19
// Proz: 0%
// Konto: 4000
// InvoiceText: "Steuerschuldner ist der Rechnungsempfänger (Reverse Charge gem. § 19 UStG)"
// RequiredFields: ["UID Verkäufer", "UID Käufer"]
// RequiresUidValidation: true
```

### Пример 5: Экспорт в США
```csharp
var input = new TaxCaseInput
{
    BuyerCountry = "US",
    BuyerCountryInEU = false,
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};

var result = engine.DetermineTaxCase(input);

// Result:
// TaxCase: Export
// Steuercode: 10
// Proz: 0%
// Konto: 4000
// InvoiceText: "Steuerfreie Ausfuhrlieferung gem. § 6 Abs. 1 Z 1 UStG"
// RequiredFields: ["Firmenname", "Lieferadresse außerhalb EU", "Zollnummer"]
// RequiresUidValidation: false
```

---

## 📋 Полная таблица Steuercode (BMD)

| Code | Описание | Использование |
|------|----------|---------------|
| **1** | Umsatzsteuer | Стандартная поставка в AT, 20% |
| **2** | Vorsteuer | Стандартная поставка в AT, 10% |
| **10** | Erwerbe gem. Art. 3/8 | Экспорт за пределы ЕС |
| **11** | Erwerbe gem. Art. 3/8, Art. 25/2 | IGL + Dreiecksgeschäft |
| **16** | Kleinunternehmer §6/1 Z 27 | Малое предприятие |
| **19** | Aufw. §19/1 Reverse Charge | Reverse Charge услуги в ЕС |
| **42** | VSt nicht abzugsfähig | НДС не вычитается |
| **43** | Steuerschuld gem. §11/12 и 14, §16/2 | Налоговая обязанность особые случаи |

*(Полный список 99 кодов в BMD NTCS)*

---

## 🎯 Интеграция с Invoice

### Шаг 1: Определение налогового случая
```csharp
var taxEngine = new AustrianTaxLogicEngine();

var input = new TaxCaseInput
{
    BuyerCountry = customer.Country,
    BuyerCountryInEU = IsEUCountry(customer.Country),
    BuyerUid = customer.VatId,
    IsGoodsSupply = invoice.IsGoodsInvoice,
    SellerIsSmallBusiness = company.IsSmallBusiness
};

var taxResult = taxEngine.DetermineTaxCase(input);
```

### Шаг 2: Применение к Invoice
```csharp
invoice.InvoiceType = taxResult.InvoiceType;
invoice.IsReverseCharge = taxResult.IsReverseCharge;
invoice.IsSmallBusinessExemption = taxResult.IsSmallBusinessExemption;
invoice.IsTaxFreeExport = taxResult.IsTaxFreeExport;
invoice.IsIntraEUSale = taxResult.IsIntraEUSale;

// New fields (need migration):
invoice.Steuercode = taxResult.Steuercode;
invoice.Konto = taxResult.Konto.ToString();
invoice.Proz = taxResult.Proz;

// Calculate tax
invoice.TaxAmount = invoice.SubTotal * (taxResult.VatRate / 100);
invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;
```

### Шаг 3: Проверка обязательных полей
```csharp
if (taxResult.RequiresUidValidation)
{
    if (string.IsNullOrEmpty(customer.VatId))
    {
        throw new ValidationException("UID Käufer ist erforderlich für diesen Steuerfall");
    }

    // Validate via VIES
    var isValid = await _viesService.ValidateVatIdAsync(customer.VatId);
    if (!isValid)
    {
        throw new ValidationException("UID Käufer ist ungültig");
    }
}
```

---

## 🧪 Тестовые сценарии

### Test 1: Kleinunternehmer → INLAND
```
Input: Small business selling to Austrian customer
Expected: StC 16, 0%, Konto 4062
```

### Test 2: INLAND → IGL
```
Input: Normal AT company → German customer with UID, goods
Expected: StC 11, 0%, Konto 4000, requires UID validation
```

### Test 3: IGL → Reverse Charge
```
Input: Change from goods to services (same German customer)
Expected: StC 19, 0%, Konto 4000
```

### Test 4: INLAND → Export
```
Input: Customer changes country to US
Expected: StC 10, 0%, Konto 4000, requires customs number
```

### Test 5: Edge Case - EU customer without UID
```
Input: French customer, no UID provided
Expected: Should fall back to Inland (20% VAT) or reject?
```

---

## ⚠️ Важные замечания

### 1. UID Validation
- IGL требует валидацию через **VIES** (EU VAT Information Exchange System)
- Reverse Charge также требует UID обоих сторон
- Kleinunternehmer НЕ МОЖЕТ иметь UID

### 2. Konto (Счета доходов)
Текущая реализация использует:
- **4000**: Стандартный счёт доходов
- **4062**: Kleinunternehmer

**TODO:** Интеграция с `Erlöskonten.xlsx` для полной таблицы счетов

### 3. Документация
Для каждого случая требуются разные документы:
- **IGL**: Lieferschein (товарная накладная)
- **Export**: Zollnummer (таможенный документ)
- **Reverse Charge**: Ничего особого

### 4. Steuercode mapping
Текущая реализация покрывает основные 6 случаев.
**TODO:** Расширение до всех 99 кодов BMD

---

## 🚀 Roadmap

### Phase 1: Core Engine ✅
- [x] Basic tax case determination
- [x] 6 main tax cases implemented
- [x] Steuercode assignment
- [x] Invoice text generation

### Phase 2: Data Integration (TODO)
- [ ] Parse `Erlöskonten.xlsx` → complete Konto mapping
- [ ] Parse `Steuerkonten.xlsx` → complete Steuercode table
- [ ] Parse all `Rechnungsmerkmale_*.pdf` → validation rules
- [ ] Add all 99 Steuercode descriptions

### Phase 3: Validation (TODO)
- [ ] VIES integration for UID validation
- [ ] Custom validation rules per tax case
- [ ] Required fields enforcement
- [ ] Customs number validation

### Phase 4: UI Integration (TODO)
- [ ] Add Steuercode display on invoice form
- [ ] Show required fields dynamically
- [ ] Tax case indicator badge
- [ ] Auto-fill legal text on PDF

### Phase 5: Testing (TODO)
- [ ] Unit tests for all 6 tax cases
- [ ] Integration tests with Invoice entity
- [ ] E2E tests with PDF generation
- [ ] Edge case testing (missing UID, invalid country, etc.)

---

## 📞 Support

**Документация:**
- `INVOICE_TYPES_EXPLANATION.md` - Объяснение типов счетов
- `INVOICE_TYPES_QUICK_REFERENCE.md` - Быстрая справка
- Папка `tabellen\шаблон BILANZ\1_AR_outbound_исходящие счета` - Примеры и инструкции

**Законодательство:**
- [UStG Austria](https://www.ris.bka.gv.at/)
- [BMD NTCS Documentation](https://www.bmd.com/)
- [VIES VAT Validation](https://ec.europa.eu/taxation_customs/vies/)
