using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class SnowCultPatternSelector {
    [HarmonyPatch(typeof(SnowCultLevelWizard), nameof(SnowCultLevelWizard.LevelInit))]
    [HarmonyPostfix]
    public static void SnowCultPhaseOnePatternManipulator(ref SnowCultLevelWizard __instance) {
        if (SnowCultPhaseOnePattern.Value != SnowCultPhaseOnePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<SnowCultPhaseOnePatterns>((int) SnowCultPhaseOnePattern.Value);
        }
    }

    [HarmonyPatch(typeof(SnowCultLevelWizard), nameof(SnowCultLevelWizard.LevelInit))]
    [HarmonyPostfix]
    public static void SnowCultPhaseOneStartPositionManipulator(ref SnowCultLevelWizard __instance) {
        if (SnowCultPhaseOneStartPosition.Value != SnowCultPhaseOneStartPositions.Random) {
            int userPattern = Utility.GetUserPattern<SnowCultPhaseOneStartPositions>((int) SnowCultPhaseOneStartPosition.Value);
            int runningTotal = 0;
            for (int i = 0; i <= __instance.attackLocationString.mainPatternString.Length; i++) {
                int count = __instance.attackLocationString.mainPatternString[i].Split(',').Length;

                if (userPattern < runningTotal + count) {
                    __instance.attackLocationString.mainIndex = i;
                    __instance.attackLocationString.subIndex = userPattern - runningTotal;
                    __instance.attackLocationString.subPatternString = __instance.attackLocationString.mainPatternString[i].Split(',');
                    break;
                }

                runningTotal += count;
            }
        }
    }

    [HarmonyPatch(typeof(SnowCultLevelYeti), nameof(SnowCultLevelYeti.StartYeti))]
    [HarmonyPrefix]
    public static bool SnowCultPhaseTwoPatternManipulator(ref SnowCultLevelYeti __instance) {
        __instance.StartCoroutine(new_intro_cr(__instance));
        if (Level.Current.mode != Level.Mode.Easy) {
            __instance.StartCoroutine(__instance.bats_attack_cr());
        }
        return false;
    }

    private static IEnumerator new_intro_cr(SnowCultLevelYeti __instance) {
        __instance.state = SnowCultLevelYeti.States.Intro;
        __instance.introRibcageClosed = false;
        __instance.transform.position = Vector3.zero;
        __instance.transform.position = __instance.yetiSpawnPoint.position;
        __instance.transform.position += Vector3.up * 300f;
        float t = 0f;
        __instance.sprite = __instance.GetComponent<SpriteRenderer>();
        __instance.animator.Play("Intro", 0, 0f);
        YieldInstruction wait = new WaitForFixedUpdate();
        while (__instance.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(__instance.animator.GetLayerName(0) + ".Intro")) {
            if (t < 0.34f) {
                __instance.transform.position = new Vector3(__instance.transform.position.x, EaseUtils.EaseOutBack(__instance.yetiSpawnPoint.position.y + 300f, __instance.yetiSpawnPoint.position.y, t * 3f));
                t += CupheadTime.FixedDelta;
            }
            __instance.introShadow.transform.position = new Vector3(__instance.transform.position.x, -25f);
            yield return wait;
        }
        __instance.SetState(SnowCultLevelYeti.States.Idle);
        __instance.StartCoroutine(new_do_patterns_cr(__instance));
        yield return null;
        yield break;
    }

    private static IEnumerator new_do_patterns_cr(SnowCultLevelYeti __instance) {
        LevelProperties.SnowCult.Yeti p = __instance.properties.CurrentState.yeti;
        __instance.patternString = p.yetiPatternString.Split(new char[]
        {
            ','
        });
        if (SnowCultPhaseTwoPattern.Value != SnowCultPhaseTwoPatterns.Random) {
            __instance.patternStringIndex = (int) SnowCultPhaseTwoPattern.Value - 1;
        } else {
            __instance.patternStringIndex = UnityEngine.Random.Range(0, __instance.patternString.Length);
        }
        while (!__instance.forceOutroToStart) {
            string text = __instance.patternString[__instance.patternStringIndex];
            if (text == null) {
                goto IL_144;
            }
            if (!(text == "S")) {
                if (!(text == "J")) {
                    if (!(text == "L")) {
                        if (!(text == "P")) {
                            goto IL_144;
                        }
                        __instance.StartIcePillar();
                    } else {
                        __instance.Snowball();
                    }
                } else {
                    __instance.StartCoroutine(__instance.start_jump_cr());
                }
            } else {
                __instance.StartCoroutine(__instance.start_dash_cr());
            }
            IL_154:
            while (__instance.state != SnowCultLevelYeti.States.Idle) {
                yield return null;
            }
            __instance.patternStringIndex = (__instance.patternStringIndex + 1) % __instance.patternString.Length;
            yield return null;
            continue;
            IL_144:
            __instance.Snowball();
            goto IL_154;
        }
        yield break;
    }

    [HarmonyPatch(typeof(SnowCultLevelJackFrost), nameof(SnowCultLevelJackFrost.StartPhase3))]
    [HarmonyPostfix]
    public static void SnowCultPhaseThreePatternManipulator(ref SnowCultLevelJackFrost __instance) {
        if (SnowCultPhaseThreePattern.Value != SnowCultPhaseThreePatterns.Random) {
            __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<SnowCultPhaseThreePatterns>((int) SnowCultPhaseThreePattern.Value);
        }
    }
}