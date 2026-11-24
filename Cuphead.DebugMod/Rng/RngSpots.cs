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

/// <summary>Which editor the menu uses for a spot. Derived keeps the flag-based default;
/// the explicit kinds exist so a setting can use a different UI per difficulty (each
/// difficulty's definition names its own).</summary>
internal enum RngUi {
    /// <summary>Derive the editor from the spot's shape and flags.</summary>
    Derived,

    /// <summary>The plain radio list, never inside a compound card.</summary>
    Radio,

    /// <summary>The shotgun-style card: one pattern pick plus a "Start at" radio.</summary>
    CompoundStart,

    /// <summary>The candy-corn card: a per-event pattern pick plus a walked start index.</summary>
    IndexedPattern,

    /// <summary>Per-round compound blocks (the UFO pattern style).</summary>
    SequenceCompound
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
    Sequence,

    /// <summary>A start position the game can only roll for part of the list; the radio
    /// list shows the rest grayed out and unselectable.</summary>
    Restricted,

    /// <summary>A per-pass pattern pick plus a once-per-fight walked index (the candy
    /// corn's turn driver).</summary>
    IndexedPattern
}

internal sealed class RngOption {
    public readonly string Label;
    public readonly int Value;
    public readonly bool Selectable;

    /// <summary>Per-column label texts for multi-column radio rows; null when the option
    /// renders as the plain Label string. Columns are drawn as separate texts inside
    /// fixed-width rects so they line up across rows in any (proportional) font.</summary>
    public readonly string[] Columns;

    public RngOption(string label, int value, bool selectable = true) {
        Label = label;
        Value = value;
        Selectable = selectable;
    }

