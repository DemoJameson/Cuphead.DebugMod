using System.Collections.Generic;
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

    [HarmonyPatch(typeof(FlowerLevelFlower), nameof(FlowerLevelFlower.Phase1IntroAudio))]
    [HarmonyPrefix]
    // the pod hands attack count and attack type index is generated once (in find_s_cr) at the start of the level, and never regenerated again
    // the game tries to make sure the attack type index is always one that corresponds to a carrots attack. however, the attack list changes throughout the fight, so it could totally end up starting from a boomerang attack anyway.
    // also, since the attack count list can get smaller, this index would occasionally be out of bounds. at the start of the pod hands attack, the game checks if it is OoB, and forces it to 0 if it is.
    public static void PodHandsManipulator(ref FlowerLevelFlower __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (FlowerPodHandsAttackCountIndexEasy.Value != FlowerPodHandsAttackCountIndexesEasy.Random) {
                __instance.podHandsAttackCountTarget = (int) FlowerPodHandsAttackCountIndexEasy.Value - 1;
            }
            if (FlowerPodHandsAttackTypeIndexEasy.Value != FlowerPodHandsAttackTypeIndexesEasy.Random) {
                __instance.currentPodHandsAttack = FlowerPodHandsAttackTypesEasy[FlowerPodHandsAttackTypeIndexEasy.Value];
            }
        }
        else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (FlowerPodHandsAttackCountIndexNormal.Value != FlowerPodHandsAttackCountIndexesNormal.Random) {
                __instance.podHandsAttackCountTarget = (int) FlowerPodHandsAttackCountIndexNormal.Value - 1;
                UnityEngine.Debug.Log("setting podHandsAttackCountTarget to " + ((int) FlowerPodHandsAttackCountIndexNormal.Value - 1).ToString());
            }
            if (FlowerPodHandsAttackTypeIndexNormal.Value != FlowerPodHandsAttackTypeIndexesNormal.Random) {
                __instance.currentPodHandsAttack = FlowerPodHandsAttackTypesNormal[FlowerPodHandsAttackTypeIndexNormal.Value];
                UnityEngine.Debug.Log("setting currentPodHandsAttack to " + (FlowerPodHandsAttackTypesNormal[FlowerPodHandsAttackTypeIndexNormal.Value].ToString()));
            }
        }
        else { // Hard
            if (FlowerPodHandsAttackCountIndexHard.Value != FlowerPodHandsAttackCountIndexesHard.Random) {
                __instance.podHandsAttackCountTarget = (int) FlowerPodHandsAttackCountIndexHard.Value - 1;
            }
            if (FlowerPodHandsAttackTypeIndexHard.Value != FlowerPodHandsAttackTypeIndexesHard.Random) {
                __instance.currentPodHandsAttack = FlowerPodHandsAttackTypesHard[FlowerPodHandsAttackTypeIndexHard.Value];
            }
        }
    }

    [HarmonyPatch(typeof(FlowerLevelFlowerAnimator), nameof(FlowerLevelFlowerAnimator.OnBlink))]
    [HarmonyPostfix]
    public static void BlinkManipulator(ref FlowerLevelFlowerAnimator __instance) {
        if (FlowerBlinkCount.Value != FlowerBlinkCounts.Random) {
            __instance.max = (int) FlowerBlinkCount.Value + 1;
        }
    }


    protected static bool IsWithinPhase(float phaseStart, float phaseEnd, FlowerLevel __instance) {
        return (__instance.properties.CurrentState.stateName == LevelProperties.Flower.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd);
    }


    public static readonly Dictionary<FlowerPodHandsAttackTypeIndexesEasy, int> FlowerPodHandsAttackTypesEasy = new Dictionary<FlowerPodHandsAttackTypeIndexesEasy, int> {
        { FlowerPodHandsAttackTypeIndexesEasy.B_01, 1 },
        { FlowerPodHandsAttackTypeIndexesEasy.C_02, 2 },
        { FlowerPodHandsAttackTypeIndexesEasy.E_04, 4 },
        { FlowerPodHandsAttackTypeIndexesEasy.H_07, 7 },
        { FlowerPodHandsAttackTypeIndexesEasy.J_09, 9 },
        { FlowerPodHandsAttackTypeIndexesEasy.K_10, 10 },
        { FlowerPodHandsAttackTypeIndexesEasy.M_12, 12 },
        { FlowerPodHandsAttackTypeIndexesEasy.N_13, 13 },
    };

    public static readonly Dictionary<FlowerPodHandsAttackTypeIndexesNormal, int> FlowerPodHandsAttackTypesNormal = new Dictionary<FlowerPodHandsAttackTypeIndexesNormal, int> {
        { FlowerPodHandsAttackTypeIndexesNormal.B_01, 1 },
        { FlowerPodHandsAttackTypeIndexesNormal.C_02, 2 },
        { FlowerPodHandsAttackTypeIndexesNormal.E_04, 4 },
        { FlowerPodHandsAttackTypeIndexesNormal.G_06, 6 },
        { FlowerPodHandsAttackTypeIndexesNormal.H_07, 7 },
        { FlowerPodHandsAttackTypeIndexesNormal.I_08, 8 },
        { FlowerPodHandsAttackTypeIndexesNormal.K_10, 10 },
        { FlowerPodHandsAttackTypeIndexesNormal.M_12, 12 },
    };

    public static readonly Dictionary<FlowerPodHandsAttackTypeIndexesHard, int> FlowerPodHandsAttackTypesHard = new Dictionary<FlowerPodHandsAttackTypeIndexesHard, int> {
        { FlowerPodHandsAttackTypeIndexesHard.B_01, 1 },
        { FlowerPodHandsAttackTypeIndexesHard.C_02, 2 },
        { FlowerPodHandsAttackTypeIndexesHard.F_05, 5 },
        { FlowerPodHandsAttackTypeIndexesHard.H_07, 7 },
        { FlowerPodHandsAttackTypeIndexesHard.I_08, 8 },
        { FlowerPodHandsAttackTypeIndexesHard.J_09, 9 },
        { FlowerPodHandsAttackTypeIndexesHard.M_12, 12 },
        { FlowerPodHandsAttackTypeIndexesHard.N_14, 14 },
    };


}