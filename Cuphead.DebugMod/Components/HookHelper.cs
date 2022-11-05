using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

public class HookHelper : PluginComponent {
    private static readonly List<Harmony> Harmonies = new();
    private static readonly List<UnityAction<Scene, Scene>> Actions = new();

    public static void Patch(Assembly assembly) {
        Harmonies.Add(Harmony.CreateAndPatchAll(assembly));
    }

    public static void Patch(Type type) {
        Harmonies.Add(Harmony.CreateAndPatchAll(type));
    }

    public static void ActiveSceneChanged(UnityAction<Scene, Scene> action) {
        Actions.Add(action);
        SceneManager.activeSceneChanged += action;
    }
    
    public static void ActiveSceneChanged(Action action) {
        void UnityAction(Scene _, Scene __) => action();
        Actions.Add(UnityAction);
        SceneManager.activeSceneChanged += UnityAction;
    }

    public void OnDestroy() {
        foreach (Harmony harmony in Harmonies) {
            harmony.UnpatchSelf();
        }

        foreach (UnityAction<Scene, Scene> action in Actions) {
            SceneManager.activeSceneChanged -= action;
        }

        Harmonies.Clear();
        Actions.Clear();
    }
}