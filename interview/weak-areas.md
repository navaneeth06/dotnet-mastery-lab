# Weak Areas

Topics to revisit. Remove a row once I can answer it confidently without hints.

| Date | Module | Topic | What I got wrong | Correct idea |
|------|--------|-------|------------------|--------------|
| 2026-11-13 | 01 | Where a reference variable is stored | Said both `p` and the `Person` object are on the heap | `p` (a local) is on the stack and holds the address; the object is on the heap |
| 2026-11-13 | 01 | `var` | Thought `var` decides the type at runtime | `var` is decided at **compile time**; `var p = new Person()` is the same as `Person p = ...` |
| 2026-11-13 | 01 | "Pass by reference" wording | Said a class object is "passed by reference" | By default C# passes **everything by value**; for a class, the value copied is the reference ("the reference is passed by value"). Real pass-by-reference needs `ref` |
| 2026-11-13 | 01 | `ref` vs `out` vs `in` | Said all three let a method modify/replace the caller's variable | Only `ref` is "read and replace". `out` = method must assign a value (used to return extra results). `in` = passed by reference but **read-only**. Studied in the ref/out/in topic |
| 2026-11-14 | 01 | Record immutability / `with` | Thought `with` deep-copies because "records are immutable" | `with` is a **shallow** copy; immutability is top-level only, so a `List` property is shared and can still be changed |
| 2026-11-14 | 01 | `==` on records | Predicted `False` for two records with the same data | Records override `==` to compare **values** → `True` (unlike Java records) |
| 2026-11-14 | 01 | Nullable reference types | Said they "prevent crashes at runtime" | **Compile-time warnings only**; no runtime check. Null can still come from JSON/DB/libraries, so validate at boundaries |
| 2026-11-15 | 01 | C# type names | Said `typeof(int).Name` is "Integer" (Java habit) | `int` is an alias for `System.Int32` → "Int32" |
| 2026-11-15 | 01 | Generics: benefits of no type erasure | Forgot the main benefit | Lead with **no boxing for value types** (`List<int>` vs Java `List<Integer>`) = performance |
| 2026-11-15 | 03 | Deferred execution | Thought `.ToList()` result also updates when the source changes | `ToList()` runs the query **once** and stores a snapshot; only the un-materialised query re-runs |
| 2026-11-15 | 03 | `yield return` | Thought the iterator body starts running when the method is called | Calling an iterator runs **nothing**; the body starts on the first `foreach` / `MoveNext()` |
| 2026-11-15 | 04 | Async execution order | Thought an async method only starts when awaited (like `yield`), and that code after `await task` runs before the task finishes | An async method **starts immediately** and runs until its first incomplete `await`; `await task` **waits** for the task to finish before continuing |
| 2026-11-15 | 04 | Deadlock with `.Result` in WPF | Said only "blocks the thread", missed the deadlock | UI thread blocks waiting; the continuation needs the same UI thread → **deadlock**, app freezes |
| 2026-11-15 | 05 | Closures in a `for` loop | Predicted 0,1,2 | Lambdas capture the **variable**, not the value; one shared `i` → 3,3,3. Fix: local copy; `foreach` is safe |
| 2026-11-20 | 07 | Scoped lifetime | Mixed it up: said different within a request, same across requests | **Scoped = one per request**: same inside a request, different for each request |
| 2026-11-20 | 07 | Local secrets | Said `.env` (Laravel habit) | .NET uses **user secrets** (`dotnet user-secrets`) locally; env vars / vault in production |
