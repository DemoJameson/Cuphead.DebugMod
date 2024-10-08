using HarmonyLib;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
public class LevelSelectorEsc : PluginComponent {
    [HarmonyPatch(typeof(LevelSelectScene), nameof(LevelSelectScene.Update))]
    [HarmonyPrefix]
    private static bool PreventQuitGame() {
        if (Input.GetKeyDown(KeyCode.Escape) && !string.IsNullOrEmpty(PreviousSceneName)) {
            SceneLoader.LoadScene(PreviousSceneName, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
        }

        return false;
    }
}