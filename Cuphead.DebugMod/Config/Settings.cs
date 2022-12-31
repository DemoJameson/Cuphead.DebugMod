using System;
using BepInEx.Configuration;
using UnityEngine;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Config;

[PluginComponentPriority(int.MaxValue)]
public class Settings : PluginComponent {
    public static ConfigEntry<KeyboardShortcut> Hitboxes;
    public static ConfigEntry<KeyboardShortcut> PlatformHitboxes;
    public static ConfigEntry<KeyboardShortcut> HighlightingHitboxes;

    public static ConfigEntry<bool> MuteInBackground;
    public static ConfigEntry<bool> RunInBackground;
    public static ConfigEntry<bool> IgnoreInputWhenLoseFocus;
    public static ConfigEntry<bool> SkipTitleScreen;
    public static ConfigEntry<KeyboardShortcut> InvincibilityOneFight;
    public static ConfigEntry<KeyboardShortcut> Gain5ExCards;
    public static ConfigEntry<KeyboardShortcut> Gain1ExCard;
    public static ConfigEntry<KeyboardShortcut> ReduceCurrency;
    public static ConfigEntry<KeyboardShortcut> AddCurrency;
    public static ConfigEntry<KeyboardShortcut> X10Damage;
    public static ConfigEntry<KeyboardShortcut> LevelSelector;
    public static ConfigEntry<KeyboardShortcut> ToggleFrameCounter;
    public static ConfigEntry<KeyboardShortcut> SwapBetweenFrameLimit;
    public static ConfigEntry<KeyboardShortcut> ClearCharmsSupers;
    public static ConfigEntry<KeyboardShortcut> QuickRestart;

    public static ConfigEntry<KeyboardShortcut> ToggleAllPanels;
    public static ConfigEntry<KeyboardShortcut> ToggleBetweenPanels;

    public static ConfigEntry<KeyboardShortcut> CameraZoomIn;
    public static ConfigEntry<KeyboardShortcut> CameraZoomOut;
    public static ConfigEntry<KeyboardShortcut> CameraZoomReset;

    public static ConfigEntry<bool> AllowRecollectCoins;
    
    public static ConfigEntry<KeyboardShortcut> DecreaseSpeed;
    public static ConfigEntry<KeyboardShortcut> IncreaseSpeed;
    public static ConfigEntry<KeyboardShortcut> ResetSpeed;
    public static ConfigEntry<KeyboardShortcut> PauseResume;
    public static ConfigEntry<KeyboardShortcut> FrameAdvance;

    #if v1_3
    public static ConfigEntry<RelicLevels> RelicLevel;
    #endif

    public static ConfigEntry<FrogsPhaseOnePatterns> FrogsPhaseOnePattern;
    public static ConfigEntry<FrogsPhaseFinalPatterns> FrogsPhaseFinalPattern;
    public static ConfigEntry<FlyingBlimpPhaseBlimp2Patterns> FlyingBlimpPhaseBlimp2Pattern;
    public static ConfigEntry<FlyingBlimpPhaseBlimp3Patterns> FlyingBlimpPhaseBlimp3Pattern;
    public static ConfigEntry<ClownDashDelays> ClownDashDelay;
    public static ConfigEntry<DevilPhaseOnePatterns> DevilPhaseOnePattern;
    public static ConfigEntry<DevilPhaseOneHeadTypes> DevilPhaseOneHeadType;
    public static ConfigEntry<DevilPhaseOneDragonDirections> DevilPhaseOneDragonDirection;
    public static ConfigEntry<DevilPhaseOneSpiderOffsets> DevilPhaseOneSpiderOffset;
    public static ConfigEntry<DevilPhaseOnePitchforkTypes> DevilPhaseOnePitchforkType;
    public static ConfigEntry<DevilPhaseTwoPatterns> DevilPhaseTwoPattern;
    public static ConfigEntry<DevilPhaseTwoBombEyeDirections> DevilPhaseTwoBombEyeDirection;
    public static ConfigEntry<FlyingMermaidPhaseOnePatterns> FlyingMermaidPhaseOnePattern;


    public static event Action OnKeyUpdate;

