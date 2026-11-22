using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.CupheadDebugMod.Config;
using BepInEx.CupheadDebugMod.Rng;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// The RNG page of the debug menu. Renders one card/row per spot from the catalog and
/// edits the active profile live. Layout is fully manual (no layout groups) so it behaves
/// the same on Unity 5.6 and 2017.4.
/// </summary>
internal sealed class RngPage {
    private const float Gap = 6f;

    private readonly RectTransform content;
    private readonly Text profileLabel;
    private readonly Text footerStatus;
    private readonly Button copyButton;
    private CanvasGroup canvasGroup;

    private Level.Mode tab = Level.Mode.Normal;
    private string selectedStage;
    private readonly HashSet<string> expanded = new HashSet<string>();

    /// <summary>The build-time width of the spot rows (window minus sidebar), kept current
    /// by the menu root when the window resizes; fixed-width controls derive from it so
    /// they fit any aspect ratio.</summary>
    public float ContentWidth {
        get; set;
    }

    /// <summary>Slider lock toggles survive page rebuilds; keyed per control ("pool:...", "form:...").</summary>
    private readonly Dictionary<string, bool> lockStates = new Dictionary<string, bool>(StringComparer.Ordinal);
    private readonly HashSet<string> expandedForms = new HashSet<string>(StringComparer.Ordinal);

