# Weak Areas

Topics to revisit. Remove a row once I can answer it confidently without hints.

| Date | Module | Topic | What I got wrong | Correct idea |
|------|--------|-------|------------------|--------------|
| 2026-11-13 | 01 | Where a reference variable is stored | Said both `p` and the `Person` object are on the heap | `p` (a local) is on the stack and holds the address; the object is on the heap |
| 2026-11-13 | 01 | `var` | Thought `var` decides the type at runtime | `var` is decided at **compile time**; `var p = new Person()` is the same as `Person p = ...` |
| 2026-11-13 | 01 | "Pass by reference" wording | Said a class object is "passed by reference" | By default C# passes **everything by value**; for a class, the value copied is the reference ("the reference is passed by value"). Real pass-by-reference needs `ref` |
| 2026-11-13 | 01 | `ref` vs `out` vs `in` | Said all three let a method modify/replace the caller's variable | Only `ref` is "read and replace". `out` = method must assign a value (used to return extra results). `in` = passed by reference but **read-only**. Studied in the ref/out/in topic |
