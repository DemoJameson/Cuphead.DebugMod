using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class BeePatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The queen's phase 2 attack cycle is decided once per fight when the level's State
    // objects are constructed, and NextPattern only walks an index forward from there.
    // The cycle start is resolved up-front against the fight's own first Generic state
    // (the later one is the Chain-only finale with no start roll). NextPattern advances
    // the index before reading it, hence preIncrement.
    [HarmonyPatch(typeof(BeeLevel), nameof(BeeLevel.PartialInit))]
    [HarmonyPostfix]
    private static void GenericPatternManipulator(BeeLevel __instance) {
        LevelProperties.Bee.State[] states = (LevelProperties.Bee.State[])RngSpots.FindField(
            typeof(LevelProperties.Bee), "states").GetValue(__instance.properties);
        if (states == null) {
            return;
        }
        foreach (LevelProperties.Bee.State state in states) {
            if (state.stateName != LevelProperties.Bee.States.Generic || state.patterns.Length < 2) {
                continue;
            }
            state.patternIndex = RngState.Index(
                RngSpots.BeeGenericPattern, Mode, state.patternIndex, state.patterns.Length, true);
        }
    }

    // The orb bee and the triangle spell each roll their side once at the top of their
    // attack coroutine (PlusOrMinus sets the queen's mirror), so both are pinned in place
    // with the vanilla roll passed through.
    [HarmonyPatch(typeof(BeeLevelQueen), nameof(BeeLevelQueen.follower_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void OrbsDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("PlusOrMinus"))) {
            ilCursor.EmitDelegate<Func<int, int>>(direction => RngState.Index(
                RngSpots.BeeOrbsDirection, Mode, direction < 0 ? 0 : 1, 2, false) == 0 ? -1 : 1);
            ilCursor.Index++; // avoid infinite loops
        }
    }

    [HarmonyPatch(typeof(BeeLevelQueen), nameof(BeeLevelQueen.triangle_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void TrianglesDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("PlusOrMinus"))) {
            ilCursor.EmitDelegate<Func<int, int>>(direction => RngState.Index(
                RngSpots.BeeTrianglesDirection, Mode, direction < 0 ? 0 : 1, 2, false) == 0 ? -1 : 1);
            ilCursor.Index++; // avoid infinite loops
        }
    }

    // Each platform row rolls its missing platform here, one UnityEngine.Random call per
    // row. The rows the user configures are stored bottom-up (the order they are
    // encountered), while the game generates its groups of 4 rows top-down - the same
    // translation the old BeePlatforms file reader performed, done here by counting the
    // calls and reading the encounter-ordered list at the mirrored position. The counter
    // resets with the fight, exactly like the old reader's pointer.
    static int platformRow;

    [HarmonyPatch(typeof(BeeLevel), nameof(BeeLevel.Start))]
    [HarmonyPrefix]
    private static void ResetPlatformRows() {
        platformRow = 0;
    }

    [HarmonyPatch(typeof(BeeLevelPlatforms), nameof(BeeLevelPlatforms.Randomize))]
    [HarmonyILManipulator]
    private static void PlatformManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random"))) {
            ilCursor.EmitDelegate<Func<int, int>>(platform => PlatformForRow(platform));
            ilCursor.Index++; // avoid infinite loops
        }
    }

    private static int PlatformForRow(int vanilla) {
        RngSpotConfig config = RngProfiles.GetSpotConfig(RngSpots.FindForMode(RngSpots.BeePlatform, Mode), Mode);
        if (config == null || (config.Pool.Count == 0 && config.Sequence.Count == 0)) {
            return vanilla;
        }

        // the encounter position this generation call produces: the first four calls
        // generate rows 4, 3, 2 and 1, then each further group of 4 mirrors the same way
        int e = platformRow < 3 ? 3 - platformRow : platformRow < 4 ? 0 : Permute(platformRow);
        platformRow++;

        bool hasPin = config.Pool.Count == 1 && config.Pool[0].IntValue >= 0;
        int last = config.Sequence.Count > 0
            ? config.Sequence[config.Sequence.Count - 1]
            : hasPin ? config.Pool[0].IntValue : -1;
        int value;
        if (e == 0) {
            value = hasPin ? config.Pool[0].IntValue : -1;
        } else if (e - 1 < config.Sequence.Count) {
            value = config.Sequence[e - 1];
        } else if (config.LoopSequence) {
            value = last;
        } else {
            return vanilla;
        }
        if (value >= 0 && value == 3 && e % 2 == 0) {
            // a repeating 3 reaches rows with only 3 platforms; fall back to the 3rd
            // platform there so no row can select an index out of range
            value = 2;
        }
        return value >= 0 ? value : vanilla;
    }

    private static int Permute(int position) {
        return (position / 4) * 4 + (3 - position % 4);
    }
}
