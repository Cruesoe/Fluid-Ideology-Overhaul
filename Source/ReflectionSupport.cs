using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using IdeologyReformation.Patches;
using Verse;

namespace IdeologyReformation;

/// <summary>
/// Wraps AccessTools lookups of RimWorld's private members so a renamed or removed
/// field logs a clear diagnostic at mod load instead of surfacing as a bare
/// NullReferenceException the first time an affected dialog opens.
/// </summary>
internal static class ReflectionSupport
{
    public static FieldInfo RequireField(Type type, string fieldName)
    {
        FieldInfo? field = AccessTools.Field(type, fieldName);
        if (field == null)
        {
            Log.Error($"[Ideology Reformation] Expected field '{type.FullName}.{fieldName}' was not found. " +
                "A RimWorld update likely renamed or removed it; patches depending on it will fail.");
        }

        return field!;
    }

    /// <summary>
    /// Forces the static field lookups in the Harmony patch classes to run now, at mod
    /// load, rather than lazily on first use, so any RequireField failure logs during
    /// startup instead of surfacing as a runtime NullReferenceException while playing.
    /// </summary>
    public static void EnsureFieldLookupsResolved()
    {
        RuntimeHelpers.RunClassConstructor(typeof(MemePreservationPatches).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DetailedPreceptEditPatch).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ReformDialogPatches).TypeHandle);
    }
}
