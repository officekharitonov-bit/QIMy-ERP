# 🏗️ ЭКСПЕРТНЫЙ АНАЛИЗ АРХИТЕКТУРЫ И ПЛАН УЛУЧШЕНИЙ QIMy

## Дата: 21 января 2026

---

## 📊 ТЕКУЩЕЕ СОСТОЯНИЕ АРХИТЕКТУРЫ QIMy

### Структура проекта (Clean Architecture):
```
QIMy/
├── src/
│   ├── QIMy.Core/              ✅ Domain Layer
│   │   ├── Entities/           ✅ 22 сущности
│   │   ├── Enums/              ✅ ClientArea, ClientType (deprecated)
│   │   ├── Interfaces/         ⚠️ Только 3 интерфейса
│   │   ├── DTOs/               ❌ Пусто
│   │   └── Models/             ❌ Пусто
│   │
│   ├── QIMy.Infrastructure/    ✅ Data Access Layer
│   │   ├── Data/               ✅ DbContext, Configurations
│   │   ├── Services/           ⚠️ Только 5 сервисов
│   │   └── Migrations/         ✅ 2 миграции
│   │
│   ├── QIMy.Web/               ✅ Presentation Layer (Blazor Server)
│   │   ├── Components/Pages/   ⚠️ Смешанная структура
│   │   │   ├── AR/            ✅ Модуль AR
│   │   │   ├── Admin/         ✅ Модуль Admin
│   │   │   └── Account/       ✅ Аутентификация
│   │   └── wwwroot/           ✅ Static files
│   │
│   ├── QIMy.API/               ⚠️ Пустой (только скелет)
│   └── QIMy.Shared/            ❌ Пустой
│
└── tests/                      ❌ Отсутствуют
```

---

## 🔍 АНАЛИЗ: ЧТО РАБОТАЕТ ХОРОШО

### ✅ Сильные стороны текущей архитектуры:

#### 1. Clean Architecture Foundation
- **Разделение слоев**: Core → Infrastructure → Presentation
- **Dependency Injection**: Правильное использование DI
- **Separation of Concerns**: Entities отделены от UI

#### 2. Domain Model (Entities)
```csharp
✅ BaseEntity - общие свойства (Id, CreatedAt, UpdatedAt, IsDeleted)
✅ 22 сущности - полная модель данных
✅ Soft Delete - через IsDeleted flag
✅ Audit Trail - CreatedAt, UpdatedAt
✅ Navigation Properties - правильные связи FK
```

**Созданные сущности**:
- Client, ClientType, ClientArea
- Invoice, InvoiceItem, InvoiceDiscount
- Supplier, ExpenseInvoice, ExpenseInvoiceItem
- Product, Unit, Account, Tax, TaxRate
- Currency, PaymentMethod, Discount, BankAccount
- Business, AppUser, Payment

#### 3. Multi-tenancy
```csharp
✅ Business entity - каждое предприятие изолировано
✅ BusinessId в сущностях
⚠️ НО: Фильтрация по BusinessId не централизована
```

#### 4. ASP.NET Core Identity
```csharp
✅ AppUser extends IdentityUser
✅ Cookie Authentication работает
✅ Login/Logout/Register страницы
✅ Cascading Authentication State
```

#### 5. VIES Integration
```csharp
✅ ViesService - SOAP API интеграция
✅ Автозаполнение CompanyName/Address
✅ Валидация VAT номеров
✅ UI feedback (spinner, messages)
```

#### 6. Модульная структура UI
```
✅ AR Module (Ausgangsrechnungen) - Clients, Invoices
✅ Admin Module - Reference data (TaxRates, Accounts, Currencies, etc.)
⚠️ ER Module (Eingangsrechnungen) - отсутствует
⚠️ KA Module (Kassa) - отсутствует
```

---

## ❌ КРИТИЧЕСКИЕ ПРОБЛЕМЫ АРХИТЕКТУРЫ

### 1. ❌ Отсутствие слоя приложения (Application Layer)
**Проблема**: Бизнес-логика смешана с Infrastructure и Presentation
```csharp
// Текущая архитектура:
QIMy.Web → QIMy.Infrastructure.Services → QIMy.Core.Entities
           ↓
      ClientService (Infrastructure)
      - GenerateNextClientCodeAsync()  // ← Бизнес-логика в Infrastructure!
      - CreateClientAsync()
      - UpdateClientAsync()
```

**Решение**: Добавить **QIMy.Application** layer
```csharp
QIMy.Web → QIMy.Application → QIMy.Infrastructure → QIMy.Core
           ↓
      ClientApplicationService
      - Commands: CreateClientCommand, UpdateClientCommand
      - Queries: GetClientByIdQuery, GetAllClientsQuery
      - Handlers: Медиаторы для CQRS
```

---

### 2. ❌ Нет репозиториев (Repository Pattern)
**Проблема**: Сервисы напрямую используют DbContext
```csharp
// ClientService.cs (ТЕКУЩЕЕ - плохая практика)
public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;  // ← Direct DbContext!

    public async Task<Client> CreateClientAsync(Client client)
    {
        _context.Clients.Add(client);  // ← Прямой доступ к DbSet
        await _context.SaveChangesAsync();
        return client;
    }
}
```

**Решение**: Repository + Unit of Work
```csharp
// IRepository<T>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// IUnitOfWork
public interface IUnitOfWork : IDisposable
{
    IRepository<Client> Clients { get; }
    IRepository<Invoice> Invoices { get; }
    Task<int> SaveChangesAsync();
}

// Usage
public class ClientService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Client> CreateClientAsync(Client client)
    {
        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();
        return client;
    }
}
```

---

