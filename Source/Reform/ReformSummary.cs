using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FluidIdeologyOverhaul.Reform;

internal sealed class ReformSummary
{
    public string Primary { get; private set; } = string.Empty;
    public List<string> PrimaryChanges { get; } = new();
    public List<string> Consequences { get; } = new();
    public string Preserved { get; private set; } = string.Empty;
    public List<string> CosmeticChanges { get; } = new();

    public string PrimaryText => JoinSection(Primary, PrimaryChanges);
    public string ConsequenceText => Consequences.Count == 0
        ? "FIO_NoConsequences".Translate()
        : BulletLines(Consequences);
    public string CosmeticText => BulletLines(CosmeticChanges);

    public static ReformSummary Build(ReformSession session)
    {
        ReformSummary summary = new ReformSummary
        {
            Primary = session.SelectedObject?.Label ?? "FIO_CosmeticOnly".Translate()
        };

        List<Precept> originalPrecepts = session.Original.PreceptsListForReading;
        List<Precept> workingPrecepts = session.Working.PreceptsListForReading;
        HashSet<int> originalIds = originalPrecepts.Select(p => p.Id).ToHashSet();
        HashSet<int> workingIds = workingPrecepts.Select(p => p.Id).ToHashSet();
        List<Precept> added = workingPrecepts.Where(p => !originalIds.Contains(p.Id)).ToList();
        List<Precept> removed = originalPrecepts.Where(p => !workingIds.Contains(p.Id)).ToList();

        AddPrimaryDetails(summary, session, originalPrecepts, workingPrecepts, added, removed);

        foreach (Precept precept in added.Where(p => !IsPrimaryPrecept(session.SelectedObject, p)))
        {
            summary.Consequences.Add("FIO_AddedItem".Translate(Describe(precept)));
        }

        foreach (Precept precept in removed.Where(p => !IsPrimaryPrecept(session.SelectedObject, p)))
        {
            summary.Consequences.Add("FIO_RemovedItem".Translate(Describe(precept)));
        }

        int preserved = originalPrecepts.Count(p => workingIds.Contains(p.Id));
        summary.Preserved = "FIO_PreservedCount".Translate(preserved, originalPrecepts.Count);
        AddCosmeticDetails(summary, session.Original, session.Working);
        return summary;
    }

    private static void AddPrimaryDetails(
        ReformSummary summary,
        ReformSession session,
        List<Precept> originalPrecepts,
        List<Precept> workingPrecepts,
        List<Precept> added,
        List<Precept> removed)
    {
        ReformObject? selected = session.SelectedObject;
        if (selected == null)
        {
            return;
        }

        if (selected.Kind == ReformObjectKind.Meme)
        {
            foreach (MemeDef meme in session.Working.memes.Where(m => m.category == MemeCategory.Normal && !session.Original.memes.Contains(m)))
            {
                summary.PrimaryChanges.Add("FIO_AddedMeme".Translate(meme.LabelCap));
            }
            foreach (MemeDef meme in session.Original.memes.Where(m => m.category == MemeCategory.Normal && !session.Working.memes.Contains(m)))
            {
                summary.PrimaryChanges.Add("FIO_RemovedMeme".Translate(meme.LabelCap));
            }
        }
        else if (selected.Kind == ReformObjectKind.Structure)
        {
            MemeDef? before = session.Original.StructureMeme;
            MemeDef? after = session.Working.StructureMeme;
            summary.PrimaryChanges.Add("FIO_ChangedFromTo".Translate(before?.LabelCap ?? "None".Translate(), after?.LabelCap ?? "None".Translate()));
        }
        else
        {
            foreach (Precept precept in removed.Where(p => IsPrimaryPrecept(selected, p)))
            {
                summary.PrimaryChanges.Add("FIO_ChangedFrom".Translate(Describe(precept)));
            }
            foreach (Precept precept in added.Where(p => IsPrimaryPrecept(selected, p)))
            {
                summary.PrimaryChanges.Add("FIO_ChangedTo".Translate(Describe(precept)));
            }

            foreach (Precept before in originalPrecepts.Where(p => IsPrimaryPrecept(selected, p)))
            {
                Precept? after = workingPrecepts.FirstOrDefault(p => p.Id == before.Id);
                if (after != null)
                {
                    AddSamePreceptDetails(summary.PrimaryChanges, before, after);
                }
            }
        }

        if (summary.PrimaryChanges.Count == 0)
        {
            summary.PrimaryChanges.Add("FIO_SelectedObjectEdited".Translate());
        }
    }

