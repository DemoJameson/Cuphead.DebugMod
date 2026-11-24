using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class ClownPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The car's dash delay and drift cycles are rolled once when the bumper car phase
    // starts (each index is then consumed directly and walked). The drift index is
    // stored before the delay index in the IL, and both have later loop-tail advance
    // stores that must not be hooked - so the drift is hooked first and the cursor then
    // moves forward to the delay store.
    [HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.bumper_car_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void BumperCarManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("movementIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(movementIndex => RngState.Index(
                RngSpots.ClownBumperCarDriftDirection, Mode, movementIndex, ClownOptionCount(RngSpots.ClownBumperCarDriftDirection, Mode), false));
            break;
        }
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("timerIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(timerIndex => RngState.Index(
                RngSpots.ClownBumperDelay, Mode, timerIndex, ClownOptionCount(RngSpots.ClownBumperDelay, Mode), false));
            break;
        }
    }

    // The duck type row and its start index are pass-through swaps: unconfigured, the
    // game's own picks flow through unchanged.
    [HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.ducks_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void DuckManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand?.ToString().Contains("Duck::duckTypeString") == true)) {
            ilCursor.EmitDelegate<Func<string[], string[]>>(SelectDuckPattern);

            if (ilCursor.TryGotoNext(MoveType.After,
                    i => i.Operand?.ToString().Contains("UnityEngine.Random::Range") == true)) {
                // this is the typeIndex roll - it comes before the position roll
                ilCursor.EmitDelegate<Func<int, int>>(SelectDuckStartIndex);
            }
        }
    }

    // The horse's attack cycle start is rolled into horseTypeIndex and consumed
    // synchronously (StartCarouselHorse starts select_horse_cr, whose first MoveNext
    // reads the index immediately), so the roll's store itself is pinned before it can
    // be consumed.
    [HarmonyPatch(typeof(ClownLevelClownHorse), nameof(ClownLevelClownHorse.StartCarouselHorse))]
    [HarmonyILManipulator]
    private static void HorseTypeManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("horseTypeIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(horseTypeIndex => RngState.Index(
                RngSpots.ClownHorseType, Mode, horseTypeIndex, ClownOptionCount(RngSpots.ClownHorseType, Mode), false));
            break;
        }
    }

    // The horse's side is re-rolled on every attack; the vanilla side is derived from
    // where SelectStartPos left the horse, then re-positioned when a different side is
    // configured.
    [HarmonyPatch(typeof(ClownLevelClownHorse), nameof(ClownLevelClownHorse.SelectStartPos))]
    [HarmonyPostfix]
    private static void HorseSideManipulator(ClownLevelClownHorse __instance) {
        bool vanillaLeft = __instance.startPos.x < 0f;
        int pick = RngState.Index(RngSpots.ClownHorseSide, Mode, vanillaLeft ? 0 : 1, 2, false);
        if (pick == (vanillaLeft ? 0 : 1)) {
            return;
        }
        if (pick == 0) {
            __instance.startPos.x = -640f + __instance.properties.CurrentState.horse.HorseXPosOffset;
            __instance.transform.position = __instance.startPos;
            __instance.transform.SetScale(new float?(-1f), new float?(1f), new float?(1f));
        } else {
            __instance.startPos.x = 640f - __instance.properties.CurrentState.horse.HorseXPosOffset;
            __instance.transform.position = __instance.startPos;
            __instance.transform.SetScale(new float?(1f), new float?(1f), new float?(1f));
        }
    }

    private static string[] SelectDuckPattern(string[] duckPatterns) {
        int pick = RngState.Index(RngSpots.ClownDuckTypePatternString, Mode, -1, duckPatterns.Length, false);
        return pick >= 0 ? new[] { duckPatterns[pick] } : duckPatterns;
    }

    private static int SelectDuckStartIndex(int randomIndex) {
        // the type cycle is 8 entries long on every difficulty
        return RngState.Index(RngSpots.ClownDuckTypePattern, Mode, randomIndex, 8, false);
    }

    private static int ClownOptionCount(string spotId, Level.Mode mode) {
        RngSpot spot = RngSpots.FindForMode(spotId, mode);
        return spot != null && spot.Options != null ? spot.Options(mode).Length : 0;
    }
}
