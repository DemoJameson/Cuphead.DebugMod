using System;
using System.IO;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class BeePatternSelector : PluginComponent {

    static int platformIndex = 0;
    static int platformChunkIndex = 3;

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

    [HarmonyPatch(typeof(BeeLevelQueen), nameof(BeeLevelQueen.follower_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoOrbsManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("PlusOrMinus"))) {
            ilCursor.EmitDelegate<Func<int, int>>(direction => GetOrbsDirection(direction));
            break; // avoid infinite loops
        }
    }

    static int GetOrbsDirection(int direction) {
        if (BeePhaseTwoOrbsDirection.Value != BeePhaseTwoOrbsDirections.Random) {
            return BeePhaseTwoOrbsDirection.Value == BeePhaseTwoOrbsDirections.Left ? -1 : 1;
        }
        return direction;
    }

    [HarmonyPatch(typeof(BeeLevelQueen), nameof(BeeLevelQueen.triangle_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoTrianglessManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("PlusOrMinus"))) {
            ilCursor.EmitDelegate<Func<int, int>>(direction => GetTrianglesDirection(direction));
            break; // avoid infinite loops
        }
    }

    static int GetTrianglesDirection(int direction) {
        if (BeePhaseTwoTrianglesDirection.Value != BeePhaseTwoTrianglesDirections.Random) {
            return BeePhaseTwoTrianglesDirection.Value == BeePhaseTwoTrianglesDirections.Left ? -1 : 1;
        }
        return direction;
    }


    [HarmonyPatch(typeof(BeeLevel), nameof(BeeLevel.Start))]
    [HarmonyPrefix]
    public static void InitializePlatformIndexes (ref BeeLevel __instance) {
        platformChunkIndex = 0;
        platformIndex = 3;
    }

    [HarmonyPatch(typeof(BeeLevelPlatforms), nameof(BeeLevelPlatforms.Randomize))]
    [HarmonyILManipulator]
    public static void PlatformManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random"))) {
            ilCursor.EmitDelegate<Func<int, int>>(platform => GetPlatformPattern(platform));
            break;
        }
    }

    static int GetPlatformPattern(int platform) {
        if (!Settings.BeeMissingPlatformPattern.Value) {
            return platform;
        }

        string[] lines = File.ReadAllLines("BeePlatforms.txt");
        int lineIndex = platformChunkIndex * 4 + platformIndex;

        if (lineIndex > lines.Length) {
            return platform;
        }

        Debug.Log("platformIndex: " + platformIndex);
        Debug.Log("platformChunkIndex: " + platformChunkIndex);
        if (!int.TryParse(lines[lineIndex], out int returnValue)) {
            Debug.Log("Couldn't parse int at line index " + platformIndex);
        }
        platformIndex--;
        if (platformIndex < 0) {
            platformIndex = 3;
            platformChunkIndex++;
        }

        UnityEngine.Debug.Log(returnValue);
        return returnValue;
    }

    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, BeeLevel __instance) {
        // i'm not actually sure on the inclusivity of values here ( <, > vs <=, >=). i'm taking a guess.
        return (__instance.properties.CurrentState.stateName == LevelProperties.Bee.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }
}
