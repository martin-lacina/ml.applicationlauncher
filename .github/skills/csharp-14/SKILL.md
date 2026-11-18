---
name: csharp-14
description: Write, review and modernize C# 14 (.NET 10) code correctly. Covers every C# 14 feature - extension members and blocks (`extension(T receiver) { }` with properties, static members and operators), `field` backed properties, first-class implicit span conversions (`T[]`/`string` to `Span<T>`/`ReadOnlySpan<T>`), `nameof(List<>)` unbound generics, modifiers on simple lambda parameters (`(text, out result) => ...`), `partial` events and constructors, user-defined compound assignment and instance increment (`void operator +=(int y)`, `void operator ++()`), null-conditional assignment (`customer?.Order = ...`), and the `#:` directives for file-based apps. Use when code targets net10.0 or LangVersion 14, when converting classic `this` extension methods to extension blocks, when deleting hand-written backing fields, when `array.Reverse()`, `Contains`, `Assert.Equal` or span/array overloads resolve differently, or when the build reports CS9258, CS9272, CS9269, CS9271, CS8908, CS8773, CS8170, an `ArrayTypeMismatchException` from a span overload, or an `extension`/`field`/`partial`/`scoped` identifier collides with a keyword.
---

# C# 14

C# 14 ships with the .NET 10 SDK and Visual Studio 2026. It is the default language version for
projects that target `net10.0`. Its features are:

| Feature | Keyword / syntax | Needs preview? |
| --- | --- | --- |
| Extension members | `extension<T>(IEnumerable<T> source) { }` blocks holding properties, methods, static members and operators | No |
| Field backed properties | `field` inside a property accessor | No |
| First-class span types | implicit `T[]` / `string` → `Span<T>` / `ReadOnlySpan<T>` conversions | No |
| Unbound generics in `nameof` | `nameof(List<>)`, `nameof(Dictionary<,>)` | No |
| Lambda parameters with modifiers | `(text, out result) => …`, `(ref int i) => …` | No |
| Partial events and constructors | `partial Widget(int size) : this(size) { }`, `partial event EventHandler? Touched;` | No |
| User-defined compound assignment | `void operator +=(int y)`, `void operator ++()` | No |
| Null-conditional assignment | `customer?.Order = …`, `arr?[42] = …` | No |
| Optional and named arguments in expression trees | named/optional call arguments inside `Expression<…>` | No |
| File-based app directives | `#:package`, `#:project`, `#:property`, `#:sdk`, `#:include`, `#!` shebang | No (SDK feature) |

Extension **indexers**, union types, `closed` hierarchies, `with(...)` collection arguments, labeled
`break`/`continue` and the memory-safety model are **C# 15**, not C# 14. Use the `csharp-15` skill for
those. In C# 14 an `extension` block carries methods, properties and operators only.

Everything from **C# 13** stays available in C# 14, and in C# 15: `params` collections,
`System.Threading.Lock`, `\e`, `allows ref struct`, `ref struct` interfaces, `ref`/`unsafe` in iterators and
`async`, partial properties and indexers, `OverloadResolutionPriorityAttribute` and method group natural
type. Use the `csharp-13` skill for their rules — this skill adds what C# 14 changed on top of them (the
`field` keyword graduated from C# 13 preview to a stable feature here, `partial` extended to events and
constructors, and the implicit span conversions in section 3 build directly on the C# 13
`params`-collection and collection-expression overload rules).

Read section 0 first, then the section for the feature you are using. Each section ends with the
mistakes to avoid.

---

## 0. Before using any C# 14 feature

1. **Check the language version.** Look at the project's `TargetFramework` and `LangVersion`.
   `net10.0` without a `LangVersion` gives C# 14. To see the active version, temporarily add
   `#error version`: the compiler reports CS8304 with the compiler and language versions.
2. **Don't set `<LangVersion>latest</LangVersion>`.** Microsoft warns against it because it varies by
   machine. Using a language version newer than the target framework's default is unsupported. Prefer
   moving the TFM to `net10.0`; C# 14 features on an older TFM are exactly the unsupported
   configuration that produces the `Enumerable.Reverse` break in section 3.
3. **Know the new contextual keywords.** They can break existing identifiers, and `@` escapes them
   (details in section 10):
   - `extension` in declaration position: a type, alias or type parameter may **not** be named
     `extension` at all; `@extension` only rescues member names.
   - `field` inside a property accessor: use `@field` or `this.field` to reach an existing symbol.
   - `scoped` in a lambda parameter list is always a modifier: use `@scoped` for a type named `scoped`.
   - `partial` in member-declaration position: `@partial F()` if `partial` was your type name.
4. **C# 14 changes overload resolution.** The implicit span conversions in section 3 are the biggest
   behavioral change of the release: they make `Span<T>`/`ReadOnlySpan<T>` overloads applicable where
   a `T[]` or `IEnumerable<T>` overload used to win. Audit an existing codebase for that first.
