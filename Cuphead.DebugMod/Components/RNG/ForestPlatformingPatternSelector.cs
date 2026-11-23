using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class ForestPlatformingPatternSelector : PluginComponent {

    // Platforming stages always run on Normal (the game forces that mode for them), so
    // there is nothing per-difficulty here.
    //
    // The spawner rolls both cycle start positions in Awake, and Spawn() advances an
    // index before using it - the same pre-increment style as the Devil's pitchfork
    // angles, so the pinned entry is the first one the spawner consumes.
    [HarmonyPatch(typeof(ForestPlatformingLevelAcornSpawner), "Awake")]
    [HarmonyPostfix]
    public static void AcornSpawnerManipulator(ref ForestPlatformingLevelAcornSpawner __instance) {
        __instance.leftRightIndex = RngState.Index(
            RngSpots.ForestFolliesAcornSide, Level.Mode.Normal,
            __instance.leftRightIndex, __instance.leftRightString.Length, true);
        string[] yPattern = __instance.yString.Split(new[] { ',' });
        __instance.yIndex = RngState.Index(
            RngSpots.ForestFolliesAcornHeight, Level.Mode.Normal,
            __instance.yIndex, yPattern.Length, true);
    }
}