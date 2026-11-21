# 12 – Architecture & System Design: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Clean Architecture 🔥

**Question:** "Explain Clean Architecture. When would you use it?"

### Short answer

> "Clean Architecture organises code in layers with one rule: **dependencies point
> inward**, toward the business logic.
> - **Domain**: entities and business rules, with no framework references.
> - **Application**: use cases, plus the **interfaces** they need (`IOrderRepository`, `IEmailSender`).
> - **Infrastructure**: implementations of those interfaces: EF Core, email, payment providers.
> - **API / Presentation**: controllers or endpoints and the composition root (DI setup).
> Infrastructure implements interfaces defined in Application, which is dependency
> inversion, so business logic doesn't depend on the database or frameworks. That
> makes it testable and lets me swap infrastructure.
> The trade-off is more projects, more mapping and more indirection. For simple
> CRUD services I'd use something lighter, like **vertical slices** organised by
> feature, and choose based on domain complexity and team size."

| Item | Layer |
|---|---|
| `Order` entity, "can't cancel after shipping" | Domain |
| `IEmailSender`, `PlaceOrderHandler`, DTOs | Application |
| `AppDbContext`, `SmtpEmailSender` | Infrastructure |
| `OrdersController`, `Program.cs` | API |

### Comparisons

Spring's controller/service/repository layering is similar but less strict.
Laravel Eloquent models mix persistence and domain (Active Record).

### Related terms

Onion / Hexagonal (ports & adapters) architecture are the same core idea.
DDD (domain-driven design) often lives in the Domain layer: aggregates, value objects, domain events.

---

## CQRS (+ MediatR)

**Question:** "What is CQRS? When would you use it?"

### Short answer

> "CQRS (Command Query Responsibility Segregation) separates **commands**, which
> change state and return little or nothing, from **queries**, which return data
> and change nothing. Each gets its own model and handler: commands load tracked
> entities, apply business rules and call `SaveChanges`; queries use `AsNoTracking`
> and project straight into DTOs. Handlers stay small and focused, and cross-cutting
> concerns (validation, logging, transactions) can be added once as pipeline
> behaviours, e.g. with MediatR.
> CQRS doesn't require separate databases or event sourcing; those are optional
> extras for when reads and writes need to scale differently.
> Trade-off: more classes and indirection, so it's overkill for simple CRUD."

### Naming tip

- **Command** = a verb that changes something: `PlaceOrder`, `UpdateAddress`, `CancelOrder`.
- **Query** = `Get...` / `List...` / `Search...`: reads only.

### Notes

- MediatR became commercially licensed in 2025; alternatives: hand-written handler
  interfaces, Wolverine, or source-generated mediators. CQRS is a pattern, not a library.
- Comparison: Laravel Jobs/Actions; Spring has no direct equivalent (Command pattern).

---

## Messaging, the Outbox pattern, idempotency 🔥

**Question:** "Why use a message broker? How do you keep the database and messages consistent?"

### Short answer

> "A broker (RabbitMQ, Azure Service Bus, SQS, Kafka) decouples services: the API
> saves its data, publishes an event like `OrderPlaced` and returns quickly;
> consumers handle email, stock and analytics asynchronously. If a consumer is down,
> messages wait in the queue, so the system is more resilient and each part scales
> independently. In .NET I'd use **MassTransit** (or NServiceBus) as the abstraction.
> The risk is the **dual-write problem**: saving to the database and publishing are
> two separate operations, so a crash between them leaves them inconsistent. The
> **Outbox pattern** fixes it: write the message to an `Outbox` table in the **same
> transaction** as the business data, and a background process publishes it with retries.
> That gives **at-least-once** delivery, so a message can arrive **more than once**.
> Consumers must therefore be **idempotent**: processing the same message twice has
> the same effect as once, e.g. by storing processed message IDs (an Inbox) or using
> unique constraints."

### Key idea

Idempotency does **not** prevent duplicates. It makes duplicates **harmless**.

### Extra points

- Use queues for commands (one consumer) and topics / pub-sub for events (many subscribers).
- Dead-letter queues hold messages that keep failing, for inspection.
- Eventual consistency: other services update a bit later; design the UI for it.
- MassTransit has a built-in EF Core outbox.

### Comparisons

Laravel Queues + Jobs (`dispatch(...)`); Spring AMQP / Spring Kafka.
