using System;
using System.Collections.Generic;
using System.Reflection;

namespace BepInEx.CupheadDebugMod.Rng;

/// <summary>
/// What kind of randomness a spot controls. The shape decides which editor the menu shows
/// and which overrides the game allows while staying 100% faithful to vanilla.
/// </summary>
internal enum RngShape {
    /// <summary>A fixed cyclic list whose random start position is re-rolled per fight (or per PatternString init).</summary>
    StartPool,

    /// <summary>An independent random pick that happens every time an attack/event fires.</summary>
    Choice,

    /// <summary>A numeric value drawn from a range.</summary>
    Float
}

/// <summary>The type badge shown on a spot card; derived from the shape unless a spot overrides it.</summary>
internal enum RngBadgeKind {
    /// <summary>The random start of a fixed cycle.</summary>
    Start,

    /// <summary>Re-rolled independently on every attack.</summary>
    PerAttack,

    /// <summary>Decided once per fight.</summary>
    Once,

    /// <summary>One ordered list is picked per fight and then walked; a companion pick
    /// chooses which attack comes first inside it.</summary>
    Order,

    /// <summary>A start position into a fixed cycle, re-rolled on every round of the
    /// attack - the round then plays the cycle from there.</summary>
    CyclePerRound,

    /// <summary>A whole sequence (and where it starts) re-rolled on every round of the
    /// attack.</summary>
    Sequence
}

internal sealed class RngOption {
    public readonly string Label;
    public readonly int Value;

    public RngOption(string label, int value) {
        Label = label;
        Value = value;
    }
}

internal struct RngFloatRange {
    public float Min;
    public float Max;
    public float Step;
    public float Default;

    public RngFloatRange(float min, float max, float step, float @default) {
        Min = min;
        Max = max;
        Step = step;
        Default = @default;
    }
}

/// <summary>
/// One controllable RNG point. Options are enumerated from the game's own data
/// (LevelProperties.*) so labels and counts can never drift from what the game uses.
/// </summary>
internal sealed class RngSpot {
    public readonly string Id;
    public readonly string Group;
    public readonly string Label;
    public readonly RngShape Shape;

    /// <summary>True when the game's data differs per difficulty, so config is stored per difficulty.</summary>
    public readonly bool PerDifficulty;

    /// <summary>True when the consumer advances the index (+1 % length) before using it.</summary>
    public readonly bool PreIncrement;

    public readonly Func<Level.Mode, RngOption[]> Options;
    public readonly Func<Level.Mode, RngFloatRange> FloatRange;
    public readonly string Hint;

    /// <summary>Optional extra availability rule (e.g. hide on difficulties where the attack never runs).</summary>
    public Func<Level.Mode, bool> Available;

    /// <summary>
    /// True when vanilla re-rolls this spot on every occurrence of its attack (parryable
    /// flame, bomb side, hop count...). Pinning such a spot to one value repeats that
    /// outcome, which vanilla itself can produce per attack but not as a constant
    /// sequence, so pinned per-event spots are flagged in the fidelity footer.
    /// </summary>
    public bool PerEvent {
        get; set;
    }

    /// <summary>Optional badge override; derived from the shape when unset.</summary>
    public RngBadgeKind? Badge {
        get; set;
    }

    /// <summary>When set, this spot is edited together with the referenced secondary spot
    /// in one compound card instead of two standalone cards.</summary>
    public string CompoundWith;

    /// <summary>True when this spot is edited inside its primary's compound card and gets
    /// no card of its own.</summary>
    public bool CompoundSecondary;

    /// <summary>For compound primaries: builds the referenced secondary's start options
    /// for a chosen primary option (the secondary's own options depend on that choice).</summary>
    public Func<Level.Mode, int, RngOption[]> CompoundStartOptions;

    /// <summary>Compound card group labels; defaults keep the pitchfork's wording.</summary>
    public string CompoundPrimaryLabel;
    public string CompoundSecondaryLabel;

    /// <summary>True when this per-event spot's plan steps render as collapsible radio
    /// lists over an ordered cycle (the jump-pattern style) instead of cyclers.</summary>
    public bool RadioPlan;

    public RngSpot(
        string id,
        string group,
        string label,
        RngShape shape,
        bool perDifficulty,
        bool preIncrement,
        Func<Level.Mode, RngOption[]> options,
        string hint
    ) : this(id, group, label, shape, perDifficulty, preIncrement, options, null, hint) {
    }

    public RngSpot(
        string id,
        string group,
        string label,
        RngShape shape,
        bool perDifficulty,
        Func<Level.Mode, RngFloatRange> floatRange,
        string hint
    ) : this(id, group, label, shape, perDifficulty, false, null, floatRange, hint) {
    }

    private RngSpot(
        string id,
        string group,
        string label,
        RngShape shape,
        bool perDifficulty,
        bool preIncrement,
        Func<Level.Mode, RngOption[]> options,
        Func<Level.Mode, RngFloatRange> floatRange,
        string hint
    ) {
        Id = id;
        Group = group;
        Label = label;
        Shape = shape;
        PerDifficulty = perDifficulty;
        PreIncrement = preIncrement;
        Options = options;
        FloatRange = floatRange;
        Hint = hint;
    }

    public string Key(Level.Mode mode) {
        return PerDifficulty ? Id + "@" + ModeKey(mode) : Id;
    }

    public bool IsAvailable(Level.Mode mode) {
        return Available == null || Available(mode);
    }

    /// <summary>The fight (first id segment) a spot belongs to ("Devil/Phase1/PatternStart" becomes "Devil").</summary>
    public string StageId {
        get { return RngSpots.StageOf(Id); }
    }

    public static string ModeKey(Level.Mode mode) {
        switch (mode) {
            case Level.Mode.Easy:
                return "easy";
            case Level.Mode.Hard:
                return "hard";
            default:
                return "normal";
        }
    }
}

internal static class RngSpots {
    // The Devil — Phase 1 (Sitting Devil)
    public const string DevilPhase1PatternStart = "Devil/Phase1/PatternStart";
    public const string DevilPhase1HeadFirst = "Devil/Phase1/HeadFirst";
    public const string DevilPhase1ClapDelay = "Devil/Phase1/ClapDelay";
    public const string DevilPhase1DragonLeft = "Devil/Phase1/DragonLeft";
    public const string DevilPhase1SpiderOffsetStart = "Devil/Phase1/SpiderOffsetStart";
    public const string DevilPhase1SpiderDelay = "Devil/Phase1/SpiderDelay";
    public const string DevilPhase1SpiderHopCount = "Devil/Phase1/SpiderHopCount";
    public const string DevilPhase1PitchforkOrder = "Devil/Phase1/PitchforkOrder";
    public const string DevilPhase1PitchforkType = "Devil/Phase1/PitchforkType";
    public const string DevilPhase1BouncerParryIndex = "Devil/Phase1/BouncerParryIndex";
    public const string DevilPhase1RingParryIndex = "Devil/Phase1/RingParryIndex";
    public const string DevilPhase1BouncerDelay = "Devil/Phase1/BouncerDelay";
    public const string DevilPhase1RingDelay = "Devil/Phase1/RingDelay";
    public const string DevilPhase1BouncerAngleStart = "Devil/Phase1/BouncerAngleStart";
    public const string DevilPhase1PinwheelAngleStart = "Devil/Phase1/PinwheelAngleStart";
    public const string DevilPhase1RingAngleStart = "Devil/Phase1/RingAngleStart";

