using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.CupheadDebugMod.Rng;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Backend for one float spot's sequence: where the value list and loop flag live. The
/// simple mode stores them in the spot's own profile config; a form stores them as part
/// of the form. Both drive the exact same sequence UI.
/// </summary>
internal abstract class FloatSequenceBackend {
    public abstract List<float> GetValues();
    public abstract bool GetLoop();
    /// <summary>Writes the full value list (step 1 first); an empty list clears the spot.</summary>
    public abstract void WriteValues(List<float> values, bool loop);
}

/// <summary>Backend for one per-event choice spot's configuration: the standing pin plus
/// the plan steps. Simple mode stores them in the spot config, a form stores them as part
/// of the form; both drive the exact same sequence UI.</summary>
internal abstract class IntSequenceBackend {
    /// <summary>True when the standing value is pinned to one option.</summary>
    public abstract bool HasPin();

    /// <summary>The pinned option index; -1 when the standing value is random.</summary>
    public abstract int GetPin();

    /// <summary>The plan steps that play after the standing value; empty when none. A step
    /// of -1 stands for "this occurrence stays random".</summary>
    public abstract List<int> GetSteps();

    public abstract bool GetLoop();

    /// <summary>Writes the whole configuration; a null pin keeps the standing value random
    /// while the steps still play.</summary>
    public abstract void Write(int? pin, List<int> steps, bool loop);
}

internal sealed class SpotIntBackend : IntSequenceBackend {
    private readonly RngSpot spot;
    private readonly Level.Mode mode;

    public SpotIntBackend(RngSpot spot, Level.Mode mode) {
        this.spot = spot;
        this.mode = mode;
    }

    public override bool HasPin() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null && config.Pool.Count >= 1;
    }

    public override int GetPin() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null && config.Pool.Count >= 1 ? config.Pool[0].IntValue : -1;
    }

    public override List<int> GetSteps() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null ? new List<int>(config.Sequence) : new List<int>();
    }

    public override bool GetLoop() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null && config.LoopSequence;
    }

    public override void Write(int? pin, List<int> steps, bool loop) {
        RngProfiles.SetChoiceValues(spot, mode, pin, steps, loop);
    }
}

internal sealed class FormIntBackend : IntSequenceBackend {
    private readonly string stageId;
    private readonly Level.Mode mode;
    private readonly int formIndex;
    private readonly RngSpot spot;

    public FormIntBackend(string stageId, Level.Mode mode, int formIndex, RngSpot spot) {
        this.stageId = stageId;
        this.mode = mode;
        this.formIndex = formIndex;
        this.spot = spot;
    }

    public override bool HasPin() {
        return GetPin() >= 0;
    }

    public override int GetPin() {
        RngForm form = Form();
        if (form == null) {
            return -1;
        }
        RngFormValue value;
        if (!form.Assignments.TryGetValue(spot.Key(mode), out value) || value.IsFloat) {
            return -1;
        }
        return value.IntValue;
    }

    public override List<int> GetSteps() {
        List<int> steps = new List<int>();
        RngForm form = Form();
        if (form == null) {
            return steps;
        }
        RngFormSequence sequence;
        if (form.Sequences.TryGetValue(spot.Key(mode), out sequence)) {
            steps.AddRange(sequence.IntSteps);
        }
        return steps;
    }

    public override bool GetLoop() {
        RngForm form = Form();
        if (form == null) {
            return true;
        }
        RngFormSequence sequence;
        return form.Sequences.TryGetValue(spot.Key(mode), out sequence) ? sequence.Loop : true;
    }

    public override void Write(int? pin, List<int> steps, bool loop) {
        RngProfiles.SetFormIntValues(stageId, mode, formIndex, spot.Key(mode), pin, steps, loop);
    }

    private RngForm Form() {
        RngFormSet set = RngProfiles.GetFormSet(stageId, mode);
        if (set == null || formIndex < 0 || formIndex >= set.Forms.Count) {
            return null;
        }
        return set.Forms[formIndex];
    }
}

internal sealed class SpotFloatBackend : FloatSequenceBackend {
    private readonly RngSpot spot;
    private readonly Level.Mode mode;

    public SpotFloatBackend(RngSpot spot, Level.Mode mode) {
        this.spot = spot;
        this.mode = mode;
    }

    public override List<float> GetValues() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null ? new List<float>(config.FloatSequence) : new List<float>();
    }

    public override bool GetLoop() {
        RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
        return config != null && config.LoopSequence;
    }

    public override void WriteValues(List<float> values, bool loop) {
        RngProfiles.SetFloatSequence(spot, mode, values, loop);
    }
}

