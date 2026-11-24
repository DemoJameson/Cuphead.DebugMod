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
    public static ConfigEntry<LobberCritSettings> GuaranteeLobberExCrit;
    public static ConfigEntry<KeyboardShortcut> InvincibilityOneFight;
    public static ConfigEntry<KeyboardShortcut> Gain5ExCards;
    public static ConfigEntry<KeyboardShortcut> Gain1ExCard;
    public static ConfigEntry<KeyboardShortcut> ReduceCurrency;
    public static ConfigEntry<KeyboardShortcut> AddCurrency;
    public static ConfigEntry<KeyboardShortcut> X10Damage;
    public static ConfigEntry<float> DamageMultiplier;
    public static ConfigEntry<KeyboardShortcut> NoDamage;
    public static ConfigEntry<KeyboardShortcut> LevelSelector;
    public static ConfigEntry<KeyboardShortcut> ToggleFrameCounter;
    public static ConfigEntry<KeyboardShortcut> SwapBetweenFrameLimit;
    public static ConfigEntry<KeyboardShortcut> FastForward;
    public static ConfigEntry<bool> FastForwardToggle;
    public static ConfigEntry<float> FastForwardSpeed;
    public static ConfigEntry<KeyboardShortcut> ClearCharmsSupers;
    public static ConfigEntry<KeyboardShortcut> QuickRestart;

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

    public static ConfigEntry<bool> RTATime;
    public static ConfigEntry<bool> IGTTime;
    public static ConfigEntry<bool> Goal;
    public static ConfigEntry<bool> Parries;
    public static ConfigEntry<bool> Supers;
    public static ConfigEntry<bool> Damage;
    public static ConfigEntry<bool> CurrentRank;
    public static ConfigEntry<bool> Difficulty;
    public static ConfigEntry<bool> DmgMultiplier;
    public static ConfigEntry<bool> PlayerCount;
    public static ConfigEntry<bool> CurrentScene;
    public static ConfigEntry<bool> WeaponCooldowns;
    public static ConfigEntry<bool> OnEXWeaponCooldown;
    public static ConfigEntry<bool> ScreenCoordinates;
    public static ConfigEntry<bool> JumpFrames;
    public static ConfigEntry<bool> QuadEXOffset;

#if v1_3
    public static ConfigEntry<RelicLevels> RelicLevel;
