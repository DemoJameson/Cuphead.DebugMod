using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FrogsPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(FrogsLevel), nameof(FrogsLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOnePatternForcer(ref FrogsLevel __instance) {
        if (Settings.FrogsPhaseOnePattern.Value == FrogsPhaseOnePatterns.Punches) {
            __instance.properties.CurrentState.patternIndex = 1;
        } else if (Settings.FrogsPhaseOnePattern.Value == FrogsPhaseOnePatterns.Fireflies) {
            __instance.properties.CurrentState.patternIndex = 0;
        }
    }
}
