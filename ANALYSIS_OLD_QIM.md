# Анализ старого QIM и план улучшений для QIMy

## Дата анализа: 19 января 2026

---

## 🎯 Ключевые паттерны и решения из старого QIM

### 1. **Автоматическая нумерация клиентов (Code)**
**Старый QIM:**
```csharp
var code = new ObjectParameter("Code", typeof(int));
entities.GetNextClientCode(BusinessID, code);
m.Code = (int)code.Value;
```

**Система кодирования:**
- Клиенты начинаются на «2»:
  - 200000-229999 – Inland (Австрия)
  - 230000-259999 – EU (ЕС)
  - 260000-299999 – Drittland (Третьи страны)
- Поставщики на «3»:
  - 300000-329999 – Inland
  - 330000-359999 – EU
  - 360000-399999 – Drittland

**✅ Внедрить в QIMy:**
- Добавить ClientCode (int) в модель Client
- Создать stored procedure для автогенерации кодов
- Классификация по ClientArea (Inland/EU/Drittland)

---

### 2. **VIES Integration (VAT проверка)**
**Старый QIM:**
```csharp
public async Task<ActionResult> CheckUid(string uid)
{
    string countryCode = uid.Substring(0, 2);
    string vatNumber = uid.Substring(2);
    
    using (checkVatPortTypeClient client = new checkVatPortTypeClient())
    {
        checkVatResponse response = await client.checkVatAsync(countryCode, vatNumber);
        name = response.Body.name;
        address = response.Body.address;
    }
}
```

**JavaScript автозаполнение:**
```javascript
$("#UID").focusout(function () {
    if (!nameTextBox.value && !addressTextBox.value) {
        CheckUid(...); // Автоматический запрос при потере фокуса
    }
});
```

**✅ УЖЕ ВНЕДРЕНО в QIMy:**
- ✅ ViesService с SOAP API
- ✅ Автозаполнение через @bind-Value:after
- ✅ Debounce 500ms
- ✅ UI feedback (spinner, сообщения)

---

### 3. **Валидация уникальности**
**Старый QIM:**
```csharp
protected override Expression<Func<Client, bool>> AnyExists => 
    (m => m.BusinessID == BusinessID && 
          (m.Code == Posted.Code || 
           (m.UID == Posted.UID && m.Name == Posted.Name)));

protected override string AlreadyExistsErrTemplate => GlobalRes.ClientExists;
```

**✅ УЖЕ ВНЕДРЕНО в QIMy:**
- ✅ Проверка дубликатов VAT при Create/Update
- ✅ InvalidOperationException с информативным сообщением

---

### 4. **BaseController - Централизованная логика**
**Старый QIM:**
```csharp
public abstract class BaseController : Controller
{
    protected string UserID => User.Identity.GetUserId();
    protected int BusinessID => (int)RouteData.Values["biz"];
    protected virtual int PageSize => 20;
    
    protected Exception GetInitialException(Exception ex);
    protected string GetExceptionMessages(Exception ex);
}
```

**📋 ВНЕДРИТЬ в QIMy:**
- Создать BaseService с общими методами
- Метод GetExceptionMessages для логирования
- Пагинация с PageSize = 20

---

### 5. **ModelController<T> - Generic CRUD**
**Старый QIM:**
```csharp
public abstract class ModelController<TModel, TID> : BaseController
{
    protected abstract DbSet<TModel> DbSet { get; }
    protected abstract Expression<Func<TModel, bool>> WhereClause { get; }
    protected abstract void InitNewModel(TModel m);
    protected abstract bool IsOwn(TModel m);
    
    protected virtual async Task OnBeforeCreateEditGet(TModel m);
    protected virtual async Task OnAfterCreateEditPost(object data);
}
```

**📋 ВНЕДРИТЬ в QIMy:**
- Создать GenericService<T> с:
  - GetAll, GetById, Create, Update, Delete
  - OnBeforeCreate, OnAfterCreate hooks
  - Автоматическая фильтрация по BusinessID