    // The Devil — Phase 2 (Giant Head)
    public const string DevilPhase2PatternStart = "Devil/Phase2/PatternStart";
    public const string DevilPhase2BombLeft = "Devil/Phase2/BombLeft";
    public const string DevilPhase2PlatformRiseStart = "Devil/Phase2/PlatformRiseStart";
    public const string DevilPhase2PlatformRiseDelay = "Devil/Phase2/PlatformRiseDelay";

    // The Devil — Phase 3 (Hands)
    public const string DevilPhase3SkullType = "Devil/Phase3/SkullType";

    // Ribby & Croaks — Phase 1 (frogs)
    public const string FrogsPhase1PatternStart = "Frogs/Phase1/PatternStart";
    public const string FrogsPhase1FirefliesPattern = "Frogs/Phase1/FirefliesPattern";

    // Ribby & Croaks — Final (slot machine; the fight never reaches this phase on Simple)
    public const string FrogsFinalSlotMode = "Frogs/Final/SlotMode";
    public const string FrogsFinalBisonLaserSide = "Frogs/Final/BisonLaserSide";

    // Goopy Le Grande — jump attack sequence
    public const string SlimeJumpCount = "Slime/Jump/Count";
    public const string SlimeJumpPatternStart = "Slime/Jump/PatternStart";

    // The Root Pack — Carrot phase
    public const string VeggiesCarrotCount = "Veggies/Carrot/Count";
    public const string VeggiesCarrotIdleDelay = "Veggies/Carrot/IdleDelay";

    // The Root Pack — Onion phase (the fight skips the onion on Simple)
    public const string VeggiesOnionTearPattern = "Veggies/Onion/TearPattern";
    public const string VeggiesOnionTearDelayLeft = "Veggies/Onion/TearDelayLeft";
    public const string VeggiesOnionTearDelayRight = "Veggies/Onion/TearDelayRight";
    public const string VeggiesOnionPinkCadenceLeft = "Veggies/Onion/PinkCadenceLeft";
    public const string VeggiesOnionPinkCadenceRight = "Veggies/Onion/PinkCadenceRight";

    // Hilda Berg (each difficulty runs two Generic blimp phases plus, on Regular, a
    // coin-flip Sagittarius-or-Gemini constellation phase)
    public const string FlyingBlimpGeneric1PatternStart = "FlyingBlimp/Generic1/PatternStart";
    public const string FlyingBlimpGeneric2PatternStart = "FlyingBlimp/Generic2/PatternStart";
    public const string FlyingBlimpConstellation = "FlyingBlimp/Constellation/Type";

    // Hilda Berg — final moon phase (Regular and Expert only)
    public const string FlyingBlimpMoonStarPositions = "FlyingBlimp/Moon/StarPositions";
    public const string FlyingBlimpMoonStarPositionsStart = "FlyingBlimp/Moon/StarPositionsStart";
    public const string FlyingBlimpMoonStarTypes = "FlyingBlimp/Moon/StarTypes";
    public const string FlyingBlimpMoonStarTypesStart = "FlyingBlimp/Moon/StarTypesStart";
    public const string FlyingBlimpMoonStarSpeed = "FlyingBlimp/Moon/StarSpeed";
    public const string FlyingBlimpMoonUfoPattern = "FlyingBlimp/Moon/UfoPattern";
    public const string FlyingBlimpMoonUfoStart = "FlyingBlimp/Moon/UfoStart";

    // Forest Follies (run 'n gun; platforming stages always run on Normal)
    public const string ForestFolliesAcornSide = "ForestFollies/Acorn/SideStart";
    public const string ForestFolliesAcornHeight = "ForestFollies/Acorn/HeightStart";

