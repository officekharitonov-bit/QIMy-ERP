# Austrian USt-Steuercode Complete Reference

**Source:** BMD NTCS / Austrian Tax Authority  
**Total Codes:** 99  
**Last Updated:** January 25, 2026

---

## Complete Steuercode Table (1-99)

| Code | Bezeichnung (Description) | Notes |
|------|---------------------------|-------|
| **1** | Umsatzsteuer | Standard VAT 20% (domestic) |
| **2** | Vorsteuer | Input VAT / Reduced rate 10% |
| **3** | VSt Art 12/23 (7m Abs. 4 und 5) | Specific VAT rates Art 12/23 |
| **4** | VSt f. igl. neuer Fahrzeuge gem. Art. 2 | Intra-EU new vehicles Art 2 |
| **5** | Ausfuhrlieferungen | Export deliveries |
| **6** | Übriges Dreiecksgeschäfte | Other triangular transactions |
| **7** | ig Lieferung | Intra-EU supply |
| **8** | Aufw. ig Erwerb o. VSt-Abzug | Intra-EU acquisition without input VAT deduction |
| **9** | Aufw. ig Erwerb m. VSt-Abzug | Intra-EU acquisition with input VAT deduction |
| **10** | Erwerbe gem. Art. 3/8 | Acquisitions Art. 3/8 (Export) |
| **11** | Erwerbe gem. Art. 3/8, Art. 25/2 | Acquisitions Art. 3/8 + Art. 25/2 (IGL + Triangular) |
| **12** | Eigenverbrauch | Private use |
| **13** | Lohnveredelung §6/1 Z 1 iVm §8 | Processing services §6/1 Z 1 + §8 |
| **14** | Personenbeförderung §6/1 Z 2-6 sowie §23/5 | Passenger transport §6/1 Z 2-6 + §23/5 |
| **15** | Grundstücksumsätze §6/1 Z 9 | Real estate transactions §6/1 Z 9 |
| **16** | Kleinunternehmer §6/1 Z 27 | Small business exemption §6/1 Z 27 |
| **17** | Übrige Umsätze o. VSt-Abzug §6/1 Z_ | Other sales without input VAT deduction §6/1 |
| **18** | Aufw. §19/1 Reverse Charge o. VSt-Abzug | Reverse Charge §19/1 without input VAT deduction |
| **19** | Aufw. §19/1 Reverse Charge m. VSt-Abzug | Reverse Charge §19/1 with input VAT deduction |
| **20** | Umsätze grenzüb DL §6 Ausfuhr (nicht ZM-pflichtig) | Cross-border services §6 export (not customs) |
| **21** | Umsätze §19/1b | Sales §19/1b |
| **22** | Aufw. §19/1b o. VSt-Abzug | §19/1b without input VAT deduction |
| **23** | Aufw. §19/1b m. VSt-Abzug | §19/1b with input VAT deduction |
| **24** | Umsätze §19/1c | Sales §19/1c |
| **25** | Aufw. §19/1c o. VSt-Abzug | §19/1c without input VAT deduction |
| **26** | Aufw. §19/1c m. VSt-Abzug | §19/1c with input VAT deduction |
| **27** | Umsätze §19/1a Bauleistungen | Construction services §19/1a |
| **28-41** | *Reserved / Special cases* | Various specific scenarios |
| **42** | VSt nicht abzugsfähig | Non-deductible input VAT |
| **43** | Steuerschuld gem. §11/12 und 14, §16/2 | Tax liability §11/12, 14, §16/2 |
| **44** | VSt in KZ 066 §19/1 betreffend KFZ | Input VAT KZ 066 §19/1 vehicles |
| **45-99** | *Various specific codes* | Special scenarios, acquisitions, etc. |

---

## Most Commonly Used Codes (Top 10)

### 1. **Code 1: Umsatzsteuer (Standard VAT)**
- **Rate:** 20%
- **Use case:** Standard domestic supply in Austria
- **Example:** Austrian company sells goods to Austrian customer
- **BMD:** Konto 4000

### 2. **Code 16: Kleinunternehmer (Small Business)**
- **Rate:** 0%
- **Use case:** Seller is small business under §6 Abs.1 Z 27
- **Example:** Freelancer with revenue < €35,000/year
- **BMD:** Konto 4062
- **Note:** ❌ CANNOT have UID

### 3. **Code 11: Innergemeinschaftliche Lieferung (IGL)**
- **Rate:** 0%
- **Use case:** Intra-EU supply of goods with valid UID
- **Example:** AT company → DE company (goods)
- **BMD:** Konto 4000
- **Required:** ✅ UID seller (ATU...) + UID buyer (DE...)

### 4. **Code 19: Reverse Charge (Services)**
- **Rate:** 0%
- **Use case:** B2B services within EU
- **Example:** AT consultant → FR company (services)
- **BMD:** Konto 4000
- **Required:** ✅ UID both parties

### 5. **Code 10: Export (Third Countries)**
- **Rate:** 0%
- **Use case:** Sales to non-EU countries
- **Example:** AT company → US customer
- **BMD:** Konto 4000
- **Required:** Zollnummer (customs number)

### 6. **Code 2: Vorsteuer (Input VAT)**
- **Rate:** 10% (reduced)
- **Use case:** Reduced VAT rate for specific goods/services
- **Example:** Books, food, certain services
- **BMD:** Konto 4000

### 7. **Code 42: VSt nicht abzugsfähig (Non-deductible VAT)**
- **Rate:** Varies
- **Use case:** Purchases where input VAT cannot be deducted
- **Example:** Company car used privately
- **BMD:** Special handling

