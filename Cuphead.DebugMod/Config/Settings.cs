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


    public static ConfigEntry<ForestPlatformingAcornSpawnerDirections> ForestPlatformingAcornSpawnerDirection;
    public static ConfigEntry<ForestPlatformingAcornSpawnerYIndexes> ForestPlatformingAcornSpawnerYIndex;
    public static ConfigEntry<FrogsPhaseOnePatterns> FrogsPhaseOnePattern;
    public static ConfigEntry<FrogsPhaseOneFirefliesPatternsEasy> FrogsPhaseOneFirefliesPatternEasy;
    public static ConfigEntry<FrogsPhaseOneFirefliesPatternsNormal> FrogsPhaseOneFirefliesPatternNormal;
    public static ConfigEntry<FrogsPhaseOneFirefliesPatternsHard> FrogsPhaseOneFirefliesPatternHard;
    public static ConfigEntry<FrogsPhaseFinalPatterns> FrogsPhaseFinalPattern;
    public static ConfigEntry<FlyingBlimpPhaseBlimp2PatternsEasy> FlyingBlimpPhaseBlimp2PatternEasy;
    public static ConfigEntry<FlyingBlimpPhaseBlimp3PatternsEasy> FlyingBlimpPhaseBlimp3PatternEasy;
    public static ConfigEntry<FlyingBlimpConstellationPatternsNormal> FlyingBlimpConstellationPatternNormal;
    public static ConfigEntry<FlyingBlimpPhaseBlimp2PatternsNormal> FlyingBlimpPhaseBlimp2PatternNormal;
    public static ConfigEntry<FlyingBlimpPhaseBlimp3PatternsNormal> FlyingBlimpPhaseBlimp3PatternNormal;
    public static ConfigEntry<FlyingBlimpPhaseBlimp2PatternsHard> FlyingBlimpPhaseBlimp2PatternHard;
    public static ConfigEntry<FlyingBlimpPhaseBlimp3PatternsHard> FlyingBlimpPhaseBlimp3PatternHard;
    public static ConfigEntry<FlowerPhaseGeneric1PatternsNormal> FlowerPhaseGeneric1PatternNormal;
    public static ConfigEntry<FlowerPhaseGeneric2PatternsNormal> FlowerPhaseGeneric2PatternNormal;
    public static ConfigEntry<FlowerPhaseGeneric3PatternsNormal> FlowerPhaseGeneric3PatternNormal;
    public static ConfigEntry<FlowerPhaseGenericHeadLungePatternsNormal> FlowerPhaseGenericHeadLungePatternNormal;
    public static ConfigEntry<FlowerPodHandsAttackCountIndexesEasy> FlowerPodHandsAttackCountIndexEasy;
    public static ConfigEntry<FlowerPodHandsAttackCountIndexesNormal> FlowerPodHandsAttackCountIndexNormal;
    public static ConfigEntry<FlowerPodHandsAttackCountIndexesHard> FlowerPodHandsAttackCountIndexHard;
    public static ConfigEntry<FlowerPodHandsAttackTypeIndexesEasy> FlowerPodHandsAttackTypeIndexEasy;
    public static ConfigEntry<FlowerPodHandsAttackTypeIndexesNormal> FlowerPodHandsAttackTypeIndexNormal;
    public static ConfigEntry<FlowerPodHandsAttackTypeIndexesHard> FlowerPodHandsAttackTypeIndexHard;
    public static ConfigEntry<FlowerBlinkCounts> FlowerBlinkCount;
    public static ConfigEntry<BaronessMinibossesEasy> BaronessMiniboss1Easy;
    public static ConfigEntry<BaronessMinibossesEasy> BaronessMiniboss2Easy;
    public static ConfigEntry<BaronessMinibossesEasy> BaronessMiniboss3Easy;
    public static ConfigEntry<BaronessMinibossesNormal> BaronessMiniboss1Normal;
    public static ConfigEntry<BaronessMinibossesNormal> BaronessMiniboss2Normal;
    public static ConfigEntry<BaronessMinibossesNormal> BaronessMiniboss3Normal;
    public static ConfigEntry<BaronessMinibossesHard> BaronessMiniboss1Hard;
    public static ConfigEntry<BaronessMinibossesHard> BaronessMiniboss2Hard;
    public static ConfigEntry<BaronessMinibossesHard> BaronessMiniboss3Hard;
    public static ConfigEntry<FlyingBirdPhaseOneDirections> FlyingBirdPhaseOneDirection;
    public static ConfigEntry<FlyingBirdPhaseOnePatternsEasy> FlyingBirdPhaseOnePatternEasy;
    public static ConfigEntry<FlyingBirdPhaseTwoPatternsEasy> FlyingBirdPhaseTwoPatternEasy;
    public static ConfigEntry<FlyingBirdPhaseOnePatternsNormal> FlyingBirdPhaseOnePatternNormal;
    public static ConfigEntry<FlyingBirdPhaseTwoPatternsNormal> FlyingBirdPhaseTwoPatternNormal;
    public static ConfigEntry<FlyingBirdPhaseOnePatternsHard> FlyingBirdPhaseOnePatternHard;
    public static ConfigEntry<FlyingBirdPhaseThreeDirections> FlyingBirdPhaseThreeDirection;
    public static ConfigEntry<FlyingBirdPhaseFinalPatterns> FlyingBirdPhaseFinalPattern;
    public static ConfigEntry<FlyingBirdPhaseFinalDirections> FlyingBirdPhaseFinalDirection;
    public static ConfigEntry<FlyingGeniePhaseOneTreasurePatterns> FlyingGeniePhaseOneTreasurePattern;
    public static ConfigEntry<FlyingGeniePhaseOneSwordTypesEasyNormal> FlyingGeniePhaseOneSwordTypeEasyNormal;
    public static ConfigEntry<FlyingGeniePhaseOneSwordTypesHard> FlyingGeniePhaseOneSwordTypeHard;
    public static ConfigEntry<FlyingGeniePhaseOneGemsTypesEasy> FlyingGeniePhaseOneGemsTypeEasy;
    public static ConfigEntry<FlyingGeniePhaseOneGemsTypesNormalHard> FlyingGeniePhaseOneGemsTypeNormalHard;
    public static ConfigEntry<FlyingGeniePhaseTwoObeliskPatterns> FlyingGeniePhaseTwoObeliskPattern;
    public static ConfigEntry<FlyingGeniePhaseOneSphinxTypes> FlyingGeniePhaseOneSphinxType;
    public static ConfigEntry<ClownDashDelaysEasy> ClownDashDelayEasy;
    public static ConfigEntry<ClownDashDelaysNormal> ClownDashDelayNormal;
    public static ConfigEntry<ClownDashDelaysHard> ClownDashDelayHard;
    public static ConfigEntry<ClownHorseTypes> ClownHorseType;
    public static ConfigEntry<ClownHorseDirections> ClownHorseDirection;
    public static ConfigEntry<DragonPhaseOnePatternsEasy> DragonPhaseOnePatternEasy;
    public static ConfigEntry<DragonPhaseTwoPatternsEasy> DragonPhaseTwoPatternEasy;
    public static ConfigEntry<DragonPhaseThreePatternsNormal> DragonPhaseThreePatternNormal;
    public static ConfigEntry<DragonPhaseOnePatternsHard> DragonPhaseOnePatternHard;
    public static ConfigEntry<DragonPhaseTwoPatternsHard> DragonPhaseTwoPatternHard;
    public static ConfigEntry<DragonPhaseOneLaserPatternsEasy> DragonPhaseOneLaserPatternEasy;
    public static ConfigEntry<DragonPhaseTwoLaserPatternsEasy> DragonPhaseTwoLaserPatternEasy;
    public static ConfigEntry<DragonPhaseOneLaserPatternsNormal> DragonPhaseOneLaserPatternNormal;
    public static ConfigEntry<DragonPhaseThreeLaserPatternsNormal> DragonPhaseThreeLaserPatternNormal;
    public static ConfigEntry<DragonPhaseOneLaserPatternsHard> DragonPhaseOneLaserPatternHard;
    public static ConfigEntry<DragonPhaseTwoLaserPatternsHard> DragonPhaseTwoLaserPatternHard;
    public static ConfigEntry<DragonPhaseOneMeteorPatternsEasy> DragonPhaseOneMeteorPatternEasy;
    public static ConfigEntry<DragonPhaseTwoMeteorPatternsEasy> DragonPhaseTwoMeteorPatternEasy;
    public static ConfigEntry<DragonPhaseTwoMeteorPatternsNormal> DragonPhaseTwoMeteorPatternNormal;
    public static ConfigEntry<DragonPhaseThreeMeteorPatternsNormal> DragonPhaseThreeMeteorPatternNormal;
    public static ConfigEntry<DragonPhaseOneMeteorPatternsHard> DragonPhaseOneMeteorPatternHard;
    public static ConfigEntry<DragonPhaseTwoMeteorPatternsHard> DragonPhaseTwoMeteorPatternHard;
    public static ConfigEntry<BeePhaseTwoPatternsEasy> BeePhaseTwoPatternEasy;
    public static ConfigEntry<BeePhaseTwoPatternsNormal> BeePhaseTwoPatternNormal;
    public static ConfigEntry<BeePhaseTwoPatternsHard> BeePhaseTwoPatternHard;
    public static ConfigEntry<BeePhaseTwoOrbsDirections> BeePhaseTwoOrbsDirection;
    public static ConfigEntry<BeePhaseTwoTrianglesDirections> BeePhaseTwoTrianglesDirection;
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
    public static ConfigEntry<DicePalaceRabbitPatterns> DicePalaceRabbitPattern;
    public static ConfigEntry<DicePalaceRabbitParryDirections> DicePalaceRabbitParryDirection;
    public static ConfigEntry<DicePalaceRoulettePatterns> DicePalaceRoulettePattern;
    public static ConfigEntry<DicePalaceRouletteTwirlAmountsNormal> DicePalaceRouletteTwirlAmountNormal;
    public static ConfigEntry<float> DevilClapDelay;
    public static ConfigEntry<DevilPhaseOnePatterns> DevilPhaseOnePattern;
    public static ConfigEntry<DevilPhaseOneHeadTypes> DevilPhaseOneHeadType;
    public static ConfigEntry<DevilPhaseOneDragonDirections> DevilPhaseOneDragonDirection;
    public static ConfigEntry<DevilPhaseOneSpiderOffsets> DevilPhaseOneSpiderOffset;
    public static ConfigEntry<DevilPhaseOneSpiderHopCounts> DevilPhaseOneSpiderHopCount;
    public static ConfigEntry<float> DevilSpiderDelay;
    public static ConfigEntry<DevilPhaseOnePitchforkTypes> DevilPhaseOnePitchforkType;
    public static ConfigEntry<DevilPhaseOneBouncerAnglesNormal> DevilPhaseOneBouncerAngleNormal;
    public static ConfigEntry<DevilPhaseOneBouncerAnglesHard> DevilPhaseOneBouncerAngleHard;
    public static ConfigEntry<DevilPhaseTwoPatternsNormal> DevilPhaseTwoPatternNormal;
    public static ConfigEntry<DevilPhaseTwoPatternsHard> DevilPhaseTwoPatternHard;
    public static ConfigEntry<DevilPhaseTwoBombEyeDirections> DevilPhaseTwoBombEyeDirection;
    public static ConfigEntry<string> DevilTest;

