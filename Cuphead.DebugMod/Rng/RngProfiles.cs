using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;

namespace BepInEx.CupheadDebugMod.Rng;

/// <summary>One option inside a weighted pool.</summary>
internal sealed class RngPoolEntry {
    /// <summary>Option index for StartPool/Choice spots.</summary>
    public int IntValue;

    /// <summary>Chosen value for Float spots.</summary>
    public float FloatValue;

    public float Weight = 1f;

    public RngPoolEntry(int intValue, float weight) {
        IntValue = intValue;
        Weight = weight;
    }

    public RngPoolEntry(float floatValue, float weight) {
        FloatValue = floatValue;
        Weight = weight;
    }
}

/// <summary>The user configuration for one spot. Null (absent) means vanilla.</summary>
internal sealed class RngSpotConfig {
    public readonly List<RngPoolEntry> Pool = new List<RngPoolEntry>();

    /// <summary>Sequence steps for per-event choice spots: played after the step-1 pin.</summary>
    public readonly List<int> Sequence = new List<int>();

    /// <summary>Sequence values for float spots: step 1 first, then the rest in order.</summary>
    public readonly List<float> FloatSequence = new List<float>();

    /// <summary>True when the last configured value repeats forever instead of falling back
    /// to natural randomness after the sequence ends.</summary>
    public bool LoopSequence;

    public int? LockPosition;

    public bool IsEmpty {
        get { return Pool.Count == 0 && Sequence.Count == 0 && FloatSequence.Count == 0 && !LockPosition.HasValue; }
    }

    /// <summary>Number of sequential values for choice spots: the step-1 pin plus the steps.</summary>
    public int ChoiceValueCount {
        get { return (Pool.Count == 1 ? 1 : 0) + Sequence.Count; }
    }

    /// <summary>True when the configuration forces one constant outcome forever (which vanilla
    /// can produce per event but not as an unbroken sequence). A looping tail of -1 means
    /// that occurrence stays random, so such a tail is not a constant.</summary>
    public bool IsConstantRepeat {
        get {
            if (LockPosition.HasValue) {
                return true;
            }
            if (!LoopSequence) {
                return false;
            }
            if (FloatSequence.Count > 0) {
                return FloatSequence[FloatSequence.Count - 1] >= 0f;
            }
            int last = Sequence.Count > 0
                ? Sequence[Sequence.Count - 1]
                : (Pool.Count == 1 ? Pool[0].IntValue : -1);
            return ChoiceValueCount > 0 && last >= 0;
        }
    }
}

/// <summary>A value assigned to a spot inside a form: an option index or a float value.</summary>
internal struct RngFormValue {
    public bool IsFloat;
    public int IntValue;
    public float FloatValue;

    public static RngFormValue Int(int value) {
        RngFormValue value2 = new RngFormValue();
        value2.IntValue = value;
        return value2;
    }

    public static RngFormValue Float(float value) {
        RngFormValue value2 = new RngFormValue();
        value2.IsFloat = true;
        value2.FloatValue = value;
        return value2;
    }
}

/// <summary>Sequence steps stored inside a form for one spot: played after the form's pin
/// (its assignment) on each occurrence, with the last value looping or handing back to
/// natural randomness.</summary>
internal sealed class RngFormSequence {
    public readonly List<int> IntSteps = new List<int>();
    public readonly List<float> FloatSteps = new List<float>();
    public bool Loop = true;
}

/// <summary>
/// One named, weighted bundle of spot assignments ("form"). A form pins individual spot
/// values; uncertainty comes from weighing the forms of a set, so a form never contains
/// distributions of its own. Unassigned spots fall through to their own spot config.
/// </summary>
internal sealed class RngForm {
    public string Name = "Form";
    public float Weight;
    public bool Enabled = true;
    public readonly Dictionary<string, RngFormValue> Assignments = new Dictionary<string, RngFormValue>(StringComparer.Ordinal);
    public readonly Dictionary<string, RngFormSequence> Sequences = new Dictionary<string, RngFormSequence>(StringComparer.Ordinal);
}

/// <summary>The weighted form list of one stage on one difficulty.</summary>
internal sealed class RngFormSet {
    public bool Enabled = true;
    public readonly List<RngForm> Forms = new List<RngForm>();
}

/// <summary>A named, complete set of spot overrides.</summary>
internal sealed class RngProfile {
    public readonly string Name;
    public readonly bool BuiltIn;
    public readonly bool ReadOnly;
    public readonly Dictionary<string, RngSpotConfig> Spots = new Dictionary<string, RngSpotConfig>(StringComparer.Ordinal);
    public readonly Dictionary<string, RngFormSet> FormSets = new Dictionary<string, RngFormSet>(StringComparer.Ordinal);

    public RngProfile(string name, bool builtIn, bool readOnly) {
        Name = name;
        BuiltIn = builtIn;
        ReadOnly = readOnly;
    }

    public RngSpotConfig GetSpot(string key) {
        RngSpotConfig config;
        return Spots.TryGetValue(key, out config) ? config : null;
    }

    public RngSpotConfig GetOrCreateSpot(string key) {
        RngSpotConfig config;
        if (!Spots.TryGetValue(key, out config)) {
            config = new RngSpotConfig();
            Spots[key] = config;
        }
        return config;
    }