---

### 6. **CSV Export/Import**
**Старый QIM:**
```csharp
public interface ICsvExporter<T>
{
    object GetExportToCsvObject(T entity);
    string FilePrefix { get; }
}

public interface ICsvImporter
{
    string[] ColumnNames { get; }
    string[] IntegersToCheck { get; }
    IEnumerable<Func<DataRow, string>> DuplicatesSelectors { get; }
    string ProcedureName { get; }
}
```

**📋 ВНЕДРИТЬ в QIMy:**
- Экспорт клиентов в CSV (для BMD/Exact/SAP интеграции)
- Импорт клиентов из CSV
- Настраиваемый разделитель (;, ,, |)

---

### 7. **Reports - PDF/Excel/Word генерация**
**Старый QIM:**
```csharp
private ActionResult FinalPartialExport(DateTime from, DateTime till, ReportExportType reportType)
{
    // Microsoft Reporting Services (RDLC)
    LocalReport localReport = new LocalReport();
    localReport.ReportPath = Server.MapPath("~/Reports/FinalReport.rdlc");
    
    ReportDataSource reportDataSource = new ReportDataSource();
    reportDataSource.Name = "DataSet";
    reportDataSource.Value = results;
    
    byte[] bytes = localReport.Render(reportType.ToString());
    return File(bytes, mimeType, fileName);
}
```

**📋 ВНЕДРИТЬ в QIMy:**
- QuestPDF для PDF генерации (современная альтернатива RDLC)
- Шаблоны счетов с логотипом
- Экспорт финальных отчетов (FinalReport)

---

### 8. **Localization - Мультиязычность**
**Старый QIM:**
```csharp
[Localization("en")]
public class ClientsController : BaseController
{
    // GlobalRes.Create, GlobalRes.Edit, GlobalRes.ClientExists
}
```

**App_LocalResources/GlobalRes.resx:**
- GlobalRes.Create = "Erstellen"
- GlobalRes.Edit = "Bearbeiten"
- GlobalRes.ClientExists = "Kunde existiert bereits"

**📋 ВНЕДРИТЬ в QIMy (Phase 2):**
- Localization для DE/EN/RU
- Resources файлы
- Переключатель языка в UI

---

### 9. **ClientType и ClientArea**
**Старый QIM:**
```csharp
public class Client
{
    public int ClientTypeID { get; set; } // B2B, B2C, Government, etc.
    public int ClientAreaID { get; set; }  // Inland, EU, Drittland
    
    public virtual ClientType ClientType { get; set; }
    public virtual ClientArea ClientArea { get; set; }
}
```

**📋 ВНЕДРИТЬ в QIMy:**
- Добавить ClientType (справочник типов клиентов)
- Добавить ClientArea (географическая классификация)
- Влияет на налоги и нумерацию

---

### 10. **QuickFields - Динамическая генерация форм**
**Старый QIM:**
```cshtml
@Html.QuickModelFields()
```
Автоматически генерировал поля формы на основе атрибутов модели.

**📋 НЕ ВНЕДРЯТЬ в QIMy:**
- В Blazor лучше явное объявление полей
- Больше контроля над UI/UX

---

## 🔥 Приоритеты внедрения

### Phase 1 - Критические улучшения (1-2 дня)

1. **✅ VIES Integration** - ГОТОВО
2. **✅ VAT Uniqueness Validation** - ГОТОВО
3. **📋 Client Code Autogeneration** - TODO
   - Создать SP GetNextClientCode
   - Добавить ClientCode в Client entity
   - Автозаполнение при Create

4. **📋 ClientArea и ClientType** - TODO
   - Миграция для справочников
   - UI для выбора типа/региона

### Phase 2 - Функциональность (3-5 дней)

5. **📋 CSV Export/Import** - TODO
   - Экспорт клиентов
   - Импорт из BMD/Exact

