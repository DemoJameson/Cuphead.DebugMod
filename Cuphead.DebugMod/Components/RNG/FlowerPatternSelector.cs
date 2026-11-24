using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class FlowerPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The three phase cycles are decided once per fight when the level's State objects
    // are constructed, and NextPattern only walks an index forward from there - nothing
    // re-rolls it when a state becomes current. All three spots are therefore resolved
    // up-front against the fight's own states (0 = Phase 1, 1 = Phase 2, 2 = Phase 3);
    // there is deliberately no state-change matching, which would miss phases skipped
    // by burst damage. NextPattern advances the index before reading it, hence
    // preIncrement.
    [HarmonyPatch(typeof(FlowerLevel), nameof(FlowerLevel.PartialInit))]
    [HarmonyPostfix]
    private static void PhasePatternManipulator(FlowerLevel __instance) {
        LevelProperties.Flower.State[] states = (LevelProperties.Flower.State[])RngSpots.FindField(
            typeof(LevelProperties.Flower), "states").GetValue(__instance.properties);
        if (states == null) {
            return;
        }
        for (int i = 0; i < states.Length && i < 3; i++) {
            LevelProperties.Flower.State state = states[i];
            if (state.patterns.Length < 2) {
                continue; // single-entry cycles (Easy/Expert, Final Phase) have no start roll
            }
            string spotId = i == 0 ? RngSpots.FlowerMainPatternStart
                : i == 1 ? RngSpots.FlowerGeneric1PatternStart
                : RngSpots.FlowerGeneric2PatternStart;
            state.patternIndex = RngState.Index(spotId, Mode, state.patternIndex, state.patterns.Length, true);
        }
    }

    // The head-lunge start is consumed directly (StartLaser reads the index as-is and
    // OnLaserComplete advances it afterwards), so no pre-increment compensation.
    [HarmonyPatch(typeof(FlowerLevelFlower), nameof(FlowerLevelFlower.LevelInit))]
    [HarmonyPostfix]
    private static void HeadLungeManipulator(FlowerLevelFlower __instance, LevelProperties.Flower properties) {
        string[] entries = properties.CurrentState.laser.attackType.Split(new[] { ',' });
        __instance.currentLaserAttack = RngState.Index(
            RngSpots.FlowerHeadLungeStart, Mode, __instance.currentLaserAttack, entries.Length, false);
    }

    // The pod-hands rolls happen once in find_s_cr, right after LevelInit. The phase-1
    // intro is long after that and before the first pod-hands attack - the legacy hook
    // point, kept. Both indices are consumed directly (StartPotHands reads them as-is
    // and advances after each attack/volley), so no pre-increment compensation.
    [HarmonyPatch(typeof(FlowerLevelFlower), "Phase1IntroAudio")]
    [HarmonyPrefix]
    private static void PodHandsManipulator(FlowerLevelFlower __instance) {
        __instance.currentPodHandsAttack = RngState.Index(
            RngSpots.FlowerPodHandsType, Mode,
            __instance.currentPodHandsAttack, OptionCount(RngSpots.FlowerPodHandsType), false);
        __instance.podHandsAttackCountTarget = RngState.Index(
            RngSpots.FlowerPodHandsCount, Mode,
            __instance.podHandsAttackCountTarget, OptionCount(RngSpots.FlowerPodHandsCount), false);
    }

    // Every blink re-rolls the idle-loop count (Range(2, 5) can only produce 2, 3 or 4);
    // the spot is per-event and shifts the vanilla roll back into a zero-based index.
    [HarmonyPatch(typeof(FlowerLevelFlowerAnimator), nameof(FlowerLevelFlowerAnimator.OnBlink))]
    [HarmonyPostfix]
    private static void BlinkManipulator(FlowerLevelFlowerAnimator __instance) {
        __instance.max = RngState.Index(RngSpots.FlowerBlinkCount, Mode, __instance.max - 2, 3, false) + 2;
    }

    private static int OptionCount(string spotId) {
        RngSpot spot = RngSpots.FindForMode(spotId, Mode);
        return spot != null && spot.Options != null ? spot.Options(Mode).Length : 0;
    }
}