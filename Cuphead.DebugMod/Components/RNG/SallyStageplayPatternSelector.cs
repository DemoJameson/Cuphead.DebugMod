using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class SallyStageplayPatternSelector {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // Act 1's attack cycle is decided once per fight when the level's State objects are
    // constructed, and NextPattern only walks an index forward from there - so the start
    // is pinned up-front (preIncrement). The later acts' cycles are not covered by the
    // legacy settings and stay vanilla.
    [HarmonyPatch(typeof(SallyStagePlayLevel), nameof(SallyStagePlayLevel.PartialInit))]
    [HarmonyPostfix]
    private static void PhaseOnePatternManipulator(SallyStagePlayLevel __instance) {
        LevelProperties.SallyStagePlay.State[] states = (LevelProperties.SallyStagePlay.State[])RngSpots.FindField(
            typeof(LevelProperties.SallyStagePlay), "states").GetValue(__instance.properties);
        if (states == null || states.Length < 1 || states[0].patterns.Length < 2) {
            return;
        }
        states[0].patternIndex = RngState.Index(
            RngSpots.SallyMainPattern, Mode, states[0].patternIndex, states[0].patterns.Length, true);
    }

    // Sally's three cycles are rolled once per fight in LevelInit. The jump type and the
    // teleport offset are consumed as-is (each attack reads the index, then the cycle
    // advances), but the jump count is consumed one entry ahead (OnJumpAttack advances
    // jumpCountIndex before jump_cr reads the count), hence its preIncrement.
    [HarmonyPatch(typeof(SallyStagePlayLevelSally), nameof(SallyStagePlayLevelSally.LevelInit))]
    [HarmonyPostfix]
    private static void SallyCycleManipulator(SallyStagePlayLevelSally __instance, LevelProperties.SallyStagePlay properties) {
        __instance.jumpTypeIndex = RngState.Index(
            RngSpots.SallyJumpType, Mode, __instance.jumpTypeIndex,
            properties.CurrentState.jump.JumpAttackString.Split(new[] { ',' }).Length, true);
        __instance.jumpCountIndex = RngState.Index(
            RngSpots.SallyJumpCount, Mode, __instance.jumpCountIndex,
            properties.CurrentState.jump.JumpAttackCountString.Split(new[] { ',' }).Length, true);
        __instance.teleportOffsetIndex = RngState.Index(
            RngSpots.SallyTeleportOffset, Mode, __instance.teleportOffsetIndex,
            properties.CurrentState.teleport.appearOffsetString.Split(new[] { ',' }).Length, false);
    }
}
