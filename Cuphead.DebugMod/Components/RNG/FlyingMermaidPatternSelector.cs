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
internal class FlyingMermaidPatternSelector : PluginComponent {

    // Sets the leading attack. Subsequent attacks work as normal.
    //[HarmonyPatch(typeof(DevilLevel), nameof(DevilLevel.Start))]
    [HarmonyPatch(typeof(FlyingMermaidLevel), nameof(FlyingMermaidLevel.Start))]
    [HarmonyPrefix]

    public static void PhaseOnePatternManipulator(ref FlyingMermaidLevel __instance) {
        if (FlyingMermaidPhaseOnePattern.Value != FlyingMermaidPhaseOnePatterns.Random) {
            if (FlyingMermaidPhaseOnePattern.Value == FlyingMermaidPhaseOnePatterns.Ghosts) {
                __instance.properties.CurrentState.patternIndex = 3;
            } else {
                __instance.properties.CurrentState.patternIndex = (int) FlyingMermaidPhaseOnePattern.Value - 2;
            }
        }
    }
}