    public void RemoveSpot(string key) {
        Spots.Remove(key);
    }
}

/// <summary>
/// Loads, stores and saves RNG profiles. Profiles live in
/// BepInEx/config/Cuphead.DebugMod/rng_profiles.cfg; a missing line always means vanilla.
/// </summary>
internal static class RngProfiles {
    public const string VanillaName = "Vanilla";
    public const string PerfectName = "Perfect RNG (1.1+ Any%)";
    public const string CustomName = "Custom";

    private static readonly Dictionary<string, RngProfile> Profiles = new Dictionary<string, RngProfile>(StringComparer.Ordinal);
    private static string activeName = CustomName;
    private static bool initialized;

    private static string FilePath {
        get { return Path.Combine(Paths.ConfigPath, Path.Combine("Cuphead.DebugMod", "rng_profiles.cfg")); }
    }

    public static string ActiveName {
        get { return activeName; }
    }

    public static RngProfile Active {
        get { return GetProfile(activeName); }
    }

    public static RngProfile GetProfile(string name) {
        RngProfile profile;
        return Profiles.TryGetValue(name, out profile) ? profile : null;
    }

    public static IEnumerable<RngProfile> All {
        get { return Profiles.Values; }
    }

    public static void Initialize() {
        if (initialized) {
            return;
        }
        initialized = true;

        BuildBuiltInProfiles();
        Load();
        Save();
    }

    public static void SetActive(string name) {
        if (!Profiles.ContainsKey(name)) {
            return;
        }
        activeName = name;
        RngState.ResetSession();
        Save();
    }

    /// <summary>Deep-copies the active built-in profile into Custom and makes Custom active.</summary>
    public static void CopyBuiltInToCustom() {
        RngProfile source = Active;
        if (source == null || !source.BuiltIn) {
            return;
        }

        RngProfile custom = GetOrCreateCustom();
        custom.Spots.Clear();
        foreach (KeyValuePair<string, RngSpotConfig> pair in source.Spots) {
            RngSpotConfig copy = custom.GetOrCreateSpot(pair.Key);
            copy.LockPosition = pair.Value.LockPosition;
            copy.Pool.AddRange(pair.Value.Pool);
            copy.Sequence.AddRange(pair.Value.Sequence);
            copy.FloatSequence.AddRange(pair.Value.FloatSequence);
            copy.LoopSequence = pair.Value.LoopSequence;
        }

        SetActive(CustomName);
    }

    public static RngSpotConfig GetSpotConfig(RngSpot spot, Level.Mode mode) {
        RngProfile profile = Active;
        if (profile == null || spot == null) {
            return null;
        }
        return profile.GetSpot(spot.Key(mode));
    }

    public static void ClearSpot(RngSpot spot, Level.Mode mode) {
        RngProfile profile = Active;
        if (profile == null || spot == null || profile.ReadOnly) {
            return;
        }
        profile.RemoveSpot(spot.Key(mode));
        RngState.ResetSession();
        Save();
    }

    public static void SetStartPool(RngSpot spot, Level.Mode mode, List<RngPoolEntry> entries) {
        RngProfile profile = Active;
        if (profile == null || spot == null || profile.ReadOnly) {
            return;
        }

        string key = spot.Key(mode);
        if (entries == null || entries.Count == 0) {
            profile.RemoveSpot(key);
        } else {
            // freshly created configurations default to looping (the constant behavior);
            // on an existing config the user's loop choice is preserved
            bool isNew = !profile.Spots.ContainsKey(key);
            RngSpotConfig config = profile.GetOrCreateSpot(key);
            config.Pool.Clear();
            config.Pool.AddRange(entries);
            if (isNew) {
                config.LoopSequence = true;
            }
        }
        RngState.ResetSession();
        Save();
    }

    /// <summary>Writes a float spot's sequence: step 1 first, then the remaining steps in
    /// order; with loop the last value repeats forever, otherwise natural randomness
    /// resumes after the sequence ends. Freshly created configurations default to looping.</summary>
    public static void SetFloatSequence(RngSpot spot, Level.Mode mode, List<float> values, bool loop) {
        RngProfile profile = Active;
        if (profile == null || spot == null || profile.ReadOnly) {
            return;
        }

        string key = spot.Key(mode);
        if (values == null || values.Count == 0) {
            profile.RemoveSpot(key);
        } else {
            bool isNew = !profile.Spots.ContainsKey(key);
            RngSpotConfig config = profile.GetOrCreateSpot(key);
            config.FloatSequence.Clear();
            config.FloatSequence.AddRange(values);
            config.LoopSequence = isNew ? true : loop;
            config.Pool.Clear();
            config.Sequence.Clear();
        }
        RngState.ResetSession();
        Save();
    }