5. **Both extension syntaxes emit the same IL.** `this`-modifier extension methods and `extension`
   blocks are source **and** binary compatible, so converting existing code is a pure refactor with no
   effect on assemblies already compiled against it.

---

## 1. Extension members: `extension` blocks

C# 14 generalizes extension methods into **extension blocks**: one receiver declaration holding several
members, including extension **properties**, **static** members that appear on the extended type, and
**user-defined operators**.

```csharp
public static class NumericSequences
{
    // instance extension members: called as if they were members of IEnumerable<int>
    extension(IEnumerable<int> source)
    {
        public bool IsEmpty => !source.Any();                                        // extension property
        public IEnumerable<int> AddValue(int operand)                                // extension method
        {
            foreach (var item in source) yield return item + operand;
        }

        // static user-defined operator, used as `left + right` on the extended type
        public static IEnumerable<int> operator +(IEnumerable<int> left, IEnumerable<int> right) => left.Concat(right);
    }

    // static extension members only: the receiver parameter name is optional
    extension(IEnumerable<int>)
    {
        public static IEnumerable<int> Generate(int low, int count, int increment) { … }
        public static IEnumerable<int> Identity => Enumerable.Empty<int>();
    }
}

IEnumerable<int> numbers = Enumerable.Range(1, 10);
bool none = numbers.IsEmpty;                        // instance syntax
var combined = numbers + Enumerable.Range(100, 10); // extension operator
var generated = IEnumerable<int>.Generate(5, 10, 2); // static syntax: `IEnumerable<int>.Identity` too
```

### Declaration rules

- Extensions live **only** in a **top-level, non-generic, `static` class**, exactly like classic
  extension methods, and may coexist with them and with ordinary static members in the same class.
- The receiver type may be non-generic, an open generic (`IEnumerable<T>`) or a closed generic, may
  carry nullable annotations, attributes and constraints:
  `extension<T>(IEnumerable<T> source) where T : IEquatable<T> { … }`.
- **Type parameters go on the block** when they appear in the receiver, and **on the member** when they
  are extra. Never declare the same type parameter in both places.
- An **instance** member requires a **named** receiver parameter. A block declaring only static members
  may drop the name (`extension(IEnumerable<int>)`).
- An unnamed receiver may carry **no** modifiers. A named receiver may carry `ref`, `ref readonly`, `in`
  (only when the receiver type is known to be a value type) and `scoped`. By-value and by-ref receivers
  need **separate blocks**.

  ```csharp
  public static class AccountExtensions
  {
      extension(ref Account account)
      {
          public void Deposit(float amount) => account.Balance += amount; // private members are still inaccessible
      }
  }
  ```

- **Forbidden on a block member:** `abstract`, `virtual`, `override`, `new`, `sealed`, `partial`,
  `protected` (and related accessibility), and `readonly`. Properties in a block may not have `init`
  accessors, members may not carry `[ModuleInitializer]`, and block members are not entry-point candidates.
- A **static** member may not use the receiver parameter (except inside `nameof`). Member parameters,
  local variables, local functions and member type parameters may not reuse the receiver name or a block
  type-parameter name.
- Every member name must differ from the enclosing static class name and from the extended type name.
- **Uniqueness:** all blocks in one static class with the same receiver type (modulo identity conversion
  and type-parameter names) form a single declaration space, and classic `this` extension methods take
  part in it. `IsEmpty` on `IEnumerable<T>` and on `IEnumerable<T>?` in the same class is a duplicate.

### Generic type parameters

```csharp
public static class GenericExtensions
{
    extension<TReceiver>(IEnumerable<TReceiver> source)
    {
        public IEnumerable<TReceiver> Spread(int start, int count) => source.Skip(start).Take(count);

        // the extra type parameter belongs to the member, not the block
        public IEnumerable<TReceiver> Append<TArg>(IEnumerable<TArg> second, Func<TArg, TReceiver> converter)
            => source.Concat(second.Select(converter));

        public static IEnumerable<TReceiver> Identity => Enumerable.Empty<TReceiver>();
    }
}
```

For each **non-method** member, every type parameter of the block must be inferrable from the combined
block-plus-member parameter list. `extension<T1>(IEnumerable<int>) { … }` is an error: `T1` can never be inferred.

### Lookup and priority

- Extension members are in scope only when the namespace of their static class is `using`-imported. A
  `using static N.E;` directive also makes block members available for extension-style access
  (`new object().M();`, `object.M2();`, `c += 1;`).
- Instance and static members **of the extended type always win**. An extension can never override or
  shadow a real member with a matching signature: the compiler binds to the type's own member.
- Lookup performs full type inference across the block's *and* the member's type parameters, and explicit
  type arguments substitute both sets: `strings.Select<string, int>(s => s.Length)`.
