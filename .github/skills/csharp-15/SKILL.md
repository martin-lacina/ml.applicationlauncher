---
name: csharp-15
description: Add C# 15 guidance to the shared C# 14 baseline for net11.0 projects using C# 15. Covers migrations, union types, closed hierarchies, extension indexers, collection expression arguments, labeled break or continue, static interface members on older runtimes, and the memory safety preview.
---

# C# 15 additions

Use this guide alongside the [C# 14 baseline](../csharp-14/SKILL.md), which covers earlier features still available in C# 15. The .NET 11 SDK defaults to C# 15 for `net11.0`. Check the effective SDK, `TargetFramework`, and `LangVersion` before using these features. Use stable C# 15 features when they improve everyday code; don't raise a project's baseline just to use one. For migrations, distinguish [compiler changes in the .NET 11 SDK](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2011) from changes gated on C# 15.

## Unions and closed hierarchies

- Use a [union](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union) to group existing case types without requiring a shared base. Use a [closed class](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/closed) when the abstraction is already an inheritance hierarchy. Review exhaustive switches when adding cases or subclasses; a broad catch-all can hide a newly unhandled case.
- A declared union boxes value-type cases. Check allocation-sensitive paths before replacing an existing representation. Overlapping case types can make implicit conversions ambiguous; select the case explicitly.
- Type patterns on a union inspect its contents; `var` and untyped property patterns inspect the wrapper. A nonnullable struct union can still have empty contents (`default`): `u is { }` isn't a has-value test, while `u is not null` tests the contents. Handle absent contents explicitly if callers can supply them.
- `closed` is implicitly abstract and restricts **direct** derivation to the declaring assembly. It doesn't seal the entire tree: an open descendant can still be subclassed elsewhere. Mark intermediate branches `closed` when exhaustiveness must extend below them, and check downstream subclasses before closing a public base type.

## Collections, indexers, and control flow

- Keep construction semantics when refactoring collection initialization. In `HashSet<string> keys = [with(StringComparer.OrdinalIgnoreCase), .. names];`, `with(...)` is the first element and supplies constructor or builder arguments, not collection items. The target type determines what arguments are accepted. Keep an explicit constructor or factory call for earlier language versions.
- An [extension indexer](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension#extension-indexers) needs a named receiver. It is considered only when normal instance indexer lookup finds no applicable candidate; it doesn't override an existing indexer. Check the caller's imports and receiver's static type before diagnosing a binding change.
- A labeled `break` can exit an enclosing loop or `switch`; a labeled `continue` can only target an enclosing loop. Place the label directly on its target. When replacing flags or `goto` (IDE0410), confirm that the jump doesn't skip intervening work the old code performed.

## Runtime and preview boundaries

- Only **non-virtual static** interface members receive the [older-runtime relaxation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface#static-interface-members-and-runtime-support). Call helpers through the interface name. Don't assume it also enables default instance implementations, `static abstract` or `static virtual` dispatch, or protected interface members on those runtimes.
- The [memory safety model](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#the-updated-memory-safety-model-preview) is **preview**, not part of the stable C# 15 baseline. `LangVersion=preview` enables the new syntax and pointer relaxations. The `updated-memory-safety-rules` feature flag additionally enables caller obligations for members marked `unsafe`. `unsafe(expression)` also requires `AllowUnsafeBlocks`. Pointer creation or pinning doesn't imply safe dereference: memory access and function-pointer invocation still need an unsafe context. Don't turn on preview project-wide as part of an unrelated change.

For examples and feature links, see [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15).
