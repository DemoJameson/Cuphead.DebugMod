using System;
using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

public class FrameAdvance : PluginComponent {
    private static string FormattedSpeed => (int) Math.Round(Time.timeScale * 100) + "%";
    private bool inFrameAdvanceMode;
    private bool alreadyFrameAdvanced = true;
    private float oldSpeed = Time.timeScale;

    private void Awake() {
        Settings.OnKeyUpdate += SettingsOnOnKeyUpdate;
    }

    private void Update() {
        if (inFrameAdvanceMode) {
            if (!alreadyFrameAdvanced) {
                Time.timeScale = 1f;
                alreadyFrameAdvanced = true;
            } else {
                Time.timeScale = 0f;
            }
        }
    }

    private void SettingsOnOnKeyUpdate() {
        if (Settings.DecreaseSpeed.IsDownEx()) {
            Time.timeScale = Mathf.Max(Time.timeScale - 0.1f, 0);
            Toast.Show($"Game Speed: {FormattedSpeed}");
        }

        if (Settings.IncreaseSpeed.IsDownEx()) {
            Time.timeScale = Mathf.Min(Time.timeScale + 0.25f, 6);
            Toast.Show($"Game Speed: {FormattedSpeed}");
        }

        if (Settings.ResetSpeed.IsDownEx()) {
            Time.timeScale = 1f;
            oldSpeed = 1f;
            Toast.Show($"Reset Game Speed");
        }

        if (Settings.PauseResume.IsDownEx() || Settings.FrameAdvance.IsDownEx() && !inFrameAdvanceMode) {
            if (inFrameAdvanceMode) {
                inFrameAdvanceMode = false;
                Time.timeScale = oldSpeed;
            } else {
                inFrameAdvanceMode = true;
                oldSpeed = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        if (Settings.FrameAdvance.IsDownEx()) {
            alreadyFrameAdvanced = false;
        }
    }
}