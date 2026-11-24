using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class MousePatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // Act 1's attack cycle is decided once per fight when the level's State objects are
    // constructed, and NextPattern only walks an index forward from there - so the start
    // is pinned up-front (preIncrement). The later phases' cycles are not covered by the
    // legacy settings and stay vanilla.
    [HarmonyPatch(typeof(MouseLevel), nameof(MouseLevel.PartialInit))]
    [HarmonyPostfix]
    private static void PhaseOnePatternManipulator(MouseLevel __instance) {
        LevelProperties.Mouse.State[] states = (LevelProperties.Mouse.State[])RngSpots.FindField(
            typeof(LevelProperties.Mouse), "states").GetValue(__instance.properties);
        if (states == null || states.Length < 1 || states[0].patterns.Length < 2) {
            return;
        }
        states[0].patternIndex = RngState.Index(
            RngSpots.MouseMainPattern, Mode, states[0].patternIndex, states[0].patterns.Length, true);
    }

    // The cherry bomb volley pattern is re-rolled once per cherry bomb attack and
    // consumed as-is (the attack walks the pattern's delays and drops), so no
    // pre-increment. The rolled index is replaced in place.
    [HarmonyPatch(typeof(MouseLevelCanMouse), "cherryBomb_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CherryBombManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("CanCherryBomb::patterns") == true) &&
            cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Call && i.Operand?.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)") == true)) {
            cursor.EmitDelegate<Func<int, int>>(randomIndex => RngState.Index(
                RngSpots.MouseCherryBomb, Mode, randomIndex, MouseCherryBombCount(Mode), false));
        }
    }

    // The catapult volley pattern is re-rolled per volley (GetRandom over the rows), so
    // the row array is swapped for a one-row array when configured - the game's own body
    // keeps running.
    [HarmonyPatch(typeof(MouseLevelCanMouse), "FireCatapult")]
    [HarmonyILManipulator]
    private static void CatapultManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("CanCatapult::patterns") == true)) {
            cursor.EmitDelegate<Func<string[], string[]>>(rows => {
                int pick = RngState.Index(RngSpots.MouseCatapult, Mode, -1, MouseCatapultCount(Mode), false);
                return pick >= 0 ? new[] { rows[pick] } : rows;
            });
        }
    }

    // Both cans re-roll their maximum wander distance on every forward pass.
    [HarmonyPatch(typeof(MouseLevelCanMouse), "move_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CanMoveMaxXPositionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("CanMove::maxXPositionRange") == true,
                i => i.OpCode == OpCodes.Callvirt && i.Operand?.ToString().Contains("MinMax::RandomFloat") == true)) {
            cursor.EmitDelegate<Func<float, float>>(randomPosition => RngState.Float(
                RngSpots.MouseMoveMaxX, Mode, randomPosition));
        }
    }

    [HarmonyPatch(typeof(MouseLevelBrokenCanMouse), "move_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void BrokenCanMoveMaxXPositionManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                   i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("BrokenCanMove::maxXPositionRange") == true,
                   i => i.OpCode == OpCodes.Callvirt && i.Operand?.ToString().Contains("MinMax::RandomFloat") == true)) {
            cursor.EmitDelegate<Func<float, float>>(randomPosition => RngState.Float(
                RngSpots.MouseBrokenMoveMaxX, Mode, randomPosition));
            cursor.Index++; // avoid infinite loops
        }
    }

    private static int MouseCherryBombCount(Level.Mode mode) {
        RngSpot spot = RngSpots.FindForMode(RngSpots.MouseCherryBomb, mode);
        return spot != null && spot.Options != null ? spot.Options(mode).Length : 0;
    }

    private static int MouseCatapultCount(Level.Mode mode) {
        RngSpot spot = RngSpots.FindForMode(RngSpots.MouseCatapult, mode);
        return spot != null && spot.Options != null ? spot.Options(mode).Length : 0;
    }
}