- Two blocks in different classes with the same name and arity are ambiguous at the receiver; call the
  generated implementation method to disambiguate — `E1.M2(receiver)`, and for a property `E1.get_P(receiver)`.
  `using static` does **not** import instance extension members as static methods, so `M(1)` on
  `extension(int i) { public void M() { } }` does not compile.
- `OverloadResolutionPriorityAttribute` is applied with the enclosing static class as the containing
  type, and an attribute on an extension property is copied onto its accessor implementation methods.

### Operators

Operators are declared **inside** the block even though their operand types are explicit; that is what
lets the block's type parameters be inferred, just as a normal operator must sit in one of its operand types.

```csharp
extension<TElement>(IEnumerable<TElement>) where TElement : INumber<TElement>
{
    public static IEnumerable<TElement> operator *(IEnumerable<TElement> vector, TElement scalar) { … }
    public static IEnumerable<TElement> operator *(TElement scalar, IEnumerable<TElement> vector) { … }
}
```

Instance compound-assignment operators also work as extension members (`public void operator +=(int i)`, see section 7).

### What it lowers to

An instance member becomes a static implementation method on the enclosing class with the receiver as an
extra first parameter — byte-for-byte the shape a classic extension method produces; a static member keeps
its signature. The block itself is encoded as unspeakable compiler-generated grouping and marker types
carrying `[Extension]` and `[ExtensionMarkerName]`: never reference those names in code, docs or tooling.
Write the block's `/// <summary>`, `<typeparamref>` and `<paramref>` on the `extension` line — the compiler
copies them onto members through `<inheritdoc>`, and members must not re-declare the receiver with `<param>`.

```csharp
/// <summary>Extensions for numeric sequences.</summary>
public static class NumericSequences
{
    /// <summary>Extensions for integer sequences.</summary>
    /// <param name="source">The sequence used as a receiver.</param>
    extension(IEnumerable<int> source)
    {
        /// <summary>Adds <paramref name="operand"/> to each element.</summary>
        public IEnumerable<int> AddValue(int operand) { … }
    }
}
```

### Mistakes to avoid

- Don't declare a type, alias or type parameter named `extension`: C# 14 forbids it outright.
- Don't put `virtual`, `protected`, `sealed`, `partial`, `readonly` or an `init` accessor on a block member.
- Don't name a member the same as the enclosing static class or the extended type.
- Don't declare an instance member in a block that has no receiver name.
- Don't declare a block type parameter that no member can infer from its arguments.
- Don't expect an extension to override an existing member — the type's own member always wins.
- Don't mix by-value and by-ref receiver members in one block; write two blocks.
- Don't refer to the receiver from a static block member, and don't reuse the receiver or block
  type-parameter names for member parameters or locals.
- Don't duplicate a member that a classic `this` extension method in the same class already declares.

---


## 2. The `field` keyword

Inside a property accessor, `field` refers to a compiler-synthesized backing field. Give a body to one
accessor and let the compiler generate the other, exactly as it would for an auto-property.

```csharp
// before: a hand-written field plus both accessor bodies
private string _msg;
public string Message
{
    get => _msg;
    set => _msg = value ?? throw new ArgumentNullException(nameof(value));
}

// after
public string Message
{
    get;                                                        // accessor generated
    set => field = value ?? throw new ArgumentNullException(nameof(value));
}

public double Hours
{
    get;
    set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
}
```

### The patterns it replaces

```csharp
// lazy initialization
public SqlConnection Connection => field ??= new SqlConnection("connection-string");

// change notification
public string DisplayName
{
    get;
    set
    {
        if (field == value) return;
        field = value;
        OnPropertyChanged();
    }
}

// normalized input
public string Email
{
    get;
    set => field = value?.Trim().ToLowerInvariant() ?? string.Empty;
}
```

### Rules

- You may give a body to one accessor or to both; a bodyless accessor (`get;`, `set;`, `init;`) is generated
  as it would be for an auto-property. Mixing the two is the point of the feature.
- An expression-bodied or get-only property can use `field` (`public string Lazily => field ??= Compute();`),
  and so can a **set-only** property.
- `field` is a keyword only as a *primary expression* inside `get`/`set`/`init` accessor bodies, inside
  attributes on those accessors, and inside lambdas, local functions and LINQ queries nested in them.
  `field` can be captured by those lambdas and local functions.
- **Not available** in: indexer accessors, event accessors (no backing field is generated for an event),
  ref-returning properties, or the property initializer expression.
- A **property initializer** writes the backing field directly and does **not** call the setter. Assign in
  the constructor when you *want* the setter to run (change notifications, validation).

  ```csharp
  public bool HasPendingChanges { get; private set; }
  public bool IsActive { get; set => Set(ref field, value); } = true;   // initializer: no change flag raised
  ```

