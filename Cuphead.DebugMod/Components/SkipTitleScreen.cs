using BepInEx.CupheadDebugMod.Config;
using UnityEngine.SceneManagement;

namespace BepInEx.CupheadDebugMod.Components;

public class SkipTitleScreen : PluginComponent {
    private void Update() {
        if (Settings.SkipTitleScreen.Value && SceneManager.GetActiveScene().name == nameof(Scenes.scene_title)) {
#if !v1_0
            CreditsScreen.goodEnding = true;
#endif
            SettingsData.Data.hasBootedUpGame = true;
            PlayerManager.SetPlayerCanJoin(PlayerId.PlayerOne, true, false);
            SceneLoader.LoadScene(Scenes.scene_slot_select, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
        }
    }
}