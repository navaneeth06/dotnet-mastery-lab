# 03 – Collections & LINQ: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Deferred execution

**Question:** "What is deferred execution in LINQ? What problems can it cause?"

### Short answer

> "Most LINQ operators, like `Where`, `Select` and `OrderBy`, don't run when you
> write them. They build a query that runs only when you **enumerate** it: with
> `foreach`, `ToList()`, `Count()`, `First()` and so on.
> The query re-runs **every time** you enumerate it, against the data as it is
> at that moment. To run it once and keep the results, call `ToList()` or
> `ToArray()`."

### Example

```csharp
var numbers = new List<int> { 1, 2, 3 };
var query = numbers.Where(n => n > 1);           // deferred
var list  = numbers.Where(n => n > 1).ToList();  // runs now, snapshot
numbers.Add(10);
query.Count();   // 3 → re-runs and sees 10
list.Count;      // 2 → snapshot taken before 10 was added
```

### Problems it can cause

1. **Multiple enumeration**: enumerating the same query twice does the work twice.
   With a database query (EF Core), that means **two trips to the database**.
2. **Surprising results**: the source changed between creating and running the query.
3. **Exceptions appear later**: an error inside a `Where` lambda is thrown where the
   query is enumerated, not where it was written.

### Which operators run immediately?

- Deferred: `Where`, `Select`, `OrderBy`, `Skip`, `Take`, `GroupBy`
- Immediate: `ToList`, `ToArray`, `ToDictionary`, `Count`, `Sum`, `First`, `Single`, `Any`, `Max`

### Java comparison

Java streams are lazy too, but a stream can be used only once (a second use
throws `IllegalStateException`). A LINQ query can be enumerated many times and
re-runs each time.

---

## `IEnumerable<T>` vs `IQueryable<T>` 🔥

**Question:** "What's the difference between `IEnumerable` and `IQueryable`?"

### Short answer

> "Both represent a sequence you can query with LINQ, but they run in different places.
> `IEnumerable<T>` runs LINQ **in memory**: the lambdas are normal C# code.
> `IQueryable<T>` builds an **expression tree** that a provider like EF Core
> **translates to SQL**, so filtering, sorting and paging happen in the database.
> With `IQueryable`, I keep adding `Where` / `OrderBy` / `Take` **before**
> materialising with `ToList()`, so only the needed rows are loaded."

### Example

```csharp
db.Users.Where(u => u.Age > 30).ToList();            // ✅ WHERE in SQL, loads 50 rows
db.Users.ToList().Where(u => u.Age > 30).ToList();   // ❌ loads 1M rows, filters in C#
```

### The hidden trap

Switching to `IEnumerable` too early, often without noticing:

```csharp
IEnumerable<User> GetUsers() => db.Users;     // return type is IEnumerable!
GetUsers().Where(u => u.Age > 30);            // ❌ runs in memory → loads the whole table
```

The same happens with `.AsEnumerable()`, or when passing a query to a method
parameter typed `IEnumerable<T>`. Which `Where` gets called is decided by the
**declared type**, at compile time.

### Laravel comparison

`User::where(...)->get()` (SQL filter) vs `User::all()->filter(...)` (PHP filter)
is exactly the same difference.

### Extra detail

`IQueryable<T>` inherits from `IEnumerable<T>`. `IQueryable` methods take
`Expression<Func<T,bool>>` (code as data, so it can be translated to SQL), while
`IEnumerable` methods take `Func<T,bool>` (compiled code).

---

## `yield return`

**Question:** "What does `yield return` do?"

### Short answer

> "It creates an **iterator**: a method that returns items one at a time, lazily.
> Calling the method runs **none** of its body. It only creates the iterator.
> The code runs when someone enumerates it: each `MoveNext()` runs until the next
> `yield return`, hands back that item, and pauses there. The compiler turns the
> method into a state machine behind the scenes.
> It's useful for large or endless sequences (reading a file line by line,
> streaming results), because nothing is loaded into memory all at once.
> This is how LINQ's own deferred operators like `Where` are built."

### Example

```csharp
var numbers = GetNumbers();      // prints nothing!
Console.WriteLine("A");
foreach (var n in numbers) Console.WriteLine(n);
// Output: A, start, 1, middle, 2

IEnumerable<int> GetNumbers()
{
    Console.WriteLine("start");
    yield return 1;
    Console.WriteLine("middle");
    yield return 2;
}
```

### Extra points

- `yield break;` ends the sequence early.
- Enumerating twice runs the body **twice** (same as any deferred query).
- Argument checks inside an iterator also run late. To validate immediately,
  split it into a normal method that checks, plus a private iterator.

### Comparisons

PHP generators (`yield`) work the same way. Java has no keyword for this; you'd
implement `Iterator<T>` by hand.