internal sealed class FormFloatBackend : FloatSequenceBackend {
    private readonly string stageId;
    private readonly Level.Mode mode;
    private readonly int formIndex;
    private readonly RngSpot spot;

    public FormFloatBackend(string stageId, Level.Mode mode, int formIndex, RngSpot spot) {
        this.stageId = stageId;
        this.mode = mode;
        this.formIndex = formIndex;
        this.spot = spot;
    }

    public override List<float> GetValues() {
        List<float> values = new List<float>();
        RngForm form = Form();
        if (form == null) {
            return values;
        }
        RngFormValue value;
        if (form.Assignments.TryGetValue(spot.Key(mode), out value) && value.IsFloat) {
            values.Add(value.FloatValue);
        }
        RngFormSequence sequence;
        if (form.Sequences.TryGetValue(spot.Key(mode), out sequence)) {
            values.AddRange(sequence.FloatSteps);
        }
        return values;
    }

    public override bool GetLoop() {
        RngForm form = Form();
        if (form == null) {
            return true;
        }
        RngFormSequence sequence;
        // a bare pin loops forever (the constant behavior) unless a sequence says otherwise
        return form.Sequences.TryGetValue(spot.Key(mode), out sequence) ? sequence.Loop : true;
    }

    public override void WriteValues(List<float> values, bool loop) {
        RngProfiles.SetFormFloatValues(stageId, mode, formIndex, spot.Key(mode), values, loop);
    }

    private RngForm Form() {
        RngFormSet set = RngProfiles.GetFormSet(stageId, mode);
        if (set == null || formIndex < 0 || formIndex >= set.Forms.Count) {
            return null;
        }
        return set.Forms[formIndex];
    }
}

/// <summary>
/// The sequence UI shared by the simple-mode spot rows and the form editors: step rows
/// (sliders for float spots, cyclers for per-event choice spots), the + append button and
/// the loop checkbox that lives on the last entry. Any future change made here
/// automatically applies to both UIs.
/// </summary>
internal static class SequenceUi {
    /// <summary>The current value list, padded with the range default so a just-activated
    /// spot always has a step to edit.</summary>
    public static List<float> PaddedFloatValues(FloatSequenceBackend backend, RngFloatRange range) {
        List<float> values = backend.GetValues();
        if (values.Count == 0) {
            values.Add(range.Default);
        }
        return values;
    }

    /// <summary>Slider for one float step; commits to that step's position on release.
    /// Returns the slider so callers can gray it out for a random (-1) step.</summary>
    public static UiSliderField AddFloatSlider(GameObject row, string name, FloatSequenceBackend backend, RngFloatRange range, int stepIndex, float value, Action refresh, float contentWidth, float placeOffset = 172f) {
        UiSliderField slider = null;
        // stretch between the left checkbox column and the right Repeat?/loop stack so
        // narrow aspect ratios keep everything on one line; capped at the theme width
        float width = Mathf.Clamp(contentWidth - 300f, 320f, MenuTheme.SliderWidth);
        slider = UiSliderField.Create(
            row.transform, name, width, 36f,
            range.Min, range.Max, range.Step, range.Step, 2, value,
            false, null, null,
            drag => {
                // live dragging only moves the visuals; the value is committed on release
            },
            () => {
                if (slider.Value < range.Min - 0.001f || slider.Value > range.Max + 0.001f) {
                    return;
                }
                List<float> values = PaddedFloatValues(backend, range);
                if (stepIndex < values.Count) {
                    values[stepIndex] = slider.Value;
                } else {
                    values.Add(slider.Value);
                }
                backend.WriteValues(values, backend.GetLoop());
                refresh();
            },
            null);
        Place(slider.Root, placeOffset);
        return slider;
    }

