using BepInEx.CupheadDebugMod.Config;
using Rewired;

namespace BepInEx.CupheadDebugMod.Components; 

public class IgnoreInputWhenLoseFocus : PluginComponent {
    private void Update() {
        if (ReInput.configuration != null) {
            ReInput.configuration.ignoreInputWhenAppNotInFocus = Settings.IgnoreInputWhenLoseFocus.Value;
        }
    }
}