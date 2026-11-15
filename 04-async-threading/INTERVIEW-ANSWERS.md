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