    /// <summary>Writes a per-event choice spot's configuration: the pinned option (null =
    /// the standing value stays random) plus the plan steps that play after it. An empty
    /// write clears the spot back to vanilla. Freshly created configurations default to
    /// looping; on an existing one the passed loop flag applies.</summary>
    public static void SetChoiceValues(RngSpot spot, Level.Mode mode, int? pin, List<int> steps, bool loop) {
        RngProfile profile = Active;
        if (profile == null || spot == null || profile.ReadOnly) {
            return;
        }

        string key = spot.Key(mode);
        if (!pin.HasValue && (steps == null || steps.Count == 0)) {
            profile.RemoveSpot(key);
            RngState.ResetSession();
            Save();
            return;
        }

        bool isNew = !profile.Spots.ContainsKey(key);
        RngSpotConfig config = profile.GetOrCreateSpot(key);
        config.Pool.Clear();
        if (pin.HasValue) {
            config.Pool.Add(new RngPoolEntry(pin.Value, 1f));
        }
        config.Sequence.Clear();
        if (steps != null) {
            config.Sequence.AddRange(steps);
        }
        config.FloatSequence.Clear();
        config.LockPosition = null;
        config.LoopSequence = isNew ? true : loop;
        if (config.IsEmpty) {
            profile.RemoveSpot(key);
        }
        RngState.ResetSession();
        Save();
    }

    // ------------------------------------------------------------------
    // Forms: named, weighted bundles of spot pins, grouped per stage and
    // difficulty. Resolution order at fight time: drawn form pin -> spot
    // config -> vanilla. See RngState.GetDrawnForm.
    // ------------------------------------------------------------------

    private static string FormSetKey(string stageId, Level.Mode mode) {
        return stageId + "@" + RngSpot.ModeKey(mode);
    }

    public static RngFormSet GetFormSet(string stageId, Level.Mode mode) {
        RngProfile profile = Active;
        if (profile == null) {
            return null;
        }
        RngFormSet set;
        return profile.FormSets.TryGetValue(FormSetKey(stageId, mode), out set) ? set : null;
    }

