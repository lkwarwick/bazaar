# Bazaar feature index

This index helps coding agents and package users discover existing Bazaar capabilities. It is intentionally concise: verify signatures, behavior, dependencies, and version-specific details in the source before using an API. Update this file in the same change whenever a reusable package feature is added or its public API or important behavior changes.

## Install the Copilot skill

To make Copilot discover Bazaar guidance in a consuming Unity project, copy this package's `SKILL.md` to `<project-root>/.github/skills/bazaar/SKILL.md`. Copilot discovers workspace skills from the project; do not rely on a skill nested only inside the installed UPM package being auto-discovered. The skill looks up this feature index in the installed package.

## Runtime: `Bazaar`

Assembly: `Bazaar`

| Feature | Use for | API / extension points | Source |
|---|---|---|---|
| Sprite animation loop | Playing a sprite sequence on a `SpriteRenderer` or UI `Image`, with optional looping, unscaled time, finish behavior, and frame/finish events. | `AnimationLoop`; `visualTarget`, `sprites`, `framesPerSecond`, `destroyOnFinish`, `playOnStart`, `loop`, `useUnscaledTime`; `OnFrameChange`, `OnAnimationFinished`; `Play()`, `Play(SpriteAnimationDefinition)`, `Stop()` | `Runtime/Bazaar/Animation/AnimationLoop.cs` |
| Named sprite animations | Selecting and playing named sprite sequences on a `SpriteRenderer` or UI `Image`. | `AnimationGroup`; `AnimationState`; `animationStates`, `defaultAnimation`; `CurrentAnimation`, `CurrentFrame`; `OnFrameChange`, `OnAnimationFinished`; `Set`, `Play`, `Stop` | `Runtime/Bazaar/Animation/AnimationGroup.cs` |
| Sprite animation assets | Reusable animation data configured as a `ScriptableObject`, including playback settings. | `SpriteAnimationDefinition`; `Sprites`, `FramesPerSecond`, `Loop`, `DestroyOnFinish`, `UseUnscaledTime` | `Runtime/Bazaar/Animation/SpriteAnimationDefinition.cs` |
| Addressable animation playback | Loading, preloading, caching, and releasing `SpriteAnimationDefinition` assets by Addressables key; async playback returns the spawned `GameObject`. | `AnimationPlayer.PlayAddressable`, `PlayAddressableAsync`, `PreloadAsync`, `ReleaseCachedDefinitions` | `Runtime/Bazaar/Animation/AnimationPlayer.cs` |
| Sprite pixel and sheet utilities | Adjusting sprite pivots, finding pixels, converting pixel coordinates, recoloring/removing pixels, outlining, and dividing sprite sheets. | `SpriteExtensions.PivotCenter`, `CenterSprite`, `GetPixelPoint`, `TryGetPixelPoint`, `PixelPointToOffset`, `RemovePixelPoints`, `ToColor`, `AddOutline`, `DivideSpriteSheet`, `DivideSpriteSheetFlat`, `GetPixelWidth` | `Runtime/Bazaar/Extensions/SpriteExtensions.cs` |
| Subclass selector attribute | Marking a string field for a subclass selection UI in the Unity Inspector. The selected value is an assembly-qualified type name. | `SubclassSelectorAttribute(Type baseType)` | `Runtime/SubclassSelectorAttribute.cs` |
| VisualElement callbacks | Registering mouse, focus, click, and navigation-submit callbacks across UI Toolkit platforms; enter, exit, and submit actions are optional. | `VisualElementExtensions.AddCallbacks`, `VisualElementExtensions.RemoveCallbacks` | `Runtime/Bazaar/UIToolkit/VisualElementExtensions.cs` |

## Runtime: `Bazaar2D`

Assembly: `Bazaar2D`

| Feature | Use for | API / extension points | Source |
|---|---|---|---|
| 2D movement base | Rigidbody-based movement for entities that provide their own input; exposes movement/facing state, freeze locks, a facing-change event, and knockback. | `Movement2D`; `speed`, `canReceiveKnockback`, `Direction`, `LastDirection`, `FacingDirection`, `LastFacingDirection`, `FreezeLocks`, `OnLastFacingDirectionChange`; `Knockback`, `SetFacingDirection`; protected overrides `ReceiveInput`, `GetSpeed` | `Runtime/Bazaar/2D/Movement2D.cs` |
| Player input movement | Reading a 2D movement vector from an Input System action reference. | `PlayerMovement2D.moveAction` | `Runtime/Bazaar/2D/PlayerMovement2D.cs` |
| 2D camera follow | Smoothly following a target with offset, distance, and Z-axis settings; static helpers move the camera or trigger a shake. | `CameraFollow2D`; `target`, `offset`, `ignoreZ`, `smoothTime`, `maxDistance`, `speedMultiplier`; `ShakeStrength`; `MoveToPosition`, `Shake` | `Runtime/Bazaar/2D/CameraFollow2D.cs` |

## Editor: `Bazaar.Editor`

Assembly: `Bazaar.Editor` (Editor-only)

| Feature | Use for | API / extension points | Source |
|---|---|---|---|
| Subclass selector drawer | Drawing the Inspector control for fields marked with `SubclassSelectorAttribute`. | Editor implementation for `SubclassSelectorAttribute` | `Editor/SubclassSelectorDrawer.cs` |

## Package notes

- The package identifier is `com.lkwarwick.bazaar`. Check the installed `package.json` for the current Unity version and dependencies.
- The runtime code uses both `Bazaar` and `Bazaar2D` namespaces. Check the source and assembly definitions; do not infer a namespace from a feature name.
- `PlayerMovement2D` uses the Unity Input System. `AnimationPlayer` uses Addressables.
- `SpriteExtensions.TryGetPixelPoint` returns `false` for a missing sprite, missing texture, or unreadable texture. Check each method's implementation for its own texture-readability and allocation behavior before using it.

## Adding or updating an entry

When a reusable package feature is added, append a row to the appropriate section above. If an existing feature changes, update its row instead of adding a duplicate. Use this format:

| Feature name | What it is for, including a useful constraint or behavior | Public types and main entry points | Package-relative source path(s) |
|---|---|---|---|
| Example feature | One concise sentence describing when to use it. | `ExampleType.Method` | `Runtime/Example/ExampleType.cs` |

Keep entries discoverable and factual. Mention only APIs that exist in the package's public source; put nuanced usage details in package documentation and keep implementation-specific caveats accurate.
