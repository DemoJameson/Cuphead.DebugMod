using System;
using System.Collections.Generic;
using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Rng;

/// <summary>
/// The runtime side of RNG control. Game patches call the resolvers here with the value
/// vanilla would have produced; vanilla is passed through untouched (no extra RNG draws)
/// and configured spots get their value from the active profile instead. When a form is
/// drawn for the fight it is authoritative: its pins apply, unpinned spots are naturally
/// random, and spot configs do not leak in.
/// </summary>
internal static class RngState {
    private static readonly Dictionary<string, int> SequenceCounters = new Dictionary<string, int>(StringComparer.Ordinal);

    /// <summary>The form drawn for a stage this fight: assignment key -> value.</summary>
    private static readonly Dictionary<string, RngForm> DrawnForms = new Dictionary<string, RngForm>(StringComparer.Ordinal);

    /// <summary>Stages whose form has already been drawn this fight (including "no form").</summary>
    private static readonly HashSet<string> DrawnStages = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Per-fight state (e.g. sequence prefixes) is dropped on level load or profile change.</summary>
    public static void ResetSession() {
        SequenceCounters.Clear();
        DrawnForms.Clear();
        DrawnStages.Clear();
    }

    /// <summary>
    /// Restarts one spot's per-event sequence, e.g. when the sub-attack it belongs to
    /// begins again (the Bulls laser plan restarts every time the slot machine picks Bulls).
    /// </summary>
    public static void ResetEventCounter(string spotId) {
        SequenceCounters.Remove(spotId);
    }

    /// <summary>
    /// Returns the form drawn for this fight, or null when no form applies. The draw
    /// happens lazily at the first spot query of the fight, which is exactly when the game
    /// activates it (LevelInit/OnStateChanged hooks already call the resolvers then), and
    /// only once per fight. The returned form is a snapshot, so later profile edits cannot
    /// mutate what this fight already drew.
    /// </summary>
    private static RngForm GetDrawnForm(string stageId, Level.Mode mode) {
        string cacheKey = stageId + "@" + RngSpot.ModeKey(mode);

        RngForm drawn;
        if (DrawnForms.TryGetValue(cacheKey, out drawn)) {
            return drawn;
        }
        if (DrawnStages.Contains(cacheKey)) {
            return null;
        }
        DrawnStages.Add(cacheKey);

        // Pattern forms are an opt-in advanced mode; with the toggle off they neither show
        // in the menu nor apply at runtime.
        if (Settings.EnablePatternForms == null || !Settings.EnablePatternForms.Value) {
            return null;
        }

        RngProfile profile = RngProfiles.Active;
        if (profile == null) {
            return null;
        }

        RngFormSet set = RngProfiles.GetFormSet(stageId, mode);
        if (set == null || !set.Enabled || set.Forms.Count == 0) {
            return null;
        }

        float total = 0f;
        foreach (RngForm form in set.Forms) {
            if (form.Enabled && form.Weight > 0f) {
                total += form.Weight;
            }
        }
        if (total <= 0f) {
            return null;
        }

        float roll = UnityEngine.Random.value * total;
        RngForm picked = null;
        foreach (RngForm form in set.Forms) {
            if (!form.Enabled || form.Weight <= 0f) {
                continue;
            }
            roll -= form.Weight;
            if (roll <= 0f) {
                picked = form;
                break;
            }
        }
        if (picked == null) {
            return null;
        }

        // Copy so later profile edits cannot mutate what this fight already drew.
        RngForm snapshot = new RngForm();
        snapshot.Name = picked.Name;
        snapshot.Weight = picked.Weight;
        snapshot.Enabled = picked.Enabled;
        foreach (KeyValuePair<string, RngFormValue> pair in picked.Assignments) {
            snapshot.Assignments[pair.Key] = pair.Value;
        }
        foreach (KeyValuePair<string, RngFormSequence> pair in picked.Sequences) {
            RngFormSequence sequenceCopy = new RngFormSequence();
            sequenceCopy.IntSteps.AddRange(pair.Value.IntSteps);
            sequenceCopy.FloatSteps.AddRange(pair.Value.FloatSteps);
            sequenceCopy.Loop = pair.Value.Loop;
            snapshot.Sequences[pair.Key] = sequenceCopy;
        }
        DrawnForms[cacheKey] = snapshot;
        return snapshot;
    }

