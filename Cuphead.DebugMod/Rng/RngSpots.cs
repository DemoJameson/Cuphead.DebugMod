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
    Order
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

    public static readonly RngSpot[] Devil = {
        new RngSpot(
            DevilPhase1PatternStart, "Phase 1", "Attack Order Start", RngShape.StartPool, true, true,
            mode => DevilStatePatterns(mode, LevelProperties.Devil.States.Main),
            "Which attack the fight's fixed cycle begins with. The game rolls a start position, then repeats the cycle in order."
        ),
        new RngSpot(
            DevilPhase1HeadFirst, "Phase 1", "First Head Attack", RngShape.StartPool, false, false,
            mode => Fixed("Dragon first", "Spider first"),
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
        ) { CompoundWith = DevilPhase1PitchforkType, Badge = RngBadgeKind.Order },
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
            DevilPhase1BouncerAngleStart, "Phase 1", "Bouncer Angle Start", RngShape.StartPool, true, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkFourFlameBouncer.angleOffset),
            "The bouncer wave cycles through fixed angles; this picks where the cycle begins."
        ),
        new RngSpot(
            DevilPhase1PinwheelAngleStart, "Phase 1", "Pinwheel Angle Start", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkFiveFlameSpinner.angleOffset),
            null
        ),
        new RngSpot(
            DevilPhase1RingAngleStart, "Phase 1", "Ring Angle Start", RngShape.StartPool, false, true,
            mode => DevilStringOptions(mode, LevelProperties.Devil.States.Main, state => state.pitchforkSixFlameRing.angleOffset),
            null
        ),

        new RngSpot(
            DevilPhase2PatternStart, "Phase 2", "Attack Order Start", RngShape.StartPool, true, true,
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
            FrogsPhase1PatternStart, "Phase 1", "Attack Order Start", RngShape.StartPool, true, true,
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

    /// <summary>Every catalog spot, in page order.</summary>
    public static readonly RngSpot[] All = BuildAll();

    private static RngSpot[] BuildAll() {
        List<RngSpot> all = new List<RngSpot>();
        all.AddRange(Devil);
        all.AddRange(Frogs);
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
        { "Frogs", "Ribby & Croaks" }
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
                return NumberedOptions(labels);
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
                return NumberedOptions(parts);
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
                return NumberedOptions(labels);
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

    /// <summary>One option per pitchfork order list, labeled by its mapped attack order.</summary>
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
                string[] labels = new string[lists.Length];
                for (int i = 0; i < lists.Length; i++) {
                    labels[i] = PrettyPitchforkList(lists[i]);
                }
                return NumberedOptions(labels);
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

    private static string PrettyPitchforkList(string list) {
        string[] entries = list.Split(new[] { ',' });
        List<string> labels = new List<string>();
        foreach (string entry in entries) {
            int parsed;
            if (int.TryParse(entry.Trim(), out parsed)) {
                labels.Add(PrettyPitchforkEntry(parsed));
            }
        }
        return string.Join(" - ", labels.ToArray());
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
            return NumberedOptions(labels);
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

    private static RngOption[] NumberedOptions(string[] labels) {
        Dictionary<string, int> seen = new Dictionary<string, int>(StringComparer.Ordinal);
        RngOption[] options = new RngOption[labels.Length];
        for (int i = 0; i < labels.Length; i++) {
            int occurrence;
            seen.TryGetValue(labels[i], out occurrence);
            seen[labels[i]] = occurrence + 1;
            options[i] = new RngOption(labels[i] + " #" + (occurrence + 1), i);
        }
        return options;
    }
}
