using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FlyingBirdPatternSelector : PluginComponent {

    // Sets the leading attack in the sequence.
    [HarmonyPatch(typeof(FlyingBirdLevel), nameof(FlyingBirdLevel.Start))]
    [HarmonyPrefix]
    public static void PhaseOnePatternManipulator(ref FlyingBirdLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy && FlyingBirdPhaseOnePatternEasy.Value != FlyingBirdPhaseOnePatternsEasy.Random) {
            int userPattern = (int) FlyingBirdPhaseOnePatternEasy.Value;
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseOnePatternsEasy>(userPattern);
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal && FlyingBirdPhaseOnePatternNormal.Value != FlyingBirdPhaseOnePatternsNormal.Random) {
            int userPattern = (int) FlyingBirdPhaseOnePatternNormal.Value;
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseOnePatternsNormal>(userPattern);
        } else if (Level.ScoringData.difficulty == Level.Mode.Hard && FlyingBirdPhaseOnePatternHard.Value != FlyingBirdPhaseOnePatternsHard.Random) {
            int userPattern = (int) FlyingBirdPhaseOnePatternHard.Value;
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseOnePatternsHard>(userPattern);
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevel), nameof(FlyingBirdLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void SubsequentPhasesPatternManipulator(ref FlyingBirdLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.85f, 0.5f)) {
                if (FlyingBirdPhaseTwoPatternEasy.Value != FlyingBirdPhaseTwoPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseTwoPatternsEasy>((int) FlyingBirdPhaseTwoPatternEasy.Value);
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.9f, 0.75f)) {
                if (FlyingBirdPhaseTwoPatternNormal.Value != FlyingBirdPhaseTwoPatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseTwoPatternsNormal>((int) FlyingBirdPhaseTwoPatternNormal.Value);
                }
            }
            if (IsWithinPhase(0.29f, 0.0f)) {
                if (FlyingBirdPhaseFinalPattern.Value != FlyingBirdPhaseFinalPatterns.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseFinalPatterns>((int) FlyingBirdPhaseFinalPattern.Value);
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(0.31f, 0.0f)) {
                if (FlyingBirdPhaseFinalPattern.Value != FlyingBirdPhaseFinalPatterns.Random) {
                    int userPattern = (int) FlyingBirdPhaseFinalPattern.Value;
                    if (FlyingBirdPhaseFinalPattern.Value == FlyingBirdPhaseFinalPatterns.Garbage)
                        userPattern = userPattern + 1;
                    else
                        userPattern = userPattern - 1;
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBirdPhaseFinalPatterns>(userPattern);
                }
            }
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevelBird), nameof(FlyingBirdLevelBird.float_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<goUp>"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(goUp =>
                FlyingBirdPhaseOneDirection.Value == FlyingBirdPhaseOneDirections.Random ?
                Rand.Bool()
                :
                FlyingBirdPhaseOneDirection.Value == FlyingBirdPhaseOneDirections.Up
            );
            break;
        }
    }


    [HarmonyPatch(typeof(FlyingBirdLevelSmallBird), nameof(FlyingBirdLevelSmallBird.moveY_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseThreeDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Rand::Bool"))) {
            // I am checking for Settings.FlyingBirdPhaseFinalDirection.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.FlyingBirdPhaseFinalDirection.Value
            ilCursor.EmitDelegate<Func<bool, bool>>(movingDirection =>
                FlyingBirdPhaseThreeDirection.Value == FlyingBirdPhaseThreeDirections.Random ?
                Rand.Bool()
                :
                FlyingBirdPhaseThreeDirection.Value == FlyingBirdPhaseThreeDirections.Up
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevelBird), nameof(FlyingBirdLevelBird.stretcherMove_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseFinalDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<movingRight>"))) {
            // I am checking for Settings.FlyingBirdPhaseFinalDirection.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.FlyingBirdPhaseFinalDirection.Value
            ilCursor.EmitDelegate<Func<bool, bool>>(movingRight =>
                FlyingBirdPhaseFinalDirection.Value == FlyingBirdPhaseFinalDirections.Random ?
                Rand.Bool()
                :
                FlyingBirdPhaseFinalDirection.Value == FlyingBirdPhaseFinalDirections.Right
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }


    protected static bool IsWithinPhase(float phaseStart, float phaseEnd) {
        return
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd;
    }

}