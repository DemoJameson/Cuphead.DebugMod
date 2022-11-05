using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

// https://github.com/BepInEx/BepInEx.Utility/blob/master/BepInEx.MuteInBackground/MuteInBackground.cs
public class MuteInBackground : PluginComponent {
    private float? originalVolume;

    private void OnApplicationFocus(bool hasFocus) {
        if (hasFocus) {
            //Restore the original volume if one was previously set
            if (originalVolume.HasValue) {
                AudioListener.volume = originalVolume.Value;
                originalVolume = null;
            }
        } else if (Settings.MuteInBackground.Value) {
            //Store the original volume and set the volume to zero
            originalVolume = AudioListener.volume;
            AudioListener.volume = 0;
        }
    }
}