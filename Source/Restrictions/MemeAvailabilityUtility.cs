using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using IdeologyReformation.Tech;

namespace IdeologyReformation.Restrictions;

public static class MemeAvailabilityUtility
{
    public static bool IsMemeAvailable(MemeDef meme)
    {
        return LockedReason(meme) == null;
    }

    /// <summary>Player-facing explanation of why this meme cannot be taken yet, or null when it can.</summary>
    public static string? LockedReason(MemeDef meme)
    {
        MemeAvailabilityExtension? restriction = meme.GetModExtension<MemeAvailabilityExtension>();
        if (restriction == null)
        {
            return null;
        }

        if (restriction.minTechLevel != TechLevel.Undefined)
        {
            TechLevel? current = TechLevelService.EffectiveTechLevel();
            if (current.HasValue && current.Value < restriction.minTechLevel)
            {
                string techLabel = ("TechLevel_" + restriction.minTechLevel).Translate();
                return "FIO_MemeLockedTechLevel".Translate(techLabel);
            }
        }

        if (restriction.requiredResearch is { Count: > 0 })
        {
            List<string> missing = restriction.requiredResearch
                .Where(project => !project.IsFinished)
                .Select(project => project.LabelCap.ToString())
                .ToList();

            if (missing.Count > 0)
            {
                return "FIO_MemeLockedResearch".Translate(missing.ToCommaList(useAnd: true));
            }
        }

        return null;
    }
}