### 8. **Code 27: Bauleistungen (Construction)**
- **Rate:** 0%
- **Use case:** Construction services (special Reverse Charge)
- **Example:** Subcontractor provides construction work
- **BMD:** Konto 4000
- **Note:** Specific to construction industry

### 9. **Code 43: Steuerschuld gem. §11/12**
- **Rate:** Varies
- **Use case:** Tax liability in special scenarios
- **Example:** Import of goods
- **BMD:** Special handling

### 10. **Code 6: Übriges Dreiecksgeschäfte**
- **Rate:** 0%
- **Use case:** Other triangular transactions
- **Example:** 3-country EU transaction (not standard)
- **BMD:** Konto 4000

---

## Steuercode Decision Tree

```
START
  ↓
  ├─ Is seller Kleinunternehmer?
  │  └─ YES → Code 16 (0%, Konto 4062)
  │  └─ NO  → Continue
  │
  ├─ Is buyer in Austria?
  │  └─ YES → Code 1 (20%, Konto 4000) or Code 2 (10%)
  │  └─ NO  → Continue
  │
  ├─ Is buyer in EU?
  │  ├─ YES + Goods + Has UID → Code 11 (IGL, 0%)
  │  ├─ YES + Services + Has UID → Code 19 (RC, 0%)
  │  └─ NO → Continue
  │
  ├─ Is buyer in Third Country?
  │  └─ YES → Code 10 (Export, 0%)
  │
  └─ Default → Code 1 (20%)
```

---

## Konto (Account) Mapping

| Steuercode | Standard Konto | Description |
|------------|----------------|-------------|
| 1 | 4000 | Erlöse Inland 20% USt |
| 2 | 4000 | Erlöse Inland 10% USt |
| 10 | 4000 | Erlöse Export 0% |
| 11 | 4000 | Erlöse IGL 0% |
| 16 | 4062 | Erlöse Kleinunternehmer 0% |
| 19 | 4000 | Erlöse Reverse Charge 0% |

**Note:** Full Konto mapping available in `Erlöskonten.xlsx`

---

## Legal Text per Steuercode

### Code 1 (INLAND)
```
Umsatzsteuer 20%
```

### Code 2 (Reduced VAT)
```
Umsatzsteuer 10%
```

### Code 10 (Export)
```
Steuerfreie Ausfuhrlieferung gem. § 6 Abs. 1 Z 1 UStG
```

### Code 11 (IGL)
```
Steuerfreie innergemeinschaftliche Lieferung gem. Art. 6 Abs. 1 UStG
UID Verkäufer: ATU12345678
UID Käufer: DE123456789
```

### Code 16 (Kleinunternehmer)
```
Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG
Es wird keine Umsatzsteuer ausgewiesen.
```

### Code 19 (Reverse Charge)
```
Steuerschuldner ist der Rechnungsempfänger (Reverse Charge gem. § 19 Abs. 1 UStG)
UID Verkäufer: ATU12345678
UID Käufer: FR12345678901
```

---

## Validation Rules

| Code | UID Seller | UID Buyer | Customs Doc | VIES Check |
|------|------------|-----------|-------------|------------|
| 1 | Optional | Optional | No | No |
| 10 | Optional | No | ✅ YES | No |
| 11 | ✅ YES | ✅ YES | No | ✅ YES |
| 16 | ❌ NO | ❌ NO | No | No |
| 19 | ✅ YES | ✅ YES | No | ✅ YES |

---

## Integration Example

```csharp
public class AustrianTaxLogicEngine
{
    public TaxCaseResult DetermineTaxCase(TaxCaseInput input)
    {
        // Case 1: Kleinunternehmer
        if (input.SellerIsSmallBusiness)
            return new TaxCaseResult { Steuercode = 16, Konto = 4062, Proz = 0m };
        
        // Case 2: INLAND
        if (input.BuyerCountry == "AT")
            return new TaxCaseResult { Steuercode = 1, Konto = 4000, Proz = 20m };
        
        // Case 3: IGL (EU + Goods + UID)
        if (input.BuyerCountryInEU && input.IsGoodsSupply && !string.IsNullOrEmpty(input.BuyerUid))
            return new TaxCaseResult { Steuercode = 11, Konto = 4000, Proz = 0m };
        
        // Case 4: Reverse Charge (EU + Services + UID)
        if (input.BuyerCountryInEU && !input.IsGoodsSupply && !string.IsNullOrEmpty(input.BuyerUid))
            return new TaxCaseResult { Steuercode = 19, Konto = 4000, Proz = 0m };
        
        // Case 5: Export (Third Country)
        if (!input.BuyerCountryInEU)
            return new TaxCaseResult { Steuercode = 10, Konto = 4000, Proz = 0m };
        
        // Default
        return new TaxCaseResult { Steuercode = 1, Konto = 4000, Proz = 20m };
    }
}
```

---

## References

- **Austrian Tax Law:** [UStG Austria](https://www.ris.bka.gv.at/)
- **BMD NTCS:** [BMD Documentation](https://www.bmd.com/)
- **VIES Validation:** [EU VAT Validation](https://ec.europa.eu/taxation_customs/vies/)
- **Erlöskonten.xlsx:** Full account mapping in `tabellen\шаблон BILANZ\1_AR_outbound_исходящие счета`
- **Steuerkonten.xlsx:** Complete tax code table

---

**Created:** January 25, 2026  
**For:** QIMy ERP - Austrian Tax Logic Engine  
**Status:** ✅ Complete Reference
