# Fast-Track Study Plan (Nov 14 – Nov 30, 2026)

Goal: interview-ready for a senior .NET role by the end of November.
Assumes about 2–3 hours a day. 🔥 = most asked in interviews.

**Fast mode:** explain → predict the output → 1–2 interview questions → model answer saved
in `INTERVIEW-ANSWERS.md`. Tests only for tricky topics.

| Day | Date | Topics | Done |
|-----|------|--------|------|
| 1 | Nov 14 | C# basics: records, `==` vs `Equals`, strings, nullable reference types | [x] |
| 2 | Nov 15 | Pattern matching, `const`/`readonly`, `ref`/`out`/`in`, generics (no type erasure) | [x] |
| 3 | Nov 16 | 🔥 LINQ: deferred execution, `IEnumerable` vs `IQueryable`, `yield` | [x] |
| 4 | Nov 17 | Memory & GC: generations, `IDisposable`, `using`, finalizers (short) | [x] |
| 5 | Nov 18 | 🔥 Async part 1: `Task` vs `Thread`, how `await` works, `Task.WhenAll` | [x] |
| 6 | Nov 19 | 🔥 Async part 2: deadlocks, `ConfigureAwait`, `CancellationToken`, `lock`, `SemaphoreSlim` | [x] |
| 7 | Nov 20 | OOP in C#: interfaces vs abstract classes, delegates, events, lambdas, SOLID | [x] |
| 8 | Nov 21 | 🔥 ASP.NET Core: request pipeline, middleware, routing, filters, minimal APIs vs controllers | [x] |
| 9 | Nov 22 | 🔥 Dependency injection: lifetimes, captive dependencies, options pattern, configuration | [x] |
| 10 | Nov 23 | 🔥 EF Core part 1: `DbContext`, change tracking, tracking vs no-tracking, migrations | [x] |
| 11 | Nov 24 | 🔥 EF Core part 2: N+1, `Include`, transactions, concurrency, raw SQL | [x] |
| 12 | Nov 25 | Security: authentication vs authorization, JWT, policies, CORS | [x] |
| 13 | Nov 26 | Testing (xUnit, mocking, `WebApplicationFactory`) + logging, caching | [x] |
| 14 | Nov 27 | Architecture: Clean Architecture, CQRS, messaging, resilience (Polly) | [x] |
| 15 | Nov 22 | Weak-areas review #1: modules 01–05 (`interview/weak-areas.md`) | [ ] |
| 16 | Nov 23 | Weak-areas review #2: modules 06–12 | [ ] |
| 17 | Nov 24 | Full mock interview #1 (mixed topics) | [ ] |
| 18 | Nov 25 | Gap topics: `Span<T>`, Dictionary internals, generic variance, `BackgroundService`, health checks, rate limiting, API versioning | [ ] |
| 19 | Nov 26 | System design practice: design an order/checkout API end-to-end | [ ] |
| 20 | Nov 27 | Full mock interview #2 | [ ] |
| 21 | Nov 28 | Live-coding practice: build a small minimal API + EF Core + test, explaining as you go | [ ] |
| 22 | Nov 29 | Final mock interview #3 + revise all `INTERVIEW-ANSWERS.md` | [ ] |
| 23 | Nov 30 | Light revision of remaining weak areas only. Rest. | [ ] |

Already done: value vs reference types, boxing/unboxing (Nov 11–14).
All 14 learning days finished on Nov 21 (6 days early); days 15–23 re-planned to use the extra time.