### 3. ❌ Нет CQRS (Command Query Responsibility Segregation)
**Проблема**: Запросы (Queries) и команды (Commands) смешаны
```csharp
// IClientService (ТЕКУЩЕЕ - нарушает SRP)
public interface IClientService
{
    // Queries
    Task<IEnumerable<Client>> GetAllClientsAsync();
    Task<Client?> GetClientByIdAsync(int id);

    // Commands
    Task<Client> CreateClientAsync(Client client);
    Task<Client> UpdateClientAsync(Client client);
    Task DeleteClientAsync(int id);
}
```

**Решение**: CQRS с MediatR
```csharp
// Commands
public record CreateClientCommand(Client Client) : IRequest<Client>;
public record UpdateClientCommand(Client Client) : IRequest<Client>;
public record DeleteClientCommand(int ClientId) : IRequest<Unit>;

// Queries
public record GetClientByIdQuery(int ClientId) : IRequest<Client?>;
public record GetAllClientsQuery() : IRequest<IEnumerable<Client>>;
public record SearchClientsQuery(string SearchTerm) : IRequest<IEnumerable<Client>>;

// Handlers
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Client>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<Client> _validator;

    public async Task<Client> Handle(CreateClientCommand request, CancellationToken ct)
    {
        // 1. Validate
        await _validator.ValidateAndThrowAsync(request.Client, ct);

        // 2. Generate ClientCode
        request.Client.ClientCode = await GenerateNextCodeAsync(request.Client.ClientAreaId);

        // 3. Save
        await _unitOfWork.Clients.AddAsync(request.Client);
        await _unitOfWork.SaveChangesAsync();

        return request.Client;
    }
}
```

---

### 4. ❌ Нет валидации (FluentValidation)
**Проблема**: Валидация через DataAnnotations - недостаточно
```csharp
// Client.cs (ТЕКУЩЕЕ - примитивная валидация)
public class Client : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; }  // ← Только базовая валидация

    public string? VatNumber { get; set; }   // ← Нет проверки формата
}
```

**Решение**: FluentValidation
```csharp
public class ClientValidator : AbstractValidator<Client>
{
    private readonly IClientRepository _clientRepo;

    public ClientValidator(IClientRepository clientRepo)
    {
        _clientRepo = clientRepo;

        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Название компании обязательно")
            .MaximumLength(200).WithMessage("Максимум 200 символов");

        RuleFor(c => c.VatNumber)
            .Must(BeValidVatNumber).When(c => !string.IsNullOrEmpty(c.VatNumber))
            .WithMessage("Неверный формат UID (например: ATU12345678)");

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
            .WithMessage("Неверный формат email");

        RuleFor(c => c.VatNumber)
            .MustAsync(BeUniqueVatNumber)
            .When(c => !string.IsNullOrEmpty(c.VatNumber))
            .WithMessage("Клиент с таким UID уже существует");
    }

    private bool BeValidVatNumber(string? vatNumber)
    {
        if (string.IsNullOrEmpty(vatNumber)) return true;
        return Regex.IsMatch(vatNumber, @"^[A-Z]{2}[A-Z0-9]{2,13}$");
    }

    private async Task<bool> BeUniqueVatNumber(Client client, string? vatNumber, CancellationToken ct)
    {
        var existing = await _clientRepo.GetByVatNumberAsync(vatNumber);
        return existing == null || existing.Id == client.Id;
    }
}
```

---

### 5. ❌ Нет DTOs (Data Transfer Objects)
**Проблема**: Entities передаются напрямую в UI
```csharp
// CreateEdit.razor (ТЕКУЩЕЕ - плохая практика)
@code {
    private Client client = new();  // ← Entity напрямую в UI!

    private async Task HandleValidSubmit()
    {
        await ClientService.CreateClientAsync(client);  // ← Передаем Entity!
    }
}
```

**Решение**: DTOs + AutoMapper
```csharp
// ClientDto.cs
public record ClientDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public int? ClientTypeId { get; init; }
    public string? ClientTypeName { get; init; }
    public int? ClientAreaId { get; init; }
    public string? ClientAreaName { get; init; }
}

// CreateClientDto.cs
public record CreateClientDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
}

// UpdateClientDto.cs
public record UpdateClientDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    // ... остальные поля
}

// AutoMapper Profile
public class ClientProfile : Profile
{
    public ClientProfile()
    {
        CreateMap<Client, ClientDto>()
            .ForMember(d => d.ClientTypeName, opt => opt.MapFrom(s => s.ClientType.Name))
            .ForMember(d => d.ClientAreaName, opt => opt.MapFrom(s => s.ClientArea.Name));

        CreateMap<CreateClientDto, Client>();
        CreateMap<UpdateClientDto, Client>();
    }
}

// Usage in UI
@code {
    private CreateClientDto clientDto = new();

    private async Task HandleValidSubmit()
    {
        var command = new CreateClientCommand(clientDto);
        var result = await Mediator.Send(command);
    }
}
```

---

### 6. ❌ Нет обработки ошибок (Global Exception Handling)
**Проблема**: Ошибки не обрабатываются централизованно
```csharp
// ClientService.cs (ТЕКУЩЕЕ - нет обработки ошибок)
public async Task<Client> CreateClientAsync(Client client)
{
    _context.Clients.Add(client);  // ← Может упасть с DbUpdateException
    await _context.SaveChangesAsync();  // ← Может упасть с SqlException
    return client;
}
```

