using System;
using BepInEx.CupheadDebugMod.Components;

namespace BepInEx.CupheadDebugMod.Rng;

public class RngCore : PluginComponent {
    private void Awake() {
        RngProfiles.Initialize();
        HookHelper.ActiveSceneChanged(() => RngState.ResetSession());
    }
}
