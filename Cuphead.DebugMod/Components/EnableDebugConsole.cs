using HarmonyLib;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components; 

[HarmonyPatch]
public class EnableDebugConsole : PluginComponent {
    [HarmonyPatch(typeof(Debug), nameof(Debug.isDebugBuild), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool DebugIsDebugBuild(ref bool __result) {
        __result = true;
        return false;
    }
}