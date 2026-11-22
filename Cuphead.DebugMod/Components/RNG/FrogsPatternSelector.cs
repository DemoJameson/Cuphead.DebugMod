using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class FrogsPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // Sets the leading attack. The game consumes its pattern index with a pre-increment
    // (AbstractLevelState.NextPattern advances before using it), so the spot resolves
    // with preIncrement=true.
    [HarmonyPatch(typeof(FrogsLevel), nameof(FrogsLevel.Start))]
    [HarmonyPrefix]
    public static void PhaseOnePatternManipulator(ref FrogsLevel __instance) {
        LevelProperties.Frogs.State state = __instance.properties.CurrentState;
        state.patternIndex = RngState.Index(RngSpots.FrogsPhase1PatternStart, Mode, state.patternIndex, state.patterns.Length, true);
    }

    // Every fireflies volley rolls one fly sequence index from the level's own pattern
    // list; that index is resolved against the Fireflies Pattern spot before the game
    // indexes the list with it.
    [HarmonyPatch(typeof(FrogsLevelTall), nameof(FrogsLevelTall.fireflies_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneFirefliesPatternManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolledIndex =>
                RngState.Index(RngSpots.FrogsPhase1FirefliesPattern, Mode, rolledIndex, RngSpots.FrogsFirefliesStrings(Mode).Length, false));
        }
    }

    // The slot machine rolls one attack per spin (0=Snakes, 1=Tigers, 2=Bulls). The
    // charm-triggered demon (Oni, mode 3) branch rolls nothing and passes through.
    [HarmonyPatch(typeof(FrogsLevelMorphed), nameof(FrogsLevelMorphed.pattern_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseFinalSlotModeManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolledMode => {
                int pick = RngState.Index(RngSpots.FrogsFinalSlotMode, Mode, rolledMode, 3, false);
                // the Bulls laser plan restarts every time a Bulls attack begins
                if (pick == (int)Slots.Mode.Bison) {
                    RngState.ResetEventCounter(RngSpots.FrogsFinalBisonLaserSide);
                }
                return pick;
            });
        }
    }

    // Every bison laser rolls its Up/Down side independently. This is the coroutine's
    // first Random.Range, so the demon branch's own rolls further down stay vanilla.
    [HarmonyPatch(typeof(FrogsLevelMorphed), "bison_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseFinalBisonLaserManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolledSide =>
                RngState.Index(RngSpots.FrogsFinalBisonLaserSide, Mode, rolledSide, 2, false));
        }
    }
}
