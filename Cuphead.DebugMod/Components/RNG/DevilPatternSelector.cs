using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using UnityEngine;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
internal class DevilPatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    //[HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.Start))]
    [HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOnePatternManipulator(ref DevilLevel __instance) {
        if (DevilPhaseOnePattern.Value != DevilPhaseOnePatterns.Random) {
            if (DevilPhaseOnePattern.Value != DevilPhaseOnePatterns.Head1) {
                __instance.properties.CurrentState.patternIndex = 6;
            } else {
                __instance.properties.CurrentState.patternIndex = (int) DevilPhaseOnePattern.Value - 2;
            }
        }

    }

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void PhaseOneHeadManipulator(ref DevilLevelSittingDevil __instance) {
        if (DevilPhaseOneHeadType.Value != DevilPhaseOneHeadTypes.Random) {
            __instance.isSpiderAttackNext = DevilPhaseOneHeadType.Value == DevilPhaseOneHeadTypes.Spider ? true : false; 
        }
    }

    // TODO: pitchfork
}