    /// <summary>Right-aligned caption for a control (e.g. the "Repeat?" label that names the
    /// loop checkbox on slider and cycler rows).</summary>
    public static Text PlaceRightLabel(GameObject row, string name, string text, float rightOffset, float width) {
        GameObject go = Ui.Create(name, row.transform);
        RectTransform rt = Ui.Rect(go);
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-rightOffset, 0f);
        rt.sizeDelta = new Vector2(width, 24f);
        Text label = Ui.Label(go, text, MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.MiddleRight);
        // never wrap - the caption must stay on one line next to its checkbox
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        return label;
    }

    /// <summary>The first value row of a float spot: slider, a per-value Random checkbox
    /// (checked = this occurrence rolls naturally) and the loop checkbox while it is the
    /// last value in play.</summary>
    public static float BuildFloatFirstRow(RectTransform content, string key, int paritySeed, FloatSequenceBackend backend, RngFloatRange range, Action refresh, float y, float contentWidth) {
        List<float> values = backend.GetValues();
        bool firstRandom = values.Count > 0 && values[0] < 0f;
        float shown = firstRandom ? range.Default : (values.Count > 0 ? values[0] : range.Default);

        GameObject row = Ui.Row(content, key + "_First", paritySeed % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt, y, 46f);
        UiSliderField slider = AddFloatSlider(row, "First", backend, range, 0, shown, refresh, contentWidth);
        slider.SetGrayed(firstRandom);

        // same Random checkbox the sequence steps get: checked = step 1 rolls naturally
        UiCheck randomCheck = UiCheck.Create(row.transform, "Random", MenuTheme.CheckSize, firstRandom, on => {
            List<float> current = backend.GetValues();
            if (current.Count == 0) {
                current.Add(on ? -1f : range.Default);
            } else {
                current[0] = on ? -1f : range.Default;
            }
            backend.WriteValues(current, backend.GetLoop());
            refresh();
        });
        PlaceLeft(randomCheck.Root, 12f);
        Ui.RowLabel(row, "Random", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 46f, 610f);

        // with no further steps, this value is the last configuration in play, so its
        // loop checkbox lives right here
        if (values.Count <= 1) {
            UiCheck loopCheck = UiCheck.Create(row.transform, "Loop", MenuTheme.CheckSize, backend.GetLoop(), on => {
                List<float> current = PaddedFloatValues(backend, range);
                if (current.Count == 0) {
                    current.Add(range.Default);
                }
                backend.WriteValues(current, on);
                refresh();
            });
            Place(loopCheck.Root, 46f);
            PlaceRightLabel(row, "RepeatLabel", "Repeat?", 82f, 90f);
        }

        y += 46f;
        return y;
    }

    /// <summary>Step rows 2..n of a float spot's sequence, each with a slider, a per-step
    /// random checkbox and a remove button; the last one carries the loop checkbox.</summary>
    public static float BuildFloatSteps(RectTransform content, string keyPrefix, int paritySeed, FloatSequenceBackend backend, RngFloatRange range, Action refresh, float y, float contentWidth) {
        List<float> values = backend.GetValues();
        for (int i = 1; i < values.Count; i++) {
            bool isLast = i == values.Count - 1;
            int captured = i;
            float capturedValue = values[i];
            bool isRandom = capturedValue < 0f;

            GameObject stepRow = Ui.Row(content, keyPrefix + "_Step" + i, (paritySeed + i) % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt, y, 46f);
            UiSliderField slider = AddFloatSlider(stepRow, "Step" + i, backend, range, i, isRandom ? range.Default : capturedValue, refresh, contentWidth);
            slider.SetGrayed(isRandom);

            // per-step random checkbox: checked = this occurrence rolls naturally again
            UiCheck randomCheck = UiCheck.Create(stepRow.transform, "Random", MenuTheme.CheckSize, isRandom, on => {
                List<float> current = backend.GetValues();
                if (captured < current.Count) {
                    current[captured] = on ? -1f : range.Default;
                }
                backend.WriteValues(current, backend.GetLoop());
                refresh();
            });
            PlaceLeft(randomCheck.Root, 12f);
            Ui.RowLabel(stepRow, "Random", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 46f, 610f);

            if (isLast) {
                UiCheck loopCheck = UiCheck.Create(stepRow.transform, "Loop", MenuTheme.CheckSize, backend.GetLoop(), on => {
                    backend.WriteValues(backend.GetValues(), on);
                    refresh();
                });
                Place(loopCheck.Root, 46f);
                PlaceRightLabel(stepRow, "RepeatLabel", "Repeat?", 82f, 90f);
            }

            GameObject remove = Ui.TextButton(stepRow.transform, "Remove", "x", 32f, 32f, () => {
                List<float> current = backend.GetValues();
                if (captured < current.Count) {
                    current.RemoveAt(captured);
                }
                backend.WriteValues(current, backend.GetLoop());
                refresh();
            }, MenuTheme.FontSizeSmall);
            Place(remove, 4f);

            y += 46f;
        }
        return y;
    }

    /// <summary>The generic + button a card header uses to append a sequence step.</summary>
    public static GameObject AddHeaderPlusButton(GameObject header, Action onClick, float placeOffset) {
        GameObject add = Ui.TextButton(header.transform, "AddStep", "+", MenuTheme.ControlSize, MenuTheme.ControlSize, onClick, MenuTheme.FontSizeButton);
        Place(add, placeOffset);
        return add;
    }

    /// <summary>Appends one plan step to a per-event choice spot, seeded from the last
    /// configured value. Always usable - a plan can be built while the standing value stays
    /// random.</summary>
    public static void AppendChoiceStep(IntSequenceBackend backend, RngOption[] options, Action refresh) {
        List<int> steps = backend.GetSteps();
        int seed = steps.Count > 0 ? steps[steps.Count - 1] : backend.GetPin();
        if (seed < 0 || seed >= options.Length) {
            seed = 0;
        }
        steps.Add(seed);
        backend.Write(backend.HasPin() ? backend.GetPin() : (int?)null, steps, backend.GetLoop());
        refresh();
    }

    /// <summary>Appends one step to a float spot's sequence, seeded from the last
    /// configured value (the range default when that value is random).</summary>
    public static void AppendFloatStep(FloatSequenceBackend backend, RngFloatRange range, Action refresh) {
        List<float> current = PaddedFloatValues(backend, range);
        float seed = current[current.Count - 1];
        current.Add(seed >= 0f ? seed : range.Default);
        backend.WriteValues(current, backend.GetLoop());
        refresh();
    }

    /// <summary>The standing row of a per-event choice spot: the cycler that forces one
    /// option, a separate Random checkbox, the loop checkbox while no plan exists, and the
    /// option pips underneath. This row is what distinguishes an independently rolled spot
    /// from a list with a random start position.</summary>
    public static float BuildChoiceStanding(RectTransform content, string key, int paritySeed, IntSequenceBackend backend, RngOption[] options, Action refresh, float y) {
        Color background = paritySeed % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt;
        bool hasPin = backend.HasPin();
        int pin = backend.GetPin();
        List<int> steps = backend.GetSteps();

        GameObject row = Ui.Row(content, key + "_Standing", background, y, 40f);

        // separate random checkbox: checked = the game picks naturally on every attack
        UiCheck randomCheck = UiCheck.Create(row.transform, "Random", MenuTheme.CheckSize, !hasPin, on => {
            if (on) {
                backend.Write(null, backend.GetSteps(), backend.GetLoop());
            } else {
                backend.Write(0, backend.GetSteps(), backend.GetLoop());
            }
            refresh();
        });
        PlaceLeft(randomCheck.Root, 12f);
        Ui.RowLabel(row, "Random", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 46f, 610f);

        // the standing cycler: the forced option while the checkbox is off, "random" while
        // it is on; clicking an arrow selects (and thereby unchecks the random box)
        string label = hasPin && pin >= 0 && pin < options.Length ? options[pin].Label : "random";
        UiCycle cycle = UiCycle.Create(row.transform, "Standing", 280f, MenuTheme.ControlSize, label,
            () => CycleChoiceStanding(backend, options, -1, refresh),
            () => CycleChoiceStanding(backend, options, +1, refresh));
        PlaceLeft(cycle.Root, 124f);

        // with no plan steps, the standing value is the last configuration in play, so its
        // loop checkbox lives right here
        if (hasPin && steps.Count == 0) {
            UiCheck loopCheck = UiCheck.Create(row.transform, "Loop", MenuTheme.CheckSize, backend.GetLoop(), on => {
                backend.Write(backend.GetPin(), backend.GetSteps(), on);
                refresh();
            });
            Place(loopCheck.Root, 46f);
            PlaceRightLabel(row, "RepeatLabel", "Repeat?", 82f, 90f);
        }

        y += 40f;

        // option pips: one dot per option under the cycler; the forced one is lit
        y = BuildChoicePips(content, key + "_Pips", background, options, hasPin && pin >= 0 ? pin : -1, y);

        return y;
    }

    /// <summary>One dot per option under a cycler row; the selected option is lit.</summary>
    public static float BuildChoicePips(RectTransform content, string key, Color background, RngOption[] options, int selected, float y) {
        GameObject pipRow = Ui.Row(content, key, background, y, 20f);
        for (int i = 0; i < options.Length; i++) {
            GameObject pip = Ui.Create("Pip" + i, pipRow.transform);
            RectTransform pipRect = Ui.Rect(pip);
            pipRect.anchorMin = new Vector2(0f, 0.5f);
            pipRect.anchorMax = new Vector2(0f, 0.5f);
            pipRect.pivot = new Vector2(0f, 0.5f);
            pipRect.anchoredPosition = new Vector2(124f + i * 18f, 0f);
            pipRect.sizeDelta = new Vector2(11f, 11f);
            Ui.Panel(pip, selected == i ? MenuTheme.Accent : MenuTheme.ButtonBg);
        }
        return y + 20f + 2f;
    }

    /// <summary>Step rows of a per-event choice spot's plan, each a cycler over the spot's
    /// options with a per-step random checkbox, option pips underneath and a remove
    /// button; the last one carries the loop checkbox. Step 1 plays right after the
    /// standing value.</summary>
    public static float BuildChoiceSteps(RectTransform content, string keyPrefix, int paritySeed, IntSequenceBackend backend, RngOption[] options, Action refresh, float y) {
        List<int> steps = backend.GetSteps();
        for (int i = 0; i < steps.Count; i++) {
            bool isLast = i == steps.Count - 1;
            int captured = i;
            int capturedValue = steps[i];
            bool isRandom = capturedValue < 0;
            Color background = (paritySeed + i) % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt;

            GameObject stepRow = Ui.Row(content, keyPrefix + "_Step" + (i + 2), background, y, 40f);

            string initialLabel = capturedValue < 0
                ? "random"
                : (capturedValue < options.Length ? options[capturedValue].Label : "#" + (capturedValue + 1));
            UiCycle cycle = UiCycle.Create(stepRow.transform, "Step", 280f, MenuTheme.ControlSize, initialLabel,
                () => CycleChoiceStep(backend, options, captured, -1, refresh),
                () => CycleChoiceStep(backend, options, captured, +1, refresh));
            PlaceLeft(cycle.Root, 124f);

            // per-step random checkbox: checked = this occurrence rolls naturally again
            UiCheck randomCheck = UiCheck.Create(stepRow.transform, "Random", MenuTheme.CheckSize, isRandom, on => {
                List<int> current = backend.GetSteps();
                if (captured < current.Count) {
                    current[captured] = on ? -1 : 0;
                }
                backend.Write(backend.HasPin() ? backend.GetPin() : (int?)null, current, backend.GetLoop());
                refresh();
            });
            PlaceLeft(randomCheck.Root, 12f);
            Ui.RowLabel(stepRow, "Random", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 46f, 610f);

            if (isLast) {
                UiCheck loopCheck = UiCheck.Create(stepRow.transform, "Loop", MenuTheme.CheckSize, backend.GetLoop(), on => {
                    backend.Write(backend.HasPin() ? backend.GetPin() : (int?)null, backend.GetSteps(), on);
                    refresh();
                });
                Place(loopCheck.Root, 46f);
                PlaceRightLabel(stepRow, "RepeatLabel", "Repeat?", 82f, 90f);
            }

            GameObject remove = Ui.TextButton(stepRow.transform, "Remove", "x", 32f, 32f, () => {
                List<int> current = backend.GetSteps();
                if (captured < current.Count) {
                    current.RemoveAt(captured);
                }
                backend.Write(backend.HasPin() ? backend.GetPin() : (int?)null, current, backend.GetLoop());
                refresh();
            }, MenuTheme.FontSizeSmall);
            Place(remove, 4f);

            y += 40f + 2f;

            // option pips under this step's cycler: the selected option is lit
            y = BuildChoicePips(content, keyPrefix + "_Step" + (i + 2) + "_Pips", background, options, capturedValue, y);
        }
        return y;
    }

    private static void CycleChoiceStanding(IntSequenceBackend backend, RngOption[] options, int direction, Action refresh) {
        int count = options.Length;
        if (count == 0) {
            return;
        }
        int current = backend.GetPin();
        int next = current < 0 || current >= count
            ? (direction > 0 ? 0 : count - 1)
            : ((current + direction) % count + count) % count;
        backend.Write(next, backend.GetSteps(), backend.GetLoop());
        refresh();
    }

    private static void CycleChoiceStep(IntSequenceBackend backend, RngOption[] options, int stepIndex, int direction, Action refresh) {
        List<int> steps = backend.GetSteps();
        if (stepIndex < 0 || stepIndex >= steps.Count) {
            return;
        }
        int count = options.Length;
        int current = steps[stepIndex];
        int next = current < 0 || current >= count
            ? (direction > 0 ? 0 : count - 1)
            : ((current + direction) % count + count) % count;
        steps[stepIndex] = next;
        backend.Write(backend.HasPin() ? backend.GetPin() : (int?)null, steps, backend.GetLoop());
        refresh();
    }

    private static void Place(GameObject go, float offset) {
        RectTransform rt = Ui.Rect(go);
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-offset, 0f);
    }

    private static void PlaceLeft(GameObject go, float leftOffset) {
        RectTransform rt = Ui.Rect(go);
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(leftOffset, 0f);
    }
}
