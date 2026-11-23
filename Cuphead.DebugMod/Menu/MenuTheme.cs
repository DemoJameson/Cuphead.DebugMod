using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    public static readonly Color ButtonBg = new Color(0.24f, 0.24f, 0.30f);

    // Type badges on spot cards: "start" = the random start of a fixed cycle, "per attack"
    // = re-rolled independently every attack, "once" = decided once per fight, "order" =
    // one ordered list picked per fight and then walked, "cycle" = a fresh start position
    // into a fixed cycle on every round of the attack, "restricted" = a start the game can
    // only roll for part of the list.
    public static readonly Color BadgeStart = new Color(0.22f, 0.38f, 0.55f);
    public static readonly Color BadgePerAttack = new Color(0.62f, 0.34f, 0.08f);
    public static readonly Color BadgeOnce = new Color(0.40f, 0.26f, 0.52f);
    public static readonly Color BadgeOrder = new Color(0.14f, 0.44f, 0.34f);
    public static readonly Color BadgeCycle = new Color(0.65f, 0.18f, 0.18f);
    public static readonly Color BadgeSequence = new Color(0.72f, 0.28f, 0.55f);
    public static readonly Color BadgeRestricted = new Color(0.45f, 0.45f, 0.50f);

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
    public static readonly float RadioSeparatorWidth = 28f;   // gap between multi-column radio label columns (the "/" column)

    // Text
    public static readonly int FontSizeTitle = 28;
    public static readonly int FontSizeLabel = 21;
    public static readonly int FontSizeSmall = 16;
    public static readonly int FontSizeButton = 20;

    /// <summary>
    /// Use one of the game's own fonts for the menu instead of Arial. The game ships a
    /// whole font family (see FontLoader.FontType on 1.2+); pick one with FontName below.
    /// </summary>
    public static bool UseGameFont = true;

    /// <summary>
    /// Prefer the bundled font (Fonts/Bundles/*.assetbundle, see Fonts/README.md) over the
    /// game's own fonts. When no bundle is embedded for the running game version the menu
    /// falls back to the game-font logic below.
    /// </summary>
    public static bool UseBundledFont = true;

    /// <summary>
    /// Exact name of the Unity Font to use, as it exists in memory (e.g.
    /// "CupheadMemphis-Medium", "CupheadVogue-Bold", "CupheadHenriette-A"). The menu needs
    /// a font whose glyph table draws ASCII normally: CupheadMemphis and CupheadVogue do,
    /// while CupheadHenriette (the game's handwriting font) draws '!' in the '1' slot and
    /// aliases '<'/',' and '>'/'.', so it is not usable here without a character remap.
    /// Empty = the automatic pick (the first loaded "Cuphead" font on 1.0/1.1, the
    /// handwriting font on 1.2+).
    /// </summary>
    //public static string FontName = "CupheadVogue-Bold";
    public static string FontName = "CupheadMemphis-Medium";

    /// <summary>
    private static Font cachedFont;
    private static Font bundledFont;
    private static bool bundledFontAttempted;

    public static Font ResolveFont() {
        if (cachedFont != null) {
            return cachedFont;
        }

        if (UseBundledFont) {
            Font font = TryLoadBundledFont();
            if (font != null) {
                cachedFont = font;
                return cachedFont;
            }
        }

        if (UseGameFont) {
            if (!string.IsNullOrEmpty(FontName)) {
                // explicit override: loaded fonts first, then the game's font loader (1.2+)
                Font[] all = Resources.FindObjectsOfTypeAll<Font>();
                if (all != null) {
                    foreach (Font font in all) {
                        if (font != null && font.name == FontName) {
                            cachedFont = font;
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
                    Font loaded = FontLoader.GetFont(FontLoader.FontType.CupheadMemphis_Medium_merged);
                    if (loaded != null) {
                        cachedFont = loaded;
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
    /// Loads the menu font from the embedded Fonts/Bundles/*.assetbundle matching the
    /// running game version's engine (5_6 for 1.0/1.1, 2017_4 for 1.2/1.3+; see
    /// Fonts/README.md for how the bundles are built). Attempted only once; when no
    /// bundle is embedded for this build the game-font logic takes over unchanged.
    /// </summary>
    private static Font TryLoadBundledFont() {
        if (bundledFontAttempted) {
            return bundledFont;
        }
        bundledFontAttempted = true;

        try {
#if !v1_0 && !v1_1
            string suffix = "2017_4";
#else
            string suffix = "5_6";
#endif
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames()) {
                if (name.IndexOf("Fonts.Bundles", StringComparison.OrdinalIgnoreCase) >= 0
                    && name.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) >= 0
                    && name.EndsWith(".assetbundle", StringComparison.OrdinalIgnoreCase)) {
                    resourceName = name;
                    break;
                }
            }
            if (resourceName == null) {
                return null;
            }

            byte[] bytes;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                if (stream == null) {
                    return null;
                }
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }

            AssetBundle bundle = AssetBundle.LoadFromMemory(bytes);
            if (bundle == null) {
                return null;
            }
            Font[] fonts = bundle.LoadAllAssets<Font>();
            bundle.Unload(false);
            if (fonts != null && fonts.Length > 0 && fonts[0] != null) {
                bundledFont = fonts[0];
                Debug.Log("Cuphead.DebugMod: bundled menu font loaded from " + resourceName);
            }
        } catch (Exception) {
            // no bundle, or a bundle that does not load cleanly; keep the fallback
        }

        return bundledFont;
    }
}
