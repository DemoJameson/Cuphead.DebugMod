using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using BepInEx.Configuration;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

public class RngOverrideComponent : PluginComponent {
    private readonly Dictionary<string, object> originalValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, object> explicitOverrides = new Dictionary<string, object>(StringComparer.Ordinal) {
        { "FrogsPhaseOnePattern", FrogsPhaseOnePatterns.Fireflies },
        { "FrogsPhaseOneFirefliesPatternNormal", FrogsPhaseOneFirefliesPatternsNormal.Two_Two_One },
        { "FrogsPhaseFinalPattern", FrogsPhaseFinalPatterns.Bison },
        { "SlimePhaseOneJumpCountNormal", SlimePhaseOneJumpCountsNormal.Seven },
        { "SlimePhaseOneJumpPatternNormal", SlimePhaseOneJumpPatternsNormal.Low08 },
        { "FlyingBlimpConstellationPatternNormal", FlyingBlimpConstellationPatternsNormal.Gemini },
        { "FlyingBlimpPhaseBlimp2PatternNormal", FlyingBlimpPhaseBlimp2PatternsNormal.Shoot1 },
        { "FlyingBlimpPhaseBlimp3PatternNormal", FlyingBlimpPhaseBlimp3PatternsNormal.Shoot2 },
        { "FlowerPhaseGeneric1PatternNormal", FlowerPhaseGeneric1PatternsNormal.HeadLunge },
        { "FlowerPhaseGeneric2PatternNormal", FlowerPhaseGeneric2PatternsNormal.HeadLunge },
        { "FlowerPhaseGenericHeadLungePatternNormal", FlowerPhaseGenericHeadLungePatternsNormal.Bottom1 },
        { "BaronessMiniboss1Normal", BaronessMinibossesNormal.Waffle },
        { "BaronessMiniboss2Normal", BaronessMinibossesNormal.CandyCorn },
        { "BaronessMiniboss3Normal", BaronessMinibossesNormal.Cupcake },
        { "FlyingBirdPhaseOneDirection", FlyingBirdPhaseOneDirections.Down },
        { "FlyingBirdPhaseOnePatternNormal", FlyingBirdPhaseOnePatternsNormal.Eggs },
        { "FlyingBirdPhaseTwoPatternNormal", FlyingBirdPhaseTwoPatternsNormal.Eggs01 },
        { "FlyingBirdPhaseFinalPattern", FlyingBirdPhaseFinalPatterns.Garbage },
        { "FlyingBirdPhaseFinalDirection", FlyingBirdPhaseFinalDirections.Right },
        { "FlyingGeniePhaseOneTreasurePattern", FlyingGeniePhaseOneTreasurePatterns.Gems },
        { "FlyingGeniePhaseOneGemsTypeNormalHard", FlyingGeniePhaseOneGemsTypesNormalHard.P_01 },
        { "FlyingGeniePhaseTwoObeliskPattern", FlyingGeniePhaseTwoObeliskPatterns.AI_2_5 },
        { "ClownDashDelayNormal", ClownDashDelaysNormal.J_2 },
        { "DragonPhaseThreePatternNormal", DragonPhaseThreePatternsNormal.Peashot },
        { "DragonPhaseOneLaserPatternNormal", DragonPhaseOneLaserPatternsNormal.Three },
        { "DragonPhaseThreeLaserPatternNormal", DragonPhaseThreeLaserPatternsNormal.Two },
        { "BeePhaseTwoPatternNormal", BeePhaseTwoPatternsNormal.Chain },
        { "BeePerfectPlatforms", true },
        { "RobotPhaseFinalGemColor", RobotPhaseFinalGemColors.Red },
        { "SallyStageplayPatternNormalHard", SallyStageplayPatternsNormalHard.Kiss1 },
        { "SallyStageplayJumpTypeNormalHard", SallyStageplayJumpTypesNormalHard.DiveKick1 },
        { "SallyStageplayTeleportOffsetNormalHard", SallyStageplayTeleportOffsetsNormalHard.A_0 },
        { "MousePhaseOnePatternNormal", MousePhaseOnePatternsNormal.Catapult1 },
        { "MouseCanMoveMaxXPosition", 340f },
        { "MouseBrokenCanMoveMaxXPosition", 28f },
        { "PiratePhaseOneGunPatternNormal", PiratePhaseOneGunPatternsNormal.Three_One },
        { "PiratePhaseTwoGunPatternNormal", PiratePhaseTwoGunPatternsNormal.Two_One_One },
        { "PiratePhaseThreeGunPatternNormal", PiratePhaseThreeGunPatternsNormal.One_Two },
        { "PiratePhaseTwoPatternNormalHard", PiratePhaseTwoPatternsNormalHard.PeashotShark },
        { "PiratePhaseThreePatternNormalHard", PiratePhaseThreePatternsNormalHard.PeashotShark },
        { "FlyingMermaidPhaseOnePatternNormalHard", FlyingMermaidPhaseOnePatternsNormalHard.Fish },
        { "FlyingMermaidPhaseOneFishPattern", FlyingMermaidPhaseOneFishPatterns.Red },
        { "FlyingMermaidPhaseOneSummonPattern", FlyingMermaidPhaseOneSummonPatterns.Pufferfish },
        { "TrainPumpkinStartingDirection", TrainPumpkinStartingDirections.Left },
        { "TrainStartingGhoul", TrainStartingGhouls.Right },
        { "DicePalaceHeartPosition1", DicePalaceHeartPositions1.Three },
        { "DicePalaceHeartPosition2", DicePalaceHeartPositions2.Five },
        { "DicePalaceHeartPosition3", DicePalaceHeartPositions3.Seven },
        { "DicePalaceCigarSpitAttackCountNormal", DicePalaceCigarSpitAttackCountsNormal.One1 },
        { "DicePalaceRabbitPattern", DicePalaceRabbitPatterns.Wand05 },
        { "DicePalaceRabbitParryDirection", DicePalaceRabbitParryDirections.Top },
        { "DicePalaceRoulettePattern", DicePalaceRoulettePatterns.Marble },
        { "DevilPhaseOnePattern", DevilPhaseOnePatterns.Pitchfork2 },
        { "DevilPhaseOneHeadType", DevilPhaseOneHeadTypes.Dragon },
        { "DevilPhaseOnePitchforkType", DevilPhaseOnePitchforkTypes.Bouncer },
        { "DevilPhaseOneBouncerParryIndex", DevilPhaseOneBouncerParryIndexes.Three },
        { "DevilPhaseOneBouncerAngleNormal", DevilPhaseOneBouncerAnglesNormal.F_70 }
    };
    private readonly List<FieldInfo> trackedEntries = new();
    private bool isActive;
    private bool initialized;
    private RngOverride lastAppliedMode = RngOverride.Custom;

