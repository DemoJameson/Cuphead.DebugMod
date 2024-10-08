using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FlowerPatternSelector : PluginComponent {

    // Sets the leading attack in the sequence.
    [HarmonyPatch(typeof(FlowerLevel), nameof(FlowerLevel.Start))]
    [HarmonyPrefix]
    public static void PhaseGenericFirstSubPhasePatternManipulator(ref FlowerLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal && FlowerPhaseGeneric1PatternNormal.Value != FlowerPhaseGeneric1PatternsNormal.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlowerPhaseGeneric1PatternsNormal>((int) FlowerPhaseGeneric1PatternNormal.Value);
        }
    }

    [HarmonyPatch(typeof(FlowerLevel), nameof(FlowerLevel.OnStateChanged))]
    [HarmonyPrefix]
    // pattern index seems to get regenerated in subphases 2 and 3
    public static void PhaseGenericSubsequentSubPhasesPatternManipulator(ref FlowerLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.89f, 0.68f, __instance)) {
                if (FlowerPhaseGeneric2PatternNormal.Value != FlowerPhaseGeneric2PatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlowerPhaseGeneric2PatternsNormal>((int) FlowerPhaseGeneric2PatternNormal.Value);
                }
            }
            if (IsWithinPhase(0.68f, 0.46f, __instance)) {
                if (FlowerPhaseGeneric3PatternNormal.Value != FlowerPhaseGeneric3PatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<FlowerPhaseGeneric3PatternsNormal>((int) FlowerPhaseGeneric3PatternNormal.Value);
                }
            }
        }
    }

    [HarmonyPatch(typeof(FlowerLevel), nameof(FlowerLevel.Start))]
    [HarmonyPostfix]
    public static void PhaseGenericHeadLungePatternManipulator(ref FlowerLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal && FlowerPhaseGenericHeadLungePatternNormal.Value != FlowerPhaseGenericHeadLungePatternsNormal.Random) {
            __instance.flower.currentLaserAttack = (int) FlowerPhaseGenericHeadLungePatternNormal.Value - 1;
        }
    }

    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, FlowerLevel __instance) {
        return (__instance.properties.CurrentState.stateName == LevelProperties.Flower.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }



}