#endif

    public static ConfigEntry<bool> LoopWinScreen;

    public static ConfigEntry<bool> SkipToLastSafeSpace;

    public static ConfigEntry<RngOverride> RngOverride;
    public static ConfigEntry<bool> ShowSimpleExpertRNG;
    public static ConfigEntry<bool> ShowUnusedLevels;

    public static ConfigEntry<KeyboardShortcut> RngMenu;
    public static ConfigEntry<bool> EnablePatternForms;

    public static ConfigEntry<BeePhaseTwoPatternsEasy> BeePhaseTwoPatternEasy;
    public static ConfigEntry<BeePhaseTwoPatternsNormal> BeePhaseTwoPatternNormal;
    public static ConfigEntry<BeePhaseTwoPatternsHard> BeePhaseTwoPatternHard;
    public static ConfigEntry<BeePhaseTwoOrbsDirections> BeePhaseTwoOrbsDirection;
    public static ConfigEntry<BeePhaseTwoTrianglesDirections> BeePhaseTwoTrianglesDirection;
    public static ConfigEntry<bool> BeePerfectPlatforms;
    public static ConfigEntry<bool> BeeMissingPlatformPattern;
    public static ConfigEntry<RobotPhaseFinalGemColors> RobotPhaseFinalGemColor;
    public static ConfigEntry<SallyStageplayPatternsEasy> SallyStageplayPatternEasy;
    public static ConfigEntry<SallyStageplayPatternsNormalHard> SallyStageplayPatternNormalHard;
    public static ConfigEntry<SallyStageplayJumpCountsEasy> SallyStageplayJumpCountEasy;
    public static ConfigEntry<SallyStageplayJumpCountsNormalHard> SallyStageplayJumpCountNormalHard;
    public static ConfigEntry<SallyStageplayJumpTypesEasy> SallyStageplayJumpTypeEasy;
    public static ConfigEntry<SallyStageplayJumpTypesNormalHard> SallyStageplayJumpTypeNormalHard;
    public static ConfigEntry<SallyStageplayTeleportOffsetsEasy> SallyStageplayTeleportOffsetEasy;
    public static ConfigEntry<SallyStageplayTeleportOffsetsNormalHard> SallyStageplayTeleportOffsetNormalHard;
    public static ConfigEntry<MousePhaseOnePatternsEasy> MousePhaseOnePatternEasy;
    public static ConfigEntry<MousePhaseOnePatternsNormal> MousePhaseOnePatternNormal;
    public static ConfigEntry<MousePhaseOnePatternsHard> MousePhaseOnePatternHard;
    public static ConfigEntry<float> MouseCanMoveMaxXPosition;
    public static ConfigEntry<float> MouseBrokenCanMoveMaxXPosition;
    public static ConfigEntry<MouseCherryBombPatternsNormal> MouseCherryBombPatternNormal;
    public static ConfigEntry<MouseCherryBombPatternsHard> MouseCherryBombPatternHard;
    public static ConfigEntry<MouseCatapultPatternsEasy> MouseCatapultPatternEasy;
    public static ConfigEntry<MouseCatapultPatternsNormal> MouseCatapultPatternNormal;
    public static ConfigEntry<MouseCatapultPatternsHard> MouseCatapultPatternHard;
    public static ConfigEntry<PiratePhaseThreeGunPatternsEasy> PiratePhaseThreeGunPatternEasy;
    public static ConfigEntry<PiratePhaseFourGunPatternsEasy> PiratePhaseFourGunPatternEasy;
    public static ConfigEntry<PiratePhaseSevenGunPatternsEasy> PiratePhaseSevenGunPatternEasy;
    public static ConfigEntry<PiratePhaseOneGunPatternsNormal> PiratePhaseOneGunPatternNormal;
    public static ConfigEntry<PiratePhaseTwoGunPatternsNormal> PiratePhaseTwoGunPatternNormal;
    public static ConfigEntry<PiratePhaseThreeGunPatternsNormal> PiratePhaseThreeGunPatternNormal;
    public static ConfigEntry<PiratePhaseOneGunPatternsHard> PiratePhaseOneGunPatternHard;
    public static ConfigEntry<PiratePhaseTwoGunPatternsHard> PiratePhaseTwoGunPatternHard;
    public static ConfigEntry<PiratePhaseThreeGunPatternsHard> PiratePhaseThreeGunPatternHard;
    public static ConfigEntry<PiratePhaseFourPatternsEasy> PiratePhaseFourPatternEasy;
    public static ConfigEntry<PiratePhaseSevenPatternsEasy> PiratePhaseSevenPatternEasy;
    public static ConfigEntry<PiratePhaseTwoPatternsNormalHard> PiratePhaseTwoPatternNormalHard;
    public static ConfigEntry<PiratePhaseThreePatternsNormalHard> PiratePhaseThreePatternNormalHard;
    public static ConfigEntry<float> PirateDogFishDelay;
    public static ConfigEntry<FlyingMermaidPhaseOneFirstPatternsEasy> FlyingMermaidPhaseOneFirstPatternEasy;
    public static ConfigEntry<FlyingMermaidPhaseOneSecondPatternsEasy> FlyingMermaidPhaseOneSecondPatternEasy;
    public static ConfigEntry<FlyingMermaidPhaseOnePatternsNormalHard> FlyingMermaidPhaseOnePatternNormalHard;
    public static ConfigEntry<FlyingMermaidPhaseOneFishPatterns> FlyingMermaidPhaseOneFishPattern;
    public static ConfigEntry<FlyingMermaidPhaseOneSummonPatterns> FlyingMermaidPhaseOneSummonPattern;
    public static ConfigEntry<TrainPumpkinStartingDirections> TrainPumpkinStartingDirection;
    public static ConfigEntry<TrainStartingGhouls> TrainStartingGhoul;
    public static ConfigEntry<DicePalaceHeartPositions1> DicePalaceHeartPosition1;
    public static ConfigEntry<DicePalaceHeartPositions2> DicePalaceHeartPosition2;
    public static ConfigEntry<DicePalaceHeartPositions3> DicePalaceHeartPosition3;
    public static ConfigEntry<DicePalaceCigarSpitAttackCountsNormal> DicePalaceCigarSpitAttackCountNormal;
    public static ConfigEntry<DicePalaceCigarSpitAttackCountsHard> DicePalaceCigarSpitAttackCountHard;
    public static ConfigEntry<DicePalaceChipsPatternsNormal> DicePalaceChipsPatternNormal;
    public static ConfigEntry<DicePalaceChipsPatternsHard> DicePalaceChipsPatternHard;
    public static ConfigEntry<DicePalaceRabbitPatterns> DicePalaceRabbitPattern;
    public static ConfigEntry<DicePalaceRabbitParryDirections> DicePalaceRabbitParryDirection;
    public static ConfigEntry<DicePalaceRoulettePatterns> DicePalaceRoulettePattern;
    public static ConfigEntry<DicePalaceRouletteTwirlAmountsNormal> DicePalaceRouletteTwirlAmountNormal;

