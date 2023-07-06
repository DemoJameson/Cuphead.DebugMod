#if v1_3

using System;
using System.ComponentModel;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components; 

[HarmonyPatch]
public class RelicLevelSelector : PluginComponent {
    [HarmonyPatch(typeof(CharmCurse), nameof(CharmCurse.CalculateLevel))]
    [HarmonyPrefix]
    private static bool FixedCharmCurseLevel(ref int __result) {
        if (RelicLevel.Value == RelicLevels.Default) {
            return true;
        } else {
            __result = Enum.GetNames(typeof(RelicLevels)).ToList().IndexOf(RelicLevel.Value.ToString()) - 2;
            return false;
        }
    }
}

#endif