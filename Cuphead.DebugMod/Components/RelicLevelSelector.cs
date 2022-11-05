#if v1_3

using System;
using System.ComponentModel;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace BepInEx.CupheadDebugMod.Components; 

[HarmonyPatch]
public class RelicLevelSelector : PluginComponent {
    private enum RelicLevels {
        Default,
        BrokenRelic,
        [Description("Cursed Relic 1")]
        CursedRelic1,
        [Description("Cursed Relic 2")]
        CursedRelic2,
        [Description("Cursed Relic 3")]
        CursedRelic3,
        [Description("Cursed Relic 4")]
        CursedRelic4,
        DivineRelic
    }

    private static ConfigEntry<RelicLevels> RelicLevel;

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

    private void Awake() {
        RelicLevel = Plugin.Instance.Config.Bind("DLC", "Relic Level", RelicLevels.Default);
    }
}

#endif