**Решение**: Global Exception Handler + Result Pattern
```csharp
// Result<T> pattern
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// Custom Exceptions
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' not found") { }
}

public class DuplicateException : Exception
{
    public DuplicateException(string entityName, string field, object value)
        : base($"{entityName} with {field}='{value}' already exists") { }
}

// Global Exception Middleware (for API)
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (DuplicateException ex)
        {
            await HandleDuplicateException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnknownException(context, ex);
        }
    }

    private async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
    }
}

// Usage in Handler
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<Client>>
{
    public async Task<Result<Client>> Handle(CreateClientCommand request, CancellationToken ct)
    {
        try
        {
            // Validate
            var validationResult = await _validator.ValidateAsync(request.Client, ct);
            if (!validationResult.IsValid)
            {
                return Result<Client>.Failure(string.Join(", ", validationResult.Errors));
            }

            // Check duplicate VAT
            if (await _unitOfWork.Clients.ExistsByVatAsync(request.Client.VatNumber))
            {
                throw new DuplicateException("Client", "VatNumber", request.Client.VatNumber);
            }

            // Create
            await _unitOfWork.Clients.AddAsync(request.Client);
            await _unitOfWork.SaveChangesAsync();

            return Result<Client>.Success(request.Client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            return Result<Client>.Failure(ex.Message);
        }
    }
}
```

---

### 7. ❌ Нет логирования (Structured Logging)
**Проблема**: Минимальное логирование
```csharp
// ТЕКУЩЕЕ - нет логов
public async Task<Client> CreateClientAsync(Client client)
{
    _context.Clients.Add(client);
    await _context.SaveChangesAsync();
    return client;  // ← Нет информации о созданном клиенте
}
```

**Решение**: Serilog + Structured Logging
```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "QIMy")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/qimy-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Seq("http://localhost:5341")  // Seq для анализа логов
    .CreateLogger();

// Usage in Handler
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<Client>>
{
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public async Task<Result<Client>> Handle(CreateClientCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating client with CompanyName={CompanyName}, VatNumber={VatNumber}",
            request.Client.CompanyName, request.Client.VatNumber);

        try
        {
            // ... бизнес-логика

            _logger.LogInformation("Client created successfully with Id={ClientId}, Code={ClientCode}",
                client.Id, client.ClientCode);

            return Result<Client>.Success(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create client with CompanyName={CompanyName}",
                request.Client.CompanyName);
            return Result<Client>.Failure(ex.Message);
        }
    }
}
```

---

### 8. ❌ Нет кэширования (Caching)
**Проблема**: Каждый запрос идет в БД
```csharp
// ТЕКУЩЕЕ - каждый раз запрос к БД
public async Task<IEnumerable<ClientType>> GetAllClientTypesAsync()
{
    return await _context.ClientTypes.ToListAsync();  // ← Каждый раз в БД!
}
```

**Решение**: Redis + IMemoryCache
```csharp
// ICacheService
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}

// RedisCacheService
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _cache.GetStringAsync(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
        };
        await _cache.SetStringAsync(key, json, options);
    }
}

// Caching Decorator Pattern
public class CachedClientService : IClientService
{
    private readonly IClientService _inner;
    private readonly ICacheService _cache;

    public async Task<IEnumerable<ClientType>> GetAllClientTypesAsync()
    {
        const string cacheKey = "ClientTypes:All";

        var cached = await _cache.GetAsync<IEnumerable<ClientType>>(cacheKey);
        if (cached != null) return cached;

        var clientTypes = await _inner.GetAllClientTypesAsync();
        await _cache.SetAsync(cacheKey, clientTypes, TimeSpan.FromHours(24));

        return clientTypes;
    }
}
```

---

### 9. ❌ Нет тестов (Unit / Integration Tests)
**Проблема**: Отсутствует папка tests/
```
QIMy/
├── src/
└── tests/  ❌ ПУСТО
```

**Решение**: xUnit + Moq + FluentAssertions
```csharp
// tests/QIMy.Application.Tests/Clients/CreateClientCommandHandlerTests.cs
public class CreateClientCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IValidator<Client>> _validator;
    private readonly CreateClientCommandHandler _handler;

    public CreateClientCommandHandlerTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _validator = new Mock<IValidator<Client>>();
        _handler = new CreateClientCommandHandler(_unitOfWork.Object, _validator.Object);
    }

    [Fact]
    public async Task Handle_ValidClient_ShouldCreateClient()
    {
        // Arrange
        var client = new Client { CompanyName = "Test GmbH" };
        var command = new CreateClientCommand(client);

        _validator.Setup(v => v.ValidateAsync(client, default))
            .ReturnsAsync(new ValidationResult());

        _unitOfWork.Setup(u => u.Clients.AddAsync(client))
            .ReturnsAsync(client);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(client);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateVat_ShouldThrowException()
    {
        // Arrange
        var client = new Client { CompanyName = "Test GmbH", VatNumber = "ATU12345678" };
        var command = new CreateClientCommand(client);

        _unitOfWork.Setup(u => u.Clients.ExistsByVatAsync(client.VatNumber))
            .ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, default);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>()
            .WithMessage("*VatNumber*");
    }
}
```

---

### 10. ❌ Нет API документации (Swagger/OpenAPI)
**Проблема**: QIMy.API проект пустой
```
QIMy.API/
├── Controllers/
│   └── ClientsController.cs  ⚠️ Пустой
└── Program.cs                ⚠️ Минимальная конфигурация
```

**Решение**: Swagger + API Versioning + API Controllers
```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QIMy API",
        Version = "v1",
        Description = "Modern Cloud Accounting Software API",
        Contact = new OpenApiContact
        {
            Name = "Kharitonov Office",
            Email = "office@kharitonov.at"
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// ClientsController.cs
/// <summary>
/// Управление клиентами
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Получить список всех клиентов
    /// </summary>
    /// <returns>Список клиентов</returns>
    /// <response code="200">Успешно</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        var query = new GetAllClientsQuery();
        var clients = await _mediator.Send(query);
        return Ok(clients);
    }

    /// <summary>
    /// Создать нового клиента
    /// </summary>
    /// <param name="dto">Данные клиента</param>
    /// <returns>Созданный клиент</returns>
    /// <response code="201">Клиент создан</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="409">Клиент с таким UID уже существует</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientDto>> CreateClient(CreateClientDto dto)
    {
        var command = new CreateClientCommand(dto);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetClientById), new { id = result.Value.Id }, result.Value);
    }
}
```

