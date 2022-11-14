using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using UnityEngine;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FrogsPatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(FrogsLevel), nameof(FrogsLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOnePatternManipulator(ref FrogsLevel __instance) {
        if (Settings.FrogsPhaseOnePattern.Value == FrogsPhaseOnePatterns.Punches) {
            __instance.properties.CurrentState.patternIndex = 1;
        } else if (Settings.FrogsPhaseOnePattern.Value == FrogsPhaseOnePatterns.Fireflies) {
            __instance.properties.CurrentState.patternIndex = 0;
        }
        Logger.LogInfo((int) Settings.FrogsPhaseFinalPattern.Value - 1);
    }

    // Forces the attack to be the selected one every time.

    [HarmonyPatch(typeof(FrogsLevelMorphed), nameof(FrogsLevelMorphed.pattern_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PhaseFinalPatternForcer(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<mode>"))) {
            // I am checking for Settings.FrogsPhaseFinalPattern.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.FrogsPhaseFinalPattern.Value
            ilCursor.EmitDelegate<Func<int, int>>(mode =>
                Settings.FrogsPhaseFinalPattern.Value == FrogsPhaseFinalPatterns.Random ?
                UnityEngine.Random.Range(0, 3)
                :
                (int) Settings.FrogsPhaseFinalPattern.Value - 1
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }
}
