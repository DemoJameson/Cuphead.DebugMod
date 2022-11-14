using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using UnityEngine;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class FlyingBlimpPatternSelector : PluginComponent {

    // Sets the leading attack in the sequence.
    [HarmonyPatch(typeof(FlyingBlimpLevel), nameof(FlyingBlimpLevel.OnStateChanged))]
    [HarmonyPrefix]

    public static void PhaseBlimpPatternManipulator(ref FlyingBlimpLevel __instance) {
        // TODO: need to figure out where i need to use < and > vs <= and >=
        // TODO: make this extensible for simple and expert mode
        if (__instance.properties.CurrentState.stateName == LevelProperties.FlyingBlimp.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health < 0.77f &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > 0.64f) {
            if (Settings.FlyingBlimpPhaseBlimp2Pattern.Value != FlyingBlimpPhaseBlimp2Patterns.Random) {
                if (Settings.FlyingBlimpPhaseBlimp2Pattern.Value == FlyingBlimpPhaseBlimp2Patterns.Tornado1) {
                    __instance.properties.CurrentState.patternIndex = 6;
                } else {
                    __instance.properties.CurrentState.patternIndex = (int) Settings.FlyingBlimpPhaseBlimp2Pattern.Value - 2;
                }
            }
        }
        if (__instance.properties.CurrentState.stateName == LevelProperties.FlyingBlimp.States.Generic &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health < 0.46f &&
        (Level.Current.timeline.health - Level.Current.timeline.damage) / Level.Current.timeline.health > 0.34f) {
            if (Settings.FlyingBlimpPhaseBlimp3Pattern.Value != FlyingBlimpPhaseBlimp3Patterns.Random) {
                if (Settings.FlyingBlimpPhaseBlimp3Pattern.Value == FlyingBlimpPhaseBlimp3Patterns.Shoot1) {
                    __instance.properties.CurrentState.patternIndex = 4;
                } else {
                    __instance.properties.CurrentState.patternIndex = (int) Settings.FlyingBlimpPhaseBlimp3Pattern.Value - 2;
                }
            }
        }

        Logger.LogInfo("pattern: " + __instance.properties.CurrentState.patternIndex);
    }
    // pattern index seems to get regenerated in phases 2 and 3



}