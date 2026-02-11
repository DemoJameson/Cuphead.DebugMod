using System.Collections.Generic;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;


namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class RumRunnersPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(RumRunnersLevelAnteater), nameof(RumRunnersLevelAnteater.LevelInit))]
    [HarmonyPostfix]
    private static void SnoutPositionManipulator(ref RumRunnersLevelAnteater __instance) {
        if (Settings.RunRunnersSnoutPosition.Value != RumRunnersSnoutPositions.Random) {
            __instance.snoutPositionPattern.subIndex = Utility.GetUserPattern<RumRunnersSnoutPositions>((int) Settings.RunRunnersSnoutPosition.Value);
        }
    }
}
