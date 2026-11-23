using System;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class FlyingGeniePatternSelector {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The chest picks its attack once per fight when it opens (the chosen attack then
    // loops until the phase ends; the game's own attack pool is never consulted again).
    // When pinned, the pool is reduced to the pinned attack so the vanilla roll lands on
    // it; when unconfigured the pool is untouched.
    [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.StartTreasure))]
    [HarmonyPrefix]
    private static void TreasureManipulator(FlyingGenieLevelGenie __instance) {
        int pick = RngState.Index(RngSpots.FlyingGenieTreasureAttack, Mode, -1, 3, false);
        if (pick < 0) {
            return;
        }
        __instance.treasureAttacks.Clear();
        __instance.treasureAttacks.Add(pick);
    }

    // The sword/gem/scarab parry cycles are rolled once per fight in LevelInit and
    // consumed directly (each projectile reads the index as-is, then the cycle advances),
    // so no pre-increment compensation.
    [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.LevelInit))]
    [HarmonyPostfix]
    private static void ParryPatternManipulator(FlyingGenieLevelGenie __instance, LevelProperties.FlyingGenie properties) {
        __instance.swordPinkIndex = RngState.Index(
            RngSpots.FlyingGenieSwordPinkStart, Mode, __instance.swordPinkIndex, __instance.swordPinkPattern.Length, false);
        __instance.gemPinkIndex = RngState.Index(
            RngSpots.FlyingGenieGemPinkStart, Mode, __instance.gemPinkIndex, __instance.gemPinkPattern.Length, false);
        __instance.sphinxPinkIndex = RngState.Index(
            RngSpots.FlyingGenieSphinxPinkStart, Mode, __instance.sphinxPinkIndex, __instance.sphinxPinkPattern.Length, false);
    }

    // The obelisk phase rolls its start once: a row in obeliskGeniePos and a position
    // inside it (3x9 in all versions and difficulties - verified). The flat 27-entry
    // spot resolves to row = combo / 9 and position = combo % 9; both stores pass the
    // vanilla rolls through untouched when unconfigured.
    [HarmonyPatch(typeof(FlyingGenieLevelGenie), nameof(FlyingGenieLevelGenie.obelisk_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void ObeliskManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("mainObeliskIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(mainObeliskIndex =>
                RngState.Index(RngSpots.FlyingGenieObeliskPatternStart, Mode, mainObeliskIndex * 9, 27, false) / 9);
            break;
        }
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("obeliskIndex"))) {
            ilCursor.EmitDelegate<Func<int, int>>(obeliskIndex =>
                RngState.Index(RngSpots.FlyingGenieObeliskPatternStart, Mode, obeliskIndex, 27, false) % 9);
            break;
        }
    }
}