    public static RngFormSet GetOrCreateFormSet(string stageId, Level.Mode mode) {
        RngProfile profile = Active;
        if (profile == null || profile.ReadOnly) {
            return null;
        }

        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null) {
            set = new RngFormSet();
            profile.FormSets[FormSetKey(stageId, mode)] = set;
            RngState.ResetSession();
            Save();
        }
        return set;
    }

    public static void SetFormSetEnabled(string stageId, Level.Mode mode, bool enabled) {
        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null || set.Enabled == enabled) {
            return;
        }
        set.Enabled = enabled;
        RngState.ResetSession();
        Save();
    }

    /// <summary>Adds a form. The first form of an empty (or all-zero) set starts at 100% so a
    /// lone form is deterministic; later additions start at 0% and get weight by slider.</summary>
    public static int AddForm(string stageId, Level.Mode mode, RngForm form) {
        RngFormSet set = GetOrCreateFormSet(stageId, mode);
        if (set == null) {
            return -1;
        }

        float existingTotal = 0f;
        foreach (RngForm existing in set.Forms) {
            if (existing.Weight > 0f) {
                existingTotal += existing.Weight;
            }
        }
        form.Weight = set.Forms.Count == 0 || existingTotal <= 0f ? 100f : 0f;
        if (string.IsNullOrEmpty(form.Name)) {
            form.Name = "Form " + (set.Forms.Count + 1);
        }
        set.Forms.Add(form);
        RngState.ResetSession();
        Save();
        return set.Forms.Count - 1;
    }

    public static void DeleteForm(string stageId, Level.Mode mode, int index) {
        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null || index < 0 || index >= set.Forms.Count) {
            return;
        }
        set.Forms.RemoveAt(index);
        RngState.ResetSession();
        Save();
    }

    public static void SetFormWeights(string stageId, Level.Mode mode, float[] weights) {
        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null) {
            return;
        }
        for (int i = 0; i < set.Forms.Count && i < weights.Length; i++) {
            set.Forms[i].Weight = weights[i];
        }
        RngState.ResetSession();
        Save();
    }

    public static void SetFormAssignment(string stageId, Level.Mode mode, int formIndex, string spotKey, RngFormValue? value) {
        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null || formIndex < 0 || formIndex >= set.Forms.Count) {
            return;
        }
        RngForm form = set.Forms[formIndex];
        if (value.HasValue) {
            form.Assignments[spotKey] = value.Value;
        } else {
            form.Assignments.Remove(spotKey);
        }
        RngState.ResetSession();
        Save();
    }

    /// <summary>Writes a form's int configuration for a spot: the pinned option (null =
    /// the standing value stays naturally random inside this form) plus the plan steps
    /// that play after it. An empty write removes the spot from the form.</summary>
    public static void SetFormIntValues(string stageId, Level.Mode mode, int formIndex, string spotKey, int? pin, List<int> steps, bool loop) {
        RngForm form = GetForm(stageId, mode, formIndex);
        if (form == null) {
            return;
        }

        if (!pin.HasValue && (steps == null || steps.Count == 0)) {
            form.Assignments.Remove(spotKey);
            form.Sequences.Remove(spotKey);
            RngState.ResetSession();
            Save();
            return;
        }

        if (pin.HasValue) {
            form.Assignments[spotKey] = RngFormValue.Int(pin.Value);
        } else {
            form.Assignments.Remove(spotKey);
        }
        WriteFormSequence(form, spotKey, steps, null, loop);
        RngState.ResetSession();
        Save();
    }

    /// <summary>Writes a form's full float value list for a spot: value[0] is the pin, further
    /// values are sequence steps; an empty list clears the spot from the form.</summary>
    public static void SetFormFloatValues(string stageId, Level.Mode mode, int formIndex, string spotKey, List<float> values, bool loop) {
        RngForm form = GetForm(stageId, mode, formIndex);
        if (form == null) {
            return;
        }

        if (values == null || values.Count == 0) {
            form.Assignments.Remove(spotKey);
            form.Sequences.Remove(spotKey);
        } else {
            form.Assignments[spotKey] = RngFormValue.Float(values[0]);
            WriteFormSequence(form, spotKey, null, values.GetRange(1, values.Count - 1), loop);
        }
        RngState.ResetSession();
        Save();
    }

    private static RngForm GetForm(string stageId, Level.Mode mode, int formIndex) {
        RngFormSet set = GetFormSet(stageId, mode);
        if (set == null || formIndex < 0 || formIndex >= set.Forms.Count) {
            return null;
        }
        return set.Forms[formIndex];
    }

    /// <summary>Stores the steps after a form's pin. A sequence with no steps only needs
    /// storing when it disables looping (a bare pin loops by default, matching the
    /// constant behavior of a form pin).</summary>
    private static void WriteFormSequence(RngForm form, string spotKey, List<int> intSteps, List<float> floatSteps, bool loop) {
        if ((intSteps == null || intSteps.Count == 0) && (floatSteps == null || floatSteps.Count == 0) && loop) {
            form.Sequences.Remove(spotKey);
            return;
        }

        RngFormSequence sequence;
        if (!form.Sequences.TryGetValue(spotKey, out sequence)) {
            sequence = new RngFormSequence();
            form.Sequences[spotKey] = sequence;
        }
        if (intSteps != null) {
            sequence.IntSteps.Clear();
            sequence.IntSteps.AddRange(intSteps);
            sequence.FloatSteps.Clear();
        }
        if (floatSteps != null) {
            sequence.FloatSteps.Clear();
            sequence.FloatSteps.AddRange(floatSteps);
            sequence.IntSteps.Clear();
        }
        sequence.Loop = loop;
    }

    /// <summary>Copies a stage's form set to other difficulties. Assignment keys carrying a
    /// "@difficulty" suffix are remapped to the target difficulty; bare keys copy as-is.</summary>
    public static void CopyFormSet(string stageId, Level.Mode fromMode, Level.Mode[] toModes) {
        RngProfile profile = Active;
        if (profile == null || profile.ReadOnly) {
            return;
        }

        RngFormSet source = GetFormSet(stageId, fromMode);
        if (source == null) {
            return;
        }

        bool changed = false;
        foreach (Level.Mode toMode in toModes) {
            if (toMode == fromMode) {
                continue;
            }
            RngFormSet copy = new RngFormSet();
            copy.Enabled = source.Enabled;
            foreach (RngForm form in source.Forms) {
                RngForm formCopy = new RngForm();
                formCopy.Name = form.Name;
                formCopy.Weight = form.Weight;
                formCopy.Enabled = form.Enabled;
                foreach (KeyValuePair<string, RngFormValue> pair in form.Assignments) {
                    formCopy.Assignments[RemapFormKey(pair.Key, fromMode, toMode)] = pair.Value;
                }
                foreach (KeyValuePair<string, RngFormSequence> pair in form.Sequences) {
                    RngFormSequence sequenceCopy = new RngFormSequence();
                    sequenceCopy.IntSteps.AddRange(pair.Value.IntSteps);
                    sequenceCopy.FloatSteps.AddRange(pair.Value.FloatSteps);
                    sequenceCopy.Loop = pair.Value.Loop;
                    formCopy.Sequences[RemapFormKey(pair.Key, fromMode, toMode)] = sequenceCopy;
                }
                copy.Forms.Add(formCopy);
            }
            profile.FormSets[FormSetKey(stageId, toMode)] = copy;
            changed = true;
        }

        if (changed) {
            RngState.ResetSession();
            Save();
        }
    }

    private static string RemapFormKey(string key, Level.Mode fromMode, Level.Mode toMode) {
        string fromSuffix = "@" + RngSpot.ModeKey(fromMode);
        if (key.EndsWith(fromSuffix, StringComparison.Ordinal)) {
            return key.Substring(0, key.Length - fromSuffix.Length) + "@" + RngSpot.ModeKey(toMode);
        }
        return key;
    }

    // ------------------------------------------------------------------
    // Built-in profiles
    // ------------------------------------------------------------------

    private static void BuildBuiltInProfiles() {
        Profiles[VanillaName] = new RngProfile(VanillaName, true, true);

        // Ported from the old RngOverrideComponent dictionary.
        RngProfile perfect = new RngProfile(PerfectName, true, true);
        RngSpot patternStart = RngSpots.Find(RngSpots.DevilPhase1PatternStart);
        RngSpot headFirst = RngSpots.Find(RngSpots.DevilPhase1HeadFirst);
        RngSpot pitchforkType = RngSpots.Find(RngSpots.DevilPhase1PitchforkType);
        RngSpot bouncerParry = RngSpots.Find(RngSpots.DevilPhase1BouncerParryIndex);
        RngSpot bouncerAngle = RngSpots.Find(RngSpots.DevilPhase1BouncerAngleStart);
        if (patternStart != null) {
            perfect.GetOrCreateSpot(patternStart.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(6, 1f));
        }
        if (headFirst != null) {
            perfect.GetOrCreateSpot(headFirst.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(0, 1f));
        }
        if (pitchforkType != null) {
            perfect.GetOrCreateSpot(pitchforkType.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(0, 1f));
        }
        if (bouncerParry != null) {
            perfect.GetOrCreateSpot(bouncerParry.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(2, 1f));
        }
        if (bouncerAngle != null) {
            perfect.GetOrCreateSpot(bouncerAngle.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(5, 1f));
        }

        // Ported from the old Forest Follies preset (Left1 + B_50): the first acorn flies
        // in from the left at height 50. Both spots consume their pinned entry first.
        RngSpot forestSide = RngSpots.Find(RngSpots.ForestFolliesAcornSide);
        RngSpot forestHeight = RngSpots.Find(RngSpots.ForestFolliesAcornHeight);
        if (forestSide != null) {
            perfect.GetOrCreateSpot(forestSide.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(0, 1f));
        }
        if (forestHeight != null) {
            perfect.GetOrCreateSpot(forestHeight.Key(Level.Mode.Normal)).Pool.Add(new RngPoolEntry(1, 1f));
        }

        Profiles[PerfectName] = perfect;
    }

    private static RngProfile GetOrCreateCustom() {
        RngProfile custom = GetProfile(CustomName);
        if (custom == null) {
            custom = new RngProfile(CustomName, false, false);
            Profiles[CustomName] = custom;
        }
        return custom;
    }

    // ------------------------------------------------------------------
    // Persistence. Format:
    //   active = Custom
    //   [profile Custom]
    //   Devil/Phase1/PatternStart@normal = pool 3:1, 7:0.5
    //   Devil/Phase1/ClapDelay = value 0.25
    //   [formset Devil/Phase1@normal]
    //   enabled = true
    //   form1.name = Form 1
    //   form1.weight = 60
    //   form1.set = Devil/Phase1/PatternStart@normal:2; Devil/Phase1/ClapDelay:f0.3
    // ------------------------------------------------------------------

    private static void Load() {
        try {
            string path = FilePath;
            if (!File.Exists(path)) {
                GetOrCreateCustom();
                return;
            }

            string[] lines = File.ReadAllLines(path);
            RngProfile current = null;
            RngFormSet currentFormSet = null;
            HashSet<string> seenFormSets = new HashSet<string>(StringComparer.Ordinal);
            int formSetOffset = 0;
            foreach (string rawLine in lines) {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal)) {
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal)) {
                    currentFormSet = null;
                    formSetOffset = 0;
                    string inner = line.Substring(1, line.Length - 2).Trim();
                    if (inner.StartsWith("formset", StringComparison.Ordinal)) {
                        string normalizedKey;
                        currentFormSet = ParseFormSetHeader(current, inner.Substring("formset".Length).Trim(), out normalizedKey);
                        if (currentFormSet != null) {
                            // repeated or legacy per-phase sections concatenate into the fight-level set
                            formSetOffset = seenFormSets.Add(normalizedKey) ? 0 : currentFormSet.Forms.Count;
                        }
                    } else {
                        current = ParseSectionHeader(line);
                    }
                    continue;
                }

                int separator = line.IndexOf('=');
                if (separator <= 0) {
                    continue;
                }
                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();

                if (currentFormSet != null) {
                    ParseFormSetEntry(currentFormSet, formSetOffset, key, value);
                    continue;
                }

                if (current == null) {
                    if (key == "active") {
                        activeName = value;
                    }
                    continue;
                }

                ParseEntry(current, key, value);
            }
        } catch (Exception) {
        }

        if (!Profiles.ContainsKey(activeName)) {
            activeName = CustomName;
        }
        GetOrCreateCustom();
        NormalizePerDifficultyKeys();
    }

    /// <summary>Profiles saved before a spot became per-difficulty store it under a bare
    /// key, which no spot.Key(mode) lookup would ever match; move such configs onto every
    /// difficulty the spot is available on so they keep applying.</summary>
    private static void NormalizePerDifficultyKeys() {
        foreach (RngProfile profile in Profiles.Values) {
            List<string> bareKeys = null;
            foreach (KeyValuePair<string, RngSpotConfig> pair in profile.Spots) {
                RngSpot spot = RngSpots.FindByStoredKey(pair.Key);
                if (spot != null && spot.PerDifficulty && pair.Key.IndexOf('@') < 0) {
                    if (bareKeys == null) {
                        bareKeys = new List<string>();
                    }
                    bareKeys.Add(pair.Key);
                }
            }
            if (bareKeys == null) {
                continue;
            }
            foreach (string bareKey in bareKeys) {
                RngSpotConfig config = profile.Spots[bareKey];
                RngSpot spot = RngSpots.FindByStoredKey(bareKey);
                profile.Spots.Remove(bareKey);
                foreach (Level.Mode mode in new[] { Level.Mode.Easy, Level.Mode.Normal, Level.Mode.Hard }) {
                    if (!spot.IsAvailable(mode) || profile.Spots.ContainsKey(spot.Key(mode))) {
                        continue;
                    }
                    RngSpotConfig copy = new RngSpotConfig();
                    copy.Pool.AddRange(config.Pool);
                    copy.Sequence.AddRange(config.Sequence);
                    copy.FloatSequence.AddRange(config.FloatSequence);
                    copy.LoopSequence = config.LoopSequence;
                    copy.LockPosition = config.LockPosition;
                    profile.Spots[spot.Key(mode)] = copy;
                }
            }
        }
    }

    private static RngProfile ParseSectionHeader(string line) {
        // [profile Name] â€” name may be quoted
        string inner = line.Substring(1, line.Length - 2).Trim();
        if (!inner.StartsWith("profile", StringComparison.Ordinal)) {
            return null;
        }
        string name = inner.Substring("profile".Length).Trim();
        if (name.Length >= 2 && name[0] == '"' && name[name.Length - 1] == '"') {
            name = name.Substring(1, name.Length - 2);
        }
        if (name.Length == 0 || name == VanillaName || name == PerfectName) {
            return null; // built-ins can't be shadowed by file data
        }
        return GetOrCreateCustomNamed(name);
    }

    private static RngProfile GetOrCreateCustomNamed(string name) {
        RngProfile profile = GetProfile(name);
        if (profile == null) {
            profile = new RngProfile(name, false, false);
            Profiles[name] = profile;
        }
        return profile;
    }

    /// <summary>[formset Stage@difficulty] sections attach to the profile section above them;
    /// built-in profiles ignore file formsets. Legacy per-phase keys ("Devil/Phase1@normal")
    /// merge into their fight-level set ("Devil@normal").</summary>
    private static RngFormSet ParseFormSetHeader(RngProfile profile, string name, out string normalizedKey) {
        normalizedKey = null;
        if (profile == null || profile.BuiltIn || string.IsNullOrEmpty(name)) {
            return null;
        }

        int at = name.LastIndexOf('@');
        if (at <= 0 || at >= name.Length - 1) {
            return null;
        }
        string stageId = name.Substring(0, at);
        int slash = stageId.IndexOf('/');
        if (slash > 0) {
            stageId = stageId.Substring(0, slash);
        }
        Level.Mode mode = ParseModeKey(name.Substring(at + 1));
        normalizedKey = FormSetKey(stageId, mode);

        RngFormSet set;
        if (!profile.FormSets.TryGetValue(normalizedKey, out set)) {
            set = new RngFormSet();
            profile.FormSets[normalizedKey] = set;
        }
        return set;
    }

    private static Level.Mode ParseModeKey(string key) {
        if (key == "easy") {
            return Level.Mode.Easy;
        }
        if (key == "hard") {
            return Level.Mode.Hard;
        }
        return Level.Mode.Normal;
    }

    private static void ParseFormSetEntry(RngFormSet set, int formOffset, string key, string value) {
        if (key == "enabled") {
            set.Enabled = value == "true";
            return;
        }

        if (!key.StartsWith("form", StringComparison.Ordinal)) {
            return;
        }
        int dot = key.IndexOf('.');
        if (dot <= 4) {
            return;
        }
        int index;
        if (!int.TryParse(key.Substring(4, dot - 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out index) || index < 1) {
            return;
        }
        index += formOffset;
        while (set.Forms.Count < index) {
            set.Forms.Add(new RngForm());
        }
        RngForm form = set.Forms[index - 1];
        string field = key.Substring(dot + 1);

        if (field == "name") {
            form.Name = value;
        } else if (field == "weight") {
            float weight;
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out weight)) {
                form.Weight = weight;
            }
        } else if (field == "enabled") {
            form.Enabled = value == "true";
        } else if (field == "seqs") {
            ParseFormSequences(form, value);
        } else if (field == "set") {
            ParseFormAssignments(form, value);
        }
    }

    private static void ParseFormSequences(RngForm form, string value) {
        string[] entries = value.Split(new[] { ';' });
        foreach (string entry in entries) {
            string item = entry.Trim();
            if (item.Length == 0) {
                continue;
            }
            int colon = item.IndexOf(':');
            if (colon <= 0) {
                continue;
            }
            string spotKey = item.Substring(0, colon).Trim();
            string spec = item.Substring(colon + 1).Trim();
            RngSpot spot = RngSpots.FindByStoredKey(spotKey);
            if (spot == null) {
                continue;
            }

            RngFormSequence sequence = new RngFormSequence();
            string[] tokens = spec.Split(new[] { ',' });
            foreach (string rawToken in tokens) {
                string token = rawToken.Trim();
                if (token.Length == 0) {
                    continue;
                }
                if (token == "loop") {
                    sequence.Loop = true;
                    continue;
                }
                if (token == "noloop") {
                    sequence.Loop = false;
                    continue;
                }
                if (spot.Shape == RngShape.Float) {
                    string number = token[0] == 'f' || token[0] == 'F' ? token.Substring(1) : token;
                    float step;
                    if (float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out step)) {
                        sequence.FloatSteps.Add(step);
                    }
                } else {
                    int step;
                    if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out step)) {
                        sequence.IntSteps.Add(step);
                    }
                }
            }

            if (sequence.IntSteps.Count > 0 || sequence.FloatSteps.Count > 0 || !sequence.Loop) {
                form.Sequences[spotKey] = sequence;
            }
        }
    }

    private static void ParseFormAssignments(RngForm form, string value) {
        string[] entries = value.Split(new[] { ';' });
        foreach (string entry in entries) {
            string item = entry.Trim();
            if (item.Length == 0) {
                continue;
            }
            int colon = item.IndexOf(':');
            if (colon <= 0) {
                continue;
            }
            string spotKey = item.Substring(0, colon).Trim();
            string raw = item.Substring(colon + 1).Trim();
            if (spotKey.Length == 0 || raw.Length == 0) {
                continue;
            }

            if (raw[0] == 'f' || raw[0] == 'F') {
                float floatValue;
                if (float.TryParse(raw.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue)) {
                    form.Assignments[spotKey] = RngFormValue.Float(floatValue);
                }
            } else {
                int intValue;
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) {
                    form.Assignments[spotKey] = RngFormValue.Int(intValue);
                }
            }
        }
    }

    private static void ParseEntry(RngProfile profile, string key, string value) {
        // segments after a '|' are either data parts (seq + pool on one line) or the
        // trailing "loop"/"noloop" flag; without a flag the legacy default applies (a
        // pinned value loops forever, matching old behavior)
        bool loop = true;
        string seqPart = null;
        string floatSeqPart = null;
        string poolPart = null;
        string valuePart = null;
        string lockPart = null;

        string[] segments = value.Split(new[] { '|' });
        for (int i = 0; i < segments.Length; i++) {
            string part = segments[i].Trim();
            if (part.Length == 0) {
                continue;
            }
            if (part == "loop") {
                loop = true;
            } else if (part == "noloop") {
                loop = false;
            } else if (part.StartsWith("seqf", StringComparison.Ordinal)) {
                floatSeqPart = part.Substring(4).Trim();
            } else if (part.StartsWith("seq", StringComparison.Ordinal)) {
                seqPart = part.Substring(3).Trim();
            } else if (part.StartsWith("pool", StringComparison.Ordinal)) {
                poolPart = part.Substring(4).Trim();
            } else if (part.StartsWith("value", StringComparison.Ordinal)) {
                valuePart = part.Substring(5).Trim();
            } else if (part.StartsWith("lock", StringComparison.Ordinal)) {
                lockPart = part.Substring(4).Trim();
            }
        }

        RngSpotConfig config = profile.GetOrCreateSpot(key);

        if (floatSeqPart != null) {
            config.FloatSequence.AddRange(ParseFloatSequence(floatSeqPart));
            config.LoopSequence = loop;
        }
        if (seqPart != null) {
            config.Sequence.AddRange(ParseSequence(seqPart));
            config.LoopSequence = loop;
        }
        if (poolPart != null) {
            config.Pool.AddRange(ParseIntPool(poolPart));
            config.LoopSequence = loop;
        }
        if (valuePart != null) {
            float floatValue;
            if (float.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue)) {
                // a single pinned value loops forever (legacy format)
                config.FloatSequence.Add(floatValue);
                config.LoopSequence = true;
            }
        }
        if (lockPart != null) {
            int position;
            if (int.TryParse(lockPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out position)) {
                config.LockPosition = position;
            }
        }

        if (config.IsEmpty) {
            profile.RemoveSpot(key);
        }
    }

    private static List<int> ParseSequence(string csv) {
        List<int> values = new List<int>();
        string[] parts = csv.Split(new[] { ',' });
        foreach (string part in parts) {
            int parsed;
            if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)) {
                values.Add(parsed);
            }
        }
        return values;
    }

    private static IEnumerable<float> ParseFloatSequence(string csv) {
        List<float> values = new List<float>();
        string[] parts = csv.Split(new[] { ',' });
        foreach (string part in parts) {
            float parsed;
            if (float.TryParse(part.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)) {
                values.Add(parsed);
            }
        }
        return values;
    }

    private static IEnumerable<RngPoolEntry> ParseIntPool(string csv) {
        List<RngPoolEntry> entries = new List<RngPoolEntry>();
        string[] parts = csv.Split(new[] { ',' });
        foreach (string part in parts) {
            string item = part.Trim();
            if (item.Length == 0) {
                continue;
            }
            int colon = item.IndexOf(':');
            string valuePart = colon >= 0 ? item.Substring(0, colon).Trim() : item;
            string weightPart = colon >= 0 ? item.Substring(colon + 1).Trim() : "1";

            int intValue;
            float weight = 1f;
            if (!int.TryParse(valuePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) {
                continue;
            }
            float weightFloat;
            if (float.TryParse(weightPart, NumberStyles.Float, CultureInfo.InvariantCulture, out weightFloat) && weightFloat > 0f) {
                weight = weightFloat;
            }
            entries.Add(new RngPoolEntry(intValue, weight));
        }
        return entries;
    }

    private static void Save() {
        try {
            string path = FilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; Cuphead.DebugMod RNG profiles.");
            sb.AppendLine("; A missing line always means vanilla. Profiles can be edited by hand while the game runs.");
            sb.AppendLine("active = " + activeName);

            List<RngProfile> userProfiles = new List<RngProfile>();
            foreach (RngProfile profile in Profiles.Values) {
                if (!profile.BuiltIn) {
                    userProfiles.Add(profile);
                }
            }

            foreach (RngProfile profile in userProfiles) {
                sb.AppendLine();
                sb.AppendLine("[profile \"" + profile.Name + "\"]");
                List<string> keys = new List<string>(profile.Spots.Keys);
                keys.Sort(StringComparer.Ordinal);
                foreach (string key in keys) {
                    string line = SerializeEntry(profile.Spots[key]);
                    if (!string.IsNullOrEmpty(line)) {
                        sb.AppendLine(key + " = " + line);
                    }
                }

                List<string> formSetKeys = new List<string>(profile.FormSets.Keys);
                formSetKeys.Sort(StringComparer.Ordinal);
                foreach (string formSetKey in formSetKeys) {
                    RngFormSet set = profile.FormSets[formSetKey];
                    sb.AppendLine();
                    sb.AppendLine("[formset " + formSetKey + "]");
                    sb.AppendLine("enabled = " + (set.Enabled ? "true" : "false"));
                    for (int i = 0; i < set.Forms.Count; i++) {
                        RngForm form = set.Forms[i];
                        sb.AppendLine("form" + (i + 1) + ".name = " + form.Name);
                        sb.AppendLine("form" + (i + 1) + ".weight = " + form.Weight.ToString("0.0############", CultureInfo.InvariantCulture));
                        if (!form.Enabled) {
                            sb.AppendLine("form" + (i + 1) + ".enabled = false");
                        }
                        string assignments = SerializeFormAssignments(form);
                        if (assignments.Length > 0) {
                            sb.AppendLine("form" + (i + 1) + ".set = " + assignments);
                        }
                        string sequences = SerializeFormSequences(form);
                        if (sequences.Length > 0) {
                            sb.AppendLine("form" + (i + 1) + ".seqs = " + sequences);
                        }
                    }
                }
            }

            File.WriteAllText(path, sb.ToString());
        } catch (Exception) {
            // Keep running with in-memory config when the disk is not writable.
        }
    }

    private static string SerializeFormAssignments(RngForm form) {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, RngFormValue> pair in form.Assignments) {
            if (sb.Length > 0) {
                sb.Append("; ");
            }
            sb.Append(pair.Key).Append(':');
            if (pair.Value.IsFloat) {
                sb.Append('f').Append(pair.Value.FloatValue.ToString("0.0############", CultureInfo.InvariantCulture));
            } else {
                sb.Append(pair.Value.IntValue.ToString(CultureInfo.InvariantCulture));
            }
        }
        return sb.ToString();
    }

    private static string SerializeFormSequences(RngForm form) {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, RngFormSequence> pair in form.Sequences) {
            RngSpot spot = RngSpots.FindByStoredKey(pair.Key);
            if (spot == null) {
                continue;
            }

            bool isFloat = spot.Shape == RngShape.Float;
            List<float> floatSteps = pair.Value.FloatSteps;
            List<int> intSteps = pair.Value.IntSteps;

            // no steps with looping enabled is the implicit default (a bare pin loops)
            if ((isFloat ? floatSteps.Count : intSteps.Count) == 0 && pair.Value.Loop) {
                continue;
            }

            if (sb.Length > 0) {
                sb.Append("; ");
            }
            sb.Append(pair.Key).Append(':');
            if (isFloat) {
                foreach (float step in floatSteps) {
                    sb.Append('f').Append(step.ToString("0.0############", CultureInfo.InvariantCulture)).Append(',');
                }
            } else {
                foreach (int step in intSteps) {
                    sb.Append(step.ToString(CultureInfo.InvariantCulture)).Append(',');
                }
            }
            if (!pair.Value.Loop) {
                sb.Append("noloop");
            } else if (sb[sb.Length - 1] == ',') {
                sb.Length -= 1;
            }
        }
        return sb.ToString();
    }

    private static string SerializeEntry(RngSpotConfig config) {
        // float spots: a single looping value stays in the compact legacy format
        if (config.FloatSequence.Count > 0) {
            if (config.FloatSequence.Count == 1 && config.LoopSequence) {
                return "value " + config.FloatSequence[0].ToString("0.0############", CultureInfo.InvariantCulture);
            }
            StringBuilder floats = new StringBuilder("seqf");
            foreach (float value in config.FloatSequence) {
                floats.Append(' ').Append(value.ToString("0.0############", CultureInfo.InvariantCulture)).Append(',');
            }
            floats.Length -= 1;
            floats.Append(config.LoopSequence ? " | loop" : " | noloop");
            return floats.ToString();
        }

        if (config.LockPosition.HasValue) {
            return "lock " + config.LockPosition.Value.ToString(CultureInfo.InvariantCulture);
        }

        // choice spots: the step-1 pin plus the sequence steps, with an explicit loop flag
        StringBuilder sb = new StringBuilder();
        if (config.Sequence.Count > 0) {
            sb.Append("seq");
            foreach (int value in config.Sequence) {
                sb.Append(' ').Append(value.ToString(CultureInfo.InvariantCulture)).Append(',');
            }
            sb.Length -= 1;
            if (config.Pool.Count > 0) {
                sb.Append(" |");
            }
        }
        if (config.Pool.Count > 0) {
            if (config.Sequence.Count == 0) {
                sb.Append("pool");
            }
            foreach (RngPoolEntry entry in config.Pool) {
                sb.Append(' ').Append(entry.IntValue.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.Weight.ToString("0.0############", CultureInfo.InvariantCulture)).Append(',');
            }
            sb.Length -= 1;
        }
        if (sb.Length == 0) {
            return null;
        }
        sb.Append(config.LoopSequence ? " | loop" : " | noloop");
        return sb.ToString().Trim();
    }

}
