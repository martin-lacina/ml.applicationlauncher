---
name: csharp-13
description: Write, review and modernize C# 13 (.NET 9) code correctly. Covers every C# 13 feature - `params` collections (`params ReadOnlySpan<T>`, `params List<T>`, `params IEnumerable<T>`), the new `System.Threading.Lock` object and how `lock` lowers for it, the `\e` escape sequence, method group natural type improvements, `[^1]` index access in object initializers, `ref` locals and `unsafe` blocks in iterators and async methods, `ref struct` interfaces, the `allows ref struct` anti-constraint, partial properties and partial indexers, `OverloadResolutionPriorityAttribute`, and the preview `field` keyword. Use when code targets net9.0 or LangVersion 13, when adding a `params Span<T>` overload next to an existing array overload, when evolving a library's overload surface without breaking compiled binaries, when a `ref struct` must implement `IDisposable` or `IEnumerable<T>`, when a generic API must accept `Span<T>` as a type argument, or when the build reports CS9259, CS0656, CS8917, CS9035, CS9256, CS1066, CS9507, CS8618, a new CS0121 ambiguity, or VB and other-language consumers that cannot call your `params` API. Every C# 13 feature stays available in C# 14 (`net10.0`) and C# 15 (`net11.0`), use the `csharp-14` and `csharp-15` skills for what those releases add or change.
---

# C# 13

C# 13 ships with the .NET 9 SDK and Visual Studio 2022 version 17.12+. It is the default language
version for projects that target `net9.0`. Its features are:

| Feature | Keyword / syntax | Needs preview? |
| --- | --- | --- |
| `params` collections | `params ReadOnlySpan<T>`, `params Span<T>`, `params List<T>`, `params IEnumerable<T>`, `params IList<T>` | No |
| New `lock` object | `System.Threading.Lock`, `lock (myLock) { }` → `using (myLock.EnterScope())` | No |
| New escape sequence | `'\e'`, `"\e"` — the ESC character U+001B | No |
| Method group natural type | `var handler = MyMethodGroup;` | No |
| Implicit index access in object initializers | `{ buffer = { [^1] = 0 } }` | No |
| `ref` and `unsafe` in iterators and async | `ref int x` … `await`, `unsafe { }` inside a `yield` method | No |
| `ref struct` interfaces | `ref struct S : IDisposable, IEnumerable<int>` | No |
| `allows ref struct` | `where T : allows ref struct` | No |
| Partial properties and indexers | `partial int Prop { get; set; }` plus an implementing part | No |
| Overload resolution priority | `[OverloadResolutionPriority(1)]` | No |
| `field` backed properties | `field` inside a property accessor | **Yes**, preview in C# 13 (Visual Studio 2022 17.12, `LangVersion preview`). Stable in C# 14 → use the `csharp-14` skill |

**Everything in this skill stays available in later language versions.** C# 14 (`net10.0`) and C# 15
(`net11.0`) include every C# 13 feature, so nothing here becomes obsolete when you upgrade. What
changes is what later releases *add on top* and, in three places, what they *strengthen*:

- `field` is a C# 13 preview and a normal C# 14 feature. Read section 11 only to recognise the preview
  syntax; implement it with the `csharp-14` skill, which has the full rules.
- `partial` properties (section 9) are extended by `partial` events and `partial` constructors in C# 14.
- The C# 14 first-class span conversions build directly on the `params`-collection and
  collection-expression overload rules in sections 1 and 12. An API you design here behaves differently
  once a consumer recompiles it for `net10.0`.

Extension indexers, union types, `closed` hierarchies, `with(...)` collection arguments, labeled
`break`/`continue` and the memory-safety model are **C# 15**, not C# 13. Do not duplicate them here.

Read section 0 first, then the section for the feature you are using. Each section ends with the
mistakes to avoid.

---

## 0. Before using any C# 13 feature

1. **Check the language version.** Look at `TargetFramework` and `LangVersion`. `net9.0` with no
   `LangVersion` gives C# 13. To see the active version, temporarily add `#error version`: the compiler
   reports CS8304 with the compiler and language versions. Full C# 13 needs the .NET 9 SDK with
   Visual Studio 2022 17.12+ (the InlineArray and iterator-context breaks in section 12 landed in 17.11).
2. **Don't set `<LangVersion>latest</LangVersion>`.** Microsoft warns against it because it varies by
   machine, and using a language version newer than the target framework's default is unsupported. If
   code must stay on C# 12 semantics, set `LangVersion 12` explicitly rather than relying on the TFM.
3. **Two features are silently invisible below C# 13.** `OverloadResolutionPriorityAttribute` is
   **ignored** at LangVersion 12 and lower (by design, so that .NET 9 APIs using it stay consumable), and
   the expanded form of a non-array `params` collection is not applicable at all. A call that resolves in
   your IDE can fail in a project pinned to an older language version.
4. **C# 13 changes overload resolution in three places.** `params` collections (section 1), better
   conversion from a collection expression (section 12) and method group natural type (section 4). The
   *same source* compiled with LangVersion 13 can pick a different overload, become ambiguous (CS0121),
   or stop inferring a delegate type (CS8917). Audit an existing codebase for these before flipping the
   language version.
