using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
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

        if ((IdeoReformStage)StageField.GetValue(__instance) == IdeoReformStage.PreceptsNarrativeAndDeities)
        {
            Rect resetRect = new Rect(
                inRect.x + (inRect.width - Window.CloseButSize.x) / 2f,
                inRect.height - Window.CloseButSize.y,
                Window.CloseButSize.x,
                Window.CloseButSize.y);
            if (Widgets.ButtonText(resetRect, "ReformIdeoResetChanges".Translate()))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                session.Reset();
                Messages.Message("FIO_ResetComplete".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }
    }

    [HarmonyPatch("ResetAllChooseOneChanges")]
    [HarmonyPrefix]
    private static bool ResetChangesPrefix(Dialog_ReformIdeo __instance)
    {
        if (!ReformSessions.TryGet(__instance, out ReformSession session))
        {
            return true;
        }

        // Take over RimWorld's existing centred Reset changes button. This restores
        // the complete opening snapshot, including precepts and cosmetic edits.
        session.Reset();
        Messages.Message("FIO_ResetComplete".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
        return false;
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

[HarmonyPatch(typeof(Widgets), nameof(Widgets.ButtonText),
    new[] { typeof(Rect), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(TextAnchor?) })]
internal static class ReformEditorButtonPatch
{
    [HarmonyPrefix]
    private static bool Prefix(string label, ref bool active, ref bool __result)
    {
        if (Find.WindowStack.currentlyDrawnWindow is not Dialog_ReformIdeo dialog
            || !ReformSessions.TryGet(dialog, out ReformSession session))
        {
            return true;
        }

        if (label == "Randomize".Translate() || label == "RandomizePrecepts".Translate())
        {
            // Randomizing memes or precepts wholesale is incompatible with the
            // one-deliberate-object rule at any lock state, so remove the button
            // outright instead of just disabling it.
            __result = false;
            return false;
        }

        if (session.SelectedObject != null && IsAddPreceptLabel(label))
        {
            active = false;
        }

        return true;
    }

    private static bool IsAddPreceptLabel(string label)
    {
        return IsAddLabel(label, "Precept".Translate())
            || IsAddLabel(label, "Role".Translate())
            || IsAddLabel(label, "Ritual".Translate())
            || IsAddLabel(label, "IdeoBuilding".Translate())
            || IsAddLabel(label, "IdeoRelic".Translate())
            || IsAddLabel(label, "IdeoWeapon".Translate())
            || IsAddLabel(label, "Animal".Translate())
            || IsAddLabel(label, "Xenotype".Translate().ToString().UncapitalizeFirst())
            || IsAddLabel(label, "IdeoApparelDesire".Translate());
    }

    private static bool IsAddLabel(string label, string objectLabel)
    {
        return label == ("AddPrecept".Translate(objectLabel).CapitalizeFirst() + "...");
    }
}

/// <summary>
/// A precept float menu can remain open while another reform object becomes locked.
/// Stop its vanilla callback before it constructs a detached precept and attempts to
/// configure it, which is unsafe after Ideo.AddPrecept has rejected the mutation.
/// </summary>
[HarmonyPatch(typeof(FloatMenuOption), nameof(FloatMenuOption.Chosen))]
internal static class LockedPreceptMenuOptionPatch
{
    [HarmonyPrefix]
    private static bool Prefix(FloatMenuOption __instance)
    {
        if (__instance.action == null || !IsIdeoAddPreceptAction(__instance.action.Method))
        {
            return true;
        }

        Dialog_ReformIdeo? dialog = Find.WindowStack.Windows.OfType<Dialog_ReformIdeo>().LastOrDefault();
        if (dialog == null
            || !ReformSessions.TryGet(dialog, out ReformSession session)
            || session.SelectedObject == null)
        {
            return true;
        }

        Messages.Message(
            "FIO_AnotherObjectLocked".Translate(session.SelectedObject.Label),
            MessageTypeDefOf.RejectInput,
            historical: false);
        return false;
    }

    private static bool IsIdeoAddPreceptAction(MethodInfo method)
    {
        Type? type = method.DeclaringType;
        while (type != null && type != typeof(IdeoUIUtility))
        {
            type = type.DeclaringType;
        }

        return type == typeof(IdeoUIUtility) && method.Name.Contains("AddPrecept");
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