    public static readonly RngSpot[] Devil = {
        new RngSpot(
            DevilPhase1PatternStart, "Phase 1", "Overall Pattern", RngShape.StartPool, true, true,
            mode => DevilStatePatterns(mode, LevelProperties.Devil.States.Main),
            "Which attack the fight's fixed cycle begins with. The game rolls a start position, then repeats the cycle in order."
        ),
        new RngSpot(
            DevilPhase1HeadFirst, "Phase 1", "First Head Attack", RngShape.StartPool, false, false,
            mode => Fixed("Dragon", "Spider"),
            "Dragon and Spider heads strictly alternate; this picks which one comes first."
        ),
        new RngSpot(
            DevilPhase1ClapDelay, "Phase 1", "Clap Delay", RngShape.Float, false,
            mode => new RngFloatRange(0.1f, 0.5f, 0.05f, 0.3f),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1DragonLeft, "Phase 1", "Dragon Side", RngShape.Choice, false, false,
            mode => Fixed("Left", "Right"),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1SpiderOffsetStart, "Phase 1", "Spider Offset Start", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.spider.positionOffset),
            "The spider head walks a fixed offset loop; this picks where the loop begins."
        ),
        new RngSpot(
            DevilPhase1SpiderDelay, "Phase 1", "Spider Delay", RngShape.Float, false,
            mode => new RngFloatRange(0.3f, 0.7f, 0.05f, 0.5f),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1SpiderHopCount, "Phase 1", "Spider Hop Count", RngShape.Choice, false, false,
            mode => Numbered(3, 3),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1PitchforkOrder, "Phase 1", "Pitchfork Attacks", RngShape.Choice, true, false,
            mode => DevilPitchforkOrderOptions(mode),
            "The fight picks one attack order once, then walks it. Pin an order to fix the attack sequence."
        ) { CompoundWith = DevilPhase1PitchforkType, Badge = RngBadgeKind.Order,
            CompoundStartOptions = (mode, orderIndex) => DevilPitchforkTypeOptions(mode, orderIndex),
            CompoundPrimaryLabel = "Attack order", CompoundSecondaryLabel = "First attack" },
        new RngSpot(
            DevilPhase1PitchforkType, "Phase 1", "Pitchfork Attack Type", RngShape.Choice, false, false,
            mode => Fixed("Bouncer", "Pinwheel", "Ring"),
            "Which pitchfork attack comes first inside the pinned attack order."
        ) { CompoundSecondary = true },
        new RngSpot(
            DevilPhase1BouncerParryIndex, "Phase 1", "Bouncer Parryable #", RngShape.Choice, false, false,
            mode => Numbered(1, 4),
            "Which of the four bouncer flames is pink."
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1RingParryIndex, "Phase 1", "Ring Parryable #", RngShape.Choice, false, false,
            mode => Numbered(1, 6),
            "Which of the six ring flames is pink."
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1BouncerDelay, "Phase 1", "Bouncer Delay", RngShape.Float, false,
            mode => new RngFloatRange(1.0f, 1.5f, 0.05f, 1.25f),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1RingDelay, "Phase 1", "Ring Delay", RngShape.Float, false,
            mode => new RngFloatRange(1.0f, 1.5f, 0.05f, 1.25f),
            null
        ) { PerEvent = true },
        new RngSpot(
            DevilPhase1BouncerAngleStart, "Phase 1", "Bouncer Angle", RngShape.StartPool, true, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkFourFlameBouncer.angleOffset),
            "The bouncer wave cycles through fixed angles; this picks where the cycle begins."
        ),
        new RngSpot(
            DevilPhase1PinwheelAngleStart, "Phase 1", "Pinwheel Angle", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkFiveFlameSpinner.angleOffset),
            null
        ),
        new RngSpot(
            DevilPhase1RingAngleStart, "Phase 1", "Ring Angle", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkSixFlameRing.angleOffset),
            null
        ),

        new RngSpot(
            DevilPhase2PatternStart, "Phase 2", "Overall Pattern", RngShape.StartPool, true, true,
            mode => DevilStatePatterns(mode, LevelProperties.Devil.States.GiantHead),
            "Which eye attack the giant head starts its fixed cycle with."
        ),
        new RngSpot(
            DevilPhase2BombLeft, "Phase 2", "Bomb Side", RngShape.Choice, false, false,
            mode => Fixed("Left", "Right"),
            null
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            DevilPhase2PlatformRiseStart, "Phase 2", "Platform Rise Order Start", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.GiantHead, state => state.giantHeadPlatforms.riseString),
            "Platforms rise in a fixed repeating order; this picks where the order begins."
        ),
        new RngSpot(
            DevilPhase2PlatformRiseDelay, "Phase 2", "Platform Rise Delay", RngShape.Float, true,
            mode => {
                if (mode == Level.Mode.Hard) {
                    return new RngFloatRange(1.5f, 2.8f, 0.05f, 2.15f);
                }
                return new RngFloatRange(2.0f, 3.5f, 0.05f, 2.75f);
            },
            null
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },

        new RngSpot(
            DevilPhase3SkullType, "Phase 3", "Skull Pink Sequence Start", RngShape.StartPool, false, false,
            DevilPinkStringOptions,
            "Each hand starts somewhere in the fixed pink sequence and walks it in order; this picks where each hand begins."
        ) { Available = mode => mode != Level.Mode.Easy }
    };

    public static readonly RngSpot[] Frogs = {
        new RngSpot(
            FrogsPhase1PatternStart, "Phase 1", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FrogsStatePatterns(mode),
            "Which attack the fight's fixed cycle begins with. The game rolls a start position, then repeats the cycle in order. Simple and Regular list the two attacks in opposite order."
        ),
        new RngSpot(
            FrogsPhase1FirefliesPattern, "Phase 1", "Fireflies Pattern", RngShape.Choice, true, false,
            mode => FrogsFirefliesOptions(mode),
            "Every fireflies volley rolls one of these fly sequences independently. Naming: S = the frog spits a batch of fireflies, D = a wait (the legacy setting's names)."
        ) { PerEvent = true },
        new RngSpot(
            FrogsFinalSlotMode, "Final", "Slot Attack", RngShape.Choice, true, false,
            mode => Fixed("Snakes", "Tigers", "Bulls"),
            "The slot machine rolls one of the three attacks every time it spins. The charm-triggered demon attack is not part of this roll and stays vanilla."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            FrogsFinalBisonLaserSide, "Final", "Bulls Laser Side", RngShape.Choice, true, false,
            mode => Fixed("Up", "Down"),
            "Every bison laser rolls its side independently, and the plan restarts each time a Bulls attack begins. The game still forces a flip after three identical lasers."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy }
    };

    public static readonly RngSpot[] Slime = {
        new RngSpot(
            SlimeJumpCount, "Phase 1", "Jumps Per Volley", RngShape.Choice, true, false,
            mode => Numbered(5, 3),
            "How many jumps the attack lasts before a punch. The game rolls this per volley."
        ) { PerEvent = true },
        new RngSpot(
            SlimeJumpPatternStart, "Phase 1", "Jump Pattern Start", RngShape.Choice, true, false,
            mode => SlimeJumpOptions(mode),
            "Each round of jumps rolls a fresh start position inside the fixed cycle, then plays the cycle from there. H = high, L = low, R = a random-height jump."
        ) { PerEvent = true, Badge = RngBadgeKind.CyclePerRound, RadioPlan = true }
    };

    public static readonly RngSpot[] Veggies = {
        new RngSpot(
            VeggiesCarrotCount, "Carrot", "Carrots Per Round", RngShape.Choice, true, false,
            mode => VeggiesCarrotCountOptions(mode),
            "How many homing carrots each mind-meld barrage sends out. The game rolls this per round."
        ) { PerEvent = true },
        new RngSpot(
            VeggiesCarrotIdleDelay, "Carrot", "Carrot Hesitation", RngShape.Float, true,
            mode => VeggiesCarrotIdleRange(mode),
            "The wait between a carrot barrage and the ring attack that follows it. Rolled per round."
        ) { PerEvent = true },
        new RngSpot(
            VeggiesOnionTearPattern, "Onion", "Tear Pattern", RngShape.Choice, true, false,
            mode => VeggiesTearPatternOptions(mode),
            "One tear volley pattern is rolled per crying round and shared by both sides; it then cycles for the whole round."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            VeggiesOnionTearDelayLeft, "Onion", "Tear Delay (Left)", RngShape.Float, true,
            mode => new RngFloatRange(0.3f, 0.7f, 0.05f, 0.5f),
            "The wait before each tear in the left stream. The game re-rolls it after every tear."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            VeggiesOnionTearDelayRight, "Onion", "Tear Delay (Right)", RngShape.Float, true,
            mode => new RngFloatRange(0.3f, 0.7f, 0.05f, 0.5f),
            "The wait before each tear in the right stream. The game re-rolls it after every tear."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            VeggiesOnionPinkCadenceLeft, "Onion", "Pink Tear Cadence (Left)", RngShape.Choice, true, false,
            mode => VeggiesPinkOptions(mode),
            "How many tears pass between parryable pink tears in the left stream. The game re-rolls it after every pink tear."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            VeggiesOnionPinkCadenceRight, "Onion", "Pink Tear Cadence (Right)", RngShape.Choice, true, false,
            mode => VeggiesPinkOptions(mode),
            "How many tears pass between parryable pink tears in the right stream. The game re-rolls it after every pink tear."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy }
    };

    public static readonly RngSpot[] FlyingBlimp = {
        new RngSpot(
            FlyingBlimpGeneric1PatternStart, "Second Blimp", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FlyingBlimpGenericOptions(mode, 0),
            "Which attack this blimp phase's fixed cycle begins with; it then repeats the cycle in order."
        ),
        new RngSpot(
            FlyingBlimpGeneric2PatternStart, "Third Blimp", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FlyingBlimpGenericOptions(mode, 1),
            null
        ),
        new RngSpot(
            FlyingBlimpConstellation, "Constellation", "Constellation", RngShape.Choice, false, false,
            mode => Fixed("Sagittarius", "Gemini"),
            null
        ) { Available = mode => mode == Level.Mode.Normal },
        new RngSpot(
            FlyingBlimpMoonStarPositions, "Moon", "Star Positions", RngShape.Choice, true, false,
            mode => FlyingBlimpStarPatternOptions(mode, true),
            "One position sequence is picked when the moon phase starts and then cycles for the whole fight; the start below chooses where it begins."
        ) { CompoundWith = FlyingBlimpMoonStarPositionsStart, Badge = RngBadgeKind.Order, Available = mode => mode != Level.Mode.Easy,
            CompoundStartOptions = (mode, stringIndex) => FlyingBlimpStarStartOptions(mode, true, stringIndex),
            CompoundPrimaryLabel = "Pattern", CompoundSecondaryLabel = "Start at" },
        new RngSpot(
            FlyingBlimpMoonStarPositionsStart, "Moon", "Star Position Start", RngShape.Choice, true, false,
            mode => new RngOption[0],
            null
        ) { CompoundSecondary = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            FlyingBlimpMoonStarTypes, "Moon", "Star Types", RngShape.Choice, true, false,
            mode => FlyingBlimpStarPatternOptions(mode, false),
            "One star type sequence (A/B/C stars, P = pink) is picked when the moon phase starts and then cycles for the whole fight; the start below chooses where it begins, which places the pink stars."
        ) { CompoundWith = FlyingBlimpMoonStarTypesStart, Badge = RngBadgeKind.Order, Available = mode => mode != Level.Mode.Easy,
            CompoundStartOptions = (mode, stringIndex) => FlyingBlimpStarStartOptions(mode, false, stringIndex),
            CompoundPrimaryLabel = "Pattern", CompoundSecondaryLabel = "Start at" },
        new RngSpot(
            FlyingBlimpMoonStarTypesStart, "Moon", "Star Type Start", RngShape.Choice, true, false,
            mode => new RngOption[0],
            null
        ) { CompoundSecondary = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            FlyingBlimpMoonStarSpeed, "Moon", "Star Speed", RngShape.Float, true,
            mode => FlyingBlimpStarSpeedRange(mode),
            "Each star's horizontal speed, rolled per star."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            FlyingBlimpMoonUfoPattern, "Moon", "UFO Pattern", RngShape.Choice, true, false,
            mode => FlyingBlimpUfoPatternOptions(mode),
            "One UFO sequence is rolled per attack round and then walked in order; the start below chooses where it begins."
        ) { PerEvent = true, CompoundWith = FlyingBlimpMoonUfoStart, Badge = RngBadgeKind.Sequence, Available = mode => mode != Level.Mode.Easy,
            CompoundStartOptions = (mode, stringIndex) => FlyingBlimpUfoStartOptions(mode, stringIndex),
            CompoundPrimaryLabel = "Pattern", CompoundSecondaryLabel = "Starting UFO" },
        new RngSpot(
            FlyingBlimpMoonUfoStart, "Moon", "UFO Start", RngShape.Choice, true, false,
            mode => new RngOption[0],
            null
        ) { PerEvent = true, CompoundSecondary = true, Available = mode => mode != Level.Mode.Easy }
    };

    public static readonly RngSpot[] ForestFollies = {
        new RngSpot(
            ForestFolliesAcornSide, "Acorn Spawner", "Acorn Side Start", RngShape.StartPool, false, true,
            mode => ForestAcornSideOptions(),
            "The spawner walks a fixed left/right cycle; this picks which side the first acorn flies in from."
        ),
        new RngSpot(
            ForestFolliesAcornHeight, "Acorn Spawner", "Acorn Height Start", RngShape.StartPool, false, true,
            mode => ForestAcornHeightOptions(),
            "The spawner walks a fixed height cycle (offset below the top of the screen); this picks the first acorn's height."
        )
    };

    /// <summary>Every catalog spot, in page order.</summary>
    public static readonly RngSpot[] All = BuildAll();

    private static RngSpot[] BuildAll() {
        List<RngSpot> all = new List<RngSpot>();
        all.AddRange(Devil);
        all.AddRange(Frogs);
        all.AddRange(Slime);
        all.AddRange(Veggies);
        all.AddRange(FlyingBlimp);
        all.AddRange(ForestFollies);
        return all.ToArray();
    }

    private static readonly Dictionary<string, RngSpot> SpotIndex = BuildIndex();

    public static RngSpot Find(string id) {
        RngSpot spot;
        return SpotIndex.TryGetValue(id, out spot) ? spot : null;
    }

    /// <summary>The stage (fight) a spot belongs to: the id's first segment ("Devil/Phase1/PatternStart" becomes "Devil").</summary>
    public static string StageOf(string spotId) {
        int first = spotId.IndexOf('/');
        return first < 0 ? spotId : spotId.Substring(0, first);
    }

    /// <summary>Display names for fight-level stages; new bosses register theirs here.</summary>
    private static readonly Dictionary<string, string> StageLabels = new Dictionary<string, string>(StringComparer.Ordinal) {
        { "Devil", "The Devil" },
        { "Frogs", "Ribby & Croaks" },
        { "Slime", "Goopy Le Grande" },
        { "Veggies", "The Root Pack" },
        { "FlyingBlimp", "Hilda Berg" },
        { "ForestFollies", "Forest Follies" }
    };

    public static string StageLabel(string stageId) {
        string label;
        return StageLabels.TryGetValue(stageId, out label) ? label : stageId;
    }

    /// <summary>Looks a spot up by a stored config/form key, which may carry a "@difficulty" suffix.</summary>
    public static RngSpot FindByStoredKey(string key) {
        int at = key.LastIndexOf('@');
        return Find(at >= 0 ? key.Substring(0, at) : key);
    }

    private static Dictionary<string, RngSpot> BuildIndex() {
        Dictionary<string, RngSpot> index = new Dictionary<string, RngSpot>(StringComparer.Ordinal);
        foreach (RngSpot spot in All) {
            index[spot.Id] = spot;
        }
        return index;
    }

    // ------------------------------------------------------------------
    // Option enumeration. Everything below reads the game's own data via
    // LevelProperties.Devil.GetMode(), so labels and counts always match
    // the version being played.
    // ------------------------------------------------------------------

    private static readonly FieldInfo DevilStatesField = FindField(typeof(LevelProperties.Devil), "states");
    private static readonly Dictionary<Level.Mode, LevelProperties.Devil.State[]> StatesCache = new Dictionary<Level.Mode, LevelProperties.Devil.State[]>();

    private static LevelProperties.Devil.State[] StatesFor(Level.Mode mode) {
        LevelProperties.Devil.State[] states;
        if (StatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Devil properties = LevelProperties.Devil.GetMode(mode);
            states = (LevelProperties.Devil.State[])DevilStatesField.GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Devil.State[0];
        }

        StatesCache[mode] = states;
        return states;
    }

    private static FieldInfo FindField(Type type, string name) {
        while (type != null) {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) {
                return field;
            }
            type = type.BaseType;
        }
        return null;
    }

    private static RngOption[] DevilStatePatterns(Level.Mode mode, LevelProperties.Devil.States stateName) {
        try {
            foreach (LevelProperties.Devil.State state in StatesFor(mode)) {
                if (state.stateName != stateName) {
                    continue;
                }
                string[] labels = new string[state.patterns.Length];
                for (int i = 0; i < state.patterns.Length; i++) {
                    labels[i] = PrettyPattern(state.patterns[i]);
                }
                return NamedOptions(labels);
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static RngOption[] DevilStringOptions(Level.Mode mode, LevelProperties.Devil.States stateName, Func<LevelProperties.Devil.State, string> pick) {
        try {
            foreach (LevelProperties.Devil.State state in StatesFor(mode)) {
                if (state.stateName != stateName) {
                    continue;
                }
                string value = pick(state);
                if (string.IsNullOrEmpty(value)) {
                    return new RngOption[0];
                }
                string[] parts = value.Split(new[] { ',' });
                for (int i = 0; i < parts.Length; i++) {
                    parts[i] = parts[i].Trim();
                }
                return NamedOptions(parts);
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static RngOption[] DevilPinkStringOptions(Level.Mode mode) {
        try {
            foreach (LevelProperties.Devil.State state in StatesFor(mode)) {
                if (state.stateName != LevelProperties.Devil.States.Hands) {
                    continue;
                }
                string pink = state.hands.pinkString;
                if (string.IsNullOrEmpty(pink)) {
                    return new RngOption[0];
                }
                string[] labels = new string[pink.Length];
                for (int i = 0; i < pink.Length; i++) {
                    labels[i] = pink[i] == 'P' ? "Parry" : "Regular";
                }
                return NamedOptions(labels);
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    // ------------------------------------------------------------------
    // Pitchfork helpers. The game picks one of the order strings once per
    // fight, then a start position inside it; the compound card edits both
    // rolls together.
    // ------------------------------------------------------------------

    /// <summary>One option per pitchfork order list, named "Set N" (edit the array below
    /// to relabel the sets).</summary>
    private static RngOption[] DevilPitchforkOrderOptions(Level.Mode mode) {
        try {
            foreach (LevelProperties.Devil.State state in StatesFor(mode)) {
                if (state.stateName != LevelProperties.Devil.States.Main) {
                    continue;
                }
                string[] lists = state.pitchfork.patternString;
                if (lists == null || lists.Length == 0) {
                    return new RngOption[0];
                }
                string[] setNames = { "Set 1", "Set 2", "Set 3" };
                RngOption[] options = new RngOption[lists.Length];
                for (int i = 0; i < lists.Length; i++) {
                    options[i] = new RngOption(i < setNames.Length ? setNames[i] : "Set " + (i + 1), i);
                }
                return options;
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>The attack types present in one order list, in first-appearance order.
    /// Values match the PitchforkType spot's option values (4=Bouncer, 5=Pinwheel, 6=Ring).</summary>
    public static RngOption[] DevilPitchforkTypeOptions(Level.Mode mode, int orderIndex) {
        try {
            foreach (LevelProperties.Devil.State state in StatesFor(mode)) {
                if (state.stateName != LevelProperties.Devil.States.Main) {
                    continue;
                }
                string[] lists = state.pitchfork.patternString;
                if (lists == null || orderIndex < 0 || orderIndex >= lists.Length) {
                    return new RngOption[0];
                }
                return DistinctPitchforkEntries(lists[orderIndex]);
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static RngOption[] DistinctPitchforkEntries(string list) {
        List<RngOption> options = new List<RngOption>();
        Dictionary<int, bool> seen = new Dictionary<int, bool>();
        string[] entries = list.Split(new[] { ',' });
        foreach (string entry in entries) {
            int parsed;
            if (!int.TryParse(entry.Trim(), out parsed) || parsed < 4 || parsed > 6 || seen.ContainsKey(parsed)) {
                continue;
            }
            seen[parsed] = true;
            options.Add(new RngOption(PrettyPitchforkEntry(parsed), parsed - 4));
        }
        return options.ToArray();
    }

    private static string PrettyPitchforkEntry(int parsed) {
        switch (parsed) {
            case 4:
                return "Bouncer";
            case 5:
                return "Pinwheel";
            case 6:
                return "Ring";
            default:
                return "#" + parsed;
        }
    }

    private static string PrettyPattern(LevelProperties.Devil.Pattern pattern) {
        switch (pattern) {
            case LevelProperties.Devil.Pattern.Head:
                return "Head";
            case LevelProperties.Devil.Pattern.Clap:
                return "Clap";
            case LevelProperties.Devil.Pattern.Pitchfork:
                return "Pitchfork";
            case LevelProperties.Devil.Pattern.BombEye:
                return "Bomb Eye";
            case LevelProperties.Devil.Pattern.SkullEye:
                return "Skull Eye";
            default:
                return pattern.ToString();
        }
    }

    // ------------------------------------------------------------------
    // Ribby & Croaks option enumeration. Same approach as the Devil's: the
    // game's own LevelProperties.Frogs data is read via reflection so labels
    // and counts match the version being played.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Frogs.State[]> FrogStatesCache = new Dictionary<Level.Mode, LevelProperties.Frogs.State[]>();

    private static LevelProperties.Frogs.State[] FrogStatesFor(Level.Mode mode) {
        LevelProperties.Frogs.State[] states;
        if (FrogStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Frogs properties = LevelProperties.Frogs.GetMode(mode);
            states = (LevelProperties.Frogs.State[])FindField(typeof(LevelProperties.Frogs), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Frogs.State[0];
        }

        FrogStatesCache[mode] = states;
        return states;
    }

    private static LevelProperties.Frogs.State FrogsMainState(Level.Mode mode) {
        foreach (LevelProperties.Frogs.State state in FrogStatesFor(mode)) {
            if (state.stateName == LevelProperties.Frogs.States.Main) {
                return state;
            }
        }
        return null;
    }

    private static RngOption[] FrogsStatePatterns(Level.Mode mode) {
        try {
            LevelProperties.Frogs.State state = FrogsMainState(mode);
            if (state == null) {
                return new RngOption[0];
            }
            string[] labels = new string[state.patterns.Length];
            for (int i = 0; i < state.patterns.Length; i++) {
                labels[i] = PrettyFrogsPattern(state.patterns[i]);
            }
            return NamedOptions(labels);
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static string PrettyFrogsPattern(LevelProperties.Frogs.Pattern pattern) {
        switch (pattern) {
            case LevelProperties.Frogs.Pattern.TallFireflies:
                return "Fireflies";
            case LevelProperties.Frogs.Pattern.ShortRage:
                return "Punches";
            case LevelProperties.Frogs.Pattern.RagePlusFireflies:
                return "Rage + Fireflies";
            default:
                return pattern.ToString();
        }
    }

    /// <summary>Legacy setting names for one fireflies pattern, in the game's own list order.</summary>
    private static string[] FrogsFirefliesTags(Level.Mode mode) {
        switch (mode) {
            case Level.Mode.Easy:
                return new[] { "2-1-1 (Slow)", "1-1-2", "2-1-1 (Fast)" };
            case Level.Mode.Hard:
                return new[] { "2-2-1", "1-1-2", "2-2 (Fast)", "2-1-1", "1-2-2", "2-2 (Slow)", "1-2-1" };
            default:
                return new[] { "2-2-1-2", "2-2-2", "2-1-2-2", "2-2-1" };
        }
    }

    private static RngOption[] FrogsFirefliesOptions(Level.Mode mode) {
        try {
            LevelProperties.Frogs.State state = FrogsMainState(mode);
            if (state == null) {
                return new RngOption[0];
            }
            string[] strings = state.tallFireflies.patterns;
            if (strings == null || strings.Length == 0) {
                return new RngOption[0];
            }
            // labels are the legacy setting names; a longer game table than the tag list
            // (e.g. a different game version) falls back to numbered labels
            string[] tags = FrogsFirefliesTags(mode);
            RngOption[] options = new RngOption[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                options[i] = new RngOption(i < tags.Length ? tags[i] : "#" + (i + 1), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>The game's own fireflies pattern strings for the current difficulty, in list
    /// order; the game patch resolves against these.</summary>
    public static string[] FrogsFirefliesStrings(Level.Mode mode) {
        try {
            LevelProperties.Frogs.State state = FrogsMainState(mode);
            if (state == null) {
                return new string[0];
            }
            return state.tallFireflies.patterns ?? new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    // ------------------------------------------------------------------
    // Goopy Le Grande option enumeration. The jump cycle lives in
    // LevelProperties.Slime.GetMode(mode) per difficulty.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Slime.State[]> SlimeStatesCache = new Dictionary<Level.Mode, LevelProperties.Slime.State[]>();

    private static LevelProperties.Slime.State[] SlimeStatesFor(Level.Mode mode) {
        LevelProperties.Slime.State[] states;
        if (SlimeStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Slime properties = LevelProperties.Slime.GetMode(mode);
            states = (LevelProperties.Slime.State[])FindField(typeof(LevelProperties.Slime), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Slime.State[0];
        }

        SlimeStatesCache[mode] = states;
        return states;
    }

    private static LevelProperties.Slime.State SlimeMainState(Level.Mode mode) {
        foreach (LevelProperties.Slime.State state in SlimeStatesFor(mode)) {
            if (state.stateName == LevelProperties.Slime.States.Main) {
                return state;
            }
        }
        return null;
    }

    /// <summary>One option per entry of the Main state's fixed jump cycle.</summary>
    private static RngOption[] SlimeJumpOptions(Level.Mode mode) {
        try {
            LevelProperties.Slime.State state = SlimeMainState(mode);
            if (state == null) {
                return new RngOption[0];
            }
            string pattern = state.jump.patternString;
            if (string.IsNullOrEmpty(pattern)) {
                return new RngOption[0];
            }
            string[] entries = pattern.Split(new[] { ',' });
            string[] labels = new string[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                string entry = entries[i].Trim();
                if (entry.Length == 0) {
                    labels[i] = "-";
                } else if (entry[0] == 'H') {
                    labels[i] = "High";
                } else if (entry[0] == 'L') {
                    labels[i] = "Low";
                } else if (entry[0] == 'R') {
                    labels[i] = "Random Height";
                } else if (entry[0] == 'D') {
                    labels[i] = "Delay";
                } else {
                    labels[i] = entry;
                }
            }
            return NamedOptions(labels);
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    // ------------------------------------------------------------------
    // The Root Pack option enumeration. The carrot data lives in
    // LevelProperties.Veggies.GetMode(mode) per difficulty.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Veggies.State[]> VeggiesStatesCache = new Dictionary<Level.Mode, LevelProperties.Veggies.State[]>();

    private static LevelProperties.Veggies.State[] VeggiesStatesFor(Level.Mode mode) {
        LevelProperties.Veggies.State[] states;
        if (VeggiesStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Veggies properties = LevelProperties.Veggies.GetMode(mode);
            states = (LevelProperties.Veggies.State[])FindField(typeof(LevelProperties.Veggies), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Veggies.State[0];
        }

        VeggiesStatesCache[mode] = states;
        return states;
    }

    private static LevelProperties.Veggies.State VeggiesMainState(Level.Mode mode) {
        foreach (LevelProperties.Veggies.State state in VeggiesStatesFor(mode)) {
            if (state.stateName == LevelProperties.Veggies.States.Main) {
                return state;
            }
        }
        return null;
    }

    /// <summary>One option per value of the live homing carrot count range (the int roll
    /// is upper-exclusive, so the max itself never occurs).</summary>
    private static RngOption[] VeggiesCarrotCountOptions(Level.Mode mode) {
        try {
            LevelProperties.Veggies.State state = VeggiesMainState(mode);
            if (state == null) {
                return new RngOption[0];
            }
            MinMax range = state.carrot.homingNumOfCarrots;
            int from = (int)range.min;
            int count = (int)range.max - from;
            return count > 0 ? Numbered(from, count) : new RngOption[0];
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>The lower bound of the live carrot count range; the resolver shifts picks
    /// into the game's own range with it.</summary>
    public static int VeggiesCarrotCountMin(Level.Mode mode) {
        try {
            LevelProperties.Veggies.State state = VeggiesMainState(mode);
            if (state == null) {
                return 0;
            }
            return (int)state.carrot.homingNumOfCarrots.min;
        } catch (Exception) {
        }
        return 0;
    }

    /// <summary>Number of carrot count options on this difficulty (used by the resolver's
    /// vanilla passthrough).</summary>
    public static int VeggiesCarrotCountOptionCount(Level.Mode mode) {
        return VeggiesCarrotCountOptions(mode).Length;
    }

    private static RngFloatRange VeggiesCarrotIdleRange(Level.Mode mode) {
        try {
            LevelProperties.Veggies.State state = VeggiesMainState(mode);
            if (state == null) {
                return new RngFloatRange(0f, 1f, 0.05f, 0.5f);
            }
            MinMax range = state.carrot.idleRange;
            return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) * 0.5f);
        } catch (Exception) {
        }
        return new RngFloatRange(0f, 1f, 0.05f, 0.5f);
    }

    /// <summary>One option per tear volley pattern of the live data (the onion phase does
    /// not exist on Simple, so this comes back empty there).</summary>
    private static RngOption[] VeggiesTearPatternOptions(Level.Mode mode) {
        string[] strings = VeggiesTearStrings(mode);
        RngOption[] options = new RngOption[strings.Length];
        for (int i = 0; i < strings.Length; i++) {
            options[i] = new RngOption("Volley " + (i + 1), i);
        }
        return options;
    }

    /// <summary>The game's own tear volley strings for the current difficulty, in list
    /// order; the game patch resolves against these.</summary>
    public static string[] VeggiesTearStrings(Level.Mode mode) {
        try {
            LevelProperties.Veggies.State state = VeggiesMainState(mode);
            if (state == null) {
                return new string[0];
            }
            return state.onion.tearPatterns ?? new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    // ------------------------------------------------------------------
    // Hilda Berg option enumeration. Each difficulty runs two Generic
    // blimp phases (the "second" and "third" blimp) whose fixed cycles
    // come from LevelProperties.FlyingBlimp.GetMode(mode).
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.FlyingBlimp.State[]> FlyingBlimpStatesCache = new Dictionary<Level.Mode, LevelProperties.FlyingBlimp.State[]>();

    private static LevelProperties.FlyingBlimp.State[] FlyingBlimpStatesFor(Level.Mode mode) {
        LevelProperties.FlyingBlimp.State[] states;
        if (FlyingBlimpStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.FlyingBlimp properties = LevelProperties.FlyingBlimp.GetMode(mode);
            states = (LevelProperties.FlyingBlimp.State[])FindField(typeof(LevelProperties.FlyingBlimp), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.FlyingBlimp.State[0];
        }

        FlyingBlimpStatesCache[mode] = states;
        return states;
    }

    /// <summary>The 0-based occurrence index of the given state among the Generic states
    /// (0 = the "second blimp" phase, 1 = the "third blimp" phase); -1 when it is not
    /// Generic. Identified by object identity, so per-version trigger differences are
    /// irrelevant.</summary>
    public static int FlyingBlimpGenericIndex(Level.Mode mode, LevelProperties.FlyingBlimp.State currentState) {
        try {
            int index = 0;
            foreach (LevelProperties.FlyingBlimp.State state in FlyingBlimpStatesFor(mode)) {
                if (state.stateName != LevelProperties.FlyingBlimp.States.Generic) {
                    continue;
                }
                if (ReferenceEquals(state, currentState)) {
                    return index;
                }
                index++;
            }
        } catch (Exception) {
        }
        return -1;
    }

    /// <summary>One option per entry of the given Generic phase's fixed cycle, labeled by
    /// attack type and per-type occurrence ("Shoot #2", "Tornado #1") in cycle order.</summary>
    private static RngOption[] FlyingBlimpGenericOptions(Level.Mode mode, int genericIndex) {
        try {
            int seen = 0;
            foreach (LevelProperties.FlyingBlimp.State state in FlyingBlimpStatesFor(mode)) {
                if (state.stateName != LevelProperties.FlyingBlimp.States.Generic) {
                    continue;
                }
                if (seen++ != genericIndex) {
                    continue;
                }
                string[] labels = new string[state.patterns.Length];
                for (int i = 0; i < state.patterns.Length; i++) {
                    labels[i] = PrettyFlyingBlimpPattern(state.patterns[i]);
                }
                return NamedOptions(labels);
            }
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static string PrettyFlyingBlimpPattern(LevelProperties.FlyingBlimp.Pattern pattern) {
        switch (pattern) {
            case LevelProperties.FlyingBlimp.Pattern.Tornado:
                return "Tornado";
            case LevelProperties.FlyingBlimp.Pattern.Shoot:
                return "Shoot";
            case LevelProperties.FlyingBlimp.Pattern.Dash:
                return "Dash";
            default:
                return pattern.ToString();
        }
    }

    // ------------------------------------------------------------------
    // Hilda Berg's moon phase. The star and UFO string data lives on the
    // Moon state of LevelProperties.FlyingBlimp.GetMode(mode) and differs
    // per version, so everything is read live.
    // ------------------------------------------------------------------

    private static LevelProperties.FlyingBlimp.State FlyingBlimpMoonState(Level.Mode mode) {
        foreach (LevelProperties.FlyingBlimp.State state in FlyingBlimpStatesFor(mode)) {
            if (state.stateName == LevelProperties.FlyingBlimp.States.Moon) {
                return state;
            }
        }
        return null;
    }

    /// <summary>The live star position/type sequences of the Moon state.</summary>
    public static string[] FlyingBlimpStarStrings(Level.Mode mode, bool positions) {
        try {
            LevelProperties.FlyingBlimp.State moon = FlyingBlimpMoonState(mode);
            if (moon == null) {
                return new string[0];
            }
            string[] strings = positions ? moon.stars.positionString : moon.stars.typeString;
            return strings ?? new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    /// <summary>One option per star position/type sequence of the live data, named "Set N".</summary>
    private static RngOption[] FlyingBlimpStarPatternOptions(Level.Mode mode, bool positions) {
        return SetOptions(FlyingBlimpStarStrings(mode, positions).Length);
    }

    /// <summary>The start options of one star sequence: position values, or star types
    /// ("Star A", "Pink Star") with per-type occurrence numbering.</summary>
    public static RngOption[] FlyingBlimpStarStartOptions(Level.Mode mode, bool positions, int stringIndex) {
        string[] strings = FlyingBlimpStarStrings(mode, positions);
        if (stringIndex < 0 || stringIndex >= strings.Length) {
            return new RngOption[0];
        }
        string[] entries = strings[stringIndex].Split(new[] { ',' });
        string[] labels = new string[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            labels[i] = positions ? entries[i].Trim() : PrettyStarType(entries[i]);
        }
        return NamedOptions(labels);
    }

    /// <summary>Entry count of one star sequence; the resolver bounds the start against
    /// the sequence the round actually picked.</summary>
    public static int FlyingBlimpStarStartCount(Level.Mode mode, bool positions, int stringIndex) {
        string[] strings = FlyingBlimpStarStrings(mode, positions);
        if (stringIndex < 0 || stringIndex >= strings.Length) {
            return 0;
        }
        return strings[stringIndex].Split(new[] { ',' }).Length;
    }

    private static string PrettyStarType(string entry) {
        if (entry.Length == 0) {
            return "-" + (entry.Length + 1);
        }
        switch (entry[0]) {
            case 'A':
                return "Star A";
            case 'B':
                return "Star B";
            case 'C':
                return "Star C";
            case 'P':
                return "Pink Star";
            default:
                return entry;
        }
    }

    /// <summary>The live star speed range from the Moon state's data.</summary>
    private static RngFloatRange FlyingBlimpStarSpeedRange(Level.Mode mode) {
        try {
            LevelProperties.FlyingBlimp.State moon = FlyingBlimpMoonState(mode);
            if (moon == null) {
                return new RngFloatRange(0f, 1f, 10f, 0.5f);
            }
            MinMax speed = moon.stars.speedX;
            return new RngFloatRange(speed.min, speed.max, 10f, (speed.min + speed.max) * 0.5f);
        } catch (Exception) {
        }
        return new RngFloatRange(0f, 1f, 10f, 0.5f);
    }

    /// <summary>The live UFO sequences of the Moon state; the game patch resolves
    /// against these.</summary>
    public static string[] FlyingBlimpUfoStrings(Level.Mode mode) {
        try {
            LevelProperties.FlyingBlimp.State moon = FlyingBlimpMoonState(mode);
            if (moon == null) {
                return new string[0];
            }
            return moon.uFO.UFOString ?? new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    /// <summary>One option per UFO sequence of the live data, named "Set N".</summary>
    private static RngOption[] FlyingBlimpUfoPatternOptions(Level.Mode mode) {
        return SetOptions(FlyingBlimpUfoStrings(mode).Length);
    }

    /// <summary>The start options of one UFO sequence ("UFO A", "UFO B") with per-type
    /// occurrence numbering.</summary>
    public static RngOption[] FlyingBlimpUfoStartOptions(Level.Mode mode, int stringIndex) {
        string[] strings = FlyingBlimpUfoStrings(mode);
        if (stringIndex < 0 || stringIndex >= strings.Length) {
            return new RngOption[0];
        }
        string[] entries = strings[stringIndex].Split(new[] { ',' });
        string[] labels = new string[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            string entry = entries[i].Trim();
            labels[i] = entry.Length > 0 && entry[0] == 'B' ? "Yellow" : "Red";
        }
        return NamedOptions(labels);
    }

    /// <summary>Entry count of one UFO sequence.</summary>
    public static int FlyingBlimpUfoStartCount(Level.Mode mode, int stringIndex) {
        string[] strings = FlyingBlimpUfoStrings(mode);
        if (stringIndex < 0 || stringIndex >= strings.Length) {
            return 0;
        }
        return strings[stringIndex].Split(new[] { ',' }).Length;
    }

    /// <summary>The live onion phase data (tear timings, pink range); null when the
    /// difficulty has no onion phase.</summary>
    public static LevelProperties.Veggies.Onion VeggiesOnionData(Level.Mode mode) {
        try {
            LevelProperties.Veggies.State state = VeggiesMainState(mode);
            return state != null ? state.onion : null;
        } catch (Exception) {
        }
        return null;
    }

    /// <summary>One option per value of the live pink tear cadence range (the int roll is
    /// upper-exclusive, so the max itself never occurs).</summary>
    private static RngOption[] VeggiesPinkOptions(Level.Mode mode) {
        try {
            LevelProperties.Veggies.Onion onion = VeggiesOnionData(mode);
            if (onion == null) {
                return new RngOption[0];
            }
            MinMax range = onion.pinkTearRange;
            int from = (int)range.min;
            int count = (int)range.max - from;
            return count > 0 ? Numbered(from, count) : new RngOption[0];
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>The lower bound of the live pink cadence range; the resolver shifts picks
    /// into the game's own range with it.</summary>
    public static int VeggiesPinkMin(Level.Mode mode) {
        try {
            LevelProperties.Veggies.Onion onion = VeggiesOnionData(mode);
            return onion != null ? (int)onion.pinkTearRange.min : 0;
        } catch (Exception) {
        }
        return 0;
    }

    /// <summary>Number of pink cadence options on this difficulty.</summary>
    public static int VeggiesPinkOptionCount(Level.Mode mode) {
        return VeggiesPinkOptions(mode).Length;
    }

    // ------------------------------------------------------------------
    // Forest Follies option enumeration. The acorn spawner's cycle strings
    // are serialized in the scene, so they are read from a live spawner
    // when one exists; the fallback matches every shipped game version.
    // ------------------------------------------------------------------

    private const string ForestAcornSideFallback = "LRLLRLRR";
    private const string ForestAcornHeightFallback = "150,50,120";

    /// <summary>One option per entry of the spawner's left/right cycle.</summary>
    private static RngOption[] ForestAcornSideOptions() {
        string s = ForestAcornLiveSpawnerString(sp => sp.leftRightString, ForestAcornSideFallback);
        RngOption[] options = new RngOption[s.Length];
        for (int i = 0; i < s.Length; i++) {
            options[i] = new RngOption(s[i] == 'L' ? "Right" : "Left", i);
        }
        return options;
    }

    /// <summary>One option per entry of the spawner's height cycle.</summary>
    private static RngOption[] ForestAcornHeightOptions() {
        string s = ForestAcornLiveSpawnerString(sp => sp.yString, ForestAcornHeightFallback);
        string[] entries = s.Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            options[i] = new RngOption(entries[i].Trim(), i);
        }
        return options;
    }

    private static string ForestAcornLiveSpawnerString(Func<ForestPlatformingLevelAcornSpawner, string> field, string fallback) {
        try {
            ForestPlatformingLevelAcornSpawner spawner = UnityEngine.Object.FindObjectOfType<ForestPlatformingLevelAcornSpawner>();
            if (spawner != null) {
                string value = field(spawner);
                if (!string.IsNullOrEmpty(value)) {
                    return value;
                }
            }
        } catch (Exception) {
        }
        return fallback;
    }

    private static RngOption[] Fixed(params string[] labels) {
        RngOption[] options = new RngOption[labels.Length];
        for (int i = 0; i < labels.Length; i++) {
            options[i] = new RngOption(labels[i], i);
        }
        return options;
    }

    private static RngOption[] Numbered(int from, int count) {
        RngOption[] options = new RngOption[count];
        for (int i = 0; i < count; i++) {
            options[i] = new RngOption((from + i).ToString(), i);
        }
        return options;
    }

    /// <summary>Maps labels 1:1 onto their index (duplicate labels are fine; rows are
    /// identified by position, not name).</summary>
    private static RngOption[] NamedOptions(string[] labels) {
        RngOption[] options = new RngOption[labels.Length];
        for (int i = 0; i < labels.Length; i++) {
            options[i] = new RngOption(labels[i], i);
        }
        return options;
    }

    /// <summary>Names for the set-style cyclers ("Set 1", "Set 2", ...). Each call site
    /// that shows a set list passes its count here; edit a call site directly if a set
    /// needs a more descriptive name.</summary>
    private static RngOption[] SetOptions(int count) {
        string[] labels = new string[count];
        for (int i = 0; i < count; i++) {
            labels[i] = "Set " + (i + 1);
        }
        return NamedOptions(labels);
    }
}
