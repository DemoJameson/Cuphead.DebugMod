using System;
using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class CircusPlatformingPatternSelector : PluginComponent {
    [HarmonyPatch(typeof(CircusPlatformingLevelCannon), nameof(CircusPlatformingLevelCannon.Start))]
    [HarmonyPostfix]
    public static void CannonStartManipulator(ref CircusPlatformingLevelCannon __instance) {
        // the muzzles are stacked top to bottom in the scene, so forward order (goingBackwards = false) fires downward
        if (CircusCannonballOrderDirection.Value == CircusCannonballOrderDirections.Up) {
            __instance.goingBackwards = true;
        } else if (CircusCannonballOrderDirection.Value == CircusCannonballOrderDirections.Down) {
            __instance.goingBackwards = false;
        }
        if (CircusCannonballPinkStart.Value != CircusCannonballPinkStarts.Random) {
            __instance.pinkIndex = (int) CircusCannonballPinkStart.Value - 1;
        }
        if (CircusCannonballMuzzleStart.Value != CircusCannonballMuzzleStarts.Random) {
            __instance.shootIndex = (int) CircusCannonballMuzzleStart.Value - 1;
        }
    }

    [HarmonyPatch(typeof(CircusPlatformingLevelHotdog), nameof(CircusPlatformingLevelHotdog.Start))]
    [HarmonyPostfix]
    public static void HotdogStartManipulator(ref CircusPlatformingLevelHotdog __instance) {
        if (CircusHotdogSpawnStart.Value != CircusHotdogSpawnStarts.Random) {
            __instance.spawnIndex = (int) CircusHotdogSpawnStart.Value - 1;
        }
        if (CircusHotdogCondimentStart.Value != CircusHotdogCondimentStarts.Random) {
            __instance.condimentIndex = (int) CircusHotdogCondimentStart.Value - 1;
        }
        if (CircusHotdogSideStart.Value != CircusHotdogSideStarts.Random) {
            __instance.sideIndex = (int) CircusHotdogSideStart.Value - 1;
        }
        if (CircusHotdogShotDelayStart.Value != CircusHotdogShotDelayStarts.Random) {
            __instance.shotDelayIndex = (int) CircusHotdogShotDelayStart.Value - 1;
            // the game derives the first projectile's delay from the rolled index in Start, so it has to be recomputed here
            __instance.currentDelay = int.Parse(__instance.shotDelayPattern[__instance.shotDelayIndex]);
        }
    }
}
