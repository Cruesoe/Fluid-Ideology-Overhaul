using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FluidIdeologyOverhaul.Tech;

/// <summary>
/// Data-driven research requirements for a meme. Other mods can attach this
/// extension to their own MemeDefs without taking a code dependency on FIO.
/// </summary>
public sealed class MemeTechRequirement : DefModExtension
{
    public List<ResearchProjectDef> researchPrerequisites = new();

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
        {
            yield return error;
        }

        if (researchPrerequisites.NullOrEmpty())
        {
            yield return "MemeTechRequirement must specify at least one research prerequisite.";
        }
    }
}

internal static class MemeTechUtility
{
    private static readonly FieldInfo PlayerFactionDefField =
        AccessTools.Field(typeof(ScenPart_PlayerFaction), "factionDef");
    private static readonly FieldInfo StartingResearchProjectField =
        AccessTools.Field(typeof(ScenPart_StartingResearch), "project");

    public static MemeTechRequirement? RequirementFor(MemeDef meme)
    {
        return meme.GetModExtension<MemeTechRequirement>();
    }

    public static IEnumerable<MemeDef> ResearchUnlockedMemes()
    {
        if (Current.Game == null || Find.ResearchManager == null)
        {
            return Enumerable.Empty<MemeDef>();
        }

        return DefDatabase<MemeDef>.AllDefsListForReading.Where(meme =>
        {
            MemeTechRequirement? requirement = RequirementFor(meme);
            return requirement != null
                && requirement.researchPrerequisites.All(project => project.IsFinished);
        });
    }

    public static bool IsLocked(MemeDef meme, bool duringFluidCreation)
    {
        MemeTechRequirement? requirement = RequirementFor(meme);
        if (requirement == null)
        {
            return false;
        }

        return requirement.researchPrerequisites.Any(project =>
            !ResearchIsAvailable(project, duringFluidCreation));
    }

    public static string LockedReason(MemeDef meme, bool duringFluidCreation)
    {
        MemeTechRequirement? requirement = RequirementFor(meme);
        if (requirement == null)
        {
            return string.Empty;
        }

        string projects = requirement.researchPrerequisites
            .Where(project => !ResearchIsAvailable(project, duringFluidCreation))
            .Select(project => project.LabelCap.ToString())
            .ToCommaList(useAnd: true);
        return "FIO_MemeRequiresResearch".Translate(projects);
    }

    private static bool ResearchIsAvailable(ResearchProjectDef project, bool duringFluidCreation)
    {
        if (duringFluidCreation)
        {
            return ScenarioStartsWith(project);
        }

        return Current.Game != null
            && Find.ResearchManager != null
            && project.IsFinished;
    }

    private static bool ScenarioStartsWith(ResearchProjectDef required)
    {
        Scenario? scenario = Find.Scenario;
        if (scenario == null)
        {
            return Current.Game != null
                && Find.ResearchManager != null
                && required.IsFinished;
        }

        ScenPart_PlayerFaction? playerFaction = scenario.AllParts
            .OfType<ScenPart_PlayerFaction>()
            .FirstOrDefault();
        FactionDef? factionDef = playerFaction == null
            ? null
            : (FactionDef?)PlayerFactionDefField.GetValue(playerFaction);

        IEnumerable<ResearchProjectDef> taggedStartingProjects =
            factionDef?.startingResearchTags.NullOrEmpty() == false
                ? DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(project =>
                    project.tags != null
                    && project.tags.Any(tag => factionDef!.startingResearchTags.Contains(tag)))
                : Enumerable.Empty<ResearchProjectDef>();

        IEnumerable<ResearchProjectDef> explicitStartingProjects = scenario.AllParts
            .OfType<ScenPart_StartingResearch>()
            .Select(part => (ResearchProjectDef?)StartingResearchProjectField.GetValue(part))
            .Where(project => project != null)
            .Cast<ResearchProjectDef>();

        return taggedStartingProjects
            .Concat(explicitStartingProjects)
            .Any(startingProject => Completes(startingProject, required, new HashSet<ResearchProjectDef>()));
    }

    private static bool Completes(
        ResearchProjectDef startingProject,
        ResearchProjectDef required,
        HashSet<ResearchProjectDef> visited)
    {
        if (startingProject == required)
        {
            return true;
        }

        if (!visited.Add(startingProject) || startingProject.prerequisites.NullOrEmpty())
        {
            return false;
        }

        return startingProject.prerequisites.Any(prerequisite =>
            Completes(prerequisite, required, visited));
    }
}
