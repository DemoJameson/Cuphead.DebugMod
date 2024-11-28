using System;
using BepInEx.CupheadDebugMod.Components.RNG;
using System.Reflection;
using BepInEx.CupheadDebugMod.Config;
using HarmonyLib;
using MonoMod.Cil;
using static BepInEx.CupheadDebugMod.Config.SettingsEnums;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
internal class WinScreenLoop : PluginComponent {

    [HarmonyPatch(typeof(WinScreen), nameof(WinScreen.main_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    public static void WinScreenLooper(ILContext il) {
        ILCursor ilCursor = new(il);
        ILLabel newLabel = null;

        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().Contains("advanceDelay"))) {
            ilCursor.Index++;


            for (int i = 0; i < 11; i++) {
                ilCursor.Remove();
            }
#if v1_3
                    for (int i = 0; i < 10; i++) {
                        ilCursor.Remove();
                    }
#endif
            ilCursor.Emit(OpCodes.Call, typeof(WinScreenLoop).GetMethod(nameof(HandleEndOfWinScreen), BindingFlags.Static | BindingFlags.Public));
            ilCursor.Index--;
            newLabel = ilCursor.DefineLabel();
            ilCursor.MarkLabel(newLabel);
            break;
        }

        // One of the instructions that's being removed has a label attached that a previous br instruction relies on. This fixes that.
        ilCursor.Index = 0;
        while (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("GetActionButtonDown"))) {
            ilCursor.Index++;
            ilCursor.Remove();
            ilCursor.Emit(OpCodes.Br, newLabel ?? throw new NullReferenceException());
            break;
        }
    }

    public static void HandleEndOfWinScreen() {
        if (Settings.LoopWinScreen.Value) {
            SceneLoader.LoadScene(Scenes.scene_win, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade, SceneLoader.Icon.Hourglass);
        } else {
            if (Level.PreviousLevel == Levels.Devil) {
                Cutscene.Load(Scenes.scene_title, Scenes.scene_cutscene_outro, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade, SceneLoader.Icon.Hourglass);
#if v1_3
            } else if (Level.PreviousLevel == Levels.Saltbaker) {
        		Cutscene.Load(Scenes.scene_map_world_DLC, Scenes.scene_cutscene_dlc_ending, SceneLoader.Transition.Iris, SceneLoader.Transition.Fade, SceneLoader.Icon.Hourglass);
#endif
            } else {
                SceneLoader.LoadLastMap();
            }
        }
    }
}
