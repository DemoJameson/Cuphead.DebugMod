using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

[HarmonyPatch]
public class GameInfoHelper : PluginComponent {
    public static bool isChalice = false;
    private static Vector3? lastPlayerPosition;
    private static string lastLevelName;
    private static string lastTime;
    private static string lastInfo;
    private static int lastEventIndex;
    private static float overDamage;
    public static Dictionary<Weapon, int> weaponsTimer = new() {
        { Weapon.level_weapon_spreadshot, 0 },
        { Weapon.level_weapon_bouncer, 0 },
        { Weapon.level_weapon_charge, 0 },
        { Weapon.level_weapon_peashot, 0 },
        { Weapon.level_weapon_homing, 0 },
        { Weapon.level_weapon_boomerang, 0 },
        { Weapon.plane_weapon_peashot, 0 },
        { Weapon.plane_weapon_laser, 0 },
        { Weapon.plane_weapon_bomb, 0 },
#if v1_3
        { Weapon.level_weapon_crackshot, 0 },
        { Weapon.level_weapon_wide_shot, 0 },
        { Weapon.level_weapon_upshot, 0 },
        { Weapon.plane_chalice_weapon_3way, 0 },
        { Weapon.plane_chalice_weapon_bomb, 0 },
#endif
    };
    public static int onEXWeaponCooldown;
    public static string onEXWeaponName;
    private static Dictionary<Weapon, int> EXWeaponsTimer = new() {
        { Weapon.level_weapon_spreadshot, 0 },
        { Weapon.level_weapon_bouncer, 0 },
        { Weapon.level_weapon_charge, 0 },
        { Weapon.level_weapon_peashot, 0 },
        { Weapon.level_weapon_homing, 0 },
        { Weapon.level_weapon_boomerang, 0 },
        { Weapon.plane_weapon_peashot, 0 },
        { Weapon.plane_weapon_laser, 0 },
        { Weapon.plane_weapon_bomb, 0 },
#if v1_3
        { Weapon.level_weapon_crackshot, 0 },
        { Weapon.level_weapon_wide_shot, 0 },
        { Weapon.level_weapon_upshot, 0 },
        { Weapon.plane_chalice_weapon_3way, 0 },
        { Weapon.plane_chalice_weapon_bomb, 0 },
#endif
    };
    public static bool canUpdateEXWeaponsTimer = true;
    public static int chargeFrames;
    private static int exFireFrames;
    private static int exDelayFrames;
    private static int exPlaneFireFrames;
    private static int exPlaneDelayFrames;
    private static readonly List<string> infos = new();
    private static readonly List<string> statuses = new();
#if v1_3
    [HarmonyPatch(typeof(PlayerStatsManager), nameof(PlayerStatsManager.FixedUpdate))]
    [HarmonyPostfix]
    private static void SetIsChalice(ref PlayerStatsManager __instance) {
        isChalice = __instance.isChalice;
    }
#endif

    [HarmonyPatch(typeof(AbstractLevelWeapon), nameof(AbstractLevelWeapon.fireWeapon_cr), MethodType.Enumerator)]
    [HarmonyPatch(typeof(AbstractLevelWeapon), nameof(AbstractLevelWeapon.chargeFireWeapon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void AbstractLevelWeaponFireWeaponCr(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::$this"));
        object thisOperand = ilCursor.Next.Operand;
        ilCursor.Goto(0);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::mode"));
        object modeOperand = ilCursor.Next.Operand;
        ilCursor.Goto(0);

        while (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Stfld && (i.Operand.ToString().Contains("::t") || i.Operand.ToString().Contains("<t>")))) {
            ilCursor.Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, thisOperand)
                .Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, modeOperand)
                .EmitDelegate<Func<float, AbstractLevelWeapon, AbstractLevelWeapon.Mode, float>>((t, levelWeapon, mode) => {
                    int frames = (levelWeapon.rapidFireRate - t).ToCeilingFrames();
                    if (mode == AbstractLevelWeapon.Mode.Basic) {
                        weaponsTimer[levelWeapon.id] = frames;
                    } else {
                        EXWeaponsTimer[levelWeapon.id] = frames;
                    }

                    return t;
                });
            ilCursor.Index++;
        }
    }

