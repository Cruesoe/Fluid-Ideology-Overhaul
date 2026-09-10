using IdeologyReformation.Tech;
using IdeologyReformation.Patches;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace IdeologyReformation;

public sealed class IdeologyReformationMod : Mod
{
    public const string HarmonyId = "cruesoe.ideologyreformation";

    public static IdeologyReformationSettings Settings { get; private set; } = null!;

    public IdeologyReformationMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<IdeologyReformationSettings>();
        new Harmony(HarmonyId).PatchAll();
        FluidIdeologyDescriptionPatch.ApplyAfterLoading();
#if DEBUG
        Log.Message("[Ideology Reformation] Phase 1 loaded.");
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
