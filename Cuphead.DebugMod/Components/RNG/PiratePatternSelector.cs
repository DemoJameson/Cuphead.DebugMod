using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;

using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using UnityEngine;
using System.Collections;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class PiratePatternSelector : PluginComponent
{
    [HarmonyPatch(typeof(PirateLevel), nameof(PirateLevel.peashot_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PhaseOneGunManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<pattern>"))) {
            ilCursor.EmitDelegate<Func<KeyValue[], KeyValue[]>>(gun => GetGunPattern());
            ilCursor.Index++; // avoid infinite loops
        }
    }

    static KeyValue[] GetGunPattern() {
        string[] arr = { "D:1, P:3, D:1, P:1",
                         "D:1, P:2, D:1.5, P:2",
                         "D:1, P:1, D:1, P:3" };


        if (Level.ScoringData.difficulty == Level.Mode.Normal) {
            if (PiratePhaseOneGunPatternNormal.Value != PiratePhaseOneGunPatternsNormal.Random) {
                return KeyValue.ListFromString(arr[(int) PiratePhaseOneGunPatternNormal.Value - 1], new char[]
                {
                    'P',
                    'D'
                });
            }
        }
        return KeyValue.ListFromString(arr[UnityEngine.Random.Range(0, 3)], new char[]
        {
            'P',
            'D'
        });
    }

} 

