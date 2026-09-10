using HarmonyLib;
using UnityEngine;
using Verse;

namespace FluidIdeologyOverhaul;

public sealed class FluidIdeologyOverhaulMod : Mod
{
    public const string HarmonyId = "cruesoe.fluidideologyoverhaul";

    public static FluidIdeologyOverhaulSettings Settings { get; private set; } = null!;

    public FluidIdeologyOverhaulMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<FluidIdeologyOverhaulSettings>();
        new Harmony(HarmonyId).PatchAll();
#if DEBUG
        Log.Message("[Fluid Ideology Overhaul] Phase 1 loaded.");
#endif
    }

    public override string SettingsCategory() => "FIO_Title".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.Label("FIO_PreservationMode".Translate());

        Rect modeRect = listing.GetRect(34f);
        if (Widgets.ButtonText(modeRect, Settings.PreservationMode.Label()))
        {
            Settings.PreservationMode = Settings.PreservationMode.Next();
        }
        TooltipHandler.TipRegion(modeRect, Settings.PreservationMode.Description());

        listing.Gap();
        listing.CheckboxLabeled(
            "FIO_DevelopmentDiagnostics".Translate(),
            ref Settings.DevelopmentDiagnostics,
            "FIO_DevelopmentDiagnosticsTip".Translate());
        listing.End();
    }
}