    /// <summary>
    /// Resolves an integer option: the start position of a cyclic list (StartPool) or the
    /// pick of a per-event choice (Choice). Returns the vanilla value when nothing is
    /// configured. With preIncrement the consumer advances the index before using it,
    /// which is what PositionStart-style spots and GetUserPattern callers expect.
    /// </summary>
    public static int Index(string spotId, Level.Mode mode, int vanillaIndex, int count, bool preIncrement) {
        RngSpot spot = RngSpots.Find(spotId);
        if (spot == null || count <= 0) {
            return vanillaIndex;
        }

        // Layer 1: a drawn form is authoritative for the whole fight. Spots it pins get
        // their pinned value; spots with sequence steps play those in order; spots left
        // unpinned are naturally random - leftover spot configs from simple mode do not
        // leak into a drawn form's fight.
        RngForm drawnForm = GetDrawnForm(spot.StageId, mode);
        if (drawnForm != null) {
            RngFormValue assigned;
            bool hasPin = drawnForm.Assignments.TryGetValue(spot.Key(mode), out assigned) && !assigned.IsFloat;
            RngFormSequence sequence;
            bool hasSteps = drawnForm.Sequences.TryGetValue(spot.Key(mode), out sequence) && sequence.IntSteps.Count > 0;

            if (hasPin || hasSteps) {
                int step = NextEventIndex(spotId);
                if (step == 0 && hasPin) {
                    return MappedIndex(assigned.IntValue, count, preIncrement, vanillaIndex);
                }
                int sequenceStep = step - (hasPin ? 1 : 0);
                if (hasSteps && sequenceStep >= 0 && sequenceStep < sequence.IntSteps.Count) {
                    return MappedIndex(sequence.IntSteps[sequenceStep], count, preIncrement, vanillaIndex);
                }
                bool loops = hasSteps ? sequence.Loop : true;
                if (loops) {
                    // the sequence tail if steps exist, otherwise the pinned value repeats
                    int last = hasSteps && sequence.IntSteps.Count > 0
                        ? sequence.IntSteps[sequence.IntSteps.Count - 1]
                        : assigned.IntValue;
                    return MappedIndex(last, count, preIncrement, vanillaIndex);
                }
                return vanillaIndex;
            }
            return vanillaIndex;
        }

        // Layer 2: no form was drawn (forms off, set parked/empty) - the spot's own
        // configuration applies.
        RngProfile profile = RngProfiles.Active;
        if (profile == null) {
            return vanillaIndex;
        }

        RngSpotConfig config = profile.GetSpot(spot.Key(mode));
        if (config == null) {
            return vanillaIndex;
        }

        // Per-event choice spots support sequences: step 1 is the pinned option, further
        // steps play in order, and the last value either loops forever or hands over to
        // natural randomness.
        if (spot.PerEvent && (config.Pool.Count == 1 || config.Sequence.Count > 0)) {
            int step = NextEventIndex(spotId);
            if (step == 0 && config.Pool.Count == 1) {
                return MappedIndex(config.Pool[0].IntValue, count, preIncrement, vanillaIndex);
            }
            int sequenceStep = step - (config.Pool.Count == 1 ? 1 : 0);
            if (sequenceStep >= 0 && sequenceStep < config.Sequence.Count) {
                return MappedIndex(config.Sequence[sequenceStep], count, preIncrement, vanillaIndex);
            }
            if (config.LoopSequence) {
                int last = config.Sequence.Count > 0
                    ? config.Sequence[config.Sequence.Count - 1]
                    : config.Pool[0].IntValue;
                return MappedIndex(last, count, preIncrement, vanillaIndex);
            }
            return vanillaIndex;
        }

        int position;
        if (config.LockPosition.HasValue) {
            position = config.LockPosition.Value;
        } else {
            position = PickPool(config.Pool, vanillaIndex);
        }

        return MappedIndex(position, count, preIncrement, vanillaIndex);
    }

    private static int MappedIndex(int position, int count, bool preIncrement, int vanillaIndex) {
        if (position < 0) {
            return vanillaIndex;
        }
        if (position >= count) {
            position %= count;
        }
        return preIncrement ? (position + count - 1) % count : position;
    }

