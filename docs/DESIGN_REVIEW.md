# Fluid Ideology Overhaul Design Review

## Outcome

The v2 brief is suitable as the product baseline. The phases are correctly ordered: establish a safe transaction boundary and one-object reform rule before adding tech gating or compatibility data. Phase 1 is the architectural core; later work should depend on it rather than patching the editor independently.

Phase 1 is now implemented against RimWorld 1.6's existing working-copy reform dialog. The mod does not persist a parallel ideology or replace the editor. It records player intent at the meme and precept mutation boundaries, treats meme regeneration as a scoped consequence operation, inserts an unconditional review before vanilla confirmation, and guards the final callback against duplicate commitment.

## Required Phase 1 architecture

Use a reform session owned by the active Fluid ideology editor. It should contain an immutable opening snapshot, an isolated working ideology, the selected deliberate object identity, and an explicit lifecycle state. The real ideology must remain unchanged until final confirmation.

The deliberate object identity should be stable across the snapshot and working copy. Def-backed objects can use their definition identity. Generated objects such as rituals and roles will need a stable session-local identity strategy rather than reference equality alone.

Record deliberate player commands at the editor boundary. Do not infer intent by counting the final ideology diff: one meme command may legitimately produce many consequential changes. A separate consequence resolver should classify required additions, changes and removals and feed the review screen.

Commit must be idempotent and must call the vanilla reform/development path exactly once. Reset discards the entire working state, including cosmetic edits, and recreates it from the opening snapshot. Go Back returns from review to the same working edit session; it is distinct from Reset.

## Preservation strategy

Preserve compatible existing content first. Any change caused by meme validity rules must be classified as required, optional or invalid. Optional newly unlocked content must not be granted automatically. This is fixed mod behavior, not a player-configurable mode.

The installed Persistent Precepts Workshop payload (`2944765939`) confirms the relevant content categories but contains only a compiled assembly and no licence file. Do not copy or decompile its code. Check the upstream repository licence before adapting any published technique, and document provenance for anything reused.

## Scope boundaries

- Apply all mechanics only to Fluid ideoligions.
- Do not retroactively validate existing ideology content on load or install.
- Keep vanilla development costs, escalation and non-banking behaviour.
- Do not change meme-cap behaviour until the open design question is resolved.
- Do not implement multiplayer guarantees in the first release.
- Do not add a hard dependency on Vanilla Expanded Framework or Persistent Precepts.

## Decisions still needed

1. Define the complete mechanical-object taxonomy, especially relics, buildings, weapons, venerated animals, preferred xenotypes, apparel and appearance controls.
2. Confirm whether changing an ideology structure is in scope and, if so, whether it is treated as its own reform object.
3. Specify behavior at the vanilla meme cap. The current build must preserve vanilla behavior.
4. Decide how the review screen represents very large preservation/cascade summaries without overwhelming the player.
5. Confirm the authoritative colony/faction source for Actual Tech Level when multiple player maps or factions are present.
6. Define tie-breaking for Highest Researched Tech when completed projects have missing or modded tech-level metadata.

## Phase 1 interpretations

- Structure changes are mechanical and consume the one-object allowance.
- Ideology style categories are presentation-only and remain free, like names, colours and icons.
- A selectable ordinary precept is identified by its issue, so replacing one value with another is one object.
- Rituals use their stable precept ID, allowing several fields on one ritual without unlocking another ritual.
- Roles use their role definition identity; apparel requirements are mechanical while titles are cosmetic.
- Relic material is mechanical. Building visual style and precept names are cosmetic.
- Preservation always runs in Preserve Where Possible behavior: it uses RimWorld's compatibility checks and restores removed opening precepts only when the new meme set still accepts them. There is no Vanilla Cascades or Strict Preservation mode.

## Implementation sequence

1. Map the RimWorld 1.6 editor call graph for opening a Fluid reform, mutating each object category, regenerating content and committing development.
2. Build the reform-session transaction and lifecycle guards with no cascade changes.
3. Intercept deliberate player commands and enforce the first-object lock.
4. Add reset, cancel and review/confirm paths, including idempotent commit protection.
5. Add preservation and consequence classification for meme addition, then meme removal.
6. Exercise Phase 1 against mod-added content before starting tech gating.
7. Add the effective-tech service, curated requirements and override settings.
8. Add data-driven Vanilla Ideology Expanded compatibility.

## Initial verification matrix

| Scenario | Required result |
| --- | --- |
| Change one precept, then try another | The second mechanical object is disabled; review shows only the first deliberate object. |
| Edit several fields on one ritual or role | All edits are allowed and count as one deliberate reform. |
| Make cosmetic edits before and after selecting an object | Cosmetic edits remain free but exist only inside the earned reform session. |
| Add or remove a meme with dependencies | One deliberate meme change is recorded; required consequences are listed separately. |
| Reset after mixed mechanical and cosmetic edits | The opening snapshot is restored and no development is spent. |
| Close or abandon before confirmation | The real ideology and save remain unchanged. |
| Confirm, then trigger close/save callbacks | The working ideology commits once and vanilla development is consumed once. |
| Open a Fixed ideology editor | Behavior remains entirely vanilla. |
| Add the mod to an existing save | No existing ideology is rerolled, removed or validated on load. |

## Repository setup choices

The assembly and namespace are `FluidIdeologyOverhaul`; the package ID and Harmony ID are `cruesoe.fluidideologyoverhaul`. The self-contained project targets RimWorld 1.6 and .NET Framework 4.7.2, references the local RimWorld and Harmony installations, declares Ideology and Harmony dependencies, and ships only `About`, `Languages` and the versioned assembly directory.
