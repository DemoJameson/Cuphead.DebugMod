using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// All menu styling lives here. Tweak colors, sizes and spacing without touching layout code.
/// FontName can be set to the name of any Unity Font loaded by the game to override the default pick.
/// </summary>
internal static class MenuTheme {
    // Colors
    public static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.55f);
    public static readonly Color WindowBg = new Color(0.09f, 0.09f, 0.11f, 0.98f);
    public static readonly Color HeaderBg = new Color(0.16f, 0.16f, 0.20f);
    public static readonly Color SidebarBg = new Color(0.12f, 0.12f, 0.15f);
    public static readonly Color RowBg = new Color(0.14f, 0.14f, 0.17f);
    public static readonly Color RowBgAlt = new Color(0.17f, 0.17f, 0.21f);
    public static readonly Color RowBgSelected = new Color(0.32f, 0.26f, 0.11f);   // warm highlight for the expanded form row
    public static readonly Color CardBg = new Color(0.19f, 0.19f, 0.23f);
    public static readonly Color SectionBg = new Color(0.55f, 0.44f, 0.10f);
    public static readonly Color Accent = new Color(1.00f, 0.81f, 0.06f);   // Cuphead gold
    public static readonly Color Text = new Color(0.95f, 0.95f, 0.96f);
    public static readonly Color TextDim = new Color(0.70f, 0.70f, 0.75f);
    public static readonly Color Modified = new Color(1.00f, 0.62f, 0.20f);
    public static readonly Color ButtonBg = new Color(0.24f, 0.24f, 0.30f);
    public static readonly Color ButtonHover = new Color(0.32f, 0.32f, 0.40f);
    public static readonly Color ButtonPressed = new Color(0.18f, 0.18f, 0.22f);

    // Type badges on spot cards: "start" = the random start of a fixed cycle, "per attack"
    // = re-rolled independently every attack, "once" = decided once per fight, "order" =
    // one ordered list picked per fight and then walked.
    public static readonly Color BadgeStart = new Color(0.22f, 0.38f, 0.55f);
    public static readonly Color BadgePerAttack = new Color(0.62f, 0.34f, 0.08f);
    public static readonly Color BadgeOnce = new Color(0.40f, 0.26f, 0.52f);
    public static readonly Color BadgeOrder = new Color(0.14f, 0.44f, 0.34f);

    // Layout. Canvas units are "1080p pixels" of the game's actively painted scene: the
    // canvas scale is the painted height (Camera.main.pixelRect) divided by
    // ReferenceHeight, so fonts and fixed-size widgets scale with it. The window itself
    // is a fraction of the painted area in both axes, so it fills the same share of it on
    // any resolution or aspect ratio (DebugMenuRoot drives both every frame).
    public static readonly float WindowWidthFraction = 0.73f;
    public static readonly float WindowHeightFraction = 0.92f;
    public static readonly float ReferenceHeight = 1080f;
    public static readonly float HeaderHeight = 60f;
    public static readonly float FooterHeight = 40f;
    public static readonly float SidebarWidth = 220f;
    public static readonly float RowHeight = 46f;
    public static readonly float SectionHeight = 38f;
    public static readonly float Padding = 10f;
    public static readonly float ControlSize = 34f;
    public static readonly float CheckSize = 28f;   // checkbox squares
    public static readonly float SliderWidth = 640f;   // float value sliders
    public static readonly float BadgeSize = 26f;   // square type badge on spot cards

    // Text
    public static readonly int FontSizeTitle = 28;
    public static readonly int FontSizeLabel = 21;
    public static readonly int FontSizeSmall = 16;
    public static readonly int FontSizeButton = 20;

    /// <summary>
    /// Use one of the game's own fonts for the menu instead of Arial. The game fonts are
    /// stylized custom-mapped fonts: their glyph table draws the ASCII punctuation/digit
    /// block (0x21-0x3F) with inverted shift state ('!' glyph sits where '1' is expected,
    /// ',' where '<' is expected, ...), so all menu text is routed through MapGlyphs()
    /// which swaps those pairs back. Letters are unaffected.
    /// </summary>
    public static bool UseGameFont = true;

    /// <summary>
    /// Optional: exact name of a Unity Font to use instead of the automatic game font pick.
    /// Empty = the game's handwriting body font (CupheadHenriette on 1.2+; the first loaded
    /// "Cuphead" font on 1.0/1.1). Useful names on 1.2+: "CupheadHenriette_A_merged",
    /// "CupheadMemphis_Medium_merged", "CupheadFelix_Regular_merged".
    /// NOTE: different game fonts may use different glyph tables — if some characters still
    /// render oddly after switching fonts, tune the GlyphRemap constants below.
    /// </summary>
    public static string FontName = "";

    /// <summary>
    /// Per-character glyph remap for the game font. Format: hex pairs "written:sent"
    /// separated by semicolons. The handwriting font's glyph table has a few wrong slots:
    /// the '1' slot contains the '!' artwork (and '<'/',' and '>'/'.' are aliased too),
    /// while the real '1' glyph lives in the '|' (0x7C) slot — hence "31:7C". Determined
    /// with the "Font calibration" section at the bottom of the RNG page; re-verify there
    /// if the resolved font ever changes.
    /// </summary>
    public static string GlyphRemap = "31:7C;27:3A;";

    /// <summary>True when the resolved font is a game font (and MapGlyphs is active).</summary>
    public static bool UsingGameFont {
        get; private set;
    }

    private static Font cachedFont;
    private static Dictionary<char, char> remapTable;
    private static string remapParsedFrom;

    public static Font ResolveFont() {
        if (cachedFont != null) {
            return cachedFont;
        }

        UsingGameFont = false;

        if (UseGameFont) {
            if (!string.IsNullOrEmpty(FontName)) {
                // explicit override: loaded fonts first, then the game's font loader (1.2+)
                Font[] all = Resources.FindObjectsOfTypeAll<Font>();
                if (all != null) {
                    foreach (Font font in all) {
                        if (font != null && font.name == FontName) {
                            cachedFont = font;
                            UsingGameFont = true;
                            return cachedFont;
                        }
                    }
                }

#if !v1_0 && !v1_1
                if (Enum.IsDefined(typeof(FontLoader.FontType), FontName)) {
                    try {
                        FontLoader.FontType type = (FontLoader.FontType)Enum.Parse(typeof(FontLoader.FontType), FontName);
                        Font loaded = FontLoader.GetFont(type);
                        if (loaded != null) {
                            cachedFont = loaded;
                            UsingGameFont = true;
                            return cachedFont;
                        }
                    } catch (Exception) {
                        // fall through to the automatic pick / Arial
                    }
                }
#endif
            } else {
#if !v1_0 && !v1_1
                try {
                    Font loaded = FontLoader.GetFont(FontLoader.FontType.CupheadHenriette_A_merged);
                    if (loaded != null) {
                        cachedFont = loaded;
                        UsingGameFont = true;
                        return cachedFont;
                    }
                } catch (Exception) {
                    // cache not populated; fall through to the scan
                }
#endif
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null) {
                    foreach (Font font in fonts) {
                        if (font != null && font.name.IndexOf("Cuphead", StringComparison.OrdinalIgnoreCase) >= 0) {
                            cachedFont = font;
                            UsingGameFont = true;
                            return cachedFont;
                        }
                    }
                }
            }
        }

        cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return cachedFont;
    }

    /// <summary>
    /// Converts text written as normal ASCII into the codepoints the game font's glyph
    /// table expects, so a human can write text naturally and it still renders correctly.
    /// No-op for Arial (which needs no correction) and when the remap table is empty.
    /// </summary>
    public static string MapGlyphs(string text) {
        if (string.IsNullOrEmpty(text) || !UsingGameFont) {
            return text;
        }

        EnsureRemapTable();
        if (remapTable.Count == 0) {
            return text;
        }

        StringBuilder sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++) {
            char replacement;
            if (remapTable.TryGetValue(text[i], out replacement)) {
                sb.Append(replacement);
            } else {
                sb.Append(text[i]);
            }
        }
        return sb.ToString();
    }

    private static void EnsureRemapTable() {
        if (remapParsedFrom == GlyphRemap) {
            return;
        }
        remapParsedFrom = GlyphRemap;
        remapTable = new Dictionary<char, char>();

        string[] entries = GlyphRemap.Split(new[] { ';' });
        for (int i = 0; i < entries.Length; i++) {
            string entry = entries[i].Trim();
            if (entry.Length == 0) {
                continue;
            }
            int separator = entry.IndexOf(':');
            if (separator <= 0) {
                continue;
            }
            int written;
            int sent;
            if (int.TryParse(entry.Substring(0, separator).Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out written)
                && int.TryParse(entry.Substring(separator + 1).Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out sent)) {
                remapTable[(char)written] = (char)sent;
            }
        }
    }
}