6. **📋 PDF Invoice Generation** - TODO
   - QuestPDF интеграция
   - Шаблон счета

7. **📋 Reports Module** - TODO
   - FinalReport (журнал регистрации)
   - VAT Summary Report

### Phase 3 - Polishing (5-7 дней)

8. **📋 Localization** - TODO
9. **📋 Generic Base Services** - TODO
10. **📋 Advanced Search/Filtering** - TODO

---

## 🎨 UI/UX улучшения из старого QIM

### Что НЕ брать:
- ❌ QuickFields (слишком магический)
- ❌ jQuery валидация (Blazor имеет встроенную)
- ❌ Inline JavaScript в views

### Что ОБЯЗАТЕЛЬНО брать:
- ✅ Автозаполнение UID при focusout
- ✅ Disabled поля во время AJAX запроса
- ✅ Placeholder "Waiting for response..."
- ✅ Validation messages рядом с полями

---

## 📊 Сравнительная таблица

| Функция | Старый QIM | Новый QIMy | Статус |
|---------|-----------|-----------|--------|
| VAT Check (VIES) | ✅ SOAP API | ✅ SOAP API | ✅ Готово |
| Автозаполнение | ✅ focusout | ✅ @bind-Value:after | ✅ Готово |
| Client Code | ✅ Auto | ❌ Нет | 📋 TODO |
| ClientType | ✅ B2B/B2C | ❌ Нет | 📋 TODO |
| ClientArea | ✅ Inland/EU/3rd | ❌ Нет | 📋 TODO |
| CSV Export | ✅ Да | ❌ Нет | 📋 TODO |
| PDF Reports | ✅ RDLC | ❌ Нет | 📋 TODO |
| Multi-language | ✅ DE/EN | ❌ Только RU | 📋 TODO |
| Soft Delete | ✅ IsDeleted | ✅ IsDeleted | ✅ Готово |
| Multi-tenancy | ✅ BusinessID | ✅ BusinessId | ✅ Готово |

---

## 🚀 Немедленные действия

### 1. Добавить Client Code (сегодня)
```sql
ALTER TABLE Clients ADD ClientCode INT NULL;
CREATE PROCEDURE GetNextClientCode
    @BusinessId INT,
    @ClientAreaId INT,
    @Code INT OUTPUT
AS
BEGIN
    -- Логика нумерации по областям
END
```

### 2. Добавить ClientType и ClientArea (сегодня)
```csharp
public enum ClientType
{
    B2B = 1,
    B2C = 2,
    Government = 3
}

public enum ClientArea
{
    Inland = 1,    // Австрия
    EU = 2,        // ЕС
    ThirdCountry = 3  // Третьи страны
}
```

### 3. Улучшить UI клиентов (завтра)
- Показывать ClientCode в списке
- Фильтр по ClientType
- Фильтр по ClientArea
- Сортировка по Code

---

## 💡 Выводы

**Старый QIM - это гениальная система с:**
1. ✅ Правильной архитектурой (BaseController, ModelController)
2. ✅ Интеграцией с VIES
3. ✅ Автоматической нумерацией
4. ✅ CSV экспортом/импортом
5. ✅ PDF отчетами

**Новый QIMy должен взять:**
- Все бизнес-логику (коды, типы, области)
- VIES интеграцию (уже есть!)
- Паттерны базовых классов
- Систему отчетов

**Новый QIMy НЕ должен брать:**
- jQuery / устаревший JavaScript
- RDLC отчеты (заменить на QuestPDF)
- Web Forms подходы

---

## 📝 Следующие шаги

1. **Сейчас:** Добавить ClientCode, ClientType, ClientArea
2. **Завтра:** CSV Export для клиентов
3. **После завтра:** PDF Invoice generation
4. **Следующая неделя:** Reports module

**Цель:** К концу недели QIMy должен быть функционально эквивалентен старому QIM по клиентам и счетам.
