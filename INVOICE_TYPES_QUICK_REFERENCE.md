# Австрийские типы счётов - Быстрая справка

## Сравнительная таблица

| Параметр | INLAND | EXPORT | INTRA-EU | REVERSE CHARGE | KLEINUNTERNEHMER |
|----------|--------|--------|----------|---|---|
| **VAT Rate** | **20%** | **0%** | **0%** | **0%** | **0%** |
| **Apply VAT to Invoice** | ✅ Yes | ❌ No | ❌ No | ❌ No | ❌ No |
| **VAT ID Required** | Optional | Yes | Yes | Yes | ❌ Not Allowed |
| **Reverse Charge** | ❌ No | ❌ No | ✅ Yes | ✅ Yes | ❌ No |
| **Country** | AT | Non-EU | EU | B2B | < €35K annually |
| **PDF Label** | Inlandslieferung | Ausfuhrlieferung | Innergemeinschaftliche Lieferung | Reverse Charge | Kleinunternehmer |
| **German Note** | Standard 20% | Tax-free export | EU reverse charge | Customer pays VAT | Small business exemption |

---

## Решение дерево - какой тип выбрать?

```
START: Какого клиента я считаю?
│
├─ Клиент в Австрии → INLAND (Domestic)
│   └─ VAT: 20% ✅
│
├─ Клиент за пределами ЕС → EXPORT
│   └─ VAT: 0% ✅
│
├─ Клиент в другой стране ЕС →Клиент в другой стране ЕС? →
│   ├─ B2B? → INTRA-EU
│   │   └─ VAT: 0% ✅ (Reverse charge on customer)
│   └─ B2C? → INLAND (apply VAT 20%)
│
├─ B2B услуга (консультинг, IT) → REVERSE CHARGE
│   └─ VAT: 0% ✅ (Customer liability)
│
└─ Мой оборот < €35K в год → KLEINUNTERNEHMER
    └─ VAT: 0% ✅ (Льгота, НИКОГДА не выставляй VAT!)
```

---

## PDF Примеры - ключевые отличия

### 1. INLAND (Внутригосударственная)
```
RECHNUNG
Inland - Inlandslieferung

Rechnungssteller: Компания AT
Rechnungsempfänger: Клиент AT

Leistung          | Menge | Preis    | Betrag
Услуга 1          |   1   | €100.00  | €100.00
─────────────────────────────────────────
Summe netto:                         €100.00
USt 20%:                              €20.00
─────────────────────────────────────────
Gesamtbetrag EUR:                    €120.00
```

### 2. EXPORT (Экспортный)
```
RECHNUNG
Exportrechnung - Ausfuhrlieferung

Rechnungssteller: Компания AT
Rechnungsempfänger: Клиент non-EU

Leistung          | Menge | Preis    | Betrag
Товар 1           |   1   | €100.00  | €100.00
─────────────────────────────────────────
Summe netto:                         €100.00
USt (steuerfrei):                      €0.00
─────────────────────────────────────────
Gesamtbetrag EUR:                    €100.00

* Ausfuhrlieferung - steuerfrei
```

### 3. INTRA-EU (Внутриобщиночная)
```
RECHNUNG
Innergemeinschaftliche Lieferung

Rechnungssteller: Компания AT
Rechnungsempfänger: Компания EU (с UID!)

Leistung          | Menge | Preis    | Betrag
Товар 1           |   1   | €120.00  | €120.00
─────────────────────────────────────────
Summe netto:                         €120.00
USt (Umkehrung):                       €0.00
─────────────────────────────────────────
Gesamtbetrag EUR:                    €120.00

* Innergemeinschaftliche Lieferung
```

### 4. REVERSE CHARGE (Обратное взимание)
```
RECHNUNG
Reverse Charge Rechnung

Rechnungssteller: Компания AT
Rechnungsempfänger: B2B Клиент

Leistung          | Menge | Preis    | Betrag
Услуга 1          |   1   | €150.00  | €150.00
─────────────────────────────────────────
Summe netto:                         €150.00
USt (Umkehrung):                       €0.00
─────────────────────────────────────────
Gesamtbetrag EUR:                    €150.00

* Reverse Charge (Umkehrung der Steuerschuld)
```

### 5. KLEINUNTERNEHMER (Малый предприниматель)
```
RECHNUNG
Kleinunternehmer

Rechnungssteller: Компания AT (малая)
Rechnungsempfänger: Клиент

Leistung          | Menge | Preis    | Betrag
Услуга 1          |   1   | €80.00   | €80.00
─────────────────────────────────────────
Summe netto:                          €80.00
USt (Kleinunternehmer):                €0.00
─────────────────────────────────────────
Gesamtbetrag EUR:                     €80.00

* Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG
```

---

## 🔴 Важные правила

### НИКОГДА делай эти комбинации:

❌ **Обе флага одновременно**:
- Reverse Charge + Small Business
- Reverse Charge + Tax Free Export
- Small Business + любой другой флаг

❌ **Small Business (Kleinunternehmer)**:
- НИКОГДА не выставляй VAT (всегда 0%)
- НИКОГДА не указывай VAT ID
- НИКОГДА не отнимай VAT от расходов

❌ **Export**:
- НЕ выставляй в AT
- ТРЕБУЕТ документов о вывозе
- Нужна страна доставки

---

## Код в системе

**В модели Invoice**:
```csharp
public InvoiceType InvoiceType { get; set; }
public bool IsReverseCharge { get; set; }
public bool IsSmallBusinessExemption { get; set; }
public bool IsTaxFreeExport { get; set; }
public bool IsIntraEUSale { get; set; }
```

**В сервисе AustrianInvoicePdfService**:
```csharp
private string GetInvoiceTypeLabel(InvoiceType type) => type switch
{
    InvoiceType.Domestic => "Inland - Inlandslieferung",
    InvoiceType.Export => "Exportrechnung - Ausfuhrlieferung",
    InvoiceType.IntraEUSale => "Innergemeinschaftliche Lieferung",
    InvoiceType.ReverseCharge => "Reverse Charge Rechnung",
    InvoiceType.SmallBusinessExemption => "Kleinunternehmer",
    _ => "Rechnung"
};
```

---

## Примеры в БД

```
ID | Number  | Type           | VAT  | Client                  | Amount
---|---------|----------------|------|-------------------------|-------
1  | 2026001 | Domestic       | 20%  | ALEMIRA GROUP           | €120
2  | 2026002 | Export         | 0%   | ALERO Handels           | €100
3  | 2026003 | IntraEUSale    | 0%   | ALERO Handels           | €120
4  | 2026004 | ReverseCharge  | 0%   | ANDREI GIGI             | €150
5  | 2026005 | SmallBusiness  | 0%   | ANDREI GIGI             | €80
```

---

## Чек-лист перед генерацией PDF

Перед тем как генерировать счет:

- [ ] Тип счета выбран правильно
- [ ] Дата счета указана
- [ ] Клиент имеет VAT ID (если требуется)
- [ ] Сумма правильна
- [ ] VAT % соответствует типу:
  - Domestic → 20%
  - Export → 0% (mark as export)
  - IntraEU → 0% (mark as EU sale)
  - ReverseCharge → 0% (flag set)
  - SmallBiz → 0% (flag set)

