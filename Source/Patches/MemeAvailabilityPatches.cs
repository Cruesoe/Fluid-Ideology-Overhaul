using HarmonyLib;
using RimWorld;
using IdeologyReformation.Restrictions;

namespace IdeologyReformation.Patches;

/// <summary>
/// Gates meme selectability (initial ideo creation, fluid ideo setup, and reform) behind
/// <see cref="MemeAvailabilityExtension"/> where present. Restricted memes are simply absent
/// from the picker grid, matching vanilla's own handling of hiddenInChooseMemes and
/// faction/DLC-gated memes.
/// </summary>
[HarmonyPatch(typeof(Dialog_ChooseMemes), "CanUseMeme")]
internal static class MemeAvailabilityPatch
{
    [HarmonyPostfix]
    private static void Postfix(MemeDef meme, ref bool __result)
    {
        if (__result && !MemeAvailabilityUtility.IsMemeAvailable(meme))
        {
            __result = false;
        }
    }
}

/// <summary>
/// Applies the same gate to <see cref="IdeoUtility"/>'s random meme generation. Vanilla builds
/// starting-scenario ideoligions and the "Randomize" button through this path (never through
/// <see cref="Dialog_ChooseMemes.CanUseMeme"/>), so without this patch a restricted meme could
/// still appear on a freshly generated ideoligion.
/// </summary>
[HarmonyPatch(typeof(IdeoUtility), "CanAdd")]
internal static class MemeAvailabilityRandomizationPatch
{
    [HarmonyPostfix]
    private static void Postfix(MemeDef meme, ref bool __result)
    {
        if (__result && !MemeAvailabilityUtility.IsMemeAvailable(meme))
        {
            __result = false;
        }
    }
}