#if v1_3
    public static ConfigEntry<RumRunnersSnoutPositions> RunRunnersSnoutPosition;
    public static ConfigEntry<SaltbakerPhaseOnePatterns> SaltbakerPhaseOnePattern;
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
        X10Damage = config.Bind("Misc", "x10 Damage", new KeyboardShortcut(KeyCode.Alpha4), --order);
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

        ForestPlatformingAcornSpawnerDirection = config.Bind("RNG Forest Follies", "Acorn Spawner Facing Direction", ForestPlatformingAcornSpawnerDirections.Random, --order);
        ForestPlatformingAcornSpawnerYIndex = config.Bind("RNG Forest Follies", "Acorn Spawner Y Coordinate", ForestPlatformingAcornSpawnerYIndexes.Random, --order);
        FrogsPhaseOnePattern = config.Bind("RNG Ribby And Croaks", "Phase One Pattern", FrogsPhaseOnePatterns.Random, --order);
        FrogsPhaseOneFirefliesPatternEasy = config.Bind("RNG Ribby And Croaks", "Phase One Fireflies Pattern Simple", FrogsPhaseOneFirefliesPatternsEasy.Random, --order);
        FrogsPhaseOneFirefliesPatternNormal = config.Bind("RNG Ribby And Croaks", "Phase One Fireflies Pattern Regular", FrogsPhaseOneFirefliesPatternsNormal.Random, --order);
        FrogsPhaseOneFirefliesPatternHard = config.Bind("RNG Ribby And Croaks", "Phase One Fireflies Pattern Expert", FrogsPhaseOneFirefliesPatternsHard.Random, --order);
        FrogsPhaseFinalPattern = config.Bind("RNG Ribby And Croaks", "Final Phase Pattern", FrogsPhaseFinalPatterns.Random, --order);
        FlyingBlimpPhaseBlimp2PatternEasy = config.Bind("RNG Hilda Berg", "Second Blimp Phase Simple", FlyingBlimpPhaseBlimp2PatternsEasy.Random, --order);
        FlyingBlimpPhaseBlimp3PatternEasy = config.Bind("RNG Hilda Berg", "Third Blimp Phase Simple", FlyingBlimpPhaseBlimp3PatternsEasy.Random, --order);
        FlyingBlimpConstellationPatternNormal = config.Bind("RNG Hilda Berg", "Constellation Phase Regular", FlyingBlimpConstellationPatternsNormal.Random, --order);
        FlyingBlimpPhaseBlimp2PatternNormal = config.Bind("RNG Hilda Berg", "Second Blimp Phase Regular", FlyingBlimpPhaseBlimp2PatternsNormal.Random, --order);
        FlyingBlimpPhaseBlimp3PatternNormal = config.Bind("RNG Hilda Berg", "Third Blimp Phase Regular", FlyingBlimpPhaseBlimp3PatternsNormal.Random, --order);
        FlyingBlimpPhaseBlimp2PatternHard = config.Bind("RNG Hilda Berg", "Second Blimp Phase Expert", FlyingBlimpPhaseBlimp2PatternsHard.Random, --order);
        FlyingBlimpPhaseBlimp3PatternHard = config.Bind("RNG Hilda Berg", "Third Blimp Phase Expert", FlyingBlimpPhaseBlimp3PatternsHard.Random, --order);
        FlowerPhaseGeneric1PatternNormal = config.Bind("RNG Cagney Carnation", "First Generic Phase Regular", FlowerPhaseGeneric1PatternsNormal.Random, --order);
        FlowerPhaseGeneric2PatternNormal = config.Bind("RNG Cagney Carnation", "Second Generic Phase Regular", FlowerPhaseGeneric2PatternsNormal.Random, --order);
        FlowerPhaseGeneric3PatternNormal = config.Bind("RNG Cagney Carnation", "Third Generic Phase Regular", FlowerPhaseGeneric3PatternsNormal.Random, --order);
        FlowerPhaseGenericHeadLungePatternNormal = config.Bind("RNG Cagney Carnation", "Generic Phase Head Lunge Regular", FlowerPhaseGenericHeadLungePatternsNormal.Random, --order);
        FlowerPodHandsAttackCountIndexEasy = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Count Index Simple", FlowerPodHandsAttackCountIndexesEasy.Random, --order);
        FlowerPodHandsAttackCountIndexNormal = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Count Index Regular", FlowerPodHandsAttackCountIndexesNormal.Random, --order);
        FlowerPodHandsAttackCountIndexHard = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Count Index Expert", FlowerPodHandsAttackCountIndexesHard.Random, --order);
        FlowerPodHandsAttackTypeIndexEasy = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Type Index Simple", FlowerPodHandsAttackTypeIndexesEasy.Random, --order);
        FlowerPodHandsAttackTypeIndexNormal = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Type Index Regular", FlowerPodHandsAttackTypeIndexesNormal.Random, --order);
        FlowerPodHandsAttackTypeIndexHard = config.Bind("RNG Cagney Carnation", "Pod Hands Attack Type Index Expert", FlowerPodHandsAttackTypeIndexesHard.Random, --order);
        FlowerBlinkCount = config.Bind("RNG Cagney Carnation", "Blink Count", FlowerBlinkCounts.Random, --order);
        BaronessMiniboss1Easy = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 1 Simple", BaronessMinibossesEasy.Random, --order);
        BaronessMiniboss2Easy = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 2 Simple", BaronessMinibossesEasy.Random, --order);
        BaronessMiniboss3Easy = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 3 Simple", BaronessMinibossesEasy.Random, --order);
        BaronessMiniboss1Normal = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 1 Regular", BaronessMinibossesNormal.Random, --order);
        BaronessMiniboss2Normal = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 2 Regular", BaronessMinibossesNormal.Random, --order);
        BaronessMiniboss3Normal = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 3 Regular", BaronessMinibossesNormal.Random, --order);
        BaronessMiniboss1Hard = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 1 Expert", BaronessMinibossesHard.Random, --order);
        BaronessMiniboss2Hard = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 2 Expert", BaronessMinibossesHard.Random, --order);
        BaronessMiniboss3Hard = config.Bind("RNG Baroness Von Bon Bon", "Miniboss 3 Expert", BaronessMinibossesHard.Random, --order);
        FlyingBirdPhaseOneDirection = config.Bind("RNG Wally Warbles", "Phase One Direction", FlyingBirdPhaseOneDirections.Random, --order);
        FlyingBirdPhaseOnePatternEasy = config.Bind("RNG Wally Warbles", "Phase One Pattern Simple", FlyingBirdPhaseOnePatternsEasy.Random, --order);
        FlyingBirdPhaseTwoPatternEasy = config.Bind("RNG Wally Warbles", "Phase Two Pattern Simple", FlyingBirdPhaseTwoPatternsEasy.Random, --order);
        FlyingBirdPhaseOnePatternNormal = config.Bind("RNG Wally Warbles", "Phase One Pattern Regular", FlyingBirdPhaseOnePatternsNormal.Random, --order);
        FlyingBirdPhaseTwoPatternNormal = config.Bind("RNG Wally Warbles", "Phase Two Pattern Regular", FlyingBirdPhaseTwoPatternsNormal.Random, --order);
        FlyingBirdPhaseThreeDirection = config.Bind("RNG Wally Warbles", "Phase Three Direction", FlyingBirdPhaseThreeDirections.Random, --order);
        FlyingBirdPhaseOnePatternHard = config.Bind("RNG Wally Warbles", "Phase One Pattern Expert", FlyingBirdPhaseOnePatternsHard.Random, --order);
        FlyingBirdPhaseFinalPattern = config.Bind("RNG Wally Warbles", "Final Phase Pattern", FlyingBirdPhaseFinalPatterns.Random, --order);
        FlyingBirdPhaseFinalDirection = config.Bind("RNG Wally Warbles", "Final Phase Direction", FlyingBirdPhaseFinalDirections.Random, --order);
        FlyingGeniePhaseOneTreasurePattern = config.Bind("RNG Djimmi The Great", "Phase One Treasure", FlyingGeniePhaseOneTreasurePatterns.Random, --order);
        FlyingGeniePhaseOneSwordTypeEasyNormal = config.Bind("RNG Djimmi The Great", "Phase One Sword Parry Pattern Simple/Regular", FlyingGeniePhaseOneSwordTypesEasyNormal.Random, --order);
        FlyingGeniePhaseOneSwordTypeHard = config.Bind("RNG Djimmi The Great", "Phase One Sword Parry Pattern Expert", FlyingGeniePhaseOneSwordTypesHard.Random, --order);
        FlyingGeniePhaseOneGemsTypeEasy = config.Bind("RNG Djimmi The Great", "Phase One Gems Parry Pattern Simple", FlyingGeniePhaseOneGemsTypesEasy.Random, --order);
        FlyingGeniePhaseOneGemsTypeNormalHard = config.Bind("RNG Djimmi The Great", "Phase One Gems Parry Pattern Regular/Expert", FlyingGeniePhaseOneGemsTypesNormalHard.Random, --order);
        FlyingGeniePhaseOneSphinxType = config.Bind("RNG Djimmi The Great", "Phase One Sphinx Parry Pattern", FlyingGeniePhaseOneSphinxTypes.Random, --order);
        FlyingGeniePhaseTwoObeliskPattern = config.Bind("RNG Djimmi The Great", "Phase Two Face Positions", FlyingGeniePhaseTwoObeliskPatterns.Random, --order);
        ClownDashDelayEasy = config.Bind("RNG Beppi The Clown", "Phase One Bumper Delays Simple", ClownDashDelaysEasy.Random, --order);
        ClownDashDelayNormal = config.Bind("RNG Beppi The Clown", "Phase One Bumper Delays Regular", ClownDashDelaysNormal.Random, --order);
        ClownDashDelayHard = config.Bind("RNG Beppi The Clown", "Phase One Bumper Delays Expert", ClownDashDelaysHard.Random, --order);
        ClownHorseType = config.Bind("RNG Beppi The Clown", "Phase Three Horse Type", ClownHorseTypes.Random, --order);
        ClownHorseDirection = config.Bind("RNG Beppi The Clown", "Phase Three Horse Direction", ClownHorseDirections.Random, --order);
        DragonPhaseOnePatternEasy = config.Bind("RNG Grim Matchstick", "Phase One Pattern Simple", DragonPhaseOnePatternsEasy.Random, --order);
        DragonPhaseTwoPatternEasy = config.Bind("RNG Grim Matchstick", "Phase Two Pattern Simple", DragonPhaseTwoPatternsEasy.Random, --order);
        DragonPhaseThreePatternNormal = config.Bind("RNG Grim Matchstick", "Phase Three Pattern Regular", DragonPhaseThreePatternsNormal.Random, --order);
        DragonPhaseOnePatternHard = config.Bind("RNG Grim Matchstick", "Phase One Pattern Expert", DragonPhaseOnePatternsHard.Random, --order);
        DragonPhaseTwoPatternHard = config.Bind("RNG Grim Matchstick", "Phase Two Pattern Expert", DragonPhaseTwoPatternsHard.Random, --order);
        DragonPhaseOneLaserPatternEasy = config.Bind("RNG Grim Matchstick", "Phase One Laser Pattern Simple", DragonPhaseOneLaserPatternsEasy.Random, --order);
        DragonPhaseTwoLaserPatternEasy = config.Bind("RNG Grim Matchstick", "Phase Two Laser Pattern Simple", DragonPhaseTwoLaserPatternsEasy.Random, --order);
        DragonPhaseOneLaserPatternNormal = config.Bind("RNG Grim Matchstick", "Phase One Laser Pattern Regular", DragonPhaseOneLaserPatternsNormal.Random, --order);
        DragonPhaseThreeLaserPatternNormal = config.Bind("RNG Grim Matchstick", "Phase Three Laser Pattern Regular", DragonPhaseThreeLaserPatternsNormal.Random, --order);
        DragonPhaseOneLaserPatternHard = config.Bind("RNG Grim Matchstick", "Phase One Laser Pattern Expert", DragonPhaseOneLaserPatternsHard.Random, --order);
        DragonPhaseTwoLaserPatternHard = config.Bind("RNG Grim Matchstick", "Phase Two Laser Pattern Expert", DragonPhaseTwoLaserPatternsHard.Random, --order);
        DragonPhaseOneMeteorPatternEasy = config.Bind("RNG Grim Matchstick", "Phase One Meteor Pattern Simple", DragonPhaseOneMeteorPatternsEasy.Random, --order);
        DragonPhaseTwoMeteorPatternEasy = config.Bind("RNG Grim Matchstick", "Phase Two Meteor Pattern Simple", DragonPhaseTwoMeteorPatternsEasy.Random, --order);
        DragonPhaseTwoMeteorPatternNormal = config.Bind("RNG Grim Matchstick", "Phase Two Meteor Pattern Regular", DragonPhaseTwoMeteorPatternsNormal.Random, --order);
        DragonPhaseThreeMeteorPatternNormal = config.Bind("RNG Grim Matchstick", "Phase Three Meteor Pattern Regular", DragonPhaseThreeMeteorPatternsNormal.Random, --order);
        DragonPhaseOneMeteorPatternHard = config.Bind("RNG Grim Matchstick", "Phase One Meteor Pattern Expert", DragonPhaseOneMeteorPatternsHard.Random, --order);
        DragonPhaseTwoMeteorPatternHard = config.Bind("RNG Grim Matchstick", "Phase Two Meteor Pattern Expert", DragonPhaseTwoMeteorPatternsHard.Random, --order);
        BeePhaseTwoPatternEasy = config.Bind("RNG Rumor Honeybottoms", "Phase Two Simple", BeePhaseTwoPatternsEasy.Random, --order);
        BeePhaseTwoPatternNormal = config.Bind("RNG Rumor Honeybottoms", "Phase Two Regular", BeePhaseTwoPatternsNormal.Random, --order);
        BeePhaseTwoPatternHard = config.Bind("RNG Rumor Honeybottoms", "Phase Two Expert", BeePhaseTwoPatternsHard.Random, --order);
        BeePhaseTwoOrbsDirection = config.Bind("RNG Rumor Honeybottoms", "Phase Two Orbs Direction", BeePhaseTwoOrbsDirections.Random, --order);
        BeePhaseTwoTrianglesDirection = config.Bind("RNG Rumor Honeybottoms", "Phase Two Triangles Direction", BeePhaseTwoTrianglesDirections.Random, --order);
        BeeMissingPlatformPattern = config.Bind("RNG Rumor Honeybottoms", "Read Missing Platforms from file", false, --order);
        RobotPhaseFinalGemColor = config.Bind("RNG Dr. Kahls Robot", "Final Phase Gem Color", RobotPhaseFinalGemColors.Random, --order);
        SallyStageplayPatternEasy = config.Bind("RNG Sally Stageplay", "Phase One Simple", SallyStageplayPatternsEasy.Random, --order);
        SallyStageplayPatternNormalHard = config.Bind("RNG Sally Stageplay", "Phase One Regular/Expert", SallyStageplayPatternsNormalHard.Random, --order);
        SallyStageplayJumpCountEasy = config.Bind("RNG Sally Stageplay", "Phase One Jump Count Simple", SallyStageplayJumpCountsEasy.Random, --order);
        SallyStageplayJumpCountNormalHard = config.Bind("RNG Sally Stageplay", "Phase One Jump Count Regular/Expert", SallyStageplayJumpCountsNormalHard.Random, --order);
        SallyStageplayJumpTypeEasy = config.Bind("RNG Sally Stageplay", "Phase One Jump Type Simple", SallyStageplayJumpTypesEasy.Random, --order);
        SallyStageplayJumpTypeNormalHard = config.Bind("RNG Sally Stageplay", "Phase One Jump Type Regular/Expert", SallyStageplayJumpTypesNormalHard.Random, --order);
        SallyStageplayTeleportOffsetEasy = config.Bind("RNG Sally Stageplay", "Phase One Teleport Offset Simple", SallyStageplayTeleportOffsetsEasy.Random, --order);
        SallyStageplayTeleportOffsetNormalHard = config.Bind("RNG Sally Stageplay", "Phase One Teleport Offset Regular/Expert", SallyStageplayTeleportOffsetsNormalHard.Random, --order);
        MousePhaseOnePatternEasy = config.Bind("RNG Werner Werman", "Phase One Simple", MousePhaseOnePatternsEasy.Random, --order);
        MousePhaseOnePatternNormal = config.Bind("RNG Werner Werman", "Phase One Regular", MousePhaseOnePatternsNormal.Random, --order);
        MousePhaseOnePatternHard = config.Bind("RNG Werner Werman", "Phase One Expert", MousePhaseOnePatternsHard.Random, --order);
        PiratePhaseThreeGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase Three Gun Simple", PiratePhaseThreeGunPatternsEasy.Random, --order);
        PiratePhaseFourGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase Four Gun Simple", PiratePhaseFourGunPatternsEasy.Random, --order);
        PiratePhaseSevenGunPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase Seven Gun Simple", PiratePhaseSevenGunPatternsEasy.Random, --order);
        PiratePhaseOneGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase One Gun Regular", PiratePhaseOneGunPatternsNormal.Random, --order);
        PiratePhaseTwoGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase Two Gun Regular", PiratePhaseTwoGunPatternsNormal.Random, --order);
        PiratePhaseThreeGunPatternNormal = config.Bind("RNG Captain Brineybeard", "Phase Three Gun Regular", PiratePhaseThreeGunPatternsNormal.Random, --order);
        PiratePhaseOneGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase One Gun Expert", PiratePhaseOneGunPatternsHard.Random, --order);
        PiratePhaseTwoGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase Two Gun Expert", PiratePhaseTwoGunPatternsHard.Random, --order);
        PiratePhaseThreeGunPatternHard = config.Bind("RNG Captain Brineybeard", "Phase Three Gun Expert", PiratePhaseThreeGunPatternsHard.Random, --order);
        PiratePhaseFourPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase Four Simple", PiratePhaseFourPatternsEasy.Random, --order);
        PiratePhaseSevenPatternEasy = config.Bind("RNG Captain Brineybeard", "Phase Seven Simple", PiratePhaseSevenPatternsEasy.Random, --order);
        PiratePhaseTwoPatternNormalHard = config.Bind("RNG Captain Brineybeard", "Phase Two Regular/Expert", PiratePhaseTwoPatternsNormalHard.Random, --order);
        PiratePhaseThreePatternNormalHard = config.Bind("RNG Captain Brineybeard", "Phase Three Regular/Expert", PiratePhaseThreePatternsNormalHard.Random, --order);
        PirateDogFishDelay = config.Bind("RNG Captain Brineybeard", "Dogfish Delay (0.3-0.5 for Simple, 1.3-2.0 for Regular, 0.9-1.3 on Expert. -1 for random)", -1f, --order);
        FlyingMermaidPhaseOneFirstPatternEasy = config.Bind("RNG Cala Maria", "Phase One First Simple", FlyingMermaidPhaseOneFirstPatternsEasy.Random, --order);
        FlyingMermaidPhaseOneSecondPatternEasy = config.Bind("RNG Cala Maria", "Phase One Second Simple", FlyingMermaidPhaseOneSecondPatternsEasy.Random, --order);
        FlyingMermaidPhaseOnePatternNormalHard = config.Bind("RNG Cala Maria", "Phase One Regular/Expert", FlyingMermaidPhaseOnePatternsNormalHard.Random, --order);
        FlyingMermaidPhaseOneFishPattern = config.Bind("RNG Cala Maria", "Phase One Fish", FlyingMermaidPhaseOneFishPatterns.Random, --order);
        FlyingMermaidPhaseOneSummonPattern = config.Bind("RNG Cala Maria", "Phase One Summon", FlyingMermaidPhaseOneSummonPatterns.Random, --order);
        TrainPumpkinStartingDirection = config.Bind("RNG Phantom Express", "Pumpkin Starting Direction", TrainPumpkinStartingDirections.Random, --order);
        TrainStartingGhoul = config.Bind("RNG Phantom Express", "Starting Ghoul", TrainStartingGhouls.Random, --order);
        DicePalaceHeartPosition1 = config.Bind("RNG King Dice", "First Heart", DicePalaceHeartPositions1.Random, --order);
        DicePalaceHeartPosition2 = config.Bind("RNG King Dice", "Second Heart", DicePalaceHeartPositions2.Random, --order);
        DicePalaceHeartPosition3 = config.Bind("RNG King Dice", "Third Heart", DicePalaceHeartPositions3.Random, --order);
        DicePalaceCigarSpitAttackCountNormal = config.Bind("RNG King Dice", "Mr. Wheezy Attack Count Regular", DicePalaceCigarSpitAttackCountsNormal.Random, --order);
        DicePalaceCigarSpitAttackCountHard = config.Bind("RNG King Dice", "Mr. Wheezy Attack Count Expert", DicePalaceCigarSpitAttackCountsHard.Random, --order);
        DicePalaceRabbitPattern = config.Bind("RNG King Dice", "Hopus Pocus Pattern", DicePalaceRabbitPatterns.Random, --order);
        DicePalaceRabbitParryDirection = config.Bind("RNG King Dice", "Hopus Pocus Parry Direction", DicePalaceRabbitParryDirections.Random, --order);
        DicePalaceRoulettePattern = config.Bind("RNG King Dice", "Pirouletta Pattern", DicePalaceRoulettePatterns.Random, --order);
        DicePalaceRouletteTwirlAmountNormal = config.Bind("RNG King Dice", "Pirouletta Twirl Amount Regular", DicePalaceRouletteTwirlAmountsNormal.Random, --order);
        DevilClapDelay = config.Bind("RNG The Devil", "Phase One Clap Delay (0.1-0.5, -1 for random)", -1f, --order);
        DevilPhaseOnePattern = config.Bind("RNG The Devil", "Phase One Pattern", DevilPhaseOnePatterns.Random, --order);
        DevilPhaseOneHeadType = config.Bind("RNG The Devil", "Phase One Head Type", DevilPhaseOneHeadTypes.Random, --order);
        DevilPhaseOneDragonDirection = config.Bind("RNG The Devil", "Phase One Dragon Direction", DevilPhaseOneDragonDirections.Random, --order);
        DevilPhaseOneSpiderOffset = config.Bind("RNG The Devil", "Phase One Spider Offset", DevilPhaseOneSpiderOffsets.Random, --order);
        DevilPhaseOneSpiderHopCount = config.Bind("RNG The Devil", "Phase One Spider Hop Count", DevilPhaseOneSpiderHopCounts.Random, --order);
        DevilSpiderDelay = config.Bind("RNG The Devil", "Phase One Spider Delay (0.3-0.7, -1 for random)", -1f, --order);
        DevilPhaseOnePitchforkType = config.Bind("RNG The Devil", "Phase One Pitchfork Type", DevilPhaseOnePitchforkTypes.Random, --order);
        DevilPhaseOneBouncerAngleNormal = config.Bind("RNG The Devil", "Phase One Bouncer Angle Regular", DevilPhaseOneBouncerAnglesNormal.Random, --order);
        DevilPhaseOneBouncerAngleHard = config.Bind("RNG The Devil", "Phase One Bouncer Angle Expert", DevilPhaseOneBouncerAnglesHard.Random, --order);
        DevilPhaseTwoPatternNormal = config.Bind("RNG The Devil", "Phase Two Regular", DevilPhaseTwoPatternsNormal.Random, --order);
        DevilPhaseTwoPatternHard = config.Bind("RNG The Devil", "Phase Two Expert", DevilPhaseTwoPatternsHard.Random, --order);
        DevilPhaseTwoBombEyeDirection = config.Bind("RNG The Devil", "Phase Two Bomb Direction", DevilPhaseTwoBombEyeDirections.Random, --order);
        DevilTest = config.Bind("RNG The Devil", "EXPERIMENTAL - Spider Offset Selection", "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19", --order);

#if v1_3
        RunRunnersSnoutPosition = config.Bind("RNG Moonshine Mob", "Snout Position", RumRunnersSnoutPositions.Random, --order);
        SaltbakerPhaseOnePattern = config.Bind("RNG Chef Saltbaker", "Phase One Pattern", SaltbakerPhaseOnePatterns.Random, --order);
        SaltbakerPhaseThreeSawPattern = config.Bind("RNG Chef Saltbaker", "Phase Three Saw Pattern", SaltbakerPhaseThreeSawPatterns.Random, --order);
#endif

    }

    private void Update() {
        OnKeyUpdate?.Invoke();
    }
}