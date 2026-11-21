# 08 – EF Core: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## `DbContext` and Unit of Work 🔥

**Question:** "What is a `DbContext`? What lifetime should it have?"

### Short answer

> "`DbContext` is a session with the database. It exposes `DbSet<T>` tables you
> query with LINQ (as `IQueryable`, so it's translated to SQL), tracks changes to
> loaded entities, and saves them with `SaveChangesAsync()`. It implements the
> **Unit of Work** pattern: all pending inserts, updates and deletes are saved
> together in **one transaction**.
> It's registered as **scoped** (one per request) because it's **not thread-safe**
> and is meant to be short-lived. A singleton `DbContext` would be shared across
> requests and its change tracker would grow forever."

### Comparisons

- Hibernate/JPA: `DbContext` ≈ `EntityManager`/`Session`. EF Core is much closer to Hibernate.
- Eloquent is **Active Record** (`$user->save()` saves one model now). EF Core is
  **Unit of Work / Data Mapper**: change many objects, then one `SaveChanges`.

---

## Change tracking and `AsNoTracking` 🔥

**Question:** "How does change tracking work? When do you use `AsNoTracking`?"

### Short answer

> "When EF loads an entity, the `DbContext` tracks it and stores a snapshot of its
> original values. On `SaveChanges`, it compares current values with the snapshot
> (**dirty checking**) and generates `UPDATE`s only for changed columns. No explicit
> `Update()` call is needed for tracked entities.
> For **read-only** queries, I use `AsNoTracking()`: no snapshots, so it's faster
> and uses less memory, but changes to those objects are **not** saved."

```csharp
var u = await db.Users.FirstAsync(x => x.Id == 1);
u.Name = "Bob";
await db.SaveChangesAsync();         // UPDATE sent

var r = await db.Users.AsNoTracking().FirstAsync(x => x.Id == 1);
r.Name = "Cara";
await db.SaveChangesAsync();         // nothing sent, r isn't tracked
```

### Entity states

`Added`, `Unchanged`, `Modified`, `Deleted`, `Detached`. You can see them with `db.Entry(entity).State`.

### Extra points

- `db.Users.Update(entity)` attaches a **detached** entity (e.g. from an API body) and marks
  **all** columns as modified.
- `ExecuteUpdateAsync` / `ExecuteDeleteAsync` (EF Core 7+) run set-based SQL directly,
  with no loading or tracking. Great for bulk changes.
- A global default is possible: `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)`.

---

## Migrations

**Question:** "How do you manage database schema changes with EF Core? How do you deploy them?"

### Short answer

> "With code-first migrations. I change the entity classes, run
> `dotnet ef migrations add <Name>`, and EF generates a migration by comparing the
> model with the last snapshot. It has `Up()` and `Down()` methods. Applied
> migrations are recorded in `__EFMigrationsHistory`.
> I always **review** the generated code: a property rename can be generated as a
> drop + add, which loses data, so I change it to `RenameColumn`.
> For production, I don't run `database update` from a laptop. I generate an
> **idempotent SQL script** (`dotnet ef migrations script --idempotent`) or a
> migration bundle, and apply it through the CI/CD pipeline, where it's reviewed.
> I avoid `Database.Migrate()` at startup when several instances run at once."

### Commands

```bash
dotnet ef migrations add AddUserPhone
dotnet ef database update
dotnet ef database update PreviousMigration     # roll back
dotnet ef migrations remove                     # remove the last, unapplied migration
dotnet ef migrations script --idempotent        # SQL for production
dotnet ef migrations bundle                     # self-contained executable
```

### Extra points

- For zero-downtime deploys, make schema changes **backward compatible** (add the
  column → deploy code → backfill → remove the old column later: expand/contract).

### Comparisons

Laravel: `make:migration` (hand-written) + `migrate`. Spring: Flyway / Liquibase.

---

## The N+1 problem 🔥🔥

**Question:** "What is the N+1 problem and how do you avoid it in EF Core?"

### Short answer

> "N+1 is when you run 1 query to load a list, then 1 more query per item to load
> related data: 100 orders means 101 database round-trips. In EF Core it happens
> when you query inside a loop, or when lazy loading is enabled.
> Fixes:
> 1. **Eager loading** with `Include` / `ThenInclude`, which loads related data in the same query (a JOIN).
> 2. **Projection** with `Select` into a DTO, which fetches only the needed columns in one query.
>    Usually the best choice for read endpoints.
> I also keep lazy loading **off** (the EF Core default) and check the generated
> SQL in the logs."

```csharp
db.Orders.Include(o => o.Customer).ToListAsync();                          // 1 query, JOIN
db.Orders.Select(o => new { o.Id, CustomerName = o.Customer.Name }).ToListAsync();  // 1 query, few columns
```

### EF Core vs Laravel / Hibernate

Lazy loading is **off by default** in EF Core. Without `Include`, a navigation
property is just **`null`** (or an empty collection), so you get a
`NullReferenceException`, not a hidden query. Laravel and Hibernate lazy-load by
default, which is where hidden N+1 queries come from.

### Extra points

- `Include` of several collections can create a huge JOIN ("cartesian explosion").
  Use `.AsSplitQuery()` to load them in separate queries.
- Watch for N+1 in loops calling a repository method per item; batch with
  `Where(x => ids.Contains(x.Id))`.
- See the SQL with `optionsBuilder.LogTo(Console.WriteLine)` or the logs at `Information` level.

---

## Transactions

**Question:** "How do transactions work in EF Core?"

### Short answer

> "Each `SaveChangesAsync()` call runs in its own transaction automatically: all
> changes succeed or none do. Often one `SaveChanges` is enough, because EF handles
> generated keys and related entities in the right order. When several
> `SaveChanges` calls (or raw SQL) must be atomic, I use
> `await using var tx = await db.Database.BeginTransactionAsync();` and
> `await tx.CommitAsync()`. If it isn't committed, disposing rolls it back."

Comparisons: Laravel `DB::transaction(fn ...)`; Spring `@Transactional`. .NET has no
built-in `@Transactional` attribute. Some teams use a unit-of-work behaviour or filter for it.

With retries enabled (`EnableRetryOnFailure`), wrap manual transactions in
`db.Database.CreateExecutionStrategy().ExecuteAsync(...)`.

---

## Optimistic concurrency 🔥

**Question:** "Two users edit the same record at the same time. How do you prevent lost updates?"

### Short answer

> "With **optimistic concurrency**: add a concurrency token, like a SQL Server
> `rowversion` with `[Timestamp]`, or `IsConcurrencyToken()` on a column. EF adds
> it to the `WHERE` clause of the UPDATE. If another user changed the row first,
> the version doesn't match, 0 rows are affected, and EF throws
> `DbUpdateConcurrencyException`. I catch it and either reload and retry, merge,
> or return **409 Conflict** so the user can refresh.
> It's optimistic because nothing is locked. It suits web apps where conflicts are
> rare. Pessimistic locking (`SELECT ... FOR UPDATE` / `UPDLOCK`) needs raw SQL in EF Core."

```csharp
[Timestamp] public byte[] RowVersion { get; set; } = null!;
// UPDATE Products SET Stock = @p0 WHERE Id = @id AND RowVersion = @originalVersion
```

Comparisons: Hibernate/JPA `@Version`. Laravel has nothing built in.

For simple counters, an atomic update avoids the conflict entirely:
`db.Products.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - 3))`.

---

## Raw SQL and SQL injection

**Question:** "How do you run raw SQL in EF Core safely?"

### Short answer

> "`FromSql($"...{value}")` for queries returning entities, and
> `ExecuteSqlAsync($"...")` for commands. With an interpolated string, EF turns each
> `{value}` into a **DbParameter**, so it's safe from SQL injection. The `...Raw`
> versions take a plain string: if you build it with concatenation, or with `$"..."`
> passed to `FromSqlRaw`, the values are pasted into the SQL, which is **SQL injection**.
> Use `...Raw` only with `{0}` placeholders plus parameters, e.g. for dynamic table names.
> `FromSql` returns an `IQueryable`, so I can compose LINQ on top (`Where`,
> `AsNoTracking`). `SqlQuery<T>` maps to non-entity types (EF Core 8+)."

```csharp
db.Users.FromSql($"SELECT * FROM Users WHERE Email = {email}")       // ✅ parameterised
db.Users.FromSqlRaw($"SELECT * FROM Users WHERE Email = '{email}'")  // ❌ injection
```

Comparison: Laravel `DB::select('... where email = ?', [$email])` vs string interpolation.