---

## 🎯 ЦЕЛЕВАЯ АРХИТЕКТУРА (BEST PRACTICES)

### Слоистая архитектура с CQRS:
```
┌─────────────────────────────────────────────────────────┐
│                 QIMy.Web (Blazor Server)                │
│                 QIMy.API (REST API)                     │
├─────────────────────────────────────────────────────────┤
│            QIMy.Application (Use Cases)                 │
│  ┌────────────────────┬────────────────────┐           │
│  │  Commands          │  Queries           │           │
│  │  - CreateClient    │  - GetAllClients   │           │
│  │  - UpdateClient    │  - GetClientById   │           │
│  │  - DeleteClient    │  - SearchClients   │           │
│  └────────────────────┴────────────────────┘           │
│                    MediatR                              │
├─────────────────────────────────────────────────────────┤
│          QIMy.Infrastructure (Persistence)              │
│  ┌──────────────────────────────────────────┐          │
│  │  Repository Pattern + Unit of Work       │          │
│  │  - IRepository<T>                        │          │
│  │  - IUnitOfWork                           │          │
│  │  - ApplicationDbContext                  │          │
│  └──────────────────────────────────────────┘          │
├─────────────────────────────────────────────────────────┤
│              QIMy.Core (Domain)                         │
│  ┌──────────────────────────────────────────┐          │
│  │  Entities (22)                           │          │
│  │  - BaseEntity                            │          │
│  │  - Client, Invoice, Product...          │          │
│  │                                          │          │
│  │  Domain Events                           │          │
│  │  - ClientCreatedEvent                   │          │
│  │  - InvoicePaidEvent                     │          │
│  └──────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────┘

Cross-Cutting Concerns:
- Logging (Serilog)
- Caching (Redis)
- Validation (FluentValidation)
- Mapping (AutoMapper)
- Exception Handling
```

---

## 📋 ДЕТАЛЬНЫЙ ПЛАН РЕАЛИЗАЦИИ

### **ФАЗА 1: ФУНДАМЕНТ (1-2 недели)**

#### Задача 1.1: Создать Application Layer
```bash
dotnet new classlib -n QIMy.Application -f net8.0
cd QIMy.Application
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package AutoMapper
dotnet add reference ../QIMy.Core/QIMy.Core.csproj
```

**Структура**:
```
QIMy.Application/
├── Common/
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs
│   │   ├── LoggingBehaviour.cs
│   │   └── PerformanceBehaviour.cs
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   ├── NotFoundException.cs
│   │   └── DuplicateException.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── ICacheService.cs
│   └── Models/
│       └── Result.cs
├── Clients/
│   ├── Commands/
│   │   ├── CreateClient/
│   │   │   ├── CreateClientCommand.cs
│   │   │   ├── CreateClientCommandHandler.cs
│   │   │   └── CreateClientCommandValidator.cs
│   │   ├── UpdateClient/
│   │   └── DeleteClient/
│   ├── Queries/
│   │   ├── GetAllClients/
│   │   ├── GetClientById/
│   │   └── SearchClients/
│   └── DTOs/
│       ├── ClientDto.cs
│       ├── CreateClientDto.cs
│       └── UpdateClientDto.cs
├── Invoices/
│   ├── Commands/
│   ├── Queries/
│   └── DTOs/
└── MappingProfiles/
    ├── ClientProfile.cs
    └── InvoiceProfile.cs
```

**Файлы для создания**:

**1. Result.cs**
```csharp
namespace QIMy.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string[] Errors { get; }

    protected Result(bool isSuccess, string[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, Array.Empty<string>());
    public static Result Failure(params string[] errors) => new(false, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string[] errors) : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public new static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
```

**2. IRepository.cs**
```csharp
namespace QIMy.Application.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
```

**3. IUnitOfWork.cs**
```csharp
namespace QIMy.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Client> Clients { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceItem> InvoiceItems { get; }
    IRepository<Product> Products { get; }
    IRepository<TaxRate> TaxRates { get; }
    IRepository<Currency> Currencies { get; }
    IRepository<Account> Accounts { get; }
    IRepository<BankAccount> BankAccounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**4. CreateClientCommand.cs**
```csharp
namespace QIMy.Application.Clients.Commands.CreateClient;

public record CreateClientCommand : IRequest<Result<ClientDto>>
{
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? VatNumber { get; init; }
    public int? ClientTypeId { get; init; }
    public int? ClientAreaId { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
}
```

**5. CreateClientCommandHandler.cs**
```csharp
namespace QIMy.Application.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public CreateClientCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateClientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating client: {CompanyName}", request.CompanyName);

        // Check duplicate VAT
        if (!string.IsNullOrEmpty(request.VatNumber))
        {
            var existingClients = await _unitOfWork.Clients
                .FindAsync(c => c.VatNumber == request.VatNumber && !c.IsDeleted, ct);

            if (existingClients.Any())
            {
                _logger.LogWarning("Client with VAT {VatNumber} already exists", request.VatNumber);
                return Result<ClientDto>.Failure($"Клиент с UID {request.VatNumber} уже существует");
            }
        }

        // Map to entity
        var client = _mapper.Map<Client>(request);

        // Generate ClientCode
        client.ClientCode = await GenerateNextClientCodeAsync(client.ClientAreaId, ct);