#if v1_3
    public static ConfigEntry<OldManPhaseOnePatterns> OldManPhaseOnePattern;
    public static ConfigEntry<OldManPhaseOnePlatformRemoveOrdersNormal> OldManPhaseOnePlatformRemoveOrderNormal;
    public static ConfigEntry<OldManPhaseTwoLeftPuppetPatternsEasy> OldManPhaseTwoLeftPuppetPatternEasy;
    public static ConfigEntry<OldManPhaseTwoRightPuppetPatternsEasy> OldManPhaseTwoRightPuppetPatternEasy;
    public static ConfigEntry<OldManPhaseTwoLeftNormalRightHardPuppetPatterns> OldManPhaseTwoLeftNormalRightHardPuppetPattern;
    public static ConfigEntry<OldManPhaseTwoRightNormalLeftHardPuppetPatterns> OldManPhaseTwoRightNormalLeftHardPuppetPattern;
    public static ConfigEntry<SnowCultPhaseOnePatterns> SnowCultPhaseOnePattern;
    public static ConfigEntry<SnowCultPhaseOneStartPositions> SnowCultPhaseOneStartPosition;
    public static ConfigEntry<SnowCultPhaseTwoPatterns> SnowCultPhaseTwoPattern;
    public static ConfigEntry<SnowCultPhaseThreePatterns> SnowCultPhaseThreePattern;
    public static ConfigEntry<float> AirplanePhaseOneAttackDelay;
    public static ConfigEntry<AirplanePhaseOnePatterns> AirplanePhaseOnePattern;
    public static ConfigEntry<AirplanePhaseOneSides> AirplanePhaseOneSide;
    public static ConfigEntry<AirplanePhaseOneParryPatterns> AirplanePhaseOneParryPattern;
    public static ConfigEntry<AirplanePhaseOneParryPatternsSimple> AirplanePhaseOneParryPatternSimple;
    public static ConfigEntry<AirplanePhaseTwoTerrierRotationDirections> AirplanePhaseTwoTerrierRotationDirection;
    public static ConfigEntry<AirplaneTurretPatterns> AirplaneTurretPattern;
    public static ConfigEntry<AirplaneTurretPatternsSimple> AirplaneTurretPatternSimple;
    public static ConfigEntry<AirplaneTurretPatternsHard> AirplaneTurretPatternHard;
    public static ConfigEntry<RumRunnersPhaseThreeSnoutPositions> RumRunnersPhaseThreeSnoutPosition;
    public static ConfigEntry<RumRunnersPhaseThreeSnoutActionsNormal> RumRunnersPhaseThreeSnoutActionNormal;
    public static ConfigEntry<RumRunnersPhaseThreeSnoutActionsHard> RumRunnersPhaseThreeSnoutActionHard;
    public static ConfigEntry<RumRunnersPhaseOneSpiderActionsEasy> RumRunnersPhaseOneSpiderActionEasy;
    public static ConfigEntry<RumRunnersPhaseOneSpiderActionsEasyNormal> RumRunnersPhaseOneSpiderActionEasyNormal;
    public static ConfigEntry<RumRunnersPhaseOneSpiderActionsHard> RumRunnersPhaseOneSpiderActionHard;
    public static ConfigEntry<RumRunnersPhaseOneSpiderInitialActions> RumRunnersPhaseOneSpiderInitialAction;
    public static ConfigEntry<RumRunnersPhaseOneSpiderPositionsEasy> RumRunnersPhaseOneSpiderPositionEasy;
    public static ConfigEntry<RumRunnersPhaseOneSpiderPositionsNormalHard> RumRunnersPhaseOneSpiderPositionNormalHard;
    public static ConfigEntry<RumRunnersPhaseOneSpiderActionPositions> RumRunnersPhaseOneSpiderActionPosition;
    public static ConfigEntry<RumRunnersPhaseOneMinePlacementsEasy> RumRunnersPhaseOneMinePlacementEasy;
    public static ConfigEntry<RumRunnersPhaseOneMinePlacementsNormal> RumRunnersPhaseOneMinePlacementNormal;
    public static ConfigEntry<RumRunnersPhaseOneMinePlacementsHard> RumRunnersPhaseOneMinePlacementHard;
    public static ConfigEntry<SaltbakerPhaseOneTablePatterns> SaltbakerPhaseOneTablePattern;
    public static ConfigEntry<SaltbakerPhaseOnePatterns> SaltbakerPhaseOnePattern;
    public static ConfigEntry<SaltbakerParryStringPatterns> SaltbakerParryStringPattern;
    public static ConfigEntry<SaltbakerPhaseThreeSawPatterns> SaltbakerPhaseThreeSawPattern;
