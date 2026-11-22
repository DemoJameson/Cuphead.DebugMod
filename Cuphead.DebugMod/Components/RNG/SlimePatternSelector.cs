using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class SlimePatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The game rolls two things per round of jumps (once per jump_cr call), both
    // independent:
    //   1. the number of jumps this round lasts (all difficulties roll MinMax(5,8) with
    //      the exclusive int range, so only 5, 6 or 7 are possible)
    //   2. a fresh start position inside the fixed jump cycle, consumed as-is
    // Both spots are configured per difficulty and play their plans one step per round.
    // The big slime's opening jump count is its own fixed value, not this roll.
    [HarmonyPatch(typeof(SlimeLevelSlime), "jump_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void JumpManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("Jump::numJumps"),
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomInt"))) {
            cursor.EmitDelegate<Func<int, int>>(randomCount => {
                // options are zero-based (5/6/7), the game assigns the roll straight to the
                // jump counter, so the picked value has to shift back into that range
                return RngState.Index(RngSpots.SlimeJumpCount, Mode, randomCount - 5, 3, false) + 5;
            });
        }

        cursor.Index = 0;
        if (cursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            cursor.EmitDelegate<Func<int, int>>(randomIndex =>
                // jump_cr reads pattern[i] before its own (i + 1) % length increment, so the
                // rolled start is consumed as-is - no pre-increment compensation. The spot is
                // per-event: the roll happens once per round of jumps, and a configured plan
                // plays one step per round.
                RngState.Index(RngSpots.SlimeJumpPatternStart, Mode, randomIndex, SlimeJumpOptionCount(Mode), false));
        }
    }

    private static int SlimeJumpOptionCount(Level.Mode mode) {
        RngSpot spot = RngSpots.Find(RngSpots.SlimeJumpPatternStart);
        return spot != null && spot.Options != null ? spot.Options(mode).Length : 0;
    }
}