        // Save
        await _unitOfWork.Clients.AddAsync(client, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Client created: Id={ClientId}, Code={ClientCode}",
            client.Id, client.ClientCode);

        // Map to DTO
        var dto = _mapper.Map<ClientDto>(client);
        return Result<ClientDto>.Success(dto);
    }

    private async Task<int> GenerateNextClientCodeAsync(int? clientAreaId, CancellationToken ct)
    {
        int baseCode = clientAreaId switch
        {
            1 => 200000, // Inland
            2 => 230000, // EU
            3 => 260000, // Drittland
            _ => 200000
        };

        int maxRange = baseCode + 29999;

        var clients = await _unitOfWork.Clients
            .FindAsync(c => c.ClientCode >= baseCode && c.ClientCode <= maxRange, ct);

        var maxCode = clients.MaxBy(c => c.ClientCode)?.ClientCode;
        return maxCode.HasValue ? maxCode.Value + 1 : baseCode;
    }
}
```

**6. CreateClientCommandValidator.cs**
```csharp
namespace QIMy.Application.Clients.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Название компании обязательно")
            .MaximumLength(200).WithMessage("Максимум 200 символов");

        RuleFor(c => c.VatNumber)
            .Matches(@"^[A-Z]{2}[A-Z0-9]{2,13}$")
            .When(c => !string.IsNullOrEmpty(c.VatNumber))
            .WithMessage("Неверный формат UID (например: ATU12345678)");

        RuleFor(c => c.Email)
            .EmailAddress()
            .When(c => !string.IsNullOrEmpty(c.Email))
            .WithMessage("Неверный формат email");

        RuleFor(c => c.ClientTypeId)
            .GreaterThan(0)
            .When(c => c.ClientTypeId.HasValue)
            .WithMessage("Неверный тип клиента");

        RuleFor(c => c.ClientAreaId)
            .GreaterThan(0)
            .When(c => c.ClientAreaId.HasValue)
            .WithMessage("Неверная область клиента");
    }
}
```

**7. ValidationBehaviour.cs**
```csharp
namespace QIMy.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

---

#### Задача 1.2: Реализовать Repository + UnitOfWork в Infrastructure

**Файлы**:

**1. Repository.cs**
```csharp
namespace QIMy.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.Where(predicate).Where(e => !e.IsDeleted).ToListAsync(ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(entity, ct);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted, ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        if (predicate != null) query = query.Where(predicate);
        return await query.CountAsync(ct);
    }
}
```

**2. ClientRepository.cs** (Специализированный репозиторий)
```csharp
namespace QIMy.Infrastructure.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.ClientCode)
            .ToListAsync(ct);
    }

    public override async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task<Client?> GetByVatNumberAsync(string vatNumber, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.VatNumber == vatNumber && !c.IsDeleted, ct);
    }

    public async Task<bool> ExistsByVatAsync(string vatNumber, CancellationToken ct = default)
    {
        return await _dbSet
            .AnyAsync(c => c.VatNumber == vatNumber && !c.IsDeleted, ct);
    }
}
```

**3. UnitOfWork.cs**
```csharp
namespace QIMy.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<Client>? _clients;
    private IRepository<Invoice>? _invoices;
    private IRepository<InvoiceItem>? _invoiceItems;
    private IRepository<Product>? _products;
    // ... остальные репозитории

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Client> Clients =>
        _clients ??= new ClientRepository(_context);

    public IRepository<Invoice> Invoices =>
        _invoices ??= new Repository<Invoice>(_context);

    public IRepository<InvoiceItem> InvoiceItems =>
        _invoiceItems ??= new Repository<InvoiceItem>(_context);

    // ... остальные репозитории

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

#### Задача 1.3: Подключить MediatR + FluentValidation + AutoMapper

**Program.cs (Web)**:
```csharp
// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateClientCommand).Assembly);

    // Add behaviours
    cfg.AddBehavior<IPipelineBehavior<CreateClientCommand, Result<ClientDto>>, ValidationBehaviour<CreateClientCommand, Result<ClientDto>>>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateClientCommandValidator).Assembly);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ClientProfile).Assembly);

// Add UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Replace old services
// builder.Services.AddScoped<IClientService, ClientService>(); // ← УДАЛИТЬ
// Теперь используем MediatR напрямую
```

---

### **ФАЗА 2: МИГРАЦИЯ СУЩЕСТВУЮЩИХ МОДУЛЕЙ (2-3 недели)**

#### Задача 2.1: Мигрировать модуль Clients на CQRS

**Создать команды**:
- CreateClientCommand ✅ (уже создали)
- UpdateClientCommand
- DeleteClientCommand
- ImportClientsCommand
- ExportClientsCommand

**Создать запросы**:
- GetAllClientsQuery
- GetClientByIdQuery
- SearchClientsQuery
- GetClientByVatQuery

**Обновить UI**:
```csharp
// CreateEdit.razor (БЫЛО)
@inject IClientService ClientService
private Client client = new();

private async Task HandleValidSubmit()
{
    await ClientService.CreateClientAsync(client);
}

// CreateEdit.razor (СТАЛО)
@inject IMediator Mediator
private CreateClientDto clientDto = new();

