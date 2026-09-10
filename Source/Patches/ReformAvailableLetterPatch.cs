using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace FluidIdeologyOverhaul.Patches;

[HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.TryAddDevelopmentPoints))]
internal static class ReformAvailableLetterPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldstr && instruction.operand as string == "LetterTextReformIdeo")
            {
                instruction.operand = "FIO_LetterTextReformIdeo";
            }

            yield return instruction;
        }
    }
}