5. **Ref-safety errors are not demoted by `unsafe`.** In sections 6, 7 and 8 the compiler reports
   errors for `ref`/`ref struct` values crossing an `await` or `yield` boundary even inside an `unsafe`
   context. Unlike some other ref-safety rules, these are never downgraded to warnings.

---

## 1. `params` collections

`params` is no longer restricted to arrays. A `params` parameter may be any type that a C# 12 collection
expression `[1, 2, 3]` can produce, which removes the "add a `params` array overload that allocates an
array and forwards it" pattern.

```csharp
public void Concat<T>(params ReadOnlySpan<T> items)
{
    for (int i = 0; i < items.Length; i++)
    {
        Console.Write(items[i]);
        Console.Write(' ');
    }
    Console.WriteLine();
}

Concat(1, 2, 3);                    // expanded form: the span is created from the arguments

ReadOnlySpan<int> numbers = [1, 2];
Concat(numbers);                    // normal form: one argument of the parameter type
```

### Valid `params` target types

The parameter type must be a valid collection-expression target, and each case fixes the *element type*:

| Target type | Element type |
| --- | --- |
| Single-dimensional array `T[]` (unchanged since C# 4) | `T` |
| `System.Span<T>`, `System.ReadOnlySpan<T>` | `T` |
| Type with an eligible collection-expression **Create method** invocable with no extra arguments | from the Create method |
| Class or struct implementing `System.Collections.IEnumerable` with an accessible no-arg constructor **and** an accessible instance `Add(single value)` method | the type's `foreach` iteration type |
| `IEnumerable<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `ICollection<T>`, `IList<T>` | `T` (the compiler synthesizes the concrete collection, e.g. a `List<T>`) |

Immutable collections work through a Create method, e.g. `params ImmutableArray<T>`.

### Declaration and binding rules

- The parameter collection must be **last**. It may follow an optional parameter, but **cannot have a
  default value** — omitting the arguments creates an empty collection instead.
- `params` still cannot combine with `in`, `ref`, `ref readonly` or `out`.
- An invocation is either the **normal form** (one expression implicitly convertible to the parameter
  type) or the **expanded form** (zero or more arguments convertible to the element type, collected as if
  they were written as a collection expression). The collection is built from the **original unconverted**
  arguments.
- The expanded form of a **non-array** parameter collection exists only in C# 13+, and only if no method
  with the same signature as the expanded form is already declared in the same type.
- Normal form beats expanded form when both are applicable.
- `params` on a `ref struct` type (a span) is implicitly **`scoped`**. Override that with
  `[UnscopedRef]`. An override of such a parameter must restate `params`, `scoped`, or both.

  ```csharp
  class Base
  {
      internal virtual void M1(params Span<int> s2) { }
  }

  class Derived : Base
  {
      // error: a scoped parameter must be restated as `params` or `scoped`
      internal override void M1(Span<int> s2) { }
  }
  ```

- Metadata differs from `params` arrays: a non-array `params` parameter is marked with
  `System.Runtime.CompilerServices.ParamCollectionAttribute`, **not** `ParamArrayAttribute`. Visual
  Basic, and likely other languages and tools, see it as an ordinary collection parameter and cannot use
  the expanded form.
- If the collection type has `required` members that its constructor does not initialize, the error is
  reported **at the declaration site** (CS9035), not at each call site.
- The expanded form of a non-array `params` collection is **not supported in expression trees** (same
  restriction as collection expressions) and is **not supported by the runtime `dynamic` binder**: such a
  call is a compile-time error.

### Overload resolution with several `params` overloads

Tie-breaking between two applicable candidates whose parameter types are equivalent:

1. A non-generic method beats a generic one.
2. A method applicable in its **normal form** beats one applicable only in its expanded form.
3. Two expanded-form `params` candidates: the one whose collection takes **fewer elements** wins.
4. Then "more specific parameter types", then non-lifted over lifted operators, then fewer optional
   parameters, then better parameter-passing mode.
5. Only at the very end, and only **when the same argument list feeds the collection in both methods**:
   for non-span types, the one implicitly convertible to the other; `ReadOnlySpan<T>` beats `Span<T>`;
   a span beats an array or array-interface type — each with an identity element conversion.

Consequence: overloads that differ only by `params` collection type behave the same as an explicit
`[1, 2, 3]` argument, so adding a `params ReadOnlySpan<T>` overload silently wins the call from
`params T[]` after a recompile. That is the intended use of the feature, and the reason to review the
call sites.

```csharp
static void M2<T>(params Span<T> y) { }   // chosen in C# 13: exact element type, and generic-but-span
static void M2(params int[] y) { }

M2([1]);  // Span overload
M2(1);    // array overload (not the generic one)
```

There is no tie-break at all when the **element types differ**, so an empty call is ambiguous:

```csharp
static void M(params ReadOnlySpan<int> a) { }
static void M(params Span<int?> a) { }

M();   // ambiguous in C# 13 (and `M1(params int[])` / `M1(params int?[])` were already ambiguous)
M([]); // ambiguous
```

Fix it at the call site with `.AsSpan()`, an explicit cast, or a collection expression with an explicit
target type — or in the API with `OverloadResolutionPriorityAttribute` (section 10).

### Order of evaluation differs from `params` arrays

For a non-array collection, the instance is created and populated **after the lexically previous
argument and before the lexically following one**. `params` arrays are created last, after every argument
is evaluated. This is observable:

```csharp
Test(b: GetB(), c: GetC(), a: GetA());
// order: GetB, the collection (which calls GetC), GetA, then Test
```

The same rule applies to a `params` indexer used in compound assignment (`p[GetA(), GetC()]++`) and in an
object initializer. For object initializers the collection instance is created once and reused across
the indexer getter calls; with a `params` array a **new array is allocated for each getter call**.

### Mistakes to avoid

- Do not assume the call still binds the array overload: adding `params ReadOnlySpan<T>` next to
  `params T[]` changes which overload existing callers get when they recompile.
- Two `params` overloads with different element types are ambiguous for an empty argument list — design
  one collection type per method, or prioritize with `[OverloadResolutionPriority]`.
- Never publish a non-array `params` API that Visual Basic or another .NET language must call in the
  expanded form; it cannot.
- Do not call a `params`-collection method from an expression tree or through `dynamic` with expanded
  arguments.
- Do not `params`-a mutable collection type whose constructor does not initialize its `required` members
  (CS9035 at the declaration).
- Do not rely on arguments being evaluated left-to-right before the collection is built; name your
  parameters or avoid side-effecting arguments.
- If a `params Span<T>` must escape, add `[UnscopedRef]` — remember the default is `scoped`.
- Restate `params` or `scoped` on every override of such a parameter.

---

## 2. New `lock` object

.NET 9 adds `System.Threading.Lock`, a purpose-built synchronization primitive. C# 13 special-cases it
in the `lock` statement, so switching to it is a type change only.

```csharp
private readonly Lock _gate = new();

lock (_gate)              // lowered to: using (_gate.EnterScope()) { ... }
{
    // critical section
}
```

The required shape is exact:

```csharp
namespace System.Threading
{
    public sealed class Lock
    {
        public void Enter();
        public void Exit();
        public Scope EnterScope();

        public ref struct Scope
        {
            public void Dispose();
        }
    }
}
```

- `lock (x)` where `x` is `System.Threading.Lock` uses `EnterScope()`/`Scope.Dispose()`. Any other
  reference type keeps the traditional `System.Threading.Monitor`-based behavior.
- `Lock.Scope` is a **`ref struct`**. That is what makes the `lock` body free of a thread lookup on exit,
  and what makes `await` inside such a `lock` a compile-time error. Extract the locked region into a
  separate non-`async` method (or use `Lock.Enter()`/`Exit()` in a `try`/`finally` if you must, at the
  cost of the thread check).
- Because `Scope` is a `ref struct`, `yield return` inside `lock (myLock)` is an error, while `yield
  return` inside a monitor-based `lock` is only a warning.
- The compiler **warns** when a `Lock` is upcast to `object` or to an interface, because the value can
  then reach a monitor-based `lock`. The warning also fires for equivalent explicit casts.

  ```csharp
  object l = new Lock();  // warning
  lock (l) { }            // this is Monitor-based locking, not Lock
  ```

  No warning when the conversion cannot lead to locking, e.g. the implicit conversion inside
  `if (l != null)`. To deliberately force monitor semantics: use `Monitor` APIs directly, suppress the
  warning, or cast indirectly through a helper such as `static object AsObject<T>(T v) => (object)v;`.

### Mistakes to avoid

- Do not store a `Lock` in a `Dictionary<object, object>`, pass it as `object`, or box it into a generic
  type parameter typed `object` — you silently lose the new semantics.
- Do not expect `lock (x)` to use the new API when `x` is a type parameter, even with
  `where T : Lock`; locking on a type parameter is always monitor-based.
- Do not reuse a `Lock` field with `Monitor.Enter`/`Monitor.Exit` mixing: `Enter`/`Exit` are the type's
  own API, and `Monitor` treats it as an ordinary object.
- Do not `await` or `yield` inside `lock (myLock)`. In C# 13 you *can* put `lock (myLock)` inside an
  `async` method or an iterator, provided the locked body itself contains no `await` or `yield` — that was
  impossible before C# 13.
- Do not implement `IAsyncDisposable` on a type whose only locking story is `Lock`: a `Lock` scope is a
  `ref struct`, so `await using (_gate.EnterScope())` does not compile.
- The shape is only loosely validated: a non-sealed look-alike `Lock` type compiles but the feature
  assumes no derived types.

---

## 3. New escape sequence `\e`

`\e` is the simple escape for the ESC character, U+001B. It joins `\'` `\"` `\\` `\0` `\a` `\b` `\f`
`\n` `\r` `\t` `\v` in the simple-escape list, and is valid in both character and (non-verbatim) string
literals.

```csharp
char esc = '\e';
Console.Write("\e[2J");          // clear screen, no ambiguity

Debug.Assert('\e' == (char)0x1B);
Debug.Assert('\e' == '\u001b');
Debug.Assert('\e' == '\x1b');
```

It exists because `\x1b` is greedy: hex escapes consume **all** following hex digits, so `"\x1b0"` is one
character U+01B0, not ESC followed by `0`. `"\e0"` is always two characters.

### Mistakes to avoid

- `\e` has no meaning in a verbatim string (`@"\e"`) or a raw string literal — the backslash stays
  literal, exactly as with `\n`.
- Do not "fix" an existing `"\x1b"` by appending digits; use `\e` or `\u001b` when the next character can
  be a hex digit (`0`-`9`, `a`-`f`).
- `\E` is not an escape sequence; only the lowercase form exists.

---

## 4. Method group natural type

Assigning a method group to `var` (or using it where no delegate type is known) requires the method group
to have one **natural type**. C# 13 determines it scope-by-scope, the way overload resolution already
works: build the candidates for the containing type, prune the ones that cannot possibly apply (generic
methods whose arity or constraints fail with no type arguments, extension methods that cannot be reduced),
and only move to the next extension scope if nothing survives. If the surviving candidates do not share a
signature, there is no natural type.

```csharp
static void Handle(string s) { }
static void Handle<T>(T v) { }        // pruned: no type arguments supplied

var handler = Handle;                 // Action<string>
handler("x");
```

### Breaking behavior in the .NET 9 SDK

Previously the compiler picked inconsistently when candidates differed only by default or `params`
parameters — the answer depended on the declaration order in source. Now those candidates are considered,
which turns the old silent choice into an error:

```csharp
static class E
{
    public static void M(this C c, params int[] x) { }
}
class C { public void M(int[] x) { } }

var d = new C().M;   // was Action<int[]>, now CS8917: the delegate type could not be inferred
```

This inference algorithm change applies at LangVersion 13 regardless of the surrounding language version.
At LangVersion 12 and lower, `params` must match across all candidates to infer a unique signature.

### Mistakes to avoid

- When inference fails, write the delegate type explicitly (`Action<int[]> d = new C().M;`) rather than
  reordering methods to make the old inference "work".
- Don't rely on a method group's natural type across an upgrade: candidates that were pruned in C# 13
  participated in C# 10-12, and pruning is what makes a unique signature possible.
- Generic methods with an unmatched arity or unsatisfied constraints no longer block inference — do not
  add an unrelated generic overload just to "help" inference.

---

## 5. Implicit index access in object initializers

The from-the-end operator `^` is allowed in an object initializer index, so you can address the tail of an
existing array or indexer-backed collection without counting from the front.

```csharp
public class TimerRemaining
{
    public int[] Buffer { get; set; } = new int[10];
}

var countdown = new TimerRemaining
{
    Buffer =
    {
        [^1] = 0,
        [^2] = 1,
        [^3] = 2,
    }
};
```

Rules and semantics:

- This applies to **single-dimension** index access: an array member, or an indexer that accepts
  `System.Index`. It is an index, not a range — `[^3..]` in an object initializer is not this feature.
- `^n` is resolved at runtime against the **length of the collection instance that already exists** (the
  property initializer value, or the value the getter returns). It does not resize anything.

### Mistakes to avoid

- The writes go into the existing instance. A getter that returns a fresh array or a defensive copy
  throws the assignments away — `buffer[^1] = 0` appears to do nothing.
- Out-of-range from the end is still out of range: `[^11]` on a 10-element array throws
  `IndexOutOfRangeException` exactly as `[-1]` would.
- For indexers, the parameter type must be `System.Index`; an `int` indexer does not accept `^`.
- On C# 12 and earlier this is a syntax error in an object initializer, so a codebase that must stay on
  an older language version has to index from the front.

---

## 6. `ref` and `unsafe` in iterators and async methods

C# 13 unifies iterators and `async` methods: `ref` locals, `ref struct` locals and `unsafe` blocks are
allowed in both, **as long as they do not cross an `await` or `yield` boundary**.

```csharp
IEnumerable<int> Enumerate(ReadOnlySpan<int> source)   // iterator with a ref struct parameter is fine
{
    for (int i = 0; i < source.Length; i++)
    {
        yield return source[i] * 2;                    // source is a parameter, not hoisted
    }
}

async Task RunAsync(char[] chars)
{
    await Task.Yield();

    var span = chars.AsSpan();      // declared and used after the last await: allowed
    Console.WriteLine(span.Length);

    await Task.Yield();
    // Console.WriteLine(span.Length);  // error: span is definitely unassigned after an await
}
```

### The rules that drive the errors

- After an `await` expression or a `yield return` statement, **every `ref` local and `ref struct` local in
  scope becomes definitely unassigned**. Reading it afterwards is an error, and this error is *not*
  demoted to a warning by `unsafe`.
- An `unsafe` context may not contain an `await` expression or a `yield return` statement at all.
- An iterator **block** always defines a safe context, even inside an `unsafe` class, even when the
  iterator member carries the `unsafe` modifier (the modifier now affects the signature only). You may
  open an explicit `unsafe { }` block inside an iterator, but the `yield` statements must stay outside
  it.
- Iterator and async parameter lists still cannot contain `in`, `ref`, `ref readonly`, `out`, a
  `ref struct` type, or (in an iterator) a pointer type.
- Taking the address of a local or parameter **in an iterator** is a compile-time error at C# 13 (in an
  `async` method it is the C# 12 warning about hoisted variables).
- Constructions that need no hoisting now work: `using` on a `ref struct`, `foreach`/`await foreach` over
  a `ref struct` enumerable, `foreach (ref int x in …)` and `lock (new Lock())` inside an `async` method,
  provided no `await` appears inside them.

### Mistakes to avoid

- Do not try to keep a `Span<T>` "alive" across `await`; re-acquire it after each suspension point, or
  keep the underlying array/field as the long-lived thing and slice it per segment.
- Do not write `unsafe` iterators that `yield` inside the unsafe block, and do not `await` inside one.
- Do not assume a lambda inside an iterator inherits an unsafe context — it never could use unsafe
  constructs, and a local function in an iterator now genuinely starts a safe context (see section 12).
- `unsafe` on an iterator is legal in C# 13 but is rarely useful: it only enables pointer-typed
  parameters and an unsafe `set` accessor.
- `stackalloc` results are not preserved across `await` or `yield`; unsafe code does not protect you from
  use-after-free here.

---

## 7. `ref struct` interfaces

A `ref struct` can implement interfaces in C# 13, which lets a stack-only type participate in
abstractions such as `IDisposable`, `IEnumerable<T>` or your own `IReader<T>`.

```csharp
ref struct FileHandle : IDisposable
{
    private readonly SafeHandle _handle;

    public void Dispose() => _handle.Dispose();
}
```

Abstraction is still denied to the value itself: a `ref struct` can never be converted to an interface it
implements, because that is a boxing conversion. The interface members are reachable only through a type
parameter that allows ref struct (section 8).

```csharp
FileHandle h = GetHandle();
IDisposable d = h;      // error: cannot box a ref struct

static void Use<T>(T value) where T : IDisposable, allows ref struct
{
    using (value) { }   // legal: the compiler knows T is IDisposable
}
```

### Rules

- A `ref struct` must implement **every** member of the interface, including members that have a default
  interface implementation. Default interface members are not available to a `ref struct`, and the runtime
  throws if one is invoked on a `ref struct` type.
- A **non-virtual** (for example `sealed`) instance interface member cannot be invoked on a type parameter
  that allows ref struct — that call is a compile-time error, because the default implementation would box
  `this`. Invoking a *virtual* member with a default body on such a type parameter can still throw at
  runtime if a DIM was added to the interface after the implementing type was compiled.
- `using` recognizes a pattern `Dispose()` method first and an `IDisposable` implementation second. On a
  type parameter that allows ref struct, only the **interface** implementation counts, so
  `where T : IMyDisposable, allows ref struct` does not satisfy `using (t)` — `IDisposable` must be in the
  effective interface set.
- `await using` and `await foreach` are **still off limits** for ref-struct resources and enumerators: both
  would have to keep a `ref struct` alive across an `await`. Give the type a synchronous `Dispose()` and use
  plain `using` (allowed in an `async` method as long as no `await` appears in the `using` body, section 6).
- `foreach` recognizes `IEnumerable`/`IEnumerable<T>` on a `ref struct` collection and `IDisposable` on a
  `ref struct` enumerator. A type-parameter enumerator that allows ref struct must be known to implement
  `IDisposable`, otherwise `foreach` fails (CS9507).
- `[UnscopedRef]` is allowed on interface methods and properties. A member may implement an
  `[UnscopedRef]` interface member with or without the attribute, but a member **with** `[UnscopedRef]`
  cannot implement an interface member that lacks it.
- `Action<…>`/`Func<…>` still do not accept a `ref struct` type argument on .NET 9 — the BCL did not
  auto-apply the anti-constraint (section 8).

### Mistakes to avoid

- Do not write `IEnumerable<int> seq = myRefStruct;` or pass a `ref struct` where an interface is
  expected. Bind it to a type parameter instead.
- Do not add a default interface method to an interface that a `ref struct` implements — existing
  implementors break until they are recompiled, and any DIM call throws.
- Do not `sealed` (non-virtual) an interface instance member you expect to be called through an
  `allows ref struct` type parameter.
- Add `[UnscopedRef]` to an interface member at definition time. Adding or removing it later is a source
  breaking change.
- Do not implement only some interface members because "the interface provides a default".

---

## 8. `allows ref struct`

`allows ref struct` is an **anti-constraint**: it removes the implicit rule that a `ref struct` can never
be a type argument.

```csharp
static T Identity<T>(T value) where T : allows ref struct => value;

var span = Identity(new Span<int>(new int[10]));   // works
```

```csharp
public class Buffer<T> where T : allows ref struct
{
    // T behaves exactly like a ref struct here
    public void Store(scoped T item) { }
}
```

### Rules

- A type parameter bound by `allows ref struct` behaves like a `ref struct`: it cannot be boxed, follows
  ref/escape analysis like a `ref struct`, cannot be used in `static` fields or array elements, and can be
  marked `scoped`.
- It must be the **last** constraint in the `where` clause.
- It cannot coexist with `where T : class` or with a constraint to a known reference type. Combining it
  with interface constraints, a parameterless-constructor constraint or a type-parameter constraint is
  fine (`where T : IDisposable, new(), allows ref struct`).
- A type parameter used as a type argument must itself allow ref structs: `M<T>` only accepts `T` if `M`'s
  parameter also allows ref struct.
- The anti-constraint is **not inherited** through a constraint chain: `where S : T` does not let `S` be
  substituted with a `ref struct`.
- Metadata uses `GenericParameterAttributes.AllowByRefLike` (`gpAllowByRefLike`); you can probe runtime
  support with `System.Runtime.CompilerServices.RuntimeFeature.ByRefLikeGenerics`.

### Versioning guidance for API authors

- Adding `allows ref struct` to a type parameter is normally source-compatible ("if it compiles, it is
  safe"), but only for definitions without implementations. Do not add it to an `abstract` method or type
  that may later gain an implementation using `T` in a ref-unsafe way — a later `virtual` body breaks
  consumers.
- **Removing** `allows ref struct` is always a breaking change, source and binary.
- The BCL opted in selectively; never assume `Func<T>`, `Action<T>` or `IEnumerable<T>` accepts a
  `ref struct` type argument on your target framework. Check the specific API.
- `allows ref struct` does not make `Span<Span<T>>` legal: `Span<T>` itself does not carry the
  anti-constraint, and ref-struct `ref` fields are not supported.

### Mistakes to avoid

- Do not put `allows ref struct` anywhere but last in the constraint list, and never next to `class`.
- Do not box such a `T`: `I1 M1<T>(T p) where T : I1, allows ref struct => p;` is an error.
- Do not declare a `static` field, array element or capture of that `T`.
- Do not assume `T` is substitutable by another type parameter unless that other type parameter allows
  ref struct too.
- Do not use `await using` or `await foreach` with a `ref struct` resource or enumerator, or with an
  `allows ref struct` type parameter — either would have to survive an `await`, which ref safety forbids
  (section 7). Use `using` / `foreach` instead.

---

## 9. Partial properties and partial indexers

Partial properties let one declaration state *what* a property is, and another (often generated, often in
another file) state *how* it works — the same split that partial methods already had.

```csharp
public partial class Widget
{
    // Defining declaration: accessor bodies are semicolons.
    public partial string Name { get; set; }
}

public partial class Widget
{
    private string _name = "";

    // Implementing declaration.
    public partial string Name
    {
        get => _name;
        set => _name = value;
    }
}
```

Indexers are supported the same way, including attribute merging across parts:

```csharp
public partial int this[[Attr1] int x] { [Attr2] get; set; }
public partial int this[[Attr3] int x] { get => _store[x]; [Attr4] set => _store[x] = value; }
// emitted: public int this[[Attr1, Attr3] int x] { [Attr2] get …; [Attr4] set … }
```

### Rules

- Only in `partial` types. Exactly **one defining** and **one implementing** declaration — no three-way
  splits, and no splitting accessors across parts.
- **Defining** declaration: all accessor bodies are semicolons and the declaration is not `extern`.
  Everything else is the **implementing** declaration. A partial property can therefore never be an
  auto-property; for a trivial body use `field` (section 11) or an expression-bodied accessor.
- Only the **defining** declaration participates in lookup and emit; nullable analysis and Go To
  Definition may see through to the implementation part.
- No `abstract` modifier. A partial property cannot explicitly implement an interface property.
- Accessor modifiers must match (`private set`, `init`) — a missing or extra accessor is an error, not a
  warning. Type/ref-kind differences and tuple-name differences are errors. Everything else is a
  signature-difference warning (CS9256).
- Nullable-context differences never warn. Default parameter values need not match, but a default value
  on an implementing partial indexer warns that it has no effect (CS1066). Differing parameter names warn
  and the **defining** names are used at use sites and in metadata.
- Attributes are merged across parts in corresponding positions, in unspecified order, duplicates kept.
  Caller-info attributes (`[CallerMemberName]` and friends) count only from the defining part.
- Doc comments are allowed on either part. If both parts have them, the **defining** part's comments are
  dropped and the implementation part's are emitted. Doc comments on accessors are not supported.

### Mistakes to avoid

- Do not write two semicolon-bodied partial properties expecting auto-implementation — one part must
  provide the body.
- Keep parameter names identical across the two parts, otherwise `<paramref>` in a doc comment points at a
  name that does not exist in the emitted signature (CS1734 next to CS9256).
- Do not put a default value on the implementing part of a partial indexer; put it on the defining part.
- Nullable warnings (for example CS8618 for a non-nullable property) are reported against the constructor
  using the *defining* signature; annotate the defining declaration.
- `partial` before the type is not optional — the compiler needs `partial` on the type and on both
  property declarations.
- `partial` events and `partial` constructors do **not** exist in C# 13. They are C# 14; see the
  `csharp-14` skill.

---

## 10. Overload resolution priority

`System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute(int priority)` lets an API author
make one overload win, even when the alternatives would be ambiguous or would be chosen first. This is the
tool for evolving a library surface without breaking binaries that were compiled earlier.

```csharp
using System.Runtime.CompilerServices;

class C1
{
    [OverloadResolutionPriority(1)]
    public void M(ReadOnlySpan<int> s) => Console.WriteLine("Span");

    public void M(int[] a) => Console.WriteLine("Array");   // priority 0 by default
}

int[] arr = [1, 2, 3];
new C1().M(arr);   // prints "Span"
```

### How the pruning works

After the applicable candidates are found, they are **grouped by declaring type**. Within each group they
are ordered by priority and every member with a lower priority than the group's highest is removed. The
groups are recombined and the normal "better function member" rules run on what is left.

Consequences you must design around:

- A **base type can never outrank a derived type**: a `Derived` candidate beats a high-priority `Base`
  candidate because pruning happens per declaring type, at the very end.
- Extension methods are grouped **per declaring static class**, so `Ext1`'s priority-1 method does not
  outrank `Ext2`'s priority-0 method.
- Priority is read from the **least-derived declaration**. It is not inherited, not re-specifiable on an
  override, and not inferred from an interface member it implements.
- Values default to 0; negatives are allowed and useful for demoting an obsolete overload below every
  default one.
- The attribute applies to methods, constructors and **indexer properties only**. It is an error to apply
  it to a non-indexer property, any property/indexer/event accessor, a conversion operator, a lambda, a
  local function, a finalizer or a static constructor — and it is an error to apply it where it would be
  ignored, such as on an `override`. Metadata on those locations is simply ignored by C#.
- At LangVersion 12 and lower the attribute is **silently ignored**, so the same code can be ambiguous in
  a project pinned to an older language version.

`Obsolete(error: true)` plus a priority demotion is the intended replacement pattern: keep the old
overload so existing binaries keep working, and make new source always pick the new one.

### Callability caveat

A low-priority overload can become **effectively uncallable from source**. `c.M1(i)` will call the
priority-1 `M1(long)` even though `M1(int)` has an identity conversion. Escape hatches: take a delegate
to the method (fails for reference-type variance cases and for `[Conditional]` methods), `UnsafeAccessor`,
reflection, or code that was never recompiled.

### Mistakes to avoid

- Do not use it to hide an overload you intend to delete — the demoted member stays callable from
  binaries, analyzers and reflection.
- Do not attach it to an `override`, an accessor or a plain property (compile-time error).
- Do not expect it to beat a derived type's member or another static class's extension method.
- Do not use it as a substitute for better parameter types: it prunes candidates before betterness, so it
  can hide a genuinely better match from a caller.
- Remember it is invisible below C# 13, which can make a "fixed" API ambiguous again.

---

## 11. The `field` keyword (preview in C# 13)

`field` names the compiler-synthesized backing field inside a property accessor, so an accessor can be
written without declaring a backing field. In C# 13 this is a **preview** feature (Visual Studio 2022
17.12, `LangVersion preview`); in C# 14 it is a normal feature.

```csharp
private string _name
{
    get => field;
    set => field = Normalize(value);
}
```

The only C# 13-specific hazard is the shadowing rule: `field` is a contextual keyword inside a property
accessor, so a type that already has a member named `field` stops referring to it there. Use `@field` or
`this.field` to reach the existing symbol.

**Do not implement this feature from this section.** Use the `csharp-14` skill, which has the complete
rules (valid accessor kinds, what `field` may not be used in, diagnostics and the backing-field
diagnostics). Reading this section is enough to recognise the preview syntax and to flag it in review as
"C# 13 preview — prefer the stable C# 14 form".

---

## 12. Better collection conversion and other compiler breaking changes in the .NET 9 SDK

**Better conversion from a collection expression.** C# 13 replaces the C# 12 tie-break with rules that
match `params` behavior. A collection conversion is better when the collection types are comparable
(`List<T>` over `IEnumerable<T>`, `ReadOnlySpan<T>` over `Span<T>`, a span over an array) **and** the
element conversions to that type are at least as good everywhere and better somewhere. If the element
conversions point the other way, the call is ambiguous — for example `ReadOnlySpan<object>` versus
`Span<string>` for `["", ""]` is resolved by the element conversions, and `M(ReadOnlySpan<int>)` versus
`M(Span<int?>)` for `M([])` is ambiguous because neither element type wins.

Watch for these when moving a codebase to C# 13:

- `System.Runtime.CompilerServices.InlineArray` on a `record struct` is now an error (CS9259). Use a plain
  `struct`.
- Iterators now introduce a **safe** context (a Roslyn spec violation is fixed). An `unsafe class`
  containing an iterator no longer propagates unsafe context into nested local functions:

  ```csharp
  unsafe class C
  {
      IEnumerable<int> M()
      {
          yield return 1;
          void Local() { int* p = null; }   // allowed in C# 12, error in C# 13
      }
  }
  ```

  Fix it by marking the local function `unsafe`. Lambdas are unaffected, and this only applies at
  LangVersion 13+.
- An empty collection expression no longer tie-breaks on span-ness: `M(ReadOnlySpan<int>)` /
  `M(Span<object>)` with `M([])` compiled in C# 12 and is an **error** in C# 13.
- An **exact element type** now beats everything: `M1([1])` picks `Span<int>` over `ReadOnlySpan<byte>`,
  and `M2([$"{1}"])` picks `Span<CustomInterpolatedStringHandler>` over `ReadOnlySpan<string>`
  (a constant interpolated string still prefers `string`). Behavior changes without any source change.
- Declaring an indexer without a proper `DefaultMemberAttribute` declaration is an error (CS0656), for
  example an interface indexer in some shapes:

  ```csharp
  public interface I1
  {
      public I1 this[I1 args] { get; }   // error CS0656
  }
  ```

- Method group inference now considers default and `params` parameters, turning previously
  order-dependent inference into an ambiguity error or CS8917 (section 4).
- `params` collections can break **consumers in other languages**: a non-array `params` parameter carries
  `ParamCollectionAttribute`, so Visual Basic callers see a plain collection parameter (section 1).
- The `dotnet_style_require_accessibility_modifiers` analyzer now applies to interface members too.
  `always` requires the redundant `public` on interface members, `for_non_interface_members` forbids it
  there, `omit_if_default` forbids redundant modifiers everywhere, `never` disables the analysis.

---

## 13. Agent checklist

- [ ] Confirm the project is on C# 13 (`net9.0`, or an explicit `LangVersion` of 13). Don't add `latest`,
      and remember `OverloadResolutionPriorityAttribute` and non-array `params` are invisible below 13.
- [ ] Add `params ReadOnlySpan<T>` (prefer `ReadOnlySpan` over `Span`) instead of a `params T[]` shim that
      allocates and forwards, and expect the new overload to win existing call sites on recompile.
- [ ] Keep one collection type per method, or prioritize with `[OverloadResolutionPriority]`; never ship
      two `params` overloads that differ only by element type.
- [ ] Check whether Visual Basic or another .NET language has to consume the `params` API before using a
      non-array collection.
- [ ] Switch synchronization to `System.Threading.Lock` by changing the field type only, and keep the
      `Lock` out of `object`-typed slots, generic type arguments and `async` critical sections.
- [ ] Use `'\e'`/`"\e"` instead of `"\x1b"` whenever the following character could be a hex digit.
- [ ] When a method group fails to infer, name the delegate type explicitly rather than reordering
      overloads.
- [ ] Use `[^1]` in object initializers against the collection instance that already exists; give up if
      the getter returns a copy.
- [ ] In iterators and async methods, declare `ref`/`ref struct` locals per segment and never read them
      after an `await` or `yield`.
- [ ] For a `ref struct`, express abstraction through a type parameter with `allows ref struct`, implement
      every interface member including DIMs, and keep `IDisposable`/`IEnumerable<T>` in the constraint
      list so `using` and `foreach` find them.
- [ ] Split generated or cross-cutting property logic into a defining and an implementing partial
      property with identical accessor modifiers and parameter names; use `field` for the trivial body.
- [ ] Use `OverloadResolutionPriorityAttribute` to steer new source to a better overload while keeping
      compiled binaries working, and never on an override, accessor or plain property.
- [ ] After a C# 12 → 13 upgrade, audit for changed overload resolution: empty collection expressions,
      exact-element-type matches, `params` array-vs-span, and method groups assigned to `var`.
- [ ] Treat `field` as C# 13 preview only — implement it with the `csharp-14` skill, and don't reach for
      C# 14/15 features (`partial` events and constructors, extension blocks, unions, `closed`) here.

## Diagnostic quick reference

| Diagnostic | Meaning in C# 13 context |
| --- | --- |
| CS9259 | `InlineArray` attribute applied to a `record struct`. |
| CS0656 | Missing compiler-required member — `DefaultMemberAttribute..ctor` for an indexer declaration. |
| CS8917 | "The delegate type could not be inferred" for a method group; use an explicit delegate type. |
| CS9035 | A `required` member is not initialized by the constructor that makes a type a valid `params` collection. |
| CS9256 | Partial property/indexer declarations have signature differences (warning). |
| CS1066 | A default value on the implementing part of a partial indexer has no effect. |
| CS1734 | XML comment `paramref` names a parameter that the partial declaration in that source does not have. |
| CS8618 | Non-nullable partial property is not assigned in the constructor (analysis uses the defining signature). |
| CS9507 | `foreach` cannot operate on an enumerator type parameter that allows ref struct and is not known to implement `IDisposable`. |
| CS0121 / CS0411 | Overloads made applicable (or ambiguous) by `params` collections or the new collection-expression betterness; or delegate inference failure. |
| "No overload for method takes N arguments" | The expanded form of a non-array `params` collection is not applicable: older LangVersion, an expression tree, `dynamic`, or a Visual Basic consumer. |
| CS8304 | Output of `#error version` — the compiler and language version in effect. |
| Warning on `object o = new Lock();` | A `System.Threading.Lock` is being upcast, so a later `lock` would use `Monitor`. |
| Error crossing `await`/`yield` with a `ref` or `ref struct` local | The local is definitely unassigned after the boundary; C# 13 does not demote this in `unsafe` code. |

## Sources

- What's new in C# 13: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-13
- C# 13 language versioning: https://learn.microsoft.com/dotnet/csharp/language-reference/configure-language-version
- Spec, parameter collections: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/params-collections.md
- Spec, better conversion from collection expression element: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/collection-expressions-better-conversion.md
- Spec, `Lock` object: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/lock-object.md
- Spec, `\e` escape sequence: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/esc-escape-sequence.md
- Spec, method group natural type improvements: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/method-group-natural-type-improvements.md
- Spec, ref and unsafe in iterators and async: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/ref-unsafe-in-iterators-async.md
- Spec, ref struct interfaces: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/ref-struct-interfaces.md
- Spec, partial properties: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/partial-properties.md
- Spec, overload resolution priority: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/overload-resolution-priority.md
- All C# 13.0 proposals: https://github.com/dotnet/csharplang/tree/main/proposals/csharp-13.0
- Generic constraints, `allows ref struct`: https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
- `ref struct` types and their restrictions: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/ref-struct
- Partial members: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/partial-member
- `lock` statement: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock
- `OverloadResolutionPriorityAttribute`: https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/general
- C# compiler breaking changes in .NET 9: https://learn.microsoft.com/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%209
- Roslyn language feature status: https://github.com/dotnet/roslyn/blob/main/docs/Language%20Feature%20Status.md
