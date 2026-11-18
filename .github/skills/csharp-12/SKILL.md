---
name: csharp-12
description: Write, review and modernize C# 12 (.NET 8) code correctly. Covers every C# 12 feature - primary constructors on classes and structs, collection expressions and the spread element, inline arrays, optional and params lambda parameters, `ref readonly` parameters, alias any type, ExperimentalAttribute, and interceptors (preview). Use when code targets net8.0 or LangVersion 12, when converting a field-plus-constructor pair or a `new List<T>`/array initializer to a primary constructor or `[e1, e2]`, when a collection-expression call reports CS0121 ambiguity, CS9174 or CS9176, when an API migrating from `ref` or `in` to `ref readonly` must not break callers, when a method group with optional or `params` parameters stops converting to `Action`/`Func` (CS0407), or when the build reports CS9105, CS9109, CS9113, CS9124, CS9107, CS9179, CS8862, CS9192, CS9193, CS9191, CS9200, CS9132, CS9166 or an `EXPxxx` experimental diagnostic. C# 12 is the base of the modern series. I.e. every feature here stays available in C# 13 (`net9.0`), C# 14 (`net10.0`) and C# 15 (`net11.0`), use the `csharp-13`, `csharp-14` and `csharp-15` skills for what those releases add or change.
---

# C# 12

C# 12 ships with the .NET 8 SDK and Visual Studio 2022 version 17.8. It is the default language
version for projects that target `net8.0`. Its features are:

| Feature | Keyword / syntax | Needs preview? |
| --- | --- | --- |
| Primary constructors | `class Widget(int id, string name) { }` | No |
| Collection expressions | `[1, 2, 3]`, `[.. other, e]` | No |
| Inline arrays | `[InlineArray(10)] struct Buffer { private int _element0; }` | No |
| Optional and `params` lambda parameters | `(int x = 2) => x + 1`, `(params int[] xs) => xs.Length` | No |
| `ref readonly` parameters | `void M(ref readonly int p)` called as `M(in v)` | No |
| Alias any type | `using Point = (int X, int Y);`, `using unsafe P = int*;` | No |
| `ExperimentalAttribute` | `[Experimental(\"EXP001\")]` | No |
| Interceptors | `[InterceptsLocation(1, \"…\")]` + `<InterceptorsNamespaces>` | **Yes** (experimental) |

**Everything in this skill stays available in later language versions.** C# 13 (`net9.0`), C# 14
(`net10.0`) and C# 15 (`net11.0`) include every C# 12 feature, so nothing here becomes obsolete when
you upgrade. What changes is what later releases *add on top* and, in two places that matter, what
they *fix or strengthen*:

- Collection-expression overload resolution is **weaker in C# 12** than in C# 13. Several overload
  pairs that compile in C# 13 are `CS0121` ambiguous in C# 12 (section 2). An API surface you design
  here can start resolving to a different overload once a consumer recompiles for `net9.0` or later.
- Interceptors were experimental in C# 12 and became a **supported compiler feature** in the .NET
  9.0.2xx SDK. Read section 8 to recognise the C# 12 form; implement against the current spec.
