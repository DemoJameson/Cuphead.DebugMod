using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

public class LevelSelectorFixer : PluginComponent {
    private void Awake() {
        HookHelper.ActiveSceneChanged(OnSceneChanged);
    }

    private void OnSceneChanged(Scene _, Scene scene) {
        if (GameObject.Find("CupheadCore")?.GetComponentInChildren<RewiredStandaloneInputModule>() is {} inputModule) {
            inputModule.allowMouseInput = scene.name == nameof(Scenes.scene_menu);
        }
    }
}
