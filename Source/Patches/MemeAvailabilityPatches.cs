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
