using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class DragonPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The head's attack cycle is decided once per fight when the level's State objects
    // are constructed, and NextPattern only walks an index forward from there. All
    // spots are therefore resolved up-front against the fight's own states, matched by
    // state name and occurrence (Regular has two Generic states). NextPattern advances
    // the index before reading it, hence preIncrement.
    [HarmonyPatch(typeof(DragonLevel), nameof(DragonLevel.PartialInit))]
    [HarmonyPostfix]
    private static void TailCycleManipulator(DragonLevel __instance) {
        LevelProperties.Dragon.State[] states = (LevelProperties.Dragon.State[])RngSpots.FindField(
            typeof(LevelProperties.Dragon), "states").GetValue(__instance.properties);
        if (states == null) {
            return;
        }
        int generics = 0;
        foreach (LevelProperties.Dragon.State state in states) {
            string spotId;
            if (state.stateName == LevelProperties.Dragon.States.Main) {
                spotId = RngSpots.DragonMainPattern;
            } else if (state.stateName == LevelProperties.Dragon.States.Generic) {
                spotId = generics == 0 ? RngSpots.DragonGeneric1Pattern : RngSpots.DragonGeneric2Pattern;
                generics++;
            } else {
                continue; // FireMarchers/ThreeHeads have no head-attack cycle
            }
            if (state.patterns.Length < 2) {
                continue; // single-entry cycles have no start roll
            }
            state.patternIndex = RngState.Index(spotId, Mode, state.patternIndex, state.patterns.Length, true);
        }
    }

    // The meteor and peashot volley patterns are re-rolled per attack from the current
    // state's rows. The roll is a GetRandom call over a row array, so the array itself
    // is swapped for a one-row array when configured - the game's own body keeps running
    // (no version-drift risk, unlike the legacy wholesale replacements), and the vanilla
    // stream is untouched either way.
    [HarmonyPatch(typeof(DragonLevelDragon), nameof(DragonLevelDragon.peashot_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void PeashotManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("Peashot::patternString") == true)) {
            ilCursor.EmitDelegate<Func<string[], string[]>>(rows => SwapRows(rows, true));
        }
    }

    [HarmonyPatch(typeof(DragonLevelDragon), nameof(DragonLevelDragon.meteor_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void MeteorManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("Meteor::pattern") == true)) {
            ilCursor.EmitDelegate<Func<string[], string[]>>(rows => SwapRows(rows, false));
        }
    }

    // The rows belong to the state that is current when the attack starts, so the spot is
    // resolved from the live fight: Main, the first Generic or the second Generic
    // (Regular only). The later states have no spots.
    private static string[] SwapRows(string[] rows, bool peashot) {
        DragonLevel level = Level.Current as DragonLevel;
        if (level == null || rows == null || rows.Length < 2) {
            return rows;
        }
        LevelProperties.Dragon.State[] states = (LevelProperties.Dragon.State[])RngSpots.FindField(
            typeof(LevelProperties.Dragon), "states").GetValue(level.properties);
        LevelProperties.Dragon.State current = level.properties.CurrentState;
        if (states == null || current == null) {
            return rows;
        }
        int index = -1;
        for (int i = 0; i < states.Length; i++) {
            if (states[i] == current) {
                index = i;
                break;
            }
        }
        if (index < 0) {
            return rows;
        }
        int generics = 0;
        for (int i = 0; i < index; i++) {
            if (states[i].stateName == LevelProperties.Dragon.States.Generic) {
                generics++;
            }
        }
        string spotId;
        if (current.stateName == LevelProperties.Dragon.States.Main) {
            spotId = peashot ? RngSpots.DragonMainPeashot : RngSpots.DragonMainMeteor;
        } else if (current.stateName == LevelProperties.Dragon.States.Generic && generics == 0) {
            spotId = peashot ? RngSpots.DragonGeneric1Peashot : RngSpots.DragonGeneric1Meteor;
        } else if (current.stateName == LevelProperties.Dragon.States.Generic) {
            spotId = peashot ? RngSpots.DragonGeneric2Peashot : RngSpots.DragonGeneric2Meteor;
        } else {
            return rows;
        }
        int pick = RngState.Index(spotId, Mode, -1, rows.Length, false);
        return pick >= 0 ? new[] { rows[pick] } : rows;
    }
}
