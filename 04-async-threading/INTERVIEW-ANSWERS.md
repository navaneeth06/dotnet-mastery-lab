# 04 – Async & Threading: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Why async/await?

**Question:** "Why use async/await? Does it make code faster?"

### Short answer

> "Async is about **not blocking threads while waiting for I/O**, like a database
> call, HTTP call or file read. While the operation is in progress, no thread is
> waiting for it; the thread goes back to the pool and serves other requests.
> It doesn't make a single request faster. It improves **scalability**: the
> server handles many more concurrent requests with the same number of threads.
> For CPU-heavy work, async alone doesn't help."

### Comparisons

- JavaScript: `Task` ≈ `Promise`, same `async/await` keywords.
- Java: `CompletableFuture`; Java 21 virtual threads solve the same problem differently
  (cheap blocking threads instead of async methods).
- PHP: normally synchronous, one blocking process per request.

---

## `Task` vs `Thread`

> "A `Thread` is an OS worker: expensive (about 1 MB of stack) and limited.
> A `Task` is an object that represents work completing in the future, like a
> promise. CPU work in a task runs on a **thread-pool** thread, but for I/O
> there's often **no thread at all** while waiting; the OS notifies when it's done.
> In modern code I almost never create threads directly; I use `Task`, `async/await`,
> and `Task.Run` for CPU-bound work."

---

## How an async method runs (execution order)

> "An async method runs **synchronously** on the caller's thread until the first
> `await` of an operation that isn't complete yet. Then it returns an incomplete
> `Task` to the caller, and the caller continues. When the awaited operation
> finishes, the rest of the method resumes. `await task` in the caller pauses the
> caller until that task completes."

```csharp
Console.WriteLine("A");
var task = DoWorkAsync();   // runs X immediately, then returns at the await
Console.WriteLine("B");
await task;                 // waits until Y has printed
Console.WriteLine("C");
// Output: A X B Y C

async Task DoWorkAsync()
{
    Console.WriteLine("X");
    await Task.Delay(100);
    Console.WriteLine("Y");
}
```

Different from `yield return`: an iterator runs **nothing** until enumerated;
an async method **starts immediately** when called ("hot" tasks).
Under the hood, the compiler turns the async method into a **state machine**.

---

## `Task.WhenAll` / `Task.WhenAny`

**Question:** "How do you run several async operations concurrently?"

### Short answer

> "Start the tasks first, without awaiting them, then await `Task.WhenAll`.
> Because async methods start as soon as they're called, all the operations run
> at the same time, so the total time is roughly the **slowest** one, not the sum.
> Awaiting each one in turn would run them sequentially."

```csharp
var userTask   = GetUserAsync();
var ordersTask = GetOrdersAsync();
await Task.WhenAll(userTask, ordersTask);     // ~max(times), not sum
var user = userTask.Result;                    // safe here: already completed (or `await userTask`)
```

### Extra points

- `Task.WhenAny` completes when the **first** task finishes (timeouts, racing).
- If tasks fail, `await Task.WhenAll(...)` throws the **first** exception;
  all exceptions are in the returned task's `.Exception.InnerExceptions`.
- ⚠️ Don't run concurrent queries on the **same EF Core `DbContext`**; it isn't
  thread-safe and throws. Use separate contexts or run them sequentially.
- To limit concurrency (e.g. 1,000 URLs), use `SemaphoreSlim` or `Parallel.ForEachAsync`.

### Comparisons

JS `Promise.all` / `Promise.race`; Java `CompletableFuture.allOf` / `anyOf`.

---

## `.Result` / `.Wait()` and deadlocks 🔥

**Question:** "Why is calling `.Result` or `.Wait()` on a task dangerous?"

### Short answer

> "It's 'sync over async': it **blocks** the current thread until the task completes,
> which throws away the benefit of async.
> In apps with a **SynchronizationContext**, like WPF, WinForms or classic ASP.NET,
> it can **deadlock**: the calling thread blocks waiting for the task, but the
> task's continuation after `await` needs to resume on that same thread. Each
> waits for the other forever, and the UI freezes.
> **ASP.NET Core has no SynchronizationContext**, so that classic deadlock doesn't
> happen, but blocking still wastes thread-pool threads and causes **thread-pool
> starvation** under load. The fix is **async all the way**: `await` instead of
> blocking, up to the controller."

### Also

- `.Result` wraps exceptions in `AggregateException`; `await` throws the original exception.
- `.GetAwaiter().GetResult()` unwraps the exception, but it **still blocks**. It's not a fix.
- If you truly must call async from sync code (e.g. `Main` in old code, constructors),
  it's a design smell. Prefer async `Main`, factory methods, etc.

---

## `ConfigureAwait(false)`

> "It tells `await` not to resume on the captured SynchronizationContext, so the
> continuation can run on any thread-pool thread. This avoids the deadlock above
> and is slightly faster.
> - **Library code**: use it, because you don't know if the caller has a SynchronizationContext.
> - **ASP.NET Core app code**: not needed, since there's no context.
> - **UI code** that updates controls after the await: don't use it, because you need the UI thread."

---

## `CancellationToken` 🔥

**Question:** "How do you support cancellation in async code?"

### Short answer

> "With a `CancellationToken`: a signal passed down the call chain. In ASP.NET Core,
> I add a `CancellationToken` parameter to the action; the framework binds it to
> `HttpContext.RequestAborted`, which is cancelled when the **client disconnects**.
> I pass it to every async call that accepts one: EF Core, `HttpClient`, my own
> services. Cancellation is **cooperative**: code must pass the token or check
> it (`ct.ThrowIfCancellationRequested()`); nothing is stopped forcibly. On cancel,
> an `OperationCanceledException` is thrown (`TaskCanceledException` derives from it)."

### Extra points

- Create your own: `CancellationTokenSource`, and `new CancellationTokenSource(TimeSpan.FromSeconds(5))` for timeouts.
- Combine tokens: `CancellationTokenSource.CreateLinkedTokenSource(ct1, ct2)`.
- Don't log a cancelled request as an error; it's normal when users leave.
- For background services, `BackgroundService.ExecuteAsync(stoppingToken)` is cancelled on app shutdown.

### Comparisons

JS `AbortController` + `fetch(url, { signal })` is the closest match.
Java: `Future.cancel()` / thread interruption.

---

## `lock` vs `SemaphoreSlim`

**Question:** "How do you protect shared state in async code?"

### Short answer

> "`lock` lets one thread at a time into a block of code, like Java's `synchronized`.
> But you **can't `await` inside a `lock`**, because a lock is owned by a thread,
> and after `await` the code may resume on a different thread. For async code, I
> use `SemaphoreSlim(1)` with `await WaitAsync()` and `Release()` in a `finally`.
> `SemaphoreSlim(n)` also **limits concurrency**, e.g. max 5 calls to an external API."

```csharp
await _gate.WaitAsync(ct);
try { await SaveAsync(); }
finally { _gate.Release(); }
```

### Extra points

- For a simple counter, `Interlocked.Increment(ref _count)` is lock-free and faster.
- .NET 9+ has a dedicated `System.Threading.Lock` type: `lock` on it is faster than `lock` on an `object`.
- Keep locked sections **short**. Never call slow I/O while holding a `lock`.
- Prefer avoiding shared mutable state: `ConcurrentDictionary`, immutable data, `Channel<T>`.
- These only work **within one process**. With multiple servers, use a **distributed lock**
  (Redis, database row locks) or optimistic concurrency.
