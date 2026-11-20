---
name: csharp-14
description: Shared baseline for writing and reviewing C# 14 and later code on net10.0 or later. Covers primary constructors, collection expressions, params, Lock, ref safety, extension blocks, field-backed properties, span overloads, and other commonly relevant language features.
---

# C# 14 baseline

`net10.0` defaults to C# 14. Check the effective SDK, target framework, and `LangVersion`. Use stable features from C# 14 **and earlier** when they improve clarity or correctness. For C# 15 projects, use this baseline together with the [C# 15 additions](../csharp-15/SKILL.md).

## Primary constructors and member declarations

- Initializing a member from a primary-constructor parameter doesn't require persistent capture; reading the parameter from an instance method or accessor normally does. Keep one source of truth: don't capture and also copy into a member (CS9124), or capture a value already forwarded to a base that may store it (CS9107). Check base-member shadowing (CS9179). Parameters aren't public properties on non-record types.
- Other explicit constructors chain through `this(...)`. Adding a primary constructor to a class removes its implicit parameterless constructor. A struct's `default` value and zero-initialized array elements still bypass constructor validation.
- For partial properties and indexers, match the defining and implementing signatures, including accessor modifiers and parameter names. Two semicolon-only declarations are not an implementation. Put caller-facing defaults and caller-info attributes on the defining declaration.

## Collection expressions, `params`, and overloads

- Collection expressions need a supported target type and materialize their elements even when the target is `IEnumerable<T>`. Don't substitute them for deferred queries. If a call with `[...]` becomes ambiguous or binds to the wrong overload, pin the intended collection type with a typed local.
- Adding `params ReadOnlySpan<T>` beside `params T[]` can rebind calls on recompilation. Test zero arguments, individual elements, an existing array or span, and an explicit collection expression separately. Consider callers that can't use expanded non-array `params`, such as Visual Basic, expression trees, and dynamic binding. Span `params` parameters are implicitly `scoped` unless annotated with `[UnscopedRef]`.
- `[OverloadResolutionPriority]` prunes applicable overloads **within each declaring type** before ordinary betterness. It can't make one extension class outrank another or be assigned on an override. Check callers before using it: it can redirect source calls on recompilation even when existing binaries remain compatible.

## Locking and ref safety

- Migrate all synchronization paths for the same object together. `lock` on a statically typed `System.Threading.Lock` uses `EnterScope`; after conversion to `object` it uses `Monitor`, a different synchronization mechanism. If the critical section must suspend, use an async-compatible primitive instead.
- Use ref locals and `ref struct` locals only within synchronous segments of async or iterator methods. Reacquire spans from the underlying storage after suspension. To consume a span before returning an iterator, use a non-iterator wrapper; moving the read before the first `yield` inside the iterator body is not enough.
- A `ref struct` can implement an interface but can't be boxed into it. Access explicit implementations through a constrained type parameter, for example `where T : IDisposable, allows ref struct`. A type parameter with that anti-constraint must obey ref safety; don't box it, store it in a heap field, or keep it across suspension.
- For published APIs, use `ref readonly` when callers should pass an existing variable and explicitly acknowledge the reference. Prefer `in variable` at the call site. A bare rvalue warns rather than fails; use `in` on the declaration if passing temporaries is intended. Check [method and delegate compatibility](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-12.0/ref-readonly-parameters.md) separately before changing an existing signature.

## Span conversions can rebind existing calls

First-class span conversions affect extension receiver eligibility and generic inference. Previous BCL conversions already allowed some assignments (for example, from `string` to `ReadOnlySpan<char>`); the change is how the compiler treats the conversions. Span conversions don't make an extension receiver eligible for a method-group conversion. `ReadOnlySpan<T>` often wins over `Span<T>` for array arguments, but an identity conversion to `T[]` can still beat a span conversion.

| Symptom after an upgrade | What to check or do |
| --- | --- |
| `Assert.Equal` with a collection expression or `ArraySegment<T>` becomes ambiguous | Specify the intended overload using `.AsSpan()` on the array if span comparison is what you want. |
| `MemoryMarshal.Cast` no longer returns a writable span for an array argument | Pass `.AsSpan()` to choose the writable overload explicitly. |
| `object[]` referring to a `string[]` now throws `ArrayTypeMismatchException` | A writable span overload may have been selected. Pin the enumerable overload when that's the intent; use `ReadOnlySpan<T>` for new APIs that don't need mutation. |
| An array's `Reverse()` becomes an in-place, `void` span call | Check the TFM. `net10.0` supplies an array LINQ overload that prevents this particular break. On an older TFM with C# 14, use `Enumerable.Reverse(array)` to make the intent explicit. |
| A tree or query provider no longer handles an array `Contains` call | Check whether it bound to `MemoryExtensions` instead of LINQ. Use `Enumerable.Contains(array, value)` when the tree must retain the LINQ call. A call accepted by the compiler isn't necessarily supported by an interpreter or provider. |

See the [span feature spec](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-14.0/first-class-span-types) and [.NET 10 compiler changes](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2010).

## Extension blocks and `field` refactors

- Extension declarations still belong in a top-level, nongeneric static class. Instance members need a named receiver; static extensions don't. Check lookup in the caller's scope: an applicable real member takes precedence, and competing extensions can be ambiguous. Disambiguate instance methods with a qualified static call, such as `FirstExtensions.M(receiver)`. For a public API refactor, preserve the callable signature and verify existing usages; don't change a method into a property and assume compatibility.
- `field` refers to synthesized storage inside a property accessor. If it would otherwise refer to an existing symbol, CS9258 flags the changed binding; use `this.field` for a member or `@field` for the identifier. A local or nested parameter named `field` in an accessor can trigger CS9272.
- A property initializer writes backing storage directly; it doesn't run validation, normalization, or notifications in the setter. Before removing a named field, check all uses outside the property, including reflection and serialization. Don't substitute `field` in an indexer, event, or ref-returning property.
- Partial constructors place `this(...)` or `base(...)` only on the implementing declaration. Partial events use the field-like defining declaration and an implementation with both `add` and `remove`.

## Assignment and expression trees

- Instance compound-assignment operators can mutate an object that other references still point to. Don't modernize `+=` or `++` across an API boundary without checking aliasing, whether the result is consumed, and whether the target is a variable or a property. `x += y` and `x = x + y` need not do the same thing. A checked operator must have its unchecked counterpart.
- Null-conditional assignment evaluates the receiver once and skips the right-hand side when that receiver is null. It supports compound assignment, not increment/decrement, `ref` assignment, or deconstruction targets. A chain only guards receivers marked with `?.` or `?[]`.
- Expression trees accept omitted optional call arguments and in-position named call arguments. Out-of-position names still fail with CS9307, including jumping past an omitted earlier argument. This doesn't remove other expression-tree restrictions, such as the ban on expanded non-array `params` calls.

For syntax and remaining features: [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14), [extension specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-14.0/extensions), [expression-tree restrictions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/expression-tree-restrictions), and [file-based apps](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps) (`#:` directives are SDK syntax).
