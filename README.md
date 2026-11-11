# dotnet-mastery-lab

A hands-on lab for learning C# and .NET core concepts to senior (5-year) interview level.
Every concept is proven with small experiments and xUnit tests, explained in my own words,
and practised through mock interviews.

## How each module works

1. **Read** – understand the concept (with an analogy).
2. **Predict** – guess the output of a snippet before running it.
3. **Experiment** – write code in `src/` and a test in `tests/` that proves the behavior.
4. **Explain** – write `NOTES.md` in my own words.
5. **Interview** – answer the questions in `INTERVIEW.md`, then do a mock interview.
6. **Revisit** – log weak spots in `interview/weak-areas.md`.

## Roadmap

- [ ] **01 – C# fundamentals**: value vs reference types, structs, records, boxing, `ref`/`in`/`out`, nullable reference types, pattern matching
- [ ] **02 – Memory & GC**: stack vs heap, generations, LOH, `IDisposable`, `using`, finalizers, `Span<T>`
- [ ] **03 – Collections & LINQ**: deferred execution, `IEnumerable` vs `IQueryable`, `yield`, Dictionary internals
- [ ] **04 – Async & threading**: `Task` vs `Thread`, async state machine, `ConfigureAwait`, deadlocks, `CancellationToken`, locks, `SemaphoreSlim`, Channels
- [ ] **05 – OOP & SOLID in C#**: interfaces vs abstract classes, generics & variance, delegates, events
- [ ] **06 – ASP.NET Core internals**: request pipeline, middleware, routing, filters, model binding, minimal APIs vs controllers
- [ ] **07 – Dependency injection**: lifetimes, captive dependencies, options pattern
- [ ] **08 – EF Core**: change tracking, N+1, tracking vs no-tracking, migrations, transactions, concurrency tokens, raw SQL
- [ ] **09 – Security**: authentication vs authorization, JWT, policies, CORS, data protection
- [ ] **10 – Testing**: xUnit, mocking, `WebApplicationFactory`, Testcontainers
- [ ] **11 – Performance & resilience**: caching, BenchmarkDotNet, Polly, logging, OpenTelemetry
- [ ] **12 – Architecture & system design**: Clean Architecture, CQRS, messaging, microservices trade-offs

## Repo structure

```
dotnet-mastery-lab/
├── CLAUDE.md                 # tutor rules for Claude
├── README.md                 # this roadmap
├── Directory.Build.props     # shared project settings
├── DotnetLab.slnx
├── 01-csharp-fundamentals/
│   ├── src/
│   ├── tests/
│   ├── NOTES.md
│   └── INTERVIEW.md
└── interview/
    ├── mock-sessions.md
    └── weak-areas.md
```

## Adding a new module

```bash
dotnet new console -n <Name> -o <NN-module-name>/src
dotnet new xunit   -n <Name>.Tests -o <NN-module-name>/tests
dotnet sln add <NN-module-name>/src
dotnet sln add <NN-module-name>/tests
dotnet add <NN-module-name>/tests reference <NN-module-name>/src
```

## Run

```bash
dotnet build
dotnet test
```

## Progress log

| Date | Module | What I learned | Weak spots |
|------|--------|----------------|------------|
|      |        |                |            |