- The `field` keyword (a C# 13 preview) is the natural follow-up to the primary-constructor state
  question in section 1; it is not C# 12.

`params` collections, `System.Threading.Lock`, partial properties, `OverloadResolutionPriority` and
`allows ref struct` are **C# 13**, not C# 12. `extension` blocks, the stable `field` keyword and
first-class span conversions are **C# 14**. Union types, `closed` hierarchies and `with(...)`
collection arguments are **C# 15**. Do not duplicate them here.

Read section 0 first, then the section for the feature you are using. Each section ends with the
mistakes to avoid.

---

## 0. Before using any C# 12 feature

1. **Check the language version.** Look at `TargetFramework` and `LangVersion`. `net8.0` with no
   `LangVersion` gives C# 12. To see the active version, temporarily add `#error version`: the
   compiler reports CS8304 with the compiler and language versions.
2. **Feature-gated errors are LangVersion errors, not syntax errors.** On `LangVersion 11` the
   compiler reports CS9058 `Feature 'collection expressions' is not available in C# 11.0. Please use
   language version 12.0 or greater.` and does not parse the feature at all. Fix the language
   version, not the code.
3. **Don't set `<LangVersion>latest</LangVersion>`.** Microsoft warns against it because it varies by
   machine, and using a language version newer than the target framework's default is unsupported.
   To pin the semantics described in this skill, set `LangVersion 12` explicitly.
4. **C# 12 is the baseline of the modern series.** Every rule in this file applies unchanged to
   `net9.0`, `net10.0` and `net11.0` projects, except where a later section flags a C# 12-specific
   overload-resolution weakness. For a `net11.0` project apply the `csharp-13`, `csharp-14` and
   `csharp-15` skills as well.
5. **No new contextual keywords.** None of the C# 12 features introduce a keyword that can break an
   existing identifier, so no `@` escapes are needed. `readonly` in `ref readonly` is a modifier
   combination, not a new keyword.
6. **Inline arrays and interceptors are library-authoring features.** You consume inline arrays from
   runtime APIs every day; you rarely declare one. Interceptors exist for source generators. A normal
   application change should not introduce either.

---

## 1. Primary constructors

A parameter list on a `class` or `struct` declaration. The parameters are in scope for the whole
type body, so a field, property, event or method can use them directly instead of declaring a
backing field plus an assigning constructor.

```csharp
// Before C# 12
class Widget
{
    private readonly int _id;
    private readonly string _name;
    public Widget(int id, string name) { _id = id; _name = name; }
    public string Describe() => $\"{_id}: {_name}\";
}

// C# 12
class Widget(int id, string name)
{
    public string Describe() => $\"{id}: {name}\";
}
```

### Scope and name resolution

A primary constructor parameter is usable only in these places (anything else is
**CS9105** `Cannot use primary constructor parameter 'int v' in this context`):

- an argument of a `nameof` expression;
- the initializer of an instance field, property or event of the declaring type;
- the argument list of the base class specification;
- the body of an **instance** method or instance accessor of the declaring type (instance
  constructors are excluded).

Inside an initializer or a base argument list the parameter **shadows** members of the type.
Everywhere else it is **shadowed by** members. So in `class C(int i) { protected int i = i; public int I => i; }`
the initializer binds to the parameter and the property body binds to the field. A name that is
shadowed by a member and then referenced where the member wins reports **CS0103** or silently means
the member, so give the parameter a distinct name when a base type declares the same name (the
compiler warns **CS9179** `Primary constructor parameter 'int a' is shadowed by a member from base`
when the parameter was not forwarded to that base constructor).

### Capture and storage

If an instance member references the parameter (outside a `nameof`), the compiler captures it into
an unspeakable private field. There is no public property: `new Widget(1, \"n\").id` reports **CS1061**
because nothing was synthesized. Only `record` types synthesize properties from primary constructor
parameters; plain classes and structs never do. Consequences:

- The captured field makes the type managed. A struct that previously satisfied `unmanaged` can stop
  satisfying it purely because a member captured a parameter.
- `ref`, `in` and `out` parameters **cannot** be captured: **CS9109** `Cannot use ref, out, or in
  primary constructor parameter 'r' inside an instance member`. `ref` and `out` are legal on a class
  primary constructor only as long as no instance member reads them.
- In a `readonly struct` the capture fields are `readonly`, so accessing a captured parameter from a
  non-readonly member behaves like accessing a readonly field.

Two warnings flag duplicated state, because a captured parameter plus a member assignment means the
value is stored twice:

- **CS9124** `Parameter 'int x' is captured into the state of the enclosing type and its value is
  also used to initialize a field, property, or event.` — the pattern `class C(int x) { public int X
  { get; } = x; public int Use => x; }` triggers it. Copy into a member *or* use the parameter
  directly; doing both is what the warning says.
- **CS9107** `Parameter 'int a' is captured into the state of the enclosing type and its value is
  also passed to the base constructor. The value might be captured by the base class as well.`

An unread parameter reports **CS9113** `Parameter 'neverUsed' is unread.`

### Constructor and initialization rules

- Adding a primary constructor to a **class** removes the implicit parameterless constructor. Declare
  one explicitly (chaining to the primary) if you need it.
- Adding one to a **struct** keeps the implicit parameterless constructor, which zeroes every field
  including the capture fields. `new Pt()` on `struct Pt(int x, int y)` therefore yields `x == 0` and
  `y == 0`; there is no way to run validation in it.
- Every other constructor must chain with `this(...)`: a constructor without a `this` initializer
  reports **CS8862** `A constructor declared in a type with parameter list must have 'this'
  constructor initializer.` A constructor with the *same* parameter list as the primary constructor
  reports **CS0111**.
- Generated order is: store capture fields, run instance initializers, call the base constructor.
  Base-class arguments see the raw parameter, not your initializer.
- A base class with a primary constructor can take arguments: `class Derived(int a) : Base(a)`. The
  argument must be an identity conversion of the parameter to count as forwarded.
- `partial` types may declare the primary constructor in at most one part.

### Mistakes to avoid

- **Reaching for a primary constructor to create public state.** Nothing is synthesized, so
  `class Point(int X, int Y)` gives you a *parameter*, not a property. Use a `record` when you want
  properties, value equality and `ToString()`; use a primary constructor when the parameters are just
  wiring (dependencies, configuration) or inputs to initializers.
- **Mutating a captured parameter expecting it to be visible.** A primary constructor parameter is not
  a property. `class Counter(int count) { public void Increment() => count++; }` compiles but is a
  hidden mutable field with no encapsulation, and it will not survive review.
- **Assuming `new S()` runs your primary constructor** on a struct. It does not. Validation belongs in
  a normal constructor or the type must be a class.
- **Naming the parameter identically to a base member.** Expect CS9179 or, worse, the base member
  silently winning. Rename the parameter.
- **Adding an unused parameter for future use.** CS9113 will fire; delete it.
- **Mixing primary constructor and hand-written field for the same value**, which is exactly what
  CS9124 reports.

---

## 2. Collection expressions

`[e1, e2, e3]` creates a collection value with no target-type-name repetition, and `[.. e]` splices
another collection in.

```csharp
int[] a = [1, 2, 3];
List<string> b = [\"one\", \"two\"];
Span<char> c = ['a', 'b', 'c'];
int[][] twoD = [[1, 2], [3, 4]];
int[] row = [1, 2];
int[] single = [.. row, .. new[] { 3 }];
static readonly ImmutableArray<string> Months = [\"Jan\", \"Feb\", \"Mar\"];
int sum = Sum([1, 2, 3, 4, 5]);
```

### Constructible target types

A collection expression converts to:

- array types (`int[]`, `string[]`);
- `Span<T>` and `ReadOnlySpan<T>`;
- `ImmutableArray<T>`;
- any generic interface in the list `IEnumerable<T>`, `IReadOnlyCollection<T>`,
  `IReadOnlyList<T>`, `ICollection<T>`, `IList<T>`;
- any type usable as a collection-initializer target — that is, one with an accessible **single
  argument** `Add` method (or a suitable `Create` method); this covers `List<T>`, `HashSet<T>`,
  `SortedSet<T>`, `ObservableCollection<T>`, `ReadOnlyCollection<T>` and friends;
- a type with a `Create`/builder method taking `ReadOnlySpan<T>` and marked with
  `[CollectionBuilder(typeof(Builder), nameof(Builder.Create))]`.

Not constructible, and reported as **CS9174** `Cannot initialize type 'X' with a collection
expression because the type is not constructible`:

- the **non-generic** `IEnumerable` and `IList`;
- `Dictionary<TKey, TValue>` and any set interface (`IReadOnlySet<T>`, `ISet<T>`) — the element type
  cannot be inferred from them, or their `Add` takes two arguments. A `Dictionary` reports
  **CS9215** `Collection expression type 'Dictionary<int, int>' must have an instance or extension
  method 'Add' that can be called with a single argument.` Use the `{ }`-brace dictionary
  initializer, or `new Dictionary<int,int> { [1] = 2 }`;
- `Stack<T>` and `Queue<T>` — they have `Push`/`Enqueue`, not `Add`, so you get **CS1061**;
- inline array types (section 6): **CS9174**. Initialize them by indexer or use a real array.

The element type comes from the **iteration type** of the target (its `GetEnumerator` instance
method or enumerable interface), never from an extension method.

### Hard rules

- There must be a target type. `var e = [];` and passing `[1, 2]` to a parameterless method or to a
  method whose parameter is `object` report **CS9176** `There is no target type for the collection
  expression.` This includes generic inference: `Foo([1, 2])` where `Foo<T>(List<T>)` and
  `Foo<T>(T[])` are the candidates reports **CS1736** rather than guessing.
- A collection expression is **not a constant**. Where a compile-time constant is required it reports
  **CS0133** `The expression being assigned to 'c' must be constant`, and as a default parameter
  value **CS1736** `Default parameter value for 'a' must be a compile-time constant.` `const int[] c
  = [1, 2];` and `void M(int[] a = [1, 2])` are both illegal.
- The spread `..e` requires an enumerable expression. `.. someInt` reports **CS9212** `Spread operator
  '..' cannot operate on variables of type 'int' because 'int' does not contain a public instance or
  extension definition for 'GetEnumerator'`.
- Every element is materialized eagerly. Assigning `[1, 2, 3]` to `IEnumerable<int>` still builds an
  in-memory collection — this is not deferred like a LINQ query, and it cannot represent an infinite
  sequence.
- `[]` on an unmodifiable target is emitted as `Array.Empty<T>()`; a `Span<T>`/`ReadOnlySpan<T>`
  target may be stack allocated. Never assume identity between two collection expressions:
  `[1, 2] != [1, 2]` by `ReferenceEquals` for arrays, and `[]` may or may not be shared.
- Elements are evaluated left to right, including each spread's elements in order. A method call in
  an element position runs at that point.

### Overload resolution — and the C# 12 gap

Verified ordering, where one candidate clearly wins:

| Candidates | Winner | Why |
| --- | --- | --- |
| `ReadOnlySpan<int>` vs `int[]` | `ReadOnlySpan<int>` | conversion to a `ref struct` is better than to a non-`ref`-struct type |
| `List<int>` vs `IReadOnlyList<int>` | `List<int>` | conversion to a non-interface type is better |
| `IList<int>` vs `ICollection<int>` | `IList<int>` | `IList<T>` derives from `ICollection<T>`, so it is the better conversion target |
| `int[]` vs `IList<int>` | `int[]` | non-interface beats interface |
| `List<int>` vs `ReadOnlyCollection<int>` | ambiguous (**CS0121**) | neither target converts to the other; element type is identical |

A collection expression with the *same* target kind on both sides is ambiguous — `Span<int>` vs
`List<int>`, `int[]` vs `List<int>`, `ImmutableArray<int>` vs `List<int>`, and
`ObservableCollection<int>` vs `List<int>` all report **CS0121**. That list is unchanged in C# 15.

**Element-type betterness does not work in C# 12.** These are **CS0121** ambiguous in C# 12 and
compile in C# 13 and later:

```csharp
void M(ImmutableArray<string> x) { } void M(ImmutableArray<object> x) { }
void M(List<string> x) { }          void M(List<object> x) { }
void M(IEnumerable<string> x) { }   void M(IReadOnlyList<object> x) { }
M([\"a\", \"b\"]);   // C# 12: CS0121.  C# 13+: picks the string element overload
```

Microsoft's reference text documents \"a better element conversion is preferred over a better
collection type conversion\" as a C# 12 rule, but the shipped compiler only applies it from C# 13. On
`net8.0`/`LangVersion 12` such a call is an error, so the practical fix is an explicit target-typed
local, not a cast-shaped guess:

```csharp
ImmutableArray<string> values = [\"a\", \"b\"];   // name the type, then pass it
M(values);
```

Adding an overload to a library method that takes a collection is therefore a **source-breaking**
change for C# 12 consumers even when it compiles for C# 13+ consumers. Keep collection-expression
parameters off a widely-consumed public surface, or gate the new overload with
`[OverloadResolutionPriority]` once you target `net9.0` (see the `csharp-13` skill).

### Mistakes to avoid

- **`var e = [...]`** — CS9176. Name the target type.
- **`const`, `default` parameter values, attribute arguments, or enum values** holding a collection
  expression — CS0133/CS1736.
- **`Dictionary<,>` or `Stack<T>` as a target** — CS9215/CS1061. Use a dictionary initializer or build
  the stack explicitly.
- **`IReadOnlySet<T> s = [\"a\"];`** — CS9174. Build `new HashSet<T>([\"a\"])` instead; the set interface
  cannot infer an element type.
- **Spreading a non-enumerable** (`..5`, or a type with only an extension `GetEnumerator`) — CS9212.
- **Expecting `IEnumerable<int>` to be lazy.** `[1, 2, 3]` is materialized.
- **Adding a collection-expression overload next to an existing collection overload and calling it
  with `[...]`** on `net8.0` — CS0121. Introduce a typed local.
- **Relying on reference equality or on `[]` being a shared instance.** Re-read `Array.Empty<T>`
  semantics before caching.
- **Using `[]` for an inline array field** — CS9174. Section 6.

---

## 3. `ref readonly` parameters

Declares a read-only reference parameter that **requires the caller to acknowledge it** and forbids
rvalues. It exists so an API can migrate away from `ref` or `in` without breaking callers.

```csharp
static void M(ref readonly int value) { }

int v = 1;
M(in v);       // preferred call form
M(ref v);      // also allowed
```

### Callsite rules

| Argument you write | Against `ref readonly` | Against `in` | Against `ref` | Against `out` |
| --- | --- | --- | --- | --- |
| `ref v` | allowed | **CS9191 warning** | allowed | CS1615 error |
| `in v` | allowed | allowed | CS1620 error | CS1615 error |
| `out v` | CS1615 error | CS1615 error | CS1615 error | allowed |
| `v` (no modifier) | **CS9192 warning** | allowed | CS1620 error | CS1620 error |
| rvalue `5` | **CS9193 warning** | allowed | CS1620 error | CS1620 error |

- **CS9191** `The 'ref' modifier for argument 1 corresponding to 'in' parameter is equivalent to 'in'.
  Consider using 'in' instead.` is the **breaking change in C# 12**: passing `ref` to an `in`
  parameter used to be an error and is now a warning. This is what makes `ref` → `in` non-source-breaking.
- **CS9192** `Argument 1 should be passed with 'ref' or 'in' keyword` and **CS9193** `Argument 1
  should be a variable because it is passed to a 'ref readonly' parameter` are the warnings that make
  `in` → `ref readonly` safe to adopt.
- The warnings are suppressed when the parameter is the receiver of an extension method call, or is
  used implicitly by a collection initializer or an interpolated-string handler.
- Overload resolution treats every *allowed* and *warning* combination above as a viable candidate.
  A by-value overload beats a `ref readonly` overload when the argument carries no modifier.
- `ref readonly` is permitted on **indexer** parameters (like `in`) but not on **operator** parameters
  (like `ref`); on an operator you get CS0631.

### Binary compatibility for API authors

| Migration | Source breaking? | Binary breaking? |
| --- | --- | --- |
| `ref` → `ref readonly` | no | only for virtual methods |
| `ref` → `in` | no (thanks to CS9191) | only for virtual methods |
| `in` → `ref readonly` | no, but new warnings | no |
| `ref readonly` → `in` | no | no |

`in` → `ref readonly` **is** source-breaking for consumers on an older compiler, and for new
compilers pinned to `LangVersion 11` or lower, which interpret the parameter as `ref` and reject the
`in` call site.

### Delegate and method-group conversions

Any allowed cross-modifier conversion compiles but warns **CS9198** `Reference kind modifier of
parameter 'in int p' doesn't match the corresponding parameter 'ref readonly int p' in target.` A
`ref` *target-delegate* parameter cannot be satisfied by an `in` or `ref readonly` lambda
parameter (CS1676 `Parameter 1 must be declared with the '<kind>' keyword`). Delegate signatures are
not covered by the migration table above: changing `ref` to `in` in a **delegate** type can break
code that assigned a `ref` method to it.

Default values are permitted on a `ref readonly` parameter but report **CS9200** `A default value is
specified for 'ref readonly' parameter 'p', but 'ref readonly' should be used only for references.
Consider declaring the parameter as 'in'.` — a default *is* an rvalue, which the feature exists to
forbid. Use `in` if you need a default.

`ref readonly` locals and `ref readonly` return values are not part of C# 12; only the parameter
modifier is.

### Mistakes to avoid

- **Calling a `ref readonly` parameter with a bare argument or a literal.** Compiles with
  CS9192/CS9193 warnings, which a `TreatWarningsAsErrors` build turns into failures. Always write `in`.
- **Trying `ref someStaticReadonlyField`.** A `static readonly` field is not writable, so `ref` to it
  reports CS0199. Use `in`.
- **Adding a default value** to silence a caller complaint — CS9200; the right answer is `in`.
- **Migrating `in` → `ref readonly` in a library** while your consumers may still build with an older
  SDK or `LangVersion 11`. That is a genuine source break.
- **Changing a delegate type's parameter kind** expecting the method-migration table to apply. It does
  not.
- **Using it as documentation only.** If you do not care about rvalue rejection or callsite
  acknowledgment, `in` remains the right modifier.

---

## 4. Optional and `params` lambda parameters

Lambdas can declare default values and `params`; method groups now carry that information into their
natural type.

```csharp
var addWithDefault = (int addTo = 2) => addTo + 1;
addWithDefault();          // 3
addWithDefault(5);         // 6

var counter = (params int[] xs) => xs.Length;
counter();                 // 0
counter(1, 2, 3);          // 3

int AddDefault(int i = 1) => i;
int Count(params int[] xs) => xs.Length;
var a = AddDefault;        // natural type is an anonymous function type, not Action<int>
a();                       // 1 — the default is used
var c = Count;
c(1, 2, 3);                // 3 — the expanded form works
```

Rules and consequences:

- The same rules as method parameters apply: a `ref`, `out` or `this` parameter cannot have a default,
  and `params` must be the last parameter.
- **`delegate { }` anonymous methods do not support defaults or `params`**; you get the long-standing
  CS1065 `Default values are not valid in this context.` (and CS1670 for `params`). Use a lambda.
- Expression trees cannot contain them either. Converting a lambda with a default to an
  `Expression<TDelegate>` reports **CS9099** `Parameter 1 has default value '2' in lambda but
  '<missing>' in the target delegate type.` as a warning when the target delegate has no default, and
  it is not supported in the tree form.
- When a lambda or method group with defaults/`params` converts to a delegate type, the target's
  parameter list must match ignoring the `params` modifiers and default values.
- **Breaking change in the .NET 8 SDK.** A method group whose method has optional or `params`
  parameters no longer has `Action<…>`/`Func<…>` as its natural type; it has an anonymous function
  type. Assigning such a method group to a matching `Action`/`Func` therefore **fails**:

  ```csharp
  Action<int[]> takesArray = Count;   // CS0407: 'int Count(params int[])' has the wrong return type
  ```

  `CS0407` is misleading — the return type is fine. The cause is that the method group is now
  considered as its expanded/optional-fulfilled signature, which no longer matches the delegate.
  Fix it with a lambda wrapper (`Action<int[]> takesArray = xs => Count(xs);`) or an explicit
  `new Action<int[]>(Count)`.
- The same breakage appears when passing the method group as a delegate argument: CS1503 `cannot
  convert from 'method group'` (plus CS1950 in a collection initializer).

### Mistakes to avoid

- **`(int x = 1) => x` inside an expression tree**, or expecting `Expression<Func<int,int>>` to
  preserve the default. It does not; CS9099.
- **Defaults in `delegate (int x = 1) { }`** — CS1065. Use `(int x = 1) => { }`.
- **Being surprised by CS0407 when a `params`/optional method group no longer converts** to
  `Action`/`Func`. Wrap it in a lambda. This is a compile-time break on upgrade, not a bug in your
  code's intent.
- **Adding a default to a `params`-lambda and then passing it to an API whose delegate has no
  default.** The call site cannot use the default; only direct invocation of the lambda can.
- **Assuming `var f = MyMethod;` still gives you `Action<T>`.** Check the natural type before relying
  on it in a generic context.

---

## 5. Alias any type

`using Alias = <any type>;` — tuples, arrays, pointers, generics and unsafe types, not just named
types.

```csharp
using Point = (int X, int Y);
using Pair = System.Collections.Generic.Dictionary<string, int[]>;
using Arr = int[];
using NVT = int?;
using unsafe PtrAlias = int*;
```

Rules:

- **`unsafe` goes before the alias name**: `using unsafe PtrAlias = int*;`. It makes the textual
  extent of the type an unsafe context; without `<AllowUnsafeBlocks>` you get CS0227 `Unsafe code may
  only appear if compiling with /unsafe`. `unsafe` is valid on `using alias` and `using static`
  directives, never on a plain `using namespace`.
- **A nullable reference type cannot be aliased**: `using X = string?;` reports **CS9132** `Using
  alias cannot be a nullable reference type.` `using X = int?;` is fine (nullable *value* type) and
  `using X = List<string?>;` is fine (the alias target itself is not nullable).
- **The alias target is resolved as if the file had no `using` directives.** This is the trap: with
  `using System.Collections.Generic;` present, `using Pair = Dictionary<string, int[]>;` still fails
  with **CS0246** `The type or namespace name 'Dictionary<,>' could not be found`. Write the target
  fully qualified (or with an `extern alias`). Order of `using` directives is irrelevant, and an alias
  never affects the resolution of another alias.
- Aliases can be used as generic arguments (`List<Point>`) and tuple element names survive
  (`p.X`, `p.Y`).

### Mistakes to avoid

- **`using X = string?;`** — CS9132. Alias the non-nullable type and write `X?` at use sites.
- **Writing an unqualified generic in the alias target** because the file has the matching `using`
  — CS0246. Fully qualify it.
- **`using Ptr = unsafe int*;`** — wrong position for `unsafe`. It is `using unsafe Ptr = int*;`.
- **Aliasing for its own sake.** An alias earns its keep on tuple and pointer types, and on a
  `Dictionary<string, (double, TimeSpan)>`-style return type. Aliasing `List<int>` to `Numbers`
  makes the code harder to read for no benefit.

---

## 6. Inline arrays

`[InlineArray(N)]` on a `struct` whose single instance field is the element type. The runtime lays
the struct out as N sequential elements, so it behaves like an `unsafe` fixed buffer with none of the
unsafety.

```csharp
using System.Runtime.CompilerServices;

[InlineArray(10)]
public struct Buffer
{
    private int _element0;
}

var b = new Buffer();
for (int i = 0; i < 10; i++) b[i] = i;
Span<int> span = b;        // implicit conversion to Span<T> / ReadOnlySpan<T>
ref int r = ref b[3];       // writable ref when the variable is writable
int last = b[^1];           // Index supported
foreach (int i in b) { }    // enumerable
```

Declaration rules (compiler-verified):

- The attribute is valid **only on structs**: on a class it reports **CS0592** `Attribute 'InlineArray'
  is not valid on this declaration type. It is only valid on 'struct' declarations.`
- Exactly **one instance field** or **CS9169** `Inline array struct must declare one and only one
  instance field.` (static fields do not count, and a struct with only a static field still fails).
- Length must be **greater than zero**: `[InlineArray(0)]` and `[InlineArray(-1)]` report **CS9167**
  `Inline array length must be greater than 0.`
- The element field may be a managed type, but not a ref-like type: a `ReadOnlySpan<int> _element0`
  field reports CS8345.

Usage facts:

- **There is no `Length` property.** `b.Length` reports **CS1061**. The length is known only at the
  declaration site; convert to `Span<T>` when you need to pass the size along.
- Constant-index bounds **are** checked at compile time: `b[99]` on `InlineArray(3)` reports **CS9166**
  `Index is outside the bounds of the inline array`.
- There is **no** implicit conversion to `T[]`: assigning gives **CS0029**. Convert to `Span<T>`
  instead.
- For ref-safety analysis the indexer behaves like `static ref T GetItem(ref InlineArray array)`, so
  the element you take is movable only if the array variable is movable.
- Do not declare your own. Consume them where runtime or library APIs expose them (the BCL uses them
  internally and exposes `Span<T>`/`ReadOnlySpan<T>` views).

### Mistakes to avoid

- **`Buf b = [1, 2, 3];`** — CS9174. Inline arrays are not collection-expression targets. Assign by
  indexer or via `CopyTo` on a `Span<T>`.
- **`b.Length`** — CS1061. Convert to `Span<T>` and use `.Length`.
- **`int[] a = b;`** — CS0029.
- **Two instance fields** or a **zero/negative length** — CS9169/CS9167.
- **Putting the attribute on a class** — CS0592.
- **Declaring one for convenience in application code** when an array or `stackalloc` span is clearer;
  the feature is for layout control in performance-critical types.

---

## 7. `ExperimentalAttribute`

Marks a type, member or whole assembly/module as experimental. Referencing it is a **warning that is
reported as an error**, so an unaddressed use fails the build.

```csharp
using System.Diagnostics.CodeAnalysis;

[Experimental(\"EXP001\")]
public class NewThing { }

[Experimental(\"EXP002\", UrlFormat = \"https://example.org/{0}\")]
public void Old() { }
```

Semantics (all verified):

- The diagnostic **ID is the string you supplied**, reported as `error EXP001: 'NewThing' is for
  evaluation purposes only and is subject to change or removal in future updates. Suppress this
  diagnostic to proceed.` Because it is technically a warning you can suppress it with
  `#pragma warning disable EXP001`, with `NoWarn`, or with `SuppressMessage`.
- References **inside an `[Experimental]` member are suppressed automatically**, which is how you
  build an experimental surface out of experimental parts.
- The attribute is **not inherited** (`Inherited = false`): `[Experimental] class Base { }` plus
  `class Derived : Base` still errors when you use `Derived`.
- Valid targets: assembly, module, class, struct, enum, constructor, method, property, field, event,
  interface, delegate. All types in an assembly or module marked with it are experimental.
- The diagnostic ID must be a valid C# identifier, otherwise **CS9211** `The diagnosticId argument to
  the 'Experimental' attribute must be a valid identifier.` `\"\"` and `\"not an identifier!\"` both fail.
- `[Experimental]` and `[Obsolete]` (or `[Deprecated]`) can coexist. `[Obsolete]`/`[Deprecated]`
  diagnostics are reported instead of `[Experimental]` when both apply, and `[Experimental]` warnings
  are still reported inside `[Obsolete]` members.
- **`Message` is not in the .NET 8 BCL.** On `net8.0`, `[Experimental(\"EXP002\", Message = \"…\")]`
  reports **CS0246** `The type or namespace name 'Message' could not be found`. `UrlFormat` exists in
  .NET 8. `Message` was added later (it is present in the .NET 10 BCL), so on `net8.0` put the
  guidance in the `UrlFormat` target or a doc comment.

Use it to gate an API you are still shaping without making it `internal`, and to force consumers to
acknowledge instability by suppressing the ID deliberately.

### Mistakes to avoid

- **Expecting a warning that leaves the build green.** It is reported as an error; the build fails
  unless the ID is suppressed somewhere.
- **Reusing one ID across unrelated features.** Suppression is by ID, so suppressing one leaks the
  other. Give each feature its own ID.
- **Inventing a non-identifier ID** like `\"exp-001\"` — CS9211.
- **Assuming `Message =` compiles on `net8.0`** — CS0246. Target-specific.
- **Thinking `[Experimental]` on a base type hides the warning for derived types** — it is not
  inherited.
- **Using it as a synonym for `[Obsolete]`.** Obsolete means \"stop using this\"; experimental means
  \"this shape may change\".

---

## 8. Interceptors (experimental)

A source generator can declaratively replace a *specific call site* with a call to its own method.
This is a code-generation facility, not an application feature.

```xml
<!-- the project consuming interceptors must opt in -->
<PropertyGroup>
  <InterceptorsNamespaces>$(InterceptorsNamespaces);MyLibrary.Generated</InterceptorsNamespaces>
</PropertyGroup>
```

```csharp
static class D
{
    [InterceptsLocation(version: 1, data: \"...(encodes the call site)\")]
    public static void InterceptorMethod(this C c, int param) { /* replaces the call */ }
}
```

Facts that matter for a C# 12 project:

- **The attribute is not in the BCL.** `System.Runtime.CompilerServices.InterceptsLocationAttribute`
  is absent from .NET 8 and from .NET 10 (verified by reflection). A generator must declare it —
  preferably as a `file class` so multiple generators do not collide. Its shape is
  `sealed class InterceptsLocationAttribute(int version, string data) : Attribute` with
  `AttributeTargets.Method`, `AllowMultiple = true`.
- **`<InterceptorsNamespaces>` is the current property name**; `<InterceptorsPreviewNamespaces>` from
  the original C# 12 docs is kept only as a compatibility alias. If both are set they are
  concatenated. Prefer the non-preview name in new projects.
- Interceptors are **experimental in C# 12** and shipped as a supported feature in the .NET 9.0.2xx
  SDK and later. Do not put hand-written interceptors into a production app; only generator authors
  should emit them.
- Only **ordinary method invocations** can be intercepted: not constructors, delegates, properties,
  indexers, local functions or operators. Delegate conversions and address-of usages cannot be
  intercepted. Conditional access (`receiver?.M()`) and `ptr->M()` can.
- The interceptor must be **accessible at the call site**, must match the interceptable signature's
  ref kinds and types, and cannot live in a generic type. A generic interceptor's arity must equal the
  original method's arity plus its containing types' arities.
- Two interceptors targeting the same location is a compile-time error, as is an
  `[InterceptsLocation]` that does not point at a real call.
- Generate the `data` blob with Roslyn's `GetInterceptableLocation(SemanticModel,
  InvocationExpressionSyntax, CancellationToken)` API. The version-1 encoding is base64 of an xxHash128
  file checksum, the `SyntaxNode.Position` of the call, and a display file name — hand-writing it is
  wrong because any edit above the call changes the position.
- The location is the position of the **simple name** syntax of the method (`MapGet` in
  `app.MapGet(...)`, `WriteLine` in `Console.WriteLine(...)`).

### Mistakes to avoid

- **Writing `using System.Runtime.CompilerServices;` and using `[InterceptsLocation]` directly** — the
  attribute is not in the BCL. Declare it `file class` in generated output, or do not use the feature.
- **Forgetting the MSBuild opt-in.** Without `InterceptorsNamespaces` the interceptor is silently
  inert.
- **Targeting a constructor, property or operator** — not interceptable.
- **Hand-rolling the `data` string.** Use `GetInterceptableLocation`; the encoding is position-based
  and breaks on unrelated edits.
- **Building interceptors into an application.** They are for generators, and they make call behavior
  invisible to readers of the source.

---

## 9. Other compiler behavior changes in the .NET 8 SDK

- **Method groups with optional/`params` parameters lost their `Action`/`Func` natural type**
  (section 4). Symptom: CS0407, CS1503, CS1950.
- **`ref` argument to an `in` parameter is now a warning (CS9191)** instead of an error (section 3).
- **A file-local declaration of `InterceptsLocationAttribute` is recognized by the compiler** when it
  is in the same file and compilation.
- Collection-expression, primary-constructor and inline-array behavior described above is the shipped
  C# 12 behavior; some collection-expression overload pairs that fail here compile in C# 13+
  (section 2). Do not infer that code is portable forward just because it compiles now.

---

## 10. Agent checklist

Before writing or reviewing C# 12 code:

1. Confirm `net8.0`/`LangVersion 12`. CS9058 means the language version is too low, not the code.
2. Primary constructor: is any parameter both captured *and* copied into a member (CS9124), or both
   captured *and* forwarded to base (CS9107)? Is any parameter unread (CS9113)? Does a member shadow
   a base member (CS9179)? Does a struct need `new S()` to validate (it cannot)? Do extra constructors
   chain with `this(...)` (CS8862)?
3. Do **not** expect properties from a primary constructor on a non-record type (CS1061). If callers
   need the values publicly, use `record` or declare properties explicitly.
4. Collection expressions: is there a target type (no `var`, CS9176)? Is the target actually
   constructible (not `Dictionary<,>`, `IReadOnlySet<T>`, `Stack<T>`, `Queue<T>`, non-generic
   `IEnumerable`/`IList`, or an inline array — CS9174/CS9215/CS1061)? Is it in a constant/default-value
   position (CS0133/CS1736)?
5. On a collection-expression call site reporting CS0121, insert an explicitly typed local rather than
   adding casts. Remember that element-type betterness does **not** apply in C# 12.
6. `ref readonly`: call with `in`. Never accept a default value (CS9200). Check whether consumers on
   older compilers are affected before migrating a published API.
7. Lambdas: no defaults/`params` in `delegate { }` (CS1065/CS1670) and none in expression trees (CS9099).
   Wrap `params`/optional method groups in a lambda when assigning to `Action`/`Func` (CS0407).
8. Aliases: fully qualify the target, `unsafe` before the alias name, and never alias `string?` (CS9132).
9. Inline arrays: consume, don't declare. No `Length` (CS1061), no `T[]` conversion (CS0029), no
   collection-expression initialization (CS9174), constant bounds are checked (CS9166).
10. `[Experimental]`: unique valid-identifier ID per feature (CS9211), suppression is per ID, not
    inherited, and `Message` is unavailable on `net8.0` (CS0246).
11. Interceptors: only from a source generator, with `file class` attribute declaration and
    `InterceptorsNamespaces` set. Never hand-write them into an app.
12. For anything beyond this list — `params` collections, partial properties, `field`, `extension`
    blocks, unions — switch to the `csharp-13`, `csharp-14` or `csharp-15` skill. C# 12 does not have
    them.

---

## Diagnostic quick reference

| Diagnostic | Meaning / fix |
| --- | --- |
| CS9058 | Feature needs `LangVersion 12.0` or greater; fix the language version. |
| CS8862 | Constructor in a type with a primary constructor must use a `this(...)` initializer. |
| CS9105 | Primary constructor parameter used outside the allowed contexts (e.g. a static member). |
| CS9109 | Cannot use a `ref`/`in`/`out` primary constructor parameter inside an instance member. |
| CS9113 | Primary constructor parameter is unread; remove it. |
| CS9124 | Parameter is captured *and* also used to initialize a field/property/event; pick one. |
| CS9107 | Parameter is captured *and* also passed to the base constructor; value may be duplicated. |
| CS9179 | Primary constructor parameter is shadowed by a base member and was not forwarded. |
| CS9176 | Collection expression has no target type; name the type (no `var`). |
| CS9174 | Target type is not constructible by a collection expression (set interfaces, non-generic collections, inline arrays). |
| CS9215 | Collection-expression target's `Add` must take a single argument (`Dictionary<,>` fails). |
| CS9212 | Spread `..` requires an enumerable expression. |
| CS0133 / CS1736 | Collection expression is not a compile-time constant (assignment / default parameter value). |
| CS1736 | Also: type argument cannot be inferred from a collection expression. |
| CS0121 | Collection-expression call ambiguous; add a typed local. Expected in C# 12 for element-type differences. |
| CS9191 | `ref` passed to an `in` parameter is equivalent to `in`; write `in`. (Warning since C# 12.) |
| CS9192 | Pass a `ref readonly` argument with `ref` or `in`. |
| CS9193 | Rvalue passed to a `ref readonly` parameter; pass a variable. |
| CS9198 | Parameter reference kind differs between lambda/method and target delegate. |
| CS9200 | Default value on a `ref readonly` parameter; use `in` instead. |
| CS1615 / CS1620 / CS1676 | Wrong argument or parameter reference kind. |
| CS0199 | `static readonly` field cannot be passed with `ref`; use `in`. |
| CS1065 / CS1670 | Defaults / `params` are not valid in `delegate { }` anonymous methods; use a lambda. |
| CS9099 | Lambda default value not present in the target delegate type (expression trees). |
| CS0407 | Method group with optional/`params` parameters no longer converts to `Action`/`Func`; wrap in a lambda. |
| CS9132 | A using alias cannot be a nullable reference type (`using X = string?;`). |
| CS0227 | Unsafe code (pointer alias) requires `<AllowUnsafeBlocks>`. |
| CS9166 | Constant index is outside the inline array bounds. |
| CS9167 | Inline array length must be greater than 0. |
| CS9169 | Inline array struct must declare exactly one instance field. |
| CS0592 | `[InlineArray]` is valid only on structs. |
| CS9211 | `[Experimental]` diagnostic ID must be a valid C# identifier. |
| `EXPxxx` (custom ID) | Reference to an `[Experimental]` API; reported as an error, suppressible by ID. |

---

## Sources

- C# language versioning — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version>
- Collection expressions reference — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions>
- `struct` types (inline arrays) — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#inline-arrays>
- Method parameters (`ref readonly`) — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters#ref-readonly-modifier>
- Lambda expressions — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression>
- General attributes read by the compiler (`Experimental`) — <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/general#experimental-attributes>
- Primary constructors tutorial — <https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/primary-constructors>
- C# 12 speclets — <https://github.com/dotnet/csharplang/tree/main/proposals/csharp-12.0>
  (`collection-expressions.md`, `primary-constructors.md`, `ref-readonly-parameters.md`,
  `inline-arrays.md`, `lambda-method-group-defaults.md`, `experimental-attribute.md`,
  `using-alias-types.md`)
- Interceptors specification — <https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md>
