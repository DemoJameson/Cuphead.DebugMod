using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

public class HookHelper : PluginComponent {
    private static Harmony harmony;
    private static readonly List<UnityAction<Scene, Scene>> Actions = new();

    public static void ActiveSceneChanged(UnityAction<Scene, Scene> action) {
        Actions.Add(action);
        SceneManager.activeSceneChanged += action;
    }
    
    public static void ActiveSceneChanged(Action action) {
        void UnityAction(Scene _, Scene __) => action();
        Actions.Add(UnityAction);
        SceneManager.activeSceneChanged += UnityAction;
    }

    private void Awake() {
        harmony = Harmony.CreateAndPatchAll(typeof(HookHelper).Assembly);
    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
        foreach (UnityAction<Scene, Scene> action in Actions) {
            SceneManager.activeSceneChanged -= action;
        }
        Actions.Clear();
    }
}