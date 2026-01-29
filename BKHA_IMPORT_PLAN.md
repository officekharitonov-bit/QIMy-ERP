# ПЛАН ИМПОРТА ДАННЫХ BKHA GmbH

## 📊 Обнаруженные Файлы

### 1. **План Счетов (Sachkonten)**
**Файл:** `C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Sachkonten 2025 BKHA GmbH - 26.01.2025.csv`
- **Всего счетов:** 92
- **Структура:**
  ```
  Kto-Nr;Bezeichnung;Kontoart;Kontoklasse;USt-Kennzeichen;USt-Pz;USt-StC
  ```
- **Примеры:**
  - 210 - Grund- und Bodenanteil bei Gebäuden
  - 810 - Beteiligungen an Kapitalgesellschaften
  - 2000 - Forderungen aus Lieferungen und Leistungen Inland

### 2. **Клиенты и Поставщики (Personenkonten)**
**Файл:** `C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv`
- **Всего записей:** 14
- **Структура:**
  ```
  Externe KontoNr;Kto-Nr;Nachname;Freifeld 06;Straße;Plz;Ort;Währung;ZZiel;SktoProz1;SktoTage1;UID-Nummer;Freifeld 11;Lief-Vorschlag Gegenkonto;Freifeld 04;Freifeld 05;Kundenvorschlag Gegenkonto;Freifeld 02;Freifeld 03;Filial-Nr;Land-Nr;IBAN
  ```

### 3. **Клиенты (Smart Import Format)**
**Файл:** `C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Clients_2026-01-25_13-55-49.csv`
- Уже экспортирован из системы
- **Записи:**
  1. 200001 - Anatolii Skrypniak
  2. 230008 - Innogate Technology s. r. o. (Словакия)
  3. 230009 - Rich land s.r.o. (Чехия)
  4. 230007 - SIA "Ultra Trade Service" (Латвия)

### 4. **Документы Компании**
**Папка:** `C:\Projects\QIMy\tabellen\BKHA GmbH\STAMM\`
- Datenblatt Angaben zum Unternehmen.pdf
- Pass Mag. Kharitonov Egor.pdf
- Eröffnungsbilanz BKHA GmbH.pdf
- Bankauszug MKRTCHAN.pdf
- Meldezettel документы

**Папка:** `C:\Projects\QIMy\tabellen\BKHA GmbH\FA und ZOLL\`
- **Bescheid Abgabensteuer - UID-Nummer.pdf** ✅
- **EORI-Antrag.pdf** ✅
- Herabsetzungsantrag 2022.pdf

---

## 📋 ДАННЫЕ ДЛЯ ИМПОРТА

### BKHA GmbH - Информация о компании

Из анализа файлов и предыдущих экспортов:

```
Название:        BKHA GmbH
UID:             ATU77062005 (необходимо проверить в PDF)
FN:              [из PDF Firmenbuch]
Адрес:           [из Datenblatt]
EORI:            [из EORI-Antrag.pdf]
IBAN:            [из банковских документов]
Geschäftsführer: Mag. Egor Kharitonov
```

---

## 🎯 ПЛАН ИМПОРТА (ПОШАГОВО)

### ШАГ 1: Проверка/Создание Business BKHA GmbH

**Действие:** Проверить существует ли BKHA GmbH в таблице Businesses

```sql
SELECT * FROM Businesses WHERE Name = 'BKHA GmbH';
```

Если нет - создать через UI: `/admin/businesses/create`

**Данные для заполнения:**
- Name: `BKHA GmbH`
- VatNumber: `ATU77062005` (проверить в PDF)
- CompanyRegistrationNumber: `[FN из PDF]`
- Address: `[из Datenblatt]`
- City: `[из документов]`
- PostalCode: `[из документов]`
- Country: `Österreich`
- CustomsNumber (EORI): `[из EORI-Antrag.pdf]`
- BankAccount (IBAN): `[из банковских документов]`

---

### ШАГ 2: Импорт Плана Счетов (Sachkonten)

**Файл:** `Sachkonten 2025 BKHA GmbH - 26.01.2025.csv`
**Количество:** 92 счета
**Модуль:** Admin → Accounts (План счетов)

**Скрипт импорта:**
```powershell
# PowerShell скрипт для импорта Sachkonten
$csvPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Sachkonten 2025 BKHA GmbH - 26.01.2025.csv"
$businessId = 2  # BKHA GmbH BusinessId

