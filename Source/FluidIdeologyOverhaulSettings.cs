using Verse;

namespace FluidIdeologyOverhaul;

public enum TechLevelSource
{
    ActualTechLevel,
    HighestResearched
}

public sealed class FluidIdeologyOverhaulSettings : ModSettings
{
    private const string NodeResearchPackageId = "ferny.noderesearch";

    public TechLevelSource TechLevelSource = DefaultTechLevelSource();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref TechLevelSource, "techLevelSource", DefaultTechLevelSource());
    }

    private static TechLevelSource DefaultTechLevelSource()
    {
        return ModsConfig.IsActive(NodeResearchPackageId)
            ? TechLevelSource.ActualTechLevel
            : TechLevelSource.HighestResearched;
    }
}