private async Task HandleValidSubmit()
{
    var command = new CreateClientCommand
    {
        CompanyName = clientDto.CompanyName,
        VatNumber = clientDto.VatNumber,
        // ... остальные поля
    };

    var result = await Mediator.Send(command);

    if (result.IsSuccess)
    {
        NavigationManager.NavigateTo("/ar/clients");
    }
    else
    {
        errorMessage = string.Join(", ", result.Errors);
    }
}
```

---

#### Задача 2.2: Мигрировать модуль Invoices на CQRS

**Создать команды**:
- CreateInvoiceCommand
- UpdateInvoiceCommand
- DeleteInvoiceCommand
- SendInvoiceCommand
- PayInvoiceCommand
- CancelInvoiceCommand

**Создать запросы**:
- GetAllInvoicesQuery
- GetInvoiceByIdQuery
- GetInvoicesByClientIdQuery
- SearchInvoicesQuery
- GetOverdueInvoicesQuery

---

#### Задача 2.3: Мигрировать модуль Admin (Reference Data)

**Создать команды/запросы для каждой сущности**:
- TaxRates: Create, Update, Delete, GetAll, GetById
- Accounts: Create, Update, Delete, GetAll, GetById
- Currencies: Create, Update, Delete, GetAll, GetById
- Products: Create, Update, Delete, GetAll, GetById, Search
- Units: Create, Update, Delete, GetAll, GetById
- PaymentMethods: Create, Update, Delete, GetAll, GetById
- Discounts: Create, Update, Delete, GetAll, GetById
- BankAccounts: Create, Update, Delete, GetAll, GetById
- Businesses: Create, Update, Delete, GetAll, GetById

---

### **ФАЗА 3: НОВЫЕ МОДУЛИ (3-4 недели)**

#### Задача 3.1: Модуль ER (Eingangsrechnungen) - Входящие счета

**Сущности** (уже есть):
- ExpenseInvoice
- ExpenseInvoiceItem
- Supplier

**Команды**:
- CreateExpenseInvoiceCommand
- UpdateExpenseInvoiceCommand
- DeleteExpenseInvoiceCommand
- ApproveExpenseInvoiceCommand
- PayExpenseInvoiceCommand

**Запросы**:
- GetAllExpenseInvoicesQuery
- GetExpenseInvoiceByIdQuery
- GetExpenseInvoicesBySupplierIdQuery
- SearchExpenseInvoicesQuery

**UI страницы**:
```
src/QIMy.Web/Components/Pages/ER/
├── Suppliers/
│   ├── Index.razor
│   └── CreateEdit.razor
└── ExpenseInvoices/
    ├── Index.razor
    ├── CreateEdit.razor
    └── Details.razor
```

---

#### Задача 3.2: Модуль KA (Kassa) - Касса

**Новые сущности**:
```csharp
// QIMy.Core/Entities/CashTransaction.cs
public class CashTransaction : BaseEntity
{
    public int BusinessId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public CashTransactionType Type { get; set; } // Income/Expense
    public string? Description { get; set; }
    public string? DocumentNumber { get; set; }
    public int? ClientId { get; set; }
    public int? SupplierId { get; set; }

    public Business Business { get; set; } = null!;
    public Client? Client { get; set; }
    public Supplier? Supplier { get; set; }
}

public enum CashTransactionType
{
    Income,   // Приход
    Expense   // Расход
}

// Kassabuch (Кассовая книга)
public class CashBook : BaseEntity
{
    public int BusinessId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }

    public Business Business { get; set; } = null!;
    public ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
}
```

**Команды**:
- CreateCashTransactionCommand
- UpdateCashTransactionCommand
- DeleteCashTransactionCommand
- CloseCashBookCommand

**Запросы**:
- GetCashBookQuery (по месяцу/году)
- GetCashTransactionsByPeriodQuery
- GetCashBalanceQuery

**UI страницы**:
```
src/QIMy.Web/Components/Pages/KA/
├── CashBook/
│   ├── Index.razor          (Кассовая книга - таблица транзакций)
│   └── CreateEdit.razor     (Добавление транзакции)
└── Reports/
    └── CashBookReport.razor  (Отчет за месяц/квартал/год)
```

---

### **ФАЗА 4: REPORTS & EXPORT (2-3 недели)**

#### Задача 4.1: PDF Generation с QuestPDF

**Установка**:
```bash
dotnet add package QuestPDF
```

**Создать сервисы**:

**IPdfService.cs**:
```csharp
namespace QIMy.Application.Common.Interfaces;