# Читаем CSV (пропускаем первые 2 строки с мета-данными)
$content = Get-Content $csvPath -Encoding UTF8
$data = $content | Select-Object -Skip 2 | ConvertFrom-Csv -Delimiter ';'

# Маппинг колонок
foreach ($row in $data) {
    $account = @{
        AccountNumber = $row.'Kto-Nr'
        Name = $row.'Bezeichnung'
        AccountType = $row.'Kontoart'
        AccountClass = $row.'Kontoklasse'
        TaxIndicator = $row.'USt-Kennzeichen'
        TaxRate = $row.'USt-Pz'
        TaxCode = $row.'USt-StC'
        BusinessId = $businessId
    }

    # POST к API или CQRS Command
    Write-Host "Importing: $($account.AccountNumber) - $($account.Name)"
}
```

**Или через UI:**
1. Переключиться на BKHA GmbH в BusinessSelector
2. Перейти в Admin → Accounts → Import
3. Загрузить CSV (пропустив 2 строки)
4. Маппинг колонок:
   - Kto-Nr → AccountNumber
   - Bezeichnung → Name
   - Kontoart → AccountType
   - Kontoklasse → AccountClass

---

### ШАГ 3: Импорт Клиентов и Поставщиков (Personenkonten)

**Файл:** `PK 2025 - BKHA GmbH - 26-01-2026.csv`
**Количество:** 14 записей

**Разделение:**
- **Клиенты (200xxx):** Kto-Nr начинается с 2
- **Поставщики (230xxx):** Kto-Nr начинается с 23

#### 3.1 Импорт Клиентов

**URL:** `/AR/Clients/SmartImport`

**Колонки для маппинга:**
```
Kto-Nr → ClientCode
Nachname → CompanyName
Straße → Address
Plz → PostalCode
Ort → City
Währung → Currency
ZZiel → PaymentTerms
SktoProz1 → DiscountPercent
SktoTage1 → DiscountDays
UID-Nummer → VatNumber
Land-Nr → CountryCode
IBAN → BankAccount
```

**Клиенты для импорта:**
1. 200001 - Anatolii Skrypniak

#### 3.2 Импорт Поставщиков

**URL:** `/ER/Suppliers/Import`

**Поставщики для импорта:**
1. 230001 - ACCU-SERVICE NV (Бельгия)
2. 230002 - Motrex Sp z.o.o. (Польша)
3. 230003 - DEXTAN LTD (Кипр)
4. 230004 - JÁSZ-PLASZTIK KFT (Венгрия)
5. 230005 - EUROCOM RS Ltd (Болгария)
6. 230006 - FREEDOM QUALITY SERVICES, S.L (Испания)
7. 230007 - SIA "Ultra Trade Service" (Латвия)
8. 230008 - Innogate Technology s. r. o. (Словакия)
9. 230009 - Rich land s.r.o. (Чехия)

---

### ШАГ 4: Извлечение Данных из PDF

**Необходимо открыть и извлечь:**

#### 4.1 UID-Nummer
**Файл:** `FA und ZOLL\Bescheid Abgabensteuer - UID-Nummer.pdf`
**Искать:** ATU... (формат австрийского UID)

#### 4.2 EORI Number
**Файл:** `FA und ZOLL\EORI-Antrag.pdf`
**Искать:** AT... (таможенный номер)

#### 4.3 Firmenbuch Nummer (FN)
**Файл:** `STAMM\Datenblatt Angaben zum Unternehmen.pdf`
**Искать:** FN xxxxx x

#### 4.4 Адрес и Kontaktdaten
**Файл:** `STAMM\Datenblatt Angaben zum Unternehmen.pdf`
- Полный адрес
- Telefon
- E-Mail
- Website

#### 4.5 Bankkonto (IBAN)
**Файлы:**
- `STAMM\eBanking _ BAWAG Online Banking.pdf`
- `STAMM\Bankauszug MKRTCHAN.pdf`
- `STAMM\Verfügerantrag - George Vertrag BKHA.pdf`

**Искать:** AT... (IBAN начинается с AT)

---

## 🔧 СКРИПТЫ ДЛЯ АВТОМАТИЗАЦИИ

### Скрипт 1: Подготовка CSV для импорта клиентов

```powershell
# Фильтровать только клиентов (200xxx)
$pkPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv"
$outputPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Clients_BKHA_Import.csv"