    private static void AddSamePreceptDetails(List<string> changes, Precept before, Precept after)
    {
        if (before is Precept_Ritual oldRitual && after is Precept_Ritual newRitual)
        {
            if (oldRitual.isAnytime != newRitual.isAnytime)
            {
                changes.Add("FIO_RitualTiming".Translate(
                    oldRitual.isAnytime ? "FIO_Anytime".Translate() : "FIO_Scheduled".Translate(),
                    newRitual.isAnytime ? "FIO_Anytime".Translate() : "FIO_Scheduled".Translate()));
            }

            int? oldDate = oldRitual.obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault()?.triggerDaysSinceStartOfYear;
            int? newDate = newRitual.obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault()?.triggerDaysSinceStartOfYear;
            if (oldDate != newDate)
            {
                changes.Add("FIO_RitualDate".Translate(oldDate?.ToString() ?? "-", newDate?.ToString() ?? "-"));
            }

            if (oldRitual.attachableOutcomeEffect != newRitual.attachableOutcomeEffect)
            {
                changes.Add("FIO_RitualReward".Translate(
                    oldRitual.attachableOutcomeEffect?.LabelCap ?? "None".Translate(),
                    newRitual.attachableOutcomeEffect?.LabelCap ?? "None".Translate()));
            }
        }

        int oldApparel = before.ApparelRequirements?.Count() ?? 0;
        int newApparel = after.ApparelRequirements?.Count() ?? 0;
        if (oldApparel != newApparel)
        {
            changes.Add("FIO_ApparelRequirements".Translate(oldApparel, newApparel));
        }

        if (before is Precept_Relic oldRelic && after is Precept_Relic newRelic && oldRelic.stuff != newRelic.stuff)
        {
            changes.Add("FIO_RelicMaterial".Translate(oldRelic.stuff?.LabelCap ?? "-", newRelic.stuff?.LabelCap ?? "-"));
        }
    }

    private static void AddCosmeticDetails(ReformSummary summary, Ideo original, Ideo working)
    {
        if (original.name != working.name)
        {
            summary.CosmeticChanges.Add("FIO_NameChanged".Translate(original.name, working.name));
        }
        if (original.description != working.description)
        {
            summary.CosmeticChanges.Add("FIO_DescriptionChanged".Translate());
        }
        if (original.adjective != working.adjective || original.memberName != working.memberName)
        {
            summary.CosmeticChanges.Add("FIO_NarrativeTermsChanged".Translate());
        }
        if (original.iconDef != working.iconDef || original.colorDef != working.colorDef)
        {
            summary.CosmeticChanges.Add("FIO_IconColorChanged".Translate());
        }
        if (original.leaderTitleMale != working.leaderTitleMale || original.leaderTitleFemale != working.leaderTitleFemale)
        {
            summary.CosmeticChanges.Add("FIO_LeaderTitlesChanged".Translate());
        }
        if (!original.thingStyleCategories.SetsEqual(working.thingStyleCategories))
        {
            summary.CosmeticChanges.Add("FIO_StylesChanged".Translate());
        }

        Dictionary<int, Precept> byId = working.PreceptsListForReading.ToDictionary(p => p.Id);
        foreach (Precept before in original.PreceptsListForReading)
        {
            if (byId.TryGetValue(before.Id, out Precept? after) && before.Label != after.Label)
            {
                summary.CosmeticChanges.Add("FIO_PreceptNameChanged".Translate(before.Label, after.Label));
            }
        }
    }

    private static bool IsPrimaryPrecept(ReformObject? selected, Precept precept)
    {
        return selected != null && selected.Equals(ReformObject.ForPrecept(precept));
    }

    private static string Describe(Precept precept)
    {
        string firstLine = precept.UIInfoFirstLine;
        string secondLine = precept.UIInfoSecondLine;
        if (secondLine.NullOrEmpty() || firstLine == secondLine)
        {
            return firstLine.NullOrEmpty() ? precept.LabelCap : firstLine;
        }

        return $"{firstLine}: {secondLine}";
    }

    private static string JoinSection(string firstLine, List<string> details)
    {
        return details.Count == 0 ? firstLine : firstLine + "\n" + BulletLines(details);
    }

    private static string BulletLines(IEnumerable<string> lines)
    {
        return string.Join("\n", lines.Select(line => "  - " + line));
    }
}