    public RngPage(RectTransform content, Text profileLabel, Text footerStatus, Button copyButton) {
        this.content = content;
        this.profileLabel = profileLabel;
        this.footerStatus = footerStatus;
        this.copyButton = copyButton;

        canvasGroup = content.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = content.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Refresh() {
        UpdateProfileHeader();
        RebuildContent();
    }

    /// <summary>The stage (fight) this page renders; one boss per page, picked from the sidebar.</summary>
    public string SelectedStage {
        get {
            if (selectedStage == null) {
                selectedStage = RngSpots.All.Length > 0 ? RngSpots.StageOf(RngSpots.All[0].Id) : "";
            }
            return selectedStage;
        }
    }

    /// <summary>Switches the page to another stage. Returns false when it was already shown.</summary>
    public bool SelectStage(string stageId) {
        if (SelectedStage == stageId) {
            return false;
        }
        selectedStage = stageId;
        Refresh();
        return true;
    }

    /// <summary>Refreshes only the profile/footer labels (used after in-place edits).</summary>
    public void RefreshHeader() {
        UpdateProfileHeader();
    }

    /// <summary>Collapses every expanded form whose key starts with the prefix (empty prefix = all).
    /// Called when forms are toggled off so they stay collapsed when toggled back on.</summary>
    public void CollapseForms(string keyPrefix) {
        List<string> matches = new List<string>();
        foreach (string key in expandedForms) {
            if (key.StartsWith(keyPrefix, StringComparison.Ordinal)) {
                matches.Add(key);
            }
        }
        foreach (string key in matches) {
            expandedForms.Remove(key);
        }
    }

    public void CycleProfile(int direction) {
        List<string> names = new List<string>(new[] { RngProfiles.VanillaName, RngProfiles.PerfectName, RngProfiles.CustomName });
        int index = names.IndexOf(RngProfiles.ActiveName);
        if (index < 0) {
            index = 0;
        }
        index = (index + direction + names.Count) % names.Count;
        RngProfiles.SetActive(names[index]);
        Refresh();
    }

    public void CopyToCustom() {
        RngProfiles.CopyBuiltInToCustom();
        Refresh();
    }

    private void UpdateProfileHeader() {
        RngProfile active = RngProfiles.Active;
        profileLabel.text = active != null ? active.Name : "?";

        bool builtIn = active != null && active.BuiltIn;
        copyButton.gameObject.SetActive(builtIn);

        int modified = CountModified(active);
        int formCount = 0;
        int perEventPins = CountPerEventPins(active, ref formCount);

        string status;
        if (active != null && active.Name == RngProfiles.VanillaName) {
            status = "Vanilla - everything unmodified";
        } else if (builtIn) {
            status = "read-only - Copy to Custom to edit";
        } else {
            status = modified == 0 ? "no spots modified" : modified + " spot" + (modified == 1 ? "" : "s") + " modified";
            if (formCount > 0) {
                status += ", " + formCount + " form" + (formCount == 1 ? "" : "s");
            }
        }
        if (perEventPins > 0) {
            status += " - (!) " + perEventPins + " per-event pin" + (perEventPins == 1 ? "" : "s") + " repeat one outcome";
        }
        footerStatus.text = status;

        bool readOnly = active != null && active.ReadOnly;
        canvasGroup.interactable = !readOnly;
        canvasGroup.alpha = readOnly ? 0.65f : 1f;
    }

    private static int CountModified(RngProfile profile) {
        if (profile == null) {
            return 0;
        }
        int count = 0;
        foreach (string key in profile.Spots.Keys) {
            if (RngSpots.FindByStoredKey(key) != null) {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Counts per-event spots configured to force one constant outcome forever (a looping
    /// pin or sequence tail, or a legacy lock). Vanilla re-rolls these per attack, so an
    /// unbroken constant is not something the unmodified game can produce - surfaced in the
    /// footer. Plain pins and non-looping sequences stay unbadged: "first value, then
    /// natural randomness" is vanilla-faithful.
    /// </summary>
    private static int CountPerEventPins(RngProfile profile, ref int formCount) {
        if (profile == null) {
            return 0;
        }

        int pins = 0;
        Level.Mode[] modes = new Level.Mode[] { Level.Mode.Easy, Level.Mode.Normal, Level.Mode.Hard };
        foreach (RngSpot spot in RngSpots.All) {
            if (!spot.PerEvent) {
                continue;
            }
            foreach (Level.Mode mode in modes) {
                RngSpotConfig config;
                if (profile.Spots.TryGetValue(spot.Key(mode), out config) && config != null && config.IsConstantRepeat) {
                    pins++;
                }
            }
        }

        foreach (RngFormSet set in profile.FormSets.Values) {
            if (!set.Enabled) {
                continue;
            }
            foreach (RngForm form in set.Forms) {
                if (!form.Enabled) {
                    continue;
                }
                foreach (string assignmentKey in form.Assignments.Keys) {
                    RngSpot spot = RngSpots.FindByStoredKey(assignmentKey);
                    if (spot != null && spot.PerEvent) {
                        pins++;
                    }
                }
                formCount++;
            }
        }
        return pins;
    }

    private void RebuildContent() {
        float scrollY = content.anchoredPosition.y;

        // a page rebuild destroys the hovered control; drop its tooltip with it
        UiTooltip.Hide();

        for (int i = content.childCount - 1; i >= 0; i--) {
            UnityEngine.Object.DestroyImmediate(content.GetChild(i).gameObject);
        }

        float y = 0f;
        y = AddTabs(y);

        // one stage per page: the stage band doubles as the page header and hosts the
        // stage's Forms strip in advanced mode
        y = AddStage(y, SelectedStage, RngSpots.StageLabel(SelectedStage));

        string lastGroup = null;
        bool advanced = Settings.EnablePatternForms.Value;
        foreach (RngSpot spot in RngSpots.All) {
            if (RngSpots.StageOf(spot.Id) != SelectedStage) {
                continue;
            }
            if (!spot.IsAvailable(tab)) {
                continue;
            }

            RngOption[] options = spot.Options != null ? spot.Options(tab) : null;
            if (options != null && options.Length < 2) {
                continue; // nothing meaningful to control on this difficulty
            }

            if (spot.Group != lastGroup) {
                y = AddPhaseRow(y, spot.Group);
                lastGroup = spot.Group;
            }

            if (advanced) {
                // advanced mode works entirely on forms; every spot is edited inside them
                continue;
            }

            if (spot.CompoundSecondary) {
                continue; // edited inside its primary's compound card
            }

            if (!string.IsNullOrEmpty(spot.CompoundWith)) {
                y = AddCompoundCard(y, spot);
            } else if (spot.Shape == RngShape.Float) {
                y = AddFloatCard(y, spot);
            } else {
                y = AddPoolCard(y, spot, options, spot.Shape == RngShape.StartPool);
            }
        }

        y = AddNote(y, "Other bosses are still set up in the legacy settings window (F1). Per-attack spots show a cycler with a Random checkbox; start-position spots list their options instead.");

        y = AddFontCalibration(y);

        content.sizeDelta = new Vector2(0f, y + 12f);
        content.anchoredPosition = new Vector2(0f, Mathf.Clamp(scrollY, 0f, Mathf.Max(0f, content.sizeDelta.y)));
    }

    private float AddStage(float y, string stageId, string stageLabel) {
        GameObject titleRow = Ui.Row(content, "Section_" + stageId, MenuTheme.SectionBg, y, MenuTheme.SectionHeight);
        Ui.RowLabel(titleRow, stageLabel, MenuTheme.FontSizeLabel, MenuTheme.WindowBg, MenuTheme.Padding, 0f);
        y += MenuTheme.SectionHeight + Gap;

        if (Settings.EnablePatternForms.Value) {
            FormSetCard card = new FormSetCard(this, stageId, tab);
            y = card.Build(content, y);
        }
        return y;
    }

    private float AddPhaseRow(float y, string label) {
        GameObject row = Ui.Row(content, "Phase_" + label, MenuTheme.WindowBg, y, 32f);
        Ui.RowLabel(row, label, MenuTheme.FontSizeSmall, MenuTheme.TextDim, MenuTheme.Padding, 0f);
        return y + 32f + 2f;
    }

    private float AddTabs(float y) {
        GameObject row = Ui.Row(content, "Tabs", MenuTheme.WindowBg, y, 52f);
        Ui.RowLabel(row, "Difficulty", MenuTheme.FontSizeLabel, MenuTheme.TextDim, MenuTheme.Padding, 330f);

        float right = MenuTheme.Padding;
        AddTabButton(row, "Expert", Level.Mode.Hard, ref right);
        AddTabButton(row, "Regular", Level.Mode.Normal, ref right);
        AddTabButton(row, "Simple", Level.Mode.Easy, ref right);
        return y + 52f + Gap;
    }

    private void AddTabButton(GameObject row, string label, Level.Mode mode, ref float right) {
        GameObject buttonGo = Ui.Create("Tab_" + label, row.transform);
        RectTransform rt = Ui.Rect(buttonGo);
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-right, 0f);
        rt.sizeDelta = new Vector2(104f, 36f);
        right += 112f;

        bool selected = tab == mode;
        Ui.MakeButton(buttonGo, () => {
            tab = mode;
            Refresh();
        }, selected ? MenuTheme.Accent : MenuTheme.ButtonBg);

        GameObject textGo = Ui.Create("Label", buttonGo.transform);
        Ui.Stretch(textGo);
        Ui.Label(textGo, label, MenuTheme.FontSizeButton, selected ? MenuTheme.WindowBg : MenuTheme.Text, TextAnchor.MiddleCenter);
    }

    private float AddNote(float y, string text) {
        GameObject row = Ui.Row(content, "Note", MenuTheme.WindowBg, y, 52f);
        Ui.RowLabel(row, text, MenuTheme.FontSizeSmall, MenuTheme.TextDim, MenuTheme.Padding, 0f);
        return y + 52f + Gap;
    }

    // ------------------------------------------------------------------
    // Font calibration — determines MenuTheme.GlyphRemap for the active font
    // ------------------------------------------------------------------

    private float AddFontCalibration(float y) {
        bool isExpanded = expanded.Contains("FontCalibration");

        GameObject header = Ui.Row(content, "FontCal", MenuTheme.CardBg, y, MenuTheme.RowHeight);

        GameObject expand = Ui.TextButton(header.transform, "Expand", isExpanded ? "-" : "+", MenuTheme.ControlSize, MenuTheme.ControlSize, () => {
            if (isExpanded) {
                expanded.Remove("FontCalibration");
            } else {
                expanded.Add("FontCalibration");
            }
            Refresh();
        }, MenuTheme.FontSizeButton);
        RectTransform expandRect = Ui.Rect(expand);
        expandRect.anchorMin = new Vector2(0f, 0.5f);
        expandRect.anchorMax = new Vector2(0f, 0.5f);
        expandRect.pivot = new Vector2(0f, 0.5f);
        expandRect.anchoredPosition = new Vector2(6f, 0f);

        Ui.RowLabel(header, "Font calibration", MenuTheme.FontSizeLabel, MenuTheme.Text, 40f, 130f);
        Font gameFont = MenuTheme.ResolveFont();
        GameObject fontNameGo = Ui.Create("FontName", header.transform);
        RectTransform fontNameRect = Ui.Rect(fontNameGo);
        fontNameRect.anchorMin = new Vector2(1f, 0.5f);
        fontNameRect.anchorMax = new Vector2(1f, 0.5f);
        fontNameRect.pivot = new Vector2(1f, 0.5f);
        fontNameRect.anchoredPosition = new Vector2(-MenuTheme.Padding, 0f);
        fontNameRect.sizeDelta = new Vector2(240f, MenuTheme.RowHeight);
        Ui.Label(fontNameGo, gameFont != null ? gameFont.name : "(none)", MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.MiddleRight);

        y += MenuTheme.RowHeight;
        if (!isExpanded) {
            return y + Gap;
        }

        if (gameFont == null) {
            GameObject noFontRow = Ui.Row(content, "FontCal_NoFont", MenuTheme.RowBg, y, 30f);
            Ui.RowLabel(noFontRow, "Menu font is Arial; there is nothing to calibrate.", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
            return y + 30f + Gap;
        }

        GameObject infoRow = Ui.Row(content, "FontCal_Info", MenuTheme.RowBg, y, 34f);
        Ui.RowLabel(infoRow,
            "Each big glyph is one ASCII character drawn in the menu font; underneath are its hex code and an Arial reference. Report every cell that disagrees with its reference as a hex pair for MenuTheme.GlyphRemap (written:sent), e.g. 31:7C. Orange cells share their glyph with another character.",
            MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
        y += 34f;

        // Detect characters that share one glyph (aliasing): the usual source of wrong symbols.
        HashSet<char> aliased = new HashSet<char>();
        try {
            StringBuilder probes = new StringBuilder();
            for (int c = 0x20; c <= 0x7E; c++) {
                probes.Append((char)c);
            }
            gameFont.RequestCharactersInTexture(probes.ToString(), MenuTheme.FontSizeLabel, FontStyle.Normal);

            Dictionary<int, char> firstOwner = new Dictionary<int, char>();
            for (int c = 0x20; c <= 0x7E; c++) {
                CharacterInfo info;
                if (gameFont.GetCharacterInfo((char)c, out info) && info.index > 0) {
                    char first;
                    if (firstOwner.TryGetValue(info.index, out first)) {
                        aliased.Add(first);
                        aliased.Add((char)c);
                    } else {
                        firstOwner[info.index] = (char)c;
                    }
                }
            }
        } catch (Exception) {
            // Dynamic-font metadata unavailable; the user reads the cells visually instead.
        }

        Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        const int columns = 8;
        // cells shrink on narrow aspect ratios so the grid never overflows the page
        float cellWidth = Mathf.Min(100f, Mathf.Max(60f, (ContentWidth - 2f * MenuTheme.Padding) / columns));
        const float cellHeight = 66f;

        for (int c = 0x20; c <= 0x7E; c++) {
            int position = c - 0x20;
            int column = position % columns;
            if (column == 0) {
                GameObject row = Ui.Row(content, "FontCal_Row" + (position / columns), (position / columns) % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt, y, cellHeight);
                y += cellHeight;
            }
            float rowTop = y - cellHeight;

            float cellX = MenuTheme.Padding + column * cellWidth;
            GameObject glyphGo = Ui.Create("FontCal_Glyph_" + c, content);
            RectTransform glyphRect = Ui.Rect(glyphGo);
            glyphRect.anchorMin = new Vector2(0f, 1f);
            glyphRect.anchorMax = new Vector2(0f, 1f);
            glyphRect.pivot = new Vector2(0f, 1f);
            glyphRect.anchoredPosition = new Vector2(cellX + 4f, -(rowTop + 6f));
            glyphRect.sizeDelta = new Vector2(cellWidth - 10f, 34f);
            Color glyphColor = aliased.Contains((char)c) ? MenuTheme.Modified : MenuTheme.Text;
            Ui.LabelWithFont(glyphGo, ((char)c).ToString(), gameFont, 26, glyphColor, TextAnchor.MiddleCenter);

            GameObject refGo = Ui.Create("FontCal_Ref_" + c, content);
            RectTransform refRect = Ui.Rect(refGo);
            refRect.anchorMin = new Vector2(0f, 1f);
            refRect.anchorMax = new Vector2(0f, 1f);
            refRect.pivot = new Vector2(0f, 1f);
            refRect.anchoredPosition = new Vector2(cellX + 4f, -(rowTop + 44f));
            refRect.sizeDelta = new Vector2(cellWidth - 10f, 18f);
            string reference = c == 0x20 ? "20 SP" : string.Format(CultureInfo.InvariantCulture, "{0:X2} {1}", c, (char)c);
            Ui.LabelWithFont(refGo, reference, arial, MenuTheme.FontSizeSmall - 1, MenuTheme.TextDim, TextAnchor.MiddleLeft);
        }

        return y + Gap;
    }

    // ------------------------------------------------------------------
    // StartPool cards
    // ------------------------------------------------------------------

    /// <summary>
    /// Renders a StartPool or Choice spot as the shared pattern card, editing the spot's
    /// own profile config (simple mode).
    /// </summary>
    private float AddPoolCard(float y, RngSpot spot, RngOption[] options, bool showPreview) {
        IntSequenceBackend sequenceBackend = SupportsSequences(spot) ? new SpotIntBackend(spot, tab) : null;
        PatternCard card = new PatternCard(this, spot, options, tab, showPreview, new PatternCard.SpotBinding(spot, tab), sequenceBackend, spot.Id);
        return card.Build(content, y);
    }

    /// <summary>
    /// Renders a compound spot (an ordered list plus a "which comes first" pick inside it,
    /// e.g. the pitchfork) as one card editing both of its spots.
    /// </summary>
    private float AddCompoundCard(float y, RngSpot spot) {
        RngSpot secondary = RngSpots.Find(spot.CompoundWith);
        CompoundCard card = new CompoundCard(
            this, spot, spot.Options(tab),
            new PatternCard.SpotBinding(spot, tab),
            new PatternCard.SpotBinding(secondary, tab),
            orderIndex => RngSpots.DevilPitchforkTypeOptions(tab, orderIndex),
            spot.Id);
        return card.Build(content, y);
    }

    private static bool SupportsSequences(RngSpot spot) {
        return spot.PerEvent && spot.Shape == RngShape.Choice;
    }

    private static string PreviewCycle(RngOption[] options, int selected) {
        if (options == null || options.Length == 0) {
            return "-";
        }
        string result;
        if (selected >= 0) {
            result = "starts at " + options[selected].Label;
            int shown = 0;
            for (int k = 1; k < options.Length && shown < 5; k++) {
                result += " - " + options[(selected + k) % options.Length].Label;
                shown++;
            }
        } else {
            result = "cycle: " + options[0].Label;
            int shown = 0;
            for (int k = 1; k < options.Length && shown < 5; k++) {
                result += " - " + options[k].Label;
                shown++;
            }
        }
        if (options.Length > 6) {
            result += " - ...";
        }
        return result;
    }

    // ------------------------------------------------------------------
    // Float cards
    // ------------------------------------------------------------------

    /// <summary>
    /// Renders a float spot as an expandable card, editing the spot's own profile config
    /// (simple mode). Active while at least one value in the list is not random.
    /// </summary>
    private float AddFloatCard(float y, RngSpot spot) {
        FloatCard card = new FloatCard(this, spot, tab, new SpotFloatBackend(spot, tab), spot.Id);
        return card.Build(content, y);
    }

    // ------------------------------------------------------------------
    // helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Small type badge at a spot card's right edge, telling the randomness kinds apart at
    /// a glance: skip-to-start = the random start of a fixed cycle, die = re-rolled
    /// independently every attack, gem = decided once per fight, stacked list = one ordered
    /// list picked per fight and then walked. Hovering shows a tooltip with the full
    /// explanation.
    /// </summary>
    private static float AddSpotBadge(GameObject row, RngSpot spot, float rightOffset) {
        RngBadgeKind kind = spot.Badge ?? (
            spot.Shape == RngShape.StartPool ? RngBadgeKind.Start
            : spot.PerEvent ? RngBadgeKind.PerAttack
            : RngBadgeKind.Once);

        string hint;
        Color background;
        Sprite icon;
        switch (kind) {
            case RngBadgeKind.Start:
                hint = "Random start position - the game rolls where a fixed cycle begins, then repeats it in order.";
                background = MenuTheme.BadgeStart;
                icon = UiIcons.StartMark;
                break;
            case RngBadgeKind.Order:
                hint = "The game picks one attack order once per fight and then walks it. Pin an order, and optionally which attack comes first inside it.";
                background = MenuTheme.BadgeOrder;
                icon = UiIcons.OrderList;
                break;
            case RngBadgeKind.PerAttack:
                hint = "Rolled independently every time this attack fires. A plan of steps can be forced before natural randomness resumes.";
                background = MenuTheme.BadgePerAttack;
                icon = UiIcons.Dice;
                break;
            default:
                hint = "Decided once per fight.";
                background = MenuTheme.BadgeOnce;
                icon = UiIcons.Gem;
                break;
        }

        GameObject badge = Ui.Create("Badge", row.transform);
        RectTransform badgeRect = Ui.Rect(badge);
        badgeRect.anchorMin = new Vector2(1f, 0.5f);
        badgeRect.anchorMax = new Vector2(1f, 0.5f);
        badgeRect.pivot = new Vector2(1f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-rightOffset, 0f);
        badgeRect.sizeDelta = new Vector2(MenuTheme.BadgeSize, MenuTheme.BadgeSize);
        Image badgeImage = Ui.Panel(badge, background);
        badgeImage.raycastTarget = true;
        UiTooltip tooltip = badge.AddComponent<UiTooltip>();
        tooltip.HintText = hint;

        GameObject iconGo = Ui.Create("Icon", badge.transform);
        RectTransform iconRect = Ui.Rect(iconGo);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        float iconSize = MenuTheme.BadgeSize - 8f;
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        Image iconImage = iconGo.AddComponent<Image>();
        iconImage.sprite = icon;
        iconImage.color = MenuTheme.Text;
        iconImage.raycastTarget = false;

        return MenuTheme.BadgeSize;
    }

    private static void PlaceRight(GameObject go, float rightOffset) {
        RectTransform rt = Ui.Rect(go);
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-rightOffset, 0f);
    }

    private static void PlaceLeft(GameObject go, float leftOffset) {
        RectTransform rt = Ui.Rect(go);
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(leftOffset, 0f);
    }

    /// <summary>
    /// The pattern-spot UI shared by both modes: random by default, or one forced value.
    /// Options are always clickable - selecting one pins it (in simple mode via the spot's
    /// profile config, inside a form via that form's assignment), unselecting it goes back
    /// to random - so the interaction is identical everywhere.
    /// </summary>
    private sealed class PatternCard {
        private readonly RngPage page;
        private readonly RngSpot spot;
        private readonly RngOption[] options;
        private readonly Level.Mode mode;
        private readonly bool showPreview;
        private readonly Binding binding;
        private readonly string expansionKey;
        private readonly IntSequenceBackend sequenceBackend;

        /// <summary>Where the card's selection is stored: the spot config, or a form assignment.</summary>
        public abstract class Binding {
            public abstract bool IsConfigured();
            public abstract int Selected();
            public abstract void Set(int index);
        }

        public sealed class SpotBinding : Binding {
            private readonly RngSpot spot;
            private readonly Level.Mode mode;

            public SpotBinding(RngSpot spot, Level.Mode mode) {
                this.spot = spot;
                this.mode = mode;
            }

            public override bool IsConfigured() {
                return RngProfiles.GetSpotConfig(spot, mode) != null;
            }

            public override int Selected() {
                RngSpotConfig config = RngProfiles.GetSpotConfig(spot, mode);
                if (config == null || config.Pool.Count == 0) {
                    return -1;
                }
                return config.Pool[0].IntValue;
            }

            public override void Set(int index) {
                if (index < 0) {
                    RngProfiles.ClearSpot(spot, mode);
                } else {
                    RngProfiles.SetStartPool(spot, mode, new List<RngPoolEntry> { new RngPoolEntry(index, 1f) });
                }
            }
        }

        public sealed class FormBinding : Binding {
            private readonly string stageId;
            private readonly Level.Mode mode;
            private readonly int formIndex;
            private readonly RngSpot spot;

            public FormBinding(string stageId, Level.Mode mode, int formIndex, RngSpot spot) {
                this.stageId = stageId;
                this.mode = mode;
                this.formIndex = formIndex;
                this.spot = spot;
            }

            public override bool IsConfigured() {
                RngForm form = Form();
                return form != null && form.Assignments.ContainsKey(spot.Key(mode));
            }

            public override int Selected() {
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

            public override void Set(int index) {
                RngProfiles.SetFormAssignment(stageId, mode, formIndex, spot.Key(mode), index < 0 ? (RngFormValue?)null : RngFormValue.Int(index));
            }

            private RngForm Form() {
                RngFormSet set = RngProfiles.GetFormSet(stageId, mode);
                if (set == null || formIndex < 0 || formIndex >= set.Forms.Count) {
                    return null;
                }
                return set.Forms[formIndex];
            }
        }

        public PatternCard(RngPage page, RngSpot spot, RngOption[] options, Level.Mode mode, bool showPreview, Binding binding, IntSequenceBackend sequenceBackend, string expansionKey) {
            this.page = page;
            this.spot = spot;
            this.options = options;
            this.mode = mode;
            this.showPreview = showPreview;
            this.binding = binding;
            this.sequenceBackend = sequenceBackend;
            this.expansionKey = expansionKey;
        }

        /// <summary>Per-event choice spots use the per-attack editor (cycler + Random
        /// checkbox + plan steps); the backend decides where everything is stored (spot
        /// config in simple mode, the form itself in forms).</summary>
        private bool SupportsSequence {
            get { return spot.PerEvent && spot.Shape == RngShape.Choice && sequenceBackend != null; }
        }

        public float Build(RectTransform content, float y) {
            bool isConfigured = SupportsSequence ? IsChoiceConfigured() : binding.IsConfigured();
            bool isExpanded = page.expanded.Contains(expansionKey);

            GameObject header = BuildCardHeader(
                content, expansionKey, y, spot, isExpanded, isConfigured,
                () => {
                    if (isExpanded) {
                        page.expanded.Remove(expansionKey);
                    } else {
                        page.expanded.Add(expansionKey);
                    }
                    page.Refresh();
                },
                SupportsSequence ? (Action)(() => SequenceUi.AppendChoiceStep(sequenceBackend, options, page.Refresh)) : null);

            y += MenuTheme.RowHeight;
            if (!isExpanded) {
                return y + Gap;
            }

            if (!string.IsNullOrEmpty(spot.Hint)) {
                GameObject hintRow = Ui.Row(content, "Card_" + expansionKey + "_Hint", MenuTheme.CardBg, y, 30f);
                Ui.RowLabel(hintRow, spot.Hint, MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
                y += 30f;
            }

            if (SupportsSequence) {
                // the per-attack editor: the standing value, the plan steps after it, and
                // the option pips - no radio list, this spot is rolled independently
                y = SequenceUi.BuildChoiceStanding(content, "Card_" + expansionKey, 0, sequenceBackend, options, page.Refresh, y);
                y = SequenceUi.BuildChoiceSteps(content, "Card_" + expansionKey, 0, sequenceBackend, options, page.Refresh, y);
            } else {
                y = BuildRadioBody(content, isConfigured, y);
            }

            return y + Gap;
        }

        /// <summary>The start-position editor: options as radio rows, always clickable -
        /// selecting one pins it, unselecting goes back to random.</summary>
        private float BuildRadioBody(RectTransform content, bool isConfigured, float y) {
            int selected = binding.Selected();
            if (selected >= options.Length) {
                selected = options.Length - 1;
            }

            if (showPreview) {
                GameObject previewRow = Ui.Row(content, "Card_" + expansionKey + "_Preview", MenuTheme.CardBg, y, 28f);
                Ui.RowLabel(previewRow, PreviewCycle(options, selected), MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
                y += 28f;
            }

            if (!isConfigured) {
                GameObject randomRow = Ui.Row(content, "Card_" + expansionKey + "_RandomNote", MenuTheme.RowBg, y, 28f);
                Ui.RowLabel(randomRow, "random - the game picks naturally", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
                y += 28f;
            }

            for (int i = 0; i < options.Length; i++) {
                GameObject row = Ui.Row(content, "Card_" + expansionKey + "_Opt" + i, i % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt, y, 40f);
                int captured = i;

                // radio behavior: only one option can be selected at a time; options are
                // always clickable - selecting one pins it, unselecting it goes back to random
                UiCheck check = UiCheck.Create(row.transform, "Select", MenuTheme.CheckSize, selected == i, on => {
                    binding.Set(on ? captured : -1);
                    page.Refresh();
                });
                RectTransform checkRect = Ui.Rect(check.Root);
                checkRect.anchorMin = new Vector2(0f, 0.5f);
                checkRect.anchorMax = new Vector2(0f, 0.5f);
                checkRect.pivot = new Vector2(0f, 0.5f);
                checkRect.anchoredPosition = new Vector2(12f, 0f);

                GameObject labelGo = Ui.Create("OptLabel", row.transform);
                RectTransform labelRect = Ui.Rect(labelGo);
                labelRect.anchorMin = new Vector2(0f, 0.5f);
                labelRect.anchorMax = new Vector2(1f, 0.5f);
                labelRect.pivot = new Vector2(0f, 0.5f);
                labelRect.offsetMin = new Vector2(46f, 0f);
                labelRect.offsetMax = new Vector2(-MenuTheme.Padding, 0f);
                Ui.Label(labelGo, options[i].Label, MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);

                y += 40f + 2f;
            }

            return y;
        }

        /// <summary>Active while the standing value is pinned or at least one plan step is
        /// not the random sentinel - mirroring the float spots' activity rule.</summary>
        private bool IsChoiceConfigured() {
            if (sequenceBackend.HasPin()) {
                return true;
            }
            foreach (int step in sequenceBackend.GetSteps()) {
                if (step >= 0) {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// The shared card header: expand toggle, spot label, type badge and the optional +
    /// button that appends a sequence step. Used by the pattern cards and the float cards
    /// alike so both card kinds stay visually identical.
    /// </summary>
    private static GameObject BuildCardHeader(RectTransform content, string key, float y, RngSpot spot, bool isExpanded, bool isConfigured, Action toggleExpand, Action addStep) {
        GameObject header = Ui.Row(content, "Card_" + key, MenuTheme.CardBg, y, MenuTheme.RowHeight);

        GameObject expand = Ui.TextButton(header.transform, "Expand", isExpanded ? "-" : "+", MenuTheme.ControlSize, MenuTheme.ControlSize, toggleExpand, MenuTheme.FontSizeButton);
        RectTransform expandRect = Ui.Rect(expand);
        expandRect.anchorMin = new Vector2(0f, 0.5f);
        expandRect.anchorMax = new Vector2(0f, 0.5f);
        expandRect.pivot = new Vector2(0f, 0.5f);
        expandRect.anchoredPosition = new Vector2(6f, 0f);

        // the badge hugs the right edge; the + sits left of it when the spot builds steps
        float badgeRight = MenuTheme.Padding;
        float badgeWidth = AddSpotBadge(header, spot, badgeRight);
        float labelRight = badgeRight + badgeWidth + 8f;
        if (addStep != null) {
            labelRight += MenuTheme.ControlSize + 10f;
            SequenceUi.AddHeaderPlusButton(header, addStep, badgeRight + badgeWidth + 8f);
        }

        GameObject headerLabelGo = Ui.Create("Label", header.transform);
        RectTransform headerLabelRect = Ui.Rect(headerLabelGo);
        headerLabelRect.anchorMin = new Vector2(0f, 0.5f);
        headerLabelRect.anchorMax = new Vector2(1f, 0.5f);
        headerLabelRect.pivot = new Vector2(0f, 0.5f);
        headerLabelRect.offsetMin = new Vector2(40f, 0f);
        headerLabelRect.offsetMax = new Vector2(-labelRight, 0f);
        Ui.Label(headerLabelGo, spot.Label + RandomSuffix(isConfigured), MenuTheme.FontSizeLabel, isConfigured ? MenuTheme.Text : MenuTheme.TextDim, TextAnchor.MiddleLeft);

        return header;
    }

    private static string RandomSuffix(bool isConfigured) {
        // a drawn form is authoritative: an unpinned spot is naturally random, leftover
        // spot configs do not leak in, so there is no inheritance to surface
        return isConfigured ? "" : "  (random)";
    }

    /// <summary>
    /// The float-spot card: the value list lives on its own rows under an expandable
    /// header, mirroring the pattern cards. The first value and every sequence step carry
    /// the same Random checkbox (checked = that occurrence rolls naturally), and the spot
    /// counts as active while at least one value in the list is not random.
    /// </summary>
    private sealed class FloatCard {
        private readonly RngPage page;
        private readonly RngSpot spot;
        private readonly Level.Mode mode;
        private readonly FloatSequenceBackend backend;
        private readonly string expansionKey;

        public FloatCard(RngPage page, RngSpot spot, Level.Mode mode, FloatSequenceBackend backend, string expansionKey) {
            this.page = page;
            this.spot = spot;
            this.mode = mode;
            this.backend = backend;
            this.expansionKey = expansionKey;
        }

        public float Build(RectTransform content, float y) {
            RngFloatRange range = spot.FloatRange(mode);
            bool isConfigured = IsConfigured();
            bool isExpanded = page.expanded.Contains(expansionKey);

            GameObject header = BuildCardHeader(
                content, expansionKey, y, spot, isExpanded, isConfigured,
                () => {
                    if (isExpanded) {
                        page.expanded.Remove(expansionKey);
                    } else {
                        page.expanded.Add(expansionKey);
                    }
                    page.Refresh();
                },
                () => SequenceUi.AppendFloatStep(backend, range, page.Refresh));

            y += MenuTheme.RowHeight;
            if (!isExpanded) {
                return y + Gap;
            }

            if (!string.IsNullOrEmpty(spot.Hint)) {
                GameObject hintRow = Ui.Row(content, "Card_" + expansionKey + "_Hint", MenuTheme.CardBg, y, 30f);
                Ui.RowLabel(hintRow, spot.Hint, MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
                y += 30f;
            }

            y = SequenceUi.BuildFloatFirstRow(content, "Card_" + expansionKey, 0, backend, range, page.Refresh, y, page.ContentWidth);
            y = SequenceUi.BuildFloatSteps(content, "Card_" + expansionKey, 0, backend, range, page.Refresh, y, page.ContentWidth);

            return y + Gap;
        }

        /// <summary>Active while at least one value in the list is not the random sentinel.</summary>
        private bool IsConfigured() {
            foreach (float value in backend.GetValues()) {
                if (value >= 0f) {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Compound card for a spot whose value is an ordered list plus a "which attack comes
    /// first" pick inside it (the pitchfork). Group 1 pins which order (or leaves it to the
    /// game); group 2 pins the first attack, with its options derived from the chosen
    /// order. The second group only unlocks once an order is pinned, and switching to an
    /// order that does not contain the pinned attack resets it to random.
    /// </summary>
    private sealed class CompoundCard {
        private readonly RngPage page;
        private readonly RngSpot primary;
        private readonly PatternCard.Binding orderBinding;
        private readonly PatternCard.Binding typeBinding;
        private readonly RngOption[] orderOptions;
        private readonly Func<int, RngOption[]> typeOptionsFor;
        private readonly string expansionKey;

        public CompoundCard(RngPage page, RngSpot primary, RngOption[] orderOptions, PatternCard.Binding orderBinding, PatternCard.Binding typeBinding, Func<int, RngOption[]> typeOptionsFor, string expansionKey) {
            this.page = page;
            this.primary = primary;
            this.orderBinding = orderBinding;
            this.typeBinding = typeBinding;
            this.orderOptions = orderOptions;
            this.typeOptionsFor = typeOptionsFor;
            this.expansionKey = expansionKey;
        }

        public float Build(RectTransform content, float y) {
            bool isConfigured = orderBinding.IsConfigured() || typeBinding.IsConfigured();
            bool isExpanded = page.expanded.Contains(expansionKey);

            GameObject header = BuildCardHeader(
                content, expansionKey, y, primary, isExpanded, isConfigured,
                () => {
                    if (isExpanded) {
                        page.expanded.Remove(expansionKey);
                    } else {
                        page.expanded.Add(expansionKey);
                    }
                    page.Refresh();
                },
                null);

            y += MenuTheme.RowHeight;
            if (!isExpanded) {
                return y + Gap;
            }

            if (!string.IsNullOrEmpty(primary.Hint)) {
                GameObject hintRow = Ui.Row(content, "Card_" + expansionKey + "_Hint", MenuTheme.CardBg, y, 30f);
                Ui.RowLabel(hintRow, primary.Hint, MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
                y += 30f;
            }

            int selectedOrder = orderBinding.Selected();
            if (selectedOrder >= orderOptions.Length) {
                selectedOrder = -1;
            }

            y = AddGroupLabel(content, expansionKey + "_OrderLabel", "Attack order", y);
            y = AddRadio(content, expansionKey, 0, "Random", -1, selectedOrder, SelectOrder, y);
            for (int i = 0; i < orderOptions.Length; i++) {
                y = AddRadio(content, expansionKey, i + 1, orderOptions[i].Label, orderOptions[i].Value, selectedOrder, SelectOrder, y);
            }

            y = AddGroupLabel(content, expansionKey + "_TypeLabel", "First attack", y);
            if (selectedOrder < 0) {
                // no order pinned: the first attack cannot deviate from the game's own roll
                y = AddRadio(content, expansionKey + "_Type", 0, "Random", -1, -1, SelectType, y);
            } else {
                RngOption[] typeOptions = typeOptionsFor(selectedOrder);
                int selectedType = typeBinding.Selected();
                y = AddRadio(content, expansionKey + "_Type", 0, "Random", -1, selectedType, SelectType, y);
                for (int i = 0; i < typeOptions.Length; i++) {
                    y = AddRadio(content, expansionKey + "_Type", i + 1, typeOptions[i].Label, typeOptions[i].Value, selectedType, SelectType, y);
                }
            }

            return y + Gap;
        }

        private void SelectOrder(int index) {
            orderBinding.Set(index);
            // a freshly chosen order that does not contain the pinned first attack - and a
            // switch back to random - resets the first attack to random
            int selectedType = typeBinding.Selected();
            if (selectedType >= 0 && (index < 0 || !OrderContainsType(index, selectedType))) {
                typeBinding.Set(-1);
            }
            page.Refresh();
        }

        private void SelectType(int index) {
            typeBinding.Set(index);
            page.Refresh();
        }

        private bool OrderContainsType(int orderIndex, int typeValue) {
            RngOption[] typeOptions = typeOptionsFor(orderIndex);
            foreach (RngOption option in typeOptions) {
                if (option.Value == typeValue) {
                    return true;
                }
            }
            return false;
        }

        private float AddGroupLabel(RectTransform content, string key, string label, float y) {
            GameObject row = Ui.Row(content, "Group_" + key, MenuTheme.CardBg, y, 30f);
            Ui.RowLabel(row, label, MenuTheme.FontSizeSmall, MenuTheme.TextDim, 40f, 0f);
            return y + 30f;
        }

        private float AddRadio(RectTransform content, string key, int rowIndex, string label, int value, int selected, Action<int> onSelect, float y) {
            GameObject row = Ui.Row(content, "Card_" + key + "_Opt" + rowIndex, rowIndex % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt, y, 40f);
            int captured = value;

            // radio behavior: one option per group; options are always clickable - selecting
            // one pins it, unselecting it goes back to random
            UiCheck check = UiCheck.Create(row.transform, "Select", MenuTheme.CheckSize, selected == value, on => {
                if (onSelect != null) {
                    onSelect(on ? captured : -1);
                }
            });
            RectTransform checkRect = Ui.Rect(check.Root);
            checkRect.anchorMin = new Vector2(0f, 0.5f);
            checkRect.anchorMax = new Vector2(0f, 0.5f);
            checkRect.pivot = new Vector2(0f, 0.5f);
            checkRect.anchoredPosition = new Vector2(12f, 0f);

            GameObject labelGo = Ui.Create("OptLabel", row.transform);
            RectTransform labelRect = Ui.Rect(labelGo);
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(1f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.offsetMin = new Vector2(46f, 0f);
            labelRect.offsetMax = new Vector2(-MenuTheme.Padding, 0f);
            Ui.Label(labelGo, label, MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);

            return y + 40f + 2f;
        }
    }

    /// <summary>
    /// The Forms UI for one fight on one difficulty (advanced mode only): an on/off strip
    /// plus directly-editable form rows. Each form pins spot values across all phases of
    /// the fight (phases render as subdivisions inside the form); the drawn form (one
    /// weighted pick per fight, see RngState.GetDrawnForm) is authoritative for the whole
    /// fight - its pins apply and unpinned spots are naturally random, with spot configs
    /// from simple mode not leaking in. Forms can be enabled/disabled per row; disabled
    /// forms are excluded from the draw and from weight normalization. Weight editing
    /// keeps the lock + auto-normalize rules (impossible moves show the ghost region).
    /// </summary>
    private sealed class FormSetCard {
        private readonly RngPage page;
        private readonly string stageId;
        private readonly Level.Mode mode;
        private readonly string setKey;
        private readonly List<RngSpot> spots = new List<RngSpot>();
        private readonly Dictionary<RngSpot, RngOption[]> optionsCache = new Dictionary<RngSpot, RngOption[]>();
        private readonly List<FormRow> rows = new List<FormRow>();

        private sealed class FormRow {
            public int Index;
            public UiSliderField Slider;
            public Text NameLabel;
            public Text SummaryLabel;
        }

        public FormSetCard(RngPage page, string stageId, Level.Mode mode) {
            this.page = page;
            this.stageId = stageId;
            this.mode = mode;
            setKey = stageId + "@" + RngSpot.ModeKey(mode);

            foreach (RngSpot spot in RngSpots.All) {
                if (RngSpots.StageOf(spot.Id) != stageId || !spot.IsAvailable(mode)) {
                    continue;
                }
                RngOption[] options = spot.Options != null ? spot.Options(mode) : null;
                if (options != null && options.Length < 2) {
                    continue;
                }
                spots.Add(spot);
                optionsCache[spot] = options;
            }
        }

        public float Build(RectTransform content, float y) {
            RngFormSet set = RngProfiles.GetFormSet(stageId, mode);
            bool hasForms = set != null && set.Forms.Count > 0;

            y = BuildStrip(content, set, hasForms, y);

            if (!hasForms) {
                return y;
            }

            // rows live in their own container so the whole section can be grayed out and
            // locked while the set's toggle is off (the toggle itself stays on the strip)
            GameObject rowsGo = Ui.Create("FormsRows_" + setKey, content);
            RectTransform rowsRect = Ui.Rect(rowsGo);
            rowsRect.anchorMin = new Vector2(0f, 1f);
            rowsRect.anchorMax = new Vector2(1f, 1f);
            rowsRect.pivot = new Vector2(0.5f, 1f);
            rowsRect.anchoredPosition = new Vector2(0f, -y);
            rowsRect.sizeDelta = new Vector2(0f, 0f);
            RectTransform rows = rowsRect;

            float localY = 0f;
            this.rows.Clear();
            for (int i = 0; i < set.Forms.Count; i++) {
                localY = BuildFormRow(rowsRect, set, i, localY);
            }
            for (int i = 0; i < set.Forms.Count; i++) {
                if (page.expandedForms.Contains(setKey + ":" + i)) {
                    localY = BuildAssignments(rowsRect, set, i, localY);
                }
            }
            rowsRect.sizeDelta = new Vector2(0f, localY);

            CanvasGroup group = rowsGo.AddComponent<CanvasGroup>();
            bool setOn = set.Enabled;
            group.alpha = setOn ? 1f : 0.5f;
            group.interactable = setOn;

            return y + localY;
        }

        // ---- strip ----------------------------------------------------------

        private float BuildStrip(RectTransform content, RngFormSet set, bool hasForms, float y) {
            GameObject strip = Ui.Row(content, "FormsStrip_" + setKey, MenuTheme.CardBg, y, 50f);
            Ui.RowLabel(strip, "Forms", MenuTheme.FontSizeLabel, MenuTheme.Text, MenuTheme.Padding, 250f);

            bool setOn = set != null && set.Enabled;
            UiCheck toggle = UiCheck.Create(strip.transform, "Toggle", MenuTheme.CheckSize, setOn, on => {
                if (on) {
                    RngProfiles.GetOrCreateFormSet(stageId, mode);
                    RngFormSet current = RngProfiles.GetFormSet(stageId, mode);
                    if (current != null && current.Forms.Count == 0) {
                        // forms mode always offers at least one form to edit
                        RngProfiles.AddForm(stageId, mode, new RngForm());
                    }
                    RngProfiles.SetFormSetEnabled(stageId, mode, true);
                } else {
                    // collapse every expanded form of this set; they stay collapsed until
                    // the user manually re-expands them after toggling back on
                    page.CollapseForms(setKey + ":");
                    RngProfiles.SetFormSetEnabled(stageId, mode, false);
                }
                page.Refresh();
            });
            PlaceLeft(toggle.Root, MenuTheme.Padding + 62f);

            float right = MenuTheme.Padding;
            if (hasForms) {
                GameObject copyToAll = BuildStripButton(strip, "CopyToAll", "Copy to all", 118f, ref right, () => {
                    RngProfiles.CopyFormSet(stageId, mode, new Level.Mode[] { Level.Mode.Easy, Level.Mode.Normal, Level.Mode.Hard });
                    page.Refresh();
                });
                SetStripButtonEnabled(copyToAll, setOn);
            }
            GameObject addForm = BuildStripButton(strip, "AddForm", "+ Add form", 114f, ref right, () => {
                RngProfiles.AddForm(stageId, mode, new RngForm());
                page.Refresh();
            });
            SetStripButtonEnabled(addForm, setOn);

            return y + 50f + Gap;
        }

        private GameObject BuildStripButton(GameObject strip, string name, string label, float width, ref float right, Action onClick) {
            GameObject button = Ui.TextButton(strip.transform, name, label, width, 36f, onClick, MenuTheme.FontSizeSmall);
            PlaceRight(button, right);
            right += width + 6f;
            return button;
        }

        private static void SetStripButtonEnabled(GameObject button, bool enabled) {
            Button component = button.GetComponent<Button>();
            if (component != null) {
                component.interactable = enabled;
            }
            Transform label = button.transform.Find("Label");
            if (label != null) {
                Text text = label.GetComponent<Text>();
                if (text != null) {
                    text.color = enabled ? MenuTheme.Text : MenuTheme.TextDim;
                }
            }
        }

        // ---- form rows ------------------------------------------------------

        private float BuildFormRow(RectTransform content, RngFormSet set, int index, float y) {
            RngForm form = set.Forms[index];
            int captured = index;
            float[] pct = ComputePct(set);
            bool isExpanded = page.expandedForms.Contains(setKey + ":" + index);
            string lockKey = "form:" + setKey + ":" + index;
            bool locked;
            page.lockStates.TryGetValue(lockKey, out locked);

            // the expanded form's row is highlighted so it is clear which one is open
            Color rowBackground = isExpanded
                ? MenuTheme.RowBgSelected
                : (index % 2 == 0 ? MenuTheme.RowBg : MenuTheme.RowBgAlt);
            GameObject row = Ui.Row(content, "FormRow_" + setKey + "_" + index, rowBackground, y, 62f);

            // per-form enable checkbox: disabled forms are kept but excluded from the draw
            UiCheck enabledCheck = UiCheck.Create(row.transform, "Enabled", MenuTheme.CheckSize, form.Enabled, on => {
                ToggleFormEnabled(set, captured, on);
            });
            RectTransform enabledCheckRect = Ui.Rect(enabledCheck.Root);
            enabledCheckRect.anchorMin = new Vector2(0f, 0.5f);
            enabledCheckRect.anchorMax = new Vector2(0f, 0.5f);
            enabledCheckRect.pivot = new Vector2(0f, 0.5f);
            enabledCheckRect.anchoredPosition = new Vector2(10f, 0f);

            // only one form may be expanded at a time: expanding this one collapses any other
            GameObject expand = Ui.TextButton(row.transform, "Expand", isExpanded ? "-" : "+", 32f, 32f, () => {
                if (isExpanded) {
                    page.expandedForms.Remove(setKey + ":" + index);
                } else {
                    page.CollapseForms("");
                    page.expandedForms.Add(setKey + ":" + index);
                }
                page.Refresh();
            }, MenuTheme.FontSizeButton);
            RectTransform expandRect = Ui.Rect(expand);
            expandRect.anchorMin = new Vector2(0f, 0.5f);
            expandRect.anchorMax = new Vector2(0f, 0.5f);
            expandRect.pivot = new Vector2(0f, 0.5f);
            expandRect.anchoredPosition = new Vector2(44f, 15f);

            GameObject nameGo = Ui.Create("Name", row.transform);
            RectTransform nameRect = Ui.Rect(nameGo);
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.offsetMin = new Vector2(84f, 5f);
            nameRect.offsetMax = new Vector2(-490f, 30f);
            Text nameLabel = Ui.Label(nameGo, form.Name, MenuTheme.FontSizeLabel, MenuTheme.Text, TextAnchor.MiddleLeft);

            GameObject summaryGo = Ui.Create("Summary", row.transform);
            RectTransform summaryRect = Ui.Rect(summaryGo);
            summaryRect.anchorMin = new Vector2(0f, 0.5f);
            summaryRect.anchorMax = new Vector2(1f, 0.5f);
            summaryRect.pivot = new Vector2(0f, 0.5f);
            summaryRect.offsetMin = new Vector2(84f, -30f);
            summaryRect.offsetMax = new Vector2(-490f, -5f);
            Text summaryLabel = Ui.Label(summaryGo, Summarize(form), MenuTheme.FontSizeSmall, MenuTheme.TextDim, TextAnchor.MiddleLeft);

            FormRow rowState = new FormRow();
            rowState.Index = index;
            rowState.NameLabel = nameLabel;
            rowState.SummaryLabel = summaryLabel;
            rowState.Slider = UiSliderField.Create(
                row.transform, "Weight", 340f, 36f,
                0f, 100f, 1f, 5f, 2, pct[index],
                true,
                () => MinPct(set, captured),
                () => MaxPct(set, captured),
                drag => OnWeightDrag(set, captured, drag),
                () => CommitWeights(set),
                isLocked => {
                    page.lockStates[lockKey] = isLocked;
                });
            PlaceRight(rowState.Slider.Root, 124f);
            rowState.Slider.SetGrayed(!form.Enabled);

            GameObject copy = Ui.TextButton(row.transform, "Duplicate", "copy", 64f, 32f, () => DuplicateForm(set, index), MenuTheme.FontSizeSmall);
            PlaceRight(copy, 56f);

            GameObject delete = Ui.TextButton(row.transform, "Delete", "x", 32f, 32f, () => DeleteForm(set, index), MenuTheme.FontSizeSmall);
            PlaceRight(delete, 12f);

            if (!form.Enabled) {
                nameLabel.color = MenuTheme.TextDim;
            }

            rows.Add(rowState);
            return y + 62f + 2f;
        }

        private string Summarize(RngForm form) {
            if (form.Assignments.Count == 0) {
                return "no pins (fully natural)";
            }
            StringBuilder summary = new StringBuilder();
            int shown = 0;
            foreach (KeyValuePair<string, RngFormValue> pair in form.Assignments) {
                if (shown >= 4) {
                    summary.Append(" +").Append(form.Assignments.Count - shown).Append(" more");
                    break;
                }
                if (summary.Length > 0) {
                    summary.Append(" - ");
                }
                RngSpot spot = RngSpots.FindByStoredKey(pair.Key);
                if (spot == null) {
                    summary.Append(pair.Key);
                } else {
                    summary.Append(spot.Label).Append(" = ");
                    if (pair.Value.IsFloat) {
                        summary.Append(pair.Value.FloatValue < 0f ? "random" : pair.Value.FloatValue.ToString("0.00", CultureInfo.InvariantCulture));
                    } else {
                        RngOption[] options;
                        optionsCache.TryGetValue(spot, out options);
                        if (pair.Value.IntValue < 0) {
                            summary.Append("random");
                        } else if (options != null && pair.Value.IntValue < options.Length) {
                            summary.Append(options[pair.Value.IntValue].Label);
                        } else {
                            summary.Append('#').Append(pair.Value.IntValue);
                        }
                    }
                }
                shown++;
            }
            return summary.ToString();
        }

        // ---- per-form assignment editor ------------------------------------

        private float BuildAssignments(RectTransform content, RngFormSet set, int formIndex, float y) {
            RngForm form = set.Forms[formIndex];

            GameObject head = Ui.Row(content, "FormEdit_" + setKey + "_" + formIndex, MenuTheme.CardBg, y, 24f);
            Ui.RowLabel(head, "Pins for this form (unset spots stay naturally random)", MenuTheme.FontSizeSmall, MenuTheme.TextDim, 28f, 0f);
            y += 24f;

            string lastPhase = null;
            for (int s = 0; s < spots.Count; s++) {
                RngSpot spot = spots[s];
                string spotKey = spot.Key(mode);
                RngOption[] options = optionsCache[spot];

                // phases are subdivisions of the form, not separate form sets
                if (spot.Group != lastPhase) {
                    GameObject phaseRow = Ui.Row(content, "FormEdit_" + setKey + "_" + formIndex + "_Phase" + spot.Group, MenuTheme.WindowBg, y, 32f);
                    Ui.RowLabel(phaseRow, spot.Group, MenuTheme.FontSizeSmall, MenuTheme.TextDim, 14f, 0f);
                    y += 32f + 2f;
                    lastPhase = spot.Group;
                }

                if (spot.CompoundSecondary) {
                    continue; // edited inside its primary's compound card
                }

                if (!string.IsNullOrEmpty(spot.CompoundWith)) {
                    // one card for the compound spot and its secondary, both bound to this form
                    RngSpot secondary = RngSpots.Find(spot.CompoundWith);
                    CompoundCard card = new CompoundCard(
                        page, spot, options,
                        new PatternCard.FormBinding(stageId, mode, formIndex, spot),
                        new PatternCard.FormBinding(stageId, mode, formIndex, secondary),
                        orderIndex => RngSpots.DevilPitchforkTypeOptions(mode, orderIndex),
                        "form:" + setKey + ":" + formIndex + ":" + spot.Id);
                    y = card.Build(content, y);
                } else if (spot.Shape == RngShape.Float) {
                    // identical to the simple-mode float card: random, pinned values or a
                    // sequence stored inside this form
                    FormFloatBackend floatBackend = new FormFloatBackend(stageId, mode, formIndex, spot);
                    FloatCard card = new FloatCard(page, spot, mode, floatBackend, "form:" + setKey + ":" + formIndex + ":" + spot.Id);
                    y = card.Build(content, y);
                } else {
                    // the exact same pattern card the simple mode renders, bound to this form
                    IntSequenceBackend sequenceBackend = SupportsSequences(spot) ? new FormIntBackend(stageId, mode, formIndex, spot) : null;
                    PatternCard card = new PatternCard(
                        page, spot, options, mode, spot.Shape == RngShape.StartPool,
                        new PatternCard.FormBinding(stageId, mode, formIndex, spot),
                        sequenceBackend,
                        "form:" + setKey + ":" + formIndex + ":" + spot.Id);
                    y = card.Build(content, y);
                }
            }

            return y + Gap;
        }

        // ---- weight math (enabled forms only; mirrors the old normalization) --

        private float[] ComputePct(RngFormSet set) {
            float[] pct = new float[set.Forms.Count];
            float total = 0f;
            int enabledCount = 0;
            foreach (RngForm form in set.Forms) {
                if (form.Enabled && form.Weight > 0f) {
                    total += form.Weight;
                    enabledCount++;
                }
            }
            for (int i = 0; i < set.Forms.Count; i++) {
                pct[i] = set.Forms[i].Enabled
                    ? (total > 0f ? set.Forms[i].Weight / total * 100f : (enabledCount > 0 ? 100f / enabledCount : 0f))
                    : 0f;
            }
            return pct;
        }

        private bool IsLocked(int index) {
            bool locked;
            page.lockStates.TryGetValue("form:" + setKey + ":" + index, out locked);
            return locked;
        }

        private float MinPct(RngFormSet set, int index) {
            float[] pct = ComputePct(set);
            for (int j = 0; j < set.Forms.Count; j++) {
                if (j != index && set.Forms[j].Enabled && !IsLocked(j) && pct[j] > 0.0001f) {
                    return 0f;
                }
            }
            return pct[index];
        }

        private float MaxPct(RngFormSet set, int index) {
            float[] pct = ComputePct(set);
            float sum = 0f;
            for (int j = 0; j < set.Forms.Count; j++) {
                if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                    sum += pct[j];
                }
            }
            return Mathf.Min(100f, pct[index] + sum);
        }

        private void OnWeightDrag(RngFormSet set, int index, float target) {
            float[] pct = ComputePct(set);
            target = Mathf.Clamp(target, MinPct(set, index), MaxPct(set, index));
            float delta = target - pct[index];
            if (Math.Abs(delta) < 0.0001f) {
                return;
            }

            float unlockedSum = 0f;
            for (int j = 0; j < set.Forms.Count; j++) {
                if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                    unlockedSum += pct[j];
                }
            }
            if (unlockedSum > 0.0001f) {
                float factor = (unlockedSum - delta) / unlockedSum;
                for (int j = 0; j < set.Forms.Count; j++) {
                    if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                        set.Forms[j].Weight = pct[j] * factor;
                    }
                }
            }
            set.Forms[index].Weight = target;
            FixDrift(set, index);
            SyncSliders(set);
        }

        /// <summary>Nudges the largest unlocked enabled row so the set sums to exactly 100%.</summary>
        private void FixDrift(RngFormSet set, int changedIndex) {
            float[] pct = ComputePct(set);
            float total = 0f;
            for (int i = 0; i < set.Forms.Count; i++) {
                total += pct[i];
            }
            float residual = 100f - total;
            if (Math.Abs(residual) < 0.005f) {
                return;
            }
            int best = changedIndex;
            float bestValue = -1f;
            for (int j = 0; j < set.Forms.Count; j++) {
                if (set.Forms[j].Enabled && !IsLocked(j) && j != changedIndex && pct[j] > bestValue) {
                    best = j;
                    bestValue = pct[j];
                }
            }
            if (best >= 0) {
                set.Forms[best].Weight = Mathf.Clamp(pct[best] + residual, 0f, 100f);
            }
        }

        /// <summary>Enabling takes an equal share from the unlocked enabled forms; disabling
        /// hands the row's share back to them proportionally.</summary>
        public void ToggleFormEnabled(RngFormSet set, int index, bool turnOn) {
            float[] pct = ComputePct(set);
            if (turnOn) {
                set.Forms[index].Enabled = true;
                float unlockedSum = 0f;
                int unlockedCount = 0;
                for (int j = 0; j < set.Forms.Count; j++) {
                    if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                        unlockedSum += pct[j];
                        unlockedCount++;
                    }
                }
                if (unlockedCount > 0) {
                    float share = unlockedSum / (unlockedCount + 1);
                    float factor = (unlockedSum - share) / unlockedSum;
                    for (int j = 0; j < set.Forms.Count; j++) {
                        if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                            set.Forms[j].Weight = pct[j] * factor;
                        }
                    }
                    set.Forms[index].Weight = share;
                } else {
                    float lockedSum = 0f;
                    for (int j = 0; j < set.Forms.Count; j++) {
                        if (j != index && set.Forms[j].Enabled && IsLocked(j)) {
                            lockedSum += pct[j];
                        }
                    }
                    set.Forms[index].Weight = Mathf.Max(0f, 100f - lockedSum);
                }
                FixDrift(set, index);
            } else {
                float unlockedSum = 0f;
                for (int j = 0; j < set.Forms.Count; j++) {
                    if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                        unlockedSum += pct[j];
                    }
                }
                if (unlockedSum > 0.0001f) {
                    float factor = (unlockedSum + pct[index]) / unlockedSum;
                    for (int j = 0; j < set.Forms.Count; j++) {
                        if (j != index && set.Forms[j].Enabled && !IsLocked(j)) {
                            set.Forms[j].Weight = pct[j] * factor;
                        }
                    }
                }
                set.Forms[index].Enabled = false;
                set.Forms[index].Weight = 0f;
                FixDrift(set, -1);
            }
            CommitWeights(set);
            page.Refresh();
        }

        private void SyncSliders(RngFormSet set) {
            float[] pct = ComputePct(set);
            foreach (FormRow row in rows) {
                if (row.Index < set.Forms.Count) {
                    row.Slider.SetValue(pct[row.Index]);
                }
            }
        }

        private void CommitWeights(RngFormSet set) {
            float[] pct = ComputePct(set);
            float[] weights = new float[set.Forms.Count];
            for (int i = 0; i < set.Forms.Count; i++) {
                weights[i] = pct[i];
            }
            RngProfiles.SetFormWeights(stageId, mode, weights);
            page.RefreshHeader();
        }

        public void DuplicateForm(RngFormSet set, int index) {
            float[] pct = ComputePct(set);
            RngForm source = set.Forms[index];
            RngForm copy = new RngForm();
            copy.Name = "Form " + (set.Forms.Count + 1);
            copy.Enabled = source.Enabled;
            foreach (KeyValuePair<string, RngFormValue> pair in source.Assignments) {
                copy.Assignments[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, RngFormSequence> pair in source.Sequences) {
                RngFormSequence sequenceCopy = new RngFormSequence();
                sequenceCopy.IntSteps.AddRange(pair.Value.IntSteps);
                sequenceCopy.FloatSteps.AddRange(pair.Value.FloatSteps);
                sequenceCopy.Loop = pair.Value.Loop;
                copy.Sequences[pair.Key] = sequenceCopy;
            }
            int newIndex = RngProfiles.AddForm(stageId, mode, copy);
            if (newIndex < 0) {
                return;
            }

            // split the source's share between the original and the duplicate
            float[] weights = new float[set.Forms.Count];
            for (int i = 0; i < set.Forms.Count; i++) {
                weights[i] = i == index || i == newIndex ? pct[index] * 0.5f : pct[i];
            }
            RngProfiles.SetFormWeights(stageId, mode, weights);
            page.Refresh();
        }

        public void DeleteForm(RngFormSet set, int index) {
            if (index < 0 || index >= set.Forms.Count) {
                return;
            }
            float[] pct = ComputePct(set);
            bool wasEnabled = set.Forms[index].Enabled;
            float removedShare = wasEnabled ? pct[index] : 0f;

            // drop the deleted row's own state first so reindexing below cannot collide with it
            page.expandedForms.Remove(setKey + ":" + index);
            page.lockStates.Remove("form:" + setKey + ":" + index);
            for (int k = index + 1; k < set.Forms.Count; k++) {
                if (page.expandedForms.Contains(setKey + ":" + k)) {
                    page.expandedForms.Remove(setKey + ":" + k);
                    page.expandedForms.Add(setKey + ":" + (k - 1));
                }
                string lockKeyK = "form:" + setKey + ":" + k;
                if (page.lockStates.ContainsKey(lockKeyK)) {
                    page.lockStates.Remove(lockKeyK);
                    page.lockStates["form:" + setKey + ":" + (k - 1)] = true;
                }
            }

            // enabled survivors keep their share; locked ones are fixed, unlocked ones absorb
            // the removed share. Disabled survivors carry no weight anyway.
            float unlockedSum = 0f;
            int unlockedCount = 0;
            for (int j = 0; j < set.Forms.Count; j++) {
                if (j == index || !set.Forms[j].Enabled) {
                    continue;
                }
                if (!IsLocked(j)) {
                    unlockedSum += pct[j];
                    unlockedCount++;
                }
            }

            float[] weights = new float[set.Forms.Count - 1];
            int write = 0;
            for (int j = 0; j < set.Forms.Count; j++) {
                if (j == index) {
                    continue;
                }
                if (!set.Forms[j].Enabled) {
                    weights[write] = 0f;
                } else if (IsLocked(j)) {
                    weights[write] = pct[j];
                } else {
                    weights[write] = unlockedSum > 0.0001f ? pct[j] / unlockedSum * (unlockedSum + removedShare) : removedShare / Mathf.Max(1, unlockedCount);
                }
                write++;
            }

            RngProfiles.DeleteForm(stageId, mode, index);
            if (weights.Length > 0) {
                RngProfiles.SetFormWeights(stageId, mode, weights);
            }
            page.Refresh();
        }
    }
}
