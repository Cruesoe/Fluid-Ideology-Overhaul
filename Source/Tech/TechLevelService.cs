using RimWorld;
using Verse;

namespace IdeologyReformation.Tech;

public static class TechLevelService
{
    public static TechLevel? EffectiveTechLevel()
    {
        if (Current.Game == null)
        {
            return null;
        }

        return IdeologyReformationMod.Settings.TechLevelSource == TechLevelSource.HighestResearched
            ? HighestResearchedTechLevel()
            : ActualTechLevel();
    }

    public static TechLevel? ActualTechLevel()
    {
        // The meme picker runs during new-game setup, before the world (and so the player
        // faction) exists.
        return Current.Game?.World == null ? null : Faction.OfPlayer?.def.techLevel;
    }

    public static TechLevel HighestResearchedTechLevel()
    {
        TechLevel highest = TechLevel.Undefined;
        foreach (ResearchProjectDef project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
        {
            if (project.techLevel > highest && project.IsFinished)
            {
                highest = project.techLevel;
            }
        }

        return highest == TechLevel.Undefined ? TechLevel.Neolithic : highest;
    }
}
