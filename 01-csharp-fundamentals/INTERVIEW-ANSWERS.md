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

---

## Boxing and unboxing

**Question:** "What is boxing? Give an example where it happens without you noticing."

### Short answer

> "Boxing is when a value type, like an `int`, is converted to `object` or to an
> interface. The runtime creates a new object on the heap and puts a **copy** of
> the value inside it. Unboxing is the opposite: a cast like `(int)obj` takes
> the value back out.
> It matters for performance: every box is a heap allocation that the garbage
> collector has to clean up, and it happens silently, with no keyword."

### Hidden boxing examples (give one or two)

- Passing an `int` to a method that takes `object`.
- Storing value types in old non-generic collections like `ArrayList`.
- Treating a struct as an interface, e.g. `IComparable c = 5;`.

### Fix

> "Use generics. `List<int>` stores ints directly with no boxing, unlike the old
> `ArrayList`."

### Java comparison

> "Java has the same idea with autoboxing (`int` → `Integer`). But Java generics
> can't hold primitives, so a `List<Integer>` boxes every number. In C#,
> `List<int>` doesn't box at all, because C# generics keep the real type at
> runtime (no type erasure)."

### Trap: `==` on boxed values

```csharp
object x = 5;
object y = 5;
x == y        // False: compares addresses (two different boxes)
x.Equals(y)   // True: compares the values inside
```

Java difference: in Java, `Integer a = 5, b = 5; a == b` is **true**, because
Java caches small Integers (-128 to 127), but it's false for 1000. C# has no
such cache, so two boxes are always two different objects.

### Mistakes to avoid

- ❌ Saying boxing "converts to a reference". It **copies** the value into a new
  heap object. Changing the original afterwards doesn't change the box.
- ❌ Forgetting the performance cost: heap allocation plus GC work.

### Trap: unboxing to a different type

```csharp
object box = 5;            // boxed int
long n = (long)box;        // compiles, but throws InvalidCastException at runtime
long ok = (long)(int)box;  // unbox to the exact type first, then convert
```

> "You can only unbox to the **exact type** that was boxed. The compiler can't
> check this, so it fails at runtime with an `InvalidCastException`."

---

## Records

**Question:** "How do records differ from classes? What does value equality mean?"

### Short answer

> "A record is a type built for holding data. The compiler generates the
> constructor, properties, `ToString`, and most importantly **value equality**:
> two records are equal if all their properties are equal, even if they're
> different objects. A normal class compares by reference (same object or not).
> Records are immutable by default, and the `with` expression creates a modified
> copy. They're a good fit for DTOs and value objects."

```csharp
public record Person(string Name, int Age);
new Person("Ann", 30) == new Person("Ann", 30)   // True (a class would give False)
```

### Java comparison

> "Similar to Java records, but in Java `==` still compares references and you
> must use `.equals()`. In C#, `==` is overridden for records, so it compares
> values. C# also has `with` for non-destructive copies."

### Follow-up: "Is `with` a deep or a shallow copy?"

> "**Shallow.** `with` copies each property's value. For a reference-type
> property like a `List`, it copies the **address**, so both records share the
> same list. Records are only immutable at the **top level**: you can't reassign
> `Items`, but you can still `Add` to the list inside it."

```csharp
public record Order(string Id, List<string> Items);
var o2 = o1 with { Id = "A2" };
o2.Items.Add("Book");     // o1.Items ALSO has "Book" (shared list)
```

Fix: use an immutable collection (e.g. `IReadOnlyList<T>` or `ImmutableList<T>`).

### Mistakes to avoid

- ❌ "Records are immutable, so nothing inside can change." Immutability is shallow.
- ❌ Saying `==` on records compares references. It compares values.

---

## `==` vs `Equals()` and strings

**Question:** "What is the difference between `==` and `.Equals()`?"

### Short answer

> "`Equals()` is a virtual method: the **actual object** decides what equal means,
> so `string` and `int` compare values. `==` is an operator, and C# picks which
> `==` to use at **compile time**, based on the **variable's declared type**.
> For reference types, `==` compares addresses unless the type overloads it.
> `string` and records overload `==` to compare values."

### The trap

```csharp
string a = "hello";
string b = "HELLO".ToLower();
a == b                 // True: string's == compares text

object o1 = a;
object o2 = b;
o1 == o2               // False: object's == compares addresses
o1.Equals(o2)          // True: Equals runs string's version at runtime
```

### Java comparison (Q16)

> "In Java, `==` on strings compares references, so you must use `.equals()`.
> In C#, `string` overloads `==` to compare text, so `==` is safe, as long as
> the variables are typed as `string`, not `object`."

### Strings are immutable (Q12)

> "Methods like `ToUpper()` return a **new** string; the original never changes.
> Immutability makes strings thread-safe and safe to use as dictionary keys.
> Because of that, building a string in a loop with `+=` creates many objects,
> so use `StringBuilder` for that."

**Interning:** identical string **literals** in code share one object, so
`(object)"hi" == (object)"hi"` is `True`. Strings created at runtime (like
`ToLower()`) are new objects, which is why `o1 == o2` above is `False`.

---

## Nullable reference types (Q9, Q10, Q20)

**Question:** "What problem do nullable reference types solve? Do they prevent null at runtime?"

### Short answer

> "They help prevent `NullReferenceException`. With nullable enabled, a plain
> `string` means 'should never be null' and `string?` means 'may be null'.
> The compiler tracks this and **warns** when you might dereference a null or
> assign null to a non-nullable variable.
> But it's **compile-time only**: there is no runtime check, so null can still
> arrive from JSON, a database, or a library without annotations. You still
> need validation at those boundaries."

### Operators

```csharp
city?.Length          // null-conditional: null instead of a crash
city ?? "Unknown"     // null-coalescing: fallback value
city?.Length ?? 0     // combined
name ??= "default";   // assign only if null
city!.Length          // null-forgiving: silences the warning, no runtime effect
```

### Q10: why is overusing `!` a smell?

> "`!` only switches off the compiler warning. It doesn't check anything at
> runtime. Using it a lot means you're bypassing the safety net, and if you're
> wrong, it still crashes. Prefer a real null check, `??`, or fixing the
> type annotation."

### Java comparison (Q20)

> "Java's `Optional<T>` is a runtime wrapper object, mainly used for return
> values. C# nullable reference types are compile-time annotations on the
> type itself: no wrapper, no allocation, and they work for fields,
> parameters and locals too."

### Don't confuse with `int?`

`int?` (a nullable **value** type) is a real runtime type, `Nullable<int>`, with
`.HasValue` and `.Value`. `string?` is only an annotation: at runtime it's the
same `string`.
