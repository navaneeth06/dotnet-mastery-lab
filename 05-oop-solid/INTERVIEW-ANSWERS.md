# 05 – OOP & SOLID in C#: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Properties

> "Properties are C#'s built-in getters and setters. `public string Name { get; set; }`
> is an auto-property: the compiler creates a private backing field. You can use
> `private set`, `init` (settable only during object creation) or expression-bodied
> read-only properties (`=> ...`). Public data should be exposed as properties, not
> fields, so you can add validation later without breaking callers."

Java: replaces hand-written `getX()` / `setX()` (or Lombok).

---

## Interface vs abstract class 🔥

**Question:** "When would you use an interface vs an abstract class?"

### Short answer

> "An interface is a **contract**: what a type can do. A class can implement many.
> An abstract class is a **partial implementation**: it can hold state, constructors
> and shared code, but a class can inherit only **one**.
> I default to **interfaces**, especially for dependency injection and unit testing,
> because they're easy to mock and swap. I use an abstract class when several
> related classes share real code or state (the template method pattern), and in
> general I prefer **composition over inheritance**."

| | Interface | Abstract class |
|---|---|---|
| How many per class | many | one |
| Fields / constructor | no | yes |
| Code | default methods (C# 8+) | yes |
| Meaning | "can do" | "is a" |

### Java comparison

Almost identical. C# uses `:` for both `extends` and `implements`, names
interfaces with an `I` prefix, and has default interface methods like Java 8.
C# 11 also allows `static abstract` members in interfaces (used for generic math).

---

## Delegates, lambdas and closures

**Question:** "What is a delegate? What's a closure, and what's the loop-capture trap?"

### Short answer

> "A delegate is a type-safe reference to a method, so methods can be passed around
> as values. In practice I use the built-in `Func<...>` (returns a value) and
> `Action<...>` (returns void). Lambdas like `x => x * 2` are a short way to create
> them; LINQ is built on them.
> A **closure** is when a lambda uses a variable from outside. It captures the
> **variable itself, not its value**, so it sees later changes. The compiler
> moves the variable into a hidden class (on the heap) so it can outlive the method."

### The `for` loop trap

```csharp
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.WriteLine(i));
// prints 3, 3, 3: one shared `i`, which is 3 when the loop ends
```

Fix: copy to a local inside the loop (`var copy = i;` and capture `copy`).
`foreach` doesn't have this problem since C# 5: each iteration gets a new variable
(so it prints 0, 1, 2).

### Comparisons

- Java: `Func` ≈ `Function<T,R>`, `Action` ≈ `Consumer<T>` / `Runnable`. Java requires captured
  locals to be *effectively final*, so this trap can't happen there.
- JS: like `var` in a `for` loop (shared → 3,3,3) vs `let` (per iteration → 0,1,2).

---

## Events

**Question:** "What's the difference between an event and a delegate? What problems can events cause?"

### Short answer

> "An event is a delegate with restricted access, used for publish/subscribe.
> Outside the class you can only **subscribe (`+=`) and unsubscribe (`-=`)**;
> only the owning class can invoke it or reset it. With a plain public delegate
> field, any caller could overwrite all subscribers with `=` or fire the event
> from outside. Subscribers run in the order they subscribed."

### The trap: memory leaks

> "The publisher holds a reference to every subscriber. If a short-lived object
> subscribes to a long-lived one (a singleton or static event) and never
> unsubscribes, the publisher keeps it **reachable**, so the GC can't collect it.
> That's a memory leak. Always `-=` when done, typically in `Dispose()`."

### Extra points

- Standard signature: `event EventHandler<TEventArgs>` with `(object sender, TEventArgs e)`.
- Fire safely with `Clicked?.Invoke()` (null when there are no subscribers).
- An exception in one handler stops the remaining handlers from running.
- For app-wide events across services, use messaging (MediatR notifications, a message bus), not C# events.

### Comparisons

JS `addEventListener`, Java listeners / Observer pattern, Laravel events & listeners.

---

## SOLID

**Question:** "Explain SOLID with C# examples."

| | Principle | Meaning | C# example |
|---|---|---|---|
| S | Single Responsibility | One reason to change | `OrderService` doesn't send emails; `IEmailSender` does |
| O | Open/Closed | Extend without modifying | New `IPaymentGateway` implementation instead of another `if/else` (strategy pattern) |
| L | Liskov Substitution | Subtypes must work wherever the base type works | Don't implement `IList` if `Add()` throws |
| I | Interface Segregation | Small, focused interfaces | `IReader` + `IWriter`, not one 30-method interface |
| D | Dependency Inversion | Depend on abstractions | Constructor takes `IEmailSender`, not `new SmtpEmailSender()` |

### Short answer (focus on D)

> "In ASP.NET Core, dependency inversion is built in through DI: classes receive
> interfaces in their constructors, and the container decides the implementation.
> That makes code testable (inject fakes or mocks) and lets me swap
> implementations without touching the consumer. Breaking D usually shows up as
> `new SomeConcreteService()` inside business logic."

### Senior nuance

> "SOLID is a guide, not a rule. Too many tiny interfaces and abstractions
> 'just in case' make code harder to follow. I add an abstraction when there's a
> real need: testing, multiple implementations, or a boundary like I/O."
