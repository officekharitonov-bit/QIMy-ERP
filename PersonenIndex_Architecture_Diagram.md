# Personen Index.xlsx - Диаграмма структуры и связей

## Архитектура справочников

```
┌────────────────────────────────────────────────────────────────────────┐
│           PERSONEN INDEX.XLSX (6 ЛИСТОВ)                              │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  ┌─────────────────────┐                                              │
│  │  Sheet 1: Personen  │                                              │
│  │    Index (MAIN)     │                                              │
│  │  ═════════════════  │                                              │
│  │  • 22 columns       │                                              │
│  │  • Formulas to      │◄──┐                                           │
│  │    other sheets     │   │                                           │
│  └─────────────────────┘   │                                           │
│           │                 │                                           │
│    ┌──────┼──────┬──────────┼────────┐                                 │
│    ▼      ▼      ▼          ▼        ▼                                 │
│  ┌──────────────────┐  ┌──────────────────┐  ┌─────────────────────┐ │
│  │ Sheet 2: EU-RATE │  │ Sheet 3: CODE_   │  │ Sheet 4: Steuer-    │ │
│  │ (27 EU Countries)│  │ INDEX (Accounts) │  │ codes (Tax Codes)   │ │
│  ├──────────────────┤  ├──────────────────┤  ├─────────────────────┤ │
│  │ Code (AT,DE...)  │  │ Kto-Nr (4211...) │  │ Kto-Nr (100...)     │ │
│  │ Name (Österreich)│  │ Bezeichnung      │  │ Bezeichnung         │ │
│  │ VAT Rate (20%)   │  │ Kontoart (Ertrag)│  │ Kontoart (Aktiv...) │ │
│  │ Currency (EUR)   │  │ Code (77...)     │  │ USt-Pz (Tax Code)   │ │
│  │ Thresholds       │  │ USt-Pz           │  │ Automatik (Rules)   │ │
│  │ Kto-Nr (4000)    │  │                  │  │ Texte (Description) │ │
│  └──────────────────┘  └──────────────────┘  └─────────────────────┘ │
│           ▲                     ▲                       ▲               │
│           │                     │                       │               │
│    ┌──────┴──────────────────┬──┴───────────────────────┘               │
│    │                         │                                          │
│  ┌─────────────────┐    ┌────────────────────────┐                    │
│  │ Sheet 5: Sorted │    │ Sheet 6: Länder        │                    │
│  │ List            │    │ (Country Master Data)  │                    │
│  ├─────────────────┤    ├────────────────────────┤                    │
│  │ Kto-Nr (200000..│    │ Name DE (Österreich)   │                    │
│  │ Country Code    │    │ Account Range? (34700) │                    │
│  │ (AT, DE, BE...) │    │ Name EN (Austria)      │                    │
│  │ Description     │    │ Country Number (1)     │                    │
│  └─────────────────┘    └────────────────────────┘                    │
│           ▲                       ▲                                     │
│           └───────────────────────┘                                     │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

## Связи и VLOOKUP формулы

### Сценарий 1: Импорт клиента по коду страны
```
INPUT: Freifeld 01 = "DE" (Германия)
  │
  ├─→ VLOOKUP("DE", EU-RATE!Code...) → Standard Rate = 19%, Currency = EUR
  │
  ├─→ VLOOKUP("DE", Länder!...) → Country# = 2
  │
  └─→ VLOOKUP(200001, Sorted list!Kto-Nr...) → Description = "Barverkaufe DEUTSCHLAND"

OUTPUT: 
  • VAT Rate: 19%
  • Currency: EUR
  • Thresholds: 125000 EUR (purchase), 100000 EUR (supply)
  • Account: 200001
```

### Сценарий 2: Поиск налогового кода по номеру счета
```
INPUT: Kto-Nr = 4211 (Erlöse EU-HU-MOSS)
  │
  └─→ VLOOKUP(4211, Steuercodes!Kto-Nr...) 
      ├─ Beschreibung = "Erlöse, steuerpflichtig in anderem EU-Land - HU - MOSS"
      ├─ Kontoart = "Ertrag" (Revenue)
      ├─ Code = 1
      └─ USt-Pz = 27 (Tax code)

OUTPUT: Tax automation rules für HU (Hungary)
```

### Сценарий 3: Полная информация для документа
```
INPUT: Kunde = "Siemens Austria", Land = "AT"
  │
  ├─→ EU-RATE[AT] → VAT 20%, EUR, 11000/35000
  ├─→ Länder[AT] → Austria (№1)
  ├─→ Sorted list[200000] → Barverkäufe AUSTRIA
  └─→ Steuercodes[4000] → Account type, rules

OUTPUT: Complete invoice context
  • VAT Rate: 20%
  • Currency: EUR  
  • Payment Terms: Thresholds 11000/35000
  • Account: 4000 (Erlöskonten)
  • Tax automation: A (automatic)
```

## Таблицы в QIMy (рекомендация)

После загрузки Personen Index.xlsx в БД создать таблицы:

```sql
CREATE TABLE tblCountries (
    CountryID INT PRIMARY KEY,
    Code VARCHAR(2),           -- AT, DE, BE...
    NameDE VARCHAR(100),       -- Österreich, Deutschland...
    NameEN VARCHAR(100),       -- Austria, Germany...
    VATRate DECIMAL(5,2),      -- 20, 19, 21...
    Currency VARCHAR(3),       -- EUR, BGN, HRK...
    PurchaseThreshold DECIMAL(12,2),
    SupplyThreshold DECIMAL(12,2)
);

CREATE TABLE tblAccountCodes (
    AccountID INT PRIMARY KEY,
    Code VARCHAR(10),          -- 4000, 4113, 200001...
    NameDE VARCHAR(200),
    AccountType VARCHAR(50),   -- Ertrag, Aktiv, Passiv...
    TaxCode INT,              -- Tax code for automation
    IsAutomatic BIT
);

CREATE TABLE tblTaxCodes (
    TaxCodeID INT PRIMARY KEY,
    Code INT,                 -- 1, 20, 27...
    Description VARCHAR(200),
    AutomationRule VARCHAR(50)
);
```

## Примечание

Все 6 листов работают вместе через **VLOOKUP / INDEX-MATCH формулы**.

Главный лист "Personen Index" содержит 22 столбца данных клиентов, каждое поле 
заполняется через поиск в справочниках:

- Страна → EU-RATE (ставка, валюта, пороги)
- Номер счета → CODE_INDEX + Steuercodes (описание, налоговый код)
- Автоматизация → Steuercodes (USt-Pz, Automatik)

Структура оптимальна для управления **налоговой калькуляцией по странам ЕС**.
