# 06 – ASP.NET Core Internals: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Request pipeline and middleware 🔥

**Question:** "What is middleware? How does the request pipeline work?"

### Short answer

> "The request pipeline is a chain of middleware components registered in
> `Program.cs`. Each one receives the `HttpContext`, can do work **before** calling
> `next()`, can **short-circuit** by not calling `next()` (e.g. returning 401 or
> serving a static file), and can do work **after** `next()` returns, on the way
> back. So requests flow in in registration order, and responses flow out in
> reverse order, like Russian dolls. **Order matters**: the exception handler goes
> first so it wraps everything, and authentication comes before authorization."

```csharp
app.Use(async (ctx, next) => { /* before */ await next(); /* after */ });
// A in → B in → endpoint → B out → A out
```

### Typical order

`UseExceptionHandler` → `UseHttpsRedirection` → `UseStaticFiles` → `UseRouting` →
`UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers` / endpoints

### Extra points

- `app.Use` = can call next; `app.Run` = terminal (ends the pipeline).
- Custom middleware is usually a class with `InvokeAsync(HttpContext ctx)` and
  `RequestDelegate next` in the constructor.
- Middleware is created **once** (singleton-like), so inject scoped services as
  `InvokeAsync` parameters, not in the constructor.
- Don't change headers after the response has started (after `next()` wrote the body).

### Comparisons

Laravel middleware (`$next($request)`) and Spring servlet `Filter`
(`chain.doFilter`) follow the same pattern.

---

## Controllers vs minimal APIs 🔥

**Question:** "When would you use minimal APIs vs controllers?"

### Short answer

> "Both run on the same pipeline, routing, model binding and DI. Minimal APIs
> map endpoints with lambdas (`app.MapGet(...)`): less ceremony and slightly faster,
> great for microservices and small APIs. Controllers group actions in classes
> with attributes and have a richer filter model, which suits large apps and
> teams used to MVC. For bigger minimal-API apps, I organise endpoints with
> `MapGroup` and extension methods so `Program.cs` stays small."

### Routing and model binding

- Route constraints: `{id:int}` → `/users/abc` doesn't match the route → **404**
  (the action never runs).
- Binding sources: route (`{id}`), query string, body (complex types / JSON),
  headers, DI services. Attributes: `[FromRoute]`, `[FromQuery]`, `[FromBody]`,
  `[FromServices]`.
- `[ApiController]` adds automatic 400 responses for invalid models and binding inference.

### Comparisons

- Controllers ≈ Spring `@RestController`: `[HttpGet("{id}")]` ≈ `@GetMapping("/{id}")`, `[Route]` ≈ `@RequestMapping`.
- Minimal APIs ≈ Laravel closure routes: `Route::get('/users/{id}', fn ($id) => ...)`.

---

## Filters vs middleware 🔥

**Question:** "What's the difference between middleware and filters? When would you use each?"

### Short answer

> "Middleware runs for **every request** in the pipeline and only knows about the
> `HttpContext`. Filters run **inside the MVC/endpoint layer**, around controller
> actions, so they know which controller and action is running and can see the
> bound model and action arguments.
> I use middleware for cross-cutting HTTP concerns: request logging, CORS,
> authentication, security headers, global exception handling. I use filters for
> action-specific logic like validation or checks that need the model."

### Filter order

Authorization → Resource → (model binding) → Action → **action runs** → Exception → Result

### Extra points

- Register globally (`options.Filters.Add<...>()`), per controller, or per action
  (`[ServiceFilter(typeof(X))]` to resolve it from DI).
- Minimal APIs use **endpoint filters** (`.AddEndpointFilter<T>()`).
- Global error handling: in modern .NET, prefer `UseExceptionHandler` + `IExceptionHandler`
  (middleware) returning `ProblemDetails`, over exception filters.

### Comparisons

Spring: middleware ≈ servlet `Filter`; MVC filters ≈ `HandlerInterceptor` / `@ControllerAdvice`.
