using System.Collections.Generic;
using System.Reflection;
using FluidIdeologyOverhaul.Tech;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluidIdeologyOverhaul.Patches;

[StaticConstructorOnStartup]
internal static class MemeResearchUnlockRegistrar
{
    static MemeResearchUnlockRegistrar()
    {
        foreach (MemeDef meme in DefDatabase<MemeDef>.AllDefsListForReading)
        {
            MemeTechRequirement? requirement = MemeTechUtility.RequirementFor(meme);
            if (requirement == null)
            {
                continue;
            }

            foreach (ResearchProjectDef project in requirement.researchPrerequisites)
            {
                project.customUnlockTexts ??= new List<string>();
                string unlockText = "FIO_ResearchUnlockMeme".Translate(meme.LabelCap);
                if (!project.customUnlockTexts.Contains(unlockText))
                {
                    project.customUnlockTexts.Add(unlockText);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Dialog_ChooseMemes), "DrawMeme")]
internal static class MemeTechChoicePatch
{
    private static readonly FieldInfo NewMemesField = AccessTools.Field(typeof(Dialog_ChooseMemes), "newMemes");
    private static readonly FieldInfo IdeoField = AccessTools.Field(typeof(Dialog_ChooseMemes), "ideo");
    private static readonly FieldInfo ReformingIdeoField = AccessTools.Field(typeof(Dialog_ChooseMemes), "reformingIdeo");

    [HarmonyPrefix]
    private static bool Prefix(Dialog_ChooseMemes __instance, MemeDef meme, Rect memeBox, bool drawHighlight)
    {
        // Existing memes remain removable even if a requirement is introduced
        // later. Research only blocks deliberately adding a locked meme.
        List<MemeDef> selected = (List<MemeDef>)NewMemesField.GetValue(__instance);
        Ideo ideo = (Ideo)IdeoField.GetValue(__instance);
        if (!ideo.Fluid)
        {
            return true;
        }

        bool duringFluidCreation = ideo.Fluid && !(bool)ReformingIdeoField.GetValue(__instance);
        if (selected.Contains(meme)
            || !MemeTechUtility.IsLocked(meme, duringFluidCreation))
        {
            return true;
        }

        Color oldColor = GUI.color;
        GUI.color = Color.gray;
        if (!drawHighlight)
        {
            Widgets.DrawLightHighlight(memeBox);
        }
        IdeoUIUtility.DoMeme(memeBox, meme, null, IdeoEditMode.None, drawHighlight);
        GUI.color = oldColor;

        TooltipHandler.TipRegion(memeBox, MemeTechUtility.LockedReason(meme, duringFluidCreation));
        return false;
    }
}