    /// <summary>The label of a column option is its " / "-joined form (used by summaries
    /// and previews); the radio row draws the columns individually.</summary>
    public RngOption(string[] columns, int value, bool selectable = true) {
        Columns = columns;
        Label = string.Join(" / ", columns);
        Value = value;
        Selectable = selectable;
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

    /// <summary>True for per-event spots whose roll happens exactly once per fight (the
    /// Baroness miniboss picks, the waffle's path): they use the same cycler editor, but
    /// the footer's constant-pin warning does not apply (a pin is just one outcome vanilla
    /// can roll) and plans never advance past their first step, so they are hidden.</summary>
    public bool OncePerFight {
        get; set;
    }

    /// <summary>Optional badge override; derived from the shape when unset.</summary>
    public RngBadgeKind? Badge {
        get; set;
    }

    /// <summary>Restricts this definition to one difficulty. Per-difficulty settings whose
    /// UI or options differ per difficulty are defined as separate per-difficulty spots;
    /// PerDifficulty definitions are expanded into one OnlyFor variant per difficulty
    /// automatically.</summary>
    public Level.Mode? OnlyFor {
        get; set;
    }

    /// <summary>Explicit editor for this definition's Simple variant; Derived keeps the
    /// flag-based default.</summary>
    public RngUi? UiEasy {
        get; set;
    }

    /// <summary>Explicit editor for this definition's Regular variant.</summary>
    public RngUi? UiNormal {
        get; set;
    }

    /// <summary>Explicit editor for this definition's Expert variant.</summary>
    public RngUi? UiHard {
        get; set;
    }

    /// <summary>The editor this spot uses on a difficulty: the definition's per-difficulty
    /// choice, or the flag-based default.</summary>
    public RngUi UiFor(Level.Mode mode) {
        switch (mode) {
            case Level.Mode.Easy:
                return UiEasy ?? RngUi.Derived;
            case Level.Mode.Hard:
                return UiHard ?? RngUi.Derived;
            default:
                return UiNormal ?? RngUi.Derived;
        }
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
        if (OnlyFor.HasValue && OnlyFor.Value != mode) {
            return false;
        }
        return Available == null || Available(mode);
    }

    /// <summary>A per-difficulty variant of a PerDifficulty definition: same setting and
    /// config key, its own UI kind.</summary>
    public RngSpot(RngSpot source, Level.Mode onlyFor) {
        Id = source.Id;
        Group = source.Group;
        Label = source.Label;
        Shape = source.Shape;
        PerDifficulty = source.PerDifficulty;
        PreIncrement = source.PreIncrement;
        Options = source.Options;
        FloatRange = source.FloatRange;
        Hint = source.Hint;
        Available = source.Available;
        PerEvent = source.PerEvent;
        OncePerFight = source.OncePerFight;
        Badge = source.Badge;
        UiEasy = source.UiEasy;
        UiNormal = source.UiNormal;
        UiHard = source.UiHard;
        CompoundWith = source.CompoundWith;
        CompoundSecondary = source.CompoundSecondary;
        CompoundStartOptions = source.CompoundStartOptions;
        CompoundPrimaryLabel = source.CompoundPrimaryLabel;
        CompoundSecondaryLabel = source.CompoundSecondaryLabel;
        RadioPlan = source.RadioPlan;
        OnlyFor = onlyFor;
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

    // Cagney Carnation (the phase cycles have start rolls on Regular only; the Easy and
    // Expert cycles are single-entry)
    public const string FlowerMainPatternStart = "Flower/Phase1/PatternStart";
    public const string FlowerGeneric1PatternStart = "Flower/Phase2/PatternStart";
    public const string FlowerGeneric2PatternStart = "Flower/Phase3/PatternStart";
    public const string FlowerHeadLungeStart = "Flower/HeadLunge/Start";
    public const string FlowerPodHandsType = "Flower/PodHands/Type";
    public const string FlowerPodHandsCount = "Flower/PodHands/Count";
    public const string FlowerBlinkCount = "Flower/Blink/Count";

    // Baroness Von Bon Bon (the miniboss roster differs per difficulty; the picks are
    // made once per fight)
    public const string BaronessMiniboss1 = "Baroness/Miniboss1/Pick";
    public const string BaronessMiniboss2 = "Baroness/Miniboss2/Pick";
    public const string BaronessMiniboss3 = "Baroness/Miniboss3/Pick";
    public const string BaronessWafflePath = "Baroness/Waffle/Path";
    public const string BaronessWaffleSplitDelay = "Baroness/Waffle/SplitDelay";
    public const string BaronessWaffleSplitOrder = "Baroness/Waffle/SplitOrder";
    public const string BaronessShotgunPattern = "Baroness/Shotgun/Pattern";
    public const string BaronessShotgunStart = "Baroness/Shotgun/Start";
    public const string BaronessShotgunDelay = "Baroness/Shotgun/Delay";
    public const string BaronessHeadTossStart = "Baroness/HeadToss/Start";
    public const string BaronessHeadTossDelay = "Baroness/HeadToss/Delay";
    public const string BaronessCornTurnStart = "Baroness/CornTurn/Start";
    public const string BaronessCornTurnPattern = "Baroness/CornTurn/Pattern";
    public const string BaronessCornTurnMiniDelay = "Baroness/CornTurn/MiniDelay";
    public const string BaronessJellybeanPattern = "Baroness/Jellybeans/Pattern";
    public const string BaronessJellybeanDelay = "Baroness/Jellybeans/Delay";

    // Funfair Fever (run 'n gun; always runs on Normal; one cannon and one hotdog, each
    // rolling its start positions once at Start)
    public const string FunfairCannonOrderDirection = "FunfairFever/Cannon/OrderDirection";
    public const string FunfairCannonColorStart = "FunfairFever/Cannon/ColorStart";
    public const string FunfairCannonMuzzleStart = "FunfairFever/Cannon/MuzzleStart";
    public const string FunfairHotdogSpawnStart = "FunfairFever/Hotdog/SpawnStart";
    public const string FunfairHotdogCondimentStart = "FunfairFever/Hotdog/CondimentStart";
    public const string FunfairHotdogSideStart = "FunfairFever/Hotdog/SideStart";
    public const string FunfairHotdogDelayStart = "FunfairFever/Hotdog/DelayStart";

    // Wally Warbles (plane fight; the state lists differ per difficulty, so the pattern
    // spots are per-difficulty)
    public const string FlyingBirdMainPatternStart = "FlyingBird/Main/PatternStart";
    public const string FlyingBirdGenericPatternStart = "FlyingBird/Generic/PatternStart";
    public const string FlyingBirdRevivalPatternStart = "FlyingBird/Revival/PatternStart";
    public const string FlyingBirdFloatDirection = "FlyingBird/Main/FloatDirection";
    public const string FlyingBirdBirdDirection = "FlyingBird/Whistle/BirdDirection";
    public const string FlyingBirdStretcherDirection = "FlyingBird/Revival/StretcherDirection";

    // Djimmi The Great (the chest attack is rolled once per fight; the parry cycles are
    // rolled once per fight and walked; the obelisk start is a flat row-major combo into
    // the 3x9 obeliskGeniePos grid)
    public const string FlyingGenieTreasureAttack = "FlyingGenie/Main/TreasureAttack";
    public const string FlyingGenieSwordPinkStart = "FlyingGenie/Main/SwordPinkStart";
    public const string FlyingGenieGemPinkStart = "FlyingGenie/Main/GemPinkStart";
    public const string FlyingGenieSphinxPinkStart = "FlyingGenie/Main/SphinxPinkStart";
    public const string FlyingGenieObeliskPatternStart = "FlyingGenie/Obelisk/PatternStart";

    // Beppi The Clown (the bumper car rolls its delay and drift starts once per fight;
    // the duck cycles are rolled once per fight and walked; the horse re-rolls its side
    // on every attack while its attack type cycle is rolled once)
    public const string ClownBumperDelay = "Clown/BumperCar/Delay";
    public const string ClownBumperCarDriftDirection = "Clown/BumperCar/DriftDirection";
    public const string ClownDuckTypePatternString = "Clown/Ducks/TypePatternString";
    public const string ClownDuckTypePattern = "Clown/Ducks/TypePattern";
    public const string ClownHorseType = "Clown/Horse/Type";
    public const string ClownHorseSide = "Clown/Horse/Side";

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
            ) { OnlyFor = Level.Mode.Normal },
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

    public static readonly RngSpot[] Flower = {
        new RngSpot(
            FlowerMainPatternStart, "Phase 1", "Overall Pattern", RngShape.StartPool, false, true,
            mode => FlowerPatternOptions(mode, 0),
            "Which attack the fight's fixed cycle begins with."
        ) { OnlyFor = Level.Mode.Normal },
        new RngSpot(
            FlowerGeneric1PatternStart, "Phase 2", "Overall Pattern", RngShape.StartPool, false, true,
            mode => FlowerPatternOptions(mode, 1),
            "Which attack this phase's fixed cycle begins with (from 89% boss health)."
        ) { OnlyFor = Level.Mode.Normal },
        new RngSpot(
            FlowerGeneric2PatternStart, "Phase 3", "Overall Pattern", RngShape.StartPool, false, true,
            mode => FlowerPatternOptions(mode, 2),
            "Which attack this phase's fixed cycle begins with (from 68% boss health)."
        ) { OnlyFor = Level.Mode.Normal },
        new RngSpot(
            FlowerHeadLungeStart, "Head Lunge", "Head Lunge Start", RngShape.StartPool, true, false,
            mode => FlowerLaserOptions(mode),
            "The head lunges in a fixed top/bottom order; this picks which lunge the first one is."
        ),
        new RngSpot(
            FlowerPodHandsType, "Pod Hands", "Attack Type Start", RngShape.StartPool, true, false,
            mode => FlowerPodTypeOptions(mode),
            "The starting entry is rolled once at the start of the fight and then followed in order across phases. Each row shows the attack type that entry produces in every pod-hands phase (Phase 2 / Phase 3 on Regular); grayed rows can never be rolled as a start. The type applies the moment the Pod Hands starts, not the moment that the decision to do Pod Hands is locked in, which may lead to confusion when crossing a phase threshold just before this attack."
        ) { Badge = RngBadgeKind.Restricted },
        new RngSpot(
            FlowerPodHandsCount, "Pod Hands", "Volley Count Start", RngShape.StartPool, true, false,
            mode => FlowerPodCountOptions(mode),
            "The starting entry is rolled once at the start of the fight and then followed in order across phases. Each row shows the volley count that entry produces in every pod-hands phase (Phase 2 / Phase 3 on Regular); on Regular, rows 15-17 overflow Phase 3's shorter cycle and wrap to row 0. The count applies the moment the Pod Hands starts, not the moment that the decision to do Pod Hands is locked in, which may lead to confusion when crossing a phase threshold just before this attack."
        ),
        new RngSpot(
            FlowerBlinkCount, "Blink", "Blink Count", RngShape.Choice, false, false,
            mode => Numbered(2, 3),
            "Idle loops the flower plays between blinks, re-rolled after every blink - this gates the fight's pacing."
        ) { PerEvent = true }
    };

    public static readonly RngSpot[] Baroness = {
        new RngSpot(
            BaronessMiniboss1, "Phase 1", "Miniboss 1", RngShape.Choice, true, false,
            mode => BaronessMinibossOptions(mode),
            "The fight picks its minibosses one after another from a pool, never repeating one within a fight; this pins the first pick."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            BaronessMiniboss2, "Phase 1", "Miniboss 2", RngShape.Choice, true, false,
            mode => BaronessMinibossOptions(mode),
            "Pins the second miniboss pick. Forcing the same miniboss the earlier picks already used is possible but not vanilla-accurate (a warning shows when it is selected)."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            BaronessMiniboss3, "Phase 1", "Miniboss 3", RngShape.Choice, true, false,
            mode => BaronessMinibossOptions(mode),
            "Pins the third miniboss pick. Forcing the same miniboss the earlier picks already used is possible but not vanilla-accurate (a warning shows when it is selected)."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            BaronessWafflePath, "Phase 1", "Waffle Path", RngShape.Choice, false, false,
            mode => Fixed("Up", "Down"),
            "Which path the waffle takes, rolled once when it is picked as a miniboss and spawns."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            BaronessWaffleSplitDelay, "Phase 1", "Waffle Split Delay", RngShape.Float, true,
            mode => BaronessWaffleSplitDelayRange(mode),
            "Delay between the waffle reassembling and splitting apart again, re-rolled per split."
        ) { PerEvent = true, Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            BaronessWaffleSplitOrder, "Phase 1", "Waffle Split Order", RngShape.Choice, true, false,
            mode => Fixed("Diagonal First", "Straight First"),
            "Which half detaches first when the waffle splits apart."
        ) { PerEvent = true, OnlyFor = Level.Mode.Hard },
        new RngSpot(
            BaronessShotgunPattern, "Phase 1", "Shotgun Pop-Up Delay", RngShape.Choice, true, false,
            mode => BaronessShotgunPatternOptions(mode),
            "One pop-up timing pattern is picked once per fight and then walked (each entry is the seconds until the baroness's next pop-up); the start below chooses where it begins."
        ) { CompoundWith = BaronessShotgunStart, Badge = RngBadgeKind.Order,
            CompoundStartOptions = (mode, patternIndex) => BaronessShotgunStartOptions(mode, patternIndex),
            CompoundPrimaryLabel = "Pattern", CompoundSecondaryLabel = "Start at" },
        new RngSpot(
            BaronessShotgunStart, "Delay", "Shotgun Pop-Up Delay Start", RngShape.Choice, true, false,
            mode => new RngOption[0],
            null
        ) { CompoundSecondary = true },
        new RngSpot(
            BaronessShotgunDelay, "Phase 1", "Shotgun Delay", RngShape.Float, true,
            mode => BaronessShotgunDelayRange(mode),
            "Delay between the baroness's shots while she is popped up, re-rolled per shot."
        ) { PerEvent = true },
        new RngSpot(
            BaronessCornTurnPattern, "Phase 1", "Candy Corn Turn Pattern String", RngShape.Choice, true, false,
            mode => BaronessCornTurnPatternOptions(mode),
            "Re-rolled every time the corn passes the castle; each option is the full turn pattern (Y = turn up, N = keep walking). A plan plays one pattern per pass."
        ) { PerEvent = true, Badge = RngBadgeKind.IndexedPattern, CompoundWith = BaronessCornTurnStart,
            CompoundPrimaryLabel = "Candy Corn Turn Pattern String",
            UiEasy = RngUi.IndexedPattern, UiNormal = RngUi.Radio, UiHard = RngUi.IndexedPattern },
        new RngSpot(
            BaronessCornTurnStart, "Phase 1", "Candy Corn Turn Pattern", RngShape.Choice, true, false,
            mode => BaronessCornTurnStartOptions(mode),
            "The starting position inside the turn pattern, rolled once per fight and then walked. Indexes beyond a shorter pattern's length break the corn's turning for the rest of the fight - exactly as they can in vanilla."
        ) { PerEvent = true, Badge = RngBadgeKind.Start, OncePerFight = true, CompoundSecondary = true,
            CompoundSecondaryLabel = "Candy Corn Turn Pattern Initial Index",
            UiEasy = RngUi.IndexedPattern, UiNormal = RngUi.Radio, UiHard = RngUi.IndexedPattern },
        new RngSpot(
            BaronessCornTurnMiniDelay, "Phase 1", "Mini Corn Spawn Delay", RngShape.Float, true,
            mode => BaronessCornTurnMiniDelayRange(mode),
            "Delay between mini candy corn spawns while the big one turns, re-rolled per mini."
        ) { PerEvent = true, Available = mode => {
                LevelProperties.Baroness.CandyCorn corn = BaronessCornData(mode);
                return corn != null && corn.spawnMinis;
            } },
        new RngSpot(
            BaronessJellybeanPattern, "Phase 1", "Jellybean Spawn Pattern", RngShape.Choice, true, false,
            mode => BaronessJellybeanPatternOptions(mode),
            "Picked once per fight and then cycled - transitions skip entries without spawning them. Each option is the full bean sequence (P = parryable). NOTE: the way jellybeans spawns work is extremely janky and it will result in getting unexpected (but consistent) first spawns."
        ),
        new RngSpot(
            BaronessJellybeanDelay, "Phase 1", "Jellybean Spawn Delay", RngShape.Float, true,
            mode => BaronessJellybeanDelayRange(mode),
            "Delay between jellybean spawns, re-rolled per spawned bean - the sequence only advances on real spawns, never on the skipped transition frames."
        ) { PerEvent = true },
        new RngSpot(
            BaronessHeadTossStart, "Final Phase", "Head Toss Pattern", RngShape.StartPool, true, false,
            mode => BaronessHeadTossOptions(mode),
            "The final phase tosses its head and waits in a fixed repeating script; this picks where the script begins."
        ),
        new RngSpot(
            BaronessHeadTossDelay, "Final Phase", "Head Toss Delay (subsequent)", RngShape.Float, true,
            mode => BaronessHeadTossDelayRange(mode),
            "Wait after each head toss, re-rolled per toss."
        ) { PerEvent = true }
    };

    public static readonly RngSpot[] FunfairFever = {
        new RngSpot(
            FunfairCannonOrderDirection, "Cannon", "Cannonball Order Direction", RngShape.Choice, false, false,
            mode => Fixed("Up", "Down"),
            "The direction the cannon walks its three muzzles in: Up fires them bottom-to-top, Down top-to-bottom."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            FunfairCannonColorStart, "Cannon", "Cannonball Color Start", RngShape.StartPool, false, false,
            mode => FunfairCannonColorOptions(),
            "The starting position inside the cannonball color cycle; this picks which color the first cannonball is (pink ones are parryable)."
        ),
        new RngSpot(
            FunfairCannonMuzzleStart, "Cannon", "Cannonball Muzzle Start", RngShape.StartPool, false, false,
            mode => FunfairCannonMuzzleOptions(),
            "The muzzle the first cannonball falls from; the cannon then walks the muzzles in its order direction."
        ),
        new RngSpot(
            FunfairHotdogSpawnStart, "Hotdog", "Hotdog Condiment Height", RngShape.StartPool, false, false,
            mode => FunfairHotdogSpawnOptions(),
            "The height the hotdog's first condiment spawns at (each entry is a spawn point, mirrored to the right side for right-side shots)."
        ),
        new RngSpot(
            FunfairHotdogCondimentStart, "Hotdog", "Hotdog Condiment Type", RngShape.StartPool, false, false,
            mode => FunfairHotdogCondimentOptions(),
            "The type of the hotdog's first condiment."
        ),
        new RngSpot(
            FunfairHotdogSideStart, "Hotdog", "Hotdog Condiment Side", RngShape.StartPool, false, false,
            mode => FunfairHotdogSideOptions(),
            "The side the hotdog's first condiment fires from."
        ),
        new RngSpot(
            FunfairHotdogDelayStart, "Hotdog", "Hotdog Condiment Delay", RngShape.StartPool, false, false,
            mode => FunfairHotdogDelayOptions(),
            "The starting entry of the hotdog's condiment delay cycle (beats between condiments)."
        )
    };

    public static readonly RngSpot[] FlyingBird = {
        new RngSpot(
            FlyingBirdMainPatternStart, "Phase 1", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FlyingBirdPatternOptions(mode, LevelProperties.FlyingBird.States.Main),
            "Which attack the bird's fixed cycle begins with; the cycle then plays in order for the rest of the phase."
        ),
        new RngSpot(
            FlyingBirdFloatDirection, "Phase 1", "Float Direction", RngShape.Choice, false, false,
            mode => Fixed("Up", "Down"),
            "The direction the bird drifts in first, rolled once when the fight starts; it then alternates up and down for the whole fight."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            FlyingBirdGenericPatternStart, "Phase 2", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FlyingBirdPatternOptions(mode, LevelProperties.FlyingBird.States.Generic),
            "Which attack this phase's fixed cycle begins with (from 90% boss health on Regular, 85% on Simple)."
        ) { Available = mode => mode != Level.Mode.Hard },
        new RngSpot(
            FlyingBirdBirdDirection, "Small Bird", "Small Bird Direction", RngShape.Choice, false, false,
            mode => Fixed("Up", "Down"),
            "The direction the Small Bird starts flying in at the start of phase 3."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            FlyingBirdRevivalPatternStart, "Final Phase", "Overall Pattern", RngShape.StartPool, true, true,
            mode => FlyingBirdPatternOptions(mode, LevelProperties.FlyingBird.States.BirdRevival),
            "Whether the final phase opens with the garbage volley or the heart attack; whichever it opens with, the two then alternate."
        ) { Available = mode => mode != Level.Mode.Easy },
        new RngSpot(
            FlyingBirdStretcherDirection, "Final Phase", "Stretcher Direction", RngShape.Choice, false, false,
            mode => Fixed("Right", "Left"),
            "The direction Wally's stretcher first sweeps in; it then alternates left and right."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true, Available = mode => mode != Level.Mode.Easy }
    };

    public static readonly RngSpot[] FlyingGenie = {
        new RngSpot(
            FlyingGenieTreasureAttack, "Phase 1", "Treasure Chest Attack", RngShape.Choice, true, false,
            mode => Fixed("Swords", "Gems", "Sphinx"),
            "The attack the treasure chest uses. Rolled once when the chest opens, then the attack loops for the whole chest phase."
        ) { PerEvent = true, Badge = RngBadgeKind.Once, OncePerFight = true },
        new RngSpot(
            FlyingGenieSwordPinkStart, "Phase 1", "Sword Parry Pattern", RngShape.StartPool, true, false,
            mode => FlyingGeniePinkOptions(mode, genie => genie.swords.swordPinkString),
            "The starting position inside the sword parry cycle; this picks whether the first sword is parryable, and the cycle then walks from there."
        ),
        new RngSpot(
            FlyingGenieGemPinkStart, "Phase 1", "Gem Parry Pattern", RngShape.StartPool, true, false,
            mode => FlyingGeniePinkOptions(mode, genie => genie.gems.gemPinkString),
            "The starting position inside the gem parry cycle; this picks whether the first gem is parryable, and the cycle then walks from there."
        ),
        new RngSpot(
            FlyingGenieSphinxPinkStart, "Phase 1", "Sphinx Parry Pattern", RngShape.StartPool, true, false,
            mode => FlyingGeniePinkOptions(mode, genie => genie.sphinx.scarabPinkString),
            "The starting position inside the scarab parry cycle; this picks whether the first scarab is parryable, and the cycle then walks from there."
        ),
        new RngSpot(
            FlyingGenieObeliskPatternStart, "Phase 2", "Obelisk Pattern", RngShape.StartPool, true, false,
            mode => FlyingGenieObeliskOptions(mode),
            "Where the obelisk cycle begins. Each option shows which genie heads rise on the first pillar (a dash means both rise together); the cycle then walks the entries in order, wrapping into the next row when one runs out."
        )
    };

    public static readonly RngSpot[] Clown = {
        new RngSpot(
            ClownBumperDelay, "Phase 1", "Bumper Dash Delay", RngShape.StartPool, true, false,
            mode => ClownBumperDelayOptions(mode),
            "The delay before the bumper car's next dash, tagged in beats; the car rolls where this delay cycle begins once per fight and then walks it dash by dash."
        ),
        new RngSpot(
            ClownBumperCarDriftDirection, "Phase 1", "Bumper Car Drift Direction", RngShape.StartPool, true, false,
            mode => ClownDriftOptions(mode),
            "The way the car drifts between dashes. Rolled once per fight and then walked; the drift direction inverts every time the car dashes, because it ends up facing the other way."
        ),
        new RngSpot(
            ClownDuckTypePatternString, "Phase 1", "Duck Type Pattern", RngShape.Choice, true, false,
            mode => ClownDuckPatternOptions(mode),
            "The duck type sequence the bumper car phase spawns (R = regular, B = bomb, P = pink parryable); the game picks one of these rows per fight, and the start below chooses where it begins."
        ) { CompoundWith = ClownDuckTypePattern, Badge = RngBadgeKind.Order,
            CompoundStartOptions = (mode, patternIndex) => ClownDuckStartOptions(mode, patternIndex),
            CompoundPrimaryLabel = "Pattern", CompoundSecondaryLabel = "Start at",
            UiEasy = RngUi.Radio },
        new RngSpot(
            ClownDuckTypePattern, "Phase 1", "Duck Type", RngShape.Choice, true, false,
            mode => ClownDuckTypeOptions(mode),
            null
        ) { CompoundSecondary = true, UiEasy = RngUi.Radio },
        new RngSpot(
            ClownHorseType, "Phase 3", "Horse Type", RngShape.StartPool, true, false,
            mode => ClownHorseTypeOptions(mode),
            "The carousel horse's attack cycle (green horses throw horseshoes in waves, yellow ones drop them from above). The cycle start is rolled once when the horse appears and then walked attack by attack."
        ),
        new RngSpot(
            ClownHorseSide, "Phase 3", "Horse Side", RngShape.Choice, true, false,
            mode => Fixed("Left", "Right"),
            "The side the carousel horse enters from, re-rolled on every attack."
        ) { PerEvent = true }
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
        all.AddRange(Flower);
        all.AddRange(Baroness);
        all.AddRange(FunfairFever);
        all.AddRange(FlyingBird);
        all.AddRange(FlyingGenie);
        all.AddRange(Clown);

        // per-difficulty settings become one spot per difficulty, each with its own UI
        List<RngSpot> expanded = new List<RngSpot>();
        foreach (RngSpot spot in all) {
            if (spot.PerDifficulty) {
                expanded.Add(new RngSpot(spot, Level.Mode.Easy));
                expanded.Add(new RngSpot(spot, Level.Mode.Normal));
                expanded.Add(new RngSpot(spot, Level.Mode.Hard));
            } else {
                expanded.Add(spot);
            }
        }
        return expanded.ToArray();
    }

    private static readonly Dictionary<string, List<RngSpot>> SpotIndex = BuildIndex();

    /// <summary>The spot definition for a difficulty: per-difficulty settings resolve to
    /// their own variant, shared settings to their single definition.</summary>
    public static RngSpot FindForMode(string id, Level.Mode mode) {
        List<RngSpot> variants;
        if (!SpotIndex.TryGetValue(id, out variants)) {
            return null;
        }
        foreach (RngSpot spot in variants) {
            if (!spot.OnlyFor.HasValue || spot.OnlyFor.Value == mode) {
                return spot;
            }
        }
        return null;
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
        { "ForestFollies", "Forest Follies" },
        { "Flower", "Cagney Carnation" },
        { "Baroness", "Baroness Von Bon Bon" },
        { "FunfairFever", "Funfair Fever" },
        { "FlyingBird", "Wally Warbles" },
        { "FlyingGenie", "Djimmi The Great" },
        { "Clown", "Beppi The Clown" }
    };

    public static string StageLabel(string stageId) {
        string label;
        return StageLabels.TryGetValue(stageId, out label) ? label : stageId;
    }

    /// <summary>Looks a spot up by a stored config/form key, which may carry a "@difficulty" suffix.</summary>
    public static RngSpot FindByStoredKey(string key) {
        int at = key.LastIndexOf('@');
        if (at < 0) {
            return FindForMode(key, Level.Mode.Normal);
        }
        string id = key.Substring(0, at);
        string modeKey = key.Substring(at + 1);
        Level.Mode mode = modeKey == "easy" ? Level.Mode.Easy
            : modeKey == "hard" ? Level.Mode.Hard
            : Level.Mode.Normal;
        return FindForMode(id, mode);
    }

    private static Dictionary<string, List<RngSpot>> BuildIndex() {
        Dictionary<string, List<RngSpot>> index = new Dictionary<string, List<RngSpot>>(StringComparer.Ordinal);
        foreach (RngSpot spot in All) {
            List<RngSpot> variants;
            if (!index.TryGetValue(spot.Id, out variants)) {
                variants = new List<RngSpot>();
                index[spot.Id] = variants;
            }
            variants.Add(spot);
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

    internal static FieldInfo FindField(Type type, string name) {
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

    // ------------------------------------------------------------------
    // Funfair Fever option enumeration. The cannon and hotdog pattern
    // strings are serialized in the scene, so they are read from the live
    // instances when the menu is opened in-level; the fallbacks are the
    // shipped values.
    // ------------------------------------------------------------------

    private const string FunfairCannonPinkFallback = "O,G,P,O,P,G,P,P";
    private const string FunfairHotdogSpawnFallback = "0,1,3,6,7,5,0,4,2,6,1,5,3,0,7,2,4,0,5,2,6,0,3,7,1,6";
    private const string FunfairHotdogCondimentFallback = "R,M,K,M,K,R,M";
    private const string FunfairHotdogSideFallback = "L,R,L,L,R,L,R,R";
    private const string FunfairHotdogDelayFallback = "3,4,5,3,4,6,3,5,3,3,4,5,3,6,4";

    /// <summary>One option per entry of the cannon's color cycle.</summary>
    private static RngOption[] FunfairCannonColorOptions() {
        string[] entries = FunfairCannonPinkString().Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            string color = entries[i].Trim();
            options[i] = new RngOption(color == "P" ? "Pink" : color == "G" ? "Green" : "Orange", i);
        }
        return options;
    }

    /// <summary>One option per muzzle of the cannon. The muzzle heights are scene data (the game
    /// code only walks the shootRoots array); the legacy mod's scene notes have index 0 at the
    /// top and the last index at the bottom.</summary>
    private static RngOption[] FunfairCannonMuzzleOptions() {
        int count = 3;
        try {
            CircusPlatformingLevelCannon cannon = UnityEngine.Object.FindObjectOfType<CircusPlatformingLevelCannon>();
            if (cannon != null && cannon.shootRoots != null && cannon.shootRoots.Length > 0) {
                count = cannon.shootRoots.Length;
            }
        } catch (Exception) {
        }
        string[] labels = { "Top", "Middle", "Bottom" };
        if (count <= labels.Length) {
            RngOption[] options = new RngOption[count];
            for (int i = 0; i < count; i++) {
                options[i] = new RngOption(labels[i], i);
            }
            return options;
        }
        return Numbered(1, count);
    }

    /// <summary>One option per entry of the hotdog's spawn-point cycle.</summary>
    private static RngOption[] FunfairHotdogSpawnOptions() {
        string[] entries = FunfairHotdogLiveString(h => h.spawnPatternString, FunfairHotdogSpawnFallback).Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            options[i] = new RngOption(entries[i].Trim(), i);
        }
        return options;
    }

    /// <summary>One option per entry of the hotdog's condiment cycle.</summary>
    private static RngOption[] FunfairHotdogCondimentOptions() {
        string[] entries = FunfairHotdogLiveString(h => h.condimentPatternString, FunfairHotdogCondimentFallback).Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            string c = entries[i].Trim();
            options[i] = new RngOption(c == "R" ? "Relish" : c == "M" ? "Mustard" : "Ketchup", i);
        }
        return options;
    }

    /// <summary>One option per entry of the hotdog's left/right cycle.</summary>
    private static RngOption[] FunfairHotdogSideOptions() {
        string[] entries = FunfairHotdogLiveString(h => h.sidePatternString, FunfairHotdogSideFallback).Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            options[i] = new RngOption(entries[i].Trim() == "L" ? "Left" : "Right", i);
        }
        return options;
    }

