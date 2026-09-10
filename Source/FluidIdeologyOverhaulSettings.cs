using Verse;

namespace FluidIdeologyOverhaul;

public enum PreservationMode
{
    VanillaCascades,
    PreserveWherePossible,
    StrictPreservation
}

public sealed class FluidIdeologyOverhaulSettings : ModSettings
{
    public PreservationMode PreservationMode = PreservationMode.PreserveWherePossible;
    public bool DevelopmentDiagnostics;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref PreservationMode, "preservationMode", PreservationMode.PreserveWherePossible);
        Scribe_Values.Look(ref DevelopmentDiagnostics, "developmentDiagnostics", false);
    }
}

internal static class PreservationModeExtensions
{
    public static PreservationMode Next(this PreservationMode mode)
    {
        return mode switch
        {
            PreservationMode.VanillaCascades => PreservationMode.PreserveWherePossible,
            PreservationMode.PreserveWherePossible => PreservationMode.StrictPreservation,
            _ => PreservationMode.VanillaCascades
        };
    }

    public static string Label(this PreservationMode mode) => mode switch
    {
        PreservationMode.VanillaCascades => "FIO_ModeVanilla".Translate(),
        PreservationMode.StrictPreservation => "FIO_ModeStrict".Translate(),
        _ => "FIO_ModePreserve".Translate()
    };

    public static string Description(this PreservationMode mode) => mode switch
    {
        PreservationMode.VanillaCascades => "FIO_ModeVanillaTip".Translate(),
        PreservationMode.StrictPreservation => "FIO_ModeStrictTip".Translate(),
        _ => "FIO_ModePreserveTip".Translate()
    };
}

