# 07 – Dependency Injection: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Service lifetimes 🔥

**Question:** "Explain Singleton, Scoped and Transient lifetimes."

### Short answer

> "- **Singleton**: one instance for the whole application lifetime, shared by all
>   requests and threads, so it must be thread-safe (ideally stateless). E.g. caches,
>   configuration.
> - **Scoped**: one instance per **scope**, which in ASP.NET Core means per HTTP
>   request. Shared by everything within that request, new for each request. The
>   classic example is EF Core's `DbContext`.
> - **Transient**: a new instance **every time** it's resolved. For lightweight,
>   stateless services.
> The container also disposes `IDisposable` services it created, at the end of the
> scope (scoped/transient) or at app shutdown (singleton)."

| Lifetime | Same request, 2 injections | 2 different requests |
|---|---|---|
| Singleton | same | same |
| Scoped | same | different |
| Transient | different | different |

### Comparisons

| .NET | Spring | Laravel |
|---|---|---|
| `AddSingleton` | default bean (singleton) | `$app->singleton()` |
| `AddScoped` | `@RequestScope` | `$app->scoped()` |
| `AddTransient` | `@Scope("prototype")` | `$app->bind()` |

Note: Spring beans are singleton **by default**; .NET has no default, so you always choose.

### Mistakes to avoid

- ❌ "A singleton is thread-safe." No: because it's shared by all requests, it
  **must be made** thread-safe.
- ❌ Making `DbContext` a singleton. It isn't thread-safe and would grow its change tracker forever.

---

## Captive dependency 🔥

**Question:** "What is a captive dependency? How do you fix it?"

### Short answer

> "It's when a longer-lived service depends on a shorter-lived one, typically a
> **singleton depending on a scoped service** like `DbContext`. The singleton is
> created once and holds that scoped instance forever, so it's 'captive': shared
> across all requests and threads, never disposed. For `DbContext`, that means
> concurrency exceptions, stale data and memory growth.
> Rule: a service must not depend on a service with a **shorter** lifetime.
> ASP.NET Core's **scope validation** catches this at startup in Development.
> Fix: change the lifetimes, or inject `IServiceScopeFactory` into the singleton
> and create a scope each time a scoped service is needed. This is the standard
> pattern in `BackgroundService`."

```csharp
using var scope = _scopeFactory.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
```

| OK | Captive |
|---|---|
| Scoped → Singleton | Singleton → Scoped |
| Transient → Singleton / Scoped | Singleton → Transient (transient held forever) |

Validation can be enabled in every environment with
`builder.Host.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; })`.

### Precise version of the rule

Strictly, "don't depend on a shorter lifetime" also covers **Scoped → Transient**:
the transient is kept for the whole request. But that's **harmless**, because it's
still disposed at the end of the request and never shared between requests.
The combinations that cause real problems all involve a **singleton**:

- **Singleton → Scoped**: ❌ always a bug (caught by scope validation).
- **Singleton → Transient**: ⚠️ a bug if the transient is stateful or not thread-safe (not caught automatically).
- **Scoped → Transient**: ✅ fine in practice.

---

## Configuration and the options pattern

**Question:** "How does configuration work in ASP.NET Core? What's the options pattern?"

### Short answer

> "Configuration is layered from several providers, and **later ones override
> earlier ones**: `appsettings.json` → `appsettings.{Environment}.json` → user secrets
> (Development) → environment variables → command-line args. So an environment
> variable overrides `appsettings.json`, which is how production values are set
> without changing files.
> With the **options pattern**, I bind a config section to a typed class with
> `services.Configure<EmailOptions>(config.GetSection("Email"))` and inject
> `IOptions<EmailOptions>`, instead of reading strings like `config["Email:Port"]`."

### The 3 interfaces

| | Reads | Lifetime | Use |
|---|---|---|---|
| `IOptions<T>` | once at startup | singleton | settings don't change at runtime |
| `IOptionsSnapshot<T>` | once per request | scoped | reload per request, in scoped services |
| `IOptionsMonitor<T>` | always current + `OnChange` | singleton | singletons needing live updates |

`IOptionsSnapshot` is scoped, so a singleton can't use it (captive dependency); use `IOptionsMonitor`.

### Secrets

- Local development: **user secrets** (`dotnet user-secrets set "Db:Password" "..."`),
  stored outside the project folder, so they're never committed.
- Production: environment variables or a vault (Azure Key Vault, AWS Secrets Manager).
- Never commit secrets in `appsettings.json`.
- Environment variable names use `__` for nesting: `Email__SmtpHost`.

### Extra points

Validate options at startup: `services.AddOptions<EmailOptions>().Bind(...).ValidateDataAnnotations().ValidateOnStart()`.

### Comparisons

Laravel `.env` + `config/*.php`; Spring `application.yml` + profiles + `@ConfigurationProperties`.
