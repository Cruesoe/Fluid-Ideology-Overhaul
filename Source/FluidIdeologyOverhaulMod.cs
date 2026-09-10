using FluidIdeologyOverhaul.Tech;
using HarmonyLib;
using RimWorld;
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
        bool useHighestResearched = Settings.TechLevelSource == TechLevelSource.HighestResearched;
        listing.CheckboxLabeled(
            "FIO_TechSourceHighestResearched".Translate(),
            ref useHighestResearched,
            "FIO_TechSourceHighestResearchedTip".Translate());
        Settings.TechLevelSource = useHighestResearched ? TechLevelSource.HighestResearched : TechLevelSource.ActualTechLevel;

        TechLevel? effectiveTechLevel = TechLevelService.EffectiveTechLevel();
        listing.Label("FIO_CurrentTechLevel".Translate(
            effectiveTechLevel.HasValue ? effectiveTechLevel.Value.ToString() : "FIO_TechLevelUnavailable".Translate()));
        listing.End();
    }
}
