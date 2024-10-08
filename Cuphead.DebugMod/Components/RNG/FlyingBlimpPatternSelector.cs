using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FlyingBlimpPatternSelector : PluginComponent {

    // Sets the leading attack in the sequence.
    [HarmonyPatch(typeof(FlyingBlimpLevel), nameof(FlyingBlimpLevel.OnStateChanged))]
    [HarmonyPrefix]

    // pattern index seems to get regenerated in phases 2 and 3
    public static void PhaseBlimpPatternManipulator(ref FlyingBlimpLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.62f, 0.46f, __instance)) {
                if (FlyingBlimpPhaseBlimp2PatternEasy.Value != FlyingBlimpPhaseBlimp2PatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp2PatternsEasy>((int) FlyingBlimpPhaseBlimp2PatternEasy.Value);
                }
            } else if (IsWithinPhase(0.23f, 0.00f, __instance)) {
                if (FlyingBlimpPhaseBlimp3PatternNormal.Value != FlyingBlimpPhaseBlimp3PatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp3PatternsNormal>((int) FlyingBlimpPhaseBlimp3PatternNormal.Value);
                }
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.77f, 0.64f, __instance)) {
                if (FlyingBlimpPhaseBlimp2PatternNormal.Value != FlyingBlimpPhaseBlimp2PatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp2PatternsNormal>((int) FlyingBlimpPhaseBlimp2PatternNormal.Value);
                }
            } else if (IsWithinPhase(0.46f, 0.34f, __instance)) {
                if (FlyingBlimpPhaseBlimp3PatternNormal.Value != FlyingBlimpPhaseBlimp3PatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp3PatternsNormal>((int) FlyingBlimpPhaseBlimp3PatternNormal.Value);
                }
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(0.8f, 0.67f, __instance)) {
                if (FlyingBlimpPhaseBlimp2PatternHard.Value != FlyingBlimpPhaseBlimp2PatternsHard.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp2PatternsHard>((int) FlyingBlimpPhaseBlimp2PatternHard.Value);
                }
            } else if (IsWithinPhase(0.51f, 0.39f, __instance)) {
                if (FlyingBlimpPhaseBlimp3PatternHard.Value != FlyingBlimpPhaseBlimp3PatternsHard.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlyingBlimpPhaseBlimp3PatternsHard>((int) FlyingBlimpPhaseBlimp3PatternHard.Value);
                }
            }
        }
    }

    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, FlyingBlimpLevel __instance) {
        // i'm not actually sure on the inclusivity of values here ( <, > vs <=, >=). i'm taking a guess.
        return (__instance.properties.CurrentState.stateName == LevelProperties.FlyingBlimp.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }



}