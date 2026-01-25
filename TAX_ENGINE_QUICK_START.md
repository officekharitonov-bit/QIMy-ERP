# ⚡ Tax Logic Engine - Quick Start

## Быстрый старт (5 минут)

### 1. Что это?
**Austrian Tax Logic Engine** - система автоматического определения налогового случая для счетов.

### 2. Как использовать?

```csharp
using QIMy.Infrastructure.Services;

// В вашем Handler или Controller:
var taxService = new InvoiceTaxService();
var client = await _context.Clients.FindAsync(clientId);

// Применить налоговую логику к счёту:
taxService.ApplyTaxLogic(
    invoice, 
    client, 
    sellerIsSmallBusiness: false, 
    isGoodsSupply: true
);

// ГОТОВО! Теперь invoice имеет:
// - invoice.Steuercode (1-99)
// - invoice.Konto ("4000", "4062")
// - invoice.Proz (0, 10, 20)
// - invoice.TaxAmount (рассчитан)
// - invoice.TotalAmount (рассчитан)
```

### 3. Основные сценарии

| Сценарий | Страна | UID | Результат | Steuercode | Proz |
|----------|--------|-----|-----------|------------|------|
| **Австрийский клиент** | AT | - | INLAND | 1 | 20% |
| **Малое предприятие** | - | - | Kleinunternehmer | 16 | 0% |
| **Немецкий клиент (товары)** | DE | ✅ | IGL | 11 | 0% |
| **Французский клиент (услуги)** | FR | ✅ | Reverse Charge | 19 | 0% |
| **США** | US | - | Export | 10 | 0% |

### 4. Проверка

```bash
# Запустить тесты:
dotnet run --project TestTaxEngine/TestTaxEngine.csproj

# Ожидаемый вывод:
✅ Test 1: INLAND → StC 1, 20%
✅ Test 2: Kleinunternehmer → StC 16, 0%
✅ Test 3: IGL → StC 11, 0%
✅ Test 4: Reverse Charge → StC 19, 0%
✅ Test 5: Export → StC 10, 0%
```

### 5. Обязательные поля

| Tax Case | UID Seller | UID Buyer | Другое |
|----------|-----------|-----------|--------|
| INLAND | - | optional | - |
| Kleinunternehmer | ❌ НЕТ | ❌ НЕТ | - |
| IGL | ✅ ATU... | ✅ DE... | VIES validation |
| Reverse Charge | ✅ | ✅ | VIES validation |
| Export | optional | - | Zollnummer |

### 6. Валидация

```csharp
// Перед сохранением:
var (isValid, errors) = taxService.ValidateInvoice(invoice, client);

if (!isValid)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"❌ {error}");
    }
    return BadRequest(errors);
}
```

### 7. Файлы

```
✅ AustrianTaxLogicEngine.cs - Основной движок
✅ InvoiceTaxService.cs - Интеграционный сервис
✅ Invoice.cs - Расширен (Steuercode, Konto, Proz)
✅ Migration: 20260125134133_AddSteuercodeKontoProz
```

### 8. Документация

- **Полное руководство:** [TAX_LOGIC_ENGINE_GUIDE.md](TAX_LOGIC_ENGINE_GUIDE.md)
- **Лог сессии:** [SESSION_LOG_20260125_TAX_ENGINE.md](SESSION_LOG_20260125_TAX_ENGINE.md)
- **Типы счетов:** [INVOICE_TYPES_EXPLANATION.md](INVOICE_TYPES_EXPLANATION.md)

---

## ⚠️ Important Notes

1. **Kleinunternehmer НЕ МОЖЕТ иметь UID** → StC 16, 0%
2. **IGL требует UID обоих сторон** → Проверка через VIES
3. **Reverse Charge только для услуг B2B** → StC 19
4. **Export требует Zollnummer** → Таможенное подтверждение

---

## 🚀 Next Steps

1. Зарегистрировать `InvoiceTaxService` в DI (Program.cs)
2. Интегрировать в CreateInvoiceHandler
3. Обновить PDF генератор
4. Добавить UI индикаторы

**Готово! 🎉**
