#if v1_3
using System;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngineInternal;
using UniverseLib;
using UniverseLib.Input;

namespace BepInEx.CupheadDebugMod.Components;

public class LevelSelectorFixer : PluginComponent {
    private static readonly Action enableEventSystem =
        AccessTools.Method(typeof(EventSystemHelper), "EnableEventSystem").CreateDelegate(typeof(Action), null) as Action;

    private static readonly Action releaseEventSystem =
        AccessTools.Method(typeof(EventSystemHelper), "ReleaseEventSystem").CreateDelegate(typeof(Action), null) as Action;

    private string currentName;
    private bool IsMenu => currentName == nameof(Scenes.scene_menu);
    
    private void Awake() {
        HookHelper.ActiveSceneChanged(OnSceneChanged);
    }

    private void OnSceneChanged(Scene _, Scene scene) {
        currentName = scene.name;
        if (IsMenu) {
            Universe.Init(logHandler: (s, _) => Logger.LogInfo(s));
        } else {
            releaseEventSystem();
        }
    }

    private void Update() {
        // avoid failing after UnityExplorer is closed, so execute every frame
        if (IsMenu) {
            enableEventSystem();
        }
    }
}
#endif