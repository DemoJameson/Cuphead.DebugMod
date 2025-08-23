using BepInEx.CupheadDebugMod.Components.RNG;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
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
                if (Settings.NoDamage.IsDownEx()) {
                    ToggleNoDamage();
                }

                if (Settings.ReduceCurrency.IsDownEx()) {
                    ReduceCurrency();
                }

                if (Settings.AddCurrency.IsDownEx()) {
                    AddCurrency();
                }
                if (Settings.SaveState.IsDownEx()) {
                    SaveManager.SaveGame();
                }
                if (Settings.LoadState.IsDownEx()) {
                    SaveManager.LoadGame();
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

    private static void ToggleNoDamage() {
        Misc.Debug_ToggleNoDamage();
        if (Misc.DEBUG_DO_NO_DAMAGE) {
            Toast.Show("Enable No Damage");
        } else {
            Toast.Show("Disable No Damage");
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

    [HarmonyPatch(typeof(WinScreen), nameof(WinScreen.rotate_bg_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void FixWinScreen(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("speed"))) {
            ilCursor.Index--;
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Ldc_R4, 53f);
            break;
        }
    }

    [HarmonyPatch(typeof(DamageReceiver), nameof(DamageReceiver.TakeDamage))]
    [HarmonyPrefix]
    public static void PatchNoDamage(ref DamageReceiver __instance, DamageDealer.DamageInfo info) {
        if (Misc.DEBUG_DO_NO_DAMAGE && (__instance.type == DamageReceiver.Type.Enemy || __instance.type == DamageReceiver.Type.Other)) {
            info.damage = 0f;
        }   
    }

    private static void Debug_ToggleNoDamage() {
        Misc.DEBUG_DO_NO_DAMAGE = !Misc.DEBUG_DO_NO_DAMAGE;
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

    [HarmonyPatch(typeof(DicePalaceMainLevelGameManager), nameof(DicePalaceMainLevelGameManager.start_mini_boss_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void OnMinibossStart(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("time"))) {
            ilCursor.Emit(OpCodes.Call, typeof(Misc).GetMethod(nameof(SetIsMinibossStartingKingDice), BindingFlags.Static | BindingFlags.Public));
            break;
        }
    }

    public static void SetIsMinibossStartingKingDice() {
        DebugInfo.isMinibossStartingKingDice = true;
    }

    [HarmonyPatch(typeof(SlimeLevelSlime), nameof(SlimeLevelSlime.TurnBig))]
    [HarmonyPrefix]
    public static void SignalSpriteSwapSlime() {
        DebugInfo.spriteSwapOnSpriteSwapFrameCounter = DebugInfo.spriteSwapLevelFrameCounter;
        //DebugInfo.onBigSlimeLevelRealTime = DebugInfo.levelRealTime;
    }

    [HarmonyPatch(typeof(MouseLevelBrokenCanMouse), nameof(MouseLevelBrokenCanMouse.StartPattern))]
    [HarmonyPrefix]
    public static void SignalSpriteSwapMouse() {
        DebugInfo.spriteSwapOnSpriteSwapFrameCounter = DebugInfo.spriteSwapLevelFrameCounter;
        //DebugInfo.onBigSlimeLevelRealTime = DebugInfo.levelRealTime;
    }

    [HarmonyPatch(typeof(WeaponBouncerProjectile), nameof(WeaponBouncerProjectile.Die))]
    [HarmonyPrefix]
    public static void OnLobberEXExplosion(ref WeaponBouncerProjectile __instance) {
        if (__instance.isEx) {
            DebugInfo.spriteSwapOnLobberEXFrameCounter = DebugInfo.spriteSwapLevelFrameCounter;
            //DebugInfo.onLobberEXSlimeLevelRealTime = DebugInfo.levelRealTime;
        }
    }

    [HarmonyPatch(typeof(WinScreenTicker), nameof(WinScreenTicker.stars_tally_up_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void OnStarSkipInput(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldstr && i.Operand.ToString().Contains("win_skill_lvl"))) {
            ilCursor.Index++;
            ilCursor.Emit(OpCodes.Call, typeof(Misc).GetMethod(nameof(SetStarSkipTime), BindingFlags.Static | BindingFlags.Public));
            break;
        }
    }

    public static void SetStarSkipTime() {
        DebugInfo.onWinScreenStarFrame = DebugInfo.winScreenFrameCounter;
    }

    [HarmonyPatch(typeof(WinScreen), nameof(WinScreen.Awake))]
    [HarmonyPrefix]
    public static void OnAwake() {
        DebugInfo.winScreenFrameCounter = 0;
        DebugInfo.onWinScreenButtonPressFrame = 0;
        DebugInfo.onWinScreenStarFrame = 0;
        DebugInfo.winScreenStarSkipFrameOffset = 0;
        DebugInfo.winScreenStarSkipFrameList = [ 0, 0 ];
    }

    public static bool DEBUG_DO_NO_DAMAGE = false;
}