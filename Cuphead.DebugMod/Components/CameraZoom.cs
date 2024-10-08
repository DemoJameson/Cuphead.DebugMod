using BepInEx.CupheadDebugMod.Config;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

public class CameraZoom : PluginComponent {
    private AbstractCupheadGameCamera Camera => FindObjectOfType<AbstractCupheadGameCamera>();
    private float? defaultZoom;

    private void Awake() {
        HookHelper.ActiveSceneChanged(OnActiveSceneChanged);
        Settings.OnKeyUpdate += OnSettingsOnOnKeyUpdate;
        SaveDefaultZoom();
    }

    private void OnActiveSceneChanged(Scene _, Scene scene) {
        // wait for camera.zoom init
        Invoke(nameof(SaveDefaultZoom), 0.1f);
    }

    private void SaveDefaultZoom() {
        defaultZoom = Camera?.zoom;
    }

    private void OnSettingsOnOnKeyUpdate() {
        if (Settings.CameraZoomIn.IsDownEx()) {
            Camera?.Zoom(Camera.zoom * 1.05f, 0f, EaseUtils.EaseType.linear);
        }

        if (Settings.CameraZoomOut.IsDownEx()) {
            Camera?.Zoom(Camera.zoom * 0.95f, 0f, EaseUtils.EaseType.linear);
        }

        if (Settings.CameraZoomReset.IsDownEx() && defaultZoom.HasValue) {
            Camera?.Zoom(defaultZoom.Value, 0f, EaseUtils.EaseType.linear);
        }
    }
}