public interface IPdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice);
    byte[] GenerateFinalReportPdf(DateTime from, DateTime till);
    byte[] GenerateVatSummaryPdf(int year, int month);
}
```

**InvoicePdfService.cs**:
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QIMy.Infrastructure.Services;

public class InvoicePdfService : IPdfService
{
    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(invoice.Business.Name).FontSize(20).Bold();
                        column.Item().Text(invoice.Business.Address);
                        column.Item().Text($"{invoice.Business.PostalCode} {invoice.Business.City}");
                        column.Item().Text($"UID: {invoice.Business.VatNumber}");
                    });

                    row.ConstantItem(100).Height(100).Image(invoice.Business.Logo);
                });

                // Content
                page.Content().PaddingVertical(20).Column(column =>
                {
                    // Client info
                    column.Item().Text($"Rechnung an:").FontSize(12).Bold();
                    column.Item().Text(invoice.Client.CompanyName);
                    column.Item().Text(invoice.Client.Address);
                    column.Item().Text($"{invoice.Client.PostalCode} {invoice.Client.City}");

                    column.Item().PaddingVertical(20).LineHorizontal(1);

                    // Invoice info
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Rechnungsnummer: {invoice.InvoiceNumber}");
                        row.RelativeItem().Text($"Datum: {invoice.InvoiceDate:dd.MM.yyyy}");
                    });

                    column.Item().PaddingVertical(10);

                    // Items table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Description
                            columns.RelativeColumn(1); // Quantity
                            columns.RelativeColumn(1); // UnitPrice
                            columns.RelativeColumn(1); // TaxRate
                            columns.RelativeColumn(1); // Total
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Beschreibung");
                            header.Cell().Element(CellStyle).Text("Menge");
                            header.Cell().Element(CellStyle).Text("Preis");
                            header.Cell().Element(CellStyle).Text("USt %");
                            header.Cell().Element(CellStyle).Text("Gesamt");
                        });

                        // Rows
                        foreach (var item in invoice.Items)
                        {
                            table.Cell().Text(item.Description);
                            table.Cell().Text(item.Quantity.ToString("N2"));
                            table.Cell().Text(item.UnitPrice.ToString("C"));
                            table.Cell().Text($"{item.Tax.TaxRate.Rate}%");
                            table.Cell().Text(item.TotalAmount.ToString("C"));
                        }
                    });

                    column.Item().PaddingVertical(10);

                    // Totals
                    column.Item().AlignRight().Column(totals =>
                    {
                        totals.Item().Text($"Netto: {invoice.SubTotal:C}");
                        totals.Item().Text($"USt: {invoice.TaxAmount:C}");
                        totals.Item().Text($"Gesamt: {invoice.TotalAmount:C}").FontSize(14).Bold();
                    });

                    column.Item().PaddingVertical(20);

                    // Payment info
                    if (invoice.BankAccount != null)
                    {
                        column.Item().Text("Zahlungsinformationen:").Bold();
                        column.Item().Text($"Bank: {invoice.BankAccount.BankName}");
                        column.Item().Text($"IBAN: {invoice.BankAccount.IBAN}");
                        column.Item().Text($"BIC: {invoice.BankAccount.BIC}");
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span($"{invoice.Business.Name} | ");
                    text.Span($"UID: {invoice.Business.VatNumber} | ");
                    text.Span($"Email: {invoice.Business.Email}");
                });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten3)
            .PaddingVertical(5)
            .PaddingHorizontal(10)
            .AlignCenter()
            .AlignMiddle();
    }
}
```

---

#### Задача 4.2: CSV Export/Import

**IExportService.cs**:
```csharp
namespace QIMy.Application.Common.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportClientsAsync(CancellationToken ct = default);
    Task<byte[]> ExportInvoicesAsync(DateTime from, DateTime till, CancellationToken ct = default);
    Task<byte[]> ExportProductsAsync(CancellationToken ct = default);
}
```

**IImportService.cs**:
```csharp
namespace QIMy.Application.Common.Interfaces;

public interface IImportService
{
    Task<ImportResult> ImportClientsAsync(Stream fileStream, CancellationToken ct = default);
    Task<ImportResult> ImportProductsAsync(Stream fileStream, CancellationToken ct = default);
}

public record ImportResult
{
    public int SuccessCount { get; init; }
    public int ErrorCount { get; init; }
    public List<string> Errors { get; init; } = new();
}
```

---

### **ФАЗА 5: CROSS-CUTTING CONCERNS (1-2 недели)**

#### Задача 5.1: Serilog + Structured Logging

**Program.cs**:
```csharp
// Add Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "QIMy")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/qimy-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();
```

---

#### Задача 5.2: Redis Caching

**appsettings.json**:
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "QIMy:"
  }
}
```

**Program.cs**:
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();
```

---

#### Задача 5.3: API Development

**Создать полноценный REST API**:
```
src/QIMy.API/
├── Controllers/
│   ├── ClientsController.cs
│   ├── InvoicesController.cs
│   ├── ProductsController.cs
│   └── ReportsController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
└── Program.cs
```

---

### **ФАЗА 6: TESTING (2-3 недели)**

#### Задача 6.1: Unit Tests

**Создать проекты тестов**:
```bash
dotnet new xunit -n QIMy.Application.Tests
dotnet new xunit -n QIMy.Infrastructure.Tests
dotnet new xunit -n QIMy.Web.Tests

dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Bogus  # для генерации тестовых данных
```

**Структура тестов**:
```
tests/
├── QIMy.Application.Tests/
│   ├── Clients/
│   │   ├── Commands/
│   │   │   └── CreateClientCommandHandlerTests.cs
│   │   └── Queries/
│   │       └── GetClientByIdQueryHandlerTests.cs
│   └── Invoices/
│       └── Commands/
│           └── CreateInvoiceCommandHandlerTests.cs
├── QIMy.Infrastructure.Tests/
│   ├── Repositories/
│   │   └── ClientRepositoryTests.cs
│   └── Services/
│       └── ViesServiceTests.cs
└── QIMy.Web.Tests/
    └── Components/
        └── ClientsIndexTests.cs
```

---

#### Задача 6.2: Integration Tests

**Создать интеграционные тесты для API**:
```csharp
public class ClientsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task GetClients_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/clients");

        // Assert
        response.EnsureSuccessStatusCode();
        var clients = await response.Content.ReadFromJsonAsync<IEnumerable<ClientDto>>();
        clients.Should().NotBeNull();
    }
}
```

---

### **ФАЗА 7: DEPLOYMENT & CI/CD (1 неделя)**

#### Задача 7.1: Docker

**Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QIMy.Web/QIMy.Web.csproj", "QIMy.Web/"]
COPY ["src/QIMy.Application/QIMy.Application.csproj", "QIMy.Application/"]
COPY ["src/QIMy.Infrastructure/QIMy.Infrastructure.csproj", "QIMy.Infrastructure/"]
COPY ["src/QIMy.Core/QIMy.Core.csproj", "QIMy.Core/"]
RUN dotnet restore "QIMy.Web/QIMy.Web.csproj"
COPY src/ .
WORKDIR "/src/QIMy.Web"
RUN dotnet build "QIMy.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QIMy.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QIMy.Web.dll"]
```

**docker-compose.yml**:
```yaml
version: '3.8'

