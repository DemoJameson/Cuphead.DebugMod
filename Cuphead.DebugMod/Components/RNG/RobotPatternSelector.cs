using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
public class RobotPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // Both cycles are rolled once per fight when the head spawns, and consumed directly
    // (each read uses the index as-is; the walk advances it only after the attack/height
    // is used), so a postfix on InitHeliHead pins both - vanilla passes through untouched,
    // unlike the legacy hook which re-rolled and shifted the vanilla RNG stream.
    [HarmonyPatch(typeof(RobotLevelHelihead), nameof(RobotLevelHelihead.InitHeliHead))]
    [HarmonyPostfix]
    public static void HeliHeadManipulator(RobotLevelHelihead __instance, LevelProperties.Robot properties) {
        __instance.attackTypeIndex = RngState.Index(
            RngSpots.RobotGemColor, Mode, __instance.attackTypeIndex,
            properties.CurrentState.inventor.gemColourString.Split(new[] { ',' }).Length, false);
        __instance.coordinateIndex = RngState.Index(
            RngSpots.RobotHeadHeight, Mode, __instance.coordinateIndex,
            __instance.screenHeights.Length, false);
    }
}
