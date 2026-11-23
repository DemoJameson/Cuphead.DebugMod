using System;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Menu;

/// <summary>
/// Tiny white-on-transparent sprites drawn procedurally so controls never depend on font
/// glyph coverage (game fonts cannot be relied on for symbols). Tint them via Image.color.
/// </summary>
internal static class UiIcons {
    private static Sprite triangleUp;
    private static Sprite triangleDown;
    private static Sprite triangleLeft;
    private static Sprite triangleRight;
    private static Sprite lockClosed;
    private static Sprite lockOpen;
    private static Sprite startMark;
    private static Sprite dice;
    private static Sprite gem;
    private static Sprite orderList;
    private static Sprite cycleRefresh;
    private static Sprite shuffle;
    private static Sprite indexedPattern;

    public static Sprite TriangleUp {
        get { return triangleUp != null ? triangleUp : triangleUp = Build(12, 12, IsTriangleUp); }
    }

    public static Sprite TriangleDown {
        get { return triangleDown != null ? triangleDown : triangleDown = Build(12, 12, IsTriangleDown); }
    }

    public static Sprite LockClosed {
        get { return lockClosed != null ? lockClosed : lockClosed = Build(12, 12, IsLockClosed); }
    }

    public static Sprite LockOpen {
        get { return lockOpen != null ? lockOpen : lockOpen = Build(12, 12, IsLockOpen); }
    }

    public static Sprite TriangleLeft {
        get { return triangleLeft != null ? triangleLeft : triangleLeft = Build(12, 12, IsTriangleLeft); }
    }

    public static Sprite TriangleRight {
        get { return triangleRight != null ? triangleRight : triangleRight = Build(12, 12, IsTriangleRight); }
    }

    // Spot type badges: skip-to-start (a list whose start position is rolled), a five-pip
    // die (re-rolled independently every attack), a gem (decided once per fight) and a
    // stacked list (one ordered list picked per fight and then walked).
    public static Sprite StartMark {
        get { return startMark != null ? startMark : startMark = Build(12, 12, IsStartMark); }
    }

    public static Sprite Dice {
        get { return dice != null ? dice : dice = Build(12, 12, IsDice); }
    }

    public static Sprite Gem {
        get { return gem != null ? gem : gem = Build(12, 12, IsGem); }
    }

    public static Sprite OrderList {
        get { return orderList != null ? orderList : orderList = Build(12, 12, IsOrderList); }
    }

    public static Sprite CycleRefresh {
        get { return cycleRefresh != null ? cycleRefresh : cycleRefresh = Build(12, 12, IsCycleRefresh); }
    }

    public static Sprite Shuffle {
        get { return shuffle != null ? shuffle : shuffle = Build(12, 12, IsShuffle); }
    }

    public static Sprite IndexedPattern {
        get { return indexedPattern != null ? indexedPattern : indexedPattern = Build(12, 12, IsIndexedPattern); }
    }

    private static Sprite Build(int width, int height, Func<int, int, bool> inside) {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                pixels[y * width + x] = inside(x, y) ? Color.white : new Color(0f, 0f, 0f, 0f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static bool IsTriangleUp(int x, int y) {
        // apex at top center, base at bottom; y counts from top
        float halfWidth = (y + 1) * 0.5f;
        return Mathf.Abs(x - 5.5f) <= halfWidth;
    }

    private static bool IsTriangleDown(int x, int y) {
        float halfWidth = (12 - y) * 0.5f;
        return Mathf.Abs(x - 5.5f) <= halfWidth;
    }

    private static bool IsTriangleLeft(int x, int y) {
        // apex at left center, base at right
        float halfHeight = (x + 1) * 0.5f;
        return Mathf.Abs(y - 5.5f) <= halfHeight;
    }

    private static bool IsTriangleRight(int x, int y) {
        // apex at right center, base at left
        float halfHeight = (12 - x) * 0.5f;
        return Mathf.Abs(y - 5.5f) <= halfHeight;
    }

    private static bool IsLockClosed(int x, int y) {
        if (y >= 5 && y <= 10 && x >= 2 && x <= 9) {
            return true; // body
        }
        bool shackleSides = (x >= 3 && x <= 4 || x >= 7 && x <= 8) && y >= 1 && y <= 4;
        bool shackleTop = y == 0 && x >= 3 && x <= 8;
        return shackleSides || shackleTop;
    }

    private static bool IsLockOpen(int x, int y) {
        if (y >= 5 && y <= 10 && x >= 2 && x <= 9) {
            return true; // body
        }
        bool leftBar = x >= 3 && x <= 4 && y >= 1 && y <= 4;
        bool raisedTop = y == 0 && x >= 3 && x <= 6;
        return leftBar || raisedTop;
    }

    private static bool IsStartMark(int x, int y) {
        if (x <= 1 && y >= 1 && y <= 10) {
            return true; // the bar
        }
        // left-pointing triangle whose apex starts right of the bar
        return x >= 3 && x <= 10 && Mathf.Abs(y - 5.5f) <= (x - 2) * 0.5f;
    }

    private static bool IsDice(int x, int y) {
        bool border = x >= 1 && x <= 10 && y >= 1 && y <= 10 && (x == 1 || x == 10 || y == 1 || y == 10);
        bool pips = (x >= 3 && x <= 4 || x >= 7 && x <= 8) && (y >= 3 && y <= 4 || y >= 7 && y <= 8);
        bool center = x >= 5 && x <= 6 && y >= 5 && y <= 6;
        return border || pips || center;
    }

    private static bool IsGem(int x, int y) {
        return Mathf.Abs(x - 5.5f) + Mathf.Abs(y - 5.5f) <= 3.5f;
    }

    private static bool IsOrderList(int x, int y) {
        // three stacked bars, the universal "list" glyph
        return x >= 1 && x <= 10 && (y >= 2 && y <= 3 || y >= 5 && y <= 6 || y >= 8 && y <= 9);
    }

    private static bool IsCycleRefresh(int x, int y) {
        // a ring with a gap at the top plus an arrowhead, the "re-roll/repeat" glyph
        float dx = x - 5.5f;
        float dy = y - 5.5f;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        bool ring = dist >= 3.2f && dist <= 5.0f && !(x >= 4 && x <= 7 && y <= 3);
        bool arrowhead = y >= 1 && y <= 3 && Mathf.Abs(x - 5.5f) <= (y - 0.5f) * 1.2f;
        return ring || arrowhead;
    }

    private static bool IsShuffle(int x, int y) {
        // two crossing diagonals with arrowheads on the right edge, the "re-rolled
        // sequence" glyph
        bool lineA = x >= 1 && x <= 8 && Mathf.Abs(y - (9 - x)) <= 1f;
        bool lineB = x >= 1 && x <= 8 && Mathf.Abs(y - (x + 2)) <= 1f;
        bool headTop = x >= 7 && x <= 10 && y >= 1 && y <= 4 && (x + y) >= 10;
        bool headBottom = x >= 7 && x <= 10 && y >= 7 && y <= 10 && (x - y) >= -3;
        return lineA || lineB || headTop || headBottom;
    }

    private static bool IsIndexedPattern(int x, int y) {
        // a die riding on a list: the per-pass pattern pick over a walked index
        bool dieBorder = x >= 3 && x <= 10 && y >= 0 && y <= 5 && (x == 3 || x == 10 || y == 0 || y == 5);
        bool diePip = x >= 6 && x <= 7 && y >= 2 && y <= 3;
        bool bars = y >= 7 && y <= 11 && (y == 7 || y == 9 || y == 11) && x <= 9;
        return dieBorder || diePip || bars;
    }
}