$content = Get-Content $pkPath -Encoding UTF8
$data = $content | Select-Object -Skip 2 | ConvertFrom-Csv -Delimiter ';'

# Фильтр клиентов
$clients = $data | Where-Object { $_.'Kto-Nr' -like '200*' }

# Экспорт
$clients | Export-Csv -Path $outputPath -Delimiter ';' -Encoding UTF8 -NoTypeInformation

Write-Host "Exported $($clients.Count) clients to $outputPath"
```

### Скрипт 2: Подготовка CSV для импорта поставщиков

```powershell
# Фильтровать только поставщиков (230xxx)
$pkPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\PK 2025 - BKHA GmbH - 26-01-2026.csv"
$outputPath = "C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Suppliers_BKHA_Import.csv"

$content = Get-Content $pkPath -Encoding UTF8
$data = $content | Select-Object -Skip 2 | ConvertFrom-Csv -Delimiter ';'

# Фильтр поставщиков
$suppliers = $data | Where-Object { $_.'Kto-Nr' -like '23*' }

# Экспорт
$suppliers | Export-Csv -Path $outputPath -Delimiter ';' -Encoding UTF8 -NoTypeInformation

Write-Host "Exported $($suppliers.Count) suppliers to $outputPath"
```

---

## ✅ ЧЕКЛИСТ ИМПОРТА

- [ ] 1. Создать/проверить BKHA GmbH в Businesses
- [ ] 2. Извлечь UID из PDF `Bescheid Abgabensteuer - UID-Nummer.pdf`
- [ ] 3. Извлечь FN из `Datenblatt Angaben zum Unternehmen.pdf`
- [ ] 4. Извлечь EORI из `EORI-Antrag.pdf`
- [ ] 5. Извлечь IBAN из банковских документов
- [ ] 6. Обновить данные BKHA GmbH в системе
- [ ] 7. Импортировать План Счетов (92 счета)
- [ ] 8. Импортировать Клиентов (1 запись)
- [ ] 9. Импортировать Поставщиков (9 записей)
- [ ] 10. Проверить все импорты через BusinessSelector

---

## 🚀 НАЧАЛО ИМПОРТА

**Следующий шаг:** Открыть PDF-файлы для извлечения:
1. UID Number
2. Firmenbuch Number
3. EORI Number
4. IBAN

**Команда для открытия:**
```powershell
Start-Process "C:\Projects\QIMy\tabellen\BKHA GmbH\FA und ZOLL\Bescheid Abgabensteuer - UID-Nummer.pdf"
Start-Process "C:\Projects\QIMy\tabellen\BKHA GmbH\STAMM\Datenblatt Angaben zum Unternehmen.pdf"
Start-Process "C:\Projects\QIMy\tabellen\BKHA GmbH\FA und ZOLL\EORI-Antrag.pdf"
```
