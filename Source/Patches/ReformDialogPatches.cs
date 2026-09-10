using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FluidIdeologyOverhaul.Reform;

namespace FluidIdeologyOverhaul.Patches;

[HarmonyPatch(typeof(Dialog_ReformIdeo))]
internal static class ReformDialogPatches
{
    private static readonly FieldInfo WorkingIdeoField = AccessTools.Field(typeof(Dialog_ReformIdeo), "newIdeo");
    private static readonly FieldInfo StageField = AccessTools.Field(typeof(Dialog_ReformIdeo), "stage");

    [HarmonyPatch(MethodType.Constructor, typeof(Ideo))]
    [HarmonyPostfix]
    private static void ConstructorPostfix(Dialog_ReformIdeo __instance, Ideo ideo)
    {
        if (!ideo.Fluid)
        {
            return;
        }

        Ideo working = (Ideo)WorkingIdeoField.GetValue(__instance);
        ReformSessions.Begin(__instance, ideo, working);
    }

    [HarmonyPatch(nameof(Dialog_ReformIdeo.DoWindowContents))]
    [HarmonyPostfix]
    private static void DrawPostfix(Dialog_ReformIdeo __instance, Rect inRect)
    {
        if (!ReformSessions.TryGet(__instance, out ReformSession session))
        {
            return;
        }

        Text.Font = GameFont.Tiny;
        GUI.color = Color.gray;
        string status = session.SelectedObject == null
            ? "FIO_NoObjectSelected".Translate()
            : "FIO_SelectedObject".Translate(session.SelectedObject.Label);
        Widgets.Label(new Rect(inRect.x, inRect.y + 70f, inRect.width - 180f, 20f), status);
        GUI.color = Color.white;
        Text.Font = GameFont.Small;

        Rect resetRect = new Rect(inRect.xMax - 170f, inRect.y, 170f, 32f);
        if (Widgets.ButtonText(resetRect, "FIO_DiscardChanges".Translate()))
        {
            session.Reset();
            object firstStage = System.Enum.Parse(StageField.FieldType, "MemesAndStyles");
            StageField.SetValue(__instance, firstStage);
            Messages.Message("FIO_ResetComplete".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }

    [HarmonyPatch(nameof(Dialog_ReformIdeo.AnyChooseOneChanges), MethodType.Getter)]
    [HarmonyPostfix]
    private static void MechanicalChoicePostfix(Dialog_ReformIdeo __instance, ref bool __result)
    {
        if (ReformSessions.TryGet(__instance, out ReformSession session))
        {
            // Style categories are presentation-only under FIO. Keep vanilla's mutual
            // exclusion for meme/structure changes. Once any object is selected, this also
            // activates RimWorld's existing grey overlays on the other mechanical choices.
            __result = session.SelectedObject != null
                || __instance.StructureMemeChanged
                || __instance.NormalMemesChanged;
        }
    }

    [HarmonyPatch(nameof(Dialog_ReformIdeo.StylesChanged), MethodType.Getter)]
    [HarmonyPostfix]
    private static void KeepCosmeticStylesAvailable(Dialog_ReformIdeo __instance, ref bool __result)
    {
        if (ReformSessions.TryGet(__instance, out ReformSession session)
            && session.SelectedObject != null)
        {
            // Dialog_ReformIdeo uses this value only to decide whether to grey the styles
            // panel. Styles remain presentation-only even while a mechanical object is locked.
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(Window), nameof(Window.PostClose))]
internal static class ReformWindowClosePatch
{
    [HarmonyPostfix]
    private static void Postfix(Window __instance)
    {
        if (__instance is Dialog_ReformIdeo reformDialog)
        {
            ReformSessions.End(reformDialog);
        }
    }
}
