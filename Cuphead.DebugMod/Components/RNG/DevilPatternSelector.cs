using System;
using System.Collections;
using BepInEx.CupheadDebugMod.Rng;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

// All values come from RngState, which passes vanilla results through untouched and only
// spends an extra RNG draw when a spot is actually configured in the active profile.
[HarmonyPatch]
internal class DevilPatternSelector : PluginComponent {

    private static Level.Mode Mode => Level.ScoringData.difficulty;

    // ---------- Phase 1 ----------

    // Skipping over original method and replacing with a modified clap_cr(). This avoids having to modify clap_cr() through IL.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.StartClap))]
    [HarmonyPrefix]
    public static bool PhaseOneClapManipulator(ref DevilLevelSittingDevil __instance) {
        __instance.state = DevilLevelSittingDevil.State.Clap;
        __instance.StartCoroutine(new_clap_cr(__instance));
        return false;
    }

    public static IEnumerator new_clap_cr(DevilLevelSittingDevil __instance) {
        LevelProperties.Devil.Clap p = __instance.properties.CurrentState.clap;
        __instance.animator.SetBool("StartRam", true);
        yield return __instance.animator.WaitForAnimationToEnd(__instance, "Ram_Start", false);
        float clapDelay = RngState.Float(RngSpots.DevilPhase1ClapDelay, Mode, p.delay.RandomFloat());
        yield return CupheadTime.WaitForSeconds(__instance, clapDelay);
        foreach (DevilLevelDevilArm devilLevelDevilArm in __instance.arms) {
            devilLevelDevilArm.Attack(p.speed);
        }
        while (__instance.arms[0].state != DevilLevelDevilArm.State.Idle) {
            yield return null;
        }
        __instance.animator.SetBool("StartRam", false);
        yield return CupheadTime.WaitForSeconds(__instance, p.hesitate);
        __instance.state = DevilLevelSittingDevil.State.Idle;
        yield break;
    }

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.Start))]
    [HarmonyPrefix]
    public static void PhaseOnePatternManipulator(ref DevilLevel __instance) {
        LevelProperties.Devil.State state = __instance.properties.CurrentState;
        state.patternIndex = RngState.Index(RngSpots.DevilPhase1PatternStart, Mode, state.patternIndex, state.patterns.Length, true);
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneHeadManipulator(ref DevilLevelSittingDevil __instance) {
        int vanilla = __instance.isSpiderAttackNext ? 1 : 0;
        int pick = RngState.Index(RngSpots.DevilPhase1HeadFirst, Mode, vanilla, 2, false);
        __instance.isSpiderAttackNext = pick == 1;
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.dragon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneDragonDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<isLeft>"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(isLeft => {
                int pick = RngState.Index(RngSpots.DevilPhase1DragonLeft, Mode, isLeft ? 0 : 1, 2, false);
                return pick == 0;
            });
            ilCursor.Index++; // avoid infinite loops
        }
    }

    // Sets where the spider head's fixed offset loop begins.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneSpiderOffsetManipulator(ref DevilLevelSittingDevil __instance) {
        __instance.spiderOffsetIndex = RngState.Index(RngSpots.DevilPhase1SpiderOffsetStart, Mode, __instance.spiderOffsetIndex, __instance.spiderOffsets.Length, true);
    }

    // Skipping over original method and replacing with a modified spider_cr(). This avoids having to modify spider_cr() through IL.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.StartHead))]
    [HarmonyPrefix]
    public static bool PhaseOneSpiderDelayAndHopCountManipulator(ref DevilLevelSittingDevil __instance) {
        __instance.state = DevilLevelSittingDevil.State.Head;
        if (__instance.isSpiderAttackNext) {
            __instance.StartCoroutine(new_spider_cr(__instance));
        } else {
            __instance.StartCoroutine(__instance.dragon_cr());
        }
        __instance.isSpiderAttackNext = !__instance.isSpiderAttackNext;
        return false;
    }

    public static IEnumerator new_spider_cr(DevilLevelSittingDevil __instance) {
        __instance.animator.SetBool("StartSpider", true);
        yield return __instance.animator.WaitForAnimationToStart(__instance, "Spider_Start", false);
        AudioManager.Play("devil_spider_head_intro");
        __instance.emitAudioFromObject.Add("devil_spider_head_intro");
        yield return __instance.animator.WaitForAnimationToEnd(__instance, "Spider_Start", false);
        LevelProperties.Devil.Spider p = __instance.properties.CurrentState.spider;
        // The game rolls its hop count with Random.Range(min, max) - the int overload,
        // whose upper bound is exclusive - so MinMax(3, 6) can only ever produce 3, 4 or 5.
        int numAttacks = RngState.Index(RngSpots.DevilPhase1SpiderHopCount, Mode, p.numAttacks.RandomInt() - 3, 3, false) + 3;
        for (int i = 0; i < numAttacks; i++) {
            float entranceDelay = RngState.Float(RngSpots.DevilPhase1SpiderDelay, Mode, p.entranceDelay.RandomFloat());
            yield return CupheadTime.WaitForSeconds(__instance, entranceDelay);
            __instance.spiderOffsetIndex = (__instance.spiderOffsetIndex + 1) % __instance.spiderOffsets.Length;
            float offset = 0f;
            float.TryParse(__instance.spiderOffsets[__instance.spiderOffsetIndex], out offset);
            __instance.spiderHead.Attack(Mathf.Clamp(PlayerManager.GetNext().center.x + offset, -620f, 620f), p.downSpeed, p.upSpeed);
            while (__instance.spiderHead.state != DevilLevelSpiderHead.State.Idle) {
                yield return null;
            }
        }
        __instance.animator.SetBool("StartSpider", false);
        yield return CupheadTime.WaitForSeconds(__instance, p.hesitate);
        __instance.state = DevilLevelSittingDevil.State.Idle;
        yield break;
    }


    // The pitchfork is decided in two game rolls, both once per fight: one of the order
    // strings is picked, then a start position inside it (the index is consumed with a
    // pre-increment per attack). The order spot replaces the rolled string - re-rolling
    // the start uniformly over the new list when no first attack is pinned - and the type
    // spot then places the first attack on the first occurrence of its type, the way the
    // game's own random start could have landed there.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOnePitchforkManipulator(ref DevilLevelSittingDevil __instance) {
        LevelProperties.Devil.State state = __instance.properties.CurrentState;
        string[] lists = state.pitchfork.patternString;

        int order = RngState.Index(RngSpots.DevilPhase1PitchforkOrder, Mode, -1, lists.Length, false);
        if (order >= 0) {
            __instance.pitchforkPattern = lists[order].Split(new[] { ',' });
        }

        int type = RngState.Index(RngSpots.DevilPhase1PitchforkType, Mode, -1, 3, false);
        if (type >= 0) {
            // the first occurrence of the pinned attack becomes the next one
            int pitchforkAttack = type + 4;
            for (int i = 0; i < __instance.pitchforkPattern.Length; i++) {
                int parsed;
                int.TryParse(__instance.pitchforkPattern[i], out parsed);
                if (parsed == pitchforkAttack) {
                    __instance.pitchforkPatternIndex = i == 0 ? __instance.pitchforkPattern.Length - 1 : i - 1;
                    break;
                }
            }
        } else if (order >= 0) {
            // the replaced list can have a different length; keep the start uniform over it
            __instance.pitchforkPatternIndex = UnityEngine.Random.Range(0, __instance.pitchforkPattern.Length);
        }
    }

    // The bouncer, pinwheel and ring spawners are constructed inside LevelInit with the same
    // projectile count (4) for the first two, so they are set by field reference instead of
    // by constructor argument. The postfix runs after the game's random pick, and the spawners
    // read their angle with an increment, hence preIncrement.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOnePitchforkAnglesManipulator(ref DevilLevelSittingDevil __instance) {
        LevelProperties.Devil.State state = __instance.properties.CurrentState;
        __instance.pitchforkFourFlameBouncerSpawner.angleOffsetIndex = RngState.Index(
            RngSpots.DevilPhase1BouncerAngleStart, Mode,
            __instance.pitchforkFourFlameBouncerSpawner.angleOffsetIndex,
            CountValues(state.pitchforkFourFlameBouncer.angleOffset), true);
        __instance.pitchforkFiveFlameSpinnerSpawner.angleOffsetIndex = RngState.Index(
            RngSpots.DevilPhase1PinwheelAngleStart, Mode,
            __instance.pitchforkFiveFlameSpinnerSpawner.angleOffsetIndex,
            CountValues(state.pitchforkFiveFlameSpinner.angleOffset), true);
        __instance.pitchforkSixFlameRingSpawner.angleOffsetIndex = RngState.Index(
            RngSpots.DevilPhase1RingAngleStart, Mode,
            __instance.pitchforkSixFlameRingSpawner.angleOffsetIndex,
            CountValues(state.pitchforkSixFlameRing.angleOffset), true);
    }

    private static int CountValues(string csv) {
        if (string.IsNullOrEmpty(csv)) {
            return 0;
        }
        int count = 1;
        for (int i = 0; i < csv.Length; i++) {
            if (csv[i] == ',') {
                count++;
            }
        }
        return count;
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkFourFlameBouncer_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneBouncerParryableProjectileManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                RngState.Index(RngSpots.DevilPhase1BouncerParryIndex, Mode, randomIndex, 4, false));
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkFourFlameBouncer_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneBouncerDelayManipulator(ILContext il) {
        ReplacePitchforkDelayRandomization(il, randomDelay => RngState.Float(RngSpots.DevilPhase1BouncerDelay, Mode, randomDelay));
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkSixFlameRing_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneRingParryableProjectileManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                RngState.Index(RngSpots.DevilPhase1RingParryIndex, Mode, randomIndex, 6, false));
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkSixFlameRing_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneRingDelayManipulator(ILContext il) {
        ReplacePitchforkDelayRandomization(il, randomDelay => RngState.Float(RngSpots.DevilPhase1RingDelay, Mode, randomDelay));
    }

    private static void ReplacePitchforkDelayRandomization(ILContext il, Func<float, float> overrideDelay) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                   i => i.Operand?.ToString().Contains("MinMax::RandomFloat") == true)) {
            cursor.EmitDelegate(overrideDelay);
            cursor.Index++;
        }
    }

    // ---------- Phase 2 ----------

    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void PhaseTwoPatternManipulator(ref DevilLevel __instance) {
        LevelProperties.Devil.State state = __instance.properties.CurrentState;
        if (state.stateName != LevelProperties.Devil.States.GiantHead) {
            return;
        }
        state.patternIndex = RngState.Index(RngSpots.DevilPhase2PatternStart, Mode, state.patternIndex, state.patterns.Length, true);
    }

    [HarmonyPatch(typeof(DevilLevelGiantHead), nameof(DevilLevelGiantHead.eye_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoBombEyeDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("bombOnLeft"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(bombOnLeft => {
                int pick = RngState.Index(RngSpots.DevilPhase2BombLeft, Mode, bombOnLeft ? 0 : 1, 2, false);
                return pick == 0;
            });
            ilCursor.Index++; // avoid infinite loops
        }
    }

    [HarmonyPatch(typeof(DevilLevelGiantHead), "platforms_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoPlatformManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            // The rise order string is identical on Regular and Expert (15 entries) and empty on
            // Simple, where this attack never runs.
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                RngState.Index(RngSpots.DevilPhase2PlatformRiseStart, Mode, randomIndex, 15, true));
        }

        ilCursor.Index = 0;
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            ilCursor.EmitDelegate<Func<float, float>>(randomDelay =>
                RngState.Float(RngSpots.DevilPhase2PlatformRiseDelay, Mode, randomDelay));
        }
    }

    // ---------- Phase 3 ----------

    [HarmonyPatch(typeof(DevilLevelHand), nameof(DevilLevelHand.StartPattern))]
    [HarmonyPostfix]
    public static void DevilPhaseThreeSkullPatternManipulator(DevilLevelHand __instance, LevelProperties.Devil.Hands properties) {
        string pinkString = properties.pinkString;
        if (string.IsNullOrEmpty(pinkString)) {
            return;
        }
        __instance.pinkStringIndex = RngState.Index(RngSpots.DevilPhase3SkullType, Mode, __instance.pinkStringIndex, pinkString.Length, false);
    }
}