- Assignment in a constructor calls the (possibly virtual) setter; if there is no setter, it writes the
  backing field directly, like a read-only auto-property.
- Field-targeted attributes are allowed on any property whose accessors use a backing field:
  `[field: Nullable] public string Name => field ??= Compute();`. They stay invalid if no accessor uses `field`.
- In a `struct`, the backing field is `readonly` (`initonly` in metadata) when the property or the containing
  struct is declared `readonly`; writing `field` elsewhere is then an error.
- `partial` properties: either part may carry the initializer (not both), and the implementing part must
  implement at least one accessor manually — the other may be auto-implemented.
- An overriding property may use `field`, which refers to **its own** backing field, never the base
  property's. Such an override must override **all** accessors.
- Nullability is tracked like a field: a null-resilient getter such as `field ??= Compute()` needs no
  constructor initialization warning, while a plain `get => field` makes the property subject to normal
  constructor and setter initialization warnings.
- `field` is contextual. In a type that already has a symbol named `field`, the accessor's `field` now binds
  to the synthesized backing field and warns **CS9258**; write `this.field` or `@field`, or rename the member.

### Mistakes to avoid

- Don't declare a local, a parameter, or a nested local function's parameter named `field` inside an
  accessor: **CS9272**. Rename it or declare it as `@field`.
- Don't write `nameof(field)`: the backing field is unnamed, so it does not compile. Use `nameof(Prop)` or
  `nameof(value)` (which is what the libraries use when a setter throws).
- Don't combine a hand-written backing field with `field` for the same property — pick one.
- Don't assume the synthesized field is shared between properties, or reachable from another member; it is
  addressable only from that property's accessors.
- Don't put `field` in an indexer or event accessor, or expect it in a ref-returning property.
- Don't expect a property initializer to run your setter — it writes the field directly.
- Watch the unused-field warnings on the synthesized field: **CS0414** (assigned but never read) and
  **CS0649** (never assigned). They mean the accessor pair is wrong, not that a field is missing.
- **Good refactor:** a property whose only purpose is validation, lazy creation, normalization or change
  notification no longer needs `private T _name;` plus two accessor bodies.

---


## 3. Implicit span conversions (`Span<T>`, `ReadOnlySpan<T>`, arrays, `string`)

C# 14 makes spans first-class by adding **implicit span conversions** that are *standard implicit*
conversions — so overload resolution, extension receivers and generic type inference all see them.

```csharp
int[] arr = [1, 2, 3];
bool starts = arr.StartsWith(1);        // MemoryExtensions(ReadOnlySpan<T>) overload; previously CS8773
ReadOnlySpan<char> text = "hello";      // no AsSpan() call needed
ReadOnlySpan<object> any = new string[] { "a" }; // covariant element conversion
```

### The conversions

| From | To | Condition |
| --- | --- | --- |
| single-dimensional `E[]` | `Span<E>` | identity |
| single-dimensional `E[]` | `ReadOnlySpan<U>` | `E` is covariantly or identically convertible to `U` |
| `Span<E>` | `ReadOnlySpan<U>` | `E` is covariantly or identically convertible to `U` |
| `ReadOnlySpan<E>` | `ReadOnlySpan<U>` | `E` is covariantly or identically convertible to `U` |
| `string` | `ReadOnlySpan<char>` | always |

Explicit span conversions are the same list plus `E[]` → `Span<U>` / `ReadOnlySpan<U>` when an explicit
reference conversion exists from `E` to `U`. There is **no** implicit `Span<T>` → `T[]` and **no**
`ReadOnlySpan<T>` → `Span<T>`.

### Rules

- The types match by fully qualified name (`System.Span<T>`, `System.ReadOnlySpan<T>`), not by any
  user-defined `ref struct`.
- **User-defined conversion operators are ignored** between types for which a language-defined span
  conversion exists.
- Span conversions apply to **extension method receivers**, but **not** in method group conversions: so
  `Action<int> a = new int[0].M;` still binds to the `IEnumerable<T>` extension instead of failing with CS1113.
- Betterness: an implicit span conversion beats a non-span conversion from the same expression, and
  `ReadOnlySpan<T>` is a better conversion target than `Span<T>`. That is what keeps most existing call
  sites unambiguous and steers you to the read-only overload.
- Generic inference now works through spans: passing `int[]` to `ReadOnlySpan<T>` infers `T = int`.
- Patterns on `ref struct` types allow only identity conversions — you cannot write `if (x is ReadOnlySpan<object>)`.

### Mistakes to avoid

- `x.Reverse()` on an array now binds to `MemoryExtensions.Reverse(Span<T>)`, which reverses **in place**
  and returns `void`. On `net10.0` the BCL added `Reverse<T>(this T[])`, so the break only bites C# 14 on
  an earlier TFM (or .NET Framework with `System.Memory`). Write `Enumerable.Reverse(x)` explicitly.
