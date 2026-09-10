using RimWorld;
using Verse;

namespace FluidIdeologyOverhaul.Reform;

internal sealed class ReformSession
{
    public Dialog_ReformIdeo Dialog { get; }
    public Ideo Original { get; }
    public Ideo Working { get; }
    public ReformObject? SelectedObject { get; private set; }
    public bool ReviewApproved { get; set; }
    public bool CommitStarted { get; private set; }

    public ReformSession(Dialog_ReformIdeo dialog, Ideo original, Ideo working)
    {
        Dialog = dialog;
        Original = original;
        Working = working;
    }

    public bool TrySelect(ReformObject reformObject, bool notify = true)
    {
        if (SelectedObject == null)
        {
            SelectedObject = reformObject;
            Diagnostics.Message($"Selected reform object: {reformObject.Key}");
            return true;
        }

        if (SelectedObject.Equals(reformObject))
        {
            return true;
        }

        if (notify)
        {
            Messages.Message(
                "FIO_AnotherObjectLocked".Translate(SelectedObject.Label),
                MessageTypeDefOf.RejectInput,
                historical: false);
        }
        Diagnostics.Message($"Rejected second reform object {reformObject.Key}; locked to {SelectedObject.Key}");
        return false;
    }

    public void Reset()
    {
        Original.CopyTo(Working);
        SelectedObject = null;
        ReviewApproved = false;
        CommitStarted = false;
        Diagnostics.Message("Reform session reset to opening snapshot.");
    }

    public bool TryBeginCommit()
    {
        if (CommitStarted)
        {
            Diagnostics.Message("Ignored duplicate reform commit callback.");
            return false;
        }

        CommitStarted = true;
        return true;
    }
}

