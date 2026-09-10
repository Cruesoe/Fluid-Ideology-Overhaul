using System.Collections.Generic;
using RimWorld;

namespace IdeologyReformation.Reform;

internal static class ReformSessions
{
    private static readonly Dictionary<Ideo, ReformSession> ByWorkingIdeo = new();
    private static readonly Dictionary<Dialog_ReformIdeo, ReformSession> ByDialog = new();

    public static ReformSession Begin(Dialog_ReformIdeo dialog, Ideo original, Ideo working)
    {
        ReformSession session = new ReformSession(dialog, original, working);
        ByWorkingIdeo[working] = session;
        ByDialog[dialog] = session;
        Diagnostics.Message($"Opened reform session for {original.name}.");
        return session;
    }

    public static bool TryGet(Ideo working, out ReformSession session) => ByWorkingIdeo.TryGetValue(working, out session!);
    public static bool TryGet(Dialog_ReformIdeo dialog, out ReformSession session) => ByDialog.TryGetValue(dialog, out session!);

    public static void End(Dialog_ReformIdeo dialog)
    {
        if (!ByDialog.TryGetValue(dialog, out ReformSession? session))
        {
            return;
        }

        ByDialog.Remove(dialog);
        ByWorkingIdeo.Remove(session.Working);
        Diagnostics.Message($"Closed reform session for {session.Original.name}.");
    }
}

internal static class MutationScope
{
    [System.ThreadStatic]
    private static int consequenceDepth;

    [System.ThreadStatic]
    private static int preceptMutationDepth;

    public static bool IsResolvingConsequences => consequenceDepth > 0;
    public static bool IsNestedPreceptMutation => preceptMutationDepth > 0;
    public static void EnterConsequences() => consequenceDepth++;
    public static void ExitConsequences() => consequenceDepth = System.Math.Max(0, consequenceDepth - 1);
    public static void EnterPreceptMutation() => preceptMutationDepth++;
    public static void ExitPreceptMutation() => preceptMutationDepth = System.Math.Max(0, preceptMutationDepth - 1);
}

internal static class Diagnostics
{
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Message(string message)
    {
        Verse.Log.Message("[Ideology Reformation] " + message);
    }
}
