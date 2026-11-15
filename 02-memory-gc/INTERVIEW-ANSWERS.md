# 02 – Memory & GC: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## How the garbage collector works

**Question:** "How does garbage collection work in .NET? What are generations?"

### Short answer

> "The GC automatically frees heap objects that are no longer reachable from any
> 'root' (local variables, static fields, CPU registers). It's **generational**,
> based on the idea that most objects die young:
> - **Gen 0**: new objects, collected very often and cheaply.
> - **Gen 1**: a buffer for objects that survived one collection.
> - **Gen 2**: long-lived objects like caches and singletons, collected rarely,
>   because a full collection is expensive.
> Objects that survive a collection are promoted to the next generation. After
> collecting, the GC **compacts** memory, moving objects together to avoid gaps."

### Large Object Heap (LOH)

> "Objects of 85,000 bytes or more go to the LOH, which is collected only with
> Gen 2 and isn't compacted by default. Lots of short-lived big arrays can cause
> fragmentation and expensive Gen 2 collections. `ArrayPool<T>` helps by reusing
> buffers."

### Should you call `GC.Collect()`?

> "Almost never. It forces a full, expensive collection of all generations and
> pauses the app's threads. It can also make things worse: objects that are still
> in use get promoted to Gen 2 early and stay there longer. The GC tunes itself
> based on allocation patterns."

### What the GC does NOT do

> "It only manages **memory**. It doesn't promptly release unmanaged resources
> like file handles, database connections or sockets. For those, we use
> `IDisposable` and `using`."

### Java comparison

Very similar to the JVM's generational GC (Young/Old generation). Java's
humongous objects in G1 are similar to the LOH. `System.gc()` ≈ `GC.Collect()`;
avoid both.

---

## `IDisposable` and `using` 🔥

**Question:** "What's the difference between the GC and `Dispose()`? Why do we need `Dispose()`?"

### Short answer

> "The GC only manages **memory**, and it runs **whenever it decides**
> (non-deterministic). Resources like DB connections, file handles and sockets
> are limited and must be released **immediately**, or we run out of connections
> or leave files locked. `IDisposable.Dispose()` gives **deterministic** cleanup,
> at the moment I choose. `using` guarantees `Dispose()` is called even if an
> exception is thrown, because it compiles to `try / finally`."

### Syntax

```csharp
using (var conn = new SqlConnection(cs)) { ... }   // disposed at the end of the braces
using var conn = new SqlConnection(cs);            // C# 8+: disposed at the end of the enclosing block
await using var stream = ...;                      // for IAsyncDisposable
```

### Order

Several `using`s are disposed in **reverse order** (last opened, first closed),
so dependent resources (a command, then its connection) close safely.
Disposal happens when the block **ends**, after the code inside it.

### Java comparison

`IDisposable` ≈ `AutoCloseable`, `Dispose()` ≈ `close()`,
`using` ≈ try-with-resources.

### Extra points

- Don't dispose objects you didn't create. In ASP.NET Core, the **DI container**
  disposes services it created.
- `HttpClient` is the classic exception: don't create and dispose one per request
  (socket exhaustion). Use `IHttpClientFactory` or a shared instance.

---

## Finalizers

> "A finalizer (`~ClassName()`) is called by the GC before freeing an object, as
> a safety net if `Dispose()` was forgotten. It's non-deterministic, might run
> late or not at all before exit, and it **slows the GC**: finalizable objects
> survive an extra collection and get promoted. In application code I just
> implement `IDisposable`. If a class does have a finalizer, `Dispose()` calls
> `GC.SuppressFinalize(this)` so the GC skips it. For unmanaged handles,
> `SafeHandle` is preferred over writing a finalizer."

Java comparison: like the deprecated `Object.finalize()`, with the same problems.