#endif


    public static event Action OnKeyUpdate;

    public void Awake() {
        ConfigFile config = Plugin.Instance.Config;
        int order = 0;

        Hitboxes = config.Bind("Hitbox", "Hitboxes", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), --order);
        PlatformHitboxes = config.Bind("Hitbox", "Platform Hitboxes", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl), --order);
        HighlightingHitboxes = config.Bind("Hitbox", "Highlighting Hitboxes", new KeyboardShortcut(KeyCode.B, KeyCode.LeftControl), --order);

        MuteInBackground = config.Bind("Misc", "Mute In Background", false, --order);
        RunInBackground = config.Bind("Misc", "Run In Background", false, --order);
        IgnoreInputWhenLoseFocus = config.Bind("Misc", "Ignore Input When Lose Focus", false, --order);
        SkipTitleScreen = config.Bind("Misc", "Skip Title Screen", true, --order);
        QuickRestart = config.Bind("Misc", "Quick Restart", new KeyboardShortcut(KeyCode.R), --order);
        LevelSelector = config.Bind("Misc", "Level Selector", new KeyboardShortcut(KeyCode.BackQuote), --order);
        Gain5ExCards = config.Bind("Misc", "Gain 5 Ex Cards", new KeyboardShortcut(KeyCode.Alpha1), --order);
        Gain1ExCard = config.Bind("Misc", "Gain 1 Ex Card", new KeyboardShortcut(KeyCode.Alpha2), --order);
        ClearCharmsSupers = config.Bind("Misc", "Clear CHARMS and SUPERS", new KeyboardShortcut(KeyCode.Alpha3), --order);
        X10Damage = config.Bind("Misc", "Toggle Damage Multiplier", new KeyboardShortcut(KeyCode.Alpha4), --order);
        DamageMultiplier = config.Bind("Misc", "Damage Multiplier", 10f, --order);
        NoDamage = config.Bind("Misc", "No Damage", new KeyboardShortcut(KeyCode.Alpha6), --order);
        InvincibilityOneFight = config.Bind("Misc", "Invincibility One Fight", new KeyboardShortcut(KeyCode.Alpha5), --order);
        ToggleFrameCounter = config.Bind("Misc", "Toggle FrameCounter", new KeyboardShortcut(KeyCode.F4), --order);
        SwapBetweenFrameLimit = config.Bind("Misc", "Swap Between Frame Limit", new KeyboardShortcut(KeyCode.F5), --order);
        FastForward = config.Bind("Misc", "Fast Forward", new KeyboardShortcut(KeyCode.F6), --order);
        FastForwardToggle = config.Bind("Misc", "Toggle Fast Forward?", false, --order);
        FastForwardSpeed = config.Bind("Misc", "Fast Forward Speed (0-100, decimals allowed)", 5.0f, --order);
        GuaranteeLobberExCrit = config.Bind("Misc", "Lobber EX Crit", LobberCritSettings.Random, --order);

        ToggleBetweenPanels = config.Bind("Panel", "Toggle Between Panels", new KeyboardShortcut(KeyCode.F2), --order);

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

        RTATime = config.Bind("InfoHUD", "RTA Time", true, --order);
        IGTTime = config.Bind("InfoHUD", "IGT Time", true, --order);
        WeaponCooldowns = config.Bind("InfoHUD", "Weapon Cooldowns", true, --order);
        OnEXWeaponCooldown = config.Bind("InfoHUD", "Weapon Cooldown on EX", true, --order);
        Goal = config.Bind("InfoHUD", "Goal", false, --order);
        Parries = config.Bind("InfoHUD", "Parries", false, --order);
        Supers = config.Bind("InfoHUD", "Supers", false, --order);
        Damage = config.Bind("InfoHUD", "Damage", false, --order);
        CurrentRank = config.Bind("InfoHUD", "Current Rank", false, --order);
        Difficulty = config.Bind("InfoHUD", "Difficulty", false, --order);
        DmgMultiplier = config.Bind("InfoHUD", "Damage Multiplier", false, --order);
        PlayerCount = config.Bind("InfoHUD", "Player Count", false, --order);
        CurrentScene = config.Bind("InfoHUD", "Current Scene", false, --order);
        ScreenCoordinates = config.Bind("InfoHUD", "Screen Coordinates", false, --order);
        JumpFrames = config.Bind("InfoHUD", "Jump Frames", false, --order);
        QuadEXOffset = config.Bind("InfoHUD", "Goopy/Werner Quad Feedback", false, --order);

#if v1_3
        RelicLevel = config.Bind("DLC", "Relic Level", RelicLevels.Default);