- Newly applicable span overloads create **CS0121** ambiguity (`Assert.Equal([2], x)`,
  `Assert.Equal(x, arraySegment)`). Work around it with `.AsSpan()`; as an API author use
  `OverloadResolutionPriorityAttribute` (also ignored on LangVersion ≤ 13).
- A now-preferred `Span<T>` parameter throws **ArrayTypeMismatchException** for a covariant array
  (`string[]` assigned to `object[]`). Pass `.AsEnumerable()`, or declare `ReadOnlySpan<T>` instead.
- When both a `Span` and a `ReadOnlySpan` overload exist, the `ReadOnlySpan` one wins and can break code
  that needs a `Span` result: `MemoryMarshal.Cast<double, ulong>(x)` → pass `x.AsSpan()`.
- API authors: adding a `ReadOnlySpan<T>` overload next to an `IEnumerable<T>` one fixes resolution for
  C# 14 users but breaks users pinned at LangVersion ≤ 13.
- Expression trees and LINQ translators: `array.Contains(x)` now binds to `MemoryExtensions.Contains`, and
  ref structs cannot be interpreted (`exp.Compile(preferInterpretation: true)` fails) or translated. Write
  `Enumerable.Contains(array, x)` where the tree must stay interpretable or translatable.

---

## 4. `nameof` with unbound generic types

```csharp
string a = nameof(List<>);          // "List"
string b = nameof(Dictionary<,>);   // "Dictionary"

[Obsolete(nameof(IReadOnlyList<>))] // attributes no longer need a made-up string
public sealed class MyList<T> : IReadOnlyList<T> { … }
```

Write the arity with commas. The result never includes the arity or type arguments: `nameof(List<int>)`
and `nameof(List<>)` are both `"List"`.

### Mistakes to avoid

- Don't expect arguments — an unbound `nameof` gives the type name only.
- Don't use it for a type you cannot name in scope; the argument must still be a resolvable type or member.

---

## 5. Simple lambda parameters with modifiers

Modifiers on lambda parameters no longer require writing the parameter types:

```csharp
delegate bool TryParse<T>(string text, out T result);
TryParse<int> parse1 = (text, out result) => int.TryParse(text, out result);   // allowed in C# 14
TryParse<int> parse2 = (string text, out int result) => int.TryParse(text, out result); // still valid

delegate void Incrementer(ref int counter);
Incrementer increment = (ref int counter) => counter++;   // scoped, ref, in, out, ref readonly all allowed
```

The `params` modifier still requires an explicitly typed parameter list.

### Mistakes to avoid

- `scoped` is **always** parsed as a modifier in a lambda parameter list. If you have a type named
  `scoped`, `(scoped scoped s) => { }` is an error; write `(scoped @scoped s) => { }`.
- If a call becomes ambiguous (CS0121) after upgrading, add explicit lambda parameter types:
  `M((int x) => { })`.
- Don't add modifiers when the delegate type can't be inferred — modifiers don't help inference, the
  types don't have to be written but the target delegate must still be known.

---


## 6. Partial events and constructors

```csharp
// exactly one *defining* declaration
public partial class Widget
{
    public partial Widget(int size);
    public partial event EventHandler? Touched;
}

// ... and exactly one *implementing* declaration
public partial class Widget
{
    public partial Widget(int size) : this(size, DateTime.UtcNow) { }   // initializer allowed only here

    public partial event EventHandler? Touched
    {
        add { touched += value; }
        remove { touched -= value; }
    }
}
```

### Rules

- Partial constructors and partial events need **exactly one** defining and one implementing declaration.
- Only the **implementing** constructor declaration may carry a `this(…)` or `base(…)` initializer.
- Only **one** partial type declaration may use primary-constructor syntax.
- The implementing event declaration provides `add` and `remove`; the defining declaration is the field-like form.

### Mistakes to avoid

- Don't put `: base(…)` or `: this(…)` on the defining constructor declaration.
- Don't declare more than two parts of the same partial constructor or event.
- Because `partial` is a modifier in more positions, it can no longer be a **return type**:
  `partial F() => new partial();` breaks; write `@partial F() => new partial();`.
- Behavior fix to know about: `partial` interface **properties and events** are now implicitly `virtual`
  and `public`, matching their non-partial equivalents (partial interface *methods* deliberately keep the
  old behavior). Pin the old behavior by marking them `private`, or `sealed` where they were not `virtual`.

---

## 7. User-defined compound assignment and increment operators

`x += y` used to desugar to `x = x + y`: build a new value, copy it, reassign. C# 14 lets a type handle
the operation **in place** with instance operators, which matters for allocation-heavy types
(`BigInteger`, tensors, matrices, buffers).

