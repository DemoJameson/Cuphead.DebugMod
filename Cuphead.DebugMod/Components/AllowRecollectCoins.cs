using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
public class RecollectCoins : PluginComponent {
    // if (PlayerData.Data.GetCoinCollected(this))
    // ↓
    // if (PlayerData.Data.GetCoinCollected(this) && !Settings.AllowRecollectCoins)
    [HarmonyPatch(typeof(LevelCoin), nameof(LevelCoin.Awake))]
    [HarmonyILManipulator]
    private static void PreventDestroy(ILContext il) {
        ILCursor ilCursor = new(il);
        if (ilCursor.TryGotoNext(MoveType.After, ins => ins.OpCode == OpCodes.Callvirt && ins.Operand.ToString().Contains("GetCoinCollected"))) {
            ilCursor.EmitDelegate<Func<bool, bool>>(collected => collected && !Settings.AllowRecollectCoins.Value);
        } else {
            Logger.LogWarning("RecollectCoins: Can't find GetCoinCollected method");
        }
    }
}