    /// <summary>One option per entry of the hotdog's shot-delay cycle.</summary>
    private static RngOption[] FunfairHotdogDelayOptions() {
        string[] entries = FunfairHotdogLiveString(h => h.shotDelayPatternString, FunfairHotdogDelayFallback).Split(new[] { ',' });
        RngOption[] options = new RngOption[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            options[i] = new RngOption(entries[i].Trim(), i);
        }
        return options;
    }

    private static string FunfairCannonPinkString() {
        try {
            CircusPlatformingLevelCannon cannon = UnityEngine.Object.FindObjectOfType<CircusPlatformingLevelCannon>();
            if (cannon != null && !string.IsNullOrEmpty(cannon.pinkString)) {
                return cannon.pinkString;
            }
        } catch (Exception) {
        }
        return FunfairCannonPinkFallback;
    }

    private static string FunfairHotdogLiveString(Func<CircusPlatformingLevelHotdog, string> field, string fallback) {
        try {
            CircusPlatformingLevelHotdog hotdog = UnityEngine.Object.FindObjectOfType<CircusPlatformingLevelHotdog>();
            if (hotdog != null) {
                string value = field(hotdog);
                if (!string.IsNullOrEmpty(value)) {
                    return value;
                }
            }
        } catch (Exception) {
        }
        return fallback;
    }