```csharp
public class Matrix
{
    public static Matrix operator +(Matrix a, Matrix b) { … }        // the static binary form, unchanged
    public void operator +=(Matrix other) { /* mutate this in place, allocate nothing */ }
    public void operator ++() { /* mutate this in place */ }
}

Matrix total = baseMatrix;
total += delta;   // calls total.op_AdditionAssignment(delta)
```

### Declaration forms

| Form | Signature | Notes |
| --- | --- | --- |
| Compound assignment | `void operator +=(T2 y)` | **instance**, one parameter, `void` return |
| Instance increment | `void operator ++()` | **instance**, no parameters, `void` return |
| Static increment | `T operator ++(T x)` | the classic unary form, still available |

Overloadable compound tokens: `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`, `<<=`, `>>=`, `>>>=`, and the
`checked` variants, which must be declared in pairs with their unchecked counterpart.

### Resolution rules

- When the target is a **variable** and a new language version is targeted, the **instance** compound
  assignment / increment operator is tried first; if nothing applies, the old binary/unary operator rules run.
- `static` is **not** allowed on a compound assignment or instance increment declaration. `override` and
  `new` **are** allowed now — these operators can override or hide base-class operators, and shadowing
  follows the ordinary method rules. `readonly` is allowed on a struct member even though it mutates the instance.
- Reference type: the operator runs on the instance and that same instance is the result (`null` target → `NullReferenceException`).
- Value type: when the **result is used**, the compiler copies `x` into a temp, runs the operator on the
  temp, assigns it back to `x` and yields the temp; when the result is unused, it runs on `x` directly.
  Side effects in `x` are evaluated once.
- Metadata names are `op_AdditionAssignment`, `op_SubtractionAssignment`, … and `op_IncrementAssignment`,
  `op_DecrementAssignment` (plus `Checked` variants). They are void instance methods, not the classic
  two-parameter static operators — CLS consumers should not rely on them.

### Mistakes to avoid

- **There is no consistency check between `+` and `+=`.** They can now genuinely differ: `+=` mutates,
  `+` returns a new object. Document the type's behavior, and keep both forms if consumers use both.
- Don't expect mutation through a property without a setter: `C.P1 += 1` on a get-only property is an
  error, and for a settable property the copy-modify-assign sequence applies.
- Don't declare only the `checked` variant — pair-wise declaration is required.
- Because the target instance can be mutated in place, `b = a += y` may now change `a` where it previously
  left it alone. Re-check aliasing assumptions in the type's consumers.
- Don't add instance operators to an immutable value type just for syntax; the copy-back sequence keeps
  the allocations and adds a mutation contract.

---

## 8. Null-conditional assignment

`?.` and `?[]` may now appear on the left of an assignment or a compound assignment:

```csharp
// before
if (customer is not null)
{
    customer.Order = GetCurrentOrder();
}

// after — GetCurrentOrder() is called only when customer is not null
customer?.Order = GetCurrentOrder();

settings?.Timeout = 30;
arr?[42] = new object();
listener?.Signal += OnSignal;      // UI event wiring without a null check
counters?.Total += 1;
stats?.Count ??= 0;
```

### Rules

- `P?.A = B` means `if (P is not null) P.A = B;` with `P` evaluated **once**. `P?[A] = B` is the same shape.
- As an expression its result type is the nullable result of the assignment:
  `P?.A = B` ≡ `(P is null) ? (T?)null : (P.A = B)`, so `return c?.M() = 42;` produces `int?`.
- All compound forms work, including `??=`.
- Nested conditional assignment guards each receiver in turn: `a?.b?.c = d` guards `a` and `a.b`.

### Mistakes to avoid

- `a?.b++`, `++a?.b`, `--a?.b` and `a?.b--` are **not** supported. Keep an explicit null check.
- It does not work on a **value-type** receiver: `myStruct?.X = 1` is an error (non-nullable), and
  `myNullableStruct?.X = 1` has no defined meaning because `Value` is not a variable.
- Conditional accesses are still not lvalues: `M(ref a?.b)` and `a?.b = ref x` are errors, and you cannot
  assign through deconstruction: `(a?.b, c?.d) = (x, y);`.
- Don't rely on the right-hand side for side effects — it is skipped entirely when the receiver is null.
- `a?.b.c = d` guards only `a`; add `?.` at each level you actually need to guard.

---

## 9. File-based apps: `#:` directives

`dotnet run app.cs` builds and runs a single file with no `.csproj`. The **compiler ignores** any directive
starting with `#:` or `#!`; the SDK parses them. In a project-based compilation these directives produce
warnings — keep them in file-based apps.

```csharp
#!/usr/bin/env -S dotnet --
#:sdk Microsoft.NET.Sdk.Web
#:package Serilog@3.1.1
#:package Spectre.Console@*
#:project ../SharedLibrary/SharedLibrary.csproj
#:include models/customer.cs
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property LogLevel=$([MSBuild]::ValueOrDefault('$(LOG_LEVEL)', 'Information'))

Console.WriteLine("hello");
```

