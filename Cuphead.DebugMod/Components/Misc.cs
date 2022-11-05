using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components; 

public class Misc : PluginComponent {
    private void Awake() {
        Settings.OnKeyUpdate += () => {
            if (Settings.SwapBetweenFrameLimit.IsDownEx()) {
                SwapBetweenFrameLimit();
            }

            if (Settings.ClearCharmsSupers.IsDownEx()) {
                ClearCharmsSupers();
            }

            if (Settings.QuickRestart.IsDownEx()) {
                TryQuickRetry();
            }

            if (PlayerData.inGame) {
                if (Settings.LevelSelector.IsDownEx() && CurrentSceneName != nameof(Scenes.scene_menu)) {
                    SceneLoader.LoadScene(Scenes.scene_menu, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
                }

                if (Settings.Gain5ExCards.IsDownEx()) {
                    Gain5ExCards();
                }

                if (Settings.Gain1ExCard.IsDownEx()) {
                    Gain1ExCard();
                }

                if (Settings.ToggleFrameCounter.IsDownEx()) {
                    FramerateCounter.Init();
                    FramerateCounter.SHOW = !FramerateCounter.SHOW;
                }

                if (Settings.InvincibilityOneFight.IsDownEx()) {
                    ToggleInvincibility();
                }

                if (Settings.X10Damage.IsDownEx()) {
                    ToggleMegaDamage();
                }

                if (Settings.ReduceCurrency.IsDownEx()) {
                    ReduceCurrency();
                }

                if (Settings.AddCurrency.IsDownEx()) {
                    AddCurrency();
                }
            }
        };
    }

     private static void TryQuickRetry() {
        if (PauseManager.state != PauseManager.State.Paused &&
            Level.Current.type is Level.Type.Battle or Level.Type.Platforming) {
            PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, canSwitch: false);
            PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, canSwitch: false);
            SceneLoader.ReloadLevel();
            if (Level.IsDicePalaceMain || Level.IsDicePalace) {
                DicePalaceMainLevelGameInfo.CleanUpRetry();
            }
        }
    }

    private static void AddCurrency() {
        if (PlayerData.Data.GetCurrency(PlayerId.PlayerOne) < 60) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 1);
        }

        if (PlayerData.Data.GetCurrency(PlayerId.PlayerTwo) < 60) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 1);
        }
    }

    private static void ReduceCurrency() {
        if (PlayerData.Data.GetCurrency(PlayerId.PlayerOne) > 0) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerOne, -1);
        }

        if (PlayerData.Data.GetCurrency(PlayerId.PlayerTwo) > 0) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, -1);
        }
    }

    private static void ToggleMegaDamage() {
        DamageReceiver.Debug_ToggleMegaDamage();
        if (DamageReceiver.DEBUG_DO_MEGA_DAMAGE) {
            Toast.Show("Enable x10 Damage");
        } else {
            Toast.Show("Disable x10 Damage");
        }
    }

    private static void ToggleInvincibility() {
        PlayerStatsManager.DebugToggleInvincible();
        if (PlayerStatsManager.DebugInvincible) {
            Toast.Show("Enable Invincibility in One Fight");
        } else {
            Toast.Show("Disable Invincibility");
        }
    }

    private static void Gain1ExCard() {
        PlayerStatsManager[] array = FindObjectsOfType<PlayerStatsManager>();
        foreach (PlayerStatsManager statesManager in array) {
            statesManager.DebugAddSuper();
        }
    }

    private static void Gain5ExCards() {
        PlayerStatsManager[] array2 = FindObjectsOfType<PlayerStatsManager>();
        foreach (PlayerStatsManager statsManager in array2) {
            statsManager.DebugFillSuper();
        }
    }

    private static void SwapBetweenFrameLimit() {
        if (Application.targetFrameRate == 60) {
            Application.targetFrameRate = -1;
            Toast.Show("60 FPS");
        } else {
            Application.targetFrameRate = 60;
            Toast.Show("Unlimited FPS");
        }
    }

    private static void ClearCharmsSupers() {
        if (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne) is { } playerLoadout1) {
            playerLoadout1.charm = Charm.None;
            playerLoadout1.super = Super.None;
        }

        if (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo) is { } playerLoadout2) {
            playerLoadout2.charm = Charm.None;
            playerLoadout2.super = Super.None;
        }

        Toast.Show("Clear CHARMS and SUPERS");
    }
}