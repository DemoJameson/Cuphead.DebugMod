using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class AirPlanePatternSelector {
    [HarmonyPatch(typeof(AirplaneLevelLeader), nameof(AirplaneLevelLeader.laser_main_cr))]
    [HarmonyPostfix]
    public static void AirplaneTurretPatternManipulator(ref AirplaneLevelLeader __instance) {
        if (AirplaneTurretPattern.Value != AirplaneTurretPatterns.Random) {
            __instance.laserPositionStringsMainIndex = Utility.GetUserPattern<AirplaneTurretPatterns>((int) AirplaneTurretPattern.Value);
        }
    }
}