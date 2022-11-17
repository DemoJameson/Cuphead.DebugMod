using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
public class EnableDebugConsole : PluginComponent {
    [HarmonyPatch(typeof(DebugConsole), nameof(DebugConsole.Update))]
    [HarmonyILManipulator]
    private static void DebugConsoleUpdate(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        if (ilCursor.TryGotoNext(i => i.MatchCall<Debug>("get_isDebugBuild"))) {
            ilCursor.Remove().Emit(OpCodes.Ldc_I4_1);
        }
    }
}