#endif

        LoopWinScreen = config.Bind("Scoreboard", "Loop Scoreboard", false, --order);

        SkipToLastSafeSpace = config.Bind("King Dice", "Skip To Last Safe Space", false, --order);

        RngOverride = config.Bind("RNG", "RNG Override", BepInEx.CupheadDebugMod.Config.SettingsEnums.RngOverride.Custom, --order);
        ShowSimpleExpertRNG = config.Bind("RNG", "Show Simple / Expert RNG", false, --order);
        ConfigExtensions.SetDifficultySpecificOptionsVisible(ShowSimpleExpertRNG.Value);
        ShowSimpleExpertRNG.SettingChanged += ShowSimpleExpertRNGOnSettingChanged;
        ShowUnusedLevels = config.Bind("Misc", "Show Unused Levels / Items", false, --order);

        BeePhaseTwoPatternEasy = config.Bind("RNG Rumor Honeybottoms", "Phase 2 Simple", BeePhaseTwoPatternsEasy.Random, --order);
        BeePhaseTwoPatternNormal = config.Bind("RNG Rumor Honeybottoms", "Phase 2 Regular", BeePhaseTwoPatternsNormal.Random, --order);
        BeePhaseTwoPatternHard = config.Bind("RNG Rumor Honeybottoms", "Phase 2 Expert", BeePhaseTwoPatternsHard.Random, --order);
        BeePhaseTwoOrbsDirection = config.Bind("RNG Rumor Honeybottoms", "Phase 2 Orbs Direction", BeePhaseTwoOrbsDirections.Random, --order);
        BeePhaseTwoTrianglesDirection = config.Bind("RNG Rumor Honeybottoms", "Phase 2 Triangles Direction", BeePhaseTwoTrianglesDirections.Random, --order);
        BeePerfectPlatforms = config.Bind("RNG Rumor Honeybottoms", "Perfect Platforms", false, --order);
        BeeMissingPlatformPattern = config.Bind("RNG Rumor Honeybottoms", "Read Missing Platforms from file", false, --order);
        RobotPhaseFinalGemColor = config.Bind("RNG Dr. Kahls Robot", "Final Phase Gem Color", RobotPhaseFinalGemColors.Random, --order);
        SallyStageplayPatternEasy = config.Bind("RNG Sally Stageplay", "Phase 1 Simple", SallyStageplayPatternsEasy.Random, --order);
        SallyStageplayPatternNormalHard = config.Bind("RNG Sally Stageplay", "Phase 1 Regular/Expert", SallyStageplayPatternsNormalHard.Random, --order);
        SallyStageplayJumpCountEasy = config.Bind("RNG Sally Stageplay", "Phase 1 Jump Count Simple", SallyStageplayJumpCountsEasy.Random, --order);
        SallyStageplayJumpCountNormalHard = config.Bind("RNG Sally Stageplay", "Phase 1 Jump Count Regular/Expert", SallyStageplayJumpCountsNormalHard.Random, --order);
        SallyStageplayJumpTypeEasy = config.Bind("RNG Sally Stageplay", "Phase 1 Jump Type Simple", SallyStageplayJumpTypesEasy.Random, --order);
        SallyStageplayJumpTypeNormalHard = config.Bind("RNG Sally Stageplay", "Phase 1 Jump Type Regular/Expert", SallyStageplayJumpTypesNormalHard.Random, --order);
        SallyStageplayTeleportOffsetEasy = config.Bind("RNG Sally Stageplay", "Phase 1 Teleport Offset Simple", SallyStageplayTeleportOffsetsEasy.Random, --order);
        SallyStageplayTeleportOffsetNormalHard = config.Bind("RNG Sally Stageplay", "Phase 1 Teleport Offset Regular/Expert", SallyStageplayTeleportOffsetsNormalHard.Random, --order);
        MousePhaseOnePatternEasy = config.Bind("RNG Werner Werman", "Phase 1 Simple", MousePhaseOnePatternsEasy.Random, --order);
        MousePhaseOnePatternNormal = config.Bind("RNG Werner Werman", "Phase 1 Regular", MousePhaseOnePatternsNormal.Random, --order);
        MousePhaseOnePatternHard = config.Bind("RNG Werner Werman", "Phase 1 Expert", MousePhaseOnePatternsHard.Random, --order);
        MouseCanMoveMaxXPosition = config.Bind("RNG Werner Werman", "Phase 1 X Movement (250-400 for Simple, 230-425 for Regular, 250-450 for Expert. -1 for random)", -1f, --order);
        MouseBrokenCanMoveMaxXPosition = config.Bind("RNG Werner Werman", "Phase 2 X Movement (300-400 for Simple, 20-50 for Regular/Expert. -1 for random)", -1f, --order);
        MouseCherryBombPatternNormal = config.Bind("RNG Werner Werman", "Phase 1 Cherry Bomb Pattern Regular", MouseCherryBombPatternsNormal.Random, --order);
        MouseCherryBombPatternHard = config.Bind("RNG Werner Werman", "Phase 1 Cherry Bomb Pattern Expert", MouseCherryBombPatternsHard.Random, --order);
        MouseCatapultPatternEasy = config.Bind("RNG Werner Werman", "Phase 1 Catapult Pattern Simple", MouseCatapultPatternsEasy.Random, --order);
        MouseCatapultPatternNormal = config.Bind("RNG Werner Werman", "Phase 1 Catapult Pattern Regular", MouseCatapultPatternsNormal.Random, --order);
        MouseCatapultPatternHard = config.Bind("RNG Werner Werman", "Phase 1 Catapult Pattern Expert", MouseCatapultPatternsHard.Random, --order);
        PiratePhaseThreeGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase 3 Gun Simple", PiratePhaseThreeGunPatternsEasy.Random, --order);
        PiratePhaseFourGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase 4 Gun Simple", PiratePhaseFourGunPatternsEasy.Random, --order);
        PiratePhaseSevenGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase 7 Gun Simple", PiratePhaseSevenGunPatternsEasy.Random, --order);
        PiratePhaseOneGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase 1 Gun Regular", PiratePhaseOneGunPatternsNormal.Random, --order);
        PiratePhaseTwoGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase 2 Gun Regular", PiratePhaseTwoGunPatternsNormal.Random, --order);
        PiratePhaseThreeGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase 3 Gun Regular", PiratePhaseThreeGunPatternsNormal.Random, --order);
        PiratePhaseOneGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase 1 Gun Expert", PiratePhaseOneGunPatternsHard.Random, --order);
        PiratePhaseTwoGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase 2 Gun Expert", PiratePhaseTwoGunPatternsHard.Random, --order);
        PiratePhaseThreeGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase 3 Gun Expert", PiratePhaseThreeGunPatternsHard.Random, --order);
        PiratePhaseFourPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase 4 Simple", PiratePhaseFourPatternsEasy.Random, --order);
        PiratePhaseSevenPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase 7 Simple", PiratePhaseSevenPatternsEasy.Random, --order);
        PiratePhaseTwoPatternNormalHard = config.Bind("RNG Captain Brineybeard", "Phase 2 Regular/Expert", PiratePhaseTwoPatternsNormalHard.Random, --order);
        PiratePhaseThreePatternNormalHard = config.Bind("RNG Captain Brineybeard", "Phase 3 Regular/Expert", PiratePhaseThreePatternsNormalHard.Random, --order);
        PirateDogFishDelay = config.Bind("RNG Captain Brineybeard", "Dogfish Delay (0.3-0.5 for Simple, 1.3-2.0 for Regular, 0.9-1.3 for Expert. -1 for random)", -1f, --order);
        FlyingMermaidPhaseOneFirstPatternEasy = config.Bind("RNG Cala Maria", "Phase 1 First Simple", FlyingMermaidPhaseOneFirstPatternsEasy.Random, --order);
        FlyingMermaidPhaseOneSecondPatternEasy = config.Bind("RNG Cala Maria", "Phase 1 Second Simple", FlyingMermaidPhaseOneSecondPatternsEasy.Random, --order);
        FlyingMermaidPhaseOnePatternNormalHard = config.Bind("RNG Cala Maria", "Phase 1 Regular/Expert", FlyingMermaidPhaseOnePatternsNormalHard.Random, --order);
        FlyingMermaidPhaseOneFishPattern = config.Bind("RNG Cala Maria", "Phase 1 Fish", FlyingMermaidPhaseOneFishPatterns.Random, --order);
        FlyingMermaidPhaseOneSummonPattern = config.Bind("RNG Cala Maria", "Phase 1 Summon", FlyingMermaidPhaseOneSummonPatterns.Random, --order);
        TrainPumpkinStartingDirection = config.Bind("RNG Phantom Express", "Pumpkin Starting Direction", TrainPumpkinStartingDirections.Random, --order);
        TrainStartingGhoul = config.Bind("RNG Phantom Express", "Starting Ghoul", TrainStartingGhouls.Random, --order);
        DicePalaceHeartPosition1 = config.Bind("RNG King Dice", "First Heart", DicePalaceHeartPositions1.Random, --order);
        DicePalaceHeartPosition2 = config.Bind("RNG King Dice", "Second Heart", DicePalaceHeartPositions2.Random, --order);
        DicePalaceHeartPosition3 = config.Bind("RNG King Dice", "Third Heart", DicePalaceHeartPositions3.Random, --order);
        DicePalaceChipsPatternNormal = config.Bind("RNG King Dice", "Chips Bettigan Pattern Regular", DicePalaceChipsPatternsNormal.Random, --order);
        DicePalaceChipsPatternHard = config.Bind("RNG King Dice", "Chips Bettigan Pattern Expert", DicePalaceChipsPatternsHard.Random, --order);
        DicePalaceCigarSpitAttackCountNormal = config.Bind("RNG King Dice", "Mr. Wheezy Attack Count Regular", DicePalaceCigarSpitAttackCountsNormal.Random, --order);
        DicePalaceCigarSpitAttackCountHard = config.Bind("RNG King Dice", "Mr. Wheezy Attack Count Expert", DicePalaceCigarSpitAttackCountsHard.Random, --order);
        DicePalaceRabbitPattern = config.Bind("RNG King Dice", "Hopus Pocus Pattern", DicePalaceRabbitPatterns.Random, --order);
        DicePalaceRabbitParryDirection = config.Bind("RNG King Dice", "Hopus Pocus Parry Direction", DicePalaceRabbitParryDirections.Random, --order);
        DicePalaceRoulettePattern = config.Bind("RNG King Dice", "Pirouletta Pattern", DicePalaceRoulettePatterns.Random, --order);
        DicePalaceRouletteTwirlAmountNormal = config.Bind("RNG King Dice", "Pirouletta Twirl Amount Regular", DicePalaceRouletteTwirlAmountsNormal.Random, --order);

        // The Devil's RNG moved into the in-game RNG menu (profiles live in
        // BepInEx/config/Cuphead.DebugMod/rng_profiles.cfg); its legacy entries were removed.
        RngMenu = config.Bind("Menu", "Debug RNG Menu", new KeyboardShortcut(KeyCode.F7), --order);
        EnablePatternForms = config.Bind("Menu", "Enable Pattern Forms", false, --order);

