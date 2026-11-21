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