    // ------------------------------------------------------------------
    // Wally Warbles option enumeration. The per-difficulty state data comes
    // from LevelProperties.FlyingBird.GetMode(mode); states are matched by
    // name because the state lists differ per difficulty (Expert has no
    // Generic phase, Simple has no BirdRevival).
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.FlyingBird.State[]> FlyingBirdStatesCache = new Dictionary<Level.Mode, LevelProperties.FlyingBird.State[]>();

    private static LevelProperties.FlyingBird.State[] FlyingBirdStatesFor(Level.Mode mode) {
        LevelProperties.FlyingBird.State[] states;
        if (FlyingBirdStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.FlyingBird properties = LevelProperties.FlyingBird.GetMode(mode);
            states = (LevelProperties.FlyingBird.State[])FindField(typeof(LevelProperties.FlyingBird), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.FlyingBird.State[0];
        }

        FlyingBirdStatesCache[mode] = states;
        return states;
    }

    private static LevelProperties.FlyingBird.State FlyingBirdStateFor(Level.Mode mode, LevelProperties.FlyingBird.States stateName) {
        foreach (LevelProperties.FlyingBird.State state in FlyingBirdStatesFor(mode)) {
            if (state.stateName == stateName) {
                return state;
            }
        }
        return null;
    }

    /// <summary>One option per entry of the state's fixed attack cycle.</summary>
    private static RngOption[] FlyingBirdPatternOptions(Level.Mode mode, LevelProperties.FlyingBird.States stateName) {
        try {
            LevelProperties.FlyingBird.State state = FlyingBirdStateFor(mode, stateName);
            if (state == null) {
                return new RngOption[0];
            }
            RngOption[] options = new RngOption[state.patterns.Length];
            for (int i = 0; i < state.patterns.Length; i++) {
                options[i] = new RngOption(FlyingBirdPatternLabel(state.patterns[i]), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static string FlyingBirdPatternLabel(LevelProperties.FlyingBird.Pattern pattern) {
        switch (pattern) {
            case LevelProperties.FlyingBird.Pattern.Eggs:
                return "Eggs";
            case LevelProperties.FlyingBird.Pattern.Lasers:
                return "Lasers";
            case LevelProperties.FlyingBird.Pattern.Garbage:
                return "Garbage";
            case LevelProperties.FlyingBird.Pattern.Heart:
                return "Heart";
            case LevelProperties.FlyingBird.Pattern.Feathers:
                return "Feathers";
            default:
                return pattern.ToString();
        }
    }

    // ------------------------------------------------------------------
    // Djimmi The Great option enumeration. The per-difficulty state data
    // comes from LevelProperties.FlyingGenie.GetMode(mode); the pink strings
    // are read from the Main state, whose copy the genie keeps for the whole
    // fight.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.FlyingGenie.State[]> FlyingGenieStatesCache = new Dictionary<Level.Mode, LevelProperties.FlyingGenie.State[]>();

    private static LevelProperties.FlyingGenie.State[] FlyingGenieStatesFor(Level.Mode mode) {
        LevelProperties.FlyingGenie.State[] states;
        if (FlyingGenieStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.FlyingGenie properties = LevelProperties.FlyingGenie.GetMode(mode);
            states = (LevelProperties.FlyingGenie.State[])FindField(typeof(LevelProperties.FlyingGenie), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.FlyingGenie.State[0];
        }

        FlyingGenieStatesCache[mode] = states;
        return states;
    }

    /// <summary>One option per entry of a parry cycle ("Parry" / "Regular").</summary>
    private static RngOption[] FlyingGeniePinkOptions(Level.Mode mode, Func<LevelProperties.FlyingGenie.State, string> pinkString) {
        try {
            LevelProperties.FlyingGenie.State[] states = FlyingGenieStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] entries = pinkString(states[0]).Split(new[] { ',' });
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(entries[i].Trim() == "P" ? "Parry" : "Regular", i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per entry of the obelisk head-order rows, in the game's own
    /// row-major walk order (3 rows of 9 in all versions - verified).</summary>
    private static RngOption[] FlyingGenieObeliskOptions(Level.Mode mode) {
        try {
            LevelProperties.FlyingGenie.State[] states = FlyingGenieStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] rows = states[0].obelisk.obeliskGeniePos;
            List<RngOption> options = new List<RngOption>();
            foreach (string row in rows) {
                foreach (string entry in row.Split(new[] { ',' })) {
                    options.Add(new RngOption(entry.Trim(), options.Count));
                }
            }
            return options.ToArray();
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    // ------------------------------------------------------------------
    // Beppi The Clown option enumeration. The per-difficulty state data
    // comes from LevelProperties.Clown.GetMode(mode); the bumper delay tags
    // are the legacy beat annotations, kept positionally.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Clown.State[]> ClownStatesCache = new Dictionary<Level.Mode, LevelProperties.Clown.State[]>();

    private static LevelProperties.Clown.State[] ClownStatesFor(Level.Mode mode) {
        LevelProperties.Clown.State[] states;
        if (ClownStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Clown properties = LevelProperties.Clown.GetMode(mode);
            states = (LevelProperties.Clown.State[])FindField(typeof(LevelProperties.Clown), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Clown.State[0];
        }

        ClownStatesCache[mode] = states;
        return states;
    }

    private const string ClownDriftFallback = "F,F,B,B,F,B,F,B,B,F,B,F,F,B,F,B";

    private static readonly Dictionary<Level.Mode, string[]> ClownBumperDelayTags = new Dictionary<Level.Mode, string[]> {
        { Level.Mode.Easy, new[] { "5B", "3B", "5B", "4B+", "2B", "4B", "4B+", "3B", "2B" } },
        { Level.Mode.Normal, new[] { "5B", "3B++", "5B", "4B+", "2B+", "5B", "4B+", "5B+", "3B", "2B+" } },
        { Level.Mode.Hard, new[] { "5B", "5B", "3B", "5B", "4B+", "2B", "5B", "3B", "5B+", "3B", "2B" } }
    };

    /// <summary>One option per entry of the car's dash delay cycle, tagged in beats.</summary>
    private static RngOption[] ClownBumperDelayOptions(Level.Mode mode) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] entries = states[0].bumperCar.attackDelayString[0].Split(new[] { ',' });
            string[] tags;
            ClownBumperDelayTags.TryGetValue(mode, out tags);
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(tags != null && i < tags.Length ? tags[i] : entries[i].Trim(), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per entry of the car's drift cycle ("Forward" / "Backward").</summary>
    private static RngOption[] ClownDriftOptions(Level.Mode mode) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            string row = states.Length >= 1 ? states[0].bumperCar.movementStrings[0] : ClownDriftFallback;
            string[] entries = row.Split(new[] { ',' });
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(entries[i].Trim() == "F" ? "Forward" : "Backward", i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per duck type row of the bumper car phase.</summary>
    private static RngOption[] ClownDuckPatternOptions(Level.Mode mode) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] rows = states[0].duck.duckTypeString;
            RngOption[] options = new RngOption[rows.Length];
            for (int i = 0; i < rows.Length; i++) {
                options[i] = new RngOption("Set " + (i + 1), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per entry of the duck type cycle, labelled by duck type.</summary>
    private static RngOption[] ClownDuckTypeOptions(Level.Mode mode) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] entries = states[0].duck.duckTypeString[0].Split(new[] { ',' });
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(ClownDuckTypeLabel(entries[i]), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per entry of the CHOSEN duck type row, for the compound card's
    /// start list; empty until a set is picked.</summary>
    private static RngOption[] ClownDuckStartOptions(Level.Mode mode, int patternIndex) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            if (states.Length < 1 || patternIndex < 0) {
                return new RngOption[0];
            }
            string[] rows = states[0].duck.duckTypeString;
            if (patternIndex >= rows.Length) {
                return new RngOption[0];
            }
            string[] entries = rows[patternIndex].Split(new[] { ',' });
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(ClownDuckTypeLabel(entries[i]), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    private static string ClownDuckTypeLabel(string entry) {
        string c = entry.Trim();
        return c == "P" ? "Parry" : c == "B" ? "Bomb" : "Regular";
    }

    /// <summary>One option per entry of the horse's wave/drop attack cycle.</summary>
    private static RngOption[] ClownHorseTypeOptions(Level.Mode mode) {
        try {
            LevelProperties.Clown.State[] states = ClownStatesFor(mode);
            if (states.Length < 1) {
                return new RngOption[0];
            }
            string[] entries = states[0].horse.HorseString[0].Split(new[] { ',' });
            RngOption[] options = new RngOption[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                options[i] = new RngOption(entries[i].Trim() == "W" ? "Green" : "Yellow", i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    // ------------------------------------------------------------------
    // Cagney Carnation option enumeration. The per-difficulty state data
    // comes from LevelProperties.Flower.GetMode(mode); the three phase
    // cycles sit at state indices 0-2 (Phase 1-3).
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Flower.State[]> FlowerStatesCache = new Dictionary<Level.Mode, LevelProperties.Flower.State[]>();

    private static LevelProperties.Flower.State[] FlowerStatesFor(Level.Mode mode) {
        LevelProperties.Flower.State[] states;
        if (FlowerStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Flower properties = LevelProperties.Flower.GetMode(mode);
            states = (LevelProperties.Flower.State[])FindField(typeof(LevelProperties.Flower), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Flower.State[0];
        }

        FlowerStatesCache[mode] = states;
        return states;
    }

    private static string FlowerPatternLabel(LevelProperties.Flower.Pattern pattern) {
        switch (pattern) {
            case LevelProperties.Flower.Pattern.Laser:
                return "Head Lunge";
            case LevelProperties.Flower.Pattern.PodHands:
                return "Pod Hands";
            case LevelProperties.Flower.Pattern.GattlingGun:
                return "Gatling Gun";
            case LevelProperties.Flower.Pattern.VineHands:
                return "Vine Hands";
            default:
                return pattern.ToString();
        }
    }

    /// <summary>One option per entry of the phase's fixed attack cycle (0 = Phase 1,
    /// 1 = Phase 2, 2 = Phase 3).</summary>
    private static RngOption[] FlowerPatternOptions(Level.Mode mode, int stateIndex) {
        try {
            LevelProperties.Flower.State[] states = FlowerStatesFor(mode);
            if (stateIndex < 0 || stateIndex >= states.Length) {
                return new RngOption[0];
            }
            LevelProperties.Flower.Pattern[] patterns = states[stateIndex].patterns;
            RngOption[] options = new RngOption[patterns.Length];
            for (int i = 0; i < patterns.Length; i++) {
                options[i] = new RngOption(FlowerPatternLabel(patterns[i]), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One option per entry of the head-lunge top/bottom order.</summary>
    private static RngOption[] FlowerLaserOptions(Level.Mode mode) {
        try {
            string[] entries = FlowerStatesFor(mode)[0].laser.attackType.Split(new[] { ',' });
            string[] labels = new string[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                labels[i] = entries[i].Trim() == "T" ? "Top" : "Bottom";
            }
            return NamedOptions(labels);
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>The states whose attack cycle contains pod hands, in fight order (Regular
    /// runs two - Phases 2 and 3; Simple and Expert one each). Their per-state pod-hands
    /// data decides what each cycle index produces.</summary>
    private static List<LevelProperties.Flower.State> FlowerPodHandsStates(Level.Mode mode) {
        List<LevelProperties.Flower.State> result = new List<LevelProperties.Flower.State>();
        try {
            foreach (LevelProperties.Flower.State state in FlowerStatesFor(mode)) {
                if (Array.IndexOf(state.patterns, LevelProperties.Flower.Pattern.PodHands) >= 0) {
                    result.Add(state);
                }
            }
        } catch (Exception) {
        }
        return result;
    }

    private static string FlowerPodTypeText(string entry) {
        return entry.Trim() == "S" ? "Carrots" : "Boomerang";
    }

    /// <summary>One row per entry of the pod-hands type cycle: the attack type each index
    /// produces in every pod-hands phase (Regular shows two phases). Rows the game's start
    /// roll can never land on (non-carrots entries, and entries past the roll domain) are
    /// display-only; the radio list renders them grayed out.</summary>
    private static RngOption[] FlowerPodTypeOptions(Level.Mode mode) {
        try {
            LevelProperties.Flower.State[] states = FlowerStatesFor(mode);
            List<LevelProperties.Flower.State> phases = FlowerPodHandsStates(mode);
            if (states == null || states.Length == 0 || phases.Count == 0) {
                return new RngOption[0];
            }
            string[] rollDomain = states[0].podHands.attacktype.Split(new[] { ',' });

            string[][] columns = new string[phases.Count][];
            int rows = rollDomain.Length;
            for (int c = 0; c < phases.Count; c++) {
                columns[c] = phases[c].podHands.attacktype.Split(new[] { ',' });
                rows = Math.Max(rows, columns[c].Length);
            }

            RngOption[] options = new RngOption[rows];
            for (int i = 0; i < rows; i++) {
                string[] parts = new string[phases.Count];
                for (int c = 0; c < phases.Count; c++) {
                    parts[c] = i < columns[c].Length ? FlowerPodTypeText(columns[c][i]) : "-";
                }
                bool selectable = i < rollDomain.Length && rollDomain[i].Trim() == "S";
                options[i] = new RngOption(parts, i, selectable);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    /// <summary>One row per entry of the pod-hands volley-count cycle: the count that
    /// index produces in every pod-hands phase. Every start is reachable, so all rows are
    /// selectable; counts past a phase's cycle length wrap to its first entry (the game's
    /// own out-of-bounds guard restarts the index at 0). Rendered as one plain label -
    /// single digits have the same width in any font.</summary>
    private static RngOption[] FlowerPodCountOptions(Level.Mode mode) {
        try {
            List<LevelProperties.Flower.State> phases = FlowerPodHandsStates(mode);
            if (phases.Count == 0) {
                return new RngOption[0];
            }

            string[][] columns = new string[phases.Count][];
            int rows = 0;
            for (int c = 0; c < phases.Count; c++) {
                columns[c] = phases[c].podHands.attackAmount.Split(new[] { ',' });
                rows = Math.Max(rows, columns[c].Length);
            }

            RngOption[] options = new RngOption[rows];
            for (int i = 0; i < rows; i++) {
                string[] parts = new string[phases.Count];
                for (int c = 0; c < phases.Count; c++) {
                    string text;
                    if (i < columns[c].Length) {
                        text = columns[c][i].Trim();
                    } else {
                        text = columns[c].Length > 0 ? columns[c][0].Trim() + " (wrap)" : "-";
                    }
                    parts[c] = text;
                }
                options[i] = new RngOption(string.Join(" / ", parts), i);
            }
            return options;
        } catch (Exception) {
        }
        return new RngOption[0];
    }

    // ------------------------------------------------------------------
    // Baroness Von Bon Bon option enumeration. The miniboss roster comes
    // from the difficulty's own miniBossString pool (Simple has no
    // cupcake); the option values index into that roster.
    // ------------------------------------------------------------------

    private static readonly Dictionary<Level.Mode, LevelProperties.Baroness.State[]> BaronessStatesCache = new Dictionary<Level.Mode, LevelProperties.Baroness.State[]>();

    private static LevelProperties.Baroness.State[] BaronessStatesFor(Level.Mode mode) {
        LevelProperties.Baroness.State[] states;
        if (BaronessStatesCache.TryGetValue(mode, out states)) {
            return states;
        }

        try {
            LevelProperties.Baroness properties = LevelProperties.Baroness.GetMode(mode);
            states = (LevelProperties.Baroness.State[])FindField(typeof(LevelProperties.Baroness), "states").GetValue(properties);
        } catch (Exception) {
            states = new LevelProperties.Baroness.State[0];
        }

        BaronessStatesCache[mode] = states;
        return states;
    }

    private static string BaronessMinibossLabel(string id) {
        switch (id) {
            case "1":
                return "Gumball";
            case "2":
                return "Waffle";
            case "3":
                return "Candy Corn";
            case "4":
                return "Cupcake";
            case "5":
                return "Jawbreaker";
            default:
                return "Miniboss " + id;
        }
    }

    /// <summary>The miniboss ids ("1".."5") this difficulty's pick pool can contain,
    /// sorted ascending - the roster of the miniboss option lists.</summary>
    public static string[] BaronessMinibossRoster(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            if (states == null || states.Length == 0) {
                return new string[0];
            }
            List<int> ids = new List<int>();
            foreach (string ordering in states[0].open.miniBossString) {
                foreach (string part in ordering.Split(new[] { ',' })) {
                    int id;
                    if (int.TryParse(part.Trim(), out id) && !ids.Contains(id)) {
                        ids.Add(id);
                    }
                }
            }
            ids.Sort();
            string[] roster = new string[ids.Count];
            for (int i = 0; i < ids.Count; i++) {
                roster[i] = ids[i].ToString();
            }
            return roster;
        } catch (Exception) {
        }
        return new string[0];
    }

    private static RngOption[] BaronessMinibossOptions(Level.Mode mode) {
        string[] roster = BaronessMinibossRoster(mode);
        string[] labels = new string[roster.Length];
        for (int i = 0; i < roster.Length; i++) {
            labels[i] = BaronessMinibossLabel(roster[i]);
        }
        return NamedOptions(labels);
    }

    /// <summary>The live castle data of the fight (pop-up timings, toss script); null when
    /// unavailable.</summary>
    private static LevelProperties.Baroness.BaronessVonBonbon BaronessMainData(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            return states != null && states.Length > 0 ? states[0].baronessVonBonbon : null;
        } catch (Exception) {
        }
        return null;
    }

    /// <summary>The pop-up timing patterns of the live data.</summary>
    public static string[] BaronessShotgunPatterns(Level.Mode mode) {
        try {
            LevelProperties.Baroness.BaronessVonBonbon data = BaronessMainData(mode);
            return data != null && data.timeString != null ? data.timeString : new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    private static RngOption[] BaronessShotgunPatternOptions(Level.Mode mode) {
        return SetOptions(BaronessShotgunPatterns(mode).Length);
    }

    /// <summary>The start options of one pop-up timing pattern: its delay entries
    /// (seconds until each pop-up).</summary>
    public static RngOption[] BaronessShotgunStartOptions(Level.Mode mode, int patternIndex) {
        string[] patterns = BaronessShotgunPatterns(mode);
        if (patternIndex < 0 || patternIndex >= patterns.Length) {
            return new RngOption[0];
        }
        string[] entries = patterns[patternIndex].Split(new[] { ',' });
        string[] labels = new string[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            labels[i] = entries[i].Trim();
        }
        return NamedOptions(labels);
    }

    /// <summary>Entry count of one pop-up timing pattern.</summary>
    public static int BaronessShotgunStartCount(Level.Mode mode, int patternIndex) {
        string[] patterns = BaronessShotgunPatterns(mode);
        if (patternIndex < 0 || patternIndex >= patterns.Length) {
            return 0;
        }
        return patterns[patternIndex].Split(new[] { ',' }).Length;
    }

    private static RngFloatRange BaronessShotgunDelayRange(Level.Mode mode) {
        try {
            LevelProperties.Baroness.BaronessVonBonbon data = BaronessMainData(mode);
            if (data != null) {
                MinMax range = data.attackDelay;
                return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) / 2f);
            }
        } catch (Exception) {
        }
        return new RngFloatRange(4f, 6.5f, 0.05f, 5.25f);
    }

    /// <summary>The head-toss script entries ("H" or "D:x") of the live data.</summary>
    public static string[] BaronessHeadTossEntries(Level.Mode mode) {
        try {
            LevelProperties.Baroness.BaronessVonBonbon data = BaronessMainData(mode);
            return data != null && !string.IsNullOrEmpty(data.finalProjectileHeadToss)
                ? data.finalProjectileHeadToss.Split(new[] { ',' })
                : new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    private static RngOption[] BaronessHeadTossOptions(Level.Mode mode) {
        string[] entries = BaronessHeadTossEntries(mode);
        string[] labels = new string[entries.Length];
        for (int i = 0; i < entries.Length; i++) {
            string entry = entries[i].Trim();
            labels[i] = entry == "H" ? "Head Toss" : entry.Length > 2 ? "Wait " + entry.Substring(2) : entry;
        }
        return NamedOptions(labels);
    }

    private static RngFloatRange BaronessHeadTossDelayRange(Level.Mode mode) {
        try {
            LevelProperties.Baroness.BaronessVonBonbon data = BaronessMainData(mode);
            if (data != null) {
                MinMax range = data.finalProjectileAttackDelayRange;
                return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) / 2f);
            }
        } catch (Exception) {
        }
        return new RngFloatRange(7.5f, 8.1f, 0.05f, 7.8f);
    }

    /// <summary>The live candy corn data of the fight (turn patterns, mini corn timing).</summary>
    private static LevelProperties.Baroness.CandyCorn BaronessCornData(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            return states != null && states.Length > 0 ? states[0].candyCorn : null;
        } catch (Exception) {
        }
        return null;
    }

    /// <summary>The live turn patterns of the candy corn (Y = turn up, N = keep walking).</summary>
    public static string[] BaronessCornTurnPatterns(Level.Mode mode) {
        try {
            LevelProperties.Baroness.CandyCorn corn = BaronessCornData(mode);
            return corn != null && corn.changeLevelString != null ? corn.changeLevelString : new string[0];
        } catch (Exception) {
        }
        return new string[0];
    }

    private static RngOption[] BaronessCornTurnPatternOptions(Level.Mode mode) {
        return NamedOptions(BaronessCornTurnPatterns(mode));
    }

    /// <summary>The longest turn pattern of the difficulty - the start index range.</summary>
    public static int BaronessCornTurnMaxIndex(Level.Mode mode) {
        int max = 0;
        foreach (string pattern in BaronessCornTurnPatterns(mode)) {
            max = Math.Max(max, pattern.Split(new[] { ',' }).Length);
        }
        return max;
    }

    /// <summary>Numeric start labels on Simple/Expert (label N = the Nth entry; the
    /// resolver writes the 0-based index). On Regular - where only one turn pattern
    /// exists - each index is labeled by what the corn does there instead.</summary>
    private static RngOption[] BaronessCornTurnStartOptions(Level.Mode mode) {
        string[] patterns = BaronessCornTurnPatterns(mode);
        if (patterns.Length < 2) {
            if (patterns.Length == 1) {
                string[] entries = patterns[0].Split(new[] { ',' });
                string[] labels = new string[entries.Length];
                for (int i = 0; i < entries.Length; i++) {
                    labels[i] = entries[i].Trim() == "Y" ? "Turn" : "Straight";
                }
                return NamedOptions(labels);
            }
            return new RngOption[0];
        }
        return Numbered(1, BaronessCornTurnMaxIndex(mode));
    }

    private static RngFloatRange BaronessCornTurnMiniDelayRange(Level.Mode mode) {
        try {
            LevelProperties.Baroness.CandyCorn corn = BaronessCornData(mode);
            if (corn != null) {
                MinMax range = corn.miniCornSpawnDelay;
                return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) / 2f);
            }
        } catch (Exception) {
        }
        return new RngFloatRange(1.5f, 2.3f, 0.05f, 1.9f);
    }

    /// <summary>The live jellybean spawn patterns of the fight (R = regular, P = parryable).</summary>
    public static string[] BaronessJellybeanPatterns(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            if (states != null && states.Length > 0 && states[0].jellybeans != null && states[0].jellybeans.typeArray != null) {
                return states[0].jellybeans.typeArray;
            }
        } catch (Exception) {
        }
        return new string[0];
    }

    private static RngOption[] BaronessJellybeanPatternOptions(Level.Mode mode) {
        return NamedOptions(BaronessJellybeanPatterns(mode));
    }

    private static RngFloatRange BaronessJellybeanDelayRange(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            if (states != null && states.Length > 0 && states[0].jellybeans != null) {
                MinMax range = states[0].jellybeans.spawnDelay;
                return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) / 2f);
            }
        } catch (Exception) {
        }
        return new RngFloatRange(4.8f, 6.6f, 0.05f, 5.7f);
    }

    /// <summary>The live waffle data of the fight (split timing).</summary>
    private static LevelProperties.Baroness.Waffle BaronessWaffleData(Level.Mode mode) {
        try {
            LevelProperties.Baroness.State[] states = BaronessStatesFor(mode);
            return states != null && states.Length > 0 ? states[0].waffle : null;
        } catch (Exception) {
        }
        return null;
    }

    private static RngFloatRange BaronessWaffleSplitDelayRange(Level.Mode mode) {
        try {
            LevelProperties.Baroness.Waffle waffle = BaronessWaffleData(mode);
            if (waffle != null) {
                MinMax range = waffle.attackDelayRange;
                return new RngFloatRange(range.min, range.max, 0.05f, (range.min + range.max) / 2f);
            }
        } catch (Exception) {
        }
        return new RngFloatRange(2f, 5f, 0.05f, 3.5f);
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