| Directive | Purpose |
| --- | --- |
| `#:sdk` | SDK id; defaults to `Microsoft.NET.Sdk` (`#:sdk Microsoft.NET.Sdk.Web`, `#:sdk Aspire.AppHost.Sdk@13.0.2`) |
| `#:package` | NuGet reference: `Name`, `Name@version`, `Name@*` for latest |
| `#:project` | Project reference; managed by `dotnet reference add --file app.cs …` |
| `#:property` | Any MSBuild property, MSBuild property functions allowed |
| `#:include` | Extra files by extension (`*.cs` → Compile, `*.resx` → EmbeddedResource, `*.json` → None, `*.razor` → Content, `*.dll` → Reference), glob patterns allowed |
| `#!` | Unix shebang; needs `chmod +x`, `LF` line endings and no BOM |

Defaults worth knowing: **native AOT is on** (`#:property PublishAot=false` disables it), `PackAsTool=true`
is on, launch profiles go in `app.run.json` next to the file, parent `Directory.Build.props`,
`Directory.Build.targets`, `Directory.Packages.props`, `nuget.config` and `global.json` all apply, and
build output is cached per file content (`dotnet clean file-based-apps` clears the cache).

### Mistakes to avoid

- Omitting a package version only works with central package management (`Directory.Packages.props`);
  otherwise write `@version` or `@*`.
- Don't put a file-based app inside a `.csproj` project's folder tree — the project's implicit build files
  leak into it. Give file-based apps their own directory.
- Don't rely on `#:` directives in project-based compilation: the compiler ignores them and the build warns.
- Don't run the same file-based app concurrently without building first (`dotnet build app.cs`, then
  `dotnet run app.cs --no-build`): two processes contending over the cached output errors.
- `dotnet run file.cs` **without** `--file` runs the local project and passes the file as an argument when
  a project file exists in the current directory.
- Included `*.cs` files may add declarations but **not** top-level statements.

---

## 10. Expression-tree arguments and other compiler changes in the .NET 10 SDK

**Optional and named arguments now work in expression trees**, so a LINQ-to-objects or provider-side tree
can express what previously required a positional workaround:

```csharp
Expression<Func<string, bool>> e = s => s.Contains(value: "net", comparisonType: StringComparison.OrdinalIgnoreCase);
Expression<Func<int, int>> shifted = x => Shift(x, amount: 2);   // optional parameter omitted in the tree
```

Watch for these breaking changes when moving a codebase to C# 14:

- `scoped` is always a lambda parameter modifier; a type named `scoped` needs `@scoped` in the type position.
- `Span<T>`/`ReadOnlySpan<T>` overloads apply in more scenarios: a different best overload, or a new CS0121
  ambiguity. Also `ReadOnlySpan` is now preferred over `Span`. See section 3.
- On C# 14 with a pre-`net10.0` TFM, `array.Reverse()` resolves to `MemoryExtensions.Reverse(Span<T>)`.
- A local or nested-function parameter named `field` in a property accessor is **CS9272**; an existing member
  named `field` binds to the synthesized backing field with warning **CS9258**.
- `extension` is a contextual keyword and may not be used as a type, alias or type parameter name at all;
  `extension M() { }` inside `class @extension` does not compile.
- `partial` can no longer be a method return type; escape it as `@partial`.
- `partial` interface properties and events are implicitly `virtual` and `public` (partial interface *methods* unchanged).
- Diagnostics are now reported for pattern-based `foreach` / `await foreach` disposal, e.g. an `[Obsolete]`
  `DisposeAsync()` on the enumerator.
- A disposed enumerator stops: `MoveNext()` returns `false` without executing further user code.
- New warning for a redundant pattern in a simple `or` pattern: `is not null or 42` warns that `42` is
  redundant — you likely meant `is not (null or 42)`.
