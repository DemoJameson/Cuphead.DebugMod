using System;
using System.Collections;
using System.Linq;
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
    public static void PhaseOneBouncerAnglesManipulator(ref DevilLevelPitchforkProjectileSpawner __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (DevilPhaseOneBouncerAngleNormal.Value != DevilPhaseOneBouncerAnglesNormal.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneBouncerAnglesNormal>((int) DevilPhaseOneBouncerAngleNormal.Value);
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (DevilPhaseOneBouncerAngleHard.Value != DevilPhaseOneBouncerAnglesHard.Random) {
                __instance.angleOffsetIndex = Utility.GetUserPattern<DevilPhaseOneBouncerAnglesHard>((int) DevilPhaseOneBouncerAngleHard.Value);
            }
        }
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
}

