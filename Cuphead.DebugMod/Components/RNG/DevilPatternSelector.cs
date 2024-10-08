using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class DevilPatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOnePatternManipulator(ref DevilLevel __instance) {
        if (DevilPhaseOnePattern.Value != DevilPhaseOnePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseOnePatterns>((int) DevilPhaseOnePattern.Value);
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneHeadManipulator(ref DevilLevelSittingDevil __instance) {
        if (DevilPhaseOneHeadType.Value != DevilPhaseOneHeadTypes.Random) {
            __instance.isSpiderAttackNext = DevilPhaseOneHeadType.Value == DevilPhaseOneHeadTypes.Spider ? true : false;
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.dragon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneDragonDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<isLeft>"))) {
            // I am checking for Settings.DevilPhaseOneDragonDirection.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.DevilPhaseOneDragonDirection.Value
            ilCursor.EmitDelegate<Func<bool, bool>>(isLeft =>
                DevilPhaseOneDragonDirection.Value == DevilPhaseOneDragonDirections.Random ?
                Rand.Bool()
                :
                DevilPhaseOneDragonDirection.Value == DevilPhaseOneDragonDirections.Left
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneSpiderOffsetManipulator(ref DevilLevelSittingDevil __instance) {
        if (DevilPhaseOneSpiderOffset.Value != DevilPhaseOneSpiderOffsets.Random) {
            __instance.spiderOffsetIndex = Utility.GetUserPattern<DevilPhaseOneSpiderOffsets>((int) DevilPhaseOneSpiderOffset.Value);
        }
    }

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.spider_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneSpiderHopCountManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<numAttacks>"))) {
            ilCursor.EmitDelegate<Func<int, int>>(numAttacks =>
                DevilPhaseOneSpiderHopCount.Value == DevilPhaseOneSpiderHopCounts.Random ?
                // spider hop count is always between 3 and 5 on every difficulty so it can be hardcoded here for simplicity
                UnityEngine.Random.Range(3, 6)
                :
                (int) DevilPhaseOneSpiderHopCount.Value + 2
            );
            ;
            ilCursor.Index++; // avoid infinite loops
        }
    }


    // The pitchfork attack gets decided in a bit of a peculiar way.
    // There are 3 pattern strings in LevelProperties.
    // The game randomly picks from one of these 3 pattern strings, and then also randomly picks an index to start from.
    // The game then follow the list in order.
    // So, I decided to dynamically look for the pattern i want inside the pattern string that got chosen...
    // ...then set the index appropriately.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOnePitchforkManipulator(ref DevilLevelSittingDevil __instance) {
        for (int i = 0; i < __instance.pitchforkPattern.Length; i++) {
            int.TryParse(__instance.pitchforkPattern[i], out var pitchforkAttack);
            if (pitchforkAttack == (int) DevilPhaseOnePitchforkType.Value + 3) {
                if (i == 0) {
                    __instance.pitchforkPatternIndex = __instance.pitchforkPattern.Length - 1;
                } else {
                    __instance.pitchforkPatternIndex = i - 1;
                }
            }
        }
    }

    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void PhaseTwoPatternManipulator(ref DevilLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (DevilPhaseTwoPatternNormal.Value != DevilPhaseTwoPatternsNormal.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseTwoPatternsNormal>((int) DevilPhaseTwoPatternNormal.Value);
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (DevilPhaseTwoPatternHard.Value != DevilPhaseTwoPatternsHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseTwoPatternsHard>((int) DevilPhaseTwoPatternHard.Value);
            }
        }

    }

    [HarmonyPatch(typeof(DevilLevelGiantHead), nameof(DevilLevelGiantHead.eye_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoBombEyeDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("bombOnLeft"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(bombOnLeft =>
                DevilPhaseTwoBombEyeDirection.Value == DevilPhaseTwoBombEyeDirections.Random ?
                Rand.Bool()
                :
                DevilPhaseTwoBombEyeDirection.Value == DevilPhaseTwoBombEyeDirections.Left
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }
}