    [HarmonyPatch(typeof(AbstractPlaneWeapon), nameof(AbstractPlaneWeapon.fireWeapon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void AbstractPlaneWeaponFireWeaponCr(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::$this"));
        object thisOperand = ilCursor.Next.Operand;

        ilCursor.Goto(0);
        while (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Stfld && (i.Operand.ToString().Contains("::t") || i.Operand.ToString().Contains("<t>")))) {
            ilCursor.Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, thisOperand)
                .EmitDelegate<Func<float, AbstractPlaneWeapon, float>>((t, planeWeapon) => {
                    Weapon weapon = planeWeapon.index switch {
                        0 => Weapon.plane_weapon_peashot,
                        1 => Weapon.plane_weapon_laser,
                        2 => Weapon.plane_weapon_bomb,
#if v1_3
                        3 => Weapon.plane_chalice_weapon_3way,
                        4 => Weapon.plane_chalice_weapon_bomb,
#endif
                        _ => Weapon.None
                    };
                    weaponsTimer[weapon] = (planeWeapon.rapidFireRate - t).ToCeilingFrames();
                    return t;
                });
            ilCursor.Index++;
        }
    }

    [HarmonyPatch(typeof(WeaponCharge), nameof(WeaponCharge.FixedUpdate))]
    [HarmonyPostfix]
    private static void WeaponChargeFixedUpdate(WeaponCharge __instance) {
        chargeFrames =
            (WeaponProperties.LevelWeaponCharge.Basic.timeStateThree - __instance.timeCharged).ToFloorFrames();
        if (chargeFrames < 0) {
            chargeFrames = 0;
        }
    }

    [HarmonyPatch(typeof(LevelPlayerWeaponManager), nameof(LevelPlayerWeaponManager.StartEx))]
    [HarmonyPostfix]
    private static void LevelPlayerWeaponManagerStartEx() {
        exDelayFrames = 34;
        exFireFrames = 16;
    }

    [HarmonyPatch(typeof(PlanePlayerWeaponManager), nameof(PlanePlayerWeaponManager.StartEx))]
    [HarmonyPostfix]
    private static void PlanePlayerWeaponManagerStartEx() {
        exPlaneDelayFrames = 54;
        exPlaneFireFrames = 31;
    }

    [HarmonyPatch(typeof(AbstractLevelWeapon), nameof(AbstractLevelWeapon.fireProjectile))]
    [HarmonyPostfix]
    private static void GetExWeaponCooldown(AbstractLevelWeapon.Mode mode, AbstractLevelWeapon __instance) {
        if (mode == AbstractLevelWeapon.Mode.Ex) {
            onEXWeaponCooldown = weaponsTimer[__instance.id];
            onEXWeaponName = GetWeaponName(__instance.id);
        }
    }

    [HarmonyPatch(typeof(AbstractPlaneWeapon), nameof(AbstractPlaneWeapon.BeginEx))]
    [HarmonyPrefix]
    private static void PrepareForGetPlaneExWeaponCooldown() {
        canUpdateEXWeaponsTimer = true;
    }

    [HarmonyPatch(typeof(AbstractPlaneWeapon), nameof(AbstractPlaneWeapon.fireProjectile))]
    [HarmonyPostfix]
    private static void GetPlaneExWeaponCooldown(AbstractPlaneWeapon.Mode mode, AbstractPlaneWeapon __instance) {
        if (mode == AbstractPlaneWeapon.Mode.Ex) {
            if (canUpdateEXWeaponsTimer) {
                Weapon currentPlaneWeapon = __instance.index switch {
                    0 => Weapon.plane_weapon_peashot,
                    1 => Weapon.plane_weapon_laser,
                    2 => Weapon.plane_weapon_bomb,
#if v1_3
                    3 => Weapon.plane_chalice_weapon_3way,
                    4 => Weapon.plane_chalice_weapon_bomb,
#endif
                    _ => Weapon.None
                };
                onEXWeaponCooldown = weaponsTimer[currentPlaneWeapon];
                onEXWeaponName = GetWeaponName(currentPlaneWeapon);
                // this prevents the timer from immediately updating again after a minibomb EX
                canUpdateEXWeaponsTimer = false;
            }

        }
    }

    private void Awake() {
        HookHelper.ActiveSceneChanged(() => {
            lastPlayerPosition = null;
            lastLevelName = null;
            lastTime = null;
            lastInfo = null;
            lastEventIndex = 0;
            overDamage = 0;
            weaponsTimer = new() {
                { Weapon.level_weapon_spreadshot, 0 },
                { Weapon.level_weapon_bouncer, 0 },
                { Weapon.level_weapon_charge, 0 },
                { Weapon.level_weapon_peashot, 0 },
                { Weapon.level_weapon_homing, 0 },
                { Weapon.level_weapon_boomerang, 0 },
                { Weapon.plane_weapon_peashot, 0 },
                { Weapon.plane_weapon_laser, 0 },
                { Weapon.plane_weapon_bomb, 0 },
#if v1_3
                { Weapon.level_weapon_crackshot, 0 },
                { Weapon.level_weapon_wide_shot, 0 },
                { Weapon.level_weapon_upshot, 0 },
                { Weapon.plane_chalice_weapon_3way, 0 },
                { Weapon.plane_chalice_weapon_bomb, 0 },
#endif
            };
            EXWeaponsTimer = new() {
                { Weapon.level_weapon_spreadshot, 0 },
                { Weapon.level_weapon_bouncer, 0 },
                { Weapon.level_weapon_charge, 0 },
                { Weapon.level_weapon_peashot, 0 },
                { Weapon.level_weapon_homing, 0 },
                { Weapon.level_weapon_boomerang, 0 },
                { Weapon.plane_weapon_peashot, 0 },
                { Weapon.plane_weapon_laser, 0 },
                { Weapon.plane_weapon_bomb, 0 },
#if v1_3
                { Weapon.level_weapon_crackshot, 0 },
                { Weapon.level_weapon_wide_shot, 0 },
                { Weapon.level_weapon_upshot, 0 },
                { Weapon.plane_chalice_weapon_3way, 0 },
                { Weapon.plane_chalice_weapon_bomb, 0 },
#endif
            };
            onEXWeaponCooldown = 0;
            onEXWeaponName = "";
            chargeFrames = 0;
            exDelayFrames = 0;
            exFireFrames = 0;
            exPlaneDelayFrames = 0;
            exPlaneFireFrames = 0;
        });
    }

    private void Update() {
        if (exDelayFrames > 0) {
            exDelayFrames--;
        }

        if (exFireFrames > 0) {
            exFireFrames--;
        }

        if (exPlaneDelayFrames > 0) {
            exPlaneDelayFrames--;
        }

        if (exPlaneFireFrames > 0) {
            exPlaneFireFrames--;
        }
    }

    public static string GetWeaponName(Weapon weapon) {
        return weapon switch {
            Weapon.level_weapon_peashot => "Peashooter",
            Weapon.level_weapon_spreadshot => "Spread",
            Weapon.level_weapon_homing => "Chaser",
            Weapon.level_weapon_bouncer => "Lobber",
            Weapon.level_weapon_charge => "Charge",
            Weapon.level_weapon_boomerang => "Roundabout",
            Weapon.plane_weapon_peashot => "Peashooter",
            Weapon.plane_weapon_bomb => "Bomb",
#if v1_3
            Weapon.level_weapon_crackshot => "Crackshot",
            Weapon.level_weapon_wide_shot => "Converge",
            Weapon.level_weapon_upshot => "TwistUp",
            Weapon.plane_chalice_weapon_3way => "Peashooter",
            Weapon.plane_chalice_weapon_bomb => "Bomb",
#endif
            _ => weapon.ToString().Replace("level_weapon_", ""),
        };
    }

    public static string FormatTime(float time) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string formatted =
            $"{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}.{timeSpan.Milliseconds.ToString().PadLeft(3, '0')}";
        return $"{formatted}({time.ToCeilingFrames()})";
    }
}
