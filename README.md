# Fluid Ideology Overhaul

Fluid Ideology Overhaul is a RimWorld 1.6 Ideology mod that makes Fluid ideoligion reform gradual and deliberate while preserving valid player-authored ideology content.

Phase 1 is implemented:

- one deliberate mechanical ideology object per reform;
- multiple mechanical fields on the selected ritual or role count as that same object;
- cosmetic changes remain free inside an earned reform;
- reset fully restores the opening snapshot;
- meme cascades are separated from deliberate player intent;
- automatic precept preservation across meme cascades (fixed behavior, not configurable);
- a review screen before every final commitment;
- exactly-once commitment through RimWorld's normal development tracker.

## Planned delivery

1. Controlled reform: implemented.
2. Tech-gated memes: planned as a generic effective-tech service and data-driven per-meme requirements.
3. Compatibility: planned, beginning with Vanilla Ideology Expanded - Memes and Structures without requiring it when absent.

Vanilla development-point costs and progression remain unchanged in the first release. Fixed ideoligions remain outside the mod's scope.

## Build

```powershell
dotnet build Source\FluidIdeologyOverhaul.csproj -c Debug
```

The project references the local RimWorld and Harmony installations directly. Set
`RIMWORLD_DIR` or `HARMONY_DLL` if they are not installed in their default Steam
locations. A successful build copies the assembly to `1.6\Assemblies`.

## Design

See `docs\DESIGN_REVIEW.md` for the implementation interpretation, risks, open decisions and initial verification plan derived from the v2 design brief.
