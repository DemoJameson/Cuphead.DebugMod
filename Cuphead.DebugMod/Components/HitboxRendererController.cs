using System.Collections;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
public class HitboxRendererController : PluginComponent {
    [HarmonyPatch(typeof(AbstractMonoBehaviour), "Awake")]
    [HarmonyPrefix]
    private static void AddController(AbstractMonoBehaviour __instance) {
        // delayed for correct processing of platform hitboxes
        __instance.StartCoroutine(AddHitboxRenderer(__instance.gameObject));
    }

    public void Awake() {
        HookHelper.ActiveSceneChanged(OnSceneChanged);
        Settings.OnKeyUpdate += SettingsOnOnKeyUpdate;
    }

    private void SettingsOnOnKeyUpdate() {
        if (Settings.Hitboxes.IsDownEx()) {
            ToggleHitboxes();
        }

        if (Settings.PlatformHitboxes.IsDownEx()) {
            TogglePlatformHitboxes();
        }

        if (Settings.HighlightingHitboxes.IsDownEx()) {
            HighlightingHitboxes();
        }
    }

    private static void HighlightingHitboxes() {
        HitboxRenderer.obscureScreen = !HitboxRenderer.obscureScreen;
        if (HitboxRenderer.blackScreen != null) {
            HitboxRenderer.blackScreen.enabled = HitboxRenderer.obscureScreen && HitboxRenderer.show;
        }
    }

    private static void TogglePlatformHitboxes() {
        HitboxRenderer.hidePlatforms = !HitboxRenderer.hidePlatforms;
        HitboxRenderer[] array4 = FindObjectsOfType<HitboxRenderer>();
        foreach (HitboxRenderer renderer in array4) {
            if (renderer.isPlatformElement) {
                renderer.SetEnabled(!HitboxRenderer.hidePlatforms && HitboxRenderer.show);
            }
        }
    }

    private static void ToggleHitboxes() {
        HitboxRenderer.show = !HitboxRenderer.show;

        HitboxRenderer[] array3 = FindObjectsOfType<HitboxRenderer>();
        foreach (HitboxRenderer renderer in array3) {
            renderer.SetEnabled(HitboxRenderer.show);
        }

        if (HitboxRenderer.blackScreen != null) {
            HitboxRenderer.blackScreen.enabled = HitboxRenderer.obscureScreen && HitboxRenderer.show;
        }
    }

    private static void OnSceneChanged(Scene _, Scene __) {
        HitboxRenderer.Setup();
    }

    private static IEnumerator AddHitboxRenderer(GameObject gameObject) {
        yield return new WaitForSeconds(0.1f);
        Collider2D[] componentsInChildren = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider2D in componentsInChildren) {
            if (!collider2D.gameObject.GetComponent<HitboxRenderer>()) {
                collider2D.gameObject.AddComponent<HitboxRenderer>();
            }
        }
    }
}