    private void Awake() {
        InitializeTrackedEntries();
    }

    private void OnDestroy() {
        RestoreOriginalValues();
    }

    private void Update() {
        if (!initialized) {
            InitializeTrackedEntries();
            return;
        }

        RngOverride mode = Settings.RngOverride.Value;
        if (mode == lastAppliedMode && isActive) {
            return;
        }

        ApplyMode(mode);
        lastAppliedMode = mode;
    }

    private void ApplyMode(RngOverride mode) {
        if (mode == RngOverride.Custom) {
            if (isActive) {
                RestoreOriginalValues();
                isActive = false;
            }

            return;
        }

        if (!isActive) {
            CaptureOriginalValues();
        }

        if (mode == RngOverride.Vanilla) {
            ApplyDefaults();
        } else if (mode == RngOverride.PerfectRng11) {
            ApplyOverrides();
        }

        isActive = true;
    }

    private void InitializeTrackedEntries() {
        if (initialized) {
            return;
        }

        trackedEntries.Clear();
        foreach (FieldInfo field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static)) {
            if (!IsTrackedConfigEntry(field)) {
                continue;
            }

            if (ShouldTrack(field.Name)) {
                trackedEntries.Add(field);
            }
        }

        initialized = true;
    }

    private static bool IsTrackedConfigEntry(FieldInfo field) {
        Type fieldType = field.FieldType;
        return fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ConfigEntry<>);
    }

    private static bool ShouldTrack(string fieldName) {
        if (fieldName == nameof(Settings.RngOverride)) {
            return false;
        }

        return fieldName.StartsWith("Frogs", StringComparison.Ordinal)
            || fieldName.StartsWith("Slime", StringComparison.Ordinal)
            || fieldName.StartsWith("FlyingBlimp", StringComparison.Ordinal)
            || fieldName.StartsWith("Flower", StringComparison.Ordinal)
            || fieldName.StartsWith("Baroness", StringComparison.Ordinal)
            || fieldName.StartsWith("FlyingBird", StringComparison.Ordinal)
            || fieldName.StartsWith("FlyingGenie", StringComparison.Ordinal)
            || fieldName.StartsWith("Clown", StringComparison.Ordinal)
            || fieldName.StartsWith("Dragon", StringComparison.Ordinal)
            || fieldName.StartsWith("Bee", StringComparison.Ordinal)
            || fieldName.StartsWith("Robot", StringComparison.Ordinal)
            || fieldName.StartsWith("SallyStageplay", StringComparison.Ordinal)
            || fieldName.StartsWith("Mouse", StringComparison.Ordinal)
            || fieldName.StartsWith("Pirate", StringComparison.Ordinal)
            || fieldName.StartsWith("FlyingMermaid", StringComparison.Ordinal)
            || fieldName.StartsWith("Train", StringComparison.Ordinal)
            || fieldName.StartsWith("DicePalace", StringComparison.Ordinal)
            || fieldName.StartsWith("Devil", StringComparison.Ordinal);
    }

    private void CaptureOriginalValues() {
        if (originalValues.Count > 0) {
            return;
        }

        foreach (FieldInfo field in trackedEntries) {
            originalValues[field.Name] = GetEntryValue(field);
        }
    }

    private void ApplyDefaults() {
        foreach (FieldInfo field in trackedEntries) {
            object defaultValue = GetDefaultValue(field);
            SetEntryValue(field, defaultValue ?? GetEntryValue(field));
        }
    }

    private void ApplyOverrides() {
        ApplyDefaults();

        foreach (FieldInfo field in trackedEntries) {
            object explicitOverride;
            if (explicitOverrides.TryGetValue(field.Name, out explicitOverride)) {
                SetEntryValue(field, explicitOverride);
            }
        }
    }

    private void RestoreOriginalValues() {
        foreach (FieldInfo field in trackedEntries) {
            if (originalValues.TryGetValue(field.Name, out object storedValue)) {
                SetEntryValue(field, storedValue);
            }
        }
    }

    private static object GetEntryValue(FieldInfo field) {
        object configEntry = field.GetValue(null);
        PropertyInfo valueProperty = configEntry.GetType().GetProperty("Value");
        return valueProperty?.GetValue(configEntry, null);
    }

    private static object GetDefaultValue(FieldInfo field) {
        object configEntry = field.GetValue(null);
        PropertyInfo defaultValueProperty = configEntry.GetType().GetProperty("DefaultValue");
        return defaultValueProperty?.GetValue(configEntry, null);
    }

    private static void SetEntryValue(FieldInfo field, object value) {
        object configEntry = field.GetValue(null);
        PropertyInfo valueProperty = configEntry.GetType().GetProperty("Value");
        valueProperty?.SetValue(configEntry, value, null);
    }
}
