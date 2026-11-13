# 01 – C# Fundamentals: Model Interview Answers

> Reference answers to learn from. Read them, then close this file and answer
> in your own words in `INTERVIEW.md`. Don't memorise them word for word.

---

## Value types vs reference types

**Question:** "What is the difference between a value type and a reference type?"

### Short answer (say this first, about 20 seconds)

> "The main difference is what happens when you copy them.
> A **reference type**, like a `class`, is shared. When you assign it to another
> variable, both variables point to the same object, so a change through one is
> visible through the other.
> A **value type**, like a `struct` or an `int`, is copied. Each variable gets its
> own copy, so changing one doesn't affect the other."

### Then add an example (shows you understand it)

> "For example, if `Point` is a class and I do `var b = a; b.X = 5;`, then `a.X`
> is also 5. If `Point` is a struct, `a.X` stays the same, because `b` is a
> separate copy."

### Then add the Java comparison (shows experience)

> "Coming from Java: Java only has this copy behaviour for primitives like `int`.
> C# lets you create your own value types with `struct`. In C#, even `int` is
> really a struct (`System.Int32`)."

### Bonus point: the "stack vs heap" trap

Interviewers often follow up with: *"So value types live on the stack?"*

> "Not always. A value type is stored **wherever its container is**.
> A local `int` variable is usually on the stack. But an `int` field inside a
> class object lives on the heap, together with that object.
> So the real difference is copy-vs-share, not stack-vs-heap."

### Follow-up: "Where are `p` and the object stored?"

```csharp
void DoWork()
{
    var p = new Person();
}
```

> "There are two pieces. `p` is a local variable, so it's on the stack, and
> it holds only the **address**. The `Person` object itself is on the heap."

Rule: **a local variable lives on the stack whether it holds a value or an
address.** For a reference type, the variable (address) and the object are
in two different places.

### Side note: `var` is decided at compile time

`var p = new Person();` is exactly the same as `Person p = new Person();`.
The compiler fills in the type when you build. It does NOT change at runtime
(unlike PHP/JavaScript). Same as `var` in Java 10+.

### Mistakes to avoid

- ❌ Saying "value types are always on the stack." (This is the trap above.)
- ❌ Saying "objects are passed by reference." By default, C# (like Java) passes
  a **copy of the reference**. You can change the object, but if you reassign
  the parameter, the caller doesn't see it.
- ❌ Only giving definitions. Always give a small example.

### Words to know

| Term | Simple meaning |
|------|----------------|
| Value type | Copied on assignment (`struct`, `int`, `bool`, `double`, `DateTime`, `enum`) |
| Reference type | Shared on assignment (`class`, `string`, arrays, `List<T>`) |
| Stack | Fast, short-lived memory for a method's local variables |
| Heap | Memory for objects; cleaned up by the garbage collector |

---

## Passing value types and reference types to methods

**Question:** "What happens when you pass a reference type to a method and reassign the parameter inside?"

### Short answer

> "By default, C# passes everything **by value**, meaning the method gets a copy.
> For a struct, it copies the whole value. For a class, it copies the
> **reference** (the address).
> So if the method **changes the object** (`p.X = 100`), the caller sees the
> change, because both addresses point to the same object.
> But if the method **reassigns the parameter** (`p = new Point()`), it only
> changes its own copy of the address. The caller's variable still points
> to the old object."

### The two cases (remember this table)

| Inside the method | Struct | Class |
|-------------------|--------|-------|
| `p.X = 100;` (change a field) | Caller does NOT see it (copy) | Caller DOES see it (same object) |
| `p = new ...;` (reassign) | Caller does NOT see it | Caller does NOT see it (only the copy of the address changed) |

### Java comparison

> "This is exactly the same as Java. Java is always pass-by-value too.
> The difference is that C# also has the `ref` keyword, which really passes
> the variable itself. With `ref`, reassigning inside the method **would**
> change the caller's variable. Java has no equivalent."

### Mistake to avoid

- ❌ Saying "objects are passed by reference." The correct phrase is
  "**the reference is passed by value**."

### Follow-up: "How can a method replace the caller's object?"

> "Use the `ref` keyword. Then the method receives the caller's **variable
> itself**, not a copy, so reassigning it inside the method changes the caller's
> variable too. `ref` has to be written both in the method and at the call site,
> so it's obvious when reading the code.
> Java can't do this. There, you'd return the new object instead (which is
> often the cleaner design in C# too)."

```csharp
void Replace(ref PointClass p) { p = new PointClass { X = 500 }; }

var c = new PointClass { X = 1 };
Replace(ref c);   // c.X is now 500
```

Don't mix up the three keywords:
- `ref`: read it **and** can replace it.
- `out`: the method **must** assign it (used to return extra results, e.g. `int.TryParse`).
- `in`: passed by reference but **read-only**, so it can't be changed.
