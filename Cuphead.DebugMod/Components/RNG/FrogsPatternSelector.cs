using System;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

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


    [HarmonyPatch(typeof(FrogsLevelTall), nameof(FrogsLevelTall.fireflies_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PhaseOneFirefliesPatternForcer(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<patternString>"))) {
            ilCursor.EmitDelegate<Func<string, string>>(x => GetFirefliesPattern());
            ilCursor.Index++; // avoid infinite loops
        }
    }

    private static string GetFirefliesPattern() {
        // I am checking for Settings.ClownDashDelay.Value dynamically during this function call.
        // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
        // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.ClownDashDelay.Value
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            return Settings.FrogsPhaseOneFirefliesPatternEasy.Value == FrogsPhaseOneFirefliesPatternsEasy.Random
                ? firefliesPatternStringsEasy[UnityEngine.Random.Range(0, 3)]
                : firefliesPatternStringsEasy[(int) Settings.FrogsPhaseOneFirefliesPatternEasy.Value - 1];
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            return Settings.FrogsPhaseOneFirefliesPatternNormal.Value == FrogsPhaseOneFirefliesPatternsNormal.Random
                ? firefliesPatternStringsNormal[UnityEngine.Random.Range(0, 4)]
                : firefliesPatternStringsNormal[(int) Settings.FrogsPhaseOneFirefliesPatternNormal.Value - 1];
        } else // hard difficulty
          {
            return Settings.FrogsPhaseOneFirefliesPatternHard.Value == FrogsPhaseOneFirefliesPatternsHard.Random
                ? firefliesPatternStringsHard[UnityEngine.Random.Range(0, 7)]
                : firefliesPatternStringsHard[(int) Settings.FrogsPhaseOneFirefliesPatternHard.Value - 1];
        }

    }


    readonly static string[] firefliesPatternStringsEasy = {
        "D:1, S:2, D:1.5, S:1, D:1, S:1",
        "D:1.5, S:1, D:1.5, S:1, D:1, S:2",
        "D:0.5, S:2, D:1, S:1, D:0.5, S:1"
    };

    readonly static string[] firefliesPatternStringsNormal = {
        "D:1, S:2, D:1, S:2, D:1, S:1, D:1, S:2",
        "D:1, S:2, D:1, S:2, D:1, S:2",
        "D:1, S:2, D:1, S:1, D:1, S:2, D:1, S:2",
        "D:1, S:2, D:1, S:2, D:1, S:1"
    };

    readonly static string[] firefliesPatternStringsHard = {
        "D:1, S:2, D:2.5, S:2, D:1.5, S:1",
        "D:0.5, S:1, D:2, S:1, D:1.5, S:2",
        "D:1, S:2, D:2.2, S:2",
        "D:1, S:2, D:2, S:1, D:1, S:1",
        "D:0.5, S:1, D:2.5, S:2, D:2.1, S:2",
        "D:1.4, S:2, D:2.4, S:2",
        "D:1, S:1, D:1.8, S:2, D:1.5, S:1"
    };




}
