using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class DevilPatternSelector : PluginComponent {

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
        float clapDelay = 0f;
        if (DevilClapDelay.Value != -1) {
            clapDelay = new MinMax(DevilClapDelay.Value, DevilClapDelay.Value);
        } else {
            clapDelay = p.delay.RandomFloat();
        }
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
        if (DevilPhaseOnePattern.Value != DevilPhaseOnePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseOnePatterns>((int) DevilPhaseOnePattern.Value);
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneHeadManipulator(ref DevilLevelSittingDevil __instance) {
        if (DevilPhaseOneHeadType.Value != DevilPhaseOneHeadTypes.Random) {
            __instance.isSpiderAttackNext = DevilPhaseOneHeadType.Value == DevilPhaseOneHeadTypes.Spider ? true : false;
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.dragon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneDragonDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<isLeft>"))) {
            // I am checking for Settings.DevilPhaseOneDragonDirection.Value dynamically during this function call.
            // This is because a HarmonyTranspiler only gets called once when the script is loaded upon game bootup...
            // ...so the function call itself that gets injected into the IL code needs to check the value as it is set by Settings.DevilPhaseOneDragonDirection.Value
            ilCursor.EmitDelegate<Func<bool, bool>>(isLeft =>
                DevilPhaseOneDragonDirection.Value == DevilPhaseOneDragonDirections.Random ?
                Rand.Bool()
                :
                DevilPhaseOneDragonDirection.Value == DevilPhaseOneDragonDirections.Left
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }

    // Sets the leading attack. Subsequent attacks work as normal.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneSpiderOffsetManipulator(ref DevilLevelSittingDevil __instance) {
        if (DevilPhaseOneSpiderOffset.Value != DevilPhaseOneSpiderOffsets.Random) {
            __instance.spiderOffsetIndex = Utility.GetUserPattern<DevilPhaseOneSpiderOffsets>((int) DevilPhaseOneSpiderOffset.Value);
        }
    }

    // Experimental
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneSpiderOffsetManipulatorExperimental(ref DevilLevelSittingDevil __instance) {
        if (!string.IsNullOrEmpty(DevilTest.Value)) {
            int[] values = DevilTest.Value
                .Split(',')
                .Select(s => int.Parse(s.Trim()))
                .ToArray();
            int randomIndex = (int) UnityEngine.Random.Range(0, values.Length);
            if (values[randomIndex] == 0) {
                __instance.spiderOffsetIndex = 19;
            }
            __instance.spiderOffsetIndex = values[randomIndex] - 1;
        }
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
        int numAttacks;
        if (DevilPhaseOneSpiderHopCount.Value == DevilPhaseOneSpiderHopCounts.Random) {
            numAttacks = p.numAttacks.RandomInt();
        }
        else {
            numAttacks = (int) DevilPhaseOneSpiderHopCount.Value + 2;
        }
        for (int i = 0; i < numAttacks; i++) {
            float entranceDelay = 0f;
            if (DevilSpiderDelay.Value != -1) {
                entranceDelay = new MinMax(DevilSpiderDelay.Value, DevilSpiderDelay.Value);
            }
            else {
                entranceDelay = p.entranceDelay.RandomFloat();
            }

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


    // The pitchfork attack gets decided in a bit of a peculiar way.
    // There are 3 pattern strings in LevelProperties.
    // The game randomly picks from one of these 3 pattern strings, and then also randomly picks an index to start from.
    // The game then follow the list in order.
    // So, I decided to dynamically look for the pattern i want inside the pattern string that got chosen...
    // ...then set the index appropriately.
    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOnePitchforkManipulator(ref DevilLevelSittingDevil __instance) {
        for (int i = 0; i < __instance.pitchforkPattern.Length; i++) {
            int.TryParse(__instance.pitchforkPattern[i], out var pitchforkAttack);
            if (pitchforkAttack == (int) DevilPhaseOnePitchforkType.Value + 3) {
                if (i == 0) {
                    __instance.pitchforkPatternIndex = __instance.pitchforkPattern.Length - 1;
                } else {
                    __instance.pitchforkPatternIndex = i - 1;
                }
            }
        }
    }

    [HarmonyPatch(typeof(DevilLevelPitchforkProjectileSpawner), MethodType.Constructor, new Type[] { typeof(int), typeof(string) })]
    [HarmonyPostfix]
    public static void PhaseOnePitchforkAnglesManipulator(ref DevilLevelPitchforkProjectileSpawner __instance, int __0) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            DevilPhaseOneBouncerAnglesNormal setting = __0 switch {
                4 => DevilPhaseOneBouncerAngleNormal.Value,
                _ => DevilPhaseOneBouncerAnglesNormal.Random
            };
            if (setting != DevilPhaseOneBouncerAnglesNormal.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneBouncerAnglesNormal>((int)setting);
            }

            if (__0 == 5 && DevilPhaseOnePinwheelAngle.Value != DevilPhaseOnePinwheelAngles.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOnePinwheelAngles>((int)DevilPhaseOnePinwheelAngle.Value);
            }
            if (__0 == 6 && DevilPhaseOneRingAngle.Value != DevilPhaseOneRingAngles.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneRingAngles>((int)DevilPhaseOneRingAngle.Value);
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            DevilPhaseOneBouncerAnglesHard setting = __0 switch {
                4 => DevilPhaseOneBouncerAngleHard.Value,
                _ => DevilPhaseOneBouncerAnglesHard.Random
            };
            if (setting != DevilPhaseOneBouncerAnglesHard.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneBouncerAnglesHard>((int)setting);
            }

            if (__0 == 5 && DevilPhaseOnePinwheelAngle.Value != DevilPhaseOnePinwheelAngles.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOnePinwheelAngles>((int)DevilPhaseOnePinwheelAngle.Value);
            }
            if (__0 == 6 && DevilPhaseOneRingAngle.Value != DevilPhaseOneRingAngles.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneRingAngles>((int)DevilPhaseOneRingAngle.Value);
            }
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkFourFlameBouncer_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneBouncerParryableProjectileManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                DevilPhaseOneBouncerParryIndex.Value == DevilPhaseOneBouncerParryIndexes.Random
                    ? randomIndex
                    : (int)DevilPhaseOneBouncerParryIndex.Value - 1);
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkFourFlameBouncer_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneBouncerDelayManipulator(ILContext il) {
        ReplacePitchforkDelayRandomization(il, OverrideBouncerDelay);
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkSixFlameRing_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneRingParryableProjectileManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                DevilPhaseOneRingParryIndex.Value == DevilPhaseOneRingParryIndexes.Random
                    ? randomIndex
                    : (int)DevilPhaseOneRingParryIndex.Value - 1);
        }
    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.pitchforkSixFlameRing_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneRingDelayManipulator(ILContext il) {
        ReplacePitchforkDelayRandomization(il, OverrideRingDelay);
    }

    private static void ReplacePitchforkDelayRandomization(ILContext il, Func<float, float> overrideDelay) {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After,
                   i => i.Operand?.ToString().Contains("MinMax::RandomFloat") == true)) {
            cursor.EmitDelegate(overrideDelay);
            cursor.Index++;
        }
    }

    private static float OverrideBouncerDelay(float randomDelay) {
        return DevilPhaseOneBouncerDelay.Value < 0f ? randomDelay : DevilPhaseOneBouncerDelay.Value;
    }

    private static float OverrideRingDelay(float randomDelay) {
        return DevilPhaseOneRingDelay.Value < 0f ? randomDelay : DevilPhaseOneRingDelay.Value;
    }

    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.OnStateChanged))]
    [HarmonyPrefix]
    public static void PhaseTwoPatternManipulator(ref DevilLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (DevilPhaseTwoPatternNormal.Value != DevilPhaseTwoPatternsNormal.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseTwoPatternsNormal>((int) DevilPhaseTwoPatternNormal.Value);
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (DevilPhaseTwoPatternHard.Value != DevilPhaseTwoPatternsHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DevilPhaseTwoPatternsHard>((int) DevilPhaseTwoPatternHard.Value);
            }
        }

    }

    [HarmonyPatch(typeof(DevilLevelGiantHead), nameof(DevilLevelGiantHead.eye_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoBombEyeDirectionManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("bombOnLeft"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(bombOnLeft =>
                DevilPhaseTwoBombEyeDirection.Value == DevilPhaseTwoBombEyeDirections.Random ?
                Rand.Bool()
                :
                DevilPhaseTwoBombEyeDirection.Value == DevilPhaseTwoBombEyeDirections.Left
            );
            ilCursor.Index++; // avoid infinite loops
        }
    }

    [HarmonyPatch(typeof(DevilLevelGiantHead), "platforms_cr", MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseTwoPlatformManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After, i =>
                i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("UnityEngine.Random::Range(System.Int32,System.Int32)"))) {
            ilCursor.EmitDelegate<Func<int, int>>(randomIndex =>
                DevilPhaseTwoPlatformRisePattern.Value == DevilPhaseTwoPlatformRisePatterns.Random
                    ? randomIndex
                    : (int)DevilPhaseTwoPlatformRisePattern.Value - 2);
        }

        ilCursor.Index = 0;
        while (ilCursor.TryGotoNext(MoveType.After, i =>
                   i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("MinMax::RandomFloat"))) {
            ilCursor.EmitDelegate<Func<float, float>>(randomDelay =>
                DevilPhaseTwoPlatformRiseDelay.Value != -1f
                    ? DevilPhaseTwoPlatformRiseDelay.Value
                    : randomDelay);
        }
    }

    [HarmonyPatch(typeof(DevilLevelHand), nameof(DevilLevelHand.StartPattern))]
    [HarmonyPostfix]
    public static void DevilPhaseThreeSkullPatternManipulator(DevilLevelHand __instance)
    {
        if (DevilPhaseThreeSkullType.Value != DevilPhaseThreeSkullTypes.Random) {
            __instance.pinkStringIndex = (int) DevilPhaseThreeSkullType.Value - 1;
        }
    }
}