#if v1_3
        OldManPhaseOnePattern = config.Bind("RNG Glumstone the Giant", "Phase One Pattern", OldManPhaseOnePatterns.Random, --order);
        OldManPhaseOnePlatformRemoveOrderNormal = config.Bind("RNG Glumstone The Giant", "Phase 1 Platform Fall Order Regular", OldManPhaseOnePlatformRemoveOrdersNormal.Random, --order);
        OldManPhaseTwoLeftPuppetPatternEasy = config.Bind("RNG Glumstone The Giant", "Phase 2 Left Puppet Simple", OldManPhaseTwoLeftPuppetPatternsEasy.Random, --order);
        OldManPhaseTwoRightPuppetPatternEasy = config.Bind("RNG Glumstone The Giant", "Phase 2 Right Puppet Simple", OldManPhaseTwoRightPuppetPatternsEasy.Random, --order);
        OldManPhaseTwoLeftNormalRightHardPuppetPattern = config.Bind("RNG Glumstone The Giant", "Phase 2 Left Puppet Regular / Right Expert", OldManPhaseTwoLeftNormalRightHardPuppetPatterns.Random, --order);
        OldManPhaseTwoRightNormalLeftHardPuppetPattern = config.Bind("RNG Glumstone The Giant", "Phase 2 Right Puppet Regular / Left Expert", OldManPhaseTwoRightNormalLeftHardPuppetPatterns.Random, --order);
        SnowCultPhaseOnePattern = config.Bind("RNG Mortimer Freeze", "Phase One Pattern", SnowCultPhaseOnePatterns.Random, --order);
        SnowCultPhaseOneStartPosition = config.Bind("RNG Mortimer Freeze", "Phase One First Attack Position", SnowCultPhaseOneStartPositions.Random, --order);
        SnowCultPhaseTwoPattern = config.Bind("RNG Mortimer Freeze", "Phase Two Pattern", SnowCultPhaseTwoPatterns.Random, --order);
        SnowCultPhaseThreePattern = config.Bind("RNG Mortimer Freeze", "Phase Three Pattern", SnowCultPhaseThreePatterns.Random, --order);
        AirplanePhaseOnePattern = config.Bind("RNG The Howling Aces", "Phase 1 Pattern", AirplanePhaseOnePatterns.Random, --order);
        AirplanePhaseOneSide = config.Bind("RNG The Howling Aces", "Phase 1 Side", AirplanePhaseOneSides.Random, --order);
        AirplanePhaseOneParryPattern = config.Bind("RNG The Howling Aces", "Phase 1 Parry Pattern", AirplanePhaseOneParryPatterns.Random, --order);
        AirplanePhaseOneParryPatternSimple = config.Bind("RNG The Howling Aces", "Phase 1 Parry Pattern Simple", AirplanePhaseOneParryPatternsSimple.Random, --order);
        AirplanePhaseOneAttackDelay = config.Bind("RNG The Howling Aces", "Phase 1 Attack Delay (2.5-4.0 for Simple, 2.0-3.5 for Regular, 1.5-3.0 for Expert. -1 for random)", -1f, --order);
        AirplanePhaseTwoTerrierRotationDirection = config.Bind("RNG The Howling Aces", "Phase 2 Rotation", AirplanePhaseTwoTerrierRotationDirections.Random, --order);
        AirplaneTurretPattern = config.Bind("RNG The Howling Aces", "Phase Three Turret Pattern", AirplaneTurretPatterns.Random, --order);
        AirplaneTurretPatternSimple = config.Bind("RNG The Howling Aces", "Phase Three Turret Pattern Simple", AirplaneTurretPatternsSimple.Random, --order);
        AirplaneTurretPatternHard = config.Bind("RNG The Howling Aces", "Phase Three Turret Pattern Expert", AirplaneTurretPatternsHard.Random, --order);
        RumRunnersPhaseOneSpiderActionEasy = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Pattern Simple", RumRunnersPhaseOneSpiderActionsEasy.Random, --order);
        RumRunnersPhaseOneSpiderActionEasyNormal = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Pattern Regular", RumRunnersPhaseOneSpiderActionsEasyNormal.Random, --order);
        RumRunnersPhaseOneSpiderActionHard = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Pattern Expert", RumRunnersPhaseOneSpiderActionsHard.Random, --order);
        RumRunnersPhaseOneSpiderInitialAction = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Initial Action", RumRunnersPhaseOneSpiderInitialActions.Random, --order);
        RumRunnersPhaseOneSpiderPositionEasy = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Position Simple", RumRunnersPhaseOneSpiderPositionsEasy.Random, --order);
        RumRunnersPhaseOneSpiderPositionNormalHard = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Position Regular/Expert", RumRunnersPhaseOneSpiderPositionsNormalHard.Random, --order);
        RumRunnersPhaseOneSpiderActionPosition = config.Bind("RNG Moonshine Mob", "Phase 1 Spider Action Position", RumRunnersPhaseOneSpiderActionPositions.Random, --order);
        RumRunnersPhaseOneMinePlacementEasy = config.Bind("RNG Moonshine Mob", "Phase 1 Mine Placement Simple", RumRunnersPhaseOneMinePlacementsEasy.Random, --order);
        RumRunnersPhaseOneMinePlacementNormal = config.Bind("RNG Moonshine Mob", "Phase 1 Mine Placement Regular", RumRunnersPhaseOneMinePlacementsNormal.Random, --order);
        RumRunnersPhaseOneMinePlacementHard = config.Bind("RNG Moonshine Mob", "Phase 1 Mine Placement Expert", RumRunnersPhaseOneMinePlacementsHard.Random, --order);
        RumRunnersPhaseThreeSnoutPosition = config.Bind("RNG Moonshine Mob", "Phase 3 Snout Position", RumRunnersPhaseThreeSnoutPositions.Random, --order);
        RumRunnersPhaseThreeSnoutActionNormal = config.Bind("RNG Moonshine Mob", "Phase 3 Snout Pattern Regular", RumRunnersPhaseThreeSnoutActionsNormal.Random, --order);
        RumRunnersPhaseThreeSnoutActionHard = config.Bind("RNG Moonshine Mob", "Phase 3 Snout Pattern Expert", RumRunnersPhaseThreeSnoutActionsHard.Random, --order);
        SaltbakerPhaseOneTablePattern = config.Bind("RNG Chef Saltbaker", "Phase One Table Pattern", SaltbakerPhaseOneTablePatterns.Random, --order);
        SaltbakerPhaseOnePattern = config.Bind("RNG Chef Saltbaker", "Phase 1 Pattern", SaltbakerPhaseOnePatterns.Random, --order);
        SaltbakerParryStringPattern = config.Bind("RNG Chef Saltbaker", "Sugarcubes Parry String", SaltbakerParryStringPatterns.Random, --order);
        SaltbakerPhaseThreeSawPattern = config.Bind("RNG Chef Saltbaker", "Phase 3 Saw Pattern", SaltbakerPhaseThreeSawPatterns.Random, --order);
        
#endif

    }

    private void ShowSimpleExpertRNGOnSettingChanged(object sender, EventArgs e) {
        ConfigExtensions.SetDifficultySpecificOptionsVisible(ShowSimpleExpertRNG.Value);
    }

    private void Update() {
        OnKeyUpdate?.Invoke();
    }
}
