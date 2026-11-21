# 11 – Performance & Resilience: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Logging 🔥

**Question:** "How do you do logging in ASP.NET Core? What is structured logging?"

### Short answer

> "I inject `ILogger<T>` and use **message templates** with named placeholders:
> `logger.LogInformation("Order {OrderId} placed", orderId)`. That's
> **structured logging**: `OrderId` is stored as a separate field, so in Seq,
> Elasticsearch or Application Insights I can filter and aggregate by it. String
> interpolation (`$"..."`) loses the structure, and it allocates the string even
> when the level is disabled.
> I configure levels per category and environment in `appsettings.json`, use
> Serilog or OpenTelemetry to ship logs, add a correlation / trace ID so a request
> can be followed across services, and never log secrets or personal data."

### Extra points

- Levels: Trace, Debug, Information, Warning, Error, Critical.
- `logger.BeginScope(...)` adds properties (e.g. OrderId) to all logs inside the scope.
- For hot paths, `[LoggerMessage]` source-generated methods avoid allocations.
- OpenTelemetry = logs + metrics + distributed traces, vendor-neutral.

Comparisons: SLF4J + Logback (Java); Laravel `Log::info('msg', ['order_id' => $id])`.

---

## Caching

**Question:** "How do you cache in ASP.NET Core? In-memory vs distributed?"

### Short answer

> "`IMemoryCache` stores data in the current process: fastest, but each server has
> its own copy and it's lost on restart. With several instances behind a load
> balancer, I use `IDistributedCache` with **Redis**, so all instances share one
> cache. In .NET 9+, `HybridCache` combines both (L1 memory + L2 Redis) and protects
> against **cache stampede**, where many requests rebuild the same expired entry at once.
> The hard part is **invalidation**: I set expirations (absolute and/or sliding) and
> remove or update entries when the underlying data changes."

```csharp
await cache.GetOrCreateAsync($"product:{id}", async e =>
{
    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await db.Products.FindAsync(id);
});
```

### Extra points

- **Output caching** (`AddOutputCache` / `.CacheOutput()`) caches whole HTTP responses on the server.
- Response caching uses HTTP headers (`Cache-Control`) so clients/CDNs cache.
- Don't cache per-user sensitive data under shared keys.

Comparison: Laravel `Cache::remember('key', 300, fn () => ...)`.
