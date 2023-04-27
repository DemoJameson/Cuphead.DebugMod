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
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class BeePatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(BeeLevel), nameof(BeeLevel.OnStateChanged))]
    [HarmonyPrefix]

    public static void PhaseTwoFirstPatternManipulator(ref BeeLevel __instance) {
        if (__instance.properties.CurrentState.stateName == LevelProperties.Bee.States.Generic) {
            if (Level.ScoringData.difficulty == Level.Mode.Easy && IsWithinPhase(0.75f, 0.45f, __instance)) {
                if (BeePhaseTwoPatternEasy.Value != BeePhaseTwoPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<BeePhaseTwoPatternsEasy>((int) BeePhaseTwoPatternEasy.Value);
                }
            } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
                if (BeePhaseTwoPatternNormal.Value != BeePhaseTwoPatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<BeePhaseTwoPatternsNormal>((int) BeePhaseTwoPatternNormal.Value);
                }
            } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
                if (BeePhaseTwoPatternHard.Value != BeePhaseTwoPatternsHard.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<BeePhaseTwoPatternsHard>((int) BeePhaseTwoPatternHard.Value);
                }
            }
        }
    }

    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, BeeLevel __instance) {
        // i'm not actually sure on the inclusivity of values here ( <, > vs <=, >=). i'm taking a guess.
        return (__instance.properties.CurrentState.stateName == LevelProperties.Bee.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }
}
