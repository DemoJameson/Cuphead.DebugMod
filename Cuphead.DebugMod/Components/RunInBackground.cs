using System;
using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

public class RunInBackground : PluginComponent {
    private void Awake() {
        Application.runInBackground = Settings.RunInBackground.Value;
        Settings.RunInBackground.SettingChanged += RunInBackgroundOnSettingChanged;
    }

    private void RunInBackgroundOnSettingChanged(object sender, EventArgs e) {
        Application.runInBackground = Settings.RunInBackground.Value;
    }
}