- `[UnscopedRef]` has no effect under the old ref-safety rules (C# ≤ 10 with net ≤ 6.0): code that used to
  compile can now fail (CS8170) and the attribute draws CS9269.
- `Microsoft.CodeAnalysis.EmbeddedAttribute` declared in source is validated (internal sealed non-static
  class with an internal or public parameterless constructor inheriting `System.Attribute`): **CS9271**.
- `record` and `record struct` cannot declare pointer-type members even when they provide their own
  `Equals` implementation (**CS8908**).
- Metadata-only EXE emission (`/refonly`, `ProduceOnlyReferenceAssembly`) now requires an entry point; mark
  the compilation a DLL instead.
- A missing `ParamCollectionAttribute` is now an error for `params` lambdas or local functions when
  compiling a `.netmodule`; define the attribute yourself.

---

## 11. Agent checklist

- [ ] Confirm the project is on C# 14 (`net10.0`, or an explicit `LangVersion` of 14). Don't add `latest`.
- [ ] Convert `this` extension methods to `extension` blocks once a type needs more than one extension
      member, and express read-only helpers as extension **properties** rather than zero-argument methods.
- [ ] Keep block members free of `virtual`/`protected`/`sealed`/`partial`/`readonly`/`init`, and give every
      block with an instance member a named receiver.
- [ ] Replace validation, lazy-creation, normalization and change-notification backing fields with `field`;
      never mix `field` and a hand-written field in the same property.
- [ ] Prefer `ReadOnlySpan<T>` over `Span<T>` in new APIs: it is the better conversion target and avoids the
      covariant-array `ArrayTypeMismatchException`.
- [ ] After a C# 13 → 14 upgrade, audit for changed overload resolution: `Reverse`, `Contains`,
      `Assert.Equal`, `MemoryMarshal.Cast`, and LINQ-vs-`MemoryExtensions` calls inside expression trees.
- [ ] Use `nameof(List<>)` for unbound generic type names instead of string literals.
- [ ] Add instance `+=`/`++` operators only for mutable, allocation-heavy types; document the divergence from
      `+`, and declare `checked` variants in pairs.
- [ ] Use `?.` assignment for reference-type receivers only; keep explicit null checks for structs and never `a?.b++`.
- [ ] Keep `#:` directives in file-based apps only, and remember native AOT and `PackAsTool` are on by default there.
- [ ] Don't reach for extension indexers, unions or `closed` here — those are C# 15 (`csharp-15` skill).

## Diagnostic quick reference

| Diagnostic | Meaning in C# 14 context |
| --- | --- |
| CS9258 | `field` in a property accessor binds to the synthesized backing field instead of an existing member (use `this.field` or `@field`). |
| CS9272 | A local or nested-function parameter named `field` declared inside a property accessor. |
| CS0414 / CS0649 | The synthesized backing field is assigned but never read, or never assigned — the accessor pair around `field` is wrong. |
| CS8908 | Pointer-type member in a `record` or `record struct`. |
| CS9269 | `[UnscopedRef]` used where it has no effect (C# ≤ 10 with net ≤ 6.0). |
| CS8170 | Struct member returns `this` or an instance member by ref — surfaces after the `[UnscopedRef]` fix. |
| CS9271 | Malformed `Microsoft.CodeAnalysis.EmbeddedAttribute` declaration in source. |
| CS8773 | Feature not available in the current language version — typically a span conversion you only get in C# 14. |
| CS0121 / CS0411 | Newly applicable span/array overloads are ambiguous, or inference fails; use `.AsSpan()`, explicit type arguments or `OverloadResolutionPriorityAttribute`. |
| CS1113 | Extension method declared on a `ref struct` receiver used in a method group conversion (still avoided, because span conversions don't apply there). |
| Syntax errors on `extension …` | `extension` used as a type, alias, type parameter or member return type; duplicate extension member in the same static class; a forbidden modifier on a block member. |
| "pattern is redundant" warning | `is not null or 42` style `or` pattern where the second pattern can never matter. |
| `ArrayTypeMismatchException` | Run time: a covariant array reached a now-preferred `Span<T>` parameter. |
| `NotSupportedException` from an interpreted expression tree | A `MemoryExtensions` span call was preferred over the LINQ method; write `Enumerable.Contains(…)` explicitly. |

## Sources

- What's new in C# 14: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14
- Extension members (programming guide): https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
- `extension` keyword: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/extension
- Spec, extension members: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/extensions.md
- `field` keyword: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/field
- Spec, `field` keyword: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/field-keyword.md
- Built-in types (implicit span conversions): https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/built-in-types
- Spec, first-class span types: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-14.0/first-class-span-types
- Spec, unbound generic types in `nameof`: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/unbound-generic-types-in-nameof.md
- Lambda expressions (parameters with modifiers): https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions
- Spec, simple lambda parameters with modifiers: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/simple-lambda-parameters-with-modifiers.md
- Constructors (partial constructors): https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/constructors
- Spec, partial events and constructors: https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/partial-events-and-constructors.md
- Spec, user-defined compound assignment: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-14.0/user-defined-compound-assignment
- Conditional member access (`?.` assignment): https://learn.microsoft.com/dotnet/csharp/language-reference/operators/member-access-operators
- Spec, null-conditional assignment: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-14.0/null-conditional-assignment
- Preprocessor directives (`#:` file-based apps): https://learn.microsoft.com/dotnet/csharp/language-reference/preprocessor-directives
- File-based apps: https://learn.microsoft.com/dotnet/core/sdk/file-based-apps
- Compiler breaking changes in .NET 10: https://learn.microsoft.com/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2010
- Roslyn language feature status: https://github.com/dotnet/roslyn/blob/main/docs/Language%20Feature%20Status.md
