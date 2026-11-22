using System;
using System.Collections;
using System.Globalization;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// resolves configured spots (see Rng/RngSpots.cs for the catalog).
[HarmonyPatch]
internal class VeggiesPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // The mind-meld barrage rolls its carrot count once per round (carrot_cr is started
    // once per rings_cr loop iteration) and assigns the roll straight to the count, so
    // picks shift back into the game's own per-difficulty range (3-4 / 4-5 / 5-6).
    [HarmonyPatch(typeof(VeggiesLevelCarrot), "carrot_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CarrotCountManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldflda) && i.Operand.ToString().Contains("homingNumOfCarrots"),
                i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand.ToString().Contains("MinMax::RandomInt"))) {
            cursor.EmitDelegate<Func<int, int>>(rolledCount => {
                int min = RngSpots.VeggiesCarrotCountMin(Mode);
                return RngState.Index(RngSpots.VeggiesCarrotCount, Mode, rolledCount - min, RngSpots.VeggiesCarrotCountOptionCount(Mode), false) + min;
            });
        }
    }

    // The hesitation between a carrot barrage and the ring volley that follows it is
    // rolled once per round in rings_cr.
    [HarmonyPatch(typeof(VeggiesLevelCarrot), "rings_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void CarrotIdleManipulator(ILContext il) {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNext(MoveType.After,
                i => (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldflda) && i.Operand.ToString().Contains("idleRange"),
                i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            cursor.EmitDelegate<Func<float, float>>(rolledDelay =>
                RngState.Float(RngSpots.VeggiesCarrotIdleDelay, Mode, rolledDelay));
        }
    }

    // Skipping the original StartTearCoroutines and starting our own tear streams instead.
    // This is what gives the left and right streams separate RNG counters: the original
    // tears_cr runs as two instances of one coroutine, whose delay and pink-cadence rolls
    // would interleave on any shared spot.
    [HarmonyPatch(typeof(VeggiesLevelOnion), nameof(VeggiesLevelOnion.StartTearCoroutines))]
    [HarmonyPrefix]
    private static bool StartTearCoroutinesManipulator(VeggiesLevelOnion __instance) {
        __instance.StopTearCoroutines();

        string[] strings = RngSpots.VeggiesTearStrings(Mode);
        if (strings.Length == 0) {
            return false; // no tear data on this difficulty (Simple)
        }

        // one volley pattern per crying round, shared by both sides (the game's own roll)
        int pick = RngState.Index(RngSpots.VeggiesOnionTearPattern, Mode, UnityEngine.Random.Range(0, strings.Length), strings.Length, false);
        string pattern = strings[pick].ToUpper();

        IEnumerator right = TearsCr(__instance, VeggiesLevelOnion.Side.Right, pattern);
        IEnumerator left = TearsCr(__instance, VeggiesLevelOnion.Side.Left, pattern);
        RightTearsField.SetValue(__instance, right);
        LeftTearsField.SetValue(__instance, left);
        __instance.StartCoroutine(right);
        __instance.StartCoroutine(left);
        return false;
    }

    // Faithful port of VeggiesLevelOnion.tears_cr, matching the 1.2/1.3 semantics (the
    // invariant-culture Parser.FloatTryParse parsing; the 1.0/1.1 shipped data is free of
    // D entries and the only other version difference is debug logging, which is dropped).
    // Every roll resolves against the stream's own per-side spots.
    private static IEnumerator TearsCr(VeggiesLevelOnion instance, VeggiesLevelOnion.Side side, string pattern) {
        Level.Mode mode = Level.ScoringData.difficulty;
        LevelProperties.Veggies.Onion onion = RngSpots.VeggiesOnionData(mode);
        if (onion == null) {
            yield break;
        }

        string delaySpot = side == VeggiesLevelOnion.Side.Right ? RngSpots.VeggiesOnionTearDelayRight : RngSpots.VeggiesOnionTearDelayLeft;
        string pinkSpot = side == VeggiesLevelOnion.Side.Right ? RngSpots.VeggiesOnionPinkCadenceRight : RngSpots.VeggiesOnionPinkCadenceLeft;
        int pinkMin = RngSpots.VeggiesPinkMin(mode);
        int pinkCount = RngSpots.VeggiesPinkOptionCount(mode);

        VeggiesLevelOnionTearProjectile projectilePrefab = (VeggiesLevelOnionTearProjectile)ProjectileField.GetValue(instance);
        VeggiesLevelOnionTearProjectile pinkProjectilePrefab = (VeggiesLevelOnionTearProjectile)PinkProjectileField.GetValue(instance);

        float tearDelay = RngState.Float(delaySpot, mode, UnityEngine.Random.Range(0.3f, 0.7f));
        yield return CupheadTime.WaitForSeconds(instance, onion.tearAnticipate);
        string[] patterns = pattern.Split(new[] { ',' });
        int currentPattern = 0;
        int numUntilPink = RngState.Index(pinkSpot, mode, onion.pinkTearRange.RandomInt() - pinkMin, pinkCount, false) + pinkMin;
        for (;;) {
            if (patterns[currentPattern][0] == 'D') {
                float wait;
                if (float.TryParse(patterns[currentPattern].Replace("D", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out wait)) {
                    yield return CupheadTime.WaitForSeconds(instance, wait);
                }
            } else {
                string[] destinations = patterns[currentPattern].Split(new[] { '-' });
                for (int i = 0; i < destinations.Length; i++) {
                    float x;
                    if (float.TryParse(destinations[i], NumberStyles.Float, CultureInfo.InvariantCulture, out x)) {
                        numUntilPink--;
                        if (numUntilPink <= 0) {
                            yield return CupheadTime.WaitForSeconds(instance, tearDelay);
                            numUntilPink = RngState.Index(pinkSpot, mode, onion.pinkTearRange.RandomInt() - pinkMin, pinkCount, false) + pinkMin;
                            pinkProjectilePrefab.Create(onion.tearTime, (side != VeggiesLevelOnion.Side.Right) ? (-x) : x);
                        } else {
                            yield return CupheadTime.WaitForSeconds(instance, tearDelay);
                            projectilePrefab.Create(onion.tearTime, (side != VeggiesLevelOnion.Side.Right) ? (-x) : x);
                        }
                        tearDelay = RngState.Float(delaySpot, mode, UnityEngine.Random.Range(0.3f, 0.7f));
                    }
                }
            }
            currentPattern = (int)Mathf.Repeat((float)(currentPattern + 1), (float)patterns.Length);
            yield return CupheadTime.WaitForSeconds(instance, onion.tearCommaDelay);
        }
    }

    private static readonly System.Reflection.FieldInfo RightTearsField = AccessTools.Field(typeof(VeggiesLevelOnion), "rightTearsCoroutine");
    private static readonly System.Reflection.FieldInfo LeftTearsField = AccessTools.Field(typeof(VeggiesLevelOnion), "leftTearsCoroutine");
    private static readonly System.Reflection.FieldInfo ProjectileField = AccessTools.Field(typeof(VeggiesLevelOnion), "projectilePrefab");
    private static readonly System.Reflection.FieldInfo PinkProjectileField = AccessTools.Field(typeof(VeggiesLevelOnion), "pinkProjectilePrefab");
}