services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=QImyDb;User=sa;Password=YourStrong@Passw0rd;
    depends_on:
      - db
      - redis

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - qimy-data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      - ACCEPT_EULA=Y
    volumes:
      - seq-data:/data

volumes:
  qimy-data:
  redis-data:
  seq-data:
```

---

#### Задача 7.2: GitHub Actions CI/CD

**.github/workflows/ci.yml**:
```yaml
name: CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

    - name: Publish
      run: dotnet publish src/QIMy.Web/QIMy.Web.csproj -c Release -o ./publish

    - name: Deploy to Azure
      if: github.ref == 'refs/heads/main'
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'qimy-erp-app'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## 📊 ИТОГОВАЯ СТРУКТУРА ПРОЕКТА

```
QIMy/
├── src/
│   ├── QIMy.Core/                    ✅ Domain Layer (без изменений)
│   │   ├── Entities/                 ✅ 22 сущности
│   │   ├── Enums/                    ✅ Enums
│   │   └── Events/                   🆕 Domain Events
│   │
│   ├── QIMy.Application/             🆕 Application Layer (НОВЫЙ)
│   │   ├── Common/
│   │   │   ├── Behaviours/           🆕 MediatR Pipeline Behaviours
│   │   │   ├── Exceptions/           🆕 Custom Exceptions
│   │   │   ├── Interfaces/           🆕 IRepository, IUnitOfWork
│   │   │   └── Models/               🆕 Result<T>
│   │   ├── Clients/
│   │   │   ├── Commands/             🆕 CQRS Commands
│   │   │   ├── Queries/              🆕 CQRS Queries
│   │   │   └── DTOs/                 🆕 ClientDto, CreateClientDto
│   │   ├── Invoices/
│   │   ├── Products/
│   │   ├── Reports/
│   │   └── MappingProfiles/          🆕 AutoMapper Profiles
│   │
│   ├── QIMy.Infrastructure/          ⚙️ Infrastructure Layer (обновлен)
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── SeedData.cs
│   │   ├── Repositories/             🆕 Repository + UnitOfWork
│   │   │   ├── Repository.cs
│   │   │   ├── ClientRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Services/                 ⚙️ Обновлены
│   │   │   ├── ViesService.cs        ✅ Без изменений
│   │   │   ├── PdfService.cs         🆕 QuestPDF
│   │   │   ├── ExportService.cs      🆕 CSV Export
│   │   │   ├── ImportService.cs      🆕 CSV Import
│   │   │   └── CacheService.cs       🆕 Redis Cache
│   │   └── Migrations/
│   │
│   ├── QIMy.Web/                     ⚙️ Presentation Layer (обновлен)
│   │   ├── Components/Pages/
│   │   │   ├── AR/                   ⚙️ Использует MediatR
│   │   │   ├── ER/                   🆕 Новый модуль
│   │   │   ├── KA/                   🆕 Новый модуль
│   │   │   └── Admin/                ⚙️ Использует MediatR
│   │   └── Program.cs                ⚙️ Serilog, Redis, MediatR
│   │
│   ├── QIMy.API/                     ⚙️ REST API (полностью реализован)
│   │   ├── Controllers/              🆕 ClientsController, InvoicesController
│   │   ├── Middleware/               🆕 Exception Handling
│   │   └── Program.cs                🆕 Swagger, Versioning
│   │
│   └── QIMy.Shared/                  🆕 Shared (DTOs for API)
│
├── tests/                            🆕 Tests
│   ├── QIMy.Application.Tests/
│   ├── QIMy.Infrastructure.Tests/
│   └── QIMy.Web.Tests/
│
├── docker-compose.yml                🆕 Docker
├── Dockerfile                        🆕 Docker
└── .github/workflows/ci.yml          🆕 CI/CD
```

---

## 🎯 МЕТРИКИ УСПЕХА

### Код качества:
- ✅ Code Coverage > 80%
- ✅ Все тесты зеленые
- ✅ Нет code smells (SonarQube)
- ✅ Performance < 200ms (95 percentile)

### Архитектура:
- ✅ Clean Architecture соблюдена
- ✅ SOLID принципы применены
- ✅ DRY - нет дублирования кода
- ✅ YAGNI - нет избыточной функциональности

### Документация:
- ✅ XML comments для всех public методов
- ✅ Swagger UI полностью описывает API
- ✅ README.md актуален
- ✅ Architecture Decision Records (ADR) ведутся

---

## 📝 ИТОГ

**Текущая архитектура QIMy** - это **хорошая база**, но требует **значительных улучшений**:

### ❌ Что не так сейчас:
1. Нет Application Layer
2. Нет Repository Pattern
3. Нет CQRS
4. Нет валидации (FluentValidation)
5. Нет DTOs
6. Нет обработки ошибок
7. Нет логирования (Serilog)
8. Нет кэширования
9. Нет тестов
10. Нет API

### ✅ Что будет после улучшений:
1. **Clean Architecture** с 4 слоями
2. **CQRS + MediatR** для разделения команд и запросов
3. **Repository + UnitOfWork** для изоляции БД
4. **FluentValidation** для бизнес-валидации
5. **DTOs + AutoMapper** для разделения Entity и API
6. **Result Pattern** для обработки ошибок
7. **Serilog + Seq** для structured logging
8. **Redis** для кэширования
9. **xUnit + Moq** для тестирования (> 80% coverage)
10. **REST API + Swagger** полностью реализован

**Время реализации**: **12-16 недель** (3-4 месяца)

**Результат**: **Enterprise-grade** архитектура, готовая к **масштабированию** и **долгосрочной поддержке**.

---

**Дата создания**: 21 января 2026
**Автор**: AI Expert Architect
**Версия**: 1.0 - Экспертный план архитектуры QIMy