    public void Awake() {
        ConfigFile config = Plugin.Instance.Config;
        int order = 0;

        Hitboxes = config.Bind("Hitbox", "Hitboxes", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), --order);
        PlatformHitboxes = config.Bind("Hitbox", "Platform Hitboxes", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl), --order);
        HighlightingHitboxes = config.Bind("Hitbox", "Highlighting Hitboxes", new KeyboardShortcut(KeyCode.B, KeyCode.LeftControl), --order);

        MuteInBackground = config.Bind("Misc", "Mute In Background",false, --order);
        RunInBackground = config.Bind("Misc", "Run In Background",false, --order);
        IgnoreInputWhenLoseFocus = config.Bind("Misc", "Ignore Input When Lose Focus",false, --order);
        SkipTitleScreen = config.Bind("Misc", "Skip Title Screen",true, --order);
        QuickRestart = config.Bind("Misc", "Quick Restart", new KeyboardShortcut(KeyCode.R), --order);
        LevelSelector = config.Bind("Misc", "Level Selector", new KeyboardShortcut(KeyCode.BackQuote), --order);
        Gain5ExCards = config.Bind("Misc", "Gain 5 Ex Cards", new KeyboardShortcut(KeyCode.Alpha1), --order);
        Gain1ExCard = config.Bind("Misc", "Gain 1 Ex Card", new KeyboardShortcut(KeyCode.Alpha2), --order);
        ClearCharmsSupers = config.Bind("Misc", "Clear CHARMS and SUPERS", new KeyboardShortcut(KeyCode.Alpha3), --order);
        X10Damage = config.Bind("Misc", "x10 Damage", new KeyboardShortcut(KeyCode.Alpha4), --order);
        InvincibilityOneFight = config.Bind("Misc", "Invincibility One Fight", new KeyboardShortcut(KeyCode.Alpha5), --order);
        ToggleFrameCounter = config.Bind("Misc", "Toggle FrameCounter", new KeyboardShortcut(KeyCode.F4), --order);
        SwapBetweenFrameLimit = config.Bind("Misc", "Swap Between Frame Limit", new KeyboardShortcut(KeyCode.F5), --order);

        ToggleAllPanels = config.Bind("Panel", "Toggle All Panels", new KeyboardShortcut(KeyCode.F2), --order);
        ToggleBetweenPanels = config.Bind("Panel", "Toggle Between Panels", new KeyboardShortcut(KeyCode.F3), --order);

        CameraZoomIn = config.Bind("Camera", "Camera Zoom In", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftControl), --order);
        CameraZoomOut = config.Bind("Camera", "Camera Zoom Out", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftControl), --order);
        CameraZoomReset = config.Bind("Camera", "Camera Zoom Reset", new KeyboardShortcut(KeyCode.End, KeyCode.LeftControl), --order);

        AllowRecollectCoins = config.Bind("Coin", "Allow Recollect Coins", true, --order);
        ReduceCurrency = config.Bind("Coin", "Reduce Coin", new KeyboardShortcut(KeyCode.Alpha5), --order);
        AddCurrency = config.Bind("Coin", "Add Coin", new KeyboardShortcut(KeyCode.Alpha6), --order);
        
        DecreaseSpeed = config.Bind("Game Speed", "Decrease Game Speed", new KeyboardShortcut(KeyCode.Minus), --order);
        IncreaseSpeed = config.Bind("Game Speed", "Increase Game Speed", new KeyboardShortcut(KeyCode.Plus), --order);
        ResetSpeed = config.Bind("Game Speed", "Reset Game Speed", new KeyboardShortcut(KeyCode.Alpha0), --order);
        PauseResume = config.Bind("Game Speed", "Pause or Resume", new KeyboardShortcut(KeyCode.RightBracket), --order);
        FrameAdvance = config.Bind("Game Speed", "Frame Advance", new KeyboardShortcut(KeyCode.LeftBracket), --order);

        #if v1_3
        RelicLevel = config.Bind("DLC", "Relic Level", RelicLevels.Default);
        #endif

        FrogsPhaseOnePattern = config.Bind("RNG", "Ribby and Croaks Phase One", FrogsPhaseOnePatterns.Random, --order);
        FrogsPhaseFinalPattern = config.Bind("RNG", "Ribby and Croaks Final Phase", FrogsPhaseFinalPatterns.Random, --order);
        FlyingBlimpPhaseBlimp2Pattern = config.Bind("RNG", "Hilda Berg Second Blimp Phase", FlyingBlimpPhaseBlimp2Patterns.Random, --order);
        FlyingBlimpPhaseBlimp3Pattern = config.Bind("RNG", "Hilda Berg Third Blimp Phase", FlyingBlimpPhaseBlimp3Patterns.Random, --order);
        ClownDashDelay = config.Bind("RNG", "Beppi The Clown Phase One Bumper Delays", ClownDashDelays.Random, --order);
        DevilPhaseOnePattern = config.Bind("RNG", "The Devil Phase One", DevilPhaseOnePatterns.Random, --order);
        DevilPhaseOneHeadType = config.Bind("RNG", "The Devil Phase One Head Type", DevilPhaseOneHeadTypes.Random, --order);
        DevilPhaseOneDragonDirection = config.Bind("RNG", "The Devil Phase One Dragon Direction", DevilPhaseOneDragonDirections.Random, --order);
        DevilPhaseOneSpiderOffset = config.Bind("RNG", "The Devil Phase One Spider Offset", DevilPhaseOneSpiderOffsets.Random, --order);
        DevilPhaseOnePitchforkType = config.Bind("RNG", "The Devil Phase One Pitchfork Type", DevilPhaseOnePitchforkTypes.Random, --order);
        DevilPhaseTwoPattern = config.Bind("RNG", "The Devil Phase Two", DevilPhaseTwoPatterns.Random, --order);
        DevilPhaseTwoBombEyeDirection = config.Bind("RNG", "The Devil Phase Two Bomb Direction", DevilPhaseTwoBombEyeDirections.Random, --order);
        FlyingMermaidPhaseOnePattern = config.Bind("RNG", "Cala Maria Phase One", FlyingMermaidPhaseOnePatterns.Random, --order);


    }

    private void Update() {
        OnKeyUpdate?.Invoke();
    }
}