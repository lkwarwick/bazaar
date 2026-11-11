---
name: bazaar
description: "Use when writing or changing Unity C# code in a project that contains Bazaar (com.lkwarwick.bazaar), deciding whether Bazaar already provides a suitable component or helper, or adding a reusable feature to Bazaar."
---

# Working with Bazaar

Use this skill to reuse Bazaar's existing APIs accurately and to keep its feature index current. The package source is authoritative; the index is a discovery aid, not a substitute for checking the code.

## Before changing code

1. Locate the installed package. Check `Packages/manifest.json` for `com.lkwarwick.bazaar`, then identify its package root. Do not assume it is embedded at `Packages/bazaar`; a package may be installed elsewhere.
2. Read `<package-root>/Documentation~/copilot/features.md` if it exists. Use it to find relevant APIs and source files.
3. Verify candidate APIs in the installed package source before using or describing them. Check the relevant assembly definition and existing call sites or examples when available. Do not rely on memory, the index alone, or a different Bazaar version.
4. Prefer a suitable existing Bazaar API over a duplicate implementation. If no existing API fits, decide whether the behavior belongs in the consuming game or is reusable across projects; do not add abstractions to Bazaar just to solve a one-off game requirement.

## Package boundaries and conventions

- Check namespaces and assembly definitions before adding references. Current runtime areas include the `Bazaar` and `Bazaar2D` assemblies; editor-only code belongs in `Bazaar.Editor`. Verify these against the installed package.
- Keep runtime code free of Unity Editor dependencies. Put editor extensions in the editor assembly.
- Check the installed `package.json` for the package's Unity version and dependencies before relying on an API.
- For Addressables, inspect the asset's configured address and use that exact key. Do not infer an address from an asset name.
- Follow the consuming project's Unity instructions and existing conventions. Do not edit generated project files or Unity `.meta` files by hand.

## When adding or changing a reusable Bazaar feature

1. Implement the feature in the appropriate package assembly and preserve existing behavior.
2. Update `<package-root>/Documentation~/copilot/features.md` in the same change:
   - Add one entry for a new reusable feature.
   - Update the existing entry when its API, behavior, source location, or important usage constraints change.
   - Do not add an entry for game-specific code that merely uses Bazaar.
3. Use the index's entry template. Include the feature's purpose, API or extension points, namespace/assembly, source path, and important caveats or dependencies.
4. Verify the entry against the source and run the narrowest relevant validation available in the repository. If the index cannot be updated because the package or its documentation is unavailable, state that clearly rather than silently omitting it.

## Accuracy

Treat implementation and signatures in the installed package as the source of truth. If the index is stale, correct it as part of the relevant package change. Do not invent APIs, behavior, examples, or package capabilities.
