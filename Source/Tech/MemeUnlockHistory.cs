using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FluidIdeologyOverhaul.Tech;

public sealed class MemeUnlockHistory : GameComponent
{
    private List<IdeoUnlockSnapshot> snapshots = new();

    public MemeUnlockHistory(Game game)
    {
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref snapshots, "fioMemeUnlockSnapshots", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            snapshots ??= new List<IdeoUnlockSnapshot>();
        }
    }

    public override void StartedNewGame()
    {
        EstablishMissingBaselines();
    }

    public override void LoadedGame()
    {
        // Older saves have no historical snapshot. Treat their current state as
        // the baseline instead of presenting long-completed research as new.
        EstablishMissingBaselines();
    }

    public List<MemeDef> NewlyUnlockedSinceLastReform(Ideo ideo)
    {
        IdeoUnlockSnapshot? snapshot = SnapshotFor(ideo);
        if (snapshot == null)
        {
            Capture(ideo);
            return new List<MemeDef>();
        }

        return MemeTechUtility.ResearchUnlockedMemes()
            .Where(meme => !snapshot.unlockedMemeDefNames.Contains(meme.defName))
            .OrderBy(meme => meme.label)
            .ToList();
    }

    public void Capture(Ideo ideo)
    {
        IdeoUnlockSnapshot? snapshot = SnapshotFor(ideo);
        if (snapshot == null)
        {
            snapshot = new IdeoUnlockSnapshot { ideologyId = ideo.id };
            snapshots.Add(snapshot);
        }

        snapshot.unlockedMemeDefNames = MemeTechUtility.ResearchUnlockedMemes()
            .Select(meme => meme.defName)
            .ToList();
    }

    private IdeoUnlockSnapshot? SnapshotFor(Ideo ideo)
    {
        return snapshots.FirstOrDefault(snapshot => snapshot.ideologyId == ideo.id);
    }

    private void EstablishMissingBaselines()
    {
        if (Find.IdeoManager == null)
        {
            return;
        }

        foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading.Where(ideo => ideo.Fluid))
        {
            if (SnapshotFor(ideo) == null)
            {
                Capture(ideo);
            }
        }
    }
}

public sealed class IdeoUnlockSnapshot : IExposable
{
    public int ideologyId;
    public List<string> unlockedMemeDefNames = new();

    public void ExposeData()
    {
        Scribe_Values.Look(ref ideologyId, "ideologyId");
        Scribe_Collections.Look(ref unlockedMemeDefNames, "unlockedMemeDefNames", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            unlockedMemeDefNames ??= new List<string>();
        }
    }
}
