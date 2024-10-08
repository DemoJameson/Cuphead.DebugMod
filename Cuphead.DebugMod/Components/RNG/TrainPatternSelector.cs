using System;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.Settings;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components.RNG;

[HarmonyPatch]
public class TrainPatternSelector : PluginComponent {

    [HarmonyPatch(typeof(TrainLevel), nameof(TrainLevel.pumpkins_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void PumpkinManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("<dir>__0"))) {
            ilCursor.Index = ilCursor.Index - 4;
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();
            ilCursor.Remove();

            ilCursor.EmitDelegate<Func<bool, int>>(dir =>
            TrainPumpkinStartingDirection.Value == TrainPumpkinStartingDirections.Random ?
            (!Rand.Bool()) ? -1 : 1
            :
            TrainPumpkinStartingDirection.Value == TrainPumpkinStartingDirections.Right ? -1 : 1
            );
            break;
        }
    }

    [HarmonyPatch(typeof(TrainLevelLollipopGhoulsManager), nameof(TrainLevelLollipopGhoulsManager.ghouls_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void GhoulManipulator(ILContext il) {
        ILCursor ilCursor = new(il);
        while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("current"))) {
            ilCursor.EmitDelegate<Func<bool, int>>(twin =>
            TrainStartingGhoul.Value == TrainStartingGhouls.Random ?
            UnityEngine.Random.Range(0, 2)
            :
            TrainStartingGhoul.Value == TrainStartingGhouls.Right ? 0 : 1
            );
            break;
        }
    }
}
