using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog). Funfair Fever is a
// run 'n gun, so it always runs on Normal, and there is exactly one cannon and one
// hotdog in the stage - each rolls all of its start positions once at Start.
[HarmonyPatch]
internal class CircusPlatformingPatternSelector : PluginComponent {

    // The cannon rolls its order direction, muzzle start and color start directly in
    // Start (no pre-increment). The muzzles are stacked top to bottom in the scene, so
    // "Up" means goingBackwards = true (the muzzle index walks upward).
    [HarmonyPatch(typeof(CircusPlatformingLevelCannon), nameof(CircusPlatformingLevelCannon.Start))]
    [HarmonyPostfix]
    public static void CannonStartManipulator(ref CircusPlatformingLevelCannon __instance) {
        int order = RngState.Index(
            RngSpots.FunfairCannonOrderDirection, Level.Mode.Normal,
            __instance.goingBackwards ? 0 : 1, 2, false);
        __instance.goingBackwards = order == 0;
        __instance.shootIndex = RngState.Index(
            RngSpots.FunfairCannonMuzzleStart, Level.Mode.Normal,
            __instance.shootIndex, __instance.shootRoots.Length, false);
        __instance.pinkIndex = RngState.Index(
            RngSpots.FunfairCannonColorStart, Level.Mode.Normal,
            __instance.pinkIndex, __instance.pinkSplits.Length, false);
    }

    // The hotdog rolls spawn, condiment, side and shot-delay starts directly in Start
    // (no pre-increment). The first projectile's delay is derived from the rolled index
    // inside Start, so it has to be recomputed after pinning.
    [HarmonyPatch(typeof(CircusPlatformingLevelHotdog), nameof(CircusPlatformingLevelHotdog.Start))]
    [HarmonyPostfix]
    public static void HotdogStartManipulator(ref CircusPlatformingLevelHotdog __instance) {
        __instance.spawnIndex = RngState.Index(
            RngSpots.FunfairHotdogSpawnStart, Level.Mode.Normal,
            __instance.spawnIndex, __instance.spawnPattern.Length, false);
        __instance.condimentIndex = RngState.Index(
            RngSpots.FunfairHotdogCondimentStart, Level.Mode.Normal,
            __instance.condimentIndex, __instance.condimentPattern.Length, false);
        __instance.sideIndex = RngState.Index(
            RngSpots.FunfairHotdogSideStart, Level.Mode.Normal,
            __instance.sideIndex, __instance.sidePattern.Length, false);
        __instance.shotDelayIndex = RngState.Index(
            RngSpots.FunfairHotdogDelayStart, Level.Mode.Normal,
            __instance.shotDelayIndex, __instance.shotDelayPattern.Length, false);
        __instance.currentDelay = int.Parse(__instance.shotDelayPattern[__instance.shotDelayIndex]);
    }
}
