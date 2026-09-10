using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace IdeologyReformation.Restrictions;

/// <summary>
/// Remembers which memes were available (per <see cref="MemeAvailabilityUtility"/>) as of the
/// last time it was asked, so callers can report which memes newly became available since then
/// (typically: since the last "you can reform your ideoligion" letter). Tracking is global for
/// the save, not per-ideo, since availability only depends on colony-wide tech/research state.
/// </summary>
public class MemeAvailabilityTracker : GameComponent
{
    private List<string> knownAvailableMemeDefNames = new();
    private bool initialized;

    public MemeAvailabilityTracker()
    {
    }

    public MemeAvailabilityTracker(Game game)
    {
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref knownAvailableMemeDefNames, "fio_knownAvailableMemes", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            knownAvailableMemeDefNames ??= new List<string>();
            initialized = true;
        }
    }

    /// <summary>
    /// Returns memes that are available now but were not the last time this was called (or, on
    /// the very first call in a save, an empty list - there is nothing to compare against yet).
    /// Advances the remembered baseline to the current state as a side effect.
    /// </summary>
    public List<MemeDef> ComputeNewlyAvailableAndAdvance()
    {
        List<MemeDef> currentlyAvailable = DefDatabase<MemeDef>.AllDefsListForReading
            .Where(MemeAvailabilityUtility.IsMemeAvailable)
            .ToList();

        List<MemeDef> newlyAvailable = initialized
            ? currentlyAvailable.Where(m => !knownAvailableMemeDefNames.Contains(m.defName)).ToList()
            : new List<MemeDef>();

        initialized = true;
        knownAvailableMemeDefNames = currentlyAvailable.Select(m => m.defName).ToList();
        return newlyAvailable;
    }
}
