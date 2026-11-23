using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class FlyingBlimpPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // Sets the leading attack of both Generic blimp phases. Each phase's cycle start is
    // decided once per fight when the level's State objects are constructed, and
    // NextPattern only walks the index forward from there - nothing re-rolls it when a
    // state becomes current. Both spots are therefore resolved up-front against the
    // fight's own states, in Generic order; NextPattern advances the index before
    // reading it, hence preIncrement.
    [HarmonyPatch(typeof(FlyingBlimpLevel), nameof(FlyingBlimpLevel.PartialInit))]
    [HarmonyPostfix]
    private static void PhasePatternManipulator(FlyingBlimpLevel __instance) {
        LevelProperties.FlyingBlimp.State[] states = (LevelProperties.FlyingBlimp.State[])RngSpots.FindField(
            typeof(LevelProperties.FlyingBlimp), "states").GetValue(__instance.properties);
        if (states == null) {
            return;
        }
        int genericIndex = 0;
        foreach (LevelProperties.FlyingBlimp.State state in states) {
            if (state.stateName != LevelProperties.FlyingBlimp.States.Generic) {
                continue;
            }
            if (genericIndex > 1 || state.patterns.Length < 2) {
                return;
            }
            string spotId = genericIndex == 0 ? RngSpots.FlyingBlimpGeneric1PatternStart : RngSpots.FlyingBlimpGeneric2PatternStart;
            state.patternIndex = RngState.Index(spotId, Mode, state.patternIndex, state.patterns.Length, true);
            genericIndex++;
        }
    }

    // The Regular-only constellation phase coin-flips between Sagittarius and Gemini
    // (Rand.Bool, true = Sagittarius in every game version); decided once per fight.
    [HarmonyPatch(typeof(FlyingBlimpLevel), nameof(FlyingBlimpLevel.OnStateChanged))]
    [HarmonyILManipulator]
    private static void ConstellationManipulator(ILContext il) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Rand::Bool"))) {
            cursor.EmitDelegate<Func<bool, bool>>(rolled =>
                RngState.Index(RngSpots.FlyingBlimpConstellation, Mode, rolled ? 0 : 1, 2, false) == 0);
            cursor.Index++;
        }
    }

    // ---- Moon phase (Regular and Expert only; Simple never reaches it) ----

    // Which sequence is picked, and where each starts. stars_cr runs once per fight and
    // rolls, in IL order: the position string, the type string, the type start index and
    // the position start index. The start delegates need the entry count of the string
    // the round actually picked, so the string picks stash their index for them.
    private static int chosenPositionString = -1;
    private static int chosenTypeString = -1;
    private static int chosenUfoString = -1;

    [HarmonyPatch(typeof(FlyingBlimpLevelMoonLady), "stars_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void StarsManipulator(ILContext il) {
        ILCursor cursor = new(il);
        int stringPicks = 0;
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("ArrayExtensions::GetRandom"))) {
            bool positions = stringPicks == 0;
            cursor.EmitDelegate<Func<string, string>>(rolled => {
                string[] strings = RngSpots.FlyingBlimpStarStrings(Mode, positions);
                int vanilla = Array.IndexOf(strings, rolled);
                if (vanilla < 0) {
                    return rolled;
                }
                int pick = RngState.Index(positions ? RngSpots.FlyingBlimpMoonStarPositions : RngSpots.FlyingBlimpMoonStarTypes, Mode, vanilla, strings.Length, false);
                if (positions) {
                    chosenPositionString = pick;
                } else {
                    chosenTypeString = pick;
                }
                return strings[pick];
            });
            stringPicks++;
            cursor.Index++;
        }

        cursor.Index = 0;
        int startPicks = 0;
        while (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            bool typeStart = startPicks == 0;
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                RngState.Index(typeStart ? RngSpots.FlyingBlimpMoonStarTypesStart : RngSpots.FlyingBlimpMoonStarPositionsStart, Mode, rolled,
                    typeStart ? RngSpots.FlyingBlimpStarStartCount(Mode, false, chosenTypeString) : RngSpots.FlyingBlimpStarStartCount(Mode, true, chosenPositionString),
                    false));
            startPicks++;
            cursor.Index++;
        }
    }

    // One UFO sequence and its start index, both re-rolled per attack round.
    [HarmonyPatch(typeof(FlyingBlimpLevelMoonLady), "ufo_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void UfoManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("ArrayExtensions::GetRandom"))) {
            cursor.EmitDelegate<Func<string, string>>(rolled => {
                string[] strings = RngSpots.FlyingBlimpUfoStrings(Mode);
                int vanilla = Array.IndexOf(strings, rolled);
                if (vanilla < 0) {
                    return rolled;
                }
                int pick = RngState.Index(RngSpots.FlyingBlimpMoonUfoPattern, Mode, vanilla, strings.Length, false);
                chosenUfoString = pick;
                return strings[pick];
            });
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(rolled =>
                RngState.Index(RngSpots.FlyingBlimpMoonUfoStart, Mode, rolled, RngSpots.FlyingBlimpUfoStartCount(Mode, chosenUfoString), false));
        }
    }

    // Each star rolls its own horizontal speed.
    [HarmonyPatch(typeof(FlyingBlimpLevelStars), "move_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void StarSpeedManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldflda) && i.Operand.ToString().Contains("speedX"),
                i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(rolled =>
                RngState.Float(RngSpots.FlyingBlimpMoonStarSpeed, Mode, rolled));
        }
    }
}
