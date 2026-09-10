# Ideology Reformation

A RimWorld 1.6 mod for **Ideology** that turns Fluid ideoligion reform into a deliberate, reviewable act instead of a free-for-all editor. Requires **Ideology** and **Harmony**.

## What it changes

In vanilla RimWorld, a Fluid ideoligion can be reformed by opening the ideoligion editor and changing almost anything at once, with little friction and no summary of what actually happened. Ideology Reformation replaces that with a guided reform session:

- **One deliberate change per reform.** Each reform is scoped to a single mechanical object — a precept, a ritual, a role, and so on. You can freely edit multiple *fields* of that same object (a ritual's date, reward and name together still count as one change), but you can't quietly change a precept and a role in the same sitting.
- **Cosmetic edits stay free.** Names, descriptions, colors, icons and style categories can be adjusted inside a reform without spending your one deliberate change.
- **Meme changes are handled separately.** Adding or removing a meme is its own kind of reform, and the precept changes memes force are shown as consequences of that choice, not hidden side effects.
- **Automatic precept preservation.** When a meme change forces other precepts to shift, existing rituals, roles, buildings, relics, weapons, venerated animals and appearance choices are kept wherever they're still valid, instead of being silently reset.
- **A review screen before anything is final.** Every reform ends with a summary of exactly what changed — primary change and any consequences — before you commit. You can go back and adjust, or cancel entirely with nothing touched.
- **Reset really resets.** Backing out of a reform at any point restores the ideoligion to how it looked when you opened the editor, with development points untouched.
- **Exactly-once commitment.** Confirming a reform spends development points through RimWorld's normal system exactly once — no double-spends, no free changes slipping through.

Fixed ideoligions are untouched by the mod; only Fluid ideoligions go through the reform session. Vanilla development-point costs are unchanged.

## Tech-gated memes

Some memes can be restricted so they're only available to reform into once your colony has reached the right tech level or finished specific research (for example, `Darkness` requires Electricity, `Transhumanist` requires Biosculpting). This is fully data-driven — see [1.6/Patches/MemeAvailability.xml](1.6/Patches/MemeAvailability.xml) — so unlocks can be rebalanced or extended for other mods' memes without a code change.

In mod settings, you can choose whether "current tech level" for these gates means your colony's actual tech level or the highest tech level you've researched into, whichever fits your game better.

## Installing

Subscribe via Steam Workshop, or place this mod's folder in your RimWorld `Mods` directory.

## Building from source

```powershell
dotnet build Source\IdeologyReformation.csproj -c Debug
```

The project references your local RimWorld and Harmony installations directly. Set `RIMWORLD_DIR` or `HARMONY_DLL` if they aren't in their default Steam locations. A successful build copies the assembly to `1.6\Assemblies`.

See [docs/DESIGN_REVIEW.md](docs/DESIGN_REVIEW.md) for the implementation design and [docs/PHASE1_TEST_PLAN.md](docs/PHASE1_TEST_PLAN.md) for the in-game test plan.
