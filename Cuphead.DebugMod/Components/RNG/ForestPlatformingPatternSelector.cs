using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class ForestPlatformingPatternSelector : PluginComponent {
    [HarmonyPatch(typeof(ForestPlatformingLevelAcornSpawner), nameof(ForestPlatformingLevelAcornSpawner.Awake))]
    [HarmonyPostfix]
    public static void AcornSpawnerManipulator(ref ForestPlatformingLevelAcornSpawner __instance) {
        if (ForestPlatformingAcornSpawnerDirection.Value != ForestPlatformingAcornSpawnerDirections.Random) {
            __instance.leftRightIndex = Utility.GetUserPattern<ForestPlatformingAcornSpawnerDirections>((int) ForestPlatformingAcornSpawnerDirection.Value);
        }
        if (ForestPlatformingAcornSpawnerYIndex.Value != ForestPlatformingAcornSpawnerYIndexes.Random) {
            __instance.yIndex = Utility.GetUserPattern<ForestPlatformingAcornSpawnerYIndexes>((int) ForestPlatformingAcornSpawnerYIndex.Value);
        }
    }
}
