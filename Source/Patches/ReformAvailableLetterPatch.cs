using System.Linq;
using FluidIdeologyOverhaul.Tech;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluidIdeologyOverhaul.Patches;

[HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.TryAddDevelopmentPoints))]
internal static class ReformAvailableLetterPatch
{
    [HarmonyPrefix]
    private static bool Prefix(IdeoDevelopmentTracker __instance, int pointsToAdd, ref bool __result)
    {
        if (!__instance.ideo.Fluid)
        {
            return true;
        }

        if (pointsToAdd <= 0 || __instance.CanReformNow)
        {
            __result = false;
            return false;
        }

        bool couldReform = __instance.CanReformNow;
        __instance.points = Mathf.Min(
            __instance.points + pointsToAdd,
            __instance.NextReformationDevelopmentPoints);

        if (!couldReform && __instance.CanReformNow)
        {
            Find.LetterStack.ReceiveLetter(
                "LetterLabelReformIdeo".Translate(),
                BuildLetterText(__instance.ideo),
                LetterDefOf.PositiveEvent);
        }

        __result = true;
        return false;
    }

    private static TaggedString BuildLetterText(Ideo ideo)
    {
        var newlyUnlocked = Current.Game
            .GetComponent<MemeUnlockHistory>()
            .NewlyUnlockedSinceLastReform(ideo);

        TaggedString discoveries = newlyUnlocked.Any()
            ? "FIO_ReformLetterNewBeliefs".Translate(
                newlyUnlocked.Select(meme => "  - " + meme.LabelCap.ToString()).ToList().ToLineList())
            : "FIO_ReformLetterNoNewBeliefs".Translate();

        return "FIO_LetterTextReformIdeo".Translate(ideo, discoveries);
    }
}

[HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.Notify_Reformed))]
internal static class ReformUnlockSnapshotPatch
{
    [HarmonyPostfix]
    private static void Postfix(IdeoDevelopmentTracker __instance)
    {
        if (__instance.ideo.Fluid && Current.Game != null)
        {
            Current.Game.GetComponent<MemeUnlockHistory>().Capture(__instance.ideo);
        }
    }
}
