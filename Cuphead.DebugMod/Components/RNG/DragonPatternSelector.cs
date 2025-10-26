using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class DragonPatternSelector {

    [HarmonyPatch(typeof(DragonLevel), nameof(DragonLevel.OnStateChanged))]
    [HarmonyPrefix]

    // pattern index seems to get regenerated in phases 2 and 3
    public static void DragonPatternManipulator(ref DragonLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(0.75f, 0.45f)) {
                if (DragonPhaseTwoPatternEasy.Value != DragonPhaseTwoPatternsEasy.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DragonPhaseTwoPatternsEasy>((int) DragonPhaseTwoPatternEasy.Value);
                }
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.82f, 0.63f)) {
                if (DragonPhaseThreePatternNormal.Value != DragonPhaseThreePatternsNormal.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DragonPhaseThreePatternsNormal>((int) DragonPhaseThreePatternNormal.Value);
                }
            }
        } else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(0.9f, 0.64f)) {
                if (DragonPhaseTwoPatternHard.Value != DragonPhaseTwoPatternsHard.Random) {
                    __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DragonPhaseTwoPatternsHard>((int) DragonPhaseTwoPatternHard.Value);
                }
            }
        }
    }

    [HarmonyPatch(typeof(DragonLevel), nameof(DragonLevel.Start))]
    [HarmonyPrefix]
    public static void DragonPhaseOnePatternManipulator(ref DragonLevel __instance) {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (DragonPhaseOnePatternEasy.Value != DragonPhaseOnePatternsEasy.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DragonPhaseOnePatternsEasy>((int) DragonPhaseOnePatternEasy.Value);
            }
        }
        else if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (DragonPhaseOnePatternHard.Value != DragonPhaseOnePatternsHard.Random) {
                __instance.properties.CurrentState.patternIndex = Utility.GetUserPattern<DragonPhaseOnePatternsHard>((int) DragonPhaseOnePatternHard.Value);
            }
        }
    }

    // Skipping over original method and replacing with a modified peashot_cr(). This avoids having to modify peashot_cr() through IL.
    [HarmonyPatch(typeof(DragonLevelDragon), nameof(DragonLevelDragon.StartPeashot))]
    [HarmonyPrefix]
    public static bool PeashotManipulator(DragonLevelDragon __instance) {
        __instance.state = DragonLevelDragon.State.Peashot;
        __instance.StartCoroutine(new_peashot_cr( __instance));
        return false;
    }


    private static IEnumerator new_peashot_cr(DragonLevelDragon __instance) {
        LevelProperties.Dragon.Peashot p = __instance.properties.CurrentState.peashot;
        int peashotIndex = -1;
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(1f, 0.75f)) {
                if (DragonPhaseOneLaserPatternEasy.Value != DragonPhaseOneLaserPatternsEasy.Random) {
                    peashotIndex = (int) DragonPhaseOneLaserPatternEasy.Value - 1;
                }
            }
            if (IsWithinPhase(0.75f, 0.45f)) {
                if (DragonPhaseTwoLaserPatternEasy.Value != DragonPhaseTwoLaserPatternsEasy.Random) {
                    peashotIndex = (int) DragonPhaseTwoLaserPatternEasy.Value - 1;
                }
            }
        }


        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(1.0f, 0.92f)) {
                if (DragonPhaseOneLaserPatternNormal.Value != DragonPhaseOneLaserPatternsNormal.Random) {
                    peashotIndex = (int) DragonPhaseOneLaserPatternNormal.Value - 1;
                }
            }
            if (IsWithinPhase(0.82f, 0.63f)) {
                if (DragonPhaseThreeLaserPatternNormal.Value != DragonPhaseThreeLaserPatternsNormal.Random) {
                    peashotIndex = (int) DragonPhaseThreeLaserPatternNormal.Value - 1;
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(1.0f, 0.9f)) {
                if (DragonPhaseOneLaserPatternHard.Value != DragonPhaseOneLaserPatternsHard.Random) {
                    peashotIndex = (int) DragonPhaseOneLaserPatternHard.Value - 1;
                }
            }
            if (IsWithinPhase(0.9f, 0.64f)) {
                if (DragonPhaseTwoLaserPatternHard.Value != DragonPhaseTwoLaserPatternsHard.Random) {
                    peashotIndex = (int) DragonPhaseTwoLaserPatternHard.Value - 1;
                }
            }
        }
        string[] pattern;

        if (peashotIndex != -1) { 
            pattern = p.patternString[peashotIndex].Split(new char[]
            {
                ','
            });
        } else {
            pattern = p.patternString.GetRandom<string>().Split(new char[]
            {
                ','
            });
        }


        __instance.animator.SetBool("Peashot", true);
        yield return __instance.animator.WaitForAnimationToEnd(__instance, "Peashot_In", false);
        __instance.animator.Play("Peashot_Zinger");
        for (int i = 0; i < pattern.Length; i++) {
            if (pattern[i].ToLower() == "p") {
                __instance.peashotRoot.LookAt2D(PlayerManager.GetNext().center);
                for (int c = 0; c < p.colorString.Length; c++) {
                    int color = 0;
                    char c2 = p.colorString[c];
                    if (c2 != 'O') {
                        if (c2 != 'P') {
                            if (c2 == 'B') {
                                color = 1;
                            }
                        } else {
                            color = 2;
                        }
                    } else {
                        color = 0;
                    }
                    AudioManager.Play("level_dragon_left_dragon_peashot_fire");
                    __instance.emitAudioFromObject.Add("level_dragon_left_dragon_peashot_fire");
                    (__instance.peashotPrefab.Create(__instance.peashotRoot.position, __instance.peashotRoot.eulerAngles.z, p.speed) as DragonLevelPeashot).color = color;
                    yield return CupheadTime.WaitForSeconds(__instance, p.shotDelay);
                }
            } else {
                float delay = 0f;
                float.TryParse(pattern[i], out delay);
                yield return CupheadTime.WaitForSeconds(__instance, delay);
            }
        }
        __instance.animator.SetBool("Peashot", false);
        yield return __instance.animator.WaitForAnimationToStart(__instance, "Peashot_Out", false);
        AudioManager.Play("level_dragon_left_dragon_peashot_out");
        __instance.emitAudioFromObject.Add("level_dragon_left_dragon_peashot_out");
        yield return CupheadTime.WaitForSeconds(__instance, p.hesitate);
        __instance.state = DragonLevelDragon.State.Idle;
        yield break;
    }

    // Skipping over original method and replacing with a modified meteor_cr(). This avoids having to modify meteor_cr() through IL.
    [HarmonyPatch(typeof(DragonLevelDragon), nameof(DragonLevelDragon.StartMeteor))]
    [HarmonyPrefix]
    public static bool MeteorManipulator(DragonLevelDragon __instance) {
        __instance.state = DragonLevelDragon.State.Meteor;
        __instance.StartCoroutine(new_meteor_cr(__instance));
        return false;
    }

    private static IEnumerator new_meteor_cr(DragonLevelDragon __instance) {
        __instance.currentMeteorProperties = __instance.properties.CurrentState.meteor;
        char[] meteorPattern = null ;

        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(1f, 0.75f)) {
                if (DragonPhaseOneMeteorPatternEasy.Value != DragonPhaseOneMeteorPatternsEasy.Random) {
                    meteorPattern = DragonPhaseOneMeteorPatternEasy.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.75f, 0.45f)) {
                if (DragonPhaseTwoMeteorPatternEasy.Value != DragonPhaseTwoMeteorPatternsEasy.Random) {
                    meteorPattern = DragonPhaseTwoMeteorPatternEasy.Value.ToString().ToCharArray();
                }
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(0.92f, 0.82f)) {
                if (DragonPhaseTwoMeteorPatternNormal.Value != DragonPhaseTwoMeteorPatternsNormal.Random) {
                    meteorPattern = DragonPhaseTwoMeteorPatternNormal.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.82f, 0.63f)) {
                if (DragonPhaseThreeMeteorPatternNormal.Value != DragonPhaseThreeMeteorPatternsNormal.Random) {
                    meteorPattern = DragonPhaseThreeMeteorPatternNormal.Value.ToString().ToCharArray();
                }
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(1f, 0.9f)) {
                if (DragonPhaseOneMeteorPatternHard.Value != DragonPhaseOneMeteorPatternsHard.Random) {
                    meteorPattern = DragonPhaseOneMeteorPatternHard.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.9f, 0.64f)) {
                if (DragonPhaseTwoMeteorPatternHard.Value != DragonPhaseTwoMeteorPatternsHard.Random) {
                    meteorPattern = DragonPhaseTwoMeteorPatternHard.Value.ToString().ToCharArray();
                }
            }
        }

        if (meteorPattern.IsNullOrEmpty()) {
            meteorPattern = __instance.currentMeteorProperties.pattern.GetRandom<string>().ToCharArray();
        }

        __instance.animator.SetTrigger("OnMeteor");
        __instance.animator.SetBool("Repeat", true);
        yield return __instance.animator.WaitForAnimationToStart(__instance, "MeteorStart", false);
        AudioManager.Play("level_dragon_left_dragon_meteor_start");
        __instance.emitAudioFromObject.Add("level_dragon_left_dragon_meteor_start");
        for (int i = 0; i < meteorPattern.Length; i++) {
            char c = meteorPattern[i];
            switch (c) {
                case 'B':
                    __instance.meteorState = DragonLevelMeteor.State.Both;
                    break;
                default:
                    if (c != 'U') {
                    }
                    __instance.meteorState = DragonLevelMeteor.State.Up;
                    break;
                case 'D':
                    __instance.meteorState = DragonLevelMeteor.State.Down;
                    break;
                case 'F':
                    __instance.meteorState = DragonLevelMeteor.State.Forward;
                    break;
            }
            if (i >= meteorPattern.Length - 1) {
                __instance.animator.SetBool("Repeat", false);
            }
            yield return __instance.animator.WaitForAnimationToStart(__instance, "Meteor_Anticipation_Loop", false);
            AudioManager.Play("level_dragon_left_dragon_meteor_anticipation_loop");
            __instance.emitAudioFromObject.Add("level_dragon_left_dragon_meteor_anticipation_loop");
            yield return CupheadTime.WaitForSeconds(__instance, __instance.currentMeteorProperties.shotDelay);
            __instance.animator.SetTrigger("OnMeteor");
            AudioManager.Stop("level_dragon_left_dragon_meteor_anticipation_loop");
            yield return __instance.animator.WaitForAnimationToStart(__instance, "Meteor_Attack", false);
            AudioManager.Play("level_dragon_left_dragon_meteor_attack");
            yield return __instance.animator.WaitForAnimationToEnd(__instance, "Meteor_Attack", false);
        }
        yield return __instance.animator.WaitForAnimationToEnd(__instance, "Meteor_Attack_End", false);
        yield return CupheadTime.WaitForSeconds(__instance, __instance.currentMeteorProperties.hesitate);
        __instance.state = DragonLevelDragon.State.Idle;
        yield break;
    }

    public static char[] SetMeteorPattern() {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(1f, 0.75f)) {
                if (DragonPhaseOneMeteorPatternEasy.Value != DragonPhaseOneMeteorPatternsEasy.Random) {
                    return DragonPhaseOneMeteorPatternEasy.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.75f, 0.45f)) {
                if (DragonPhaseTwoMeteorPatternEasy.Value != DragonPhaseTwoMeteorPatternsEasy.Random) {
                    return DragonPhaseTwoMeteorPatternEasy.Value.ToString().ToCharArray();
                }
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            UnityEngine.Debug.Log("difficulty is normal");
            if (IsWithinPhase(0.92f, 0.82f)) {
                UnityEngine.Debug.Log("it's within phase");
                if (DragonPhaseTwoMeteorPatternNormal.Value != DragonPhaseTwoMeteorPatternsNormal.Random) {
                    UnityEngine.Debug.Log("setting meteor pattern");
                    return DragonPhaseTwoMeteorPatternNormal.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.82f, 0.63f)) {
                if (DragonPhaseThreeMeteorPatternNormal.Value != DragonPhaseThreeMeteorPatternsNormal.Random) {
                    return DragonPhaseThreeMeteorPatternNormal.Value.ToString().ToCharArray();
                }
            }
        }
        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(1f, 0.9f)) {
                if (DragonPhaseOneMeteorPatternHard.Value != DragonPhaseOneMeteorPatternsHard.Random) {
                    return DragonPhaseOneMeteorPatternHard.Value.ToString().ToCharArray();
                }
            }
            if (IsWithinPhase(0.9f, 0.64f)) {
                if (DragonPhaseTwoMeteorPatternHard.Value != DragonPhaseTwoMeteorPatternsHard.Random) {
                    return DragonPhaseTwoMeteorPatternHard.Value.ToString().ToCharArray();
                }
            }
        }

        DragonLevelDragon dragonLevelDragonInstance = UnityEngine.Object.FindObjectOfType<DragonLevelDragon>();
        string[] meteorPatterns = dragonLevelDragonInstance.currentMeteorProperties.pattern;
        int randomIndex = UnityEngine.Random.Range(0, meteorPatterns.Length);
        return meteorPatterns[randomIndex].ToCharArray();
    }

    public static int SetPeashotPattern () {
        if (Level.ScoringData.difficulty == Level.Mode.Easy) {
            if (IsWithinPhase(1f, 0.75f)) {
                if (DragonPhaseOneLaserPatternEasy.Value != DragonPhaseOneLaserPatternsEasy.Random) {
                    return (int) DragonPhaseOneLaserPatternEasy.Value - 1;
                }
            }
            if (IsWithinPhase(0.75f, 0.45f)) {
                if (DragonPhaseTwoLaserPatternEasy.Value != DragonPhaseTwoLaserPatternsEasy.Random) {
                    return (int) DragonPhaseTwoLaserPatternEasy.Value - 1;
                }
            }
        }


        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (IsWithinPhase(1.0f, 0.92f)) {
                if (DragonPhaseOneLaserPatternNormal.Value != DragonPhaseOneLaserPatternsNormal.Random) {
                    return (int) DragonPhaseOneLaserPatternNormal.Value - 1;
                }
            }
            if (IsWithinPhase(0.82f, 0.63f)) {
                if (DragonPhaseThreeLaserPatternNormal.Value != DragonPhaseThreeLaserPatternsNormal.Random) {
                    return (int) DragonPhaseThreeLaserPatternNormal.Value - 1;
                }
            }
        }

        if (Level.ScoringData.difficulty == Level.Mode.Hard) {
            if (IsWithinPhase(1.0f, 0.9f)) {
                if (DragonPhaseOneLaserPatternHard.Value != DragonPhaseOneLaserPatternsHard.Random) {
                    return (int) DragonPhaseOneLaserPatternHard.Value - 1;
                }
            }
            if (IsWithinPhase(0.9f, 0.64f)) {
                if (DragonPhaseTwoLaserPatternHard.Value != DragonPhaseTwoLaserPatternsHard.Random) {
                    return (int) DragonPhaseTwoLaserPatternHard.Value - 1;
                }
            }
        }

        DragonLevelDragon dragonLevelDragonInstance = UnityEngine.Object.FindObjectOfType<DragonLevelDragon>();
        return UnityEngine.Random.Range(0, dragonLevelDragonInstance.properties.CurrentState.peashot.patternString.Length);

    }


    protected static bool IsWithinPhase(float phaseStart, float phaseEnd) {
        // i'm not actually sure on the inclusivity of values here ( <, > vs <=, >=). i'm taking a guess.
        return
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health <= phaseStart &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > phaseEnd;
    }

}