    /// <summary>A float step/pin of -1 stands for "this occurrence stays random"; anything
    /// below zero is the sentinel (configured values are always positive delays).</summary>
    private static float ResolvedFloat(float value, float vanillaValue) {
        return value < 0f ? vanillaValue : value;
    }

    /// <summary>The zero-based event index for a spot's sequence, advancing once per event
    /// and resetting with the per-fight state.</summary>
    private static int NextEventIndex(string spotId) {
        int counter;
        if (!SequenceCounters.TryGetValue(spotId, out counter)) {
            counter = 0;
        }
        SequenceCounters[spotId] = counter + 1;
        return counter;
    }

    /// <summary>Resolves a float: the drawn form's value, the configured value, a weighted pick from a float pool, or vanilla.</summary>
    public static float Float(string spotId, Level.Mode mode, float vanillaValue) {
        RngSpot spot = RngSpots.Find(spotId);
        if (spot == null) {
            return vanillaValue;
        }

        RngForm drawnForm = GetDrawnForm(spot.StageId, mode);
        if (drawnForm != null) {
            RngFormValue assigned;
            bool hasPin = drawnForm.Assignments.TryGetValue(spot.Key(mode), out assigned) && assigned.IsFloat;
            RngFormSequence sequence;
            bool hasSteps = drawnForm.Sequences.TryGetValue(spot.Key(mode), out sequence) && sequence.FloatSteps.Count > 0;

            if (hasPin || hasSteps) {
                int step = NextEventIndex(spotId);
                if (step < (hasPin ? 1 : 0)) {
                    return ResolvedFloat(assigned.FloatValue, vanillaValue);
                }
                int sequenceStep = step - (hasPin ? 1 : 0);
                if (hasSteps && sequenceStep >= 0 && sequenceStep < sequence.FloatSteps.Count) {
                    return ResolvedFloat(sequence.FloatSteps[sequenceStep], vanillaValue);
                }
                bool loops = hasSteps ? sequence.Loop : true;
                if (loops) {
                    return ResolvedFloat(
                        hasSteps && sequence.FloatSteps.Count > 0
                            ? sequence.FloatSteps[sequence.FloatSteps.Count - 1]
                            : assigned.FloatValue,
                        vanillaValue);
                }
                return vanillaValue;
            }
            // drawn form but this float is unpinned: naturally random within the game's range
            return vanillaValue;
        }

        RngProfile profile = RngProfiles.Active;
        if (profile == null) {
            return vanillaValue;
        }

        RngSpotConfig config = profile.GetSpot(spot.Key(mode));
        if (config == null) {
            return vanillaValue;
        }

        // Float spots are per-event draws: a sequence plays step 1 first, then the rest in
        // order, and the last value either loops forever or hands over to natural randomness.
        if (config.FloatSequence.Count > 0) {
            int step = NextEventIndex(spotId);
            if (step < config.FloatSequence.Count) {
                return ResolvedFloat(config.FloatSequence[step], vanillaValue);
            }
            if (config.LoopSequence) {
                return ResolvedFloat(config.FloatSequence[config.FloatSequence.Count - 1], vanillaValue);
            }
            return vanillaValue;
        }

        if (config.Pool.Count > 0) {
            float total = 0f;
            foreach (RngPoolEntry entry in config.Pool) {
                total += entry.Weight;
            }
            if (total > 0f) {
                float roll = UnityEngine.Random.value * total;
                foreach (RngPoolEntry entry in config.Pool) {
                    roll -= entry.Weight;
                    if (roll <= 0f) {
                        return entry.FloatValue;
                    }
                }
                return config.Pool[config.Pool.Count - 1].FloatValue;
            }
        }

        return vanillaValue;
    }

    private static int PickPool(List<RngPoolEntry> pool, int fallback) {
        if (pool == null || pool.Count == 0) {
            return fallback;
        }

        float total = 0f;
        foreach (RngPoolEntry entry in pool) {
            total += entry.Weight;
        }
        if (total <= 0f) {
            return fallback;
        }

        float roll = UnityEngine.Random.value * total;
        foreach (RngPoolEntry entry in pool) {
            roll -= entry.Weight;
            if (roll <= 0f) {
                return entry.IntValue;
            }
        }
        return pool[pool.Count - 1].IntValue;
    }
}
