using System.Linq;
using RimWorld;
using Verse;
using IdeologyReformation.Tech;

namespace IdeologyReformation.Restrictions;

public static class MemeAvailabilityUtility
{
    public static bool IsMemeAvailable(MemeDef meme)
    {
        MemeAvailabilityExtension? restriction = meme.GetModExtension<MemeAvailabilityExtension>();
        if (restriction == null)
        {
            return true;
        }

        if (restriction.minTechLevel != TechLevel.Undefined)
        {
            TechLevel? current = TechLevelService.EffectiveTechLevel();
            if (current.HasValue && current.Value < restriction.minTechLevel)
            {
                return false;
            }
        }

        if (restriction.requiredResearch is { Count: > 0 }
            && !restriction.requiredResearch.All(project => project.IsFinished))
        {
            return false;
        }

        return true;
    }
}
