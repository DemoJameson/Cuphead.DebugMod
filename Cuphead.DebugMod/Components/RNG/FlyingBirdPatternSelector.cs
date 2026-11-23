using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
public class FlyingBirdPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The phase cycles are decided once per fight when the level's State objects are
    // constructed, and NextPattern only walks an index forward from there. All spots are
    // therefore resolved up-front against the fight's own states, matched by state name
    // because the state lists differ per difficulty (Expert has no Generic phase, Simple
    // has no BirdRevival). NextPattern advances the index before reading it, hence
    // preIncrement.
    [HarmonyPatch(typeof(FlyingBirdLevel), nameof(FlyingBirdLevel.PartialInit))]
    [HarmonyPostfix]
    public static void PhasePatternManipulator(FlyingBirdLevel __instance) {
        LevelProperties.FlyingBird.State[] states = (LevelProperties.FlyingBird.State[])RngSpots.FindField(
            typeof(LevelProperties.FlyingBird), "states").GetValue(__instance.properties);
        if (states == null) {
            return;
        }
        foreach (LevelProperties.FlyingBird.State state in states) {
            if (state.patterns.Length < 2) {
                continue; // single-entry cycles (Whistle) and the empty HouseDeath have no start roll
            }
            string spotId;
            switch (state.stateName) {
                case LevelProperties.FlyingBird.States.Main:
                    spotId = RngSpots.FlyingBirdMainPatternStart;
                    break;
                case LevelProperties.FlyingBird.States.Generic:
                    spotId = RngSpots.FlyingBirdGenericPatternStart;
                    break;
                case LevelProperties.FlyingBird.States.BirdRevival:
                    spotId = RngSpots.FlyingBirdRevivalPatternStart;
                    break;
                default:
                    continue;
            }
            state.patternIndex = RngState.Index(spotId, Mode, state.patternIndex, state.patterns.Length, true);
        }
    }

    // The three movement directions are plain Rand.Bool() rolls made once per fight
    // inside coroutine state machines, so they are pinned by IL replacement. The
    // delegates pass the original roll through RngState untouched when unconfigured -
    // unlike the legacy hooks, no extra Rand.Bool() is consumed, so the vanilla RNG
    // stream is preserved either way.
    [HarmonyPatch(typeof(FlyingBirdLevelBird), nameof(FlyingBirdLevelBird.float_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void FloatDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<goUp>"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(goUp =>
                RngState.Index(RngSpots.FlyingBirdFloatDirection, Mode, goUp ? 0 : 1, 2, false) == 0);
            break;
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevelSmallBird), nameof(FlyingBirdLevelSmallBird.moveY_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void SmallBirdDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Rand::Bool"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(movingDirection =>
                RngState.Index(RngSpots.FlyingBirdBirdDirection, Mode, movingDirection ? 0 : 1, 2, false) == 0);
            ilCursor.Index++; // avoid infinite loops
        }
    }

    [HarmonyPatch(typeof(FlyingBirdLevelBird), nameof(FlyingBirdLevelBird.stretcherMove_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void StretcherDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<movingRight>"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(movingRight =>
                RngState.Index(RngSpots.FlyingBirdStretcherDirection, Mode, movingRight ? 0 : 1, 2, false) == 0);
            break;
        }
    }
}