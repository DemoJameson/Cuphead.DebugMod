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
internal class OldManPatternSelector {
    [HarmonyPatch(typeof(OldManLevelOldMan), nameof(OldManLevelOldMan.LevelInit))]
    [HarmonyPostfix]
    public static void OldManPhaseOnePatternManipulator(ref OldManLevelOldMan __instance) {
        if (OldManPhaseOnePattern.Value != OldManPhaseOnePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<OldManPhaseOnePatterns>((int) OldManPhaseOnePattern.Value);
        }
    }
}