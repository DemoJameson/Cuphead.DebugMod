using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class SallyStageplayPatternSelector {

    [HarmonyPatch(typeof(SallyStagePlayLevel), nameof(SallyStagePlayLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOneFirstPatternManipulator(ref SallyStagePlayLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (SallyStageplayPatternEasy.Value != SallyStageplayPatternsEasy.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<SallyStageplayPatternsEasy>((int) SallyStageplayPatternEasy.Value);
            }
        } else {
            if (SallyStageplayPatternNormalHard.Value != SallyStageplayPatternsNormalHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<SallyStageplayPatternsNormalHard>((int) SallyStageplayPatternNormalHard.Value);
            }
        }
    }

    [HarmonyPatch(typeof(SallyStagePlayLevelSally), nameof(SallyStagePlayLevelSally.LevelInit))]
    [HarmonyPostfix]

    public static void PhaseOneJumpTeleportManipulator(ref SallyStagePlayLevelSally __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (SallyStageplayJumpCountEasy.Value != SallyStageplayJumpCountsEasy.Random) {
                __instance.jumpCountIndex = Utility.GetUserPattern<SallyStageplayJumpCountsEasy>((int) SallyStageplayJumpCountEasy.Value);
            }
            if (SallyStageplayJumpTypeEasy.Value != SallyStageplayJumpTypesEasy.Random) {
                __instance.jumpTypeIndex = Utility.GetUserPattern<SallyStageplayJumpTypesEasy>((int) SallyStageplayJumpTypeEasy.Value);
            }
            if (SallyStageplayTeleportOffsetEasy.Value != SallyStageplayTeleportOffsetsEasy.Random) {
                __instance.teleportOffsetIndex = ((int) SallyStageplayTeleportOffsetEasy.Value) - 1;
            }
        } else {
            if (SallyStageplayJumpCountNormalHard.Value != SallyStageplayJumpCountsNormalHard.Random) {
                __instance.jumpCountIndex = Utility.GetUserPattern<SallyStageplayJumpCountsNormalHard>((int) SallyStageplayJumpCountNormalHard.Value);
            }
            if (SallyStageplayJumpTypeNormalHard.Value != SallyStageplayJumpTypesNormalHard.Random) {
                __instance.jumpTypeIndex = Utility.GetUserPattern<SallyStageplayJumpTypesNormalHard>((int) SallyStageplayJumpTypeNormalHard.Value);
            }
            if (SallyStageplayTeleportOffsetNormalHard.Value != SallyStageplayTeleportOffsetsNormalHard.Random) {
                __instance.teleportOffsetIndex = ((int) SallyStageplayTeleportOffsetNormalHard.Value) - 1;
            }
